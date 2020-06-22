#region

using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

#endregion

namespace OverwatchMatchHistoryTracker.Options
{
    [Verb("display", HelpText = "Display tabulated match history data.")]
    public class DisplayOption
    {
        private const string _DISPLAY_FORMAT = " {0} | {1} | {2} | {3} | {4} ";

        private static readonly List<Example> _Examples = new List<Example>
        {
            new Example("Displays historic match data", new DisplayOption
            {
                Name = "ShadowDragon",
                Role = "DPS",
                Outcome = "win"
            }),
        };

        public static void Display(string timestamp, string sr, string change, string map, string comment)
        {
            Console.WriteLine(_DISPLAY_FORMAT,
                timestamp.PadLeft(10 + (timestamp.Length / 2)).PadRight(19),
                sr.PadLeft(2).PadRight(4),
                change.PadLeft(4).PadRight(6),
                map.PadLeft(13 + (map.Length / 2)).PadRight(25),
                comment);
        }

        private string _Name;
        private string _Role;
        private string _Outcome;

        [Usage]
        public static IEnumerable<Example> Examples => _Examples;

        [Value(0, MetaName = nameof(Name), Required = true, HelpText = "Name of player to display data from.")]
        public string Name
        {
            get => _Name;
            set => _Name = value.ToLowerInvariant();
        }

        [Value(1, MetaName = nameof(Role), Required = true, HelpText = "Role for which to display data from.")]
        public string Role
        {
            get => _Role;
            set => _Role = value.ToLowerInvariant();
        }

        [Value(2, MetaName = nameof(Outcome), Required = false, HelpText = "Only display matches of given outcome (win / loss/ draw).",
            Default = "overall")]
        public string Outcome
        {
            get => _Outcome;
            set => _Outcome = value.ToLowerInvariant();
        }

        public DisplayOption() => _Name = _Role = _Outcome = string.Empty;
    }
}
