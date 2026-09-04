using System;

namespace xBot.App
{
    /// <summary>
    /// Pure combat decisions shared by the runtime and scenario tests.
    /// Keeping these checks free of UI/game state makes the rules predictable.
    /// </summary>
    public sealed class CombatTargetInput
    {
        public bool AllowedByType { get; set; } = true;
        public bool Avoided { get; set; }
        public bool IsDimensionPillar { get; set; }
        public bool IgnoreDimensionPillars { get; set; }
        public bool DoNotFollowMobs { get; set; }
        public bool WithinTrainingArea { get; set; } = true;
    }

    public static class CombatPolicy
    {
        public static bool CanTarget(CombatTargetInput input)
        {
            if (input == null || !input.AllowedByType || input.Avoided)
                return false;

            if (input.IgnoreDimensionPillars && input.IsDimensionPillar)
                return false;

            if (input.DoNotFollowMobs && !input.WithinTrainingArea)
                return false;

            return true;
        }

        public static bool ShouldBerserk(bool berserkBarFull, bool alreadyBerserk,
            bool hpFullTrigger, int nearbyMobCount, bool monsterCountEnabled,
            int monsterCountThreshold, bool mobRuleTrigger)
        {
            if (!berserkBarFull || alreadyBerserk || nearbyMobCount <= 0)
                return false;

            if (hpFullTrigger || mobRuleTrigger)
                return true;

            return monsterCountEnabled
                && nearbyMobCount >= Math.Max(1, monsterCountThreshold);
        }
    }
}
