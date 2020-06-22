#region

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

#endregion

namespace OverwatchMatchHistoryTracker
{
    [Verb("collate", HelpText = "Collates previously tabulated data.")]
    public class CollateOption
    {
        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Get average historic SR", new CollateOption
            {
                Operation = "average",
                Name = "ShadowDragon",
                Role = "DPS"
            }),
            new Example("Get average historic SR change", new CollateOption
            {
                Operation = "averagec",
                Name = "ShadowDragon",
                Role = "DPS"
            })
        };

        private string _Operation;
        private string _Name;
        private string _Role;
        private string _Outcome;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Value(0, Required = true, HelpText = "Collation operation to run.")]
        public string Operation
        {
            get => _Operation;
            set => _Operation = value.ToLowerInvariant();
        }

        [Value(1, Required = true, HelpText = "Name of player to collate data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(2, Required = true, HelpText = "Role for which to collate data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        [Value(3, Required = false, HelpText = "Constrains the collation to only matches with given outcome (win / loss / draw).", Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set => _Outcome = value.ToLowerInvariant();
        }

        public CollateOption() => _Role = _Outcome = _Operation = _Name = string.Empty;
    }
}
