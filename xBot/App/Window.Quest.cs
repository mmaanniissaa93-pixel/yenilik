using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using xBot.App.Theme;
using xBot.Game;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    public partial class Window
    {
        public Panel TabPageV_Control01_Quest_Panel;
        private QuestPanel questPanel;
        private Timer _questUiTimer;
        private DateTime _lastQuestUiRefresh;
        private string questSearch = "";
        private string lastQuestStatus = "";

        private void BuildQuestTab()
        {
            if (pnlWindow == null || TabPageV_Control01_Quest_Panel != null) return;
            TabPageV_Control01_Quest_Panel = new Panel { Name = "TabPageV_Control01_Quest_Panel",
                Location = TabPageV_Control01_Town_Panel.Location, Size = TabPageV_Control01_Town_Panel.Size, Visible = false };
            questPanel = new QuestPanel { Dock = DockStyle.Fill };
            TabPageV_Control01_Quest_Panel.Controls.Add(questPanel); pnlWindow.Controls.Add(TabPageV_Control01_Quest_Panel);
            questPanel.RefreshRequested += RefreshQuestAutomationUi;
            questPanel.SearchRequested += query => { questSearch = query; RefreshQuestCatalog(); };
            questPanel.DetailsRequested += ShowQuestDetails;
            questPanel.MenuRequested += BuildQuestMenu;
            questPanel.ResetRequested += () => {
                if (MessageBox.Show(this, "Görev otomasyon tercihleri sıfırlansın mı? Aktif görevler bırakılmaz.", "Görev tercihlerini sıfırla", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                QuestAutomationManager.ResetRules(); Settings.SaveCharacterSettings(); RefreshQuestAutomationUi();
            };
            questPanel.OptionsChanged += () => {
                QuestAutomationManager.SetOptions(questPanel.LevelMargin, questPanel.WaitForAll, questPanel.TownEventsOnly);
                Settings.SaveCharacterSettings(); questPanel.SetStatus("Seçenekler kaydedildi.");
            };
            _questUiTimer = new Timer { Interval = 1000 };
            _questUiTimer.Tick += (s, e) => {
                QuestAutomationManager.PollTimeout();
                if (!questPanel.Visible) return;
                if (_lastQuestUiRefresh != InfoManager.LastQuestUpdateTime) RefreshQuestList();
                foreach (var grid in new[] { questPanel.ActiveGrid, questPanel.CatalogGrid })
                    foreach (DataGridViewRow row in grid.Rows) row.Cells["State"].Value = QuestStateLabel((uint)row.Tag);
                if (lastQuestStatus != QuestAutomationManager.Status)
                {
                    lastQuestStatus = QuestAutomationManager.Status;
                    questPanel.SetStatus(lastQuestStatus.StartsWith("Failed:", StringComparison.Ordinal)
                        ? "Görev işlemi tamamlanamadı. Açıklamayı görmek için göreve çift tıklayın."
                        : QuestAutomationManager.IsBusy ? "Görev otomasyonu çalışıyor. İlerlemeyi Durum sütunundan izleyebilirsiniz."
                        : "Görev durumları güncellendi.");
                }
            };
            questPanel.Disposed += (s, e) => { _questUiTimer.Stop(); _questUiTimer.Dispose(); };
            _questUiTimer.Start(); RefreshQuestAutomationUi();
        }

        public void RefreshQuestAutomationUi()
        {
            if (questPanel == null) return;
            questPanel.SetOptions(QuestAutomationManager.MaximumLevelAbovePlayer, QuestAutomationManager.WaitForAllEnabledQuests, QuestAutomationManager.EventQuestsInTownOnly);
            RefreshQuestList(); RefreshQuestCatalog();
        }

        private void RefreshQuestList()
        {
            var entries = new List<QuestListEntry>();
            if (InfoManager.Character?.Quests != null)
                foreach (SRQuest quest in InfoManager.Character.Quests.Snapshot())
                {
                    var entry = QuestEntry(quest.ID, DataManager.GetQuestData(quest.ID));
                    if (string.IsNullOrWhiteSpace(entry.Name)) entry.Name = quest.Name;
                    entry.Objectives = QuestObjectives(quest); entries.Add(entry);
                }
            questPanel.SetActive(entries); _lastQuestUiRefresh = InfoManager.LastQuestUpdateTime;
        }
        private void RefreshQuestCatalog()
        {
            var entries = new List<QuestListEntry>();
            foreach (var data in DataManager.QueryQuests(questSearch))
            {
                uint id; if (uint.TryParse(data["id"], out id)) entries.Add(QuestEntry(id, data));
            }
            questPanel.SetCatalog(entries, DataManager.IsQuestCatalogAvailable());
            questPanel.SetStatus(entries.Count + " görev  ·  Sağ tıkla otomasyonu etkinleştirin; kabul ve teslim otomatik yürütülür.");
        }
        private QuestListEntry QuestEntry(uint id, NameValueCollection data)
        {
            QuestAutomationRule rule = QuestAutomationManager.GetRule(id);
            int level; int.TryParse(data?["level"], out level);
            return new QuestListEntry { Id = id, Name = !string.IsNullOrWhiteSpace(rule?.DisplayName) ? rule.DisplayName : data?["name"] ?? ("Görev " + id),
                Npc = CleanQuestText(data?["notice_npc_text"], "NPC bilgisi yok"), State = QuestStateLabel(id), Level = level, Enabled = rule?.Enabled == true,
                Completion = rule?.CompletionAction == QuestCompletionAction.ReturnTown ? "Şehre dön" : rule?.CompletionAction == QuestCompletionAction.RunScript ? "Script çalıştır" : "NPC'ye teslim et" };
        }
        private static string QuestStateLabel(uint id)
        {
            switch (QuestAutomationManager.GetStateText(id))
            {
                case "Inactive": return "Alınmadı"; case "Going to NPC": return "NPC'ye gidiyor";
                case "Accepting": return "Kabul ediliyor"; case "Active": return "Devam ediyor";
                case "Completed": return "Tamamlandı"; case "Turning in": return "Teslim ediliyor";
                case "Waiting to repeat": return "Tekrar bekliyor"; case "Failed": return "İşlem başarısız";
                default: return QuestAutomationManager.GetStateText(id);
            }
        }
        private static string QuestObjectives(SRQuest quest)
        {
            var names = new List<string>();
            if (quest?.Objectives != null)
                for (int i = 0; i < quest.Objectives.Capacity; i++)
                    if (!string.IsNullOrWhiteSpace(quest.Objectives[i]?.Name)) names.Add("• " + quest.Objectives[i].Name);
            return string.Join("\n", names);
        }
        private void ChangeQuest(uint id, Action<QuestAutomationRule> update)
        {
            QuestAutomationRule rule = QuestAutomationManager.GetRule(id) ?? new QuestAutomationRule { QuestId = id };
            update(rule); QuestAutomationManager.SaveRule(rule); Settings.SaveCharacterSettings(); RefreshQuestAutomationUi();
        }
        private static ToolStripMenuItem QuestMenuItem(ToolStripItemCollection items, string text, Action action, bool check = false)
        {
            var item = new ToolStripMenuItem(text) { Checked = check };
            item.Click += (s, e) => action(); items.Add(item); return item;
        }
        private void BuildQuestMenu(uint id, ContextMenuStrip menu)
        {
            var rule = QuestAutomationManager.GetRule(id) ?? new QuestAutomationRule { QuestId = id };
            QuestMenuItem(menu.Items, "Görevi görüntüle", () => ShowQuestDetails(id));
            if (InfoManager.Character?.Quests?[id] != null)
                QuestMenuItem(menu.Items, "Görevi bırak", () => {
                    if (InfoManager.inGame && InfoManager.Character?.Quests?[id] != null) PacketBuilder.AbandonQuest(id);
                });
            menu.Items.Add(new ToolStripSeparator());
            QuestMenuItem(menu.Items, "Otomasyonu etkinleştir", () => ChangeQuest(id, r => { r.Enabled = true; r.AutoAccept = r.AutoTurnIn = true; }), rule.Enabled);
            QuestMenuItem(menu.Items, "Otomasyonu kapat", () => ChangeQuest(id, r => r.Enabled = false), !rule.Enabled);
            var completion = new ToolStripMenuItem("Tamamlanınca"); menu.Items.Add(completion);
            QuestMenuItem(completion.DropDownItems, "NPC'ye teslim et (otomatik)", () => ChangeQuest(id, r => { r.CompletionAction = QuestCompletionAction.TurnInAtNpc; r.ScriptPath = ""; }), rule.CompletionAction == QuestCompletionAction.None || rule.CompletionAction == QuestCompletionAction.TurnInAtNpc);
            QuestMenuItem(completion.DropDownItems, "Şehre dön", () => ChangeQuest(id, r => r.CompletionAction = QuestCompletionAction.ReturnTown), rule.CompletionAction == QuestCompletionAction.ReturnTown);
            QuestMenuItem(completion.DropDownItems, "Script çalıştır…", () => {
                using (var dialog = new OpenFileDialog { Filter = "Yürüyüş scripti (*.txt;*.xcript;*.rbs)|*.txt;*.xcript;*.rbs|Tüm dosyalar|*.*" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) ChangeQuest(id, r => { r.ScriptPath = dialog.FileName; r.CompletionAction = QuestCompletionAction.RunScript; });
            }, rule.CompletionAction == QuestCompletionAction.RunScript);
            QuestMenuItem(completion.DropDownItems, "Scripti kaldır", () => ChangeQuest(id, r => { r.ScriptPath = ""; r.CompletionAction = QuestCompletionAction.TurnInAtNpc; })).Enabled = !string.IsNullOrWhiteSpace(rule.ScriptPath);
            QuestMenuItem(menu.Items, "Sunucu tekrar sunarsa yeniden al", () => ChangeQuest(id, r => r.RepeatIfAvailable = !r.RepeatIfAvailable), rule.RepeatIfAvailable);
            var rewards = new ToolStripMenuItem("Ödül tercihi"); menu.Items.Add(rewards);
            QuestMenuItem(rewards.DropDownItems, "Takılı silaha göre (otomatik)", () => ChangeQuest(id, r => r.RewardPreference = QuestRewardPreference.EquippedWeapon), rule.RewardPreference == QuestRewardPreference.EquippedWeapon);
            var weaponMenu = new ToolStripMenuItem("Silah türü"); rewards.DropDownItems.Add(weaponMenu);
            foreach (SRTypes.Weapon weapon in Enum.GetValues(typeof(SRTypes.Weapon)))
            {
                if (weapon == SRTypes.Weapon.None) continue;
                var choice = weapon;
                QuestMenuItem(weaponMenu.DropDownItems, weapon.ToString(), () => ChangeQuest(id, r => { r.RewardPreference = QuestRewardPreference.WeaponType; r.PreferredWeaponType = (byte)choice; }), rule.RewardPreference == QuestRewardPreference.WeaponType && rule.PreferredWeaponType == (byte)weapon);
            }
            foreach (var reward in DataManager.GetQuestRewardItems(id))
            {
                uint rewardId; if (!uint.TryParse(reward["reward_id"], out rewardId) || rewardId == 0) continue;
                uint chosen = rewardId;
                QuestMenuItem(rewards.DropDownItems, reward["name"] ?? reward["item_servername"], () => ChangeQuest(id, r => { r.RewardPreference = QuestRewardPreference.RewardId; r.PreferredRewardId = chosen; }), rule.RewardPreference == QuestRewardPreference.RewardId && rule.PreferredRewardId == chosen);
            }
        }
        private static string CleanQuestText(string text, string fallback = "Bilgi bulunmuyor")
        {
            if (string.IsNullOrWhiteSpace(text) || text == "xxx" || text.StartsWith("SN_", StringComparison.OrdinalIgnoreCase)) return fallback;
            return text.Replace("\\n", "\n").Replace("\\r", "").Trim();
        }
        private void ShowQuestDetails(uint id)
        {
            var data = DataManager.GetQuestData(id); var entry = QuestEntry(id, data);
            var sections = new List<KeyValuePair<string, string>>();
            Action<string, string> section = (title, value) => sections.Add(new KeyValuePair<string, string>(title, value));
                section(entry.Name, "Seviye " + entry.Level + "  ·  " + entry.State);
                string failure = QuestAutomationManager.GetFailureReason(id);
                if (!string.IsNullOrWhiteSpace(failure)) section("İşlem bilgisi", failure);
                section("Görev", CleanQuestText(data?["mission_text"], CleanQuestText(QuestObjectives(InfoManager.Character?.Quests?[id]), "Görev açıklaması katalogda bulunmuyor.")));
                var itemRewards = DataManager.GetQuestRewardItems(id).Select(r => (r["amount"] ?? "1") + " × " + (r["name"] ?? r["item_servername"])).ToList();
                string rewardText = CleanQuestText(data?["reward_text"], "");
                if (itemRewards.Count > 0) rewardText += (rewardText.Length > 0 ? "\n" : "") + string.Join("\n", itemRewards);
                section("Ödüller", rewardText.Length > 0 ? rewardText : "Ödül bilgisi katalogda bulunmuyor.");
                section("NPC", entry.Npc);
                section("Teslim koşulu", CleanQuestText(data?["completion_text"], "Sunucunun görev durumuna göre belirlenir."));
                section("İlerleme", CleanQuestText(QuestObjectives(InfoManager.Character?.Quests?[id]), "Aktif hedef bilgisi yok."));
            using (var dialog = QuestPanel.CreateDetailsDialog(entry.Name, sections)) dialog.ShowDialog(this);
        }
    }
}
