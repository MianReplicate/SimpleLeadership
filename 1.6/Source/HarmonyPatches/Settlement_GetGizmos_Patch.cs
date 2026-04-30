using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(Settlement), nameof(Settlement.GetGizmos))]
    public static class Settlement_GetGizmos_Patch
    {
        public static void Postfix(Settlement __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!DebugSettings.ShowDevGizmos) return;

            var gizmos = new List<Gizmo>(__result);
            var tracker = WorldComponent_LeaderTracker.Instance;

            gizmos.Add(new Command_Action
            {
                defaultLabel = "DEV: Generate New Base Leader",
                action = () =>
                {
                    var data = tracker.GetLeadershipDataFor(__instance.Faction);
                    if (data == null) return;
                    var newLeader = tracker.GenerateBaseLeader(__instance.Faction);
                    if (newLeader != null)
                        data.settlementLeaders[__instance] = newLeader;
                }
            });

            foreach (var def in DefDatabase<PowerEventDef>.AllDefs)
            {
                object target = typeof(SettlementPowerEvent).IsAssignableFrom(def.workerClass)
                    ? __instance
                    : __instance.Faction;

                var activeEvent = target.GetActiveEvents<PowerEventBase>().FirstOrDefault(e => e.def == def);

                if (activeEvent != null)
                {
                    gizmos.Add(new Command_Action
                    {
                        defaultLabel = "DEV: End " + def.label,
                        action = () => WorldComponent_LeaderTracker.Instance.EndPowerEvent(activeEvent)
                    });
                }
                else
                {
                    gizmos.Add(new Command_Action
                    {
                        defaultLabel = "DEV: Start " + def.label,
                        action = () => WorldComponent_LeaderTracker.Instance.StartPowerEvent(def, target)
                    });
                }
            }

            __result = gizmos;
        }
    }
}
