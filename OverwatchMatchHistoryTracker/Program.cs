#region

using System.Threading.Tasks;
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker
{
    internal class Program
    {
        private static async Task Main(string[] args) => await new OverwatchTracker().Process(args);
    }
}
