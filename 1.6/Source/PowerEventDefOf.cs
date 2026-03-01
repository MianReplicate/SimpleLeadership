using RimWorld;

namespace SimpleLeadership
{
    [DefOf]
    public static class PowerEventDefOf
    {
        public static PowerEventDef SL_PowerVoid;
        public static PowerEventDef SL_PowerStruggle;
        public static PowerEventDef SL_Fortifying;
        public static PowerEventDef SL_Inspection;
        public static PowerEventDef SL_Vigilant;
        public static PowerEventDef SL_Famine;
        public static PowerEventDef SL_Sanctioned;
        public static PowerEventDef SL_Support;
        public static PowerEventDef SL_PrisonerTransfer;
        public static PowerEventDef SL_Reinforcements;
        static PowerEventDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PowerEventDefOf));
        }
    }
}
