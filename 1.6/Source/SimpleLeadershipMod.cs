using HarmonyLib;
using UnityEngine;
using Verse;

namespace SimpleLeadership
{
    public class SimpleLeadershipMod : Mod
    {
        public static SimpleLeadershipSettings Settings;

        public SimpleLeadershipMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SimpleLeadershipSettings>();
            new Harmony("pb3n.Taranchuk.SimpleLeadership").PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }
}
