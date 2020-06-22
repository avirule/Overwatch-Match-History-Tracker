#region

using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

#endregion

namespace OverwatchMatchHistoryTracker
{
    [Verb("display", HelpText = "Display tabulated match history data.")]
    public class DisplayOption
    {
        public const string DISPLAY_FORMAT = " {0} | {1} | {2} | {3} ";

        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Displays historic match data", new DisplayOption
            {
                Name = "ShadowDragon",
                Role = "DPS"
            }),
        };

        public static void Display(string timestamp, string sr, string map, string comment)
        {
            Console.WriteLine(DISPLAY_FORMAT, timestamp.PadRight(19), sr.PadRight(4), map.PadRight(25), comment);
        }

        private string _Name;
        private string _Role;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Value(1, Required = true, HelpText = "Name of player to display data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(2, Required = true, HelpText = "Role for which to display data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        public DisplayOption() => _Name = _Role = string.Empty;
    }
}
