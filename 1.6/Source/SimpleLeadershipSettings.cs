using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace SimpleLeadership
{
    public class SimpleLeadershipSettings : ModSettings
    {
        [Header("SL_General")]
        [Label("SL_EnableAlerts")]
        public bool enableAlerts = true;

        [Label("SL_EnableEvents")]
        public bool enableEvents = true;

        [Label("SL_DistanceWeight")]
        [Percentage]
        public float distanceWeight = 0.6f;

        [Label("SL_LeaderSpawnChance")]
        [Percentage]
        public float leaderSpawnChance = 0.02f;

        [Label("SL_BasesPerLeader")]
        [Range(1, 20)]
        [Step(1f)]
        public int basesPerLeader = 5;

        [Header("SL_Blacklist")]
        [Description("SL_BlacklistDesc")]
        [DrawMethod("DrawFactionBlacklist", SerializeField = false)]
        [SettingOptions(drawValue: false, showDefaultValue: false)]
        public List<string> factionBlacklist = [];

        public override void ExposeData()
        {
            base.ExposeData();
            SimpleSettings.AutoExpose(this);
            Scribe_Collections.Look(ref factionBlacklist, "factionBlacklist", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                factionBlacklist ??= [];
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            SimpleSettings.DrawWindow(this, inRect);
        }

        public float DrawFactionBlacklist(ModSettings _, SimpleSettings.MemberWrapper member, Rect area)
        {
            var defs = DefDatabase<FactionDef>.AllDefs
                .Where(d => d.humanlikeFaction && !d.isPlayer && d.settlementGenerationWeight > 0f)
                .OrderBy(d => d.label)
                .ToList();

            const float RowHeight = 28f;
            float height = 0f;
            foreach (var def in defs)
            {
                bool blacklisted = factionBlacklist.Contains(def.defName);
                bool prev = blacklisted;
                Widgets.CheckboxLabeled(new Rect(area.x, area.y + height, area.width, RowHeight), def.label.CapitalizeFirst(), ref blacklisted);
                if (prev != blacklisted)
                {
                    if (blacklisted) factionBlacklist.Add(def.defName);
                    else factionBlacklist.Remove(def.defName);
                }
                height += RowHeight;
            }
            return height;
        }
    }
}
