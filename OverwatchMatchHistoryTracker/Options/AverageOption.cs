#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using OverwatchMatchHistoryTracker.Helpers;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("average", HelpText = "Get average historic SR.")]
    public class AverageOption
    {
        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Get average historic SR", new AverageOption
            {
                Name = "ShadowDragon",
                Role = "DPS"
            }),
        };

        private string _Name;
        private string _Role;
        private string _Outcome;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Option('c', "change", Required = false, HelpText = "Returns average SR change instead.")]
        public bool Change { get; set; }

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player to collate data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for which to collate data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        [Value(2, MetaName = nameof(Outcome), Required = false,
            HelpText = "Constrains the collation to only matches with given outcome (win / loss / draw).", Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set => _Outcome = value.ToLowerInvariant();
        }

        public AverageOption() => _Role = _Outcome = _Name = string.Empty;

        public static async ValueTask Process(AverageOption averageOption)
        {
            if (!RolesHelper.Valid.Contains(averageOption.Role))
            {
                throw new InvalidOperationException
                (
                    $"Invalid role provided: '{averageOption.Role}' (valid roles are {string.Join(", ", RolesHelper.Valid.Select(role => $"'{role}'"))})."
                );
            }

            double average = averageOption.Change
                ? await GetMatchSRChanges(averageOption.Name, averageOption.Role, averageOption.Outcome).DefaultIfEmpty().AverageAsync()
                : await GetMatchSRs(averageOption.Name, averageOption.Role, averageOption.Outcome).DefaultIfEmpty().AverageAsync();

            Console.WriteLine(average == 0d
                ? $"No or not enough historic SR data for outcome '{averageOption.Outcome}'."
                : $"Average historic SR for outcome '{averageOption.Outcome}': {average:0}");
        }

        private static async IAsyncEnumerable<int> GetMatchSRs(string name, string role, string outcome)
        {
            Stack<int> orderedSRs = new Stack<int>(await MatchHistoryProvider.GetOrderedSRs(name, role).ToListAsync());

            while (orderedSRs.Count > 1)
            {
                int sr = orderedSRs.Pop();
                int srChange = sr - orderedSRs.Peek();

                switch (outcome)
                {
                    case "win" when srChange > 0:
                    case "loss" when srChange < 0:
                    case "draw" when srChange == 0:
                        yield return sr;
                        break;
                }
            }
        }

        private static async IAsyncEnumerable<int> GetMatchSRChanges(string name, string role, string outcome)
        {
            Stack<int> orderedSRs = new Stack<int>(await MatchHistoryProvider.GetOrderedSRs(name, role).ToListAsync());

            while (orderedSRs.Count > 0)
            {
                int sr = orderedSRs.Pop();

                if (!orderedSRs.TryPeek(out int peek))
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
