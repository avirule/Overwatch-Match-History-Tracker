#region

using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using CommandLine;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    public abstract class CommandOption
    {
        private string _Name;

        [NotMapped]
        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of account.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [NotMapped]
        public string? ProcessingFinishedMessage { get; protected set; }

        public CommandOption() => _Name = string.Empty;

        public abstract ValueTask Process(MatchesContext matchesContext);
    }
}
