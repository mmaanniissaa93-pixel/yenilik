using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
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

        // In-memory Rules Map (O(1) lookup by item ServerName or Name)
        private static readonly Dictionary<string, ItemFilterRule> Rules = new Dictionary<string, ItemFilterRule>(StringComparer.OrdinalIgnoreCase);

        public static void SetRule(string itemName, bool pickup, bool sell, bool store)
        {
            if (string.IsNullOrEmpty(itemName))
                return;

            Rules[itemName] = new ItemFilterRule
            {
                ItemName = itemName,
                Pickup = pickup,
                Sell = sell,
                Store = store
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
                Rules.Remove(itemName);
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
                FindRule(item.Name, item.ServerName));
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
                FindRule(drop.Name, drop.ServerName));
        }

        public static bool ShouldSell(SRItem item)
        {
            if (item == null)
                return false;

            return ItemFilterPolicy.ShouldSell(
                CreateInput(item, item is SREquipable),
                FindRule(item.Name, item.ServerName));
        }

        public static bool ShouldStore(SRItem item)
        {
            if (item == null)
                return false;

            return ItemFilterPolicy.ShouldStore(
                CreateInput(item, item is SREquipable),
                FindRule(item.Name, item.ServerName));
        }

        private static ItemFilterRule FindRule(string itemName, string serverName)
        {
            ItemFilterRule rule;
            if (!string.IsNullOrEmpty(itemName) && Rules.TryGetValue(itemName, out rule))
                return rule;
            if (!string.IsNullOrEmpty(serverName) && Rules.TryGetValue(serverName, out rule))
                return rule;
            return null;
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
            return new ItemFilterInput
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
                IsFemale = equip != null && equip.GetGenre() == SREquipable.Genre.Female
            };
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
                Degree = GetDegree(drop.LevelRequired),
                IsChina = ContainsToken(drop.ServerName, "_CH_"),
                IsEurope = ContainsToken(drop.ServerName, "_EU_"),
                IsMale = ContainsToken(drop.ServerName, "_M_"),
                IsFemale = ContainsToken(drop.ServerName, "_W_")
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
                r["Sell"] = rule.Sell;
                r["Store"] = rule.Store;
                rulesArray.Add(r);
            }
            json["Rules"] = rulesArray;
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
                    SetRule(
                        (string)r["Name"],
                        r.ContainsKey("Pickup") && (bool)r["Pickup"],
                        r.ContainsKey("Sell") && (bool)r["Sell"],
                        r.ContainsKey("Store") && (bool)r["Store"]
                    );
                }
            }
        }
    }
}
