using RimWorld;
using Verse;

namespace SimpleLeadership
{
    public class PowerStruggle : SettlementPowerEvent
    {
        public override void OnResolve()
        {
            if (settlement == null || settlement.Faction == null) { base.OnResolve(); return; }
            Faction faction = settlement.Faction;
            var leaderTracker = WorldComponent_LeaderTracker.Instance;
            var data = leaderTracker.GetLeadershipDataFor(faction);

            if (data != null)
            {
                Pawn newLeader = leaderTracker.GenerateBaseLeader(faction);
                data.settlementLeaders[settlement] = newLeader;
            }
            base.OnResolve();
        }

        public override bool ShouldGiveMessage()
        {
            return true;
        }
    }
}
