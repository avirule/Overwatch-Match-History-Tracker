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
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class OverwatchTracker
    {
        private static readonly Type[] _OptionTypes =
        {
            typeof(MatchOption),
            typeof(AverageOption),
            typeof(DisplayOption),
            typeof(ExportOption)
        };

        public static async ValueTask Process(IEnumerable<string> args)
        {
            try
            {
                object? parsed = null;
                Parser.Default.ParseArguments(args, _OptionTypes).WithParsed(obj => parsed = obj);

                switch (parsed)
                {
                    case MatchOption matchOption:
                        await ProcessMatchOption(matchOption);
                        break;
                    case AverageOption averageOption:
                        await ProcessAverageOption(averageOption);
                        break;
                    case DisplayOption displayOption:
                        await ProcessDisplayOption(displayOption);
                        break;
                    case ExportOption exportOption:
                        await ProcessExportOption(exportOption);
                        break;
                    default:
                        throw new InvalidOperationException("Did not recognize given arguments.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region MatchOption

        private static async ValueTask ProcessMatchOption(MatchOption matchOption)
        {
            if (!RolesHelper.ValidRoles.Contains(matchOption.Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{matchOption.Role}' (valid roles are 'tank', 'dps', and 'support')."
                );
            }

            SqliteCommand command = await GetCommand(matchOption.Name, matchOption.NewPlayer);
            command.CommandText =
                $@"
                    CREATE TABLE IF NOT EXISTS {matchOption.Role}
                    (
                        timestamp TEXT NOT NULL,
                        sr INT NOT NULL CHECK (sr >= 0 AND sr <= 6000),
                        map TEXT NOT NULL CHECK
                            (
                                {string.Join(" OR ", MapsHelper.Valid.Select(validMap => $"map = \"{validMap}\""))}
                            ),
                        comment TEXT DEFAULT NULL
                    )
                ";
            await command.ExecuteNonQueryAsync();

            if (MapsHelper.Aliases.ContainsKey(matchOption.Map))
            {
                matchOption.Map = MapsHelper.Aliases[matchOption.Map];
            }

            command.CommandText = $"INSERT INTO {matchOption.Role} (timestamp, sr, map, comment) VALUES (datetime(), $sr, $map, $comment)";
            command.Parameters.AddWithValue("$sr", matchOption.SR);
            command.Parameters.AddWithValue("$map", matchOption.Map);
            command.Parameters.AddWithValue("$comment", matchOption.Comment);
            await command.ExecuteNonQueryAsync();

            Console.WriteLine("Successfully committed match data.");
        }

        #endregion

        #region AverageOption

        private static async ValueTask ProcessAverageOption(AverageOption averageOption)
        {
            if (!RolesHelper.ValidRoles.Contains(averageOption.Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{averageOption.Role}' (valid roles are {string.Join(", ", RolesHelper.ValidRoles.Select(role => $"'{role}'"))})."
                );
            }

            double average = averageOption.Change
                ? await GetMatchSRChanges(averageOption.Name, averageOption.Role, averageOption.Outcome).DefaultIfEmpty().AverageAsync()
                : await GetMatchSRs(averageOption.Name, averageOption.Role, averageOption.Outcome).DefaultIfEmpty().AverageAsync();

            Console.WriteLine(average == 0d
                ? $"No or not enough historic SR data for outcome '{averageOption.Outcome}'."
                : $"Average historic SR for outcome '{averageOption.Outcome}': {average:0}");
        }

        private static async IAsyncEnumerable<int> GetMatchSRs(string name, string role, string outcome)
        {
            Stack<int> orderedSRs = new Stack<int>(await GetOrderedSRs(name, role).ToListAsync());

            while (orderedSRs.Count > 1)
            {
                int sr = orderedSRs.Pop();
                int srChange = sr - orderedSRs.Peek();

                switch (outcome)
                {
                    case "win" when srChange > 0:
                    case "loss" when srChange < 0:
                    case "draw" when srChange == 0:
                        yield return sr;
                        break;
                }
            }
        }

        private static async IAsyncEnumerable<int> GetMatchSRChanges(string name, string role, string outcome)
        {
            Stack<int> orderedSRs = new Stack<int>(await GetOrderedSRs(name, role).ToListAsync());

            while (orderedSRs.Count > 0)
            {
                int sr = orderedSRs.Pop();

                if (!orderedSRs.TryPeek(out int peek))
                {
                    // break out if we can't peek a value (count is 0)
                    yield break;
                }

                int srChange = sr - peek;

                switch (outcome)
                {
                    case "win" when srChange > 0:
                    case "loss" when srChange < 0:
                    case "draw" when srChange == 0:
                    case "overall" when srChange != 0:
                        yield return Math.Abs(srChange);
                        break;
                }
            }
        }

        #endregion

        #region DisplayOption

        private static async ValueTask ProcessDisplayOption(DisplayOption displayOption)
        {
            if (!RolesHelper.ValidRoles.Contains(displayOption.Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{displayOption.Role}' (valid roles are {string.Join(", ", RolesHelper.ValidRoles.Select(role => $"'{role}'"))})."
                );
            }

            SqliteCommand command = await GetCommand(displayOption.Name, false);
            command.CommandText = $"SELECT * FROM {displayOption.Role} ORDER BY datetime(timestamp)";

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
                Console.WriteLine(); // add blank new line
                DisplayOption.Display("timestamp", "sr", "change", "map", "comment"); // headers
                Console.WriteLine($" {new string('-', 73)}"); // header-body seperator

                // body (values)
                int lastSR = historicData[0].SR;
                foreach ((string timestamp, int sr, string map, string comment) in historicData)
                {
                    int change = sr - lastSR;

                    switch (displayOption.Outcome)
                    {
                        case "win" when change > 0:
                        case "loss" when change < 0:
                        case "draw" when change == 0:
                        case "overall":
                            string changeString = change > 0 ? $"+{change}" : change.ToString(); // add positive-sign to wins
                            DisplayOption.Display(timestamp, sr.ToString(), changeString, map, comment);
                            lastSR = sr;
                            break;
                    }
                }
            }
        }

        #endregion

        #region ExportOption

        private static async ValueTask ProcessExportOption(ExportOption exportOption)
        {
            if (!RolesHelper.ValidRoles.Contains(exportOption.Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{exportOption.Role}' (valid roles are {string.Join(", ", RolesHelper.ValidRoles.Select(role => $"'{role}'"))})."
                );
            }

            SqliteCommand command = await GetCommand(exportOption.Name, false);
            command.CommandText = $"SELECT * FROM {exportOption.Role} ORDER BY datetime(timestamp)";

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
                using XLWorkbook workbook = ConstructSpreadsheet(historicData);
                workbook.SaveAs($"{exportOption.Name}_{exportOption.Role}.xlsx");
            }
        }

        private static XLWorkbook ConstructSpreadsheet(IReadOnlyList<(string Timestamp, int SR, string Map, string Comment)> historicData)
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Support");

            worksheet.Row(1).Height = 10;
            worksheet.Column(1).Width = 1;

            // headers
            worksheet.Cell("B2").Value = "timestamp";
            worksheet.Cell("B2").Style.Font.Bold = true;
            worksheet.Column("B").Width = 20;
            worksheet.Column("B").Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell("C2").Value = "sr";
            worksheet.Cell("C2").Style.Font.Bold = true;
            worksheet.Column("C").Width = 5;
            worksheet.Column("C").Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell("D2").Value = "change";
            worksheet.Cell("D2").Style.Font.Bold = true;
            worksheet.Column("D").Width = 7;
            worksheet.Column("D").Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell("E2").Value = "map";
            worksheet.Cell("E2").Style.Font.Bold = true;
            worksheet.Column("E").Width = 26;
            worksheet.Column("E").Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell("F2").Value = "comment";
            worksheet.Cell("F2").Style.Font.Bold = true;
            worksheet.Column("F").Width = 150;
            worksheet.Column("F").Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // data
            for (int index = 0; index < historicData.Count; index++)
            {
                (string timestamp, int sr, string map, string comment) = historicData[index];

                worksheet.Cell($"B{index + 3}").Value = timestamp;
                worksheet.Cell($"C{index + 3}").Value = sr;

                int change = index == (historicData.Count - 1) ? 0 : historicData[index + 1].SR - sr;
                string changeString = change > 0 ? $"+{change}" : change.ToString();
                worksheet.Cell($"D{index + 3}").Value = changeString;
                worksheet.Cell($"D{index + 3}").Style.Fill.BackgroundColor =
                    change > 0
                        ? XLColor.Green
                        : change < 0
                            ? XLColor.Red
                            : XLColor.Gray;

                worksheet.Cell($"E{index + 3}").Value = map;
                worksheet.Cell($"F{index + 3}").Value = comment;
            }

            return workbook;
        }

        #endregion

        #region Database

        private static async ValueTask<SqliteCommand> GetCommand(string name, bool newPlayer)
        {
            string databasePath = $@"{Environment.CurrentDirectory}/{name}.sqlite";

            if (!File.Exists(databasePath) && !newPlayer)
            {
                throw new InvalidOperationException
                (
                    $"No match history database has been created for player '{name}'. Use the '-n' flag to create it instead of throwing an error."
                );
            }

            SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}");
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand();
            return command;
        }

        private static async IAsyncEnumerable<int> GetOrderedSRs(string name, string role)
        {
            SqliteCommand command = await GetCommand(name, false);
            command.CommandText = $"SELECT sr FROM {role} ORDER BY datetime(timestamp)";

            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return reader.GetInt32(0);
            }
        }

        #endregion
    }
}
