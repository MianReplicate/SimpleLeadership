using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(Lord), nameof(Lord.GotoToil))]
    public static class Lord_GotoToil_Patch
    {
        public static void Prefix(Lord __instance, LordToil newLordToil)
        {
            if (__instance.LordJob is not LordJob_DefendBase) return;
            if (newLordToil is not LordToil_AssaultColony) return;
            if (__instance.Map?.Parent is not Settlement settlement) return;
            if (!settlement.IsInPowerEvent(PowerEventDefOf.SL_Reinforcements)) return;

            var comp = __instance.Map.GetComponent<MapComponent_DelayedRaid>();
            if (comp.IsScheduled) return;

            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, __instance.Map);
            parms.faction = settlement.Faction;
            parms.points = StorytellerUtility.DefaultThreatPointsNow(__instance.Map) * 0.75f;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;

            comp.Schedule(parms, 5000);

            Find.LetterStack.ReceiveLetter(
                "SL_ReinforcementsCalledLabel".Translate(),
                "SL_ReinforcementsCalledBody".Translate(settlement.Label, 5000.ToStringTicksToPeriod()),
                LetterDefOf.ThreatBig,
                settlement
            );
        }
    }
}
