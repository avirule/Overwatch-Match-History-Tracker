#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.Options.MatchOption;

#endregion

namespace OverwatchMatchHistoryTracker.Options.AverageOption
{
    [Verb(nameof(Average), HelpText = _HELP_TEXT)]
    public class Average : CommandRoleOption
    {
        private const string _HELP_TEXT = "Get average historic SR.";

        [Usage]
        public static IEnumerable<Example> Examples { get; } = new List<Example>
        {
            new Example(_HELP_TEXT, new Average
            {
                Name = "ShadowDragon",
                Role = Role.DPS
            }),
        };

        [Option('c', "change", Required = false, HelpText = "Returns average SR change instead.")]
        public bool Change { get; set; }

        [Value(2, MetaName = nameof(Outcome), Required = false, Default = Outcome.Overall,
            HelpText = "Constrains the collation to only matches with given outcome (win / loss / draw / overall).")]
        public Outcome Outcome { get; set; }

        public override async ValueTask Process(MatchesContext matchesContext)
        {
            IAsyncEnumerable<(Match Match, int SR)> matches = matchesContext.GetMatchesByOutcomeAsync(Role, Outcome);
            IAsyncEnumerable<int> srs = Change
                ? matches.Where(result => result.SR != 0).Select(result => Math.Abs(result.SR))
                : matches.Select(result => result.Match.SR);

            double average = await srs.DefaultIfEmpty().AverageAsync();

            Console.WriteLine(average == 0d
                ? $"No or not enough historic SR data for outcome '{Outcome}'."
                : $"Average historic SR for outcome '{Outcome}': {average:0}");
        }
    }
}
