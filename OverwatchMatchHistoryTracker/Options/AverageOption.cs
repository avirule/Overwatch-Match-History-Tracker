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
            set => _Outcome = value.ToLowerInvariant();
        }

        public AverageOption() => _Outcome = string.Empty;

        public override async ValueTask Process(MatchHistoryContext matchHistoryContext)
        {
            VerifyRole(Role);

            IAsyncEnumerable<int> srs = matchHistoryContext.Matches.ToAsyncEnumerable().Where(match => match.Role.Equals(Role))
                .Select(match => match.SR);

            double average = Change
                ? await GetMatchSRChanges(srs, Outcome).DefaultIfEmpty().AverageAsync()
                : await GetMatchSRs(srs, Outcome).DefaultIfEmpty().AverageAsync();

            Console.WriteLine(average == 0d
                ? $"No or not enough historic SR data for outcome '{Outcome}'."
                : $"Average historic SR for outcome '{Outcome}': {average:0}");
        }

        private static async IAsyncEnumerable<int> GetMatchSRs(IAsyncEnumerable<int> srs, string outcome)
        {
            int lastSR = -1;
            await foreach (int sr in srs)
            {
                if (lastSR > -1)
                {
                    int srChange = lastSR - sr;

                    switch (outcome)
                    {
                        case "win" when srChange > 0:
                        case "loss" when srChange < 0:
                        case "draw" when srChange == 0:
                        case "overall":
                            yield return sr;
                            break;
                    }
                }

                lastSR = sr;
            }
        }

        private static async IAsyncEnumerable<int> GetMatchSRChanges(IAsyncEnumerable<int> srs, string outcome)
        {
            int lastSR = -1;
            await foreach (int sr in srs)
            {
                if (lastSR > -1)
                {
                    int srChange = lastSR - sr;

                    switch (outcome)
                    {
                        case "win" when srChange > 0:
                        case "loss" when srChange < 0:
                        case "draw" when srChange == 0:
                        case "overall" when srChange != 0:
                            yield return Math.Abs(srChange);
                            break;
                    }
                }

                lastSR = sr;
            }
        }
    }
}
