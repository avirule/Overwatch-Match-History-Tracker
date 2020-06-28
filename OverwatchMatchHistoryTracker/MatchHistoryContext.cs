#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class MatchHistoryContext : DbContext
    {
        public static async ValueTask<MatchHistoryContext> GetMatchHistoryContext(string name)
        {
            string databasePath = $@"{Environment.CurrentDirectory}/{name}.sqlite";

            if (!File.Exists(databasePath))
            {
                Console.Write($"No match history database exists for '{name}'. Would you like to create one (y / n)? ");
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

            MatchHistoryContext matchHistoryContext = new MatchHistoryContext(name);
            await matchHistoryContext.Database.EnsureCreatedAsync();
            return matchHistoryContext;
        }

        public string Name { get; }
        public DbSet<Match> Matches { get; set; } = null!; // property is initialized by DbContext

        public MatchHistoryContext(string name) => Name = name.ToLowerInvariant();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite($"Data Source=\"{Environment.CurrentDirectory}/{Name}.sqlite\"");

        public IAsyncEnumerable<Match> GetOrderedMatches() => Matches.ToAsyncEnumerable().OrderBy(match => match.Timestamp);
    }
}
