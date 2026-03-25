using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class PawnGenerator_GeneratePawn_Patch
    {
        public static void Postfix(Pawn __result, PawnGenerationRequest request)
        {
            if (__result != null && !request.ForceDead && request.Context == PawnGenerationContext.NonPlayer)
            {
                if (request.Tile != -1)
                {
                    Settlement settlement = Find.WorldObjects.SettlementAt(request.Tile);
                    if (settlement != null && settlement.IsInPowerEvent(PowerEventDefOf.SL_Famine))
                    {
                        if (__result.Faction == settlement.Faction)
                        {
                            var hediff = HediffMaker.MakeHediff(HediffDefOf.Malnutrition, __result);
                            hediff.Severity = Rand.Range(0.2f, 0.85f);
                            __result.health.AddHediff(hediff);
                        }
                    }
                }
            }
        }
    }
}
