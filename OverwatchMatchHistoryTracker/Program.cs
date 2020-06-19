#region

using System.Threading.Tasks;

#endregion

namespace OverwatchMatchHistoryTracker
{
    internal class Program
    {
        // -c "total shit" riki support 2555 hanamura


        private static async Task Main(string[] args)
        {
            OverwatchTracker tracker = new OverwatchTracker(args);
            await tracker.Process();
        }
    }
}
