#region

using System.Collections.Generic;

#endregion

namespace OverwatchMatchHistoryTracker.Helpers
{
    public static class MapsHelper
    {
        public const string BLIZZARD_WORLD = "blizzard world";
        public const string BUSAN = "busan";
        public const string DORADO = "dorado";
        public const string EICHENWALDE = "eichenwalde";
        public const string HANAMURA = "hanamura";
        public const string HAVANA = "havana";
        public const string HOLLYWOOD = "hollywood";
        public const string HORIZON_LUNAR_COLONY = "horizon lunar colony";
        public const string ILIOS = "ilios";
        public const string JUNKERTOWN = "junkertown";
        public const string KINGS_ROW = "king's row";
        public const string LIJIANG_TOWER = "lijiang tower";
        public const string NEPAL = "nepal";
        public const string NUMBANI = "numbani";
        public const string OASIS = "oasis";
        public const string PARIS = "paris";
        public const string RIALTO = "rialto";
        public const string ROUTE66 = "route 66";
        public const string TEMPLE_OF_ANUBIS = "temple of anubis";
        public const string VOLSKAYA_INDUSTRIES = "volskaya industries";
        public const string WATCHPOINT_GIBRALTAR = "watchpoint: gibraltar";

        public static readonly HashSet<string> Valid = new HashSet<string>
        {
            BLIZZARD_WORLD,
            BUSAN,
            DORADO,
            EICHENWALDE,
            HANAMURA,
            HAVANA,
            HOLLYWOOD,
            HORIZON_LUNAR_COLONY,
            ILIOS,
            JUNKERTOWN,
            KINGS_ROW,
            LIJIANG_TOWER,
            NEPAL,
            NUMBANI,
            OASIS,
            PARIS,
            RIALTO,
            ROUTE66,
            TEMPLE_OF_ANUBIS,
            VOLSKAYA_INDUSTRIES,
            WATCHPOINT_GIBRALTAR,
        };

        public static readonly IReadOnlyDictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "bw", BLIZZARD_WORLD },
            { "bworld", BLIZZARD_WORLD },
            { "hlc", HORIZON_LUNAR_COLONY },
            { "horizon", HORIZON_LUNAR_COLONY },
            { "krow", KINGS_ROW },
            { "kingsrow", KINGS_ROW },
            { "ltower", LIJIANG_TOWER },
            { "lt", LIJIANG_TOWER },
            { "lijiang", LIJIANG_TOWER },
            { "r66", ROUTE66 },
            { "route", ROUTE66 },
            { "toa", TEMPLE_OF_ANUBIS },
            { "temple", TEMPLE_OF_ANUBIS },
            { "anubis", TEMPLE_OF_ANUBIS },
            { "vi", VOLSKAYA_INDUSTRIES },
            { "volskaya", VOLSKAYA_INDUSTRIES },
            { "wg", WATCHPOINT_GIBRALTAR },
            { "watchpoint", WATCHPOINT_GIBRALTAR },
            { "gibraltar", WATCHPOINT_GIBRALTAR },
            { "junker", JUNKERTOWN }
        };
    }
}
