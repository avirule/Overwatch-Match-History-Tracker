#region

using System;
using OverwatchMatchHistoryTracker.Helpers;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class Match
    {
        public int MatchID { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Entropic { get; set; }
        public string Role { get; set; }
        public int SR { get; set; }
        public Map Map { get; set; }
        public string? Comment { get; set; }

        public Match()
        {
            Entropic = true;
            Role = string.Empty;
        }
    }
}
