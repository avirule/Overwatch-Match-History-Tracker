#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("average", HelpText = "Get average historic SR.")]
    public class AverageOption : CommandOption
    {
        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Get average historic SR", new AverageOption
            {
                Name = "ShadowDragon",
                Role = "DPS"
            }),
        };

        private string _Outcome;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Option('c', "change", Required = false, HelpText = "Returns average SR change instead.")]
        public bool Change { get; set; }

        [Value(2, MetaName = nameof(Outcome), Required = false,
            HelpText = "Constrains the collation to only matches with given outcome (win / loss / draw).", Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set
            {
                string outcome = value.ToLowerInvariant();

                // if (!_ValidOutcomes.Contains(outcome))
                // {
                //     throw new InvalidOperationException($"Given outcome must be: {string.Join(", ", _ValidOutcomes)}");
                // }

                _Outcome = outcome;
            }
        }

        public AverageOption() => _Outcome = string.Empty;

        public override async ValueTask Process(MatchHistoryContext matchHistoryContext)
        {
            VerifyRole(Role);

            IAsyncEnumerable<Match> matches = matchHistoryContext.GetOrderedMatches().Where(match => match.Role.Equals(Role));

            double average = Change
                ? await GetMatchSRs(matches, Outcome, true).DefaultIfEmpty().AverageAsync()
                : await GetMatchSRs(matches, Outcome, false).DefaultIfEmpty().AverageAsync();

            Console.WriteLine(average == 0d
                ? $"No or not enough historic SR data for outcome '{Outcome}'."
                : $"Average historic SR for outcome '{Outcome}': {average:0}");
        }

        private static async IAsyncEnumerable<int> GetMatchSRs(IAsyncEnumerable<Match> matches, string outcome, bool change)
        {
            int lastSR = -1;
            await foreach (Match match in matches)
            {
                if (!match.Entropic)
                {
                    lastSR = -1;
                }

                if (lastSR > -1)
                {
                    int srChange = match.SR - lastSR;

                    switch (outcome)
                    {
                        case "win" when srChange > 0:
                        case "loss" when srChange < 0:
                        case "draw" when srChange == 0:
                        case "overall":
                            yield return change ? Math.Abs(srChange) : match.SR;
                            break;
                    }
                }

                lastSR = match.SR;
            }
        }
    }
}
