#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.Options.DisplayOption;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ConvertToAutoProperty

#endregion

namespace OverwatchMatchHistoryTracker.Options.MatchOption
{
    [Verb(nameof(Match), HelpText = _HELP_TEXT)]
    public class Match : CommandOption
    {
        public enum Header
        {
            Timestamp,
            Entropic,
            Role,
            SR,
            Map,
            Comment
        }

        private const string _HELP_TEXT = "Commit match data to match history database";

        [Usage]
        public static IEnumerable<Example> Examples { get; } = new List<Example>
        {
            new Example(_HELP_TEXT, new Match
            {
                Name = "ShadowDragon",
                Role = Role.DPS,
                SR = 1675,
                Map = Map.Hanamura,
                Comment = "Didn't get enough healing."
            })
        };

        private bool _Entropic;

        [Column(Order = 0)]
        public int ID { get; set; }

        [Column(Order = 1)]
        public DateTime Timestamp { get; set; }

        [Column(Order = 2)]
        [Option('e', "entropic", Required = false, Default = false /* equates to True in setter */, HelpText =
            "Indicates match should not be collated for entropic data (averages, for instance). This can be used when there is a gap in committed match history.")]
        public bool Entropic
        {
            get => _Entropic;
            set => _Entropic = !value;
        }

        [Column(Order = 3)]
        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for player.")]
        public Role Role { get; set; }

        [Column(Order = 4)]
        [Value(2, MetaName = nameof(SR), Required = true, HelpText = "SR after match ended.")]
        public int SR { get; set; }

        [Column(Order = 5)]
        [Value(3, MetaName = nameof(Map), Required = true, HelpText = "Name of map match took place on.")]
        public Map Map { get; set; }

        [Column(Order = 6)]
        [Value(4, MetaName = nameof(Comment), Required = false, HelpText = "Personal comments for match.")]
        public string? Comment { get; set; }

        public Match()
        {
            ProcessingFinishedMessage = "Successfully committed match data.";
            Timestamp = DateTime.Now;
            SR = -1;
        }

        public Match(Match match) => (ID, Timestamp, Name, Role, SR, Map, Comment) =
            (match.ID, match.Timestamp, match.Name, match.Role, match.SR, match.Map, match.Comment);

        public override async ValueTask Process(MatchesContext matchesContext)
        {
            int change = SR - ((await matchesContext.GetMatchesByRoleAsync(Role).LastOrDefaultAsync())?.SR ?? SR);
            string changeString = Display.FormatSRChange(change);
            Display.TableDisplay
            (
                new TableCell(Timestamp.ToString(Display.DATE_TIME_FORMAT), 19),
                new TableCell(Entropic ? "True" : "False", 5),
                new TableCell(SR.ToString(), 4),
                new TableCell(changeString, 6),
                new TableCell(Map.ToString(), 25),
                new TableCell(Comment ?? string.Empty, 0)
            );
            await matchesContext.Matches.AddAsync(this);
        }
    }
}
