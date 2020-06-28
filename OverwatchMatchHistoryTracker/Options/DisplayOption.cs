#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("display", HelpText = "Display tabulated match history data.")]
    public class DisplayOption : CommandOption
    {
        private const string _DISPLAY_FORMAT = " {0} | {1} | {2} | {3} | {4} ";

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
                change.PadLeft(3 + (change.Length / 2)).PadRight(6),
                map.PadLeft(13 + (map.Length / 2)).PadRight(25),
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
            set => _Outcome = value.ToLowerInvariant();
        }

        public DisplayOption() => _Outcome = string.Empty;

        public override async ValueTask Process(MatchHistoryContext matchHistoryContext)
        {
            VerifyRole(Role);

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
                int lastSR = (await matches.FirstAsync()).SR;
                await foreach (Match match in matches)
                {
                    int change = match.SR - lastSR;

                    switch (Outcome)
                    {
                        case "win" when change > 0:
                        case "loss" when change < 0:
                        case "draw" when change == 0:
                        case "overall":
                            string changeString = change > 0 ? $"+{change}" : change.ToString(); // add positive-sign to wins
                            Display(match.Timestamp.ToString("yyyy-mm-dd hh:mm:ss"), match.SR.ToString(), changeString, match.Map, match.Comment);
                            lastSR = match.SR;
                            break;
                    }
                }
            }
        }
    }
}
