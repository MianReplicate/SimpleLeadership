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
                .Where(s => s.Faction == faction && s.Spawned)
                .ToList();

            if (factionSettlements.Count == 0)
                return null;

            float weight = SimpleLeadershipMod.Settings.distanceWeight;
            if (weight <= 0f)
                return factionSettlements.RandomElement();

            int targetTile = -1;
            if (target is Map map) targetTile = map.Tile;
            else if (target is WorldObject worldObject) targetTile = worldObject.Tile;

            if (targetTile != -1)
            {
                return factionSettlements.RandomElementByWeight(s =>
                {
                    float dist = Find.WorldGrid.ApproxDistanceInTiles(s.Tile, targetTile);
                    float distWeight = Mathf.InverseLerp(100f, 5f, dist);
                    float blended = Mathf.Lerp(1f, distWeight, weight);
                    return Mathf.Max(blended, 0.01f);
                });
            }

            return factionSettlements.FirstOrDefault();
        }
    }
}
