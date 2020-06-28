#region

using System.Collections.Generic;

#endregion

namespace OverwatchMatchHistoryTracker.Helpers
{
    public enum Map
    {
        BlizzardWorld,
        Busan,
        Dorado,
        Eichenwalde,
        Hanamura,
        Havana,
        Hollywood,
        HorizonLunarColony,
        Ilios,
        Junkertown,
        KingsRow,
        LijiangTower,
        Nepal,
        Numbani,
        Oasis,
        Paris,
        Rialto,
        Route66,
        TempleOfAnubis,
        VolskayaIndustries,
        WatchpointGibraltar
    }

    public static class MapsHelper
    {
        public static readonly IReadOnlyDictionary<string, Map> Aliases = new Dictionary<string, Map>
        {
            { "bw", Map.BlizzardWorld },
            { "bworld", Map.BlizzardWorld },
            { "hlc", Map.HorizonLunarColony },
            { "horizon", Map.HorizonLunarColony },
            { "krow", Map.KingsRow },
            { "kingsrow", Map.KingsRow },
            { "ltower", Map.LijiangTower },
            { "lt", Map.LijiangTower },
            { "lijiang", Map.LijiangTower },
            { "r66", Map.Route66 },
            { "route", Map.Route66 },
            { "toa", Map.TempleOfAnubis },
            { "temple", Map.TempleOfAnubis },
            { "anubis", Map.TempleOfAnubis },
            { "vi", Map.VolskayaIndustries },
            { "volskaya", Map.VolskayaIndustries },
            { "wg", Map.WatchpointGibraltar },
            { "watchpoint", Map.WatchpointGibraltar },
            { "gibraltar", Map.WatchpointGibraltar },
            { "junker", Map.Junkertown }
        };
    }
}
