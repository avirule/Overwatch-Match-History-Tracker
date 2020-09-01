#region

using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    public abstract class CommandOption
    {
        [NotMapped]
        public string? ProcessingFinishedMessage { get; protected set; }

        public abstract ValueTask Process(MatchesContext matchesContext);
    }
}
