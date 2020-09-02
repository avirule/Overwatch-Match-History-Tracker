#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.Options.DisplayOption;
using OverwatchMatchHistoryTracker.Options.MatchOption;

#endregion

namespace OverwatchMatchHistoryTracker.Options.ModifyOption
{
    [Verb(nameof(Modify), HelpText = _HELP_TEXT)]
    public class Modify : CommandOption
    {
        private const string _HELP_TEXT = "Modify an existing match history entry";

        [Usage]
        public static IEnumerable<Example> Examples { get; } = new List<Example>
        {
            new Example(_HELP_TEXT, new Modify
            {
                Name = "ShadowDragon",
                MatchID = 1,
                MatchHeader = Match.Header.Comment,
                Value = "Might have gotten enough healing."
            })
        };

        [Value(1, MetaName = nameof(MatchID), HelpText = "ID of match history entry to modify (can be obtained with the 'display' command).",
            Required = true)]
        public int MatchID { get; set; }

        [Value(2, MetaName = nameof(MatchHeader), HelpText = "Column to modify by header.", Required = true)]
        public Match.Header MatchHeader { get; set; }

        [Value(3, MetaName = nameof(Value), HelpText = "New value for column entry.", Required = true)]
        public string? Value { get; set; }

        public override ValueTask Process(MatchesContext matchesContext)
        {
            Match match = matchesContext.Matches.FirstOrDefault(match => match.ID == MatchID)
                          ?? throw new ArgumentOutOfRangeException(nameof(MatchID), $"Given {nameof(MatchID)} does not exist in database.");

            ProcessingFinishedMessage = $"[OLD] {Display.TableDisplay(match)}\r\n";

            switch (MatchHeader)
            {
                case Match.Header.Timestamp when DateTime.TryParse(Value, out DateTime result):
                    match.Timestamp = result;
                    break;
                case Match.Header.Entropic when bool.TryParse(Value, out bool result):
                    match.Entropic = result;
                    break;
                case Match.Header.SR when int.TryParse(Value, out int result):
                    match.SR = result;
                    break;
                case Match.Header.Role when Enum.TryParse(typeof(Role), Value, true, out object? result) && result is Role role:
                    match.Role = role;
                    break;
                case Match.Header.Map when Enum.TryParse(typeof(Map), Value, true, out object? result) && result is Map map:
                    match.Map = map;
                    break;
                case Match.Header.Comment:
                    match.Comment = Value;
                    break;
                default:
                    throw new ArgumentException("Type mismatch between header and value.", nameof(Value));
            }

            ProcessingFinishedMessage += $"[NEW] {Display.TableDisplay(match)}\r\n";

            return default;
        }
    }
}
