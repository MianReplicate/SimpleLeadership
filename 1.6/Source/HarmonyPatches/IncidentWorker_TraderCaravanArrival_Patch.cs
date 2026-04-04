using HarmonyLib;
using RimWorld;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "TryExecuteWorker")]
    public static class IncidentWorker_TraderCaravanArrival_Patch
    {
        public static bool Prefix(IncidentParms parms)
        {
            if (parms.faction != null && parms.faction.IsInPowerEvent(PowerEventDefOf.SL_Sanctioned))
            {
                return false;
            }
            return true;
        }
    }
}
