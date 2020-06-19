#region

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ConvertToAutoProperty

#endregion

namespace OverwatchMatchHistoryTracker
{
    [Verb("match", HelpText = "Commits new match data.")]
    public class MatchOption
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

        private bool _NewPlayer;
        private string _Name;
        private string _Role;
        private string _Map;
        private int _SR;
        private string _Comment;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Option('n', "new", Required = false, HelpText = "Used to create new player match history database.")]
        public bool NewPlayer
        {
            get => _NewPlayer;
            set => _NewPlayer = value;
        }

        [Value(0, HelpText = "Name of player to log match info for.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, HelpText = "Role player queued as for match.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        [Value(2, HelpText = "Final SR after match ended.")]
        public int SR
        {
            get => _SR;
            set => _SR = value;
        }

        [Value(3, HelpText = "Name of map match took place on.")]
        public string Map
        {
            get => _Map;
            set => _Map = value.ToLowerInvariant();
        }

        [Value(4, Required = false, HelpText = "Personal comments for match.")]
        public string Comment
        {
            get => _Comment;
            set => _Comment = value;
        }

        public MatchOption()
        {
            _NewPlayer = false;
            _Name = _Role = _Map = _Comment = string.Empty;
            _SR = -1;
        }
    }
}
