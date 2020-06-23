#region

using System.Collections.Generic;

#endregion

namespace OverwatchMatchHistoryTracker.Helpers
{
    public class RolesHelper
    {
        public const string TANK = "tank";
        public const string DPS = "dps";
        public const string SUPPORT = "support";

        public static readonly HashSet<string> ValidRoles = new HashSet<string>
        {
            TANK,
            DPS,
            SUPPORT
        };
    }
}
