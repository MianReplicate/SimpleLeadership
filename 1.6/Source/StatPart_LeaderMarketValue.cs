using System.Linq;
using RimWorld;
using Verse;

namespace SimpleLeadership
{
    [HotSwappable]
    public class StatPart_LeaderMarketValue : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is not Pawn pawn || pawn.Faction == null) return;

            if (pawn == pawn.Faction.leader || (pawn == WorldComponent_LeaderTracker.Instance.GetLeadershipDataFor(pawn.Faction)?.exLeader && pawn.Faction.leader == null))
                val *= 2f;
            else if (WorldComponent_LeaderTracker.Instance.GetSettlementsOfBaseLeader(pawn).Any())
                val *= 1.6f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is not Pawn pawn || pawn.Faction == null) return null;

            if (pawn == pawn.Faction.leader)
                return "SL_FactionLeaderValueBonus".Translate() + ": x" + 2f.ToStringPercent();
            if (WorldComponent_LeaderTracker.Instance.GetSettlementsOfBaseLeader(pawn).Any())
                return "SL_BaseLeaderValueBonus".Translate() + ": x" + 1.6f.ToStringPercent();
            return null;
        }
    }
}
