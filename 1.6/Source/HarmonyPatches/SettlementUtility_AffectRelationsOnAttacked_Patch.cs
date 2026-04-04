using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(SettlementUtility), nameof(SettlementUtility.AffectRelationsOnAttacked))]
    public static class SettlementUtility_AffectRelationsOnAttacked_Patch
    {
        public static void Postfix(MapParent mapParent)
        {
            if (mapParent is not Settlement settlement || !settlement.HasMap)
                return;

            if (!settlement.IsInPowerEvent(PowerEventDefOf.SL_Support))
                return;

            if (!settlement.Faction.HostileTo(Faction.OfPlayer))
                return;

            MapGenerator_GenerateMap_Patch.DoSupportArriving(settlement.Map, settlement);
        }
    }
}
