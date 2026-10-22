using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using xBot.Game.Navigation;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    public enum MonsterPreferenceType
    {
        None,
        Ignore,
        Avoid,
        Prefer
    }

    public class MonsterPreferenceEntry
    {
        public SRMob.Mob? MobType { get; set; }
        public uint MonsterId { get; set; }
        public string Name { get; set; }
        public MonsterPreferenceType Preference { get; set; }
    }

    public class MobTargetRule
    {
        public bool Avoid { get; set; }
        public bool Prefer { get; set; }
        public bool Berserk { get; set; }
    }

    public static class CombatAIEngine
    {
        // Canavar Tercihleri (Boş başlar, kullanıcı sağ tık ile ekler)
        public static List<MonsterPreferenceEntry> MonsterPreferences { get; } = new List<MonsterPreferenceEntry>();
        public static bool SwitchTargetByPosition { get; set; } = false;
        public static bool AllowAttackingQuestMobs { get; set; } = false;

        // Berserk Triggers
        public static bool ZerkWhenFull { get; set; } = false; // "Doldukça"
        public static bool ZerkEvenIfNotAttacking { get; set; } = false; // "Canavar saldırmasa bile"
        public static bool ZerkMonsterCountEnabled { get; set; } = false;
        public static int ZerkMonsterCount { get; set; } = 3;
        public static HashSet<SRMob.Mob> ZerkMobTypes { get; } = new HashSet<SRMob.Mob>();
        public static bool ZerkPillars { get; set; } = false;
        public static bool ZerkInScript { get; set; } = false;
        public static bool IsBerserkEnabled => ZerkWhenFull || ZerkMonsterCountEnabled || (ZerkMobTypes != null && ZerkMobTypes.Count > 0) || ZerkPillars || ZerkInScript;

        // Eski uyumluluk bayrakları
        public static bool UseZerkPotion { get; set; } = false;
        public static bool UseEnergyOfLife { get; set; } = false;

        // Advanced Target Options
        public static bool IgnoreDimensionPillars { get; set; } = true;
        public static bool AttackWeakerFirst { get; set; } = false;
        public static bool DoNotFollowMobs { get; set; } = true;

        // PhBot Attack Options (Saldırı Sekmesi Seçenekleri)
        public static bool KillSteal { get; set; } = true;
        public static bool ProtectParty { get; set; } = false;
        public static string ProtectPartyTarget { get; set; } = "";
        public static bool AttackLowerFirst { get; set; } = true;
        public static bool SwitchMonsterAfterDot { get; set; } = false;
        public static int SwitchMonsterDotDelay { get; set; } = 0;
        public static bool UseLowerSkills { get; set; } = true;
        public static bool SlowerAttackMode { get; set; } = false;
        public static bool Lagtastic { get; set; } = false;
        public static bool AutoSelectUniques { get; set; } = false;
        public static bool AutoSelectTitans { get; set; } = false;
        public static bool UseTeleportSkills { get; set; } = false;

        // SBot Combat & Targeting Options
        public static bool StickToMob { get; set; } = false; // extra:sticktomob (Stay at same mob until it's dead)
        public static bool SameMob { get; set; } = false; // partyserk:samemob (Prefer mobs that get attacked by player [Name])
        public static string SameMobCharName { get; set; } = ""; // partyserk:samemob_charname
        public static bool DefendPet { get; set; } = false; // extra:defendpet (Prioritize mobs attacking attack pet, pick pet, or transport)
        public static bool SkillIterate { get => SkillManager.InOrderCombo; set => SkillManager.InOrderCombo = value; } // skills:skilliterate
        public static bool SkillIterateMob { get; set; } = true; // skills:skilliteratemob (Reset skill iteration for every new mob)
        public static bool NoAttack { get => SkillManager.NoAttackMode; set => SkillManager.NoAttackMode = value; } // skills:noattack

        // SBot Emergency Situation (emerg:*)
        public static bool EmergencyAttEnabled { get; set; } = false;
        public static int EmergencyAttType { get; set; } = 0; // 0: current mobs attacking >= X, 1: mobs attacked in timeframe >= X
        public static int EmergencyMobCount { get; set; } = 3;
        public static bool EmergencyHpEnabled { get; set; } = false;
        public static int EmergencyHpPercent { get; set; } = 50;
        public static bool EmergencyCastWhileWalking { get; set; } = false; // emerg_castwalking
        public static List<string> EmergencyBuffSkills { get; } = new List<string>();
        public static CombatAttackingTracker AttackingTracker { get; } = new CombatAttackingTracker();

        // SBot Died Stop (extra:diedstop, extra:diedstopval)
        public static bool DiedStop { get; set; } = false;
        public static int DiedStopCount { get; set; } = 3;
        public static int SessionDiedCount { get; set; } = 0;

        public static void ResetSessionState()
        {
            SessionDiedCount = 0;
            AttackingTracker.Clear();
        }

        // Avoidance & Preference Table per Rarity (thread-safe)
        private static readonly ConcurrentDictionary<SRMob.Mob, MobTargetRule> TargetRules = new ConcurrentDictionary<SRMob.Mob, MobTargetRule>();

        static CombatAIEngine()
        {
            InitializeDefaultRules();
        }

        public static void InitializeDefaultRules()
        {
            var mobTypes = new[]
            {
                SRMob.Mob.General,
                SRMob.Mob.Champion,
                SRMob.Mob.Giant,
                SRMob.Mob.PartyGeneral,
                SRMob.Mob.PartyChampion,
                SRMob.Mob.PartyGiant,
                SRMob.Mob.Elite,
                SRMob.Mob.Unique,
                SRMob.Mob.Event
            };

            foreach (var type in mobTypes)
            {
                if (!TargetRules.ContainsKey(type))
                {
                    TargetRules[type] = new MobTargetRule
                    {
                        Avoid = false,
                        Prefer = false,
                        Berserk = false
                    };
                }
            }
        }

        public static MobTargetRule GetRule(SRMob.Mob mobType)
        {
            if (TargetRules.TryGetValue(mobType, out var rule))
                return rule;

            var newRule = new MobTargetRule();
            TargetRules[mobType] = newRule;
            return newRule;
        }

        public static void SetRule(SRMob.Mob mobType, bool avoid, bool prefer, bool berserk)
        {
            var rule = GetRule(mobType);
            rule.Avoid = avoid;
            rule.Prefer = prefer;
            rule.Berserk = berserk;
        }

        public static bool ShouldAvoid(SRMob.Mob mobType)
        {
            return GetRule(mobType).Avoid;
        }

        public static bool IsPreferred(SRMob.Mob mobType)
        {
            return GetRule(mobType).Prefer;
        }

        public static bool ShouldZerkOnMob(SRMob.Mob mobType)
        {
            return ZerkMobTypes.Contains(mobType) || GetRule(mobType).Berserk;
        }

        public static bool IsDimensionPillar(SRMob mob)
        {
            if (mob == null || string.IsNullOrEmpty(mob.ServerName))
                return false;

            string name = mob.ServerName.ToUpperInvariant();
            return name.Contains("DIMENSION_PILLAR") || name.Contains("DIMENSIONPILLAR") || name.Contains("PILLAR");
        }

        public static MonsterPreferenceEntry FindPreference(SRMob mob)
        {
            if (mob == null) return null;
            lock (MonsterPreferences)
            {
                if (MonsterPreferences.Count == 0) return null;
                for (int i = 0; i < MonsterPreferences.Count; i++)
                {
                    var entry = MonsterPreferences[i];
                    if (entry.MonsterId > 0 && entry.MonsterId == mob.ID)
                        return entry;
                    if (entry.MobType.HasValue && entry.MobType.Value == mob.MobType)
                        return entry;
                }
            }
            return null;
        }

        public static int GetPreferenceIndex(SRMob mob)
        {
            if (mob == null) return -1;
            lock (MonsterPreferences)
            {
                if (MonsterPreferences.Count == 0) return -1;
                for (int i = 0; i < MonsterPreferences.Count; i++)
                {
                    var entry = MonsterPreferences[i];
                    if (entry.MonsterId > 0 && entry.MonsterId == mob.ID)
                        return i;
                    if (entry.MobType.HasValue && entry.MobType.Value == mob.MobType)
                        return i;
                }
            }
            return -1;
        }

        public static bool CheckBerserkTrigger(List<SRMob> nearbyMobs, double currentHPPercent)
        {
            if (xBot.Game.InfoManager.Character == null)
                return false;

            // Berserk barı 5 puan (tam dolu) olmalı
            if (xBot.Game.InfoManager.Character.BerserkPoints < 5)
                return false;

            // Zaten Berserk modundaysa tetikleme
            if (xBot.Game.InfoManager.Character.GameStateType == SRModel.GameState.Berserk)
                return false;

            // 1) "Doldukça" (ZerkWhenFull): Berserk barı dolar dolmaz tetikle!
            if (ZerkWhenFull)
            {
                if (ZerkEvenIfNotAttacking)
                    return true;
                if (nearbyMobs != null && nearbyMobs.Count > 0)
                    return true;
            }

            if (nearbyMobs == null || nearbyMobs.Count == 0)
                return false;

            // 2) "Saldıran canavar sayısı >" X (ZerkMonsterCountEnabled)
            uint myId = xBot.Game.InfoManager.Character.UniqueID;
            var myPos = xBot.Game.InfoManager.Character.GetRealtimePosition();
            if (ZerkMonsterCountEnabled)
            {
                int count = 0;
                for (int i = 0; i < nearbyMobs.Count; i++)
                {
                    var m = nearbyMobs[i];
                    if (m == null) continue;
                    bool isAttackingMe = (myId != 0 && m.TargetUniqueID == myId)
                        || (myPos != null && m.Position != null && TeleportTransitionPolicy.SameSpace(myPos, m.GetRealtimePosition()) && myPos.DistanceTo(m.GetRealtimePosition()) <= 4.5);
                    if (ZerkEvenIfNotAttacking || isAttackingMe)
                        count++;
                }
                if (count > ZerkMonsterCount)
                    return true;
            }

            // 3) Canavar türü kontrolleri (Giant, Genel Parti, Şampiyon Parti, Giant Parti, Titan, Elit, Güçlü, Etkinlik, Unique, Pillar)
            for (int i = 0; i < nearbyMobs.Count; i++)
            {
                var m = nearbyMobs[i];
                if (m == null) continue;

                bool typeMatch = ZerkMobTypes.Contains(m.MobType) || GetRule(m.MobType).Berserk;
                if (!typeMatch && ZerkPillars && IsDimensionPillar(m))
                    typeMatch = true;

                if (typeMatch)
                {
                    bool isAttackingMe = (myId != 0 && m.TargetUniqueID == myId)
                        || (myPos != null && m.Position != null && TeleportTransitionPolicy.SameSpace(myPos, m.GetRealtimePosition()) && myPos.DistanceTo(m.GetRealtimePosition()) <= 15.0);
                    if (ZerkEvenIfNotAttacking || isAttackingMe)
                        return true;
                }
            }

            return false;
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["ZerkWhenFull"] = ZerkWhenFull;
            json["ZerkEvenIfNotAttacking"] = ZerkEvenIfNotAttacking;
            json["ZerkMonsterCountEnabled"] = ZerkMonsterCountEnabled;
            json["ZerkMonsterCount"] = ZerkMonsterCount;
            json["ZerkInScript"] = ZerkInScript;
            json["ZerkPillars"] = ZerkPillars;
            json["SwitchTargetByPosition"] = SwitchTargetByPosition;
            json["AllowAttackingQuestMobs"] = AllowAttackingQuestMobs;
            json["IgnoreDimensionPillars"] = IgnoreDimensionPillars;
            json["AttackWeakerFirst"] = AttackWeakerFirst;
            json["DoNotFollowMobs"] = DoNotFollowMobs;
            json["KillSteal"] = KillSteal;
            json["ProtectParty"] = ProtectParty;
            json["ProtectPartyTarget"] = ProtectPartyTarget ?? "";
            json["AttackLowerFirst"] = AttackLowerFirst;
            json["SwitchMonsterAfterDot"] = SwitchMonsterAfterDot;
            json["SwitchMonsterDotDelay"] = SwitchMonsterDotDelay;
            json["UseLowerSkills"] = UseLowerSkills;
            json["SlowerAttackMode"] = SlowerAttackMode;
            json["Lagtastic"] = Lagtastic;
            json["AutoSelectUniques"] = AutoSelectUniques;
            json["AutoSelectTitans"] = AutoSelectTitans;
            json["UseTeleportSkills"] = UseTeleportSkills;

            // SBot options
            json["StickToMob"] = StickToMob;
            json["SameMob"] = SameMob;
            json["SameMobCharName"] = SameMobCharName ?? "";
            json["DefendPet"] = DefendPet;
            json["SkillIterateMob"] = SkillIterateMob;
            json["EmergencyAttEnabled"] = EmergencyAttEnabled;
            json["EmergencyAttType"] = EmergencyAttType;
            json["EmergencyMobCount"] = EmergencyMobCount;
            json["EmergencyHpEnabled"] = EmergencyHpEnabled;
            json["EmergencyHpPercent"] = EmergencyHpPercent;
            json["EmergencyCastWhileWalking"] = EmergencyCastWhileWalking;
            json["DiedStop"] = DiedStop;
            json["DiedStopCount"] = DiedStopCount;

            JArray emergBuffs = new JArray();
            lock (EmergencyBuffSkills)
            {
                foreach (var b in EmergencyBuffSkills) emergBuffs.Add(b);
            }
            json["EmergencyBuffSkills"] = emergBuffs;

            JObject rulesObj = new JObject();
            foreach (var kvp in TargetRules)
            {
                JObject ro = new JObject();
                ro["Avoid"] = kvp.Value.Avoid;
                ro["Prefer"] = kvp.Value.Prefer;
                ro["Berserk"] = kvp.Value.Berserk;
                rulesObj[kvp.Key.ToString()] = ro;
            }
            json["TargetRules"] = rulesObj;

            JArray zerkMobs = new JArray();
            foreach (var t in ZerkMobTypes) zerkMobs.Add(t.ToString());
            json["ZerkMobTypes"] = zerkMobs;

            JArray prefsArray = new JArray();
            lock (MonsterPreferences)
            {
                foreach (var p in MonsterPreferences)
                {
                    JObject po = new JObject();
                    po["MobType"] = p.MobType.HasValue ? p.MobType.Value.ToString() : null;
                    po["MonsterId"] = p.MonsterId;
                    po["Name"] = p.Name ?? "";
                    po["Preference"] = p.Preference.ToString();
                    prefsArray.Add(po);
                }
            }
            json["MonsterPreferences"] = prefsArray;

            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("ZerkWhenFull")) ZerkWhenFull = (bool)json["ZerkWhenFull"];
            if (json.ContainsKey("ZerkEvenIfNotAttacking")) ZerkEvenIfNotAttacking = (bool)json["ZerkEvenIfNotAttacking"];
            if (json.ContainsKey("ZerkMonsterCountEnabled")) ZerkMonsterCountEnabled = (bool)json["ZerkMonsterCountEnabled"];
            if (json.ContainsKey("ZerkMonsterCount")) ZerkMonsterCount = (int)json["ZerkMonsterCount"];
            if (json.ContainsKey("ZerkInScript")) ZerkInScript = (bool)json["ZerkInScript"];
            if (json.ContainsKey("ZerkPillars")) ZerkPillars = (bool)json["ZerkPillars"];
            if (json.ContainsKey("SwitchTargetByPosition")) SwitchTargetByPosition = (bool)json["SwitchTargetByPosition"];
            if (json.ContainsKey("AllowAttackingQuestMobs")) AllowAttackingQuestMobs = (bool)json["AllowAttackingQuestMobs"];
            if (json.ContainsKey("IgnoreDimensionPillars")) IgnoreDimensionPillars = (bool)json["IgnoreDimensionPillars"];
            if (json.ContainsKey("AttackWeakerFirst")) AttackWeakerFirst = (bool)json["AttackWeakerFirst"];
            if (json.ContainsKey("DoNotFollowMobs")) DoNotFollowMobs = (bool)json["DoNotFollowMobs"];
            if (json.ContainsKey("KillSteal")) KillSteal = (bool)json["KillSteal"];
            if (json.ContainsKey("ProtectParty")) ProtectParty = (bool)json["ProtectParty"];
            if (json.ContainsKey("ProtectPartyTarget")) ProtectPartyTarget = (string)json["ProtectPartyTarget"];
            if (json.ContainsKey("AttackLowerFirst")) AttackLowerFirst = (bool)json["AttackLowerFirst"];
            if (json.ContainsKey("SwitchMonsterAfterDot")) SwitchMonsterAfterDot = (bool)json["SwitchMonsterAfterDot"];
            if (json.ContainsKey("SwitchMonsterDotDelay")) SwitchMonsterDotDelay = (int)json["SwitchMonsterDotDelay"];
            if (json.ContainsKey("UseLowerSkills")) UseLowerSkills = (bool)json["UseLowerSkills"];
            if (json.ContainsKey("SlowerAttackMode")) SlowerAttackMode = (bool)json["SlowerAttackMode"];
            if (json.ContainsKey("Lagtastic")) Lagtastic = (bool)json["Lagtastic"];
            if (json.ContainsKey("AutoSelectUniques")) AutoSelectUniques = (bool)json["AutoSelectUniques"];
            if (json.ContainsKey("AutoSelectTitans")) AutoSelectTitans = (bool)json["AutoSelectTitans"];
            if (json.ContainsKey("UseTeleportSkills")) UseTeleportSkills = (bool)json["UseTeleportSkills"];

            // SBot options
            if (json.ContainsKey("StickToMob")) StickToMob = (bool)json["StickToMob"];
            if (json.ContainsKey("SameMob")) SameMob = (bool)json["SameMob"];
            if (json.ContainsKey("SameMobCharName")) SameMobCharName = (string)json["SameMobCharName"];
            if (json.ContainsKey("DefendPet")) DefendPet = (bool)json["DefendPet"];
            if (json.ContainsKey("SkillIterateMob")) SkillIterateMob = (bool)json["SkillIterateMob"];
            if (json.ContainsKey("EmergencyAttEnabled")) EmergencyAttEnabled = (bool)json["EmergencyAttEnabled"];
            if (json.ContainsKey("EmergencyAttType")) EmergencyAttType = (int)json["EmergencyAttType"];
            if (json.ContainsKey("EmergencyMobCount")) EmergencyMobCount = (int)json["EmergencyMobCount"];
            if (json.ContainsKey("EmergencyHpEnabled")) EmergencyHpEnabled = (bool)json["EmergencyHpEnabled"];
            if (json.ContainsKey("EmergencyHpPercent")) EmergencyHpPercent = (int)json["EmergencyHpPercent"];
            if (json.ContainsKey("EmergencyCastWhileWalking")) EmergencyCastWhileWalking = (bool)json["EmergencyCastWhileWalking"];
            if (json.ContainsKey("DiedStop")) DiedStop = (bool)json["DiedStop"];
            if (json.ContainsKey("DiedStopCount")) DiedStopCount = (int)json["DiedStopCount"];

            if (json.ContainsKey("EmergencyBuffSkills") && json["EmergencyBuffSkills"] is JArray ebArr)
            {
                lock (EmergencyBuffSkills)
                {
                    EmergencyBuffSkills.Clear();
                    foreach (var token in ebArr) EmergencyBuffSkills.Add(token.ToString());
                }
            }

            if (json.ContainsKey("TargetRules") && json["TargetRules"] is JObject rObj)
            {
                foreach (var prop in rObj.Properties())
                {
                    if (Enum.TryParse<SRMob.Mob>(prop.Name, out var mt) && prop.Value is JObject ro)
                    {
                        var rule = GetRule(mt);
                        if (ro.ContainsKey("Avoid")) rule.Avoid = (bool)ro["Avoid"];
                        if (ro.ContainsKey("Prefer")) rule.Prefer = (bool)ro["Prefer"];
                        if (ro.ContainsKey("Berserk")) rule.Berserk = (bool)ro["Berserk"];
                    }
                }
            }

            if (json.ContainsKey("ZerkMobTypes") && json["ZerkMobTypes"] is JArray arr)
            {
                ZerkMobTypes.Clear();
                foreach (var token in arr)
                {
                    if (Enum.TryParse<SRMob.Mob>(token.ToString(), out var mt))
                        ZerkMobTypes.Add(mt);
                }
            }

            if (json.ContainsKey("MonsterPreferences") && json["MonsterPreferences"] is JArray pArr)
            {
                lock (MonsterPreferences)
                {
                    MonsterPreferences.Clear();
                    foreach (var token in pArr)
                    {
                        if (token is JObject po)
                        {
                            var entry = new MonsterPreferenceEntry();
                            if (po.ContainsKey("MobType") && po["MobType"]?.Type == JTokenType.String &&
                                Enum.TryParse<SRMob.Mob>(po["MobType"].ToString(), out var mt))
                            {
                                entry.MobType = mt;
                            }
                            if (po.ContainsKey("MonsterId")) entry.MonsterId = (uint)po["MonsterId"];
                            if (po.ContainsKey("Name")) entry.Name = (string)po["Name"];
                            if (po.ContainsKey("Preference") && Enum.TryParse<MonsterPreferenceType>(po["Preference"].ToString(), out var pr))
                            {
                                entry.Preference = pr;
                            }
                            MonsterPreferences.Add(entry);
                        }
                    }
                }
            }
        }
    }
}
