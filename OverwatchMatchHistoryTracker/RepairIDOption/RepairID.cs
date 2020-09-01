#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using OverwatchMatchHistoryTracker.MatchOption;
using OverwatchMatchHistoryTracker.Options;

#endregion

namespace OverwatchMatchHistoryTracker.RepairIDOption
{
    [Verb("repairid")]
    public class RepairID : CommandNameOption
    {
        public override async ValueTask Process(MatchesContext matchesContext)
        {
            Console.WriteLine("Repairing `MatchID` to ensure it is consistent with timestamps.");

            List<Match> matches = await matchesContext.GetMatchesAsync().ToListAsync();

            int id = 1;
            foreach (Match match in matches)
            {
                if (match.ID != id)
                {
                    Match newMatch = new Match(match)
                    {
                        ID = id
                    };

                    matchesContext.Matches.Remove(match);
                    await matchesContext.Matches.AddAsync(newMatch);
                }

                id++;
            }

            Console.WriteLine("Repaired match IDs.");
        }
    }
}
