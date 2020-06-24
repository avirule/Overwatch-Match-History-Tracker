#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Data.Sqlite;
using OverwatchMatchHistoryTracker.Helpers;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ConvertToAutoProperty

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("match", HelpText = "Commits new match data.")]
    public class MatchOption : CommandOption
    {
        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Commit match data to match history database", new MatchOption
            {
                Name = "ShadowDragon",
                Role = "DPS",
                SR = 1675,
                Map = "Hanamura",
                Comment = "Didn't get enough healing."
            })
        };

        private string _Name;
        private string _Role;
        private string _Map;
        private int _SR;
        private string _Comment;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player to log match info for.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role player queued as for match.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        [Value(2, MetaName = nameof(SR), Required = true, HelpText = "Final SR after match ended.")]
        public int SR
        {
            get => _SR;
            set => _SR = value;
        }

        [Value(3, MetaName = nameof(Map), Required = true, HelpText = "Name of map match took place on.")]
        public string Map
        {
            get => _Map;
            set => _Map = value.ToLowerInvariant();
        }

        [Value(4, MetaName = nameof(Comment), Required = false, HelpText = "Personal comments for match.")]
        public string Comment
        {
            get => _Comment;
            set => _Comment = value;
        }

        public MatchOption()
        {
            _Name = _Role = _Map = _Comment = string.Empty;
            _SR = -1;
        }

        public override async ValueTask Process()
        {
            if (!RolesHelper.Valid.Contains(Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{Role}' (valid roles are 'tank', 'dps', and 'support')."
                );
            }

            SqliteCommand command = await MatchHistoryProvider.GetDatabaseCommand(Name);
            command.CommandText =
                $@"
                    CREATE TABLE IF NOT EXISTS {Role}
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

            if (MapsHelper.Aliases.ContainsKey(Map))
            {
                Map = MapsHelper.Aliases[Map];
            }

            command.CommandText = $"INSERT INTO {Role} (timestamp, sr, map, comment) VALUES (datetime(), $sr, $map, $comment)";
            command.Parameters.AddWithValue("$sr", SR);
            command.Parameters.AddWithValue("$map", Map);
            command.Parameters.AddWithValue("$comment", Comment);
            await command.ExecuteNonQueryAsync();

            Console.WriteLine("Successfully committed match data.");
        }
    }
}
