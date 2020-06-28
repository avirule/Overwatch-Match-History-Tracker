#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
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

        private string _Map;
        private int _SR;
        private string? _Comment;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

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
            set
            {
                string lower = value.ToLowerInvariant();
                _Map = MapsHelper.Aliases.ContainsKey(lower) ? MapsHelper.Aliases[lower] : lower;
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
            Map = _Comment = string.Empty;
            _SR = -1;
        }

        public override async ValueTask Process(MatchHistoryContext matchHistoryContext)
        {
            VerifyRole(Role);
            VerifySR(SR);
            VerifyMap(Map);
            await matchHistoryContext.Matches.AddAsync(this); // MatchOption implicitly converts to Match

            Console.WriteLine("Successfully committed match data.");
        }

        public static implicit operator Match(MatchOption matchOption) => new Match
        {
            Timestamp = DateTime.Now,
            Role = matchOption.Role,
            SR = matchOption.SR,
            Map = matchOption.Map,
            Comment = matchOption.Comment
        };
    }
}
