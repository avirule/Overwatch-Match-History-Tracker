#region

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ConvertToAutoProperty

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class MatchInfo
    {
        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Commit match data to match history database", new MatchInfo
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

        [Option('n', "new", Required = false, HelpText = "Used to create new player match history database.")]
        public bool NewPlayer { get; set; }

        [Value(0, HelpText = "Name of player to log match info for.")]
        public string Name
        {
            get => _Name;
            private set => _Name = value.ToLowerInvariant();
        }

        [Value(1, HelpText = "Role player queued as for match.")]
        public string Role
        {
            get => _Role;
            private set => _Role = value.ToLowerInvariant();
        }

        [Value(2, HelpText = "Final SR after match ended.")]
        public int SR
        {
            get => _SR;
            private set => _SR = value;
        }

        [Value(3, HelpText = "Name of map match took place on.")]
        public string Map
        {
            get => _Map;
            private set => _Map = value.ToLowerInvariant();
        }

        [Option('c', "comment", Required = false, HelpText = "Personal comments for match.")]
        public string Comment
        {
            get => _Comment;
            private set => _Comment = value;
        }

        public MatchInfo()
        {
            _Name = _Role = _Map = _Comment = string.Empty;
            SR = -1;
        }
    }
}
