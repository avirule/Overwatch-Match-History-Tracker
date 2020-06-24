#region

using System.Threading.Tasks;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    public abstract class CommandOption
    {
        public abstract ValueTask Process();
    }
}
