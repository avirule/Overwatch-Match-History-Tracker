#region

using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using OverwatchMatchHistoryTracker.Helpers;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    public abstract class CommandOption
    {
        private string _Name;
        private string _Role;

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for player.")]
        public string Role
        {
            get => _Role;
            set
            {
                string role = value.ToLowerInvariant();
                VerifyRole(role);
                _Role = role;
            }
        }

        public string? CompleteText { get; protected set; }

        public CommandOption() => _Name = _Role = CompleteText = string.Empty;

        public abstract ValueTask Process(MatchHistoryContext matchHistoryContext);

        protected static void VerifyRole(string role)
        {
            if (!RolesHelper.Valid.Contains(role))
            {
                throw new ArgumentException($"Given role must be any of {string.Join(", ", RolesHelper.Valid.Select(role => "'role'"))}.");
            }
        }

        protected static void VerifySR(int sr)
        {
            if ((sr > 6000) || (sr < 0))
            {
                throw new ArgumentException("Given SR must be between 0 and 6000.");
            }
        }

        protected static void VerifyMap(string map)
        {
            if (!MapsHelper.Valid.Contains(map))
            {
                throw new ArgumentException("Given map is not valid.");
            }
        }
    }
}
