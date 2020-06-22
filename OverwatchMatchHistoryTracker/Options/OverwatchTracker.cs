#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Data.Sqlite;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    public class OverwatchTracker
    {
        private static readonly Type[] _OptionTypes =
        {
            typeof(MatchOption),
            typeof(AverageOption),
            typeof(DisplayOption)
        };

        private static readonly HashSet<string> _ValidRoles = new HashSet<string>
        {
            "tank",
            "dps",
            "support"
        };

        private static readonly string _CurrentDirectory = Environment.CurrentDirectory;
        private static readonly string _DatabasePathFormat = $@"{_CurrentDirectory}/{{0}}.sqlite";

        private SqliteConnection? _Connection;

        public async ValueTask Process(IEnumerable<string> args)
        {
            try
            {

                Parser.Default.ParseArguments(args, _OptionTypes).WithParsed(async parsed =>
                {
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
                        default:
                            throw new InvalidOperationException("Did not recognize given arguments.");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                async ValueTask StatefulClose()
                {
                    if (_Connection is null)
                    {
                        return;
                    }

                    await _Connection.CloseAsync();
                }

                await StatefulClose();
            }
        }

        #region MatchOption

        private async ValueTask ProcessMatchOption(MatchOption matchOption)
        {
            if (!_ValidRoles.Contains(matchOption.Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{matchOption.Role}' (valid roles are 'tank', 'dps', and 'support')."
                );
            }
            else if (!File.Exists(string.Format(_DatabasePathFormat, matchOption.Name)) && !matchOption.NewPlayer)
            {
                throw new InvalidOperationException
                (
                    $"No match history database has been created for player '{matchOption.Name}'. Use the '-n' flag to create it instead of throwing an error."
                );
            }

            SqliteCommand command = await GetCommand(matchOption.Name);
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

        private async ValueTask ProcessAverageOption(AverageOption averageOption)
        {
            if (averageOption.Change)
            {
                await DisplayAverageChange(averageOption.Name, averageOption.Role, averageOption.Outcome);
            }
            else
            {
                await DisplayAverageSR(averageOption.Name, averageOption.Role, averageOption.Outcome);
            }
        }

        private async ValueTask DisplayAverageSR(string name, string role, string outcome)
        {
            List<int> orderedSRs = await GetOrderedSRs(name, role).ToListAsync();

            if (orderedSRs.Count > 0)
            {
                for (int sr = orderedSRs[^1], index = orderedSRs.Count - 1; index > 0; index--, sr = orderedSRs[index])
                {
                    int srChange = sr - orderedSRs[index];

                    switch (outcome)
                    {
                        case "win" when srChange > 0:
                        case "loss" when srChange < 0:
                        case "draw" when srChange == 0:
                            // don't drop values that are valid for outcome
                            break;
                        default:
                            orderedSRs.RemoveAt(index);
                            break;
                    }
                }
            }

            Console.WriteLine(orderedSRs.Count == 0
                ? $"No historic SR data for outcome '{outcome}'."
                : $"Average historic SR for outcome '{outcome}': {orderedSRs.Average():0}");
        }

        private async ValueTask DisplayAverageChange(string name, string role, string outcome)
        {
            List<int> orderedSRs = await GetOrderedSRs(name, role).ToListAsync();
            List<int> outcomeSRChanges = new List<int>();

            if (orderedSRs.Count > 0)
            {
                for (int sr = orderedSRs[0], index = 1; index < orderedSRs.Count; sr = orderedSRs[index], index++)
                {
                    int srChange = orderedSRs[index] - sr;

                    switch (outcome.ToLowerInvariant())
                    {
                        case "win" when srChange > 0:
                        case "loss" when srChange < 0:
                        case "draw" when srChange == 0:
                        case "overall" when srChange != 0:
                            outcomeSRChanges.Add(Math.Abs(srChange));
                            break;
                    }
                }
            }

            Console.WriteLine(outcomeSRChanges.Count == 0
                ? $"No historic SR change data for outcome '{outcome}'."
                : $"Average historic SR change for outcome '{outcome}': {outcomeSRChanges.Average():0}");
        }

        #endregion


        private async ValueTask CollatePeak(string name, string role)
        {
            List<int> orderedSRs = await GetOrderedSRs(name, role).ToListAsync();

            Console.WriteLine(orderedSRs.Count == 0 ? "No historic SR data." : $"Peak historic SR: {orderedSRs.Max()}");
        }

        private async ValueTask CollateValley(string name, string role)
        {
            List<int> orderedSRs = await GetOrderedSRs(name, role).ToListAsync();

            Console.WriteLine(orderedSRs.Count == 0 ? "No historic SR data." : $"Valley historic SR: {orderedSRs.Min()}");
        }

        #region DisplayOption

        private async ValueTask ProcessDisplayOption(DisplayOption displayOption)
        {
            SqliteCommand command = await GetCommand(displayOption.Name);
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
                Console.WriteLine("No historic match data to display.");
            }
            else
            {
                DisplayOption.Display("timestamp", "sr", "change", "map", "comment");
                Console.WriteLine($" {new string('-', 73)}");

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

        private async ValueTask<SqliteCommand> GetCommand(string name)
        {
            _Connection = new SqliteConnection($"Data Source={string.Format(_DatabasePathFormat, name)}");
            await _Connection.OpenAsync();
            await using SqliteCommand command = _Connection.CreateCommand();
            return command;
        }

        private async IAsyncEnumerable<int> GetOrderedSRs(string name, string role)
        {
            SqliteCommand command = await GetCommand(name);
            command.CommandText = $"SELECT sr FROM {role} ORDER BY datetime(timestamp)";

            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return reader.GetInt32(0);
            }
        }
    }
}
