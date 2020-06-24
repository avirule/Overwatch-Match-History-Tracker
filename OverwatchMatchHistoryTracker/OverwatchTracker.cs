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

                switch (parsed)
                {
                    case MatchOption matchOption:
                        await MatchOption.Process(matchOption);
                        break;
                    case AverageOption averageOption:
                        await AverageOption.Process(averageOption);
                        break;
                    case DisplayOption displayOption:
                        await DisplayOption.Process(displayOption);
                        break;
                    case ExportOption exportOption:
                        await ExportOption.Process(exportOption);
                        break;
                    default:
                        throw new InvalidOperationException("Did not recognize given arguments.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} Operation not completed.");
            }
        }
    }
}
