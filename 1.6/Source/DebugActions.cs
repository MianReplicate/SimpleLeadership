using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.Planet;
using LudeonTK;

namespace SimpleLeadership
{
    public static class DebugActions
    {
        [DebugAction("SimpleLeadership", null, false, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnBaseLeader()
        {
            var tracker = WorldComponent_LeaderTracker.Instance;
            var currentMap = Find.CurrentMap;
            var playerTile = currentMap != null ? currentMap.Tile : PlanetTile.Invalid;

            var allEntries = new List<(Settlement settlement, Pawn leader, Faction faction)>();
            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                var data = tracker.GetLeadershipDataFor(faction);
                if (data != null && data.settlementLeaders.Any())
                {
                    foreach (var kvp in data.settlementLeaders)
                    {
                        if (kvp.Value != null)
                        {
                            allEntries.Add((kvp.Key, kvp.Value, faction));
                        }
                    }
                }
            }

            if (playerTile.Valid)
            {
                allEntries = allEntries.OrderBy(e => e.settlement != null ? Find.WorldGrid.ApproxDistanceInTiles(playerTile, e.settlement.Tile) : float.MaxValue).ToList();
            }

            List<FloatMenuOption> list = [];
            foreach (var entry in allEntries)
            {
                Settlement localSettlement = entry.settlement;
                Pawn localLeader = entry.leader;
                Faction localFac = entry.faction;
                string settlementName = localSettlement != null ? localSettlement.Label : "Unknown";
                list.Add(new FloatMenuOption(localFac.Name + " - " + localLeader.Name.ToStringFull + " (" + settlementName + ")", delegate
                {
                    GenSpawn.Spawn(localLeader, UI.MouseCell(), Find.CurrentMap);
                }));
            }

            if (list.Any())
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
            else
            {
                Messages.Message("No base leaders found.", MessageTypeDefOf.RejectInput);
            }
        }

        [DebugAction("SimpleLeadership", null, false, false, false, false, false, 0, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.WorldRenderedNow)]
        public static void ShowBaseLeaders()
        {
            var tracker = WorldComponent_LeaderTracker.Instance;
            var allLeaders = new List<(Pawn leader, Faction faction, int baseCount, float avgDistance)>();
            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                var data = tracker.GetLeadershipDataFor(faction);
                if (data != null && data.settlementLeaders.Any())
                {
                    var leaders = data.settlementLeaders
                        .Where(kvp => kvp.Value != null && !kvp.Value.Dead)
                        .Select(kvp => kvp.Value)
                        .Distinct()
                        .ToList();

                    foreach (var leader in leaders)
                    {
                        var leaderSettlements = data.settlementLeaders
                            .Where(kvp => kvp.Value == leader && kvp.Key.Tile.Valid)
                            .Select(kvp => kvp.Key)
                            .ToList();

                        int baseCount = leaderSettlements.Count;
                        float avgDistance = 0f;

                        if (baseCount > 1)
                        {
                            float totalDistance = 0f;
                            int distanceCount = 0;
                            for (int i = 0; i < leaderSettlements.Count; i++)
                            {
                                for (int j = i + 1; j < leaderSettlements.Count; j++)
                                {
                                    totalDistance += Find.WorldGrid.ApproxDistanceInTiles(leaderSettlements[i].Tile, leaderSettlements[j].Tile);
                                    distanceCount++;
                                }
                            }
                            avgDistance = totalDistance / distanceCount;
                        }

                        allLeaders.Add((leader, faction, baseCount, avgDistance));
                    }
                }
            }

            if (allLeaders.Any())
            {
                string message = "Base Leaders:\n";
                foreach (var entry in allLeaders.OrderBy(e => e.faction.Name))
                {
                    message += $"{entry.faction.Name}: {entry.leader.Name} - {entry.baseCount} bases, avg dist: {entry.avgDistance:F1}\n";
                }
                Log.Message(message);
            }
            else
            {
                Log.Message("No base leaders found.");
            }
        }
    }
}
