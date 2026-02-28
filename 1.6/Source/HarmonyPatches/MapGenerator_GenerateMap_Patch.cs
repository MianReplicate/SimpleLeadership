using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    public static class MapGenerator_GenerateMap_Patch
    {
        public static void Postfix(Map __result)
        {
            if (__result == null || __result.Parent is not Settlement settlement)
                return;

            if (settlement.IsInPowerEvent(PowerEventDefOf.SL_Support) && settlement.Faction.HostileTo(Faction.OfPlayer))
            {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, __result);
                parms.faction = settlement.Faction;
                parms.points = StorytellerUtility.DefaultThreatPointsNow(__result) * 0.5f;
                parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;

                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + 200, parms);

                Messages.Message("SL_SupportArriving".Translate(settlement.Label), MessageTypeDefOf.ThreatBig);
            }

            if (settlement.IsInPowerEvent(PowerEventDefOf.SL_PrisonerTransfer))
            {
                SpawnPrisoners(__result, settlement.Faction);
            }
        }

        private static void SpawnPrisoners(Map map, Faction faction)
        {
            IntVec3 spawnCenter = IntVec3.Invalid;

            var bed = map.listerBuildings.allBuildingsNonColonist
                .OfType<Building_Bed>()
                .FirstOrDefault(b => b.Faction == faction);

            if (bed != null)
            {
                spawnCenter = bed.Position;
            }
            else
            {
                CellFinder.TryFindRandomCellNear(map.Center, map, 10, (c) => c.GetRoom(map) != null && !c.GetRoom(map).PsychologicallyOutdoors, out spawnCenter);
            }

            if (spawnCenter.IsValid)
            {
                int count = Rand.Range(3, 6);
                for (int i = 0; i < count; i++)
                {
                    PawnKindDef slaveKind = PawnKindDefOf.Slave;
                    PawnGenerationRequest request = new PawnGenerationRequest(slaveKind, faction, PawnGenerationContext.NonPlayer, -1, true);
                    Pawn prisoner = PawnGenerator.GeneratePawn(request);

                    prisoner.SetFaction(Faction.OfAncients);

                    GenSpawn.Spawn(prisoner, spawnCenter, map);

                    if (prisoner.guest != null)
                    {
                        prisoner.guest.SetGuestStatus(faction, GuestStatus.Prisoner);
                    }
                }
                Messages.Message("SL_PrisonersDetected".Translate(), MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}
