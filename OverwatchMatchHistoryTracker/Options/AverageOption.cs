#region

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("average", HelpText = "Get average historic SR.")]
    public class AverageOption : CollateOption
    {
        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Get average historic SR", new AverageOption
            {
                Name = "ShadowDragon",
                Role = "DPS"
            }),
        };

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Option('c', "change", Required = false, HelpText = "Returns average SR change instead.")]
        public bool Change { get; set; }

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player to collate data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for which to collate data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        [Value(2, MetaName = nameof(Outcome), Required = false,
            HelpText = "Constrains the collation to only matches with given outcome (win / loss / draw).", Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set => _Outcome = value.ToLowerInvariant();
        }

        public AverageOption() => _Role = _Outcome = _Name = string.Empty;
    }
}
