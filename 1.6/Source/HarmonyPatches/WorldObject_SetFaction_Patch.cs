using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(WorldObject), nameof(WorldObject.SetFaction))]
    public static class WorldObject_SetFaction_Patch
    {
        public static void Prefix(WorldObject __instance, out Faction __state)
        {
            __state = __instance.Faction;
        }

        public static void Postfix(WorldObject __instance, Faction newFaction, Faction __state)
        {
            if (__instance is Settlement settlement && __state != newFaction)
            {
                var tracker = WorldComponent_LeaderTracker.Instance;
                if (tracker.initialized)
                {
                    if (__state != null)
                    {
                        var oldData = tracker.GetLeadershipDataFor(__state);
                        if (oldData != null && oldData.settlementLeaders.ContainsKey(settlement))
                        {
                            oldData.settlementLeaders.Remove(settlement);
                        }
                    }

                    tracker.AssignLeaderToSettlement(settlement);
                }
            }
        }
    }
}
