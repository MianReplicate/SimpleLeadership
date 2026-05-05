using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
    public static class IncidentWorker_RaidEnemy_TryResolveRaidFaction_Patch
    {
        public static PlanetTile RaidContextTargetTile;
        internal static Settlement ChosenOriginSettlement;
        public static void Prefix(IncidentParms parms)
        {
            ChosenOriginSettlement = null;
            if (SimpleLeadershipMod.Settings.distanceWeight <= 0f)
                return;
            var targetTile = PlanetTile.Invalid;
            if (parms.target is Map map) targetTile = map.Tile;
            else if (parms.target is WorldObject worldObject) targetTile = worldObject.Tile;
            if (targetTile == PlanetTile.Invalid)
                return;
            RaidContextTargetTile = targetTile;
            if (Rand.Value >= SimpleLeadershipMod.Settings.distanceWeight)
                return;
            ChosenOriginSettlement = Find.WorldObjects.Settlements
                .Where(s => s.Faction != null && !s.Faction.IsPlayer && s.Spawned && s.Tile.Valid)
                .MinBy(s => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, targetTile));
        }

        public static void Postfix(IncidentParms parms)
        {
            RaidContextTargetTile = PlanetTile.Invalid;

            if (parms.faction == null || parms.target == null)
            {
                return;
            }
            var originSettlement = ChosenOriginSettlement;
            ChosenOriginSettlement = null;
            RaidContext.CurrentOrigin = originSettlement;

            if (originSettlement != null)
                WorldComponent_LeaderTracker.Instance.lastRaidOrigin[parms.faction] = originSettlement;
            else
                WorldComponent_LeaderTracker.Instance.lastRaidOrigin.Remove(parms.faction);

            if (originSettlement != null)
            {
                if (originSettlement.IsInPowerEvent<PowerStruggle>())
                {
                    parms.points *= 0.6f;
                }
                else if (originSettlement.IsInPowerEvent(PowerEventDefOf.SL_Fortifying))
                {
                    parms.points *= 2f;
                }
            }
        }
    }
}
