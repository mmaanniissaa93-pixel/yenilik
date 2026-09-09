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
    /// Network handlers only observe state; the bot thread executes actions.
    /// </summary>
    public static class QuestAutomationManager
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<uint, QuestAutomationRule> Rules = new Dictionary<uint, QuestAutomationRule>();
        private static readonly HashSet<string> HandledCompletions = new HashSet<string>();
        private static readonly HashSet<uint> CompletedThisSession = new HashSet<uint>();
        private static readonly HashSet<uint> UnavailableThisSession = new HashSet<uint>();
        private static readonly Queue<QuestAutomationRequest> Pending = new Queue<QuestAutomationRequest>();
        private static readonly Dictionary<uint, DateTime> LastTownQuestAttempts = new Dictionary<uint, DateTime>();
        private static readonly Dictionary<uint, DateTime> LastReturnAttempts = new Dictionary<uint, DateTime>();
        private static uint PendingTalkQuestId;
        private static DateTime PendingTalkExpiresUtc;
        private static uint PendingTalkNpcUniqueId;
        private static bool PendingAutoReward;
        private static QuestNpcOperation PendingNpcOperation;

        public static string Status { get; private set; } = "Hazır";
        public static QuestTalkSnapshot LastTalk { get; private set; }
        public static byte? LastTalkChoice { get; private set; }

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
                PendingTalkQuestId = questId;
                PendingTalkExpiresUtc = DateTime.UtcNow.AddSeconds(10);
                PendingTalkNpcUniqueId = npcUniqueId;
                PendingNpcOperation = operation;
                PendingAutoReward = operation == QuestNpcOperation.TurnIn;
                LearnNpc(Rules.ContainsKey(questId) ? Rules[questId] : null, npcUniqueId);
                Status = (operation == QuestNpcOperation.TurnIn ? "Teslim" : "Kabul")
                    + " menüsü bekleniyor: " + ResolveDisplayName(null, questId);
            }
            return true;
        }

        public static void ObserveTalkPacket(byte[] raw)
        {
            QuestTalkSnapshot snapshot = QuestAutomationPolicy.ParseTalkPacket(raw);
            uint pendingQuestId;
            lock (Sync)
            {
                LastTalk = snapshot;
                pendingQuestId = DateTime.UtcNow <= PendingTalkExpiresUtc ? PendingTalkQuestId : 0;
                if (pendingQuestId == 0)
                {
                    PendingTalkQuestId = 0;
                    PendingTalkNpcUniqueId = 0;
                    PendingAutoReward = false;
                    PendingNpcOperation = QuestNpcOperation.Accept;
                }
                Status = string.IsNullOrWhiteSpace(snapshot.Error)
                    ? "Quest konuşması alındı: tip " + snapshot.Type + ", " + snapshot.Choices.Count + " seçenek"
                    : "Quest konuşması çözülemedi: " + snapshot.Error;
            }
            string details = "[QuestTalk][S->C] type=" + snapshot.Type
                + ", header=" + Safe(snapshot.Header)
                + ", choices=" + string.Join(" | ", snapshot.Choices)
                + (snapshot.ReferenceId == 0 ? "" : ", ref=" + snapshot.ReferenceId)
                + (string.IsNullOrWhiteSpace(snapshot.Error) ? "" : ", error=" + snapshot.Error)
                + ", raw=" + snapshot.RawHex;
            Window.Get?.Log(details, string.IsNullOrWhiteSpace(snapshot.Error)
                ? Theme.LogLevel.Info : Theme.LogLevel.Warning);

            if (pendingQuestId != 0 && string.IsNullOrWhiteSpace(snapshot.Error))
            {
                System.Collections.Specialized.NameValueCollection data = DataManager.GetQuestData(pendingQuestId);
                string nameString = data == null ? "" : data["namestring"];
                int menuPosition = QuestAutomationPolicy.FindTalkChoice(snapshot.Choices, nameString);
                byte actionCode = QuestAutomationPolicy.ResolveTalkActionCode(snapshot.Type, snapshot.Choices, nameString);
                lock (Sync)
                {
                    PendingTalkExpiresUtc = DateTime.MinValue;
                    Status = actionCode != 0
                        ? "Quest NPC seçimi gönderiliyor: " + nameString + " (menü #" + menuPosition + ", kod " + actionCode + ")"
                        : "Quest NPC menüsünde bulunamadı: " + nameString;
                }
                if (actionCode != 0)
                {
                    Window.Get?.Log("[QuestTalk] PK2 eşleşmesi: " + nameString
                        + " -> menu=" + menuPosition + ", action=" + actionCode);
                    PacketBuilder.SelectQuestTalkOption(actionCode);
                }
                else

                {
                    lock (Sync)
                    {
                        UnavailableThisSession.Add(pendingQuestId);
                        PendingTalkQuestId = 0;
                        PendingTalkNpcUniqueId = 0;
                        PendingAutoReward = false;
                    }
                    Window.Get?.Log("[QuestTalk] Açık NPC menüsünde " + nameString + " bulunamadı.", Theme.LogLevel.Warning);

                }
            }
        }

        public static void ObserveQuestIdResponse(uint questId)
        {
            uint requestedQuestId;
            uint npcUniqueId;
            bool autoReward;
            lock (Sync)
            {
                requestedQuestId = PendingTalkQuestId;
                npcUniqueId = PendingTalkNpcUniqueId;
                autoReward = PendingAutoReward;
                PendingTalkQuestId = 0;
                PendingTalkNpcUniqueId = 0;
                PendingAutoReward = false;
            }
            if (!autoReward || requestedQuestId == 0) return;
            if (questId != requestedQuestId)
            {
                Status = "NPC farklı quest döndürdü: beklenen " + requestedQuestId + ", gelen " + questId;
                Window.Get?.Log("Quest: " + Status, Theme.LogLevel.Warning);
                return;
            }
            uint rewardId;
            string rewardError;
            if (!TryResolveRewardId(questId, out rewardId, out rewardError))
            {
                Status = "Quest ödülü otomatik seçilemedi: " + questId + " — " + rewardError;
                Window.Get?.Log("Quest: " + Status, Theme.LogLevel.Warning);
                return;
            }
            Status = rewardId == 0 ? "Sabit quest ödülü alınıyor: " + questId
                : "Quest ödülü alınıyor: " + questId + " / " + rewardId;
            Window.Get?.Log("Quest: " + Status);
            PacketBuilder.ReceiveEventQuestReward(questId, rewardId);
            lock (Sync)
            {
                CompletedThisSession.Add(questId);
                UnavailableThisSession.Remove(questId);
                LastTownQuestAttempts[questId] = DateTime.UtcNow;
            }
            if (npcUniqueId != 0) PacketBuilder.CloseNPC(npcUniqueId);
        }

        public static void ObserveTalkChoice(byte choice, byte[] raw)
        {
            lock (Sync)
            {
                LastTalkChoice = choice;
                Status = "Quest konuşma action kodu gönderildi: " + choice;
            }
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
                if (!rule.Enabled)
                    RemoveHandledKeys(rule.QuestId);
            }
        }

        public static void ResetRules()
        {
            lock (Sync)
            {
                Rules.Clear();
                HandledCompletions.Clear();
                CompletedThisSession.Clear();
                UnavailableThisSession.Clear();
                Pending.Clear();
                LastTownQuestAttempts.Clear();
                LastReturnAttempts.Clear();
                PendingTalkQuestId = 0;
                PendingTalkNpcUniqueId = 0;
                PendingAutoReward = false;
                PendingNpcOperation = QuestNpcOperation.Accept;
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
                    UnavailableThisSession.Remove(quest.ID);
                    if (PendingTalkQuestId == quest.ID && PendingNpcOperation == QuestNpcOperation.Accept)
                    {
                        PendingTalkQuestId = 0;
                        PendingTalkNpcUniqueId = 0;
                        PendingAutoReward = false;
                        Status = "Görev kabul edildi: " + ResolveDisplayName(rule, quest.ID);
                    }
                    return;
                }
                string key = QuestAutomationPolicy.CompletionKey(quest.ID, quest.State);
                if (!rule.AutoTurnIn) return;
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
                    NpcPosition = rule.NpcRegion == 0 ? null : new SRCoord(rule.NpcRegion, rule.NpcX, rule.NpcY, rule.NpcZ),
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
            if (bot == null) return false;
            QuestAutomationRequest request;
            lock (Sync)
            {
                request = Pending.Count == 0 ? null : Pending.Dequeue();
            }
            if (request == null) request = CreateAutomaticTownQuestRequest();
            if (request == null) return false;
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
                    ["ScriptPath"] = rule.ScriptPath
                    ,["NpcServerName"] = rule.NpcServerName
                    ,["NpcModelId"] = rule.NpcModelId
                    ,["NpcRegion"] = rule.NpcRegion
                    ,["NpcX"] = rule.NpcX
                    ,["NpcY"] = rule.NpcY
                    ,["NpcZ"] = rule.NpcZ
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
                CompletedThisSession.Clear();
                UnavailableThisSession.Clear();
                Pending.Clear();
                LastTownQuestAttempts.Clear();
                LastReturnAttempts.Clear();
                PendingTalkQuestId = 0;
                PendingTalkNpcUniqueId = 0;
                PendingAutoReward = false;
                PendingNpcOperation = QuestNpcOperation.Accept;
                Status = "Hazır";
                JArray rules = root == null ? null : root["Rules"] as JArray;
                if (rules == null) return;
                foreach (JObject item in rules)
                {
                    uint questId = ReadUInt(item, "QuestId");
                    if (questId == 0) continue;
                    QuestCompletionAction action;
                    if (!Enum.TryParse((string)item["CompletionAction"], true, out action)) action = QuestCompletionAction.None;
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
            List<NameValueCollection> rewards = DataManager.GetQuestRewardItems(questId);
            // No selectable row, or a single fixed item row: the retail client
            // confirms with selection=0 and does not append an item reference.
            if (rewards.Count <= 1) return true;
            SRItem weapon = InfoManager.Character == null || InfoManager.Character.Inventory == null
                ? null : InfoManager.Character.Inventory[6];
            if (weapon != null)
            {
                foreach (NameValueCollection reward in rewards)
                {
                    byte tid4;
                    if (byte.TryParse(reward["tid4"], out tid4) && tid4 == weapon.ID4)
                    {
                        rewardId = ReadRowUInt(reward, "reward_id");
                        if (rewardId != 0) return true;
                    }
                }
            }
            error = "birden fazla seçilebilir ödül var ve kullanılan silahla eşleşme bulunamadı";
            return false; // never guess among multiple selectable rewards
        }

        private static uint ReadRowUInt(NameValueCollection row, string key)
        {
            uint value;
            return row != null && uint.TryParse(row[key], out value) ? value : 0;
        }

        private static void ExecuteNpcTurnIn(Bot bot, QuestAutomationRequest request)
        {
            SRNpc npc = InfoManager.Npcs.Find(n => n != null
                && ((request.NpcModelId != 0 && n.ID == request.NpcModelId)
                    || (!string.IsNullOrWhiteSpace(request.NpcServerName)
                        && string.Equals(n.ServerName, request.NpcServerName, StringComparison.OrdinalIgnoreCase))));
            SRCoord current = InfoManager.Character == null ? null : InfoManager.Character.GetRealtimePosition();
            SRCoord target = npc == null ? request.NpcPosition : npc.GetRealtimePosition();

            NameValueCollection questData = DataManager.GetQuestData(request.QuestId);
            string questServerName = questData == null ? "" : questData["servername"];
            TownServiceType serviceType;
            bool hasTownService = TryResolveTownService(questData, out serviceType);
            // PK2 NoticeNPC resolves common town quest givers. The level-milestone
            // QEV family is known to use the current town's potion merchant.
            if (npc == null && current != null && (hasTownService
                || (!string.IsNullOrWhiteSpace(questServerName)
                    && questServerName.StartsWith("QEV_ALL_BASIC_", StringComparison.OrdinalIgnoreCase))))
            {
                if (!hasTownService) serviceType = TownServiceType.PotionMerchant;
                TownServiceInfo service = TownManager.Get.FindNearestService(current, serviceType);
                if (service != null)
                {
                    target = service.Coord;
                    npc = TownManager.Get.FindLiveNpc(service) as SRNpc;
                    if (npc != null) target = npc.GetRealtimePosition();
                    request.NpcModelId = service.NpcId;
                    request.NpcServerName = npc == null ? "" : npc.ServerName;
                }
            }
            if (string.IsNullOrWhiteSpace(request.NpcServerName) && request.NpcModelId == 0 && target == null)
            {
                Status = "Quest NPC'si PK2/collision verisinden çözülemedi: " + request.DisplayName;
                Window.Get?.Log("Quest: " + Status, Theme.LogLevel.Warning);
                return;
            }
            if (target == null || current == null)
            {
                Status = "Quest NPC konumu bulunamadı: " + request.DisplayName;
                Window.Get?.Log("Quest: " + Status, Theme.LogLevel.Warning);
                return;
            }
            if (current.DistanceTo(target) > 18.0)
            {
                Status = "Quest NPC'sine yürünüyor: " + request.DisplayName;
                List<SRCoord> path = NavigationManager.Get.FindPath(current, target);
                if (path != null && path.Count > 0)
                    foreach (SRCoord point in path) bot.WaitMovement(point, 8);
                else
                    bot.WaitMovement(target, 12);
            }
            npc = InfoManager.Npcs.Find(n => n != null
                && ((request.NpcModelId != 0 && n.ID == request.NpcModelId)
                    || (!string.IsNullOrWhiteSpace(request.NpcServerName)
                        && string.Equals(n.ServerName, request.NpcServerName, StringComparison.OrdinalIgnoreCase))));
            if (npc == null || !bot.WaitSelectEntity(npc.UniqueID, 10, 250, "Quest NPC seçiliyor..."))
            {
                Status = "Quest NPC'si seçilemedi: " + request.DisplayName;
                Window.Get?.Log("Quest: " + Status, Theme.LogLevel.Warning);
                return;
            }
            string error;
            if (!ArmQuestTalkSelection(request.QuestId, npc.UniqueID, request.NpcOperation, out error))
            {
                Status = error;
                Window.Get?.Log("Quest: " + error, Theme.LogLevel.Warning);
                return;
            }
            Status = (request.NpcOperation == QuestNpcOperation.TurnIn ? "Görev teslim" : "Görev kabul")
                + " konuşması açılıyor: " + request.DisplayName;
            PacketBuilder.TalkNPC(npc.UniqueID, 2);
        }

        private static QuestAutomationRequest CreateAutomaticTownQuestRequest()
        {
            if (InfoManager.Character == null || InfoManager.Character.Quests == null) return null;
            SRCoord current = InfoManager.Character.GetRealtimePosition();
            if (current == null) return null;
            bool isInTown = TownManager.Get.IsNearTown(current, 220.0);
            foreach (QuestAutomationRule rule in GetRules())
            {
                if (!rule.Enabled) continue;
                NameValueCollection data = DataManager.GetQuestData(rule.QuestId);
                string serverName = data == null ? "" : data["servername"];
                bool isEventClaim = !string.IsNullOrWhiteSpace(serverName)
                    && serverName.StartsWith("QEV_ALL_BASIC_", StringComparison.OrdinalIgnoreCase);
                byte requiredLevel;
                if (byte.TryParse(data["level"], out requiredLevel) && requiredLevel > InfoManager.Character.Level) continue;
                SRQuest activeQuest = InfoManager.Character.Quests[rule.QuestId];
                bool completedThisSession;
                lock (Sync)
                {
                    if (UnavailableThisSession.Contains(rule.QuestId)) continue;
                    completedThisSession = CompletedThisSession.Contains(rule.QuestId);
                }
                QuestNpcOperation? resolvedOperation = QuestAutomationPolicy.ResolveNpcOperation(
                    activeQuest != null, activeQuest == null ? (byte)0 : activeQuest.State,
                    rule.AutoAccept, rule.AutoTurnIn, completedThisSession,
                    rule.RepeatIfAvailable, isEventClaim);
                if (!resolvedOperation.HasValue) continue;
                QuestNpcOperation operation = resolvedOperation.Value;
                if (!isInTown)
                {
                    lock (Sync)
                    {
                        DateTime lastReturn;
                        if (LastReturnAttempts.TryGetValue(rule.QuestId, out lastReturn)
                            && DateTime.UtcNow - lastReturn < TimeSpan.FromSeconds(20)) continue;
                        LastReturnAttempts[rule.QuestId] = DateTime.UtcNow;
                    }
                    return new QuestAutomationRequest
                    {
                        QuestId = rule.QuestId,
                        DisplayName = ResolveDisplayName(rule, rule.QuestId),
                        Action = QuestCompletionAction.ReturnTown,
                        NpcOperation = operation
                    };
                }
                bool hasKnownNpc = rule.NpcModelId != 0 || !string.IsNullOrWhiteSpace(rule.NpcServerName)
                    || rule.NpcRegion != 0;
                TownServiceType ignoredService;
                bool canResolveTownNpc = TryResolveTownService(data, out ignoredService);
                if (!hasKnownNpc && !isEventClaim && !canResolveTownNpc)
                {
                    Status = "Görev NPC'si henüz öğrenilmedi: " + ResolveDisplayName(rule, rule.QuestId)
                        + " (bir kez NPC yanında 'Select at NPC' kullanın)";
                    continue;
                }
                lock (Sync)
                {
                    DateTime last;
                    if (LastTownQuestAttempts.TryGetValue(rule.QuestId, out last)
                        && DateTime.UtcNow - last < TimeSpan.FromMinutes(2)) continue;
                    LastTownQuestAttempts[rule.QuestId] = DateTime.UtcNow;
                }
                return new QuestAutomationRequest
                {
                    QuestId = rule.QuestId,
                    DisplayName = ResolveDisplayName(rule, rule.QuestId),
                    Action = QuestCompletionAction.TurnInAtNpc,
                    NpcServerName = rule.NpcServerName,
                    NpcModelId = rule.NpcModelId,
                    NpcPosition = rule.NpcRegion == 0 ? null : new SRCoord(rule.NpcRegion, rule.NpcX, rule.NpcY, rule.NpcZ),
                    NpcOperation = operation
                };
            }
            return null;
        }

        private static bool TryResolveTownService(NameValueCollection questData, out TownServiceType serviceType)
        {
            serviceType = TownServiceType.PotionMerchant;
            if (questData == null) return false;
            string notice = (questData["notice_npc"] ?? "").ToUpperInvariant();
            if (notice.Contains("POTION") || notice.Contains("HERBALIST") || notice.Contains("ALCHEMY"))
            {
                serviceType = TownServiceType.PotionMerchant;
                return true;
            }
            if (notice.Contains("BLACKSMITH") || notice.Contains("SMITH")
                || notice.Contains("WEAPON") || notice.Contains("ARMOR"))
            {
                serviceType = TownServiceType.Blacksmith;
                return true;
            }
            if (notice.Contains("STORAGE") || notice.Contains("WAREHOUSE"))
            {
                serviceType = TownServiceType.Storage;
                return true;
            }
            if (notice.Contains("GROCERY") || notice.Contains("ACCESSORY"))
            {
                serviceType = TownServiceType.GroceryMerchant;
                return true;
            }
            return false;
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
