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

            Stack<int> srs = new Stack<int>(await ((IAsyncEnumerable<Match>)matchHistoryContext.Matches).Where(match => match.Role.Equals(Role))
                .Select(match => match.SR).ToListAsync());

            double average = Change
                ? await GetMatchSRChanges(srs, Outcome).DefaultIfEmpty().AverageAsync()
                : await GetMatchSRs(srs, Outcome).DefaultIfEmpty().AverageAsync();

            Console.WriteLine(average == 0d
                ? $"No or not enough historic SR data for outcome '{Outcome}'."
                : $"Average historic SR for outcome '{Outcome}': {average:0}");
        }

        private static async IAsyncEnumerable<int> GetMatchSRs(Stack<int> srs, string outcome)
        {
            while (srs.Count > 1)
            {
                int sr = srs.Pop();
                int srChange = sr - srs.Peek();

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
        }

        private static async IAsyncEnumerable<int> GetMatchSRChanges(Stack<int> srs, string outcome)
        {
            while (srs.Count > 0)
            {
                int sr = srs.Pop();

                if (!srs.TryPeek(out int peek))
                {
                    // break out if we can't peek a value (count is 0)
                    yield break;
                }

                int srChange = sr - peek;

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
        }
    }
}
