#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using DocumentFormat.OpenXml.Wordprocessing;
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

        private bool _Entropic;
        private Map _Map;
        private int _SR;
        private string? _Comment;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Option('e', "entropic", Required = false, HelpText = "Indicates match should not be collated for entropic data (averages, for instance). This can be used when there is a gap in committed match history.")]
        public bool Entropic
        {
            get => _Entropic;
            set => _Entropic = !value;
        }

        [Value(2, MetaName = nameof(SR), Required = true, HelpText = "Final SR after match ended.")]
        public int SR
        {
            get => _SR;
            set
            {
                VerifySR(value);
                _SR = value;
            }
        }

        [Value(3, MetaName = nameof(Map), Required = true, HelpText = "Name of map match took place on.")]
        public string Map
        {
            get => _Map.ToString();
            set
            {
                string mapString = value.ToLowerInvariant();
                Map map = MapsHelper.Aliases.ContainsKey(mapString) ? MapsHelper.Aliases[mapString] : Enum.Parse<Map>(mapString, true);
                _Map = map;
            }
        }

        [Value(4, MetaName = nameof(Comment), Required = false, HelpText = "Personal comments for match.")]
        public string? Comment
        {
            get => _Comment;
            set => _Comment = value;
        }

        public MatchOption()
        {
            CompleteText = "Successfully committed match data.";
            _Comment = string.Empty;
            _SR = -1;
        }

        public override async ValueTask Process(MatchHistoryContext matchHistoryContext) => await matchHistoryContext.Matches.AddAsync(this);

        public static implicit operator Match(MatchOption matchOption) => new Match
        {
            Timestamp = DateTime.Now,
            Entropic = matchOption.Entropic,
            Role = matchOption.Role,
            SR = matchOption.SR,
            Map = matchOption._Map,
            Comment = matchOption.Comment
        };
    }
}
