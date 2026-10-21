using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Navigation;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Party;

namespace xBot.App
{
    public class PartyBuffAssignment
    {
        public string PlayerName { get; set; } = "";
        public string SkillName { get; set; } = "";
        public uint SkillId { get; set; }
    }

    public static class PartySupportManager
    {
        public static bool PartyHealEnabled { get; set; } = true;
        public static byte PartyHealHPPercent { get; set; } = 70;
        public static bool PartyAutoRessEnabled { get; set; } = true;
        public static bool PartyCureEnabled { get; set; } = true;
        public static bool PartyBuffsEnabled { get; set; } = true;

        // Party Buff Assignments (Parti Sekmesi)
        public static List<PartyBuffAssignment> PartyBuffAssignments { get; } = new List<PartyBuffAssignment>();

        // Party Options (Parti Ayarları Sekmesi)
        public static int PartyBuffRadius { get; set; } = 50;
        public static bool BuffAndReturnToCenter { get; set; } = true;
        public static bool BuffAllNearbyPlayers { get; set; } = false;
        public static bool BuffProximityStart { get; set; } = false;
        public static bool BuffProximityStop { get; set; } = true;
        public static List<string> BuffProximityPlayers { get; } = new List<string>();

        private static readonly Dictionary<string, (DateTime FirstDeadTime, int Retries)> DeadPlayerTracker = new Dictionary<string, (DateTime, int)>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, DateTime> TemporarilyDisabledResPlayers = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, DateTime> recentBuffs = new Dictionary<string, DateTime>();
        private static DateTime lastSupportActionUtc = DateTime.MinValue;
        private static readonly object supportLock = new object();

        public static void ResetRuntimeState()
        {
            lock (supportLock)
            {
                recentBuffs.Clear();
                DeadPlayerTracker.Clear();
                TemporarilyDisabledResPlayers.Clear();
                lastSupportActionUtc = DateTime.MinValue;
            }
        }

        private static bool IsNearStatueOfJustice(SRCoord pos, double dangerRadius = 25.0)
        {
            if (pos == null || InfoManager.Mobs == null)
                return false;

            try
            {
                for (int i = 0; i < InfoManager.Mobs.Count; i++)
                {
                    SRMob m = InfoManager.Mobs.GetAt(i);
                    if (m == null) continue;
                    string sn = m.ServerName ?? "";
                    string nm = m.Name ?? "";
                    bool isStatue = sn.IndexOf("STATUE_OF_JUSTICE", StringComparison.OrdinalIgnoreCase) >= 0
                        || sn.IndexOf("BOTKILL", StringComparison.OrdinalIgnoreCase) >= 0
                        || nm.IndexOf("Statue of Justice", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!isStatue) continue;

                    SRCoord mPos = m.GetRealtimePosition() ?? m.Position;
                    if (mPos != null && pos.DistanceTo(mPos) <= dangerRadius)
                        return true;
                }
            }
            catch { }
            return false;
        }

        private static bool IsPlayerResTemporarilyDisabled(string playerName, DateTime nowUtc)
        {
            if (string.IsNullOrEmpty(playerName))
                return false;
            if (TemporarilyDisabledResPlayers.TryGetValue(playerName, out DateTime untilUtc))
            {
                if (nowUtc < untilUtc)
                    return true;
                TemporarilyDisabledResPlayers.Remove(playerName);
            }
            return false;
        }

        public static void RunTick()
        {
            if (InfoManager.Character == null || !InfoManager.inGame || !Bot.Get.isBotting)
                return;

            if (InfoManager.Character.LifeStateType == SRModel.LifeState.Dead)
                return;

            // Throttle party support actions to prevent packet spam
            if ((DateTime.UtcNow - lastSupportActionUtc).TotalMilliseconds < 1500)
                return;

            if (!Monitor.TryEnter(supportLock))
                return;

            SRSkill pendingSkill = null;
            uint pendingTarget = 0;
            int pendingSleep = 0;
            string pendingLog = null;
            string pendingBuffKey = null;
            try
            {
                foreach (var key in new List<string>(recentBuffs.Keys))
                    if (recentBuffs[key] <= DateTime.UtcNow) recentBuffs.Remove(key);
                SRCoord myPos = InfoManager.Character.GetRealtimePosition();
                if (myPos == null)
                    return;

                // 1. Check for Dead Party Members or Regex Players to Resurrect
                if (PartyAutoRessEnabled)
                {
                    DateTime now = DateTime.UtcNow;
                    double resRange = Math.Max(10.0, (double)ResurrectPolicy.ResurrectRange);

                    // Count dead party members in range (SBot: >1 dead in range -> Group Resurrect)
                    int deadPartyInRangeCount = 0;
                    if (InfoManager.Party != null && InfoManager.Party.Members != null)
                    {
                        for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                        {
                            SRPartyMember m = InfoManager.Party.Members.GetAt(i);
                            if (m == null || m.Name == InfoManager.Character.Name || m.HPPercent != 0)
                                continue;
                            if (IsPlayerResTemporarilyDisabled(m.Name, now))
                                continue;
                            SRPlayer lp = InfoManager.Players.Find(p => p != null && string.Equals(p.Name, m.Name, StringComparison.OrdinalIgnoreCase));
                            if (lp != null && lp.Position != null && myPos.DistanceTo(lp.Position) <= resRange)
                                deadPartyInRangeCount++;
                        }
                    }

                    bool preferGroupRess = deadPartyInRangeCount > 1;

                    // A) Parti üyelerini tara
                    if (InfoManager.Party != null && InfoManager.Party.Members != null)
                    {
                        for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                        {
                            SRPartyMember member = InfoManager.Party.Members.GetAt(i);
                            if (member == null || member.Name == InfoManager.Character.Name)
                                continue;

                            if (member.HPPercent == 0)
                            {
                                if (IsPlayerResTemporarilyDisabled(member.Name, now))
                                    continue;

                                bool allowRes = ResurrectPolicy.ResurrectAllParty || ResurrectPolicy.MatchesRegex(member.Name);
                                if (allowRes)
                                {
                                    SRPlayer livePlayer = InfoManager.Players.Find(p => p != null && string.Equals(p.Name, member.Name, StringComparison.OrdinalIgnoreCase));
                                    if (livePlayer != null && livePlayer.Position != null && myPos.DistanceTo(livePlayer.Position) <= resRange)
                                    {
                                        // SBot Statue of Justice trap protection
                                        if (IsNearStatueOfJustice(livePlayer.Position) || IsNearStatueOfJustice(myPos))
                                        {
                                            TemporarilyDisabledResPlayers[member.Name] = now.AddMinutes(15);
                                            DeadPlayerTracker.Remove(member.Name);
                                            Window.Get?.Log($"* Autores: Player [{member.Name}] to be resurrected is too close to a statue, deactivating autores for the player (15m).");
                                            continue;
                                        }

                                        if (!DeadPlayerTracker.TryGetValue(member.Name, out var track))
                                        {
                                            track = (now, 0);
                                            DeadPlayerTracker[member.Name] = track;
                                        }

                                        if (ResurrectPolicy.RetryLimit > 0 && track.Retries >= ResurrectPolicy.RetryLimit)
                                        {
                                            TemporarilyDisabledResPlayers[member.Name] = now.AddMinutes(15);
                                            DeadPlayerTracker.Remove(member.Name);
                                            Window.Get?.Log($"* Autores: Deactivating autores for [{member.Name}] temporarily (for 15 minutes).");
                                            continue;
                                        }

                                        if ((now - track.FirstDeadTime).TotalMilliseconds >= ResurrectPolicy.ResurrectDelayMs)
                                        {
                                            SRSkill ressSkill = FindResurrectSkill(preferGroupRess);
                                            if (ressSkill != null && ressSkill.isCastingEnabled)
                                            {
                                                bool isGroupSkill = PartyPolicy.IsGroupRessSkill(ressSkill.ServerName);
                                                pendingSkill = ressSkill;
                                                pendingTarget = isGroupSkill && !ressSkill.isTargetRequired ? 0u : livePlayer.UniqueID;
                                                pendingSleep = Math.Max(500, ressSkill.CastingTime + 200);
                                                pendingLog = $"Party Support: Resurrecting [{member.Name}] with {ressSkill.Name}...";
                                                DeadPlayerTracker[member.Name] = (track.FirstDeadTime, track.Retries + 1);
                                                lastSupportActionUtc = DateTime.UtcNow;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                DeadPlayerTracker.Remove(member.Name);
                                TemporarilyDisabledResPlayers.Remove(member.Name);
                            }
                        }
                    }

                    // B) Regex ile eşleşen yakındaki ölü diğer oyuncuları tara
                    if (pendingSkill == null && ResurrectPolicy.ResurrectRegexList.Count > 0)
                    {
                        for (int i = 0; i < InfoManager.Players.Count; i++)
                        {
                            SRPlayer p = InfoManager.Players.GetAt(i);
                            if (p == null || p.Name == InfoManager.Character.Name) continue;
                            if (p.LifeStateType != SRModel.LifeState.Dead)
                            {
                                DeadPlayerTracker.Remove(p.Name);
                                TemporarilyDisabledResPlayers.Remove(p.Name);
                                continue;
                            }
                            if (IsPlayerResTemporarilyDisabled(p.Name, now))
                                continue;

                            if (p.LifeStateType == SRModel.LifeState.Dead && ResurrectPolicy.MatchesRegex(p.Name))
                            {
                                if (p.Position != null && myPos.DistanceTo(p.Position) <= resRange)
                                {
                                    if (IsNearStatueOfJustice(p.Position) || IsNearStatueOfJustice(myPos))
                                    {
                                        TemporarilyDisabledResPlayers[p.Name] = now.AddMinutes(15);
                                        DeadPlayerTracker.Remove(p.Name);
                                        Window.Get?.Log($"* Autores: Player [{p.Name}] to be resurrected is too close to a statue, deactivating autores for the player (15m).");
                                        continue;
                                    }

                                    if (!DeadPlayerTracker.TryGetValue(p.Name, out var track))
                                    {
                                        track = (now, 0);
                                        DeadPlayerTracker[p.Name] = track;
                                    }

                                    if (ResurrectPolicy.RetryLimit > 0 && track.Retries >= ResurrectPolicy.RetryLimit)
                                    {
                                        TemporarilyDisabledResPlayers[p.Name] = now.AddMinutes(15);
                                        DeadPlayerTracker.Remove(p.Name);
                                        Window.Get?.Log($"* Autores: Deactivating autores for [{p.Name}] temporarily (for 15 minutes).");
                                        continue;
                                    }

                                    if ((now - track.FirstDeadTime).TotalMilliseconds >= ResurrectPolicy.ResurrectDelayMs)
                                    {
                                        SRSkill ressSkill = FindResurrectSkill(false);
                                        if (ressSkill != null && ressSkill.isCastingEnabled)
                                        {
                                            pendingSkill = ressSkill;
                                            pendingTarget = p.UniqueID;
                                            pendingSleep = Math.Max(500, ressSkill.CastingTime + 200);
                                            pendingLog = $"Party Support: Resurrecting non-party [{p.Name}] with {ressSkill.Name}...";
                                            DeadPlayerTracker[p.Name] = (track.FirstDeadTime, track.Retries + 1);
                                            lastSupportActionUtc = DateTime.UtcNow;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // 2. Check for Low HP Party Members to Heal
                if (pendingSkill == null && PartyHealEnabled && InfoManager.Party?.Members != null)
                {
                    for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                    {
                        SRPartyMember member = InfoManager.Party.Members.GetAt(i);
                        if (member == null || member.Name == InfoManager.Character.Name)
                            continue;

                        if (PartyPolicy.ShouldHealPartyMember(member.HPPercent, PartyHealHPPercent))
                        {
                            SRPlayer livePlayer = InfoManager.Players.Find(p => p != null && string.Equals(p.Name, member.Name, StringComparison.OrdinalIgnoreCase));
                            if (livePlayer != null && livePlayer.Position != null && myPos.DistanceTo(livePlayer.Position) <= 35.0)
                            {
                                SRSkill healSkill = FindPartyHealSkill();
                                if (healSkill != null && healSkill.isCastingEnabled)
                                {
                                    pendingSkill = healSkill; pendingTarget = livePlayer.UniqueID;
                                    pendingSleep = Math.Max(500, healSkill.CastingTime + 200);
                                    pendingLog = $"Party Support: Healing [{member.Name}] ({member.HPPercent}% HP) with {healSkill.Name}...";
                                    lastSupportActionUtc = DateTime.UtcNow;
                                    break;
                                }
                            }
                        }
                    }
                }

                // 3. Check Cure Bad Status (sadece gerçekten kötü status varsa)
                if (pendingSkill == null && PartyCureEnabled)
                {
                    SRSkill cureSkill = FindPartyCureSkill();
                    bool needCure = InfoManager.Character.BadStatusFlags != SRModel.BadStatus.None;
                    if (!needCure && InfoManager.Party?.Members != null)
                    {
                        for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                        {
                            var m = InfoManager.Party.Members.GetAt(i);
                            if (m == null || m.Name == InfoManager.Character.Name) continue;
                            var lp = InfoManager.Players.Find(p => p != null && string.Equals(p.Name, m.Name, StringComparison.OrdinalIgnoreCase));
                            if (lp != null && lp.BadStatusFlags != SRModel.BadStatus.None
                                && lp.Position != null && InfoManager.Character.GetRealtimePosition() != null
                                && InfoManager.Character.GetRealtimePosition().DistanceTo(lp.Position) <= 35.0)
                            { needCure = true; break; }
                        }
                    }
                    if (needCure && cureSkill != null && cureSkill.isCastingEnabled && (DateTime.UtcNow - lastSupportActionUtc).TotalSeconds > 15)
                    {
                        pendingSkill = cureSkill; pendingTarget = 0;
                        pendingSleep = Math.Max(400, cureSkill.CastingTime + 100);
                        pendingLog = "Party Support: Curing bad status...";
                        lastSupportActionUtc = DateTime.UtcNow;
                    }
                }

                // 4. Check Assigned Party Buffs (Parti Sekmesi & Parti Ayarları)
                if (pendingSkill == null && PartyBuffsEnabled && PartyBuffAssignments.Count > 0
                    && !(Bot.Get.IsLuring && LurePolicy.BuffAtLureEnd))
                {
                    // Yakın oyuncu kuralı (Proximity Rule)
                    bool proximityAllow = true;
                    if (BuffProximityPlayers.Count > 0)
                    {
                        bool anyProximityNear = false;
                        for (int pi = 0; pi < InfoManager.Players.Count; pi++)
                        {
                            var p = InfoManager.Players.GetAt(pi);
                            if (p == null || p.Name == InfoManager.Character.Name) continue;
                            if (p.Position != null && myPos.DistanceTo(p.Position) <= 60.0)
                            {
                                if (BuffProximityPlayers.Contains(p.Name))
                                {
                                    anyProximityNear = true;
                                    break;
                                }
                            }
                        }

                        if (BuffProximityStop && anyProximityNear)
                            proximityAllow = false;
                        else if (BuffProximityStart && !anyProximityNear)
                            proximityAllow = false;
                    }

                    if (proximityAllow)
                    {
                        double buffRange = Math.Max(10.0, (double)PartyBuffRadius);
                        for (int ai = 0; ai < PartyBuffAssignments.Count; ai++)
                        {
                            var assign = PartyBuffAssignments[ai];
                            if (assign == null || string.IsNullOrEmpty(assign.PlayerName)) continue;

                            SRSkill buffSkill = FindBuffSkill(assign.SkillId, assign.SkillName);
                            if (buffSkill == null || !buffSkill.Enabled || !buffSkill.isCastingEnabled) continue;
                            foreach (var targetPlayer in InfoManager.Players.Snapshot())
                            {
                                if (targetPlayer == null || targetPlayer.LifeStateType == SRModel.LifeState.Dead) continue;
                                if (!string.Equals(targetPlayer.Name, assign.PlayerName, StringComparison.OrdinalIgnoreCase)
                                    && !(assign.PlayerName == "*" && BuffAllNearbyPlayers)) continue;
                                if (targetPlayer.Position == null || !TeleportTransitionPolicy.SameSpace(myPos, targetPlayer.Position)
                                    || myPos.DistanceTo(targetPlayer.Position) > buffRange) continue;
                                string key = targetPlayer.UniqueID + ":" + buffSkill.ID;
                                if (recentBuffs.ContainsKey(key)) continue;
                                if (targetPlayer.Buffs != null && targetPlayer.Buffs.Find(b => b != null
                                    && (b.ID == buffSkill.ID || (buffSkill.GroupID != 0 && b.GroupID == buffSkill.GroupID))) != null) continue;
                                pendingSkill = buffSkill;
                                pendingTarget = targetPlayer.UniqueID;
                                pendingBuffKey = key;
                                pendingSleep = Math.Max(400, buffSkill.CastingTime + 150);
                                pendingLog = $"Party Support: Casting [{buffSkill.Name}] on [{targetPlayer.Name}]...";
                                lastSupportActionUtc = DateTime.UtcNow;
                                break;
                            }
                            if (pendingSkill != null) break;
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(supportLock);
            }

            // Lock DIŞINDA cast+sleep: lock içeride Sleep yok, support tepkisi 2sn kitlenmez
            if (pendingSkill != null)
            {
                if (pendingLog != null) Window.Get?.LogProcess(pendingLog);
                Bot.Get.CheckWeaponSwitch(pendingSkill);
                if (!PacketBuilder.CastSkill(pendingSkill.ID, pendingTarget)) return;
                if (pendingBuffKey != null)
                {
                    lock (supportLock) recentBuffs[pendingBuffKey] = DateTime.UtcNow.AddSeconds(5);
                }
                Thread.Sleep(pendingSleep);
                Bot.Get.EnsureMainWeapon();

                // Buffla ve merkeze dön
                if (pendingBuffKey != null && BuffAndReturnToCenter && Window.Get != null
                    && Bot.Get.isBotting && InfoManager.inGame)
                {
                    SRCoord trainPos = Window.Get.TrainingArea_GetPosition();
                    if (trainPos != null)
                    {
                        Bot.Get.ReturnAfterPartyBuff(trainPos);
                    }
                }
            }
        }

        private static SRSkill FindBuffSkill(uint skillId, string skillName)
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null) return null;
            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                var s = InfoManager.Character.Skills.GetAt(i);
                if (s == null) continue;
                if (skillId > 0 && s.ID == skillId) return s;
                if (!string.IsNullOrEmpty(skillName) && (s.Name == skillName || s.ServerName == skillName)) return s;
            }
            return null;
        }

        private static SRSkill FindResurrectSkill(bool preferGroupRess = false)
        {
            if (InfoManager.Character?.Skills == null)
                return null;

            // SBot: Menzilde >1 ölü parti üyesi varsa önce Group Resurrect dene
            if (preferGroupRess)
            {
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    SRSkill sk = InfoManager.Character.Skills.GetAt(i);
                    if (sk != null && sk.Enabled && sk.isCastingEnabled && PartyPolicy.IsGroupRessSkill(sk.ServerName))
                        return sk;
                }
            }

            // Kullanıcı özel skill seçmişse sırasıyla (Primary -> Backup -> Secondary Backup) dene
            if (ResurrectPolicy.SelectedResSkills.Count > 0)
            {
                for (int s = 0; s < ResurrectPolicy.SelectedResSkills.Count; s++)
                {
                    string want = ResurrectPolicy.SelectedResSkills[s];
                    for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                    {
                        var sk = InfoManager.Character.Skills.GetAt(i);
                        if (sk != null && sk.Enabled && sk.isCastingEnabled && (sk.Name == want || sk.ServerName == want))
                            return sk;
                    }
                }
            }

            return FindPartyRessSkill();
        }

        private static SRSkill FindPartyHealSkill()
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return null;

            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                if (skill != null && skill.Enabled && skill.isCastingEnabled && PartyPolicy.IsClericHealingSkill(skill.ServerName))
                {
                    return skill;
                }
            }
            return null;
        }

        private static SRSkill FindPartyRessSkill()
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return null;

            // Önce tekli diriltme, o beklemedeyse grup diriltme (SBot fallback sırası)
            SRSkill groupFallback = null;
            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                if (skill != null && skill.Enabled && skill.isCastingEnabled && PartyPolicy.IsClericRessSkill(skill.ServerName))
                {
                    if (PartyPolicy.IsGroupRessSkill(skill.ServerName))
                    {
                        if (groupFallback == null) groupFallback = skill;
                    }
                    else
                    {
                        return skill;
                    }
                }
            }
            return groupFallback;
        }

        private static SRSkill FindPartyCureSkill()
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return null;

            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                if (skill != null && skill.Enabled && skill.isCastingEnabled && PartyPolicy.IsClericCureSkill(skill.ServerName))
                {
                    return skill;
                }
            }
            return null;
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["PartyHealEnabled"] = PartyHealEnabled;
            json["PartyHealHPPercent"] = PartyHealHPPercent;
            json["PartyAutoRessEnabled"] = PartyAutoRessEnabled;
            json["PartyCureEnabled"] = PartyCureEnabled;
            json["PartyBuffsEnabled"] = PartyBuffsEnabled;

            json["PartyBuffRadius"] = PartyBuffRadius;
            json["BuffAndReturnToCenter"] = BuffAndReturnToCenter;
            json["BuffAllNearbyPlayers"] = BuffAllNearbyPlayers;
            json["BuffProximityStart"] = BuffProximityStart;
            json["BuffProximityStop"] = BuffProximityStop;

            JArray pArr = new JArray();
            lock (BuffProximityPlayers)
            {
                foreach (var p in BuffProximityPlayers) pArr.Add(p);
            }
            json["BuffProximityPlayers"] = pArr;

            JArray assignArr = new JArray();
            lock (PartyBuffAssignments)
            {
                foreach (var a in PartyBuffAssignments)
                {
                    JObject ao = new JObject();
                    ao["PlayerName"] = a.PlayerName ?? "";
                    ao["SkillName"] = a.SkillName ?? "";
                    ao["SkillId"] = a.SkillId;
                    assignArr.Add(ao);
                }
            }
            json["PartyBuffAssignments"] = assignArr;

            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            if (json.ContainsKey("PartyHealEnabled")) PartyHealEnabled = (bool)json["PartyHealEnabled"];
            if (json.ContainsKey("PartyHealHPPercent")) PartyHealHPPercent = (byte)json["PartyHealHPPercent"];
            if (json.ContainsKey("PartyAutoRessEnabled")) PartyAutoRessEnabled = (bool)json["PartyAutoRessEnabled"];
            if (json.ContainsKey("PartyCureEnabled")) PartyCureEnabled = (bool)json["PartyCureEnabled"];
            if (json.ContainsKey("PartyBuffsEnabled")) PartyBuffsEnabled = (bool)json["PartyBuffsEnabled"];

            if (json.ContainsKey("PartyBuffRadius")) PartyBuffRadius = (int)json["PartyBuffRadius"];
            if (json.ContainsKey("BuffAndReturnToCenter")) BuffAndReturnToCenter = (bool)json["BuffAndReturnToCenter"];
            if (json.ContainsKey("BuffAllNearbyPlayers")) BuffAllNearbyPlayers = (bool)json["BuffAllNearbyPlayers"];
            if (json.ContainsKey("BuffProximityStart")) BuffProximityStart = (bool)json["BuffProximityStart"];
            if (json.ContainsKey("BuffProximityStop")) BuffProximityStop = (bool)json["BuffProximityStop"];

            if (json.ContainsKey("BuffProximityPlayers") && json["BuffProximityPlayers"] is JArray pArr)
            {
                lock (BuffProximityPlayers)
                {
                    BuffProximityPlayers.Clear();
                    foreach (var token in pArr) BuffProximityPlayers.Add(token.ToString());
                }
            }

            if (json.ContainsKey("PartyBuffAssignments") && json["PartyBuffAssignments"] is JArray aArr)
            {
                lock (PartyBuffAssignments)
                {
                    PartyBuffAssignments.Clear();
                    foreach (var token in aArr)
                    {
                        if (token is JObject ao)
                        {
                            var entry = new PartyBuffAssignment();
                            if (ao.ContainsKey("PlayerName")) entry.PlayerName = (string)ao["PlayerName"];
                            if (ao.ContainsKey("SkillName")) entry.SkillName = (string)ao["SkillName"];
                            if (ao.ContainsKey("SkillId")) entry.SkillId = (uint)ao["SkillId"];
                            PartyBuffAssignments.Add(entry);
                        }
                    }
                }
            }
        }
    }
}
