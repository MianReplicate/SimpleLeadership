using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using UnityEngine;

namespace SimpleLeadership
{
    [StaticConstructorOnStartup]
    public static class Utils
    {
        static Utils()
        {
            foreach (var def in DefDatabase<WorldObjectDef>.AllDefs)
            {
                if (typeof(Site).IsAssignableFrom(def.worldObjectClass))
                {
                    def.comps ??= new List<WorldObjectCompProperties>();
                    def.comps.Add(new WorldObjectCompProperties_SiteOwnership());
                }
                else if (typeof(Settlement).IsAssignableFrom(def.worldObjectClass))
                {
                    if (def.inspectorTabs == null)
                        def.inspectorTabs = new List<Type>();
                    def.inspectorTabs.Add(typeof(WITab_FactionLeadership));

                    if (def.inspectorTabsResolved == null)
                        def.inspectorTabsResolved = new List<InspectTabBase>();
                    def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(WITab_FactionLeadership)));
                }
            }
        }

        public static float SafeApproxDistanceInTiles(PlanetTile a, PlanetTile b)
        {
            if (a.Layer == b.Layer)
                return Find.WorldGrid.ApproxDistanceInTiles(a, b);
            return int.MaxValue;
        }

        public static bool IsInPowerEvent<T>(this object obj) where T : PowerEventBase
        {
            return WorldComponent_LeaderTracker.Instance.IsInPowerEvent<T>(obj);
        }

        public static bool IsInPowerEvent(this object obj, PowerEventDef def)
        {
            if (obj == null || def == null)
                return false;
            return WorldComponent_LeaderTracker.Instance.GetActiveEventsFor(obj).Any(ev => ev.def == def);
        }

        public static IEnumerable<T> GetActiveEvents<T>(this object obj) where T : PowerEventBase
        {
            return WorldComponent_LeaderTracker.Instance.GetActiveEventsFor(obj).OfType<T>();
        }

        public static bool IsValidLeaderCandidate(this Pawn pawn)
        {
            if (pawn == null || pawn.Dead)
                return false;
            if (pawn.IsPrisonerOfColony)
                return false;
            if (pawn.MapHeld != null && pawn.MapHeld.IsPlayerHome)
                return false;
            return true;
        }

        public static float CalculateSpawnChance(Pawn leader, Faction faction, Settlement settlement, bool isRaidingPlayer = false)
        {
            if (leader == null || leader.Dead || leader.Spawned)
                return 0f;

            float spawnChance;
            if (isRaidingPlayer)
            {
                float baseChance = SimpleLeadershipMod.Settings.leaderSpawnChance;
                spawnChance = leader == faction.leader ? baseChance * 0.2f : baseChance;
            }
            else
            {
                var leaderTracker = WorldComponent_LeaderTracker.Instance;
                if (leader == faction.leader)
                {
                    int settlementCount = Find.WorldObjects.Settlements.Count(s => s.Faction == faction);
                    spawnChance = settlementCount > 0 ? 1f / settlementCount : 0f;
                }
                else
                {
                    int controlledBases = Find.WorldObjects.Settlements.Count(s => s.Faction == faction && leaderTracker.GetBaseLeader(s) == leader);
                    spawnChance = controlledBases > 0 ? 1f / controlledBases : 0f;
                }
            }

            if (settlement != null && settlement.IsInPowerEvent(PowerEventDefOf.SL_Inspection))
            {
                spawnChance += 0.3f;
            }

            return spawnChance;
        }

        public static void CheckForBaseLeaderDefeat(Pawn pawn)
        {
            if (pawn.Faction == null)
                return;

            WorldComponent_LeaderTracker leaderTracker = WorldComponent_LeaderTracker.Instance;
            var leaderSettlements = leaderTracker.GetSettlementsOfBaseLeader(pawn).ToList();

            if (leaderSettlements.Any())
            {
                if (pawn != pawn.Faction.leader)
                {
                    foreach (var settlement in leaderSettlements)
                    {
                        if (SimpleLeadershipMod.Settings.enableEvents)
                            leaderTracker.StartPowerEvent(PowerEventDefOf.SL_PowerStruggle, settlement);
                    }
                }
            }
        }

        public static void HandleLeaderLost(Faction faction, Pawn oldLeader, string labelKey, string bodyKey, string leaderNamedKey)
        {
            string label = labelKey.Translate(faction.Name, faction.LeaderTitle).Resolve().CapitalizeFirst();
            string body = bodyKey.Translate(faction.NameColored, faction.LeaderTitle, oldLeader.Named(leaderNamedKey)).Resolve().CapitalizeFirst();

            var leaderTracker = WorldComponent_LeaderTracker.Instance;
            var candidates = leaderTracker.GetBaseLeadersFor(faction).Where(p => p.IsValidLeaderCandidate() && p != oldLeader).ToList();
            
            if (candidates.Any())
            {
                var actingLeader = candidates.RandomElement();
                var data = leaderTracker.GetLeadershipDataFor(faction);
                if (data != null)
                {
                    data.exLeader = oldLeader;
                    data.actingLeader = actingLeader;
                }
                
                string actingLeaderText = "SL_ActingLeaderChosen".Translate(faction.LeaderTitle.Named("LEADERTITLE"), actingLeader.Named("PAWN")).Resolve();
                body += "\n\n" + actingLeaderText;
            }

            if (!faction.temporary)
            {
                Find.LetterStack.ReceiveLetter(label, body, LetterDefOf.NeutralEvent, oldLeader, faction);
            }
            
            if (SimpleLeadershipMod.Settings.enableEvents)
                leaderTracker.StartPowerEvent(PowerEventDefOf.SL_PowerVoid, faction);
            
            faction.leader = null;
        }

    }
}
