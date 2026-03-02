using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(Settlement), "get_CanTradeNow")]
    public static class Settlement_CanTradeNow_Patch
    {
        public static void Postfix(Settlement __instance, ref bool __result)
        {
            if (!__result) return;
            if (Settlement_GetCaravanGizmos_Patch.GettingGizmos) return;
            if (__instance.Faction != null && __instance.Faction.IsInPowerEvent(PowerEventDefOf.SL_Sanctioned))
                __result = false;
        }
    }
}
