#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.MatchOption;
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker.WinRateOption
{
    [Verb("winrate", HelpText = _HELP_TEXT)]
    public class WinRate : CommandRoleOption
    {
        private const string _HELP_TEXT = "Calculates winrate for a given player and role.";

        [Usage]
        public static IEnumerable<Example> Examples { get; } = new List<Example>
        {
            new Example(_HELP_TEXT, new WinRate
            {
                Name = "ShadowDragon",
                Role = Role.DPS,
            })
        };

        public override async ValueTask Process(MatchesContext matchesContext)
        {
            List<Match> total = await matchesContext.GetMatchesByRoleAsync(Role).ToListAsync();
            IAsyncEnumerable<(Match Match, int Change)> wins = matchesContext.GetMatchesByOutcomeAsync(total.ToAsyncEnumerable(), Outcome.Win);

            double sumWins = await wins.CountAsync();
            double sumTotal = total.Count;

            double winRate = sumWins / sumTotal;
            double percentageWinRate = winRate * 100d;

            Console.WriteLine($"Winrate for {Role} is: {percentageWinRate:0.00}%");
        }
    }
}
