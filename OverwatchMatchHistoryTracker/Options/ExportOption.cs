#region

using CommandLine;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("export", HelpText = "Exports a match history database to another format.")]
    public class ExportOption
    {
        private string _Name;
        private string _Role;
        private string _Format;

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player to use data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for which to use data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        // [Value(2, MetaName = nameof(Format), Required = true, HelpText = "Format of export.")]
        // public string Format
        // {
        //     get => _Format;
        //     set => _Format = value.ToLowerInvariant();
        // }

        public ExportOption() => _Name = _Role = _Format = string.Empty;
    }
}
