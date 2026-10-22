using System;
using System.Collections.Generic;

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
        public bool AllowAvoidFallback { get; set; }
        public bool IsDimensionPillar { get; set; }
        public bool IgnoreDimensionPillars { get; set; }
        public bool DoNotFollowMobs { get; set; }
        public bool WithinTrainingArea { get; set; } = true;
    }

    public sealed class CombatAttackingTracker
    {
        private readonly Dictionary<uint, long> _attackTimestamps = new Dictionary<uint, long>();

        public void RecordAttack(uint mobUniqueId, long nowMs)
        {
            lock (_attackTimestamps)
            {
                _attackTimestamps[mobUniqueId] = nowMs;
            }
        }

        public int GetAttackingCount(long nowMs, long timeframeMs)
        {
            lock (_attackTimestamps)
            {
                int count = 0;
                var toRemove = new List<uint>();
                foreach (var kvp in _attackTimestamps)
                {
                    if (nowMs - kvp.Value <= timeframeMs)
                        count++;
                    else
                        toRemove.Add(kvp.Key);
                }
                for (int i = 0; i < toRemove.Count; i++)
                    _attackTimestamps.Remove(toRemove[i]);
                return count;
            }
        }

        public void Clear()
        {
            lock (_attackTimestamps)
            {
                _attackTimestamps.Clear();
            }
        }
    }

    public static class CombatPolicy
    {
        public static bool IsLiveTarget(bool present, bool dead)
        {
            return present && !dead;
        }

        public static bool CanSendAttack(bool liveTarget, uint targetId, uint selectedId)
        {
            return liveTarget && targetId != 0 && targetId == selectedId;
        }

        public static bool CanTarget(CombatTargetInput input)
        {
            if (input == null || !input.AllowedByType || (input.Avoided && !input.AllowAvoidFallback))
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

        /// <summary>
        /// SBot extra:sticktomob — stay at same mob until it's dead, unreachable, or out of training bounds.
        /// </summary>
        public static bool ShouldLockTarget(bool stickToMob, bool hasSelectedTarget, bool isLive, bool isWithinBoundary, bool isReachable)
        {
            return stickToMob && hasSelectedTarget && isLive && isWithinBoundary && isReachable;
        }

        /// <summary>
        /// SBot emerg:* — trigger emergency buffs if mobs attacking >= threshold or HP% <= threshold.
        /// </summary>
        public static bool IsEmergencyBuffTriggered(
            bool attEnabled, int attType, int currentAttackingCount, int timeframeAttackingCount, int mobThreshold,
            bool hpEnabled, int currentHpPercent, int hpThreshold)
        {
            if (attEnabled && mobThreshold > 0)
            {
                int count = (attType == 1) ? timeframeAttackingCount : currentAttackingCount;
                if (count >= mobThreshold)
                    return true;
            }

            if (hpEnabled && hpThreshold > 0 && currentHpPercent <= hpThreshold)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pure calculation of target scoring considering SBot/phBot priority factors.
        /// </summary>
        public static double CalculateTargetScore(
            double baseScore,
            double distance,
            bool isPreferredType,
            bool isAttackingParty,
            bool isAttackingPet,
            bool isAttackedByDesignatedPlayer,
            bool attackLowerFirst,
            int mobLevel,
            bool attackWeakerFirst,
            long currentHp,
            long maxHp)
        {
            return CalculateTargetScore(baseScore, distance, isPreferredType, isAttackingParty,
                isAttackingPet, isAttackedByDesignatedPlayer, attackLowerFirst, mobLevel,
                attackWeakerFirst, currentHp, maxHp, 0, false, false);
        }

        /// <summary>
        /// Scores tactical urgency and the amount of movement still needed to get a usable attack.
        /// Range-adjusted travel cost keeps ranged characters from walking toward targets they can
        /// already hit; current-target and self-defense bonuses reduce needless retargeting.
        /// </summary>
        public static double CalculateTargetScore(
            double baseScore,
            double distance,
            bool isPreferredType,
            bool isAttackingParty,
            bool isAttackingPet,
            bool isAttackedByDesignatedPlayer,
            bool attackLowerFirst,
            int mobLevel,
            bool attackWeakerFirst,
            long currentHp,
            long maxHp,
            double attackRange,
            bool isCurrentTarget,
            bool isAttackingPlayer)
        {
            double score = baseScore;

            if (isAttackingPlayer)
                score += 11000.0;

            if (isAttackedByDesignatedPlayer)
                score += 10000.0;

            if (isAttackingPet)
                score += 9000.0;

            if (isAttackingParty)
                score += 8000.0;

            if (isPreferredType)
                score += 200.0;

            if (attackLowerFirst && mobLevel > 0)
                score += Math.Max(-200.0, Math.Min(200.0, (140 - mobLevel) * 4.0));

            if (attackWeakerFirst && maxHp > 0)
            {
                double hpRatio = Math.Max(0.0, Math.Min(1.0, (double)currentHp / maxHp));
                score += (1.0 - hpRatio) * 80.0;
            }

            if (distance <= 6.0)
                score += 35.0;

            double range = Math.Max(0.0, attackRange);
            double movementNeeded = Math.Max(0.0, distance - range);
            double distanceWithinRange = Math.Min(Math.Max(0.0, distance), range);
            if (range > 0.0 && distance <= range)
                score += 45.0;
            if (isCurrentTarget)
                score += 90.0;

            score -= (movementNeeded * 1.5) + (distanceWithinRange * 0.25);
            return score;
        }

        /// <summary>
        /// SBot skills:skilliteratemob — whether to reset skill iteration index when starting a new mob.
        /// </summary>
        public static bool ShouldIterateReset(bool skillIterateMob, bool isNewMob)
        {
            return skillIterateMob && isNewMob;
        }
    }
}
