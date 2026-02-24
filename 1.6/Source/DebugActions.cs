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
        private static void SpawnBaseLeader()
        {
            var tracker = WorldComponent_LeaderTracker.Instance;
            if (tracker == null)
            {
                Log.Error("WorldComponent_LeaderTracker not found");
                return;
            }

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
    }
}
