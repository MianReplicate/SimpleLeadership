using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(Faction), "Notify_MemberCaptured")]
    public static class Faction_Notify_MemberCaptured_Patch
    {
        public static void Prefix(Faction __instance, Pawn member)
        {
            if (__instance.leader != member)
            {
                return;
            }

            Utils.HandleLeaderLost(__instance, member, "SL_LeaderCapturedLabel", "SL_LeaderCaptured", "LEADER");

            return;
        }
    }
}
