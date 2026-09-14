using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xBot.Game;
using xBot.Game.Navigation;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    public partial class Bot
    {
        private readonly CombatRotationState rotation = new CombatRotationState();
        private long nextTeleportAttempt;
        private long nextLureAction;
        private int lureStage;
        private int lureDirection;
        private SRCoord lureEdge;
        private readonly Random lureRandom = new Random();
        private long nextEasterCheck;
        private readonly HashSet<uint> usedEasterNpcs = new HashSet<uint>();
        public bool IsLuring { get; private set; }

        private bool IsWithinTrainingBoundary(SRCoord point)
        {
            if (point == null) return false;
            if (!TrainingOptionsPolicy.StayInTrainingArea) return true;
            var center = Window.Get.TrainingArea_GetPosition();
            int radius = Window.Get.TrainingArea_GetRadius();
            return center == null || radius <= 0 || (TeleportTransitionPolicy.SameSpace(center, point)
                && center.DistanceTo(point) <= radius);
        }

        private void TryUseEasterEggNpc()
        {
            if (!TrainingOptionsPolicy.UseEasterEggEventNpcs || EngineNow < nextEasterCheck) return;
            nextEasterCheck = EngineNow + 5000;
            var start = InfoManager.Character?.GetRealtimePosition();
            if (start == null) return;
            usedEasterNpcs.RemoveWhere(id => InfoManager.Npcs.Find(n => n != null && n.UniqueID == id) == null);
            var npc = InfoManager.Npcs.Snapshot().Where(n => n != null && !usedEasterNpcs.Contains(n.UniqueID)
                && (n.ServerName ?? "").EndsWith("_EVENT_EASTEREGG", StringComparison.OrdinalIgnoreCase)
                && TeleportTransitionPolicy.SameSpace(start, n.GetRealtimePosition())
                && IsWithinTrainingBoundary(n.GetRealtimePosition()) && start.DistanceTo(n.GetRealtimePosition()) <= 50)
                .OrderBy(n => start.DistanceTo(n.GetRealtimePosition())).FirstOrDefault();
            if (npc == null || !ApproachTargetWithCollision(npc.GetRealtimePosition(), 4, null)) return;
            if (!WaitSelectEntity(npc.UniqueID, 3, 250, "Easter Egg seçiliyor...")) return;
            // Use only an unambiguous option advertised by this event NPC.
            if (npc.TalkOptions == null || npc.TalkOptions.Length != 1)
            {
                Window.Get.LogProcess("Easter Egg: tek bir etkileşim seçeneği sunulmadı.", Window.ProcessState.Warning);
                usedEasterNpcs.Add(npc.UniqueID);
                PacketBuilder.CloseNPC(npc.UniqueID);
                return;
            }
            InfoManager.MonitorNpcTalk.Reset();
            PacketBuilder.TalkNPC(npc.UniqueID, npc.TalkOptions[0]);
            long deadline = EngineNow + 1500;
            while (isBotting && EngineNow < deadline)
            {
                if (InfoManager.MonitorNpcTalk.WaitOne(50) && InfoManager.LastNpcTalkEntityUniqueID == npc.UniqueID)
                { usedEasterNpcs.Add(npc.UniqueID); break; }
            }
            if (isBotting && InfoManager.inGame) PacketBuilder.CloseNPC(npc.UniqueID);
        }

        private static bool HasDamageOverTime(SRMob mob)
        {
            return mob != null && (mob.BadStatusFlags & (SRModel.BadStatus.Bleed
                | SRModel.BadStatus.Poisoning | SRModel.BadStatus.Burn)) != SRModel.BadStatus.None;
        }

        private bool ShouldRotateTarget(SRMob current, SRCoord center, int radius)
        {
            if (!CombatAIEngine.SwitchTargetByPosition && !CombatAIEngine.SwitchMonsterAfterDot)
            { rotation.Reset(); return false; }
            var alternatives = InfoManager.Mobs.Snapshot().Where(m => m != null && m.UniqueID != current.UniqueID).ToList();
            if (CombatAIEngine.SwitchTargetByPosition)
            {
                var currentPreference = CombatAIEngine.FindPreference(current);
                int currentIndex = currentPreference != null && currentPreference.Preference == MonsterPreferenceType.Prefer
                    ? CombatAIEngine.GetPreferenceIndex(current) : -1;
                var preferred = alternatives.Where(m => {
                    var preference = CombatAIEngine.FindPreference(m);
                    int index = CombatAIEngine.GetPreferenceIndex(m);
                    return preference != null && preference.Preference == MonsterPreferenceType.Prefer
                        && index >= 0 && (currentIndex < 0 || index < currentIndex);
                }).ToList();
                if (GetMobFiltered(preferred, center, radius) != null) return true;
            }
            if (!CombatAIEngine.SwitchMonsterAfterDot) { rotation.Reset(); return false; }
            if (!rotation.DotDelayElapsed(current.UniqueID, HasDamageOverTime(current), EngineNow,
                CombatAIEngine.SwitchMonsterDotDelay)) return false;
            if (GetMobFiltered(alternatives, center, radius) == null) return false;
            rotation.Defer(current.UniqueID, EngineNow);
            return true;
        }

        public void ReturnAfterPartyBuff(SRCoord center)
        {
            var position = InfoManager.Character?.GetRealtimePosition();
            if (IsLuring || !TeleportTransitionPolicy.SameSpace(position, center)) return;
            ApproachTargetWithCollision(center, 2, null);
        }

        public void RunScriptBerserk()
        {
            var character = InfoManager.Character;
            if (!CombatAIEngine.ZerkInScript || character == null || !isBotting
                || character.BerserkPoints < 5 || character.GameStateType == SRModel.GameState.Berserk
                || (DateTime.UtcNow - s_lastBerserkAttemptUtc).TotalSeconds < 3) return;
            s_lastBerserkAttemptUtc = DateTime.UtcNow;
            PacketBuilder.ActivateBerserk();
        }

        private bool TryCastTeleportSkill(SRMob mob, double stopRange)
        {
            var character = InfoManager.Character;
            if (!CombatAIEngine.UseTeleportSkills || character?.Skills == null || !IsLiveCombatTarget(mob)
                || character.isRiding || EngineNow < nextTeleportAttempt || !NavigationManager.Get.IsAvailable) return false;
            var start = character.GetRealtimePosition();
            var target = mob.GetRealtimePosition();
            if (!TeleportTransitionPolicy.SameSpace(start, target)) return false;
            double distance = start.DistanceTo(target);
            if (distance <= stopRange + 5) return false;
            SRSkill skill = character.Skills.Snapshot().Where(s => s != null && s.Enabled && s.isCastingEnabled
                && s.MPUsage <= character.MP && GetTeleportDistance(s) > 0)
                .OrderByDescending(GetTeleportDistance).FirstOrDefault();
            if (skill == null) return false;
            double step = Math.Min(GetTeleportDistance(skill), distance - Math.Max(2, stopRange));
            var destination = new SRCoord(start.PosX + (target.PosX - start.PosX) * step / distance,
                start.PosY + (target.PosY - start.PosY) * step / distance, start.Region, start.Z);
            var center = Window.Get.TrainingArea_GetPosition();
            int radius = Window.Get.TrainingArea_GetRadius();
            if (TrainingOptionsPolicy.StayInTrainingArea && center != null && radius > 0
                && center.DistanceTo(destination) > radius) return false;
            var path = NavigationManager.Get.FindPath(start, destination);
            // A blink must not cut across a wall or use a region transition.
            if (path == null || path.Count == 0) return false;
            var previous = start;
            double length = 0;
            foreach (var point in path) { length += previous.DistanceTo(point); previous = point; }
            length += previous.DistanceTo(destination);
            if (length > step + 1) return false;
            nextTeleportAttempt = EngineNow + 5000;
            if (!WaitForCharacterAction(mob) || !TryPrepareAttackSkill(skill, Window.Get)) return false;
            if (!PacketBuilder.CastSkillAtPosition(skill.ID, destination)) return false;
            long deadline = EngineNow + 1800;
            while (isBotting && EngineNow < deadline && InfoManager.Character == character)
            {
                if (character.GetRealtimePosition().DistanceTo(destination) <= 3) return true;
                if (!SleepInterruptible(50)) break;
            }
            return false;
        }

        internal static double GetTeleportDistance(SRSkill skill)
        {
            // tele/tel2 params in the loaded skill database: speed, distance (decimetres).
            string[] values = (skill?.Params ?? "").Split('|');
            for (int i = 0; i + 2 < values.Length; i++)
                if ((values[i] == "1952803941" || values[i] == "1952803890")
                    && double.TryParse(values[i + 2], out double distance))
                    return Math.Max(0, Math.Min(50, distance / 10));
            return 0;
        }

        private bool LureShouldPause(SRCoord center, int radius)
        {
            var character = InfoManager.Character;
            if (!isBotting || character == null || character.LifeStateType != SRModel.LifeState.Alive || ProtectionManager.HasPendingReturn) return true;
            var members = InfoManager.Party?.Members?.Snapshot();
            int dead = members == null ? 0 : members.Count(m => m != null && m.HPPercent == 0);
            var mobs = InfoManager.Mobs.Snapshot().Where(m => IsLiveCombatTarget(m)
                && TeleportTransitionPolicy.SameSpace(center, m.GetRealtimePosition())
                && center.DistanceTo(m.GetRealtimePosition()) <= radius).ToList();
            bool partyNear = !LurePolicy.StopIfPartyAway || members != null;
            if (LurePolicy.StopIfPartyAway && members != null)
                foreach (var member in members)
                {
                    if (member == null || member.Name == character.Name) continue;
                    if (LurePolicy.PartyNearWhitelist.Count > 0 && !LurePolicy.PartyNearWhitelist.Contains(member.Name)) continue;
                    var player = InfoManager.Players.Find(p => p != null && p.Name == member.Name);
                    if (player?.Position == null || !TeleportTransitionPolicy.SameSpace(center, player.Position)
                        || center.DistanceTo(player.Position) > Math.Max(1, LurePolicy.PartyMemberFarAwayDistance))
                    { partyNear = false; break; }
                }
            if (LurePolicy.StopTownLessEnabled && (members?.Count ?? 0) < Math.Max(1, LurePolicy.StopTownLessCount))
            {
                ProtectionManager.RequestReturn("Lure: party member count is below the configured limit.", true);
                return true;
            }
            return LurePolicy.ShouldPauseLure(dead, mobs.Count(m => m.MobType == SRMob.Mob.PartyGiant), mobs.Count, partyNear);
        }

        public bool CastLureSkill(SRSkill skill, SRCoord center, int radius)
        {
            if (skill == null || !skill.Enabled || !skill.isCastingEnabled || InfoManager.Character == null
                || skill.MPUsage > InfoManager.Character.MP || !WaitForCharacterAction()) return false;
            uint target = 0;
            if (skill.isTargetRequired)
            {
                var mob = GetMobFiltered(InfoManager.Mobs.Snapshot().Where(m => m != null
                    && m.TargetUniqueID != InfoManager.Character.UniqueID).ToList(), center, radius);
                if (mob == null || !EnsureCombatTargetSelected(mob)) return false;
                if (!IsWithinTrainingBoundary(mob.GetRealtimePosition())) return false;
                target = mob.UniqueID;
            }
            if (!TryPrepareAttackSkill(skill, Window.Get) || !PacketBuilder.CastSkill(skill.ID, target)) return false;
            return SleepInterruptible(Math.Max(350, skill.CastingTime + 100));
        }

        public void RunLureBuffs() { if (isBotting && InfoManager.inGame) BuffLoop(); }

        private bool CheckLureTick(Window window, SRCoord center, int radius)
        {
            bool enabled = LurePolicy.WalkBackDistEnabled || LurePolicy.LureSkillEnabled || LurePolicy.UseScript;
            if (!enabled || center == null) { IsLuring = false; lureStage = 0; return false; }
            if (EngineNow < nextLureAction)
            {
                if (lureStage == 0) return false;
                SleepInterruptible(100); return true;
            }
            if (LureShouldPause(center, radius)) { IsLuring = false; lureStage = 0; nextLureAction = EngineNow + 1000; return false; }
            IsLuring = true;
            try
            {
                if (LurePolicy.UseScript)
                {
                    if (string.IsNullOrWhiteSpace(LurePolicy.ScriptPath) || !File.Exists(LurePolicy.ScriptPath))
                    { window.LogProcess("Lure: selected script does not exist.", Window.ProcessState.Warning); nextLureAction = EngineNow + 5000; return false; }
                    Script previous = currentScript;
                    bool completed = false;
                    try
                    {
                        currentScript = new Script(LurePolicy.ScriptPath) { IsLureScript = true,
                            ContinueCondition = () => !LureShouldPause(center, radius) };
                        currentScript.Run();
                        completed = currentScript.Completed;
                    }
                    finally { currentScript = previous; }
                    if (completed && LurePolicy.BuffAtLureEnd) RunLureBuffs();
                    nextLureAction = EngineNow + Math.Max(100, LurePolicy.DelayCenterMs);
                    return true;
                }
                var skill = InfoManager.Character?.Skills?.Find(s => s != null && (s.Name == LurePolicy.LureSkillName || s.ServerName == LurePolicy.LureSkillName));
                if (!LurePolicy.WalkBackDistEnabled || LurePolicy.WalkBackDist <= 0)
                {
                    if (LurePolicy.LureSkillEnabled) CastLureSkill(skill, center, radius);
                    nextLureAction = EngineNow + Math.Max(500, LurePolicy.DelayCenterMs);
                    return false;
                }
                if (LurePolicy.AttackLimitEnabled && InfoManager.Mobs.Snapshot().Count(m => IsLiveCombatTarget(m)
                    && m.TargetUniqueID == InfoManager.Character.UniqueID) >= Math.Max(1, LurePolicy.AttackMobLimit)) lureStage = 3;
                if (lureStage == 0)
                {
                    double distance = Math.Min(200, LurePolicy.WalkBackDist);
                    if (TrainingOptionsPolicy.StayInTrainingArea && radius > 0) distance = Math.Min(distance, radius);
                    double angle = LurePolicy.RandomWalk ? lureRandom.NextDouble() * Math.PI * 2 : (lureDirection++ % 8) * Math.PI / 4;
                    if (LurePolicy.WalkToSpawns || LurePolicy.SmartWalk)
                    {
                        var candidates = InfoManager.Mobs.Snapshot().Where(m => IsLiveCombatTarget(m)
                            && TeleportTransitionPolicy.SameSpace(center, m.GetRealtimePosition())).ToList();
                        var mob = GetMobFiltered(candidates, center, (int)Math.Max(radius, distance));
                        if (mob != null) angle = Math.Atan2(mob.GetRealtimePosition().PosY - center.PosY, mob.GetRealtimePosition().PosX - center.PosX);
                    }
                    lureEdge = new SRCoord(center.PosX + Math.Cos(angle) * distance, center.PosY + Math.Sin(angle) * distance, center.Region, center.Z);
                    if (!MoveLurePoint(lureEdge)) { lureStage = 3; return true; }
                    if (LurePolicy.LureSkillEnabled) CastLureSkill(skill, center, (int)Math.Max(radius, distance));
                    lureStage = 1;
                    nextLureAction = EngineNow + Math.Max(0, LurePolicy.DelayEdgeMs);
                }
                else if (lureStage == 1)
                {
                    var half = new SRCoord((center.PosX + lureEdge.PosX) / 2, (center.PosY + lureEdge.PosY) / 2, center.Region, center.Z);
                    MoveLurePoint(half);
                    lureStage = 3;
                    nextLureAction = EngineNow + Math.Max(0, LurePolicy.DelayHalfMs);
                }
                else
                {
                    MoveLurePoint(center);
                    lureStage = 0;
                    if (LurePolicy.BuffAtLureEnd) RunLureBuffs();
                    nextLureAction = EngineNow + Math.Max(100, LurePolicy.DelayCenterMs);
                }
                return true;
            }
            finally { IsLuring = lureStage != 0; }
        }

        private bool MoveLurePoint(SRCoord target)
        {
            var start = InfoManager.Character?.GetRealtimePosition();
            if (!TeleportTransitionPolicy.SameSpace(start, target)) return false;
            if (LurePolicy.SmartWalk && NavigationManager.Get.IsAvailable)
            {
                var path = NavigationManager.Get.FindPath(start, target);
                if (path == null) return false;
                foreach (var point in path)
                    if (!ApproachTargetWithCollision(point, 2, null)) return false;
                return true;
            }
            return ApproachTargetWithCollision(target, 2, null);
        }
    }
}
