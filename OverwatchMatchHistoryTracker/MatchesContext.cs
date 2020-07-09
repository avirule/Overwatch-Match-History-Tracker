#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OverwatchMatchHistoryTracker.MatchOption;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class MatchesContext : DbContext
    {
        public static async ValueTask<MatchesContext> GetMatchesContext(string name)
        {
            string databasePath = $@"{Environment.CurrentDirectory}/{name}.sqlite";

            if (!File.Exists(databasePath))
            {
                Console.Write($"No match history database exists for '{name}'. Would you like to create one (y / n)? ");
                PromptDatabaseCreation();
            }

            MatchesContext matchesContext = new MatchesContext(name);
            await matchesContext.Database.EnsureCreatedAsync();
            return matchesContext;
        }

        private static void PromptDatabaseCreation()
        {
            ConsoleKey key = Console.ReadKey().Key;
            Console.Write("\r\n");

            switch (key)
            {
                case ConsoleKey.Y:
                    break;
                default:
                    throw new InvalidOperationException("Match history database not found.");
            }
        }

        public string Name { get; }
        public DbSet<Match> Matches { get; set; } = null!; // property is initialized by DbContext

        public MatchesContext(string name) => Name = name.ToLowerInvariant();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite($"Data Source=\"{Environment.CurrentDirectory}/{Name}.sqlite\"");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            EntityTypeBuilder<Match> matchEntity = modelBuilder.Entity<Match>();

            // constraints
            matchEntity.HasCheckConstraint(nameof(Match.SR), $"({nameof(Match.SR)} >= 0 AND {nameof(Match.SR)} <= 5000)");
            matchEntity.HasCheckConstraint(nameof(Match.Map),
                $@"({string.Join(" OR ", Enum.GetNames(typeof(Map)).Select(map => $"{nameof(Match.Map)} = \"{map}\""))})");

            // conversions
            matchEntity.Property(match => match.Map).HasConversion(map => map.ToString(), map => Enum.Parse<Map>(map, true));
            matchEntity.Property(match => match.Role).HasConversion(role => role.ToString(), role => Enum.Parse<Role>(role, true));
        }

        public IAsyncEnumerable<Match> GetMatchesAsync() => Matches.ToAsyncEnumerable().OrderBy(match => match.Timestamp);
        public IAsyncEnumerable<Match> GetMatchesByRoleAsync(Role role) => GetMatchesAsync().Where(match => match.Role == role);

        public IAsyncEnumerable<(Match, int)> GetMatchesByOutcomeAsync(Role role, Outcome outcome) =>
            GetMatchesByOutcomeAsync(GetMatchesByRoleAsync(role), outcome);

        public async IAsyncEnumerable<(Match, int)> GetMatchesByOutcomeAsync(IAsyncEnumerable<Match> matches, Outcome outcome)
        {
            int lastSR = -1;
            await foreach (Match match in matches)
            {
                if (!match.Entropic)
                {
                    lastSR = -1;
                }

                int entropicSR = lastSR == -1 ? match.SR : lastSR;
                int srChange = match.SR - entropicSR;

                switch (outcome)
                {
                    case Outcome.Win when (srChange > 0) && (lastSR > -1):
                    case Outcome.Loss when (srChange < 0) && (lastSR > -1):
                    case Outcome.Draw when (srChange == 0) && (lastSR > -1):
                    case Outcome.Overall:
                        yield return (match, srChange);
                        break;
                }

                lastSR = match.SR;
            }
        }
    }
}
