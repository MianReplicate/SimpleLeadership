using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "FactionCanBeGroupSource")]
    public static class IncidentWorker_RaidEnemy_FactionCanBeGroupSource_Patch
    {
        public static void Postfix(ref bool __result, Faction f, IncidentParms parms)
        {
            if (!__result || f == null) return;

            if (f.IsInPowerEvent<PowerVoid>())
            {
                __result = false;
                return;
            }

            var playerBases = Find.WorldObjects.Settlements
                .Where(s => s.Faction == Faction.OfPlayer).ToList();
            if (!playerBases.Any()) return;

            Settlement nearestFactionBase = Find.WorldObjects.Settlements
                .Where(s => s.Faction == f && s.Spawned && s.Tile.Valid)
                .OrderBy(s => playerBases.Min(p => Find.WorldGrid.ApproxDistanceInTiles(p.Tile, s.Tile)))
                .FirstOrDefault();

            if (nearestFactionBase == null) return;

            float nearestDistance = playerBases.Min(p => Find.WorldGrid.ApproxDistanceInTiles(p.Tile, nearestFactionBase.Tile));
            if (nearestDistance > 30f)
            {
                float suppressChance = Mathf.Clamp01((nearestDistance - 30f) / 90f) * 0.8f;
                if (Rand.Chance(suppressChance))
                    __result = false;
            }
        }
    }
}
