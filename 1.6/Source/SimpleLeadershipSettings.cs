using UnityEngine;
using Verse;

namespace SimpleLeadership
{
    public class SimpleLeadershipSettings : ModSettings
    {
        [Header("SL_General")]
        [Label("SL_EnableAlerts")]
        public bool enableAlerts = true;

        [Label("SL_EnableEvents")]
        public bool enableEvents = true;

        [Label("SL_LeaderSpawnChance")]
        [Percentage]
        public float leaderSpawnChance = 0.02f;

        public override void ExposeData()
        {
            base.ExposeData();
            SimpleSettings.AutoExpose(this);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            SimpleSettings.DrawWindow(this, inRect);
        }
    }
}
