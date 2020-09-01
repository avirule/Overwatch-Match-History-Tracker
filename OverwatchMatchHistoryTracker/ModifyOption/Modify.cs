#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.MatchOption;
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker.ModifyOption
{
    [Verb(nameof(Modify), HelpText = _HELP_TEXT)]
    public class Modify : CommandNameOption
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
        public object? Value { get; set; }


        public override async ValueTask Process(MatchesContext matchesContext)
        {
            Match match = matchesContext.Matches.FirstOrDefault(match => match.ID == MatchID)
                          ?? throw new ArgumentOutOfRangeException(nameof(MatchID), $"Given {nameof(MatchID)} does not exist in database.");

            switch (MatchHeader)
            {
                case Match.Header.Timestamp when Value is DateTime value:
                    match.Timestamp = value;
                    break;
                case Match.Header.Entropic when Value is bool value:
                    match.Entropic = value;
                    break;
                case Match.Header.Role when Value is Role value:
                    match.Role = value;
                    break;
                case Match.Header.SR when Value is int value:
                    match.SR = value;
                    break;
                case Match.Header.Map when Value is Map value:
                    match.Map = value;
                    break;
                case Match.Header.Comment when Value is string value:
                    match.Comment = value;
                    break;
                default:
                    throw new ArgumentException("Type mismatch between header and value.", nameof(Value));
            }

            await matchesContext.SaveChangesAsync();
        }
    }
}
