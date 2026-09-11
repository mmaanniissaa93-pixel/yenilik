using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace xBot.App
{
    public enum EzFilterCategory
    {
        Armor,
        Weapons,
        HP,
        MP,
        Vigor,
        Universal,
        Purification,
        Elixirs,
        Materials,
        Elements,
        Tablets,
        Stones,
        Quest
    }

    public class EzCategoryRule
    {
        public bool Pick { get; set; } = false;
        public bool PetPick { get; set; } = false;
        public bool Sell { get; set; } = false;
        public bool Store { get; set; } = false;
        public bool GuildStore { get; set; } = false;
    }

    public static class EzFilterManager
    {
        private static readonly ConcurrentDictionary<EzFilterCategory, EzCategoryRule> Rules =
            new ConcurrentDictionary<EzFilterCategory, EzCategoryRule>();

        static EzFilterManager()
        {
            Reset();
        }

        public static void Reset()
        {
            foreach (EzFilterCategory cat in Enum.GetValues(typeof(EzFilterCategory)))
            {
                Rules[cat] = new EzCategoryRule();
            }
        }

        public static EzCategoryRule GetRule(EzFilterCategory category)
        {
            if (Rules.TryGetValue(category, out var rule))
                return rule;

            var newRule = new EzCategoryRule();
            Rules[category] = newRule;
            return newRule;
        }

        public static void SetRule(EzFilterCategory category, bool pick, bool petPick, bool sell, bool store, bool guildStore)
        {
            var rule = GetRule(category);
            rule.Pick = pick;
            rule.PetPick = petPick;
            rule.Sell = sell;
            rule.Store = store;
            rule.GuildStore = guildStore;
        }

        public static bool HasAnyRule()
        {
            foreach (var kvp in Rules)
            {
                var r = kvp.Value;
                if (r != null && (r.Pick || r.PetPick || r.Sell || r.Store || r.GuildStore))
                    return true;
            }
            return false;
        }

        public static EzFilterCategory? Classify(byte id2, byte id3, byte id4, string serverName = "")
        {
            string sn = serverName ?? "";

            // Armor: tid2=1 AND tid3 IN (1,2,3,9,10,11)
            if (id2 == 1 && (id3 == 1 || id3 == 2 || id3 == 3 || id3 == 9 || id3 == 10 || id3 == 11))
                return EzFilterCategory.Armor;

            // Silahlar: tid2=1 AND tid3 IN (4, 6) (Weapons and Shields)
            if (id2 == 1 && (id3 == 4 || id3 == 6))
                return EzFilterCategory.Weapons;

            // HP Potions: tid2=3 AND tid3=1 AND (tid4=1 or serverName contains HP_POTION or HP_SPOTION)
            if (id2 == 3 && id3 == 1 && (id4 == 1 || sn.IndexOf("_HP_", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.HP;

            // MP Potions: tid2=3 AND tid3=1 AND (tid4=2 or serverName contains MP_POTION or MP_SPOTION)
            if (id2 == 3 && id3 == 1 && (id4 == 2 || sn.IndexOf("_MP_", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.MP;

            // Vigorlar: tid2=3 AND tid3=1 AND (tid4=3 or serverName contains ALL_POTION or ALL_SPOTION)
            if (id2 == 3 && id3 == 1 && (id4 == 3 || sn.IndexOf("_ALL_", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.Vigor;

            // Universal Pills: tid2=3 AND tid3=2 AND (tid4=6 or serverName contains CURE_ALL)
            if (id2 == 3 && id3 == 2 && (id4 == 6 || sn.IndexOf("CURE_ALL", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.Universal;

            // Purification Pills: tid2=3 AND tid3=2 AND (tid4=1 or serverName contains CURE_RANDOM)
            if (id2 == 3 && id3 == 2 && (id4 == 1 || sn.IndexOf("CURE_RANDOM", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.Purification;

            // Elixirler: tid2=3 AND tid3=10
            if (id2 == 3 && id3 == 10)
                return EzFilterCategory.Elixirs;

            // Quest: tid2=3 AND tid3=9 (or servername contains ITEM_QNO or QITEM)
            if ((id2 == 3 && id3 == 9) || sn.IndexOf("ITEM_QNO", StringComparison.OrdinalIgnoreCase) >= 0 || sn.IndexOf("QITEM", StringComparison.OrdinalIgnoreCase) >= 0)
                return EzFilterCategory.Quest;

            // Elements: tid2=3 AND tid3=11 AND tid4=5 (or servername contains ELEMENT)
            if (id2 == 3 && id3 == 11 && (id4 == 5 || sn.IndexOf("ELEMENT", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.Elements;

            // Tablets: tid2=3 AND tid3=11 AND tid4=3 (or servername contains TABLET)
            if (id2 == 3 && id3 == 11 && (id4 == 3 || sn.IndexOf("TABLET", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.Tablets;

            // Stones: tid2=3 AND tid3=11 AND tid4 IN (1, 2, 7, 8, 10) (or servername contains STONE)
            if (id2 == 3 && id3 == 11 && (id4 == 1 || id4 == 2 || id4 == 7 || id4 == 8 || id4 == 10 || sn.IndexOf("STONE", StringComparison.OrdinalIgnoreCase) >= 0))
                return EzFilterCategory.Stones;

            // Materials: tid2=3 AND tid3=11 (materials/drops)
            if (id2 == 3 && id3 == 11)
                return EzFilterCategory.Materials;

            return null;
        }

        public static JObject ToJson()
        {
            var json = new JObject();
            foreach (var kvp in Rules)
            {
                var r = kvp.Value;
                if (r == null) continue;
                var obj = new JObject
                {
                    ["Pick"] = r.Pick,
                    ["PetPick"] = r.PetPick,
                    ["Sell"] = r.Sell,
                    ["Store"] = r.Store,
                    ["GuildStore"] = r.GuildStore
                };
                json[kvp.Key.ToString()] = obj;
            }
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            Reset();
            foreach (EzFilterCategory cat in Enum.GetValues(typeof(EzFilterCategory)))
            {
                string key = cat.ToString();
                if (json[key] is JObject obj)
                {
                    var rule = GetRule(cat);
                    if (obj["Pick"] != null) rule.Pick = (bool)obj["Pick"];
                    if (obj["PetPick"] != null) rule.PetPick = (bool)obj["PetPick"];
                    if (obj["Sell"] != null) rule.Sell = (bool)obj["Sell"];
                    if (obj["Store"] != null) rule.Store = (bool)obj["Store"];
                    if (obj["GuildStore"] != null) rule.GuildStore = (bool)obj["GuildStore"];
                }
            }
        }
    }
}