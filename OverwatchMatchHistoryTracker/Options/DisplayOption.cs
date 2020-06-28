#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using DocumentFormat.OpenXml.Spreadsheet;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("display", HelpText = "Display tabulated match history data.")]
    public class DisplayOption : CommandOption
    {
        private const string _DISPLAY_FORMAT = " {0} | {1} | {2} | {3} | {4} ";

        private static readonly HashSet<string> _ValidOutcomes = new HashSet<string>
        {
            "win",
            "loss",
            "draw",
            "overall"
        };

        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Displays historic match data", new DisplayOption
            {
                Name = "ShadowDragon",
                Role = "DPS",
                Outcome = "win"
            }),
        };

        public static void Display(string timestamp, string sr, string change, string map, string? comment)
        {
            Console.WriteLine(_DISPLAY_FORMAT,
                timestamp.PadLeft(10 + (timestamp.Length / 2)).PadRight(19),
                sr.PadLeft(2 + (sr.Length / 2)).PadRight(4),
                map.PadLeft(13 + (map.Length / 2)).PadRight(25),
                change.PadLeft(3 + (change.Length / 2)).PadRight(6),
                comment ?? string.Empty);
        }

        private string _Outcome;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Value(2, MetaName = nameof(Outcome), Required = false, HelpText = "Only display matches of given outcome (win / loss/ draw).",
            Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set
            {
                string outcome = value.ToLowerInvariant();

                if (!_ValidOutcomes.Contains(outcome))
                {
                    throw new InvalidOperationException($"Given outcome must be: {string.Join(", ", _ValidOutcomes)}");
                }

                _Outcome = outcome;
            }
        }

        public DisplayOption() => _Outcome = string.Empty;

        public override async ValueTask Process(MatchHistoryContext matchHistoryContext)
        {
            IAsyncEnumerable<Match> matches = matchHistoryContext.GetOrderedMatches().Where(match => match.Role.Equals(Role));

            if (!await matches.AnyAsync())
            {
                Console.WriteLine("No historic match data.");
            }
            else
            {
                Console.WriteLine(); // add blank new line
                Display("timestamp", "sr", "change", "map", "comment"); // headers
                Console.WriteLine($" {new string('-', 73)}"); // header-body separator

                // body (values)
                int lastSR = -1;
                await foreach (Match match in matches)
                {
                    if (!match.Entropic)
                    {
                        lastSR = -1;
                    }

                    if (lastSR > -1)
                    {
                        int srChange = lastSR - match.SR;

                        switch (Outcome)
                        {
                            case "win" when srChange > 0:
                            case "loss" when srChange < 0:
                            case "draw" when srChange == 0:
                            case "overall":
                                string changeString = srChange > 0 ? $"+{srChange}" : srChange.ToString(); // add positive-sign to wins
                                Display(match.Timestamp.ToString("yyyy-mm-dd hh:mm:ss"), match.SR.ToString(), changeString, match.Map.ToString(), match.Comment);
                                break;
                        }
                    }

                    lastSR = match.SR;
                }
            }
        }
    }
}
