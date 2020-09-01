#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.MatchOption;
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker.ExportOption
{
    [Verb(nameof(Export), HelpText = _HELP_TEXT)]
    public class Export : CommandRoleOption
    {
        private const string _HELP_TEXT = "Exports a match history database to another format.";

        [Usage]
        public static IEnumerable<Example> Examples { get; } = new List<Example>
        {
            new Example(_HELP_TEXT, new Export
            {
                Name = "ShadowDragon",
                Role = Role.DPS
            })
        };

        public override async ValueTask Process(MatchesContext matchesContext)
        {
            IAsyncEnumerable<Match> matches = matchesContext.GetMatchesByRoleAsync(Role);

            if (!await matches.AnyAsync())
            {
                Console.WriteLine("No historic match data.");
            }
            else
            {
                using IXLWorkbook workbook = ConstructSpreadsheet(await matches.ToListAsync());

                string filePath = Path.GetFullPath($"{Name}_{Role}.xlsx");
                workbook.SaveAs(filePath);
                Console.WriteLine($"Successfully exported match data to \"{filePath}\"");
            }
        }

        private static IXLWorkbook ConstructSpreadsheet(IReadOnlyList<Match> matches)
        {
            const int row_index_offset = 3;
            int lastRow = (matches.Count + row_index_offset) - 1;

            IXLWorkbook workbook = new XLWorkbook();
            IXLWorksheet worksheet = workbook.Worksheets.Add("Support");

            // headers
            worksheet.Cell("B2").Value = "timestamp";
            worksheet.Cell("C2").Value = "sr";
            worksheet.Cell("D2").Value = "change";
            worksheet.Cell("E2").Value = "map";
            worksheet.Cell("F2").Value = "comment";

            // bold the headers
            worksheet.Range(worksheet.Cell("B2").Address, worksheet.Cell("F2").Address).Style.Font.Bold = true;

            worksheet.Row(2).Cells().Style.Border.TopBorder = XLBorderStyleValues.Thin;
            worksheet.Row(2).Cells().Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            // data
            for (int index = 0; index < matches.Count; index++)
            {
                Match match = matches[index];

                worksheet.Cell($"B{index + row_index_offset}").Value = match.Timestamp.ToString("yyyy-mm-dd hh:mm:ss");
                worksheet.Cell($"C{index + row_index_offset}").Value = match.SR;

                int change = index == (matches.Count - 1) ? 0 : matches[index + 1].SR - match.SR;
                string changeString = change > 0 ? $"+{change}" : change.ToString();
                worksheet.Cell($"D{index + row_index_offset}").Value = changeString;
                worksheet.Cell($"D{index + row_index_offset}").Style.Fill.BackgroundColor =
                    change > 0
                        ? XLColor.LimeGreen
                        : change < 0
                            ? XLColor.FerrariRed
                            : XLColor.LightGray;

                worksheet.Cell($"E{index + row_index_offset}").Value = match.Map;
                worksheet.Cell($"F{index + row_index_offset}").Value = match.Comment ?? string.Empty;
            }

            // most sizing and styling happens after filling sheet with data to ensure
            // the styling happens for all relevant cells (styling prior wouldn't, for
            // instance, affect the empty cells that would-be filled with data.

            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Column("B").Cells().Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            worksheet.Column("B").Cells().Style.Border.RightBorder = XLBorderStyleValues.Thin;
            worksheet.Column("C").Cells().Style.Border.RightBorder = XLBorderStyleValues.Thin;
            worksheet.Column("D").Cells().Style.Border.RightBorder = XLBorderStyleValues.Thin;
            worksheet.Column("E").Cells().Style.Border.RightBorder = XLBorderStyleValues.Thin;

            // use range to ensure empty comments are bordered
            worksheet.Range(worksheet.Cell("F2").Address, worksheet.Cell($"F{lastRow}").Address).Cells().Style.Border.RightBorder =
                XLBorderStyleValues.Thin;

            worksheet.Row(lastRow).Cells().Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            worksheet.Row(1).Height = 10;
            worksheet.Column(1).Width = 1;

            return workbook;
        }
    }
}
