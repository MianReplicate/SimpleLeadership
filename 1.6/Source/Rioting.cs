using RimWorld;
using Verse;

namespace SimpleLeadership
{
    public class Rioting : SettlementPowerEvent
    {
        public override void OnResolve()
        {
            if (settlement == null || settlement.Faction == null) return;
            Faction faction = settlement.Faction;
            var leaderTracker = WorldComponent_LeaderTracker.Instance;
            var data = leaderTracker.GetLeadershipDataFor(faction);

            if (data != null)
            {
                if (Rand.Chance(0.2f))
                {
                    Pawn newLeader = leaderTracker.GenerateBaseLeader(faction);
                    data.settlementLeaders[settlement] = newLeader;

                    SendMessage("SL_RiotingSuccess".Translate(settlement.Label, newLeader.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    SendMessage("SL_RiotingFailed".Translate(settlement.Label), MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}
