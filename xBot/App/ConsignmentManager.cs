using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Item;

namespace xBot.App
{
    public static class ConsignmentManager
    {
        private static readonly ConcurrentDictionary<string, ConsignmentRule> Rules =
            new ConcurrentDictionary<string, ConsignmentRule>(StringComparer.OrdinalIgnoreCase);

        public static bool AutoRetrieveExpired { get; set; } = true;
        public static bool AutoSettleSold { get; set; } = true;
        public static bool AutoOpenPlayerStall { get; set; } = true;
        public static int MinimumPlayerStallItems { get; set; } = 1;

        public static IEnumerable<ConsignmentRule> GetRules()
        {
            return Rules.Values;
        }

        public static ConsignmentRule GetRule(string itemName)
        {
            ConsignmentRule rule;
            return !string.IsNullOrWhiteSpace(itemName) && Rules.TryGetValue(itemName, out rule) ? rule : null;
        }

        public static string SetRule(string itemName, ushort quantity, ulong price, bool enabled)
        {
            var rule = new ConsignmentRule
            {
                ItemName = (itemName ?? "").Trim(),
                Quantity = quantity,
                Price = price,
                Enabled = enabled
            };
            string error = ConsignmentPolicy.ValidateRule(rule);
            if (!string.IsNullOrEmpty(error))
                return error;
            Rules[rule.ItemName] = rule;
            return "";
        }

        public static void RemoveRule(string itemName)
        {
            ConsignmentRule ignored;
            if (!string.IsNullOrWhiteSpace(itemName))
                Rules.TryRemove(itemName, out ignored);
        }

        public static List<ConsignmentRegistrationCandidate> GetInventoryCandidates()
        {
            var result = new List<ConsignmentRegistrationCandidate>();
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return result;

            var inventory = InfoManager.Character.Inventory;
            for (int slot = 13; slot < inventory.Capacity && slot <= byte.MaxValue; slot++)
            {
                SRItem item = inventory[slot];
                if (item == null)
                    continue;
                ConsignmentRule rule = GetRule(item.ServerName) ?? GetRule(item.Name);
                if (rule == null)
                {
                    int bestSpecificity = -1;
                    foreach (ConsignmentRule candidateRule in Rules.Values)
                    {
                        string pattern = candidateRule.ItemName ?? "";
                        if (pattern.IndexOf('*') < 0 && pattern.IndexOf('?') < 0)
                            continue;
                        if (!ConsignmentPolicy.MatchesItemPattern(pattern, item.Name, item.ServerName))
                            continue;
                        int specificity = pattern.Replace("*", "").Replace("?", "").Length;
                        if (specificity > bestSpecificity)
                        {
                            rule = candidateRule;
                            bestSpecificity = specificity;
                        }
                    }
                }
                var candidate = ConsignmentPolicy.CreateCandidate(rule, (byte)slot, item.Quantity);
                if (candidate != null)
                {
                    candidate.ItemName = string.IsNullOrWhiteSpace(item.Name) ? item.ServerName : item.Name;
                    candidate.ItemId = item.ID;
                    candidate.RentableId = item.Rentable != null ? item.Rentable.ID : 0;
                    result.Add(candidate);
                }
            }
            return result;
        }

        public static JObject ToJson()
        {
            var root = new JObject();
            root["AutoRetrieveExpired"] = AutoRetrieveExpired;
            root["AutoSettleSold"] = AutoSettleSold;
            root["AutoOpenPlayerStall"] = AutoOpenPlayerStall;
            root["MinimumPlayerStallItems"] = MinimumPlayerStallItems;
            var rules = new JArray();
            foreach (ConsignmentRule rule in Rules.Values)
            {
                rules.Add(new JObject
                {
                    ["ItemName"] = rule.ItemName,
                    ["Enabled"] = rule.Enabled,
                    ["Quantity"] = rule.Quantity,
                    ["Price"] = rule.Price
                });
            }
            root["Rules"] = rules;
            return root;
        }

        public static void FromJson(JObject root)
        {
            Rules.Clear();
            AutoRetrieveExpired = root == null || !root.ContainsKey("AutoRetrieveExpired") || (bool)root["AutoRetrieveExpired"];
            AutoSettleSold = root == null || !root.ContainsKey("AutoSettleSold") || (bool)root["AutoSettleSold"];
            AutoOpenPlayerStall = root == null || !root.ContainsKey("AutoOpenPlayerStall") || (bool)root["AutoOpenPlayerStall"];
            MinimumPlayerStallItems = root != null && root.ContainsKey("MinimumPlayerStallItems")
                ? Math.Max(1, Math.Min(10, (int)root["MinimumPlayerStallItems"])) : 1;
            if (root == null || !root.ContainsKey("Rules"))
                return;
            foreach (JToken token in (JArray)root["Rules"])
            {
                try
                {
                    var rule = (JObject)token;
                    SetRule((string)rule["ItemName"],
                        rule.ContainsKey("Quantity") ? (ushort)rule["Quantity"] : (ushort)1,
                        rule.ContainsKey("Price") ? (ulong)rule["Price"] : 0UL,
                        !rule.ContainsKey("Enabled") || (bool)rule["Enabled"]);
                }
                catch { }
            }
        }
    }
}
