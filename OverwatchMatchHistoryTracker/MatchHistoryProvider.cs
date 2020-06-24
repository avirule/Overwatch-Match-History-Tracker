#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OverwatchMatchHistoryTracker.Helpers;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public static class MatchHistoryProvider
    {
        public static async ValueTask<SqliteCommand> GetDatabaseCommand(string name)
        {
            string databasePath = $@"{Environment.CurrentDirectory}/{name}.sqlite";

            if (!File.Exists(databasePath))
            {
                Console.Write($"No match history database exists for '{name}'. Would you like to create one (y / n)? ");
                ConsoleKey key = Console.ReadKey().Key;
                Console.Write("\r\n");

                switch (key)
                {
                    case ConsoleKey.Y:
                        break;
                    default:
                        throw new InvalidOperationException("Match history database not found.");
                }
            }

            SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}");
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand();
            return command;
        }

        public static async ValueTask VerifyRoleTableExists(SqliteCommand command, string role)
        {
            if (!RolesHelper.Valid.Contains(role))
            {
                throw new ArgumentException("Invalid role provided.", nameof(role));
            }
            else
            {
                command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{role}'";
                object result = await command.ExecuteScalarAsync();

                if (result is null)
                {
                    throw new InvalidOperationException($"No data for role '{role}' exists.");
                }
            }
        }

        public static async IAsyncEnumerable<int> GetOrderedSRs(string name, string role)
        {
            SqliteCommand command = await GetDatabaseCommand(name);
            await VerifyRoleTableExists(command, role);
            command.CommandText = $"SELECT sr FROM {role} ORDER BY datetime(timestamp)";

            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return reader.GetInt32(0);
            }
        }
    }
}
