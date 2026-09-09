using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    public sealed class QuestAutomationRule
    {
        public uint QuestId { get; set; }
        public string DisplayName { get; set; } = "";
        public bool Enabled { get; set; }
        public QuestCompletionAction CompletionAction { get; set; }
        public string ScriptPath { get; set; } = "";
    }

    public sealed class QuestAutomationRequest
    {
        public uint QuestId { get; set; }
        public string DisplayName { get; set; }
        public QuestCompletionAction Action { get; set; }
        public string ScriptPath { get; set; }
    }

    /// <summary>
    /// Character-profile quest rules and a single-owner completion queue.
    /// Network handlers only observe state; the bot thread executes actions.
    /// </summary>
    public static class QuestAutomationManager
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<uint, QuestAutomationRule> Rules = new Dictionary<uint, QuestAutomationRule>();
        private static readonly HashSet<string> HandledCompletions = new HashSet<string>();
        private static readonly Queue<QuestAutomationRequest> Pending = new Queue<QuestAutomationRequest>();

        public static string Status { get; private set; } = "Hazır";

        public static IList<QuestAutomationRule> GetRules()
        {
            lock (Sync)
            {
                List<QuestAutomationRule> copy = new List<QuestAutomationRule>();
                foreach (QuestAutomationRule rule in Rules.Values) copy.Add(Clone(rule));
                copy.Sort((a, b) => a.QuestId.CompareTo(b.QuestId));
                return copy.AsReadOnly();
            }
        }

        public static QuestAutomationRule GetRule(uint questId)
        {
            lock (Sync)
            {
                QuestAutomationRule rule;
                return Rules.TryGetValue(questId, out rule) ? Clone(rule) : null;
            }
        }

        public static QuestAutomationRule EnsureRule(uint questId, string suggestedName)
        {
            lock (Sync)
            {
                QuestAutomationRule rule;
                if (!Rules.TryGetValue(questId, out rule))
                {
                    rule = new QuestAutomationRule { QuestId = questId, DisplayName = suggestedName ?? "" };
                    Rules[questId] = rule;
                }
                else if (string.IsNullOrWhiteSpace(rule.DisplayName) && !string.IsNullOrWhiteSpace(suggestedName))
                    rule.DisplayName = suggestedName;
                return Clone(rule);
            }
        }

        public static void SaveRule(QuestAutomationRule rule)
        {
            if (rule == null || rule.QuestId == 0) return;
            lock (Sync)
            {
                Rules[rule.QuestId] = Clone(rule);
                if (!rule.Enabled)
                    RemoveHandledKeys(rule.QuestId);
            }
        }

        public static void ObserveQuest(SRQuest quest)
        {
            if (quest == null) return;
            lock (Sync)
            {
                QuestAutomationRule rule;
                if (!Rules.TryGetValue(quest.ID, out rule)) return;
                if (!QuestAutomationPolicy.IsReadyToTurnIn(quest.State))
                {
                    RemoveHandledKeys(quest.ID);
                    return;
                }
                string key = QuestAutomationPolicy.CompletionKey(quest.ID, quest.State);
                if (!QuestAutomationPolicy.ShouldQueue(rule.Enabled, rule.CompletionAction, quest.State,
                    HandledCompletions.Contains(key))) return;
                HandledCompletions.Add(key);
                Pending.Enqueue(new QuestAutomationRequest
                {
                    QuestId = quest.ID,
                    DisplayName = string.IsNullOrWhiteSpace(rule.DisplayName) ? "Quest " + quest.ID : rule.DisplayName,
                    Action = rule.CompletionAction,
                    ScriptPath = rule.ScriptPath
                });
                Status = "Tamamlandı: " + (string.IsNullOrWhiteSpace(rule.DisplayName) ? quest.ID.ToString() : rule.DisplayName);
            }
        }

        public static void ObserveActiveQuests()
        {
            if (InfoManager.Character == null || InfoManager.Character.Quests == null) return;
            foreach (SRQuest quest in InfoManager.Character.Quests.Snapshot()) ObserveQuest(quest);
        }

        public static bool TryExecutePending(Bot bot)
        {
            if (bot == null) return false;
            QuestAutomationRequest request;
            lock (Sync)
            {
                if (Pending.Count == 0) return false;
                request = Pending.Dequeue();
            }
            if (request.Action == QuestCompletionAction.ReturnTown)
            {
                bool sent = bot.UseReturnScroll();
                Status = sent ? "Şehre dönülüyor: " + request.DisplayName : "Return scroll bulunamadı: " + request.DisplayName;
                Window.Get?.Log("Quest: " + Status, sent ? Theme.LogLevel.Info : Theme.LogLevel.Warning);
                return true;
            }
            if (request.Action == QuestCompletionAction.RunScript)
            {
                if (string.IsNullOrWhiteSpace(request.ScriptPath) || !File.Exists(request.ScriptPath))
                {
                    Status = "Script bulunamadı: " + request.DisplayName;
                    Window.Get?.Log("Quest: " + Status, Theme.LogLevel.Warning);
                    return true;
                }
                Status = "Script çalışıyor: " + request.DisplayName;
                Window.Get?.Log("Quest: " + Status);
                new Script(request.ScriptPath).Run(0);
                Status = "Script tamamlandı: " + request.DisplayName;
                return true;
            }
            return false;
        }

        public static JObject ToJson()
        {
            JArray rules = new JArray();
            foreach (QuestAutomationRule rule in GetRules())
            {
                rules.Add(new JObject
                {
                    ["QuestId"] = rule.QuestId,
                    ["DisplayName"] = rule.DisplayName,
                    ["Enabled"] = rule.Enabled,
                    ["CompletionAction"] = rule.CompletionAction.ToString(),
                    ["ScriptPath"] = rule.ScriptPath
                });
            }
            return new JObject { ["Rules"] = rules };
        }

        public static void FromJson(JObject root)
        {
            lock (Sync)
            {
                Rules.Clear();
                HandledCompletions.Clear();
                Pending.Clear();
                Status = "Hazır";
                JArray rules = root == null ? null : root["Rules"] as JArray;
                if (rules == null) return;
                foreach (JObject item in rules)
                {
                    uint questId = ReadUInt(item, "QuestId");
                    if (questId == 0) continue;
                    QuestCompletionAction action;
                    if (!Enum.TryParse((string)item["CompletionAction"], true, out action)) action = QuestCompletionAction.None;
                    Rules[questId] = new QuestAutomationRule
                    {
                        QuestId = questId,
                        DisplayName = (string)item["DisplayName"] ?? "",
                        Enabled = item["Enabled"] != null && (bool)item["Enabled"],
                        CompletionAction = action,
                        ScriptPath = (string)item["ScriptPath"] ?? ""
                    };
                }
            }
        }

        private static QuestAutomationRule Clone(QuestAutomationRule rule)
        {
            return new QuestAutomationRule
            {
                QuestId = rule.QuestId, DisplayName = rule.DisplayName, Enabled = rule.Enabled,
                CompletionAction = rule.CompletionAction, ScriptPath = rule.ScriptPath
            };
        }

        private static void RemoveHandledKeys(uint questId)
        {
            string prefix = questId + ":";
            HandledCompletions.RemoveWhere(key => key.StartsWith(prefix, StringComparison.Ordinal));
        }

        private static uint ReadUInt(JObject root, string key)
        {
            uint value;
            return root[key] != null && uint.TryParse(root[key].ToString(), out value) ? value : 0;
        }
    }
}
