#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.DisplayOption;
using OverwatchMatchHistoryTracker.Options;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ConvertToAutoProperty

#endregion

namespace OverwatchMatchHistoryTracker.MatchOption
{
    [Verb(nameof(Match), HelpText = "Commits new match data.")]
    public class Match : CommandOption
    {
        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Commit match data to match history database", new Match
            {
                Name = "ShadowDragon",
                Role = Role.DPS,
                SR = 1675,
                Map = Map.Hanamura,
                Comment = "Didn't get enough healing."
            })
        };

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        private bool _Entropic;


        [Column(Order = 0)]
        public int MatchID { get; set; }

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

        public override async ValueTask Process(MatchesContext matchesContext)
        {
            int change = SR - ((await matchesContext.Matches.LastOrDefaultAsync())?.SR ?? SR);
            string changeString = Display.FormatSRChange(change);
            Display.TableDisplay
            (
                new Display.TableCell(Timestamp.ToString(Display.DATE_TIME_FORMAT), 19),
                new Display.TableCell(Entropic ? "True" : "False", 5),
                new Display.TableCell(SR.ToString(), 4),
                new Display.TableCell(changeString, 6),
                new Display.TableCell(Map.ToString(), 25),
                new Display.TableCell(Comment ?? string.Empty, 0)
            );
            await matchesContext.Matches.AddAsync(this);
        }
    }
}
