using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(FactionGiftUtility), "GiveGift", new[] { typeof(List<Tradeable>), typeof(Faction), typeof(GlobalTargetInfo) })]
    public static class FactionGiftUtility_GiveGift_Patch
    {
        public static void Prefix(List<Tradeable> tradeables, Faction giveTo, GlobalTargetInfo lookTarget)
        {
            if (giveTo == null || !lookTarget.IsValid || !(lookTarget.WorldObject is Settlement settlement))
                return;

            if (settlement.IsInPowerEvent(PowerEventDefOf.SL_Famine))
            {
                bool hasFood = tradeables.Any(t => t.ThingDef.IsNutritionGivingIngestible);
                if (hasFood)
                {
                    giveTo.TryAffectGoodwillWith(Faction.OfPlayer, 15, canSendMessage: true, canSendHostilityLetter: true, reason: HistoryEventDefOf.GaveGift);
                }
            }
        }
    }

    [HarmonyPatch(typeof(FactionGiftUtility), "GiveGift", new[] { typeof(List<ActiveTransporterInfo>), typeof(Settlement) })]
    public static class FactionGiftUtility_GiveGift_Pods_Patch
    {
        public static void Prefix(List<ActiveTransporterInfo> pods, Settlement giveTo)
        {
            if (giveTo == null || !giveTo.IsInPowerEvent(PowerEventDefOf.SL_Famine))
                return;

            bool hasFood = false;
            for (int i = 0; i < pods.Count; i++)
            {
                ThingOwner innerContainer = pods[i].innerContainer;
                for (int j = 0; j < innerContainer.Count; j++)
                {
                    if (innerContainer[j].def.IsNutritionGivingIngestible)
                    {
                        hasFood = true;
                        break;
                    }
                }
                if (hasFood)
                    break;
            }

            if (hasFood)
            {
                giveTo.Faction.TryAffectGoodwillWith(Faction.OfPlayer, 15, canSendMessage: true, canSendHostilityLetter: true, reason: HistoryEventDefOf.GaveGift);
            }
        }
    }
}
