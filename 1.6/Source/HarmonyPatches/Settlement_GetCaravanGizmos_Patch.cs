using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HotSwappable]
    [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetCaravanGizmos))]
    public static class Settlement_GetCaravanGizmos_Patch
    {
        public static bool GettingGizmos = false;
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Settlement __instance)
        {
            bool sanctioned = __instance.Faction != null && __instance.Faction.IsInPowerEvent(PowerEventDefOf.SL_Sanctioned);
            GettingGizmos = true;
            try
            {
                foreach (var gizmo in values)
                {
                    if (sanctioned && gizmo is Command_Action cmd && cmd.icon == CaravanVisitUtility.TradeCommandTex)
                    {
                        cmd.Disable("SL_SanctionedCannotTrade".Translate());
                    }
                    yield return gizmo;
                }
            }
            finally
            {
                GettingGizmos = false;
            }
        }
    }
}
