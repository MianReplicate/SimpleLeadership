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
            var factionBeds = map.listerBuildings.allBuildingsNonColonist
                .OfType<Building_Bed>()
                .Where(b => b.Faction == faction)
                .ToList();

            if (!factionBeds.Any())
                return;

            var bedsByRoom = factionBeds
                .GroupBy(b => b.Position.GetRoom(map))
                .Where(g => g.Key != null)
                .OrderByDescending(g => g.Count())
                .ToList();

            List<Building_Bed> selectedBeds = null;

            foreach (var roomGroup in bedsByRoom)
            {
                int bedCount = roomGroup.Count();
                if (bedCount >= 3 && bedCount <= 5)
                {
                    selectedBeds = roomGroup.ToList();
                    break;
                }
            }

            if (selectedBeds == null)
            {
                var allBeds = factionBeds.ToList();
                int takeCount = System.Math.Min(allBeds.Count, Rand.RangeInclusive(3, 5));
                selectedBeds = allBeds.Take(takeCount).ToList();
            }

            foreach (var bed in selectedBeds)
            {
                bed.ForPrisoners = true;

                PawnKindDef slaveKind = PawnKindDefOf.Slave;
                PawnGenerationRequest request = new PawnGenerationRequest(slaveKind, faction, PawnGenerationContext.NonPlayer, -1, true);
                Pawn prisoner = PawnGenerator.GeneratePawn(request);

                prisoner.SetFaction(Faction.OfAncients);

                GenSpawn.Spawn(prisoner, bed.Position, map);

                if (prisoner.guest != null)
                {
                    prisoner.guest.SetGuestStatus(faction, GuestStatus.Prisoner);
                }
            }

            Messages.Message("SL_PrisonersDetected".Translate(), MessageTypeDefOf.NeutralEvent);
        }
    }
}
