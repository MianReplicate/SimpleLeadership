using RimWorld;
using Verse;

namespace SimpleLeadership
{
    public class Rioting : SettlementPowerEvent
    {
        public override void OnResolve()
        {
            Faction faction = settlement.Faction;
            var leaderTracker = WorldComponent_LeaderTracker.Instance;
            var data = leaderTracker.GetLeadershipDataFor(faction);

            if (data != null)
            {
                Pawn newLeader = leaderTracker.GenerateBaseLeader(faction);
                data.settlementLeaders[settlement] = newLeader;

                SendMessage("SL_RiotingSuccess".Translate(settlement.Label, newLeader.Named("PAWN")), MessageTypeDefOf.NeutralEvent);
            }
            base.OnResolve();
        }
    }
}
