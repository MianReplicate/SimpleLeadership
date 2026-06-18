using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(PawnGroupMakerUtility), "GeneratePawns")]
    public static class PawnGroupMakerUtility_GeneratePawns_Patch
    {
        public static void Postfix(ref IEnumerable<Pawn> __result, PawnGroupMakerParms parms)
        {
            if (parms == null || parms.faction == null || parms.faction.def.humanlikeFaction is false)
                return;
            try
            {
                Settlement settlement = RaidContext.CurrentOrigin;
                if (settlement == null || settlement.Faction != parms.faction)
                {
                    var factionSettlements = Find.WorldObjects.Settlements.Where(s => s.Faction == parms.faction && s.Tile.Valid);
                    if (factionSettlements.Any())
                    {
                        settlement = factionSettlements.MinBy(s => Utils.SafeApproxDistanceInTiles(parms.tile, s.Tile));
                    }
                }
                if (settlement == null) return;

                bool isRaidingPlayer = false;
                MapParent mapParent = Find.WorldObjects.MapParentAt(parms.tile);
                if (mapParent != null && mapParent.HasMap && mapParent.Map.ParentFaction == Faction.OfPlayer)
                {
                    isRaidingPlayer = true;
                }

                if (isRaidingPlayer)
                {
                    var tracker = WorldComponent_LeaderTracker.Instance;
                    if (tracker.lastLeaderRaidTick.TryGetValue(parms.faction, out int lastTick) && Find.TickManager.TicksGame < lastTick + 30000)
                    {
                        return;
                    }
                    tracker.lastLeaderRaidTick[parms.faction] = Find.TickManager.TicksGame;
                }

                List<Pawn> pawnList = __result.ToList();

                Pawn factionLeader = parms.faction.leader;
                if (factionLeader != null && !factionLeader.Dead && !factionLeader.Spawned && !__result.Contains(factionLeader))
                {
                    float spawnChance = Utils.CalculateSpawnChance(factionLeader, parms.faction, settlement, isRaidingPlayer);
                    if (Rand.Chance(spawnChance))
                    {
                        pawnList.Add(factionLeader);
                        __result = pawnList;
                    }
                }

                Pawn baseLeader = WorldComponent_LeaderTracker.Instance.GetBaseLeader(settlement);
                if (baseLeader != null && !baseLeader.Dead && !baseLeader.Spawned && !pawnList.Contains(baseLeader))
                {
                    float spawnChance = Utils.CalculateSpawnChance(baseLeader, parms.faction, settlement, isRaidingPlayer);
                    if (Rand.Chance(spawnChance))
                    {
                        pawnList.Add(baseLeader);
                        __result = pawnList;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[SimpleLeadership] Error in PawnGroupMakerUtility_GeneratePawns_Patch: " + ex.Message);
            }
        }
    }
}
