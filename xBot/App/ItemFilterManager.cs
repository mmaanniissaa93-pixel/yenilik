using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using xBot.Game.Objects.Item;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    public class ItemFilterRule
    {
        public string ItemName { get; set; } = "";
        public bool Pickup { get; set; } = true;
        public bool Sell { get; set; } = false;
        public bool Store { get; set; } = false;
    }

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

        public static IEnumerable<ItemFilterRule> GetAllRules()
        {
            return Rules.Values;
        }

        public static bool ShouldPickup(SRItem item)
        {
            if (item == null)
                return false;

            // Check explicit rule first
            if (Rules.TryGetValue(item.Name, out var rule) || Rules.TryGetValue(item.ServerName, out rule))
            {
                return rule.Pickup;
            }

            // Gold, elixirs, stones always allowed by default unless rule says otherwise
            if (item.isType(1, 1, 0) || item.isType(1, 2, 0) || item.isType(3, 3, 0))
                return true;

            // Check equipment attributes if it's an equipable
            var equip = item as SREquipable;
            if (equip != null)
            {
                if (OnlySox && equip.GetRarity() == SREquipable.Rarity.None)
                    return false;

                var genre = equip.GetGenre();
                if (genre == SREquipable.Genre.Male && !FilterMale) return false;
                if (genre == SREquipable.Genre.Female && !FilterFemale) return false;
            }

            return true;
        }

        /// <summary>
        /// Overload for ground drops (SRDrop). Evaluates degree and rarity filters.
        /// </summary>
        public static bool ShouldPickup(SRDrop drop)
        {
            if (drop == null)
                return true; // default allow

            // Check explicit name rule first
            if (Rules.TryGetValue(drop.Name, out var rule) || Rules.TryGetValue(drop.ServerName, out rule))
                return rule.Pickup;

            // Only equipable items are degree/SoX filtered here
            if (!drop.isEquipable())
                return true;

            // SoX check via Rarity byte (0 = none)
            if (OnlySox && drop.Rarity == 0)
                return false;

            return true;
        }

        public static bool ShouldSell(SRItem item)
        {
            if (item == null)
                return false;

            if (Rules.TryGetValue(item.Name, out var rule) || Rules.TryGetValue(item.ServerName, out rule))
            {
                return rule.Sell;
            }

            return false;
        }

        public static bool ShouldStore(SRItem item)
        {
            if (item == null)
                return false;

            if (Rules.TryGetValue(item.Name, out var rule) || Rules.TryGetValue(item.ServerName, out rule))
            {
                return rule.Store;
            }

            var equip = item as SREquipable;
            if (equip != null && equip.GetRarity() != SREquipable.Rarity.None)
            {
                // Auto store Sox items if enabled
                return true;
            }

            return false;
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
