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
            new Example("Commit match data to match history database", new CollateOption()
            {
                Name = "ShadowDragon",
                Role = "DPS"
            })
        };

        private string _Outcome;
        private string _Operation;
        private string _Name;
        private string _Role;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Option('o', "outcome", Required = false, HelpText = "Constrains the collation to only matches with given outcome (win / loss / draw).",
            Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set => _Outcome = value.ToLowerInvariant();
        }

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

        public CollateOption() => _Role = _Outcome = _Operation = _Name = string.Empty;
    }
}
