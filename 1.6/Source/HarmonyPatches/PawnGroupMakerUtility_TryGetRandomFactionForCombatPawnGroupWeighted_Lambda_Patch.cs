using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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
            Log.Error("SimpleLeadership: Could not find compiler-generated lambda in PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroupWeighted");
            return null;
        }

        public static void Postfix(Faction f, ref float __result)
        {
            int? targetTile = IncidentWorker_RaidEnemy_TryResolveRaidFaction_Patch.RaidContextTargetTile;
            float distanceWeight = SimpleLeadershipMod.Settings.distanceWeight;

            if (targetTile == null || distanceWeight <= 0f)
                return;

            var factionSettlements = Find.WorldObjects.Settlements
                .Where(s => s.Faction == f && s.Spawned)
                .ToList();

            if (factionSettlements.Count == 0)
                return;

            float minDist = factionSettlements.Min(s => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, targetTile.Value));
            float distWeight = Mathf.InverseLerp(100f, 5f, minDist);
            float blended = Mathf.Lerp(1f, distWeight, distanceWeight);

            __result *= Mathf.Max(blended, 0.01f);
        }
    }
}
