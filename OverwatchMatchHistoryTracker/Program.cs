#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

#endregion

namespace OverwatchMatchHistoryTracker
{
    internal class Program
    {
        private static readonly IReadOnlyList<string> _ValidMaps = new[]
        {
            "Blizzard World",
            "Busan",
            "Dorado",
            "Eichenwalde",
            "Hanamura",
            "Havana",
            "Hollywood",
            "Horizon Lunar Colony",
            "Ilios",
            "Junkertown",
            "King's Row",
            "Lijiang Tower",
            "Nepal",
            "Numbani",
            "Oasis",
            "Paris",
            "Rialto",
            "Route 66",
            "Temple of Anubis",
            "Volskaya Industries",
            "Watchpoint: Gibraltar",
        };

        // -c "total shit" riki support 2555 hanamura

        private static readonly string _CurrentDirectory = Environment.CurrentDirectory;
        private static readonly string _DatabasePathFormat = $@"{_CurrentDirectory}/{{0}}.sqlite";

        private static async Task Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Not enough arguments." /* todo example usage */);
                Environment.Exit(-1);
            }

            File.Delete(string.Format(_DatabasePathFormat, args[0]));

            await VerifyPrerequisiteFiles(args[0], args[1]);
        }

        private static async ValueTask VerifyPrerequisiteFiles(string name, string role)
        {
            await using SqliteConnection connection = new SqliteConnection($"Data Source={string.Format(_DatabasePathFormat, name)}");
            await using SqliteCommand command = connection.CreateCommand();

            await connection.OpenAsync();

            command.CommandText = $@"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{role}_history'";
            if (await command.ExecuteScalarAsync() == null)
            {
                command.CommandText =
                    $@"
                        CREATE TABLE {role}_history 
                            (
                                timestamp TEXT NOT NULL,
                                map TEXT NOT NULL CHECK
                                    (
                                        {string.Join(" OR ", _ValidMaps.Select(map => $"map = \"{map}\""))}
                                    ),
                                sr INT NOT NULL CHECK (sr >= 0 AND sr <= 6000),
                                comment TEXT
                            )
                    ";
                await command.ExecuteNonQueryAsync();
            }

            command.CommandText = $"INSERT INTO {role}_history (timestamp, map, sr, comment) VALUES (datetime(), $map, $sr, $comment)";
            command.Parameters.AddWithValue("$map", "Hanamura");
            command.Parameters.AddWithValue("$sr", "2555");
            command.Parameters.AddWithValue("$comment", "");
            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }
    }
}
