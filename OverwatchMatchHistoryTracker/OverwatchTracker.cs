#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Data.Sqlite;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class OverwatchTracker
    {
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
                Parser.Default.ParseArguments<MatchOption, CollateOption>(args).WithParsed(async obj =>
                {
                    switch (obj)
                    {
                        case MatchOption matchOption:
                            if (!string.IsNullOrEmpty(matchOption.Map) && MapsHelper.Aliases.ContainsKey(matchOption.Map))
                            {
                                matchOption.Map = MapsHelper.Aliases[matchOption.Map];
                            }

                            await ProcessMatchOption(matchOption);
                            break;
                        case CollateOption collateOption:
                            await ProcessCollateOption(collateOption);
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
            if (!string.IsNullOrEmpty(matchOption.Map) && MapsHelper.Aliases.ContainsKey(matchOption.Map))
            {
                matchOption.Map = MapsHelper.Aliases[matchOption.Map];
            }

            ValidateArguments(matchOption);
            await VerifyDatabase(matchOption);
            await CommitMatchInfo(matchOption);

            Console.WriteLine("Successfully committed match data.");
        }

        private static void ValidateArguments(MatchOption matchOption)
        {
            Debug.Assert(!(matchOption is null), "MatchOption should be parsed prior to this point.");

            if (!_ValidRoles.Contains(matchOption.Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{matchOption.Role}' (valid roles are 'tank', 'dps', and 'support')."
                );
            }
            else if (!MapsHelper.Valid.Contains(matchOption.Map))
            {
                throw new InvalidOperationException
                (
                    $"Invalid map provided: '{matchOption.Map}'"
                );
            }
            else if ((matchOption.SR < 0) || (matchOption.SR > 6000))
            {
                throw new InvalidOperationException
                (
                    "Provided SR value must be between 0 and 6000 (minimum and maximum as determined by Blizzard)."
                );
            }
            else if (!File.Exists(string.Format(_DatabasePathFormat, matchOption.Name)) && !matchOption.NewPlayer)
            {
                throw new InvalidOperationException
                (
                    $"No match history database has been created for player '{matchOption.Name}'. Use the '-n' flag to create it instead of throwing an error."
                );
            }
        }

        private async ValueTask VerifyDatabase(MatchOption matchOption)
        {
            Debug.Assert(!(matchOption is null), "MatchOption should be parsed prior to this point.");

            _Connection = new SqliteConnection($"Data Source={string.Format(_DatabasePathFormat, matchOption.Name)}");
            await _Connection.OpenAsync();
            await using SqliteCommand command = _Connection.CreateCommand();

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
        }

        private async ValueTask CommitMatchInfo(MatchOption matchOption)
        {
            Debug.Assert(!(_Connection is null), "Connection should be initialized prior to this point.");
            Debug.Assert(!(matchOption is null), "MatchOption should be parsed prior to this point.");

            await using SqliteCommand command = _Connection.CreateCommand();

            command.CommandText = $"INSERT INTO {matchOption.Role} (timestamp, sr, map, comment) VALUES (datetime(), $sr, $map, $comment)";
            command.Parameters.AddWithValue("$sr", matchOption.SR);
            command.Parameters.AddWithValue("$map", matchOption.Map);
            command.Parameters.AddWithValue("$comment", matchOption.Comment);
            await command.ExecuteNonQueryAsync();
        }

        #endregion

        #region CollateOption

        private async ValueTask ProcessCollateOption(CollateOption collateOption)
        {
            switch (collateOption.Operation)
            {
                case "average":
                    await CollateAverage(collateOption.Name, collateOption.Role, collateOption.Outcome);
                    break;
                case "averagec":
                    await CollateAverageChange(collateOption.Name, collateOption.Role, collateOption.Outcome);
                    break;
            }
        }

        private async ValueTask CollateAverage(string name, string role, string outcome)
        {
            _Connection = new SqliteConnection($"Data Source={string.Format(_DatabasePathFormat, name)}");
            await _Connection.OpenAsync();
            await using SqliteCommand command = _Connection.CreateCommand();
            command.CommandText = $"SELECT sr FROM {role} ORDER BY datetime(timestamp)";

            List<int> orderedSRs = new List<int>();
            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orderedSRs.Add(reader.GetInt32(0));
            }

            for (int sr = orderedSRs[0], index = 1; index < orderedSRs.Count; index++, sr = orderedSRs[index])
            {
                int srChange = orderedSRs[index] - sr;

                switch (outcome.ToLowerInvariant())
                {
                    case "win" when srChange > 0:
                    case "loss" when srChange > 0:
                    case "draw" when srChange == 0:
                        // don't drop values that are valid for outcome
                        break;
                    default:
                        orderedSRs.RemoveAt(index);
                        break;
                }
            }

            Console.WriteLine(orderedSRs.Count == 0
                ? $"No historic SR data for outcome '{outcome}'."
                : $"Average historic SR for outcome '{outcome}': {orderedSRs.Average():0}");
        }

        private async ValueTask CollateAverageChange(string name, string role, string outcome)
        {
            _Connection = new SqliteConnection($"Data Source={string.Format(_DatabasePathFormat, name)}");
            await _Connection.OpenAsync();
            await using SqliteCommand command = _Connection.CreateCommand();
            command.CommandText = $"SELECT sr FROM {role} ORDER BY datetime(timestamp)";

            List<int> orderedSRs = new List<int>();
            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                orderedSRs.Add(reader.GetInt32(0));
            }

            List<int> outcomeSRChanges = new List<int>();
            for (int sr = orderedSRs[0], index = 1; index < orderedSRs.Count; index++, sr = orderedSRs[index])
            {
                int srChange = orderedSRs[index] - sr;

                switch (outcome.ToLowerInvariant())
                {
                    case "win" when srChange > 0:
                    case "loss" when srChange > 0:
                    case "draw" when srChange == 0:
                    case "overall" when srChange != 0:
                        outcomeSRChanges.Add(Math.Abs(srChange));
                        break;
                }
            }

            Console.WriteLine(orderedSRs.Count == 0
                ? $"No historic SR change data for outcome '{outcome}'."
                : $"Average historic SR change for outcome '{outcome}': {outcomeSRChanges.Average():0}");
        }

        #endregion
    }
}
