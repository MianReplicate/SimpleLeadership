using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(Faction), "Notify_LeaderDied")]
    public static class Faction_Notify_LeaderDied_Patch
    {
        public static bool Prefix(Faction __instance)
        {
            Pawn oldLeader = __instance.leader;
            if (oldLeader == null)
            {
                return true;
            }

            Utils.HandleLeaderLost(__instance, oldLeader, "LetterLeadersDeathLabel", "LetterLeadersDeath", "OLDLEADER");

            QuestUtility.SendQuestTargetSignals(oldLeader.questTags, "NoLongerFactionLeader", oldLeader.Named("SUBJECT"));

            return false;
        }
    }
}
