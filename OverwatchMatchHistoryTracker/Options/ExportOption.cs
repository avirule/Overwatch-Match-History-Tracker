#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CommandLine;
using Microsoft.Data.Sqlite;
using OverwatchMatchHistoryTracker.Helpers;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("export", HelpText = "Exports a match history database to another format.")]
    public class ExportOption : CommandOption
    {
        private string _Name;
        private string _Role;

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player to use data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for which to use data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        public ExportOption() => _Name = _Role = string.Empty;

        public override async ValueTask Process()
        {
            if (!RolesHelper.Valid.Contains(Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{Role}' (valid roles are {string.Join(", ", RolesHelper.Valid.Select(role => $"'{role}'"))})."
                );
            }

            SqliteCommand command = await MatchHistoryProvider.GetDatabaseCommand(Name);
            await MatchHistoryProvider.VerifyRoleTableExists(command, Role);
            command.CommandText = $"SELECT * FROM {Role} ORDER BY datetime(timestamp)";

            List<(string Timestamp, int SR, string Map, string Comment)> historicData = new List<(string, int, string, string)>();
            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                historicData.Add((reader.GetString(0), reader.GetInt32(1), reader.GetString(2),
                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3)));
            }

            if (historicData.Count == 0)
            {
                Console.WriteLine("No historic match data.");
            }
            else
            {
                using IXLWorkbook workbook = ConstructSpreadsheet(historicData);

                string filePath = Path.GetFullPath($"{Name}_{Role}.xlsx");
                workbook.SaveAs(filePath);
                Console.WriteLine($"Successfully exported match data to \"{filePath}\"");
            }
        }

        private static IXLWorkbook ConstructSpreadsheet(IReadOnlyList<(string Timestamp, int SR, string Map, string Comment)> historicData)
        {
            const int row_index_offset = 3;
            int lastRow = (historicData.Count + row_index_offset) - 1;

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
            for (int index = 0; index < historicData.Count; index++)
            {
                (string timestamp, int sr, string map, string comment) = historicData[index];

                worksheet.Cell($"B{index + row_index_offset}").Value = timestamp;
                worksheet.Cell($"C{index + row_index_offset}").Value = sr;

                int change = index == (historicData.Count - 1) ? 0 : historicData[index + 1].SR - sr;
                string changeString = change > 0 ? $"+{change}" : change.ToString();
                worksheet.Cell($"D{index + row_index_offset}").Value = changeString;
                worksheet.Cell($"D{index + row_index_offset}").Style.Fill.BackgroundColor =
                    change > 0
                        ? XLColor.LimeGreen
                        : change < 0
                            ? XLColor.FerrariRed
                            : XLColor.LightGray;

                worksheet.Cell($"E{index + row_index_offset}").Value = map;
                worksheet.Cell($"F{index + row_index_offset}").Value = comment;
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
