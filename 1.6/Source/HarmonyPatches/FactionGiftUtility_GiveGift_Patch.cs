using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SimpleLeadership
{
    [HarmonyPatch(typeof(FactionGiftUtility), "GiveGift", new[] { typeof(List<Tradeable>), typeof(Faction), typeof(GlobalTargetInfo) })]
    public static class FactionGiftUtility_GiveGift_Patch
    {
        public static void Prefix(List<Tradeable> tradeables, Faction giveTo, GlobalTargetInfo lookTarget, out (int goodwill, bool hasFood) __state)
        {
            bool isInFamine = lookTarget.WorldObject is Settlement s && s.IsInPowerEvent(PowerEventDefOf.SL_Famine);
            bool hasFood = isInFamine && tradeables.Any(t => t.ActionToDo == TradeAction.PlayerSells && t.ThingDef.IsNutritionGivingIngestible);
            __state = (giveTo.PlayerGoodwill, hasFood);
        }

        public static void Postfix(Faction giveTo, (int goodwill, bool hasFood) __state)
        {
            if (!__state.hasFood) return;
            int gained = giveTo.PlayerGoodwill - __state.goodwill;
            if (gained <= 0) return;
            int bonus = Mathf.RoundToInt(gained * 0.4f);
            Faction.OfPlayer.TryAffectGoodwillWith(giveTo, bonus, canSendMessage: true, canSendHostilityLetter: true, reason: HistoryEventDefOf.GaveGift);
        }
    }

    [HarmonyPatch(typeof(FactionGiftUtility), "GiveGift", new[] { typeof(List<ActiveTransporterInfo>), typeof(Settlement) })]
    public static class FactionGiftUtility_GiveGift_Pods_Patch
    {
        public static void Prefix(List<ActiveTransporterInfo> pods, Settlement giveTo, out (int goodwill, bool hasFood) __state)
        {
            bool hasFood = giveTo.IsInPowerEvent(PowerEventDefOf.SL_Famine) &&
                pods.Any(pod => pod.innerContainer.Any(t => t.def.IsNutritionGivingIngestible));
            __state = (giveTo.Faction.PlayerGoodwill, hasFood);
        }

        public static void Postfix(Settlement giveTo, (int goodwill, bool hasFood) __state)
        {
            if (!__state.hasFood) return;
            int gained = giveTo.Faction.PlayerGoodwill - __state.goodwill;
            if (gained <= 0) return;
            int bonus = Mathf.RoundToInt(gained * 0.4f);
            Faction.OfPlayer.TryAffectGoodwillWith(giveTo.Faction, bonus, canSendMessage: true, canSendHostilityLetter: true, reason: HistoryEventDefOf.GaveGift);
        }
    }
}
