#region

using System;
using System.Threading.Tasks;

#endregion

namespace OverwatchMatchHistoryTracker
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            OverwatchTracker tracker = new OverwatchTracker(args);
            await tracker.Process();

            Console.WriteLine("Successfully committed match data.");
        }
    }
}
