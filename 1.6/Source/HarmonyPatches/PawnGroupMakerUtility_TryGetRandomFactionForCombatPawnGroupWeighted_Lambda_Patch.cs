using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch]
    public static class PawnGroupMakerUtility_TryGetRandomFactionForCombatPawnGroupWeighted_Lambda_Patch
    {
        public static bool Prepare() => TargetMethod() != null;

        public static MethodBase TargetMethod()
        {
            foreach (var nestedType in typeof(PawnGroupMakerUtility).GetNestedTypes(AccessTools.all))
            {
                foreach (var method in nestedType.GetMethods(AccessTools.all))
                {
                    if (!method.Name.Contains("<TryGetRandomFactionForCombatPawnGroupWeighted>"))
                        continue;
                    if (method.ReturnType != typeof(float))
                        continue;
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1 || parameters[0].ParameterType != typeof(Faction))
                        continue;
                    return method;
                }
            }
            return null;
        }

        public static void Postfix(Faction f, ref float __result)
        {
            var chosen = IncidentWorker_RaidEnemy_TryResolveRaidFaction_Patch.ChosenOriginSettlement;
            if (chosen == null)
                return;
            if (f != chosen.Faction)
                __result = 0f;
        }
    }
}
