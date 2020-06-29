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

namespace OverwatchMatchHistoryTracker.DisplayOption
{
    [Verb(nameof(Display), HelpText = "Display tabulated match history data.")]
    public class Display : CommandOption
    {
        public struct TableCell
        {
            public string Data { get; set; }
            public int DisplayWidth { get; }

            public TableCell(string data, int displayWidth) => (Data, DisplayWidth) = (data, displayWidth);
        }

        public const string DATE_TIME_FORMAT = "yyyy-mm-dd hh:mm:ss";

        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Displays historic match data", new Display
            {
                Name = "ShadowDragon",
                Role = Role.DPS,
                Outcome = Outcome.Win
            }),
        };

        public static int TableDisplay(params TableCell[] data)
        {
            static string ComputeTableCell(TableCell tableCell)
            {
                tableCell.Data ??= " ";
                return tableCell.Data.PadLeft((tableCell.DisplayWidth / 2) + (tableCell.Data.Length / 2)).PadRight(tableCell.DisplayWidth);
            }

            string tableRow = string.Join(" | ", data.Select(ComputeTableCell));
            Console.WriteLine(tableRow);
            return tableRow.Length;
        }

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for player.")]
        public Role Role { get; set; }

        [Value(2, MetaName = nameof(Outcome), Required = false, Default = Outcome.Overall,
            HelpText = "Only display matches of given outcome (win / loss/ draw).")]
        public Outcome Outcome { get; set; }

        public override async ValueTask Process(MatchesContext matchesContext)
        {
            IAsyncEnumerable<Match> matches = matchesContext.GetMatchesByRoleAsync(Role);

            if (!await matches.AnyAsync())
            {
                Console.WriteLine("No historic match data.");
            }
            else
            {
                Console.WriteLine(); // add blank new line

                int rowLength = TableDisplay
                (
                    new TableCell("ID", 4),
                    new TableCell("Timestamp", 19),
                    new TableCell("Entropic", 8),
                    new TableCell("SR", 4),
                    new TableCell("Change", 6),
                    new TableCell("Map", 25),
                    new TableCell("Comment", 0)
                );
                Console.WriteLine($"{new string('-', rowLength)}"); // header-body separator

                await foreach ((Match match, int sr) in matchesContext.GetMatchesByOutcomeAsync(Role, Outcome))
                {
                    string changeString = FormatSRChange(sr);

                    TableDisplay
                    (
                        new TableCell(match.MatchID.ToString(), 4),
                        new TableCell(match.Timestamp.ToString(DATE_TIME_FORMAT), 19),
                        new TableCell(match.Entropic ? "True" : "False", 8),
                        new TableCell(match.SR.ToString(), 4),
                        new TableCell(changeString, 6),
                        new TableCell(match.Map.ToString(), 25),
                        new TableCell(match.Comment ?? string.Empty, 0)
                    );
                }
            }
        }

        public static string FormatSRChange(int change) => change > 0 ? $"+{change}" : change.ToString();
    }
}
