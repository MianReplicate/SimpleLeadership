using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(WorldObjectsHolder), nameof(WorldObjectsHolder.Add))]
    public class WorldObjectsHolder_Add_Patch
    {
        public static void Postfix(WorldObject o)
        {
            if (o is Settlement settlement)
            {
                var tracker = WorldComponent_LeaderTracker.Instance;
                if (tracker.initialized)
                {
                    tracker.AssignLeaderToSettlement(settlement);
                }
            }
        }
    }
}
