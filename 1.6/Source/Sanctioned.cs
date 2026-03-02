using RimWorld;
using Verse;

namespace SimpleLeadership
{
    public class Sanctioned : PowerEventBase
    {
        public Faction faction;

        public override void SetParameters(params object[] args)
        {
            if (args.Length > 0 && args[0] is Faction f)
            {
                faction = f;
            }
        }

        public override bool IsDuplicate(PowerEventBase other)
        {
            return other is Sanctioned otherSanction && otherSanction.faction == faction;
        }

        public override bool IsTarget(object target)
        {
            return target is Faction f && f == faction;
        }

        public override object GetTarget() => faction;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
        }

        protected override string GetFormattedMessage(string message)
        {
            return string.Format(message, faction.Name);
        }
    }
}
