using System.Collections.Generic;
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
        public static void Postfix(IncidentParms parms)
        {
            if (parms.faction == null || parms.target == null)
                return;
            Settlement originSettlement = FindMostLikelyOriginSettlement(parms.faction, parms.target);
            RaidContext.CurrentOrigin = originSettlement;

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

            List<Settlement> factionSettlements = Find.WorldObjects.Settlements
                .Where(s => s.Faction == faction && s.Spawned && s.Tile.Valid)
                .ToList();

            if (factionSettlements.Count == 0)
                return null;

            var targetTile = PlanetTile.Invalid;
            if (target is Map map) targetTile = map.Tile;
            else if (target is WorldObject worldObject) targetTile = worldObject.Tile;

            if (targetTile != PlanetTile.Invalid)
            {
                return factionSettlements.RandomElementByWeight(s =>
                {
                    var distance = Find.WorldGrid.ApproxDistanceInTiles(s.Tile, targetTile);
                    return 1f / (distance + 1);
                });
            }
            return factionSettlements.FirstOrDefault();
        }
    }
}
