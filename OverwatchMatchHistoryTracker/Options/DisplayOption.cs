#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Data.Sqlite;
using OverwatchMatchHistoryTracker.Helpers;

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

        public static void Display(string timestamp, string sr, string change, string map, string comment)
        {
            Console.WriteLine(_DISPLAY_FORMAT,
                timestamp.PadLeft(10 + (timestamp.Length / 2)).PadRight(19),
                sr.PadLeft(2 + (sr.Length / 2)).PadRight(4),
                change.PadLeft(3 + (change.Length / 2)).PadRight(6),
                map.PadLeft(13 + (map.Length / 2)).PadRight(25),
                comment);
        }

        private string _Name;
        private string _Role;
        private string _Outcome;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player to display data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for which to display data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        [Value(2, MetaName = nameof(Outcome), Required = false, HelpText = "Only display matches of given outcome (win / loss/ draw).",
            Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set => _Outcome = value.ToLowerInvariant();
        }

        public DisplayOption() => _Name = _Role = _Outcome = string.Empty;

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
                Console.WriteLine(); // add blank new line
                Display("timestamp", "sr", "change", "map", "comment"); // headers
                Console.WriteLine($" {new string('-', 73)}"); // header-body separator

                // body (values)
                int lastSR = historicData[0].SR;
                foreach ((string timestamp, int sr, string map, string comment) in historicData)
                {
                    int change = sr - lastSR;

                    switch (Outcome)
                    {
                        case "win" when change > 0:
                        case "loss" when change < 0:
                        case "draw" when change == 0:
                        case "overall":
                            string changeString = change > 0 ? $"+{change}" : change.ToString(); // add positive-sign to wins
                            Display(timestamp, sr.ToString(), changeString, map, comment);
                            lastSR = sr;
                            break;
                    }
                }
            }
        }
    }
}
