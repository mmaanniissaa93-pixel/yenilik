using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    public class MobTargetRule
    {
        public bool Avoid { get; set; }
        public bool Prefer { get; set; }
        public bool Berserk { get; set; }
    }

    public static class CombatAIEngine
    {
        // Berserk Triggers
        public static bool ZerkWhenHPFull { get; set; } = false;
        public static bool ZerkMonsterCountEnabled { get; set; } = true;
        public static int ZerkMonsterCount { get; set; } = 4;
        public static bool ZerkAvoidanceBased { get; set; } = true;
        public static bool ZerkRarityBased { get; set; } = true;

        // Advanced Target Options
        public static bool IgnoreDimensionPillars { get; set; } = true;
        public static bool AttackWeakerFirst { get; set; } = false;
        public static bool DoNotFollowMobs { get; set; } = true;

        // Avoidance & Preference Table per Rarity
        private static readonly Dictionary<SRMob.Mob, MobTargetRule> TargetRules = new Dictionary<SRMob.Mob, MobTargetRule>();

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
                SRMob.Mob.Unique
            };

            foreach (var type in mobTypes)
            {
                if (!TargetRules.ContainsKey(type))
                {
                    TargetRules[type] = new MobTargetRule
                    {
                        Avoid = false,
                        Prefer = (type == SRMob.Mob.Giant || type == SRMob.Mob.PartyGiant || type == SRMob.Mob.Unique || type == SRMob.Mob.Elite),
                        Berserk = (type == SRMob.Mob.Giant || type == SRMob.Mob.PartyGiant || type == SRMob.Mob.Unique)
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
            return GetRule(mobType).Berserk;
        }

        public static bool IsDimensionPillar(SRMob mob)
        {
            if (mob == null || string.IsNullOrEmpty(mob.ServerName))
                return false;

            string name = mob.ServerName.ToUpperInvariant();
            return name.Contains("DIMENSION") || name.Contains("GATE") || name.Contains("ENVY") || name.Contains("PILLAR");
        }

        public static bool CheckBerserkTrigger(List<SRMob> nearbyMobs, double currentHPPercent)
        {
            if (xBot.Game.InfoManager.Character == null)
                return false;

            bool hasMobRuleTrigger = (ZerkAvoidanceBased || ZerkRarityBased)
                && nearbyMobs != null
                && nearbyMobs.Any(m => ShouldZerkOnMob(m.MobType));

            return CombatPolicy.ShouldBerserk(
                xBot.Game.InfoManager.Character.BerserkPoints >= 5,
                xBot.Game.InfoManager.Character.SpeedBerserk > 0,
                ZerkWhenHPFull && currentHPPercent >= 99.0,
                nearbyMobs == null ? 0 : nearbyMobs.Count,
                ZerkMonsterCountEnabled,
                ZerkMonsterCount,
                hasMobRuleTrigger);
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["ZerkWhenHPFull"] = ZerkWhenHPFull;
            json["ZerkMonsterCountEnabled"] = ZerkMonsterCountEnabled;
            json["ZerkMonsterCount"] = ZerkMonsterCount;
            json["ZerkAvoidanceBased"] = ZerkAvoidanceBased;
            json["ZerkRarityBased"] = ZerkRarityBased;
            json["IgnoreDimensionPillars"] = IgnoreDimensionPillars;
            json["AttackWeakerFirst"] = AttackWeakerFirst;
            json["DoNotFollowMobs"] = DoNotFollowMobs;

            JObject rulesJson = new JObject();
            foreach (var kvp in TargetRules)
            {
                JObject r = new JObject();
                r["Avoid"] = kvp.Value.Avoid;
                r["Prefer"] = kvp.Value.Prefer;
                r["Berserk"] = kvp.Value.Berserk;
                rulesJson[kvp.Key.ToString()] = r;
            }
            json["TargetRules"] = rulesJson;

            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("ZerkWhenHPFull")) ZerkWhenHPFull = (bool)json["ZerkWhenHPFull"];
            if (json.ContainsKey("ZerkMonsterCountEnabled")) ZerkMonsterCountEnabled = (bool)json["ZerkMonsterCountEnabled"];
            if (json.ContainsKey("ZerkMonsterCount")) ZerkMonsterCount = (int)json["ZerkMonsterCount"];
            if (json.ContainsKey("ZerkAvoidanceBased")) ZerkAvoidanceBased = (bool)json["ZerkAvoidanceBased"];
            if (json.ContainsKey("ZerkRarityBased")) ZerkRarityBased = (bool)json["ZerkRarityBased"];
            if (json.ContainsKey("IgnoreDimensionPillars")) IgnoreDimensionPillars = (bool)json["IgnoreDimensionPillars"];
            if (json.ContainsKey("AttackWeakerFirst")) AttackWeakerFirst = (bool)json["AttackWeakerFirst"];
            if (json.ContainsKey("DoNotFollowMobs")) DoNotFollowMobs = (bool)json["DoNotFollowMobs"];

            if (json.ContainsKey("TargetRules"))
            {
                JObject rulesJson = (JObject)json["TargetRules"];
                foreach (var prop in rulesJson.Properties())
                {
                    if (Enum.TryParse(prop.Name, out SRMob.Mob type))
                    {
                        JObject r = (JObject)prop.Value;
                        SetRule(type,
                            r.ContainsKey("Avoid") && (bool)r["Avoid"],
                            r.ContainsKey("Prefer") && (bool)r["Prefer"],
                            r.ContainsKey("Berserk") && (bool)r["Berserk"]
                        );
                    }
                }
            }
        }
    }
}
