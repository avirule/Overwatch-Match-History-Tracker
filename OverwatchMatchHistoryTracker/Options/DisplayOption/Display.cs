#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.Options.MatchOption;

#endregion

namespace OverwatchMatchHistoryTracker.Options.DisplayOption
{
    [Verb(nameof(Display), HelpText = _HELP_TEXT)]
    public class Display : CommandRoleOption
    {
        private const string _HELP_TEXT = "Display tabulated match history data.";

        public const string DATE_TIME_FORMAT = "d/M/yyyy HH:mm:ss";

        [Usage]
        public static IEnumerable<Example> Examples { get; } = new List<Example>
        {
            new Example(_HELP_TEXT, new Display
            {
                Name = "ShadowDragon",
                Role = Role.DPS,
                Outcome = Outcome.Win
            }),
        };

        public static string TableDisplay(Match match) =>
            TableDisplay(
                new TableCell(match.ID.ToString(), 4),
                new TableCell(match.Timestamp.ToString(DATE_TIME_FORMAT), 19),
                new TableCell(match.Entropic ? "True" : "False", 8),
                new TableCell(match.Role.ToString(), 10),
                new TableCell(match.SR.ToString(), 4),
                new TableCell(match.Map.ToString(), 25),
                new TableCell(match.Comment ?? string.Empty, 0)
            );

        public static string TableDisplay(params TableCell[] data)
        {
            static string ComputeTableCell(TableCell tableCell)
            {
                tableCell.Data ??= " ";
                return tableCell.Data.PadLeft((tableCell.DisplayWidth / 2) + (tableCell.Data.Length / 2)).PadRight(tableCell.DisplayWidth);
            }

            return string.Join(" | ", data.Select(ComputeTableCell));
        }

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

                string tableDisplay = TableDisplay
                (
                    new TableCell("ID", 4),
                    new TableCell("Timestamp", 19),
                    new TableCell("Entropic", 8),
                    new TableCell("SR", 4),
                    new TableCell("Change", 6),
                    new TableCell("Map", 25),
                    new TableCell("Comment", 0)
                );
                Console.WriteLine(tableDisplay);
                Console.WriteLine($"{new string('-', tableDisplay.Length)}"); // header-body separator

                await foreach ((Match match, int sr) in matchesContext.GetMatchesByOutcomeAsync(Role, Outcome))
                {
                    string changeString = FormatSRChange(sr);

                    tableDisplay = TableDisplay(
                        new TableCell(match.ID.ToString(), 4),
                        new TableCell(match.Timestamp.ToString(DATE_TIME_FORMAT), 19),
                        new TableCell(match.Entropic ? "True" : "False", 8),
                        new TableCell(match.SR.ToString(), 4),
                        new TableCell(changeString, 6),
                        new TableCell(match.Map.ToString(), 25),
                        new TableCell(match.Comment ?? string.Empty, 0)
                    );

                    Console.WriteLine(tableDisplay);
                }
            }
        }

        public static string FormatSRChange(int change) => change > 0 ? $"+{change}" : change.ToString();
    }
}
