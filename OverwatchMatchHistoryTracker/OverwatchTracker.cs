#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OverwatchMatchHistoryTracker.Options;
using OverwatchMatchHistoryTracker.Options.AverageOption;
using OverwatchMatchHistoryTracker.Options.DisplayOption;
using OverwatchMatchHistoryTracker.Options.ExportOption;
using OverwatchMatchHistoryTracker.Options.MatchOption;
using OverwatchMatchHistoryTracker.Options.ModifyOption;
using OverwatchMatchHistoryTracker.Options.WinRateOption;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class OverwatchTracker
    {
        private static readonly Type[] _OptionTypes =
        {
            typeof(Match),
            typeof(Average),
            typeof(Display),
            typeof(Export),
            typeof(WinRate),
            typeof(Modify)
            // typeof(RepairID)
        };

        public static async ValueTask Process(IEnumerable<string> args)
        {
            try
            {
                object? parsed = null;
                Parser parser = new Parser(settings =>
                {
                    settings.HelpWriter = Console.Error;
                    settings.CaseSensitive = false;
                    settings.CaseInsensitiveEnumValues = true;
                });

                parser.ParseArguments(args, _OptionTypes).WithParsed(obj => parsed = obj);

                if (parsed is CommandOption commandOption)
                {
                    MatchesContext matchesContext = await MatchesContext.GetMatchesContext(commandOption.Name);
                    await commandOption.Process(matchesContext);
                    await matchesContext.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(commandOption.ProcessingFinishedMessage))
                    {
                        Console.WriteLine(commandOption.ProcessingFinishedMessage);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Did not recognize given arguments.");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqliteException sqliteException)
                {
                    switch (sqliteException.SqliteErrorCode)
                    {
                        case 1:
                            Console.WriteLine(
                                "A database format error has occurred. The referenced database may be from an older version, or corrupted.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"{ex.InnerException?.Message ?? "No inner exception."} Operation not completed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} Operation not completed.");
            }
        }
    }
}
