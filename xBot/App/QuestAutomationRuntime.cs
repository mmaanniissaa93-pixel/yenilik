using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace xBot.App
{
    public enum QuestAutomationState
    {
        Inactive, GoingToNpc, Accepting, Active, Completed, TurningIn, WaitingToRepeat, Failed
    }

    public enum QuestRewardPreference { EquippedWeapon, WeaponType, ItemName, RewardId }

    // Pure transaction model. Only a parsed server update can confirm an operation.
    public sealed class QuestNpcTransaction
    {
        public uint QuestId { get; set; }
        public QuestNpcOperation Operation { get; set; }
        public int Attempts { get; private set; }
        public bool Waiting { get; private set; }
        public bool ActionSent { get; set; }
        public bool RewardSent { get; set; }
        public bool Confirmed { get; private set; }
        public bool Terminal { get; private set; }
        public DateTime DeadlineUtc { get; private set; }
        public DateTime RetryAtUtc { get; private set; }
        public string Error { get; private set; } = "";

        public bool Begin(DateTime now)
        {
            if (Waiting || Confirmed || Terminal || now < RetryAtUtc) return false;
            Attempts++;
            Waiting = true;
            ActionSent = RewardSent = false;
            DeadlineUtc = now.AddSeconds(12);
            return true;
        }

        public void AwaitConfirmation(DateTime now) { DeadlineUtc = now.AddSeconds(12); }

        public void Fail(DateTime now, string error, bool terminal = false)
        {
            Waiting = false;
            Error = error;
            Terminal = terminal || Attempts >= 3;
            RetryAtUtc = now.AddSeconds(5 * Math.Max(1, Attempts));
        }

        public bool Observe(uint questId, byte updateType)
        {
            if (questId != QuestId || Confirmed || !ActionSent) return false;
            // Add acknowledges acceptance; an ordinary progress update cannot.
            // Abandon (4) must never masquerade as a rewarded removal (3).
            bool success = Operation == QuestNpcOperation.Accept ? updateType == 1
                : updateType == 3 && RewardSent;
            if (!success) return false;
            Confirmed = true;
            Waiting = false;
            Terminal = false;
            Error = "";
            return true;
        }
    }

    public static class QuestRewardPolicy
    {
        public static bool TryResolve(IList<NameValueCollection> rewards, QuestRewardPreference preference,
            uint preferredId, byte weaponType, string itemName, out uint rewardId)
        {
            rewardId = 0;
            if (rewards == null) return false;
            // Preserve the verified fixed reward wire format for zero/one item catalogs.
            if (rewards.Count <= 1) return true;
            HashSet<uint> matches = new HashSet<uint>();
            foreach (NameValueCollection reward in rewards)
            {
                uint id = Number(reward, "reward_id");
                bool match;
                switch (preference)
                {
                    case QuestRewardPreference.RewardId: match = id == preferredId; break;
                    case QuestRewardPreference.ItemName:
                        match = !string.IsNullOrWhiteSpace(itemName)
                            && (string.Equals(reward["name"], itemName.Trim(), StringComparison.OrdinalIgnoreCase)
                                || string.Equals(reward["item_servername"], itemName.Trim(), StringComparison.OrdinalIgnoreCase));
                        break;
                    default:
                        match = weaponType != 0 && Number(reward, "tid2") == 1 && Number(reward, "tid3") == 6
                            && Number(reward, "tid4") == weaponType;
                        break;
                }
                if (match && id != 0) matches.Add(id);
            }
            if (matches.Count != 1) return false;
            rewardId = matches.First();
            return true;
        }

        private static uint Number(NameValueCollection row, string key)
        {
            uint value;
            return row != null && uint.TryParse(row[key], out value) ? value : 0;
        }
    }

    public sealed class QuestNpcPosition
    {
        public uint ModelId { get; set; }
        public ushort Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }

    public static class QuestNpcCatalogPolicy
    {
        // Verified against Media.pk2 npcpos.txt: model, region, X, height Z, Y.
        public static QuestNpcPosition ParsePosition(string line)
        {
            string[] fields = (line ?? "").Trim().Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            uint model;
            int rawRegion;
            double x, y, z;
            if (fields.Length != 5 || !uint.TryParse(fields[0], out model) || model == 0
                || !int.TryParse(fields[1], out rawRegion) || rawRegion == 0 || rawRegion < short.MinValue || rawRegion > ushort.MaxValue
                || !Coordinate(fields[2], out x) || !Coordinate(fields[3], out z)
                || !Coordinate(fields[4], out y)) return null;
            ushort region = unchecked((ushort)rawRegion);
            if (region <= short.MaxValue && (x < 0 || x > 1920 || y < 0 || y > 1920)) return null;
            return new QuestNpcPosition { ModelId = model, Region = region,
                X = (int)Math.Round(x), Y = (int)Math.Round(y), Z = (int)Math.Round(z) };
        }

        private static bool Coordinate(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && !double.IsNaN(value) && !double.IsInfinity(value) && Math.Abs(value) < 10000000;
        }

        public static bool MatchesNotice(string notice, string npcCode, string npcName)
        {
            if (string.IsNullOrWhiteSpace(notice)) return false;
            if (string.Equals(notice.Trim(), npcCode, StringComparison.OrdinalIgnoreCase)) return true;
            // Names in NoticeNPC can be reordered: "Soboi, blacksmith of Hotan".
            // Require every name token, and reject one-word titles/partial names.
            string[] name = Tokens(npcName);
            HashSet<string> words = new HashSet<string>(Tokens(notice), StringComparer.OrdinalIgnoreCase);
            return name.Length >= 2 && name.All(words.Contains);
        }

        public static bool MatchesQuestNpc(string questCode, string notice, string npcCode, string npcName)
        {
            if (MatchesNotice(notice, npcCode, npcName)) return true;
            // A family code alone is insufficient. Corroborate it with the visible
            // NPC name for notices that omit a title ("Baekako of Donwhang").
            if (string.IsNullOrWhiteSpace(questCode) || string.IsNullOrWhiteSpace(npcCode)
                || !npcCode.StartsWith("NPC_", StringComparison.OrdinalIgnoreCase)
                || questCode.Length <= 4 || !questCode.Substring(4).StartsWith(npcCode.Substring(4) + "_", StringComparison.OrdinalIgnoreCase)) return false;
            HashSet<string> words = new HashSet<string>(Tokens(notice), StringComparer.OrdinalIgnoreCase);
            HashSet<string> titles = new HashSet<string>(new[] { "general", "blacksmith", "soldier", "solider", "priest", "merchant",
                "storekeeper", "storage", "keeper", "potion", "guide", "guard", "nun", "herbalist", "jewelry", "jeweler",
                "jewel", "craftsman", "trader", "trades", "boss", "associate", "ticket", "agent", "of", "the",
                "hotan", "jangan", "donwhang", "constantinople", "samarkand" }, StringComparer.OrdinalIgnoreCase);
            return Tokens(npcName).Any(t => t.Length >= 3 && !titles.Contains(t) && words.Contains(t));
        }

        private static string[] Tokens(string text)
        {
            return Regex.Split((text ?? "").ToLowerInvariant(), @"[^\p{L}\p{N}]+").Where(t => t.Length > 0).ToArray();
        }
    }
}
