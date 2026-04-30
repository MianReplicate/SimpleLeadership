using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryResolveRaidFaction")]
    public static class IncidentWorker_RaidEnemy_TryResolveRaidFaction_Patch
    {
        public static PlanetTile RaidContextTargetTile;
        public static void Prefix(IncidentParms parms)
        {
            if (SimpleLeadershipMod.Settings.distanceWeight <= 0f)
                return;
            var targetTile = PlanetTile.Invalid;
            if (parms.target is Map map) targetTile = map.Tile;
            else if (parms.target is WorldObject worldObject) targetTile = worldObject.Tile;
            RaidContextTargetTile = targetTile != PlanetTile.Invalid ? targetTile : PlanetTile.Invalid;
        }

        public static void Postfix(IncidentParms parms)
        {
            var previousTargetTile = RaidContextTargetTile;
            RaidContextTargetTile = PlanetTile.Invalid;

            if (parms.faction == null || parms.target == null)
            {
                return;
            }
            Settlement originSettlement = FindMostLikelyOriginSettlement(parms.faction, parms.target);
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

        private static Settlement FindMostLikelyOriginSettlement(Faction faction, IIncidentTarget target)
        {
            if (faction == null)
                return null;

            var factionSettlements = Find.WorldObjects.Settlements
                .Where(s => s.Faction == faction && s.Spawned && s.Tile.Valid)
                .ToList();

            if (factionSettlements.Count == 0)
                return null;

            float weight = SimpleLeadershipMod.Settings.distanceWeight;

            var targetTile = PlanetTile.Invalid;
            if (target is Map map) targetTile = map.Tile;
            else if (target is WorldObject worldObject) targetTile = worldObject.Tile;

            if (targetTile == PlanetTile.Invalid || weight <= 0f)
                return factionSettlements.RandomElement();

            var sorted = factionSettlements
                .OrderBy(s => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, targetTile))
                .ToList();

            float decay = 1f - weight;
            var ranked = sorted.Select((s, i) => (settlement: s, w: Mathf.Pow(decay, i))).ToList();
            return ranked.RandomElementByWeight(x => x.w).settlement;
        }
    }
}
