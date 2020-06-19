#region

using CommandLine;

#endregion

namespace OverwatchMatchHistoryTracker
{
    [Verb("collate", HelpText = "Collates previously tabulated data.")]
    public class CollateOption
    {
        private string _OutcomeConstraint;
        private string _Name;

        [Option('o', "outcome", Required = false, HelpText = "Constrains the collation to only matches with given outcome (win / loss / draw).")]
        public string OutcomeConstraint
        {
            get => _OutcomeConstraint;
            set => _OutcomeConstraint = value.ToLowerInvariant();
        }

        [Value(0, HelpText = "Name of player to collate data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }
    }
}
