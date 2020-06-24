#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class OverwatchTracker
    {
        private static readonly Type[] _OptionTypes =
        {
            typeof(MatchOption),
            typeof(AverageOption),
            typeof(DisplayOption),
            typeof(ExportOption)
        };

        public static async ValueTask Process(IEnumerable<string> args)
        {
            try
            {
                object? parsed = null;
                Parser.Default.ParseArguments(args, _OptionTypes).WithParsed(obj => parsed = obj);

                if (parsed is CommandOption commandOption)
                {
                    await commandOption.Process();
                    return;
                }

                throw new InvalidOperationException("Did not recognize given arguments.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} Operation not completed.");
            }
        }
    }
}
