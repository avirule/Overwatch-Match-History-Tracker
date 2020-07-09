#region

using CommandLine;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    public abstract class CommandRoleOption : CommandOption
    {
        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role of player.")]
        public Role Role { get; set; }
    }
}
