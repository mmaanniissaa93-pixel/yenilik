using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Navigation;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

namespace xBot.App
{
    public sealed class QuestAutomationRule
    {
        public uint QuestId { get; set; }
        public string DisplayName { get; set; } = "";
        public bool Enabled { get; set; }
        public bool AutoAccept { get; set; } = true;
        public bool AutoTurnIn { get; set; } = true;
        public bool RepeatIfAvailable { get; set; } = true;
        public QuestCompletionAction CompletionAction { get; set; }
        public string ScriptPath { get; set; } = "";
        public string NpcServerName { get; set; } = "";
        public QuestRewardPreference RewardPreference { get; set; }
        public uint PreferredRewardId { get; set; }
        public byte PreferredWeaponType { get; set; }
        public string PreferredItemName { get; set; } = "";
        public uint NpcModelId { get; set; }
        public ushort NpcRegion { get; set; }
        public int NpcX { get; set; }
        public int NpcY { get; set; }
        public int NpcZ { get; set; }
    }

    public sealed class QuestAutomationRequest
    {
        public uint QuestId { get; set; }
        public string DisplayName { get; set; }
        public QuestCompletionAction Action { get; set; }
        public string ScriptPath { get; set; }
        public string NpcServerName { get; set; }
        public uint NpcModelId { get; set; }
        public SRCoord NpcPosition { get; set; }
        public QuestNpcOperation NpcOperation { get; set; }
    }

    /// <summary>
    /// Character-profile quest rules and a single-owner completion queue.
    /// The bot owns navigation; packet handlers advance the one pending conversation.
    /// </summary>
    public static class QuestAutomationManager
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<uint, QuestAutomationRule> Rules = new Dictionary<uint, QuestAutomationRule>();
        private static readonly HashSet<string> HandledCompletions = new HashSet<string>();
        private static readonly HashSet<uint> CompletedThisSession = new HashSet<uint>();
        private static readonly Queue<QuestAutomationRequest> Pending = new Queue<QuestAutomationRequest>();
        private static readonly Dictionary<uint, QuestNpcTransaction> Transactions = new Dictionary<uint, QuestNpcTransaction>();
        private static readonly Dictionary<uint, QuestAutomationState> States = new Dictionary<uint, QuestAutomationState>();
        private static readonly Dictionary<uint, DateTime> RepeatAfterUtc = new Dictionary<uint, DateTime>();
        private static QuestNpcTransaction Current;
        private static uint PendingTalkNpcUniqueId;
        private static bool MenuSelected;

        public static int MaximumLevelAbovePlayer { get; private set; }
        public static bool WaitForAllEnabledQuests { get; private set; }
        public static bool EventQuestsInTownOnly { get; private set; } = true;

        public static void SetOptions(int levelMargin, bool waitForAll, bool eventsInTown)
        {
            lock (Sync)
            {
                MaximumLevelAbovePlayer = Math.Max(0, Math.Min(20, levelMargin));
                WaitForAllEnabledQuests = waitForAll;
                EventQuestsInTownOnly = eventsInTown;
            }
        }

        private static bool MustWaitForOtherQuests()
        {
            if (!WaitForAllEnabledQuests) return false;
            foreach (QuestAutomationRule rule in Rules.Values)
            {
                if (!rule.Enabled) continue;
                SRQuest active = InfoManager.Character?.Quests?[rule.QuestId];
                // Inactive/unavailable catalog entries must not block completed active quests forever.
                if (active != null && !QuestAutomationPolicy.IsReadyToTurnIn(active.State)) return true;
            }
            return false;
        }

        public static string Status { get; private set; } = "Hazır";
        public static QuestTalkSnapshot LastTalk { get; private set; }
        public static byte? LastTalkChoice { get; private set; }

        public static bool IsBusy { get { lock (Sync) return Current != null; } }

        public static void PollTimeout()
        {
            lock (Sync)
                if (Current != null && PendingTalkNpcUniqueId != 0 && DateTime.UtcNow > Current.DeadlineUtc)
                    FailCurrent("Sunucu onayı zaman aşımı");
        }

        public static bool ArmQuestTalkSelection(uint questId, uint npcUniqueId, bool autoReward, out string error)
        {
            return ArmQuestTalkSelection(questId, npcUniqueId,
                autoReward ? QuestNpcOperation.TurnIn : QuestNpcOperation.Accept, out error);
        }

        public static bool ArmQuestTalkSelection(uint questId, uint npcUniqueId,
            QuestNpcOperation operation, out string error)
        {
            error = "";
            System.Collections.Specialized.NameValueCollection data = DataManager.GetQuestData(questId);
            if (data == null || string.IsNullOrWhiteSpace(data["namestring"]))
            {
                error = "Görevin PK2 NameString kaydı bulunamadı; PK2 veritabanını güncelleyin.";
                return false;
            }
            lock (Sync)
            {
                if (operation == QuestNpcOperation.Accept && InfoManager.Character?.Quests?[questId] != null)
                { error = "Görev zaten aktif."; return false; }
                if (Current != null) { error = "Başka bir görev işlemi sunucu onayı bekliyor."; return false; }
                QuestNpcTransaction transaction;
                if (!Transactions.TryGetValue(questId, out transaction) || transaction.Confirmed
                    || transaction.Operation != operation)
                    Transactions[questId] = transaction = new QuestNpcTransaction { QuestId = questId, Operation = operation };
                if (!transaction.Begin(DateTime.UtcNow)) { error = transaction.Error; return false; }
                Current = transaction;
                MenuSelected = false;
                PendingTalkNpcUniqueId = npcUniqueId;
                LearnNpc(Rules.ContainsKey(questId) ? Rules[questId] : null, npcUniqueId);
                States[questId] = operation == QuestNpcOperation.Accept ? QuestAutomationState.Accepting : QuestAutomationState.TurningIn;
                Status = StateText(States[questId]) + ": " + ResolveDisplayName(null, questId)
                    + " (" + transaction.Attempts + "/3)";
            }
            return true;
        }

        public static void ObserveTalkPacket(byte[] raw)
        {
            QuestTalkSnapshot snapshot = QuestAutomationPolicy.ParseTalkPacket(raw);
            lock (Sync)
            {
                LastTalk = snapshot;
                Window.Get?.Log("[QuestTalk][S->C] type=" + snapshot.Type + ", choices="
                    + string.Join(" | ", snapshot.Choices) + ", raw=" + snapshot.RawHex);
                if (Current == null || PendingTalkNpcUniqueId == 0 || MenuSelected) return;
                if (DateTime.UtcNow > Current.DeadlineUtc) { FailCurrent("NPC menü zaman aşımı"); return; }
                if (!string.IsNullOrWhiteSpace(snapshot.Error)) { FailCurrent(snapshot.Error); return; }
                // Other dialog types are not the quest list; keep waiting for type 4.
                if (snapshot.Type != 4) return;
                NameValueCollection data = DataManager.GetQuestData(Current.QuestId);
                byte action = QuestAutomationPolicy.ResolveTalkActionCode(snapshot.Type, snapshot.Choices, data?["namestring"]);
                if (action == 0) { FailCurrent("Görev NPC menüsünde sunulmadı"); return; }
                MenuSelected = true;
                Current.ActionSent = true;
                Current.AwaitConfirmation(DateTime.UtcNow);
                PacketBuilder.SelectQuestTalkOption(action);
            }
        }

        public static void ObserveQuestIdResponse(uint questId)
        {
            lock (Sync)
            {
                if (Current == null || !MenuSelected || Current.RewardSent) return;
                if (DateTime.UtcNow > Current.DeadlineUtc) { FailCurrent("Quest ID zaman aşımı"); return; }
                if (Current.QuestId != questId) { FailCurrent("NPC farklı quest ID döndürdü"); return; }
                if (Current.Operation != QuestNpcOperation.TurnIn) return;
                uint rewardId;
                string error;
                if (!TryResolveRewardId(questId, out rewardId, out error)) { FailCurrent(error, true); return; }
                Current.RewardSent = true;
                Current.AwaitConfirmation(DateTime.UtcNow);
                Status = "Turning in: " + questId + " — 0x30D5 silinme onayı bekleniyor";
                PacketBuilder.ReceiveEventQuestReward(questId, rewardId);
            }
        }

        private static void FailCurrent(string error, bool terminal = false)
        {
            if (Current == null) return;
            Current.Fail(DateTime.UtcNow, error, terminal);
            States[Current.QuestId] = QuestAutomationState.Failed;
            Status = "Failed: " + Current.QuestId + " — " + error
                + (Current.Terminal ? " (otomasyonu yeniden etkinleştirerek tekrar dene)" : " (kontrollü tekrar bekleniyor)");
            Window.Get?.Log("Quest: " + Status, Theme.LogLevel.Warning);
            ReleaseConversation();
        }

        private static void ReleaseConversation()
        {
            uint npc = PendingTalkNpcUniqueId;
            Current = null;
            PendingTalkNpcUniqueId = 0;
            MenuSelected = false;
            if (npc != 0 && InfoManager.inGame) PacketBuilder.CloseNPC(npc);
        }

        public static void ObserveServerUpdate(uint questId, byte updateType)
        {
            lock (Sync)
            {
                QuestNpcTransaction transaction;
                if (Transactions.TryGetValue(questId, out transaction) && transaction.Observe(questId, updateType))
                {
                    if (transaction.Operation == QuestNpcOperation.TurnIn)
                    {
                        CompletedThisSession.Add(questId);
                        RepeatAfterUtc[questId] = DateTime.UtcNow.AddSeconds(5);
                        QuestAutomationRule rule;
                        States[questId] = Rules.TryGetValue(questId, out rule) && rule.RepeatIfAvailable
                            ? QuestAutomationState.WaitingToRepeat : QuestAutomationState.Inactive;
                    }
                    else
                    {
                        CompletedThisSession.Remove(questId);
                        RepeatAfterUtc.Remove(questId);
                        States[questId] = QuestAutomationState.Active;
                    }
                    RemoveHandledKeys(questId);
                    Status = StateText(States[questId]) + ": " + questId + " — 0x30D5 onaylandı";
                    Window.Get?.Log("Quest: " + Status);
                    if (Current == transaction) ReleaseConversation();
                }
                if (updateType == 3 && Transactions.TryGetValue(questId, out transaction) && !transaction.Confirmed
                    && transaction.Operation == QuestNpcOperation.TurnIn)
                {
                    if (Current == transaction) FailCurrent("Ödül isteğiyle eşleşmeyen görev silinmesi", true);
                    transaction.Fail(DateTime.UtcNow, "Teslim doğrulanamadı", true);
                    States[questId] = QuestAutomationState.Failed;
                }
                if (updateType == 4)
                {
                    if (Current != null && Current.QuestId == questId) FailCurrent("Görev bırakıldı", true);
                    Transactions[questId] = transaction = new QuestNpcTransaction { QuestId = questId };
                    transaction.Fail(DateTime.UtcNow, "Görev bırakıldı", true);
                    States[questId] = QuestAutomationState.Failed;
                    RemoveHandledKeys(questId);
                }
            }
        }

        public static void Suspend()
        {
            lock (Sync) if (Current != null) FailCurrent("Bot durduruldu veya teleport başladı");
        }

        public static void CancelSession()
        {
            lock (Sync)
            {
                Current = null;
                PendingTalkNpcUniqueId = 0;
                MenuSelected = false;
                Transactions.Clear();
                States.Clear();
                RepeatAfterUtc.Clear();
                Pending.Clear();
                HandledCompletions.Clear();
                CompletedThisSession.Clear();
            }
        }

        public static string GetFailureReason(uint questId)
        {
            lock (Sync)
            {
                QuestNpcTransaction transaction;
                return Transactions.TryGetValue(questId, out transaction) ? transaction.Error : "";
            }
        }

        public static string GetStateText(uint questId)
        {
            lock (Sync)
            {
                QuestAutomationState state;
                if (States.TryGetValue(questId, out state) && state != QuestAutomationState.Active
                    && state != QuestAutomationState.Completed) return StateText(state);
                SRQuest quest = InfoManager.Character?.Quests?[questId];
                return quest == null ? "Inactive" : QuestAutomationPolicy.IsReadyToTurnIn(quest.State) ? "Completed" : "Active";
            }
        }

        private static string StateText(QuestAutomationState state)
        {
            switch (state)
            {
                case QuestAutomationState.GoingToNpc: return "Going to NPC";
                case QuestAutomationState.TurningIn: return "Turning in";
                case QuestAutomationState.WaitingToRepeat: return "Waiting to repeat";
                default: return state.ToString();
            }
        }

        public static void ObserveTalkChoice(byte choice, byte[] raw)
        {
            lock (Sync) { LastTalkChoice = choice; }

            // This byte is an action code (normally 6 for dynamic SN_ quest rows),
            // not a menu index, so it must not be associated by position here.
            Window.Get?.Log("[QuestTalk][C->S] action=" + choice + ", raw=" + ToHex(raw));
        }

        public static void ObserveRewardSelection(byte[] raw)
        {
            QuestRewardSelection reward = QuestAutomationPolicy.ParseRewardSelection(raw);
            Window.Get?.Log(string.IsNullOrWhiteSpace(reward.Error)
                ? "[QuestTalk][C->S] reward questId=" + reward.QuestId + ", selection=" + reward.Selection + ", rewardId=" + reward.RewardId + ", raw=" + ToHex(raw)
                : "[QuestTalk][C->S] reward çözülemedi: " + reward.Error + ", raw=" + ToHex(raw),
                string.IsNullOrWhiteSpace(reward.Error) ? Theme.LogLevel.Info : Theme.LogLevel.Warning);
        }

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
                    // Suggested/catalog names are display data, not user aliases.
                    // Keep the persisted alias empty unless the user explicitly saves one.
                    rule = new QuestAutomationRule { QuestId = questId, DisplayName = "" };
                    Rules[questId] = rule;
                }
                return Clone(rule);
            }
        }

        public static void SaveRule(QuestAutomationRule rule)
        {
            if (rule == null || rule.QuestId == 0) return;
            lock (Sync)
            {
                QuestAutomationRule existing;
                if (Rules.TryGetValue(rule.QuestId, out existing) && string.IsNullOrWhiteSpace(rule.NpcServerName))
                {
                    rule.NpcServerName = existing.NpcServerName;
                    rule.NpcModelId = existing.NpcModelId;
                    rule.NpcRegion = existing.NpcRegion;
                    rule.NpcX = existing.NpcX;
                    rule.NpcY = existing.NpcY;
                    rule.NpcZ = existing.NpcZ;
                }
                Rules[rule.QuestId] = Clone(rule);
                if (Current != null && Current.QuestId == rule.QuestId) ReleaseConversation();
                Transactions.Remove(rule.QuestId);
                States.Remove(rule.QuestId);
                RemoveHandledKeys(rule.QuestId);
            }
        }

        public static void ResetRules()
        {
            lock (Sync)
            {
                if (Current != null) ReleaseConversation();
                Rules.Clear();
                HandledCompletions.Clear();
                CompletedThisSession.Clear();
                CancelSession();
                Pending.Clear();
                SetOptions(0, false, true);
                Status = "Quest ayarları sıfırlandı";
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
                if (MustWaitForOtherQuests()) return;
                string key = QuestAutomationPolicy.CompletionKey(quest.ID, quest.State);
                if (!rule.AutoTurnIn || rule.CompletionAction == QuestCompletionAction.None
                    || rule.CompletionAction == QuestCompletionAction.TurnInAtNpc) return;
                QuestCompletionAction completionAction = rule.CompletionAction == QuestCompletionAction.None
                    ? QuestCompletionAction.TurnInAtNpc : rule.CompletionAction;
                if (!QuestAutomationPolicy.ShouldQueue(rule.Enabled, completionAction, quest.State,
                    HandledCompletions.Contains(key))) return;
                HandledCompletions.Add(key);
                Pending.Enqueue(new QuestAutomationRequest
                {
                    QuestId = quest.ID,
                    DisplayName = ResolveDisplayName(rule, quest.ID),
                    Action = completionAction,
                    ScriptPath = rule.ScriptPath,
                    NpcServerName = rule.NpcServerName,
                    NpcModelId = rule.NpcModelId,
                    NpcPosition = rule.NpcRegion == 0 ? null : new SRCoord(rule.NpcRegion, rule.NpcX, rule.NpcZ, rule.NpcY),
                    NpcOperation = QuestNpcOperation.TurnIn
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
            if (bot == null || !InfoManager.inGame || InfoManager.inTeleport) return false;
            lock (Sync)
            {
                if (Current != null)
                {
                    if (DateTime.UtcNow > Current.DeadlineUtc) FailCurrent("Sunucu onayı zaman aşımı");
                    return true; // Own the bot loop until acknowledgment or timeout.
                }
            }
            QuestAutomationRequest request;
            lock (Sync)
            {
                request = Pending.Count == 0 ? null : Pending.Dequeue();
            }
            if (request == null) request = CreateAutomaticTownQuestRequest();
            if (request == null) return false;
            lock (Sync)
            {
                if (request.NpcOperation == QuestNpcOperation.TurnIn && MustWaitForOtherQuests())
                {
                    Pending.Enqueue(request);
                    return false;
                }
            }
            QuestAutomationRule activeRule = GetRule(request.QuestId);
            if (activeRule == null || !activeRule.Enabled) return false;
            if (request.Action == QuestCompletionAction.ReturnTown || request.Action == QuestCompletionAction.RunScript)
            {
                SRQuest active = InfoManager.Character?.Quests?[request.QuestId];
                if (active == null || !QuestAutomationPolicy.IsReadyToTurnIn(active.State)
                    || !activeRule.AutoTurnIn || activeRule.CompletionAction != request.Action
                    || (request.Action == QuestCompletionAction.RunScript && activeRule.ScriptPath != request.ScriptPath)) return false;
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
            if (request.Action == QuestCompletionAction.TurnInAtNpc)
            {
                ExecuteNpcTurnIn(bot, request);
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
                    ["AutoAccept"] = rule.AutoAccept,
                    ["AutoTurnIn"] = rule.AutoTurnIn,
                    ["RepeatIfAvailable"] = rule.RepeatIfAvailable,
                    ["CompletionAction"] = rule.CompletionAction.ToString(),
                    ["RewardPreference"] = rule.RewardPreference.ToString(),
                    ["PreferredRewardId"] = rule.PreferredRewardId,
                    ["PreferredWeaponType"] = rule.PreferredWeaponType,
                    ["PreferredItemName"] = rule.PreferredItemName,
                    ["ScriptPath"] = rule.ScriptPath
                    ,["NpcServerName"] = rule.NpcServerName
                    ,["NpcModelId"] = rule.NpcModelId
                    ,["NpcRegion"] = rule.NpcRegion
                    ,["NpcX"] = rule.NpcX
                    ,["NpcY"] = rule.NpcY
                    ,["NpcZ"] = rule.NpcZ
                });
            }
            return new JObject { ["Rules"] = rules, ["MaximumLevelAbovePlayer"] = MaximumLevelAbovePlayer, ["WaitForAllEnabledQuests"] = WaitForAllEnabledQuests, ["EventQuestsInTownOnly"] = EventQuestsInTownOnly };
        }

        public static void FromJson(JObject root)
        {
            lock (Sync)
            {
                if (Current != null) ReleaseConversation();
                Rules.Clear();
                HandledCompletions.Clear();
                CompletedThisSession.Clear();
                CancelSession();
                Pending.Clear();
                Status = "Hazır";
                SetOptions(root == null ? 0 : ReadInt(root, "MaximumLevelAbovePlayer"),
                    root?["WaitForAllEnabledQuests"] != null && (bool)root["WaitForAllEnabledQuests"],
                    root?["EventQuestsInTownOnly"] == null || (bool)root["EventQuestsInTownOnly"]);
                JArray rules = root == null ? null : root["Rules"] as JArray;
                if (rules == null) return;
                foreach (JObject item in rules)
                {
                    uint questId = ReadUInt(item, "QuestId");
                    if (questId == 0) continue;
                    QuestCompletionAction action;
                    if (!Enum.TryParse((string)item["CompletionAction"], true, out action)) action = QuestCompletionAction.None;
                    QuestRewardPreference preference;
                    if (!Enum.TryParse((string)item["RewardPreference"], true, out preference) || !Enum.IsDefined(typeof(QuestRewardPreference), preference)) preference = QuestRewardPreference.EquippedWeapon;
                    bool enabled = item["Enabled"] != null && (bool)item["Enabled"];
                    // v1 stored Enabled + DoNothing even though users reasonably
                    // expected an enabled quest to be handled automatically.
                    if (enabled && action == QuestCompletionAction.None) action = QuestCompletionAction.TurnInAtNpc;
                    Rules[questId] = new QuestAutomationRule
                    {
                        QuestId = questId,
                        DisplayName = (string)item["DisplayName"] ?? "",
                        Enabled = enabled,
                        AutoAccept = item["AutoAccept"] == null || (bool)item["AutoAccept"],
                        AutoTurnIn = item["AutoTurnIn"] == null || (bool)item["AutoTurnIn"],
                        RepeatIfAvailable = item["RepeatIfAvailable"] == null || (bool)item["RepeatIfAvailable"],
                        CompletionAction = action,
                        RewardPreference = preference,
                        PreferredRewardId = ReadUInt(item, "PreferredRewardId"),
                        PreferredWeaponType = (byte)ReadUInt(item, "PreferredWeaponType"),
                        PreferredItemName = (string)item["PreferredItemName"] ?? "",
                        ScriptPath = (string)item["ScriptPath"] ?? ""
                        ,NpcServerName = (string)item["NpcServerName"] ?? ""
                        ,NpcModelId = ReadUInt(item, "NpcModelId")
                        ,NpcRegion = (ushort)ReadUInt(item, "NpcRegion")
                        ,NpcX = ReadInt(item, "NpcX")
                        ,NpcY = ReadInt(item, "NpcY")
                        ,NpcZ = ReadInt(item, "NpcZ")
                    };
                }
            }
        }

        private static QuestAutomationRule Clone(QuestAutomationRule rule)
        {
            return new QuestAutomationRule
            {
                QuestId = rule.QuestId, DisplayName = rule.DisplayName, Enabled = rule.Enabled,
                AutoAccept = rule.AutoAccept, AutoTurnIn = rule.AutoTurnIn,
                RepeatIfAvailable = rule.RepeatIfAvailable,
                RewardPreference = rule.RewardPreference, PreferredRewardId = rule.PreferredRewardId,
                PreferredWeaponType = rule.PreferredWeaponType, PreferredItemName = rule.PreferredItemName,
                CompletionAction = rule.CompletionAction, ScriptPath = rule.ScriptPath
                ,NpcServerName = rule.NpcServerName, NpcModelId = rule.NpcModelId,
                NpcRegion = rule.NpcRegion, NpcX = rule.NpcX, NpcY = rule.NpcY, NpcZ = rule.NpcZ
            };
        }

        private static string ResolveDisplayName(QuestAutomationRule rule, uint questId)
        {
            if (rule != null && !string.IsNullOrWhiteSpace(rule.DisplayName)) return rule.DisplayName;
            try
            {
                System.Collections.Specialized.NameValueCollection data = DataManager.GetQuestData(questId);
                if (data != null && !string.IsNullOrWhiteSpace(data["name"])) return data["name"];
            }
            catch { }
            return "Quest " + questId;
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

        private static int ReadInt(JObject root, string key)
        {
            int value;
            return root[key] != null && int.TryParse(root[key].ToString(), out value) ? value : 0;
        }

        private static void LearnNpc(QuestAutomationRule rule, uint npcUniqueId)
        {
            if (rule == null || npcUniqueId == 0) return;
            SRNpc npc = InfoManager.Npcs[npcUniqueId];
            if (npc == null) return;
            rule.NpcServerName = npc.ServerName ?? "";
            rule.NpcModelId = npc.ID;
            SRCoord position = npc.GetRealtimePosition();
            if (position == null) return;
            rule.NpcRegion = position.Region;
            rule.NpcX = position.X;
            rule.NpcY = position.Y;
            rule.NpcZ = position.Z;
        }

        private static bool TryResolveRewardId(uint questId, out uint rewardId, out string error)
        {
            rewardId = 0;
            error = "";
            List<NameValueCollection> rewards;
            if (!DataManager.TryGetQuestRewardItems(questId, out rewards))
            {
                error = "Ödül kataloğu okunamadı; PK2 veritabanını güncelleyin";
                return false;
            }
            QuestAutomationRule rule = GetRule(questId) ?? new QuestAutomationRule();
            SRItem weapon = InfoManager.Character?.Inventory?[6];
            byte weaponType = rule.RewardPreference == QuestRewardPreference.WeaponType
                ? rule.PreferredWeaponType : weapon == null ? (byte)0 : weapon.ID4;
            if (QuestRewardPolicy.TryResolve(rewards, rule.RewardPreference, rule.PreferredRewardId,
                weaponType, rule.PreferredItemName, out rewardId)) return true;
            error = "Ödül tercihi tek bir eşyayla eşleşmedi; eşya adı veya ödül seçip kaydedin";
            return false;
        }

        private static uint ReadRowUInt(NameValueCollection row, string key)
        {
            uint value;
            return row != null && uint.TryParse(row[key], out value) ? value : 0;
        }

        private static SRNpc FindRequestNpc(QuestAutomationRequest request, SRCoord origin)
        {
            SRNpc best = null;
            double distance = double.MaxValue;
            foreach (SRNpc npc in InfoManager.Npcs.Snapshot())
            {
                if (npc == null || (request.NpcModelId != 0 ? npc.ID != request.NpcModelId
                    : string.IsNullOrWhiteSpace(request.NpcServerName)
                        || !string.Equals(npc.ServerName, request.NpcServerName, StringComparison.OrdinalIgnoreCase))) continue;
                SRCoord position = npc.GetRealtimePosition();
                if (!SameWorld(origin, position)) continue;
                double next = origin.DistanceTo(position);
                if (next < distance) { best = npc; distance = next; }
            }
            return best;
        }

        private static bool SameWorld(SRCoord a, SRCoord b)
        {
            return a != null && b != null && ((!a.inDungeon() && !b.inDungeon()) || a.Region == b.Region);
        }

        private static void ExecuteNpcTurnIn(Bot bot, QuestAutomationRequest request)
        {
            string error;
            if (!ArmQuestTalkSelection(request.QuestId, 0, request.NpcOperation, out error)) return;
            QuestNpcTransaction owner;
            lock (Sync) { owner = Current; States[request.QuestId] = QuestAutomationState.GoingToNpc; }
            try
            {
                SRCoord current = InfoManager.Character?.GetRealtimePosition();
                SRNpc npc = FindRequestNpc(request, current);
                SRCoord target = npc == null ? request.NpcPosition : npc.GetRealtimePosition();
                if (target == null)
                {
                    double distance = double.MaxValue;
                    foreach (NameValueCollection row in DataManager.GetQuestNpcPositions(request.QuestId, request.NpcModelId, request.NpcServerName))
                    {
                        SRCoord candidate = new SRCoord((ushort)ReadRowUInt(row, "region"),
                            int.Parse(row["x"]), int.Parse(row["z"]), int.Parse(row["y"]));
                        if (!SameWorld(current, candidate)) continue;
                        double next = current.DistanceTo(candidate);
                        if (next >= distance) continue;
                        distance = next;
                        target = candidate;
                        request.NpcModelId = ReadRowUInt(row, "model_id");
                        request.NpcServerName = row["servername"];
                    }
                }
                // Only the verified milestone event family uses a generic town merchant.
                NameValueCollection data = DataManager.GetQuestData(request.QuestId);
                if (target == null && (data?["servername"] ?? "").StartsWith("QEV_ALL_BASIC_", StringComparison.OrdinalIgnoreCase))
                {
                    TownServiceInfo service = TownManager.Get.FindNearestService(current, TownServiceType.PotionMerchant);
                    if (service != null) { target = service.Coord; request.NpcModelId = service.NpcId; }
                }
                if (!SameWorld(current, target))
                {
                    lock (Sync) if (Current == owner) FailCurrent("NPC kimliği/konumu çözülemedi veya başka dünyada; PK2 kataloğunu güncelleyin", true);
                    return;
                }
                Status = "Going to NPC: " + request.DisplayName;
                if (current.DistanceTo(target) > 18)
                {
                    // Stop short of the NPC's collision volume; do not walk onto its center.
                    double distance = current.DistanceTo(target);
                    SRCoord stand = new SRCoord(target.PosX + (current.PosX - target.PosX) * 12 / distance,
                        target.PosY + (current.PosY - target.PosY) * 12 / distance, target.Region, target.Z);
                    List<SRCoord> path = NavigationManager.Get.FindPath(current, stand);
                    if (path == null || path.Count == 0)
                    {
                        lock (Sync) if (Current == owner) FailCurrent("NPC için navmesh yolu bulunamadı");
                        return;
                    }
                    foreach (SRCoord point in path)
                    {
                        lock (Sync) if (Current != owner) return;
                        if (!bot.isBotting || !InfoManager.inGame || InfoManager.inTeleport || !bot.WaitMovement(point, 8))
                        {
                            lock (Sync) if (Current == owner) FailCurrent("NPC yürüyüşü kesildi");
                            return;
                        }
                    }
                }
                current = InfoManager.Character?.GetRealtimePosition();
                npc = FindRequestNpc(request, current);
                if (npc == null || current.DistanceTo(npc.GetRealtimePosition()) > 20
                    || !bot.WaitSelectEntity(npc.UniqueID, 10, 250, "Quest NPC seçiliyor..."))
                {
                    lock (Sync) if (Current == owner) FailCurrent("NPC yakınında seçim doğrulanamadı");
                    return;
                }
                lock (Sync)
                {
                    if (Current != owner || !bot.isBotting || !InfoManager.inGame || InfoManager.inTeleport) return;
                    PendingTalkNpcUniqueId = npc.UniqueID;
                    owner.AwaitConfirmation(DateTime.UtcNow);
                    States[request.QuestId] = request.NpcOperation == QuestNpcOperation.Accept ? QuestAutomationState.Accepting : QuestAutomationState.TurningIn;
                    Status = StateText(States[request.QuestId]) + ": " + request.DisplayName;
                    PacketBuilder.TalkNPC(npc.UniqueID, 2);
                }
            }
            catch (Exception ex) { lock (Sync) if (Current == owner) FailCurrent(ex.Message); }
        }

        private static QuestAutomationRequest CreateAutomaticTownQuestRequest()
        {
            if (InfoManager.Character?.Quests == null) return null;
            foreach (QuestAutomationRule rule in GetRules())
            {
                if (!rule.Enabled) continue;
                NameValueCollection data = DataManager.GetQuestData(rule.QuestId);
                if (data == null) continue;
                byte requiredLevel;
                if (byte.TryParse(data["level"], out requiredLevel) && requiredLevel > InfoManager.Character.Level + MaximumLevelAbovePlayer) continue;
                bool eventClaim = (data["servername"] ?? "").StartsWith("QEV_ALL_BASIC_", StringComparison.OrdinalIgnoreCase);
                SRQuest active = InfoManager.Character.Quests[rule.QuestId];
                lock (Sync)
                {
                    DateTime repeatAt;
                    if (RepeatAfterUtc.TryGetValue(rule.QuestId, out repeatAt) && DateTime.UtcNow < repeatAt) continue;
                    QuestNpcTransaction transaction;
                    if (Transactions.TryGetValue(rule.QuestId, out transaction)
                        && (transaction.Terminal || (!transaction.Confirmed && DateTime.UtcNow < transaction.RetryAtUtc))) continue;
                    if (active == null && transaction != null && transaction.Operation == QuestNpcOperation.TurnIn && !transaction.Confirmed) continue;
                    // Return/script completions remain separate from NPC turn-in.
                    if (active != null && (rule.CompletionAction == QuestCompletionAction.ReturnTown
                        || rule.CompletionAction == QuestCompletionAction.RunScript)) continue;
                    QuestNpcOperation? operation = QuestAutomationPolicy.ResolveNpcOperation(active != null,
                        active == null ? (byte)0 : active.State, rule.AutoAccept, rule.AutoTurnIn,
                        CompletedThisSession.Contains(rule.QuestId), rule.RepeatIfAvailable, eventClaim);
                    if (!operation.HasValue) continue;
                    if (operation == QuestNpcOperation.TurnIn && MustWaitForOtherQuests()) continue;
                    if (eventClaim && EventQuestsInTownOnly && !TownManager.Get.IsNearTown(InfoManager.Character.GetRealtimePosition(), 220)) continue;
                    return new QuestAutomationRequest
                    {
                        QuestId = rule.QuestId, DisplayName = ResolveDisplayName(rule, rule.QuestId),
                        Action = QuestCompletionAction.TurnInAtNpc, NpcOperation = operation.Value,
                        NpcModelId = rule.NpcModelId, NpcServerName = rule.NpcServerName,
                        NpcPosition = rule.NpcRegion == 0 ? null : new SRCoord(rule.NpcRegion, rule.NpcX, rule.NpcZ, rule.NpcY)
                    };
                }
            }
            return null;
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Replace("\r", " ").Replace("\n", " ");
        }

        private static string ToHex(byte[] raw)
        {
            if (raw == null || raw.Length == 0) return "";
            return BitConverter.ToString(raw).Replace('-', ' ');
        }
    }
}
