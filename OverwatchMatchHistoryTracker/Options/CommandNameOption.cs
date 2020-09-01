#region

using System.ComponentModel.DataAnnotations.Schema;
using CommandLine;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    public abstract class CommandNameOption : CommandOption
    {
        private string _Name;

        [NotMapped]
        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of account.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        public CommandNameOption() => _Name = string.Empty;
    }
}
