#region

using System;

#endregion

namespace OverwatchMatchHistoryTracker
{
    public class Match
    {
        public int MatchID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Role { get; set; }
        public int SR { get; set; }
        public string Map { get; set; }
        public string? Comment { get; set; }
    }
}
