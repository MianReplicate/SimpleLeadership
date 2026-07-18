using System.Collections.Generic;
using Verse;
using RimWorld.Planet;

namespace SimpleLeadership
{
    public class FactionLeadershipData : IExposable
    {
        public Dictionary<Settlement, Pawn> settlementLeaders = [];
        public Pawn actingLeader;
        public Pawn exLeader;

        public void ExposeData()
        {
            Scribe_Collections.Look(ref settlementLeaders, "settlementLeaders", LookMode.Reference, LookMode.Reference);
            Scribe_References.Look(ref actingLeader, "actingLeader");
            Scribe_References.Look(ref exLeader, "exLeader");
        }
    }
}
