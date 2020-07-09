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
            IAsyncEnumerable<(Match Match, int Change)> wins = matchesContext.GetMatchesByOutcomeAsync(Role, Outcome.Win);
            IAsyncEnumerable<(Match match, int Change)> losses = matchesContext.GetMatchesByOutcomeAsync(Role, Outcome.Loss);

            double sumWins = await wins.CountAsync();
            double sumLosses = await losses.CountAsync();
            double sumTotal = sumWins + sumLosses;

            double winRate = sumWins / sumTotal;

            Console.WriteLine($"Winrate is: {winRate:0.00}");
        }
    }
}
