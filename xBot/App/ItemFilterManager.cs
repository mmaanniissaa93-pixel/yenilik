using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Item;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    public static class ItemFilterManager
    {
        // Global Filter Criteria
        public static int MinDegree { get; set; } = 1;
        public static int MaxDegree { get; set; } = 15;
        public static bool OnlySox { get; set; } = false;
        public static bool FilterChina { get; set; } = true;
        public static bool FilterEurope { get; set; } = true;
        public static bool FilterMale { get; set; } = true;
        public static bool FilterFemale { get; set; } = true;

        // phBot tarzı sekmeler: Options / Store Gold / Dismantle + Blues listesi
        public static PickFilterOptions Pick { get; set; } = new PickFilterOptions();
        public static StoreGoldOptions StoreGold { get; set; } = new StoreGoldOptions();
        public static DismantleOptions Dismantle { get; set; } = new DismantleOptions();
        private static readonly ConcurrentDictionary<string, BlueAttributeRule> Blues =
            new ConcurrentDictionary<string, BlueAttributeRule>(System.StringComparer.OrdinalIgnoreCase);

        // In-memory Rules Map (O(1) lookup by item ServerName or Name) - thread-safe
        private static readonly ConcurrentDictionary<string, ItemFilterRule> Rules = new ConcurrentDictionary<string, ItemFilterRule>(StringComparer.OrdinalIgnoreCase);

        public static void SetRule(string itemName, bool pickup, bool sell, bool store)
        {
            SetRuleFull(itemName, pickup, false, sell, store, false);
        }

        public static void SetRuleFull(string itemName, bool pickup, bool pet, bool sell, bool store, bool storeGuild)
        {
            if (string.IsNullOrEmpty(itemName))
                return;

            Rules[itemName] = new ItemFilterRule
            {
                ItemName = itemName,
                Pickup = pickup,
                Pet = pet,
                Sell = sell,
                Store = store,
                StoreGuild = storeGuild,
                MatchType = ItemFilterMatchType.Exact,
                Pattern = ""
            };
        }

        public static void SetRuleWithPattern(string itemName, bool pickup, bool sell, bool store, ItemFilterMatchType matchType, string pattern = "", bool pet = false, bool storeGuild = false)
        {
            if (string.IsNullOrEmpty(itemName))
                return;

            Rules[itemName] = new ItemFilterRule
            {
                ItemName = itemName,
                Pickup = pickup,
                Pet = pet,
                Sell = sell,
                Store = store,
                StoreGuild = storeGuild,
                MatchType = matchType,
                Pattern = pattern ?? ""
            };
        }

        public static ItemFilterRule GetRule(string itemName)
        {
            if (!string.IsNullOrEmpty(itemName) && Rules.TryGetValue(itemName, out var rule))
                return rule;
            return null;
        }

        public static void ClearRules()
        {
            Rules.Clear();
        }

        public static void RemoveRule(string itemName)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                ItemFilterRule dummy;
                Rules.TryRemove(itemName, out dummy);
            }
        }

        public static IEnumerable<ItemFilterRule> GetAllRules()
        {
            return Rules.Values;
        }

        public static bool ShouldPickup(SRItem item)
        {
            if (item == null)
                return false;

            var equip = item as SREquipable;
            return ItemFilterPolicy.ShouldPickup(
                CreateInput(item, equip != null),
                GetOptions(),
                FindRule(item.Name, item.ServerName),
                Pick);
        }

        /// <summary>
        /// Overload for ground drops (SRDrop). Evaluates degree and rarity filters.
        /// </summary>
        public static bool ShouldPickup(SRDrop drop)
        {
            if (drop == null)
                return false;

            return ItemFilterPolicy.ShouldPickup(
                CreateInput(drop),
                GetOptions(),
                FindRule(drop.Name, drop.ServerName),
                Pick);
        }

        public static bool ShouldSell(SRItem item)
        {
            if (item == null)
                return false;

            return ItemFilterPolicy.ShouldSell(
                CreateInput(item, item is SREquipable),
                FindRule(item.Name, item.ServerName),
                Pick);
        }

        public static bool ShouldStore(SRItem item)
        {
            if (item == null)
                return false;

            return ItemFilterPolicy.ShouldStore(
                CreateInput(item, item is SREquipable),
                FindRule(item.Name, item.ServerName),
                Pick,
                Blues);
        }

        public static bool ShouldStoreGuild(SRItem item)
        {
            if (item == null)
                return false;

            return ItemFilterPolicy.ShouldStoreGuild(
                CreateInput(item, item is SREquipable),
                FindRule(item.Name, item.ServerName));
        }

        public static bool ShouldUsePet(SRDrop drop, bool petAvailable, bool petFull)
        {
            if (drop == null)
                return false;

            return ItemFilterPolicy.ShouldUsePet(
                CreateInput(drop),
                FindRule(drop.Name, drop.ServerName),
                Pick,
                petAvailable,
                petFull);
        }

        public static bool ShouldDismantle(SRItem item)
        {
            if (item == null)
                return false;

            return ItemFilterPolicy.ShouldDismantle(
                CreateInput(item, item is SREquipable),
                Dismantle);
        }

        public static int GetOwnerKind(SRDrop drop)
        {
            if (drop == null || !drop.hasOwner)
                return 0;
            try
            {
                string myName = InfoManager.Character != null ? InfoManager.Character.Name : "";
                if (!string.IsNullOrEmpty(myName) && !string.IsNullOrEmpty(drop.OwnerName)
                    && string.Equals(myName, drop.OwnerName, System.StringComparison.OrdinalIgnoreCase))
                    return 1;
                // Parti üyesi mi?
                try
                {
                    var party = InfoManager.Party;
                    if (party != null && party.Members != null)
                    {
                        foreach (var m in party.Members.Snapshot())
                        {
                            if (m != null && !string.IsNullOrEmpty(m.Name)
                                && string.Equals(m.Name, drop.OwnerName, System.StringComparison.OrdinalIgnoreCase))
                                return 2;
                        }
                    }
                }
                catch { }
                return 3;
            }
            catch { return 0; }
        }

        public static IDictionary<string, BlueAttributeRule> GetBlues()
        {
            return Blues;
        }

        /// <summary>
        /// MATTR_* kodlarının görünen adları (yeşil taş adları). DB'de name çoğu
        /// boş olduğu için liste ham kodla doluyordu.
        /// </summary>
        private static readonly Dictionary<string, string> BlueDisplayNames =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "MATTR_APE", "Ape" },
            { "MATTR_ASTRAL", "Astral" },
            { "MATTR_ATHANASIA", "Immortal" },
            { "MATTR_BLOCKRATE", "Block" },
            { "MATTR_CRITICAL", "Critical" },
            { "MATTR_DEC_MAXDUR", "Durability Decrease" },
            { "MATTR_DUR", "Durability Increase" },
            { "MATTR_DUR_SET", "Durability Increase (Set)" },
            { "MATTR_ER", "Evade Rate" },
            { "MATTR_ER_SET", "Evade Rate (Set)" },
            { "MATTR_EVADE_BLOCK", "Evade Block" },
            { "MATTR_EVADE_CRITICAL", "Evade Critical" },
            { "MATTR_HP", "HP" },
            { "MATTR_HP_SET", "HP (Set)" },
            { "MATTR_HR", "Attack Rate" },
            { "MATTR_INT", "Int" },
            { "MATTR_INT_3JOB", "Int (3-Job)" },
            { "MATTR_INT_AVATAR", "Int (Avatar)" },
            { "MATTR_INT_SET", "Int (Set)" },
            { "MATTR_LUCK", "Lucky" },
            { "MATTR_LUCK_SET", "Lucky (Set)" },
            { "MATTR_MP", "MP" },
            { "MATTR_MP_SET", "MP (Set)" },
            { "MATTR_NASRUN_BLOCKRATE", "Block (Devil Spirit)" },
            { "MATTR_NASRUN_HPNA", "HP (Devil Spirit)" },
            { "MATTR_NASRUN_MPNA", "MP (Devil Spirit)" },
            { "MATTR_NASRUN_UMDU", "Umdu (Devil Spirit)" },
            { "MATTR_NOT_REPARABLE", "Not Reparable" },
            { "MATTR_REGENHPMP", "HP/MP Regen" },
            { "MATTR_REINFORCE_ITEM", "Reinforcement" },
            { "MATTR_REINFORCE_ITEM_SET", "Reinforcement (Set)" },
            { "MATTR_REPAIR", "Repair" },
            { "MATTR_RESIST_ALL_SET", "All Resist (Set)" },
            { "MATTR_RESIST_BURN", "Burn Resist" },
            { "MATTR_RESIST_CSMP", "Curse Resist" },
            { "MATTR_RESIST_DISEASE", "Disease Resist" },
            { "MATTR_RESIST_ESHOCK", "Electric Shock Resist" },
            { "MATTR_RESIST_FEAR", "Fear Resist" },
            { "MATTR_RESIST_FROSTBITE", "Freeze Resist" },
            { "MATTR_RESIST_POISON", "Poison Resist" },
            { "MATTR_RESIST_SLEEP", "Sleep Resist" },
            { "MATTR_RESIST_STUN", "Stun Resist" },
            { "MATTR_RESIST_ZOMBIE", "Zombie Resist" },
            { "MATTR_SOLID", "Steady" },
            { "MATTR_STR", "Str" },
            { "MATTR_STR_3JOB", "Str (3-Job)" },
            { "MATTR_STR_AVATAR", "Str (Avatar)" },
            { "MATTR_STR_SET", "Str (Set)" },
        };

        public static string GetBlueDisplayName(string serverName, string dbName)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                string friendly;
                if (BlueDisplayNames.TryGetValue(serverName, out friendly) && !string.IsNullOrEmpty(friendly))
                    return friendly;
                // Avatar/taş türevleri: MATTR_AVATAR_STR_2 -> "Str (Avatar 2)" gibi sadeleştir.
                string s = serverName;
                if (s.StartsWith("MATTR_AVATAR_", System.StringComparison.OrdinalIgnoreCase))
                {
                    string rest = s.Substring("MATTR_AVATAR_".Length).Replace("_", " ").Trim();
                    if (rest.Length > 0)
                        return ToTitle(rest) + " (Avatar)";
                }
                if (s.StartsWith("MATTR_", System.StringComparison.OrdinalIgnoreCase))
                {
                    string rest = s.Substring("MATTR_".Length).Replace("_", " ").Trim();
                    if (rest.Length > 0)
                        return ToTitle(rest);
                }
            }
            if (!string.IsNullOrEmpty(dbName))
                return dbName;
            return serverName ?? "";
        }

        private static string ToTitle(string value)
        {
            try
            {
                string[] parts = value.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    string p = parts[i].ToLower();
                    if (p.Length > 0)
                        parts[i] = char.ToUpper(p[0]) + (p.Length > 1 ? p.Substring(1) : "");
                }
                return string.Join(" ", parts);
            }
            catch { return value; }
        }

        public static void SetBlue(string serverName, string name, bool store)
        {
            if (string.IsNullOrEmpty(serverName))
                return;
            Blues[serverName] = new BlueAttributeRule { ServerName = serverName, Name = name ?? serverName, Store = store };
        }

        public static void ClearBlues()
        {
            Blues.Clear();
        }

        private static ItemFilterRule FindRule(string itemName, string serverName)
        {
            ItemFilterRule rule;
            // 1. Exact match first (fastest)
            if (!string.IsNullOrEmpty(itemName) && Rules.TryGetValue(itemName, out rule))
                return rule;
            if (!string.IsNullOrEmpty(serverName) && Rules.TryGetValue(serverName, out rule))
                return rule;

            // 2. Pattern matching (Wildcard/Regex) - check all rules with patterns
            var candidates = new List<ItemFilterRule>();
            if (!string.IsNullOrEmpty(itemName))
            {
                foreach (var r in Rules.Values)
                {
                    if (r.MatchType != ItemFilterMatchType.Exact && MatchesPattern(itemName, r))
                        candidates.Add(r);
                }
            }
            if (!string.IsNullOrEmpty(serverName))
            {
                foreach (var r in Rules.Values)
                {
                    if (r.MatchType != ItemFilterMatchType.Exact && MatchesPattern(serverName, r))
                        candidates.Add(r);
                }
            }

            // Return most specific match (Exact > Wildcard > Regex, then by pattern length)
            if (candidates.Count > 0)
            {
                candidates.Sort((a, b) =>
                {
                    int typeCmp = a.MatchType.CompareTo(b.MatchType); // Exact=0, Wildcard=1, Regex=2
                    if (typeCmp != 0) return typeCmp;
                    return (a.Pattern?.Length ?? 0).CompareTo(b.Pattern?.Length ?? 0); // Longer pattern = more specific
                });
                return candidates[0];
            }

            return null;
        }

        private static bool MatchesPattern(string value, ItemFilterRule rule)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(rule.Pattern))
                return false;

            try
            {
                switch (rule.MatchType)
                {
                    case ItemFilterMatchType.Wildcard:
                        return WildcardMatch(value, rule.Pattern);
                    case ItemFilterMatchType.Regex:
                        return System.Text.RegularExpressions.Regex.IsMatch(value, rule.Pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            catch
            {
                // Invalid pattern - treat as no match
            }
            return false;
        }

        /// <summary>
        /// Glob-style wildcard matching: * = any chars, ? = single char
        /// </summary>
        private static bool WildcardMatch(string input, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return string.IsNullOrEmpty(input);

            int p = 0, i = 0;
            int starIdx = -1, matchIdx = 0;

            while (i < input.Length)
            {
                if (p < pattern.Length && (pattern[p] == input[i] || pattern[p] == '?'))
                {
                    p++; i++;
                }
                else if (p < pattern.Length && pattern[p] == '*')
                {
                    starIdx = p;
                    matchIdx = i;
                    p++;
                }
                else if (starIdx != -1)
                {
                    p = starIdx + 1;
                    matchIdx++;
                    i = matchIdx;
                }
                else
                {
                    return false;
                }
            }

            while (p < pattern.Length && pattern[p] == '*')
                p++;

            return p == pattern.Length;
        }

        private static ItemFilterOptions GetOptions()
        {
            return new ItemFilterOptions
            {
                MinDegree = MinDegree,
                MaxDegree = MaxDegree,
                OnlySox = OnlySox,
                FilterChina = FilterChina,
                FilterEurope = FilterEurope,
                FilterMale = FilterMale,
                FilterFemale = FilterFemale
            };
        }

        private static ItemFilterInput CreateInput(SRItem item, bool isEquipable)
        {
            SREquipable equip = item as SREquipable;
            var input = new ItemFilterInput
            {
                ItemName = item.Name,
                ServerName = item.ServerName,
                IsEquipable = isEquipable,
                IsGold = item.isType(1, 1, 0) || item.isType(1, 2, 0),
                IsElixirOrStone = item.isType(3, 11, 1) || item.isType(3, 11, 2),
                IsSox = equip != null && equip.GetRarity() != SREquipable.Rarity.None,
                Degree = GetDegree(item.LevelRequired),
                IsChina = equip != null && equip.GetRace() == xBot.Game.Objects.Common.SRTypes.Race.Chinese,
                IsEurope = equip != null && equip.GetRace() == xBot.Game.Objects.Common.SRTypes.Race.European,
                IsMale = equip != null && equip.GetGenre() == SREquipable.Genre.Male,
                IsFemale = equip != null && equip.GetGenre() == SREquipable.Genre.Female,
                Plus = equip != null ? equip.Plus : 0,
                IsArrowBolt = item.ID1 == 3 && item.ID2 == 3 && item.ID3 == 4,
                OwnerKind = 1
            };
            if (equip != null)
            {
                input.IsRare = equip.GetRarity() != SREquipable.Rarity.None;
                input.IsBlue = equip.MagicOptions != null && equip.MagicOptions.Count > 0;
                if (input.IsBlue)
                {
                    input.BlueNames = new List<string>();
                    try
                    {
                        for (int i = 0; i < equip.MagicOptions.Count; i++)
                        {
                            var mo = equip.MagicOptions[i];
                            if (mo == null) continue;
                            string key = !string.IsNullOrEmpty(mo.ServerName) ? mo.ServerName : mo.Name;
                            if (!string.IsNullOrEmpty(key) && !input.BlueNames.Contains(key))
                                input.BlueNames.Add(key);
                            if (!string.IsNullOrEmpty(mo.Name) && !input.BlueNames.Contains(mo.Name))
                                input.BlueNames.Add(mo.Name);
                        }
                    }
                    catch { }
                }
            }
            return input;
        }

        private static ItemFilterInput CreateInput(SRDrop drop)
        {
            return new ItemFilterInput
            {
                ItemName = drop.Name,
                ServerName = drop.ServerName,
                IsEquipable = drop.isEquipable(),
                IsGold = drop.isGold(),
                IsElixirOrStone = drop.ID2 == 3 && drop.ID3 == 11 && (drop.ID4 == 1 || drop.ID4 == 2),
                IsSox = drop.Rarity != 0,
                IsRare = drop.Rarity != 0,
                IsBlue = drop.Rarity != 0,
                Plus = drop.Plus,
                IsArrowBolt = drop.ID2 == 3 && drop.ID3 == 4,
                Degree = GetDegree(drop.LevelRequired),
                IsChina = ContainsToken(drop.ServerName, "_CH_"),
                IsEurope = ContainsToken(drop.ServerName, "_EU_"),
                IsMale = ContainsToken(drop.ServerName, "_M_"),
                IsFemale = ContainsToken(drop.ServerName, "_W_"),
                OwnerKind = GetOwnerKind(drop)
            };
        }

        private static int GetDegree(byte level)
        {
            return level == 0 ? 0 : (level + 7) / 8;
        }

        private static bool ContainsToken(string value, string token)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["MinDegree"] = MinDegree;
            json["MaxDegree"] = MaxDegree;
            json["OnlySox"] = OnlySox;
            json["FilterChina"] = FilterChina;
            json["FilterEurope"] = FilterEurope;
            json["FilterMale"] = FilterMale;
            json["FilterFemale"] = FilterFemale;

            JArray rulesArray = new JArray();
            foreach (var rule in Rules.Values)
            {
                JObject r = new JObject();
                r["Name"] = rule.ItemName;
                r["Pickup"] = rule.Pickup;
                r["Pet"] = rule.Pet;
                r["Sell"] = rule.Sell;
                r["Store"] = rule.Store;
                r["StoreGuild"] = rule.StoreGuild;
                r["MatchType"] = (int)rule.MatchType;
                r["Pattern"] = rule.Pattern ?? "";
                rulesArray.Add(r);
            }
            json["Rules"] = rulesArray;

            // Options sekmesi
            JObject p = new JObject();
            p["PickItemsFirst"] = Pick.PickItemsFirst;
            p["UsePickPet"] = Pick.UsePickPet;
            p["PickOthersItems"] = Pick.PickOthersItems;
            p["PickPartyItems"] = Pick.PickPartyItems;
            p["DontPickItems"] = Pick.DontPickItems;
            p["AllowSellAll"] = Pick.AllowSellAll;
            p["OnlyPickRareBlue"] = Pick.OnlyPickRareBlue;
            p["OnlyStoreRareBlue"] = Pick.OnlyStoreRareBlue;
            p["PickArrowsBolts"] = Pick.PickArrowsBolts;
            p["ArrowBoltAmount"] = Pick.ArrowBoltAmount;
            p["OnlyStoreSpecificBlues"] = Pick.OnlyStoreSpecificBlues;
            p["OnlyPickSpecificBlues"] = Pick.OnlyPickSpecificBlues;
            p["PickWithCharIfPetGoneFull"] = Pick.PickWithCharIfPetGoneFull;
            p["DontMovePetItemsExceptStoreSell"] = Pick.DontMovePetItemsExceptStoreSell;
            p["OnlyStorePlusEnabled"] = Pick.OnlyStorePlusEnabled;
            p["OnlyStorePlus"] = Pick.OnlyStorePlus;
            p["NoSellPlusEnabled"] = Pick.NoSellPlusEnabled;
            p["NoSellPlus"] = Pick.NoSellPlus;
            p["PickEvenWhenFull"] = Pick.PickEvenWhenFull;
            json["PickOptions"] = p;

            JArray bluesArray = new JArray();
            foreach (var b in Blues.Values)
            {
                JObject o = new JObject();
                o["ServerName"] = b.ServerName;
                o["Name"] = b.Name;
                o["Store"] = b.Store;
                bluesArray.Add(o);
            }
            json["Blues"] = bluesArray;

            // Store Gold sekmesi
            JObject g = new JObject();
            g["Enabled"] = StoreGold.Enabled;
            g["GoldKeepAmount"] = StoreGold.GoldKeepAmount;
            g["TakeGoldFromStorage"] = StoreGold.TakeGoldFromStorage;
            g["TakeGoldFromGuildStorage"] = StoreGold.TakeGoldFromGuildStorage;
            g["StoreGoldInStorage"] = StoreGold.StoreGoldInStorage;
            g["StoreGoldMax"] = StoreGold.StoreGoldMax;
            g["StoreGoldInGuildStorage"] = StoreGold.StoreGoldInGuildStorage;
            g["StoreGoldGuildMax"] = StoreGold.StoreGoldGuildMax;
            json["StoreGold"] = g;

            // Dismantle sekmesi
            JObject d = new JObject();
            JArray degArr = new JArray();
            if (Dismantle.Degrees != null)
                foreach (int deg in Dismantle.Degrees)
                    degArr.Add(deg);
            d["Degrees"] = degArr;
            d["WhiteItems"] = Dismantle.WhiteItems;
            d["PlussedBelowEnabled"] = Dismantle.PlussedBelowEnabled;
            d["PlussedBelow"] = Dismantle.PlussedBelow;
            d["BlueItems"] = Dismantle.BlueItems;
            d["RareItems"] = Dismantle.RareItems;
            d["BeforeBlacksmith"] = Dismantle.BeforeBlacksmith;
            d["BeforeGrocery"] = Dismantle.BeforeGrocery;
            d["BeforeHerbalist"] = Dismantle.BeforeHerbalist;
            d["BeforeStorage"] = Dismantle.BeforeStorage;
            d["BeforeGuildStorage"] = Dismantle.BeforeGuildStorage;
            json["Dismantle"] = d;

            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("MinDegree")) MinDegree = (int)json["MinDegree"];
            if (json.ContainsKey("MaxDegree")) MaxDegree = (int)json["MaxDegree"];
            if (json.ContainsKey("OnlySox")) OnlySox = (bool)json["OnlySox"];
            if (json.ContainsKey("FilterChina")) FilterChina = (bool)json["FilterChina"];
            if (json.ContainsKey("FilterEurope")) FilterEurope = (bool)json["FilterEurope"];
            if (json.ContainsKey("FilterMale")) FilterMale = (bool)json["FilterMale"];
            if (json.ContainsKey("FilterFemale")) FilterFemale = (bool)json["FilterFemale"];

            if (json.ContainsKey("Rules"))
            {
                Rules.Clear();
                foreach (JToken t in (JArray)json["Rules"])
                {
                    JObject r = (JObject)t;
                    var matchType = ItemFilterMatchType.Exact;
                    if (r.ContainsKey("MatchType"))
                        matchType = (ItemFilterMatchType)(int)r["MatchType"];
                    var pattern = r.ContainsKey("Pattern") ? (string)r["Pattern"] : "";
                    string name = (string)r["Name"];
                    bool pickup = r.ContainsKey("Pickup") && (bool)r["Pickup"];
                    bool pet = r.ContainsKey("Pet") && (bool)r["Pet"];
                    bool sell = r.ContainsKey("Sell") && (bool)r["Sell"];
                    bool store = r.ContainsKey("Store") && (bool)r["Store"];
                    bool storeGuild = r.ContainsKey("StoreGuild") && (bool)r["StoreGuild"];
                    if (matchType == ItemFilterMatchType.Exact && string.IsNullOrEmpty(pattern))
                    {
                        SetRuleFull(name, pickup, pet, sell, store, storeGuild);
                    }
                    else
                    {
                        var rule = new ItemFilterRule
                        {
                            ItemName = name,
                            Pickup = pickup,
                            Pet = pet,
                            Sell = sell,
                            Store = store,
                            StoreGuild = storeGuild,
                            MatchType = matchType,
                            Pattern = pattern
                        };
                        if (!string.IsNullOrEmpty(name))
                            Rules[name] = rule;
                    }
                }
            }

            if (json.ContainsKey("PickOptions"))
            {
                JObject po = (JObject)json["PickOptions"];
                Pick.PickItemsFirst = GetBool(po, "PickItemsFirst", false);
                Pick.UsePickPet = GetBool(po, "UsePickPet", true);
                Pick.PickOthersItems = GetBool(po, "PickOthersItems", false);
                Pick.PickPartyItems = GetBool(po, "PickPartyItems", false);
                Pick.DontPickItems = GetBool(po, "DontPickItems", false);
                Pick.AllowSellAll = GetBool(po, "AllowSellAll", false);
                Pick.OnlyPickRareBlue = GetBool(po, "OnlyPickRareBlue", false);
                Pick.OnlyStoreRareBlue = GetBool(po, "OnlyStoreRareBlue", false);
                Pick.PickArrowsBolts = GetBool(po, "PickArrowsBolts", false);
                Pick.ArrowBoltAmount = GetInt(po, "ArrowBoltAmount", 200);
                Pick.OnlyStoreSpecificBlues = GetBool(po, "OnlyStoreSpecificBlues", false);
                Pick.OnlyPickSpecificBlues = GetBool(po, "OnlyPickSpecificBlues", false);
                Pick.PickWithCharIfPetGoneFull = GetBool(po, "PickWithCharIfPetGoneFull", true);
                Pick.DontMovePetItemsExceptStoreSell = GetBool(po, "DontMovePetItemsExceptStoreSell", true);
                Pick.OnlyStorePlusEnabled = GetBool(po, "OnlyStorePlusEnabled", false);
                Pick.OnlyStorePlus = GetInt(po, "OnlyStorePlus", 0);
                Pick.NoSellPlusEnabled = GetBool(po, "NoSellPlusEnabled", false);
                Pick.NoSellPlus = GetInt(po, "NoSellPlus", 0);
                Pick.PickEvenWhenFull = GetBool(po, "PickEvenWhenFull", false);
            }

            if (json.ContainsKey("Blues"))
            {
                Blues.Clear();
                foreach (JToken t in (JArray)json["Blues"])
                {
                    JObject o = (JObject)t;
                    string sn = (string)o["ServerName"];
                    if (string.IsNullOrEmpty(sn))
                        continue;
                    Blues[sn] = new BlueAttributeRule
                    {
                        ServerName = sn,
                        Name = o.ContainsKey("Name") ? (string)o["Name"] : sn,
                        Store = o.ContainsKey("Store") && (bool)o["Store"]
                    };
                }
            }

            if (json.ContainsKey("StoreGold"))
            {
                JObject go = (JObject)json["StoreGold"];
                StoreGold.Enabled = GetBool(go, "Enabled", false);
                StoreGold.GoldKeepAmount = GetULong(go, "GoldKeepAmount", 1000000);
                StoreGold.TakeGoldFromStorage = GetBool(go, "TakeGoldFromStorage", false);
                StoreGold.TakeGoldFromGuildStorage = GetBool(go, "TakeGoldFromGuildStorage", false);
                StoreGold.StoreGoldInStorage = GetBool(go, "StoreGoldInStorage", false);
                StoreGold.StoreGoldMax = GetULong(go, "StoreGoldMax", 0);
                StoreGold.StoreGoldInGuildStorage = GetBool(go, "StoreGoldInGuildStorage", false);
                StoreGold.StoreGoldGuildMax = GetULong(go, "StoreGoldGuildMax", 0);
            }

            if (json.ContainsKey("Dismantle"))
            {
                JObject d = (JObject)json["Dismantle"];
                Dismantle.Degrees.Clear();
                if (d.ContainsKey("Degrees"))
                {
                    foreach (JToken t in (JArray)d["Degrees"])
                    {
                        try { Dismantle.Degrees.Add((int)t); } catch { }
                    }
                }
                Dismantle.WhiteItems = GetBool(d, "WhiteItems", false);
                Dismantle.PlussedBelowEnabled = GetBool(d, "PlussedBelowEnabled", false);
                Dismantle.PlussedBelow = GetInt(d, "PlussedBelow", 3);
                Dismantle.BlueItems = GetBool(d, "BlueItems", false);
                Dismantle.RareItems = GetBool(d, "RareItems", false);
                Dismantle.BeforeBlacksmith = GetBool(d, "BeforeBlacksmith", false);
                Dismantle.BeforeGrocery = GetBool(d, "BeforeGrocery", false);
                Dismantle.BeforeHerbalist = GetBool(d, "BeforeHerbalist", false);
                Dismantle.BeforeStorage = GetBool(d, "BeforeStorage", false);
                Dismantle.BeforeGuildStorage = GetBool(d, "BeforeGuildStorage", false);
            }
        }

        private static bool GetBool(JObject o, string key, bool def)
        {
            try
            {
                if (o.ContainsKey(key))
                    return (bool)o[key];
            }
            catch { }
            return def;
        }

        private static int GetInt(JObject o, string key, int def)
        {
            try
            {
                if (o.ContainsKey(key))
                    return (int)o[key];
            }
            catch { }
            return def;
        }

        private static ulong GetULong(JObject o, string key, ulong def)
        {
            try
            {
                if (o.ContainsKey(key))
                    return (ulong)o[key];
            }
            catch { }
            return def;
        }
    }
}
