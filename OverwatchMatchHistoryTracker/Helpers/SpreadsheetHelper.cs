#region

using System;
using System.Linq;

#endregion

namespace OverwatchMatchHistoryTracker.Helpers
{
    public class SpreadsheetHelper
    {
        public static int SpreadsheetCharToInt(char a)
        {
            if ((a < 65) || (a > 90))
            {
                throw new ArgumentOutOfRangeException(nameof(a), "Argument must be A-Z.");
            }
            else
            {
                return a - 65;
            }
        }

        public static int SpreadsheetColumnIndex(string column) => column.Select(SpreadsheetCharToInt).Sum();
    }
}
