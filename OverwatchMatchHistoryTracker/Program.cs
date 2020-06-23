#define UNIT_TEST

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            OverwatchTracker tracker = new OverwatchTracker();

#if DEBUG && UNIT_TEST
            foreach (string[] unitTestArgs in _UnitTestArgs.Values.SelectMany(unitTestArgsCollection => unitTestArgsCollection))
            {
                await tracker.Process(unitTestArgs);
            }
#else
            await tracker.Process(args);
#endif
        }

#if DEBUG && UNIT_TEST

        private static readonly Dictionary<Type, string[][]> _UnitTestArgs = new Dictionary<Type, string[][]>
        {
            {
                typeof(MatchOption), new[]
                {
                    new[]
                    {
                        "match",
                        "riki",
                        "support",
                        "2527",
                        "hanamura",
                        "unit test comment"
                    }
                }
            },
            {
                typeof(AverageOption), new[]
                {
                    new[]
                    {
                        "average",
                        "riki",
                        "support"
                    },
                    new[]
                    {
                        "average",
                        "riki",
                        "support",
                        "win"
                    },
                    new[]
                    {
                        "average",
                        "riki",
                        "support",
                        "loss"
                    },
                    new[]
                    {
                        "average",
                        "-c",
                        "riki",
                        "support"
                    },
                    new[]
                    {
                        "average",
                        "-c",
                        "riki",
                        "support",
                        "win"
                    },
                    new[]
                    {
                        "average",
                        "-c",
                        "riki",
                        "support",
                        "loss"
                    }
                }
            },
            {
                typeof(DisplayOption), new[]
                {
                    new[]
                    {
                        "display",
                        "riki",
                        "support"
                    },
                    new[]
                    {
                        "display",
                        "riki",
                        "support",
                        "win"
                    },

                    new[]
                    {
                        "display",
                        "riki",
                        "support",
                        "loss"
                    }
                }
            }
        };

#endif
    }
}
