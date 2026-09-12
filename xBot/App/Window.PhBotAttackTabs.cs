using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using xBot.Game;
using xBot.Game.Objects.Entity;
using xBot.Network;

namespace xBot.App
{
    public partial class Window
    {
        // Controls for Party Buffs Tab
        private ListBox PhBot_Party_BuffSkillsList;
        private ListView PhBot_Party_AssignmentsList;
        private Button PhBot_Party_BtnTransfer;
        private Button PhBot_Party_BtnRemove;
        private Button PhBot_Party_BtnAdd;

        // Controls for Party Options Tab
        private TextBox PhBot_PartyOpt_RadiusText;
        private CheckBox PhBot_PartyOpt_BuffAndReturn;
        private CheckBox PhBot_PartyOpt_BuffNearbyPlayers;
        private RadioButton PhBot_PartyOpt_RadioStart;
        private RadioButton PhBot_PartyOpt_RadioStop;
        private ListBox PhBot_PartyOpt_PlayersList;
        private Button PhBot_PartyOpt_BtnRemove;
        private Button PhBot_PartyOpt_BtnAdd;

        // Controls for Resurrect Tab
        private ListBox PhBot_Res_RegexList;
        private ListBox PhBot_Res_NearbyList;
        private Button PhBot_Res_BtnTransfer;
        private Button PhBot_Res_BtnRemove;
        private Button PhBot_Res_BtnAdd;
        private TextBox PhBot_Res_RadiusText;
        private TextBox PhBot_Res_DelayText;
        private CheckBox PhBot_Res_AllPartyCheck;
        private CheckBox PhBot_Res_RetryCheck;
        private TextBox PhBot_Res_RetryText;
        private CheckBox PhBot_Res_AcceptOthersCheck;
        private CheckBox PhBot_Res_AcceptPartyOnlyCheck;
        private ListBox PhBot_Res_SkillsList;

        // Controls for Lure Tab
        private TextBox PhBot_Lure_WalkDistanceText;
        private ComboBox PhBot_Lure_SkillCombo;
        private CheckBox PhBot_Lure_BuffAtEndCheck;
        private CheckBox PhBot_Lure_StopIfPartyDeadCheck;
        private CheckBox PhBot_Lure_TownIfPartyLessCheck;
        private TextBox PhBot_Lure_TownIfPartyLessCount;
        private CheckBox PhBot_Lure_StopGiantPartyCheck;
        private TextBox PhBot_Lure_StopGiantPartyCount;
        private CheckBox PhBot_Lure_StopMobCountCheck;
        private TextBox PhBot_Lure_StopMobCount;
        private CheckBox PhBot_Lure_AttackLimitCheck;
        private TextBox PhBot_Lure_AttackLimitText;
        private TextBox PhBot_Lure_EdgeDelayText;
        private TextBox PhBot_Lure_CenterDelayText;
        private TextBox PhBot_Lure_HalfDelayText;
        private TextBox PhBot_Lure_ScriptPathText;
        private Button PhBot_Lure_BtnLoadScript;
        private CheckBox PhBot_Lure_RandomWalkCheck;
        private CheckBox PhBot_Lure_SpawnsCheck;
        private CheckBox PhBot_Lure_SmartCheck;
        private CheckBox PhBot_Lure_StopIfPartyAwayCheck;
        private TextBox PhBot_Lure_PartyAwayDistText;
        private ListBox PhBot_Lure_Whitelist;
        private Button PhBot_Lure_BtnRemove;
        private Button PhBot_Lure_BtnAdd;
        private CheckBox PhBot_Lure_BuffAfterScriptCheck;

        #region TAB 2: PARTI (Party Buffs)
        public void BuildPhBotPartyTab(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.FromArgb(245, 247, 251);
            p.AutoScroll = true;

            // Left: Party Buffs ListBox
            GroupBox grpBuffs = new GroupBox
            {
                Text = "Parti Buffları",
                Location = new Point(12, 10),
                Size = new Size(250, 390),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            PhBot_Party_BuffSkillsList = new ListBox
            {
                Location = new Point(10, 24),
                Size = new Size(230, 350),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                IntegralHeight = false
            };
            grpBuffs.Controls.Add(PhBot_Party_BuffSkillsList);
            p.Controls.Add(grpBuffs);

            // Middle buttons: Transfer, Remove, Add
            PhBot_Party_BtnTransfer = new Button
            {
                Text = "◄",
                Location = new Point(270, 70),
                Size = new Size(35, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Party_BtnTransfer.Click += (s, e) =>
            {
                PromptAddPartyBuffAssignment();
            };
            p.Controls.Add(PhBot_Party_BtnTransfer);

            PhBot_Party_BtnRemove = new Button
            {
                Text = "K",
                Location = new Point(270, 110),
                Size = new Size(35, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Party_BtnRemove.Click += (s, e) =>
            {
                if (PhBot_Party_AssignmentsList.SelectedItems.Count > 0)
                {
                    int idx = PhBot_Party_AssignmentsList.SelectedIndices[0];
                    if (idx >= 0 && idx < PartySupportManager.PartyBuffAssignments.Count)
                    {
                        PartySupportManager.PartyBuffAssignments.RemoveAt(idx);
                        RefreshPartyAssignmentsList();
                    }
                }
            };
            p.Controls.Add(PhBot_Party_BtnRemove);

            PhBot_Party_BtnAdd = new Button
            {
                Text = "E",
                Location = new Point(270, 150),
                Size = new Size(35, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Party_BtnAdd.Click += (s, e) =>
            {
                PromptAddPartyBuffAssignment();
            };
            p.Controls.Add(PhBot_Party_BtnAdd);

            // Right: Party buff assignments ListView
            GroupBox grpAssignments = new GroupBox
            {
                Text = "Oyuncu Buff Atamaları",
                Location = new Point(315, 10),
                Size = new Size(420, 390),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            PhBot_Party_AssignmentsList = new ListView
            {
                Location = new Point(10, 24),
                Size = new Size(400, 350),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Party_AssignmentsList.Columns.Add("Oyuncu Adı", 130);
            PhBot_Party_AssignmentsList.Columns.Add("Buff Skili", 250);
            grpAssignments.Controls.Add(PhBot_Party_AssignmentsList);
            p.Controls.Add(grpAssignments);

            PopulatePartyBuffSkills();
            RefreshPartyAssignmentsList();
        }

        private void PromptAddPartyBuffAssignment()
        {
            string player = PromptInput("Oyuncu Adı Girin:", "Parti Buff Ekle");
            if (string.IsNullOrWhiteSpace(player)) return;

            string skill = PhBot_Party_BuffSkillsList?.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(skill))
            {
                skill = PromptInput("Buff Skili Adı Girin:", "Buff Skili");
            }
            if (string.IsNullOrWhiteSpace(skill)) return;

            PartySupportManager.PartyBuffAssignments.Add(new PartyBuffAssignment { PlayerName = player.Trim(), SkillName = skill.Trim() });
            RefreshPartyAssignmentsList();
        }

        private void RefreshPartyAssignmentsList()
        {
            if (PhBot_Party_AssignmentsList == null) return;
            PhBot_Party_AssignmentsList.Items.Clear();
            foreach (var item in PartySupportManager.PartyBuffAssignments)
            {
                var lvi = new ListViewItem(item.PlayerName);
                lvi.SubItems.Add(item.SkillName);
                PhBot_Party_AssignmentsList.Items.Add(lvi);
            }
        }

        private void PopulatePartyBuffSkills()
        {
            if (PhBot_Party_BuffSkillsList == null) return;
            PhBot_Party_BuffSkillsList.Items.Clear();

            try
            {
                if (Skills_lstvSkills != null)
                {
                    foreach (ListViewItem it in Skills_lstvSkills.Items)
                    {
                        if (it != null && !string.IsNullOrEmpty(it.Text))
                        {
                            if (!PhBot_Party_BuffSkillsList.Items.Contains(it.Text))
                                PhBot_Party_BuffSkillsList.Items.Add(it.Text);
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        #region TAB 3: PARTI AYARLARI (Party Options)
        public void BuildPhBotPartyOptionsTab(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.FromArgb(245, 247, 251);
            p.AutoScroll = true;

            GroupBox grpSettings = new GroupBox
            {
                Text = "Ayarlar",
                Location = new Point(12, 10),
                Size = new Size(720, 390),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            Label lblRadius = new Label
            {
                Text = "Buff yarıçapı:",
                Location = new Point(15, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            grpSettings.Controls.Add(lblRadius);

            PhBot_PartyOpt_RadiusText = new TextBox
            {
                Text = PartySupportManager.PartyBuffRadius.ToString(),
                Location = new Point(110, 27),
                Size = new Size(50, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_PartyOpt_RadiusText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_PartyOpt_RadiusText.Text, out int r))
                    PartySupportManager.PartyBuffRadius = r;
            };
            grpSettings.Controls.Add(PhBot_PartyOpt_RadiusText);

            Label lblM = new Label
            {
                Text = "m",
                Location = new Point(165, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            grpSettings.Controls.Add(lblM);

            PhBot_PartyOpt_BuffAndReturn = new CheckBox
            {
                Text = "Buffla ve merkeze geri dön",
                Location = new Point(18, 62),
                AutoSize = true,
                Checked = PartySupportManager.BuffAndReturnToCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_PartyOpt_BuffAndReturn.CheckedChanged += (s, e) =>
            {
                PartySupportManager.BuffAndReturnToCenter = PhBot_PartyOpt_BuffAndReturn.Checked;
            };
            grpSettings.Controls.Add(PhBot_PartyOpt_BuffAndReturn);

            PhBot_PartyOpt_BuffNearbyPlayers = new CheckBox
            {
                Text = "Yakındaki oyuncuları buffla",
                Location = new Point(18, 92),
                AutoSize = true,
                Checked = PartySupportManager.BuffAllNearbyPlayers,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_PartyOpt_BuffNearbyPlayers.CheckedChanged += (s, e) =>
            {
                PartySupportManager.BuffAllNearbyPlayers = PhBot_PartyOpt_BuffNearbyPlayers.Checked;
            };
            grpSettings.Controls.Add(PhBot_PartyOpt_BuffNearbyPlayers);

            Panel pnlRadio = new Panel
            {
                Location = new Point(35, 120),
                Size = new Size(400, 30)
            };
            PhBot_PartyOpt_RadioStart = new RadioButton
            {
                Text = "Yakındaysa botu başlat",
                Location = new Point(0, 5),
                AutoSize = true,
                Checked = PartySupportManager.BuffProximityStart,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_PartyOpt_RadioStart.CheckedChanged += (s, e) =>
            {
                PartySupportManager.BuffProximityStart = PhBot_PartyOpt_RadioStart.Checked;
            };
            pnlRadio.Controls.Add(PhBot_PartyOpt_RadioStart);

            PhBot_PartyOpt_RadioStop = new RadioButton
            {
                Text = "Yakındaysa botu durdur",
                Location = new Point(180, 5),
                AutoSize = true,
                Checked = PartySupportManager.BuffProximityStop,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_PartyOpt_RadioStop.CheckedChanged += (s, e) =>
            {
                PartySupportManager.BuffProximityStop = PhBot_PartyOpt_RadioStop.Checked;
            };
            pnlRadio.Controls.Add(PhBot_PartyOpt_RadioStop);
            grpSettings.Controls.Add(pnlRadio);

            PhBot_PartyOpt_PlayersList = new ListBox
            {
                Location = new Point(35, 155),
                Size = new Size(260, 200),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                IntegralHeight = false
            };
            grpSettings.Controls.Add(PhBot_PartyOpt_PlayersList);

            PhBot_PartyOpt_BtnRemove = new Button
            {
                Text = "K",
                Location = new Point(305, 175),
                Size = new Size(35, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_PartyOpt_BtnRemove.Click += (s, e) =>
            {
                if (PhBot_PartyOpt_PlayersList.SelectedIndex >= 0)
                {
                    string sel = PhBot_PartyOpt_PlayersList.SelectedItem as string;
                    if (sel != null)
                    {
                        PartySupportManager.BuffProximityPlayers.Remove(sel);
                        RefreshPartyOptPlayersList();
                    }
                }
            };
            grpSettings.Controls.Add(PhBot_PartyOpt_BtnRemove);

            PhBot_PartyOpt_BtnAdd = new Button
            {
                Text = "E",
                Location = new Point(305, 215),
                Size = new Size(35, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_PartyOpt_BtnAdd.Click += (s, e) =>
            {
                string pName = PromptInput("Oyuncu Adı:", "Yakın Oyuncu Listesine Ekle");
                if (!string.IsNullOrWhiteSpace(pName))
                {
                    pName = pName.Trim();
                    if (!PartySupportManager.BuffProximityPlayers.Contains(pName))
                    {
                        PartySupportManager.BuffProximityPlayers.Add(pName);
                        RefreshPartyOptPlayersList();
                    }
                }
            };
            grpSettings.Controls.Add(PhBot_PartyOpt_BtnAdd);

            p.Controls.Add(grpSettings);
            RefreshPartyOptPlayersList();
        }

        private void RefreshPartyOptPlayersList()
        {
            if (PhBot_PartyOpt_PlayersList == null) return;
            PhBot_PartyOpt_PlayersList.Items.Clear();
            foreach (var pl in PartySupportManager.BuffProximityPlayers)
                PhBot_PartyOpt_PlayersList.Items.Add(pl);
        }
        #endregion

        #region TAB 4: CANLANDIR (Resurrect)
        public void BuildPhBotResurrectTab(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.FromArgb(245, 247, 251);
            p.AutoScroll = true;

            PhBot_Res_RegexList = new ListBox
            {
                Location = new Point(15, 15),
                Size = new Size(180, 360),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                IntegralHeight = false
            };
            p.Controls.Add(PhBot_Res_RegexList);

            PhBot_Res_BtnTransfer = new Button
            {
                Text = "◄",
                Location = new Point(203, 60),
                Size = new Size(32, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Res_BtnTransfer.Click += (s, e) =>
            {
                if (PhBot_Res_NearbyList.SelectedItem is string sel && !string.IsNullOrWhiteSpace(sel))
                {
                    if (!ResurrectPolicy.ResurrectRegexList.Contains(sel))
                    {
                        ResurrectPolicy.ResurrectRegexList.Add(sel);
                        RefreshResurrectRegexList();
                    }
                }
            };
            p.Controls.Add(PhBot_Res_BtnTransfer);

            PhBot_Res_BtnRemove = new Button
            {
                Text = "K",
                Location = new Point(203, 95),
                Size = new Size(32, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Res_BtnRemove.Click += (s, e) =>
            {
                if (PhBot_Res_RegexList.SelectedItem is string sel)
                {
                    ResurrectPolicy.ResurrectRegexList.Remove(sel);
                    RefreshResurrectRegexList();
                }
            };
            p.Controls.Add(PhBot_Res_BtnRemove);

            PhBot_Res_BtnAdd = new Button
            {
                Text = "E",
                Location = new Point(203, 130),
                Size = new Size(32, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Res_BtnAdd.Click += (s, e) =>
            {
                string r = PromptInput("Canlandırılacak Oyuncu / Regex:", "Canlandırma Listesine Ekle");
                if (!string.IsNullOrWhiteSpace(r))
                {
                    r = r.Trim();
                    if (!ResurrectPolicy.ResurrectRegexList.Contains(r))
                    {
                        ResurrectPolicy.ResurrectRegexList.Add(r);
                        RefreshResurrectRegexList();
                    }
                }
            };
            p.Controls.Add(PhBot_Res_BtnAdd);

            PhBot_Res_NearbyList = new ListBox
            {
                Location = new Point(243, 15),
                Size = new Size(180, 360),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                IntegralHeight = false
            };
            p.Controls.Add(PhBot_Res_NearbyList);

            Label lblRad = new Label
            {
                Text = "Canlandırma yarıçapı:",
                Location = new Point(435, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(lblRad);

            PhBot_Res_RadiusText = new TextBox
            {
                Text = ResurrectPolicy.ResurrectRange.ToString(),
                Location = new Point(565, 15),
                Size = new Size(45, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Res_RadiusText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Res_RadiusText.Text, out int rad))
                    ResurrectPolicy.ResurrectRange = rad;
            };
            p.Controls.Add(PhBot_Res_RadiusText);

            Label lblRadM = new Label
            {
                Text = "m",
                Location = new Point(615, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(lblRadM);

            Label lblDel = new Label
            {
                Text = "Canlandırma gecikmesi:",
                Location = new Point(435, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(lblDel);

            PhBot_Res_DelayText = new TextBox
            {
                Text = ResurrectPolicy.ResurrectDelayMs.ToString(),
                Location = new Point(565, 45),
                Size = new Size(60, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Res_DelayText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Res_DelayText.Text, out int del))
                    ResurrectPolicy.ResurrectDelayMs = del;
            };
            p.Controls.Add(PhBot_Res_DelayText);

            Label lblDelMs = new Label
            {
                Text = "ms",
                Location = new Point(630, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(lblDelMs);

            PhBot_Res_AllPartyCheck = new CheckBox
            {
                Text = "Bütün parti üyelerini canlandır",
                Location = new Point(438, 78),
                AutoSize = true,
                Checked = ResurrectPolicy.ResurrectAllParty,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Res_AllPartyCheck.CheckedChanged += (s, e) =>
            {
                ResurrectPolicy.ResurrectAllParty = PhBot_Res_AllPartyCheck.Checked;
            };
            p.Controls.Add(PhBot_Res_AllPartyCheck);

            PhBot_Res_RetryCheck = new CheckBox
            {
                Text = "Yeniden dene",
                Location = new Point(438, 108),
                AutoSize = true,
                Checked = ResurrectPolicy.RetryLimit > 0,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(PhBot_Res_RetryCheck);

            PhBot_Res_RetryText = new TextBox
            {
                Text = ResurrectPolicy.RetryLimit.ToString(),
                Location = new Point(540, 105),
                Size = new Size(35, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Res_RetryText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Res_RetryText.Text, out int lim))
                    ResurrectPolicy.RetryLimit = lim;
            };
            PhBot_Res_RetryCheck.CheckedChanged += (s, e) =>
            {
                if (!PhBot_Res_RetryCheck.Checked) ResurrectPolicy.RetryLimit = 0;
                else if (int.TryParse(PhBot_Res_RetryText.Text, out int lim)) ResurrectPolicy.RetryLimit = lim;
            };
            p.Controls.Add(PhBot_Res_RetryText);

            Label lblRetTimes = new Label
            {
                Text = "kere",
                Location = new Point(580, 108),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(lblRetTimes);

            PhBot_Res_AcceptOthersCheck = new CheckBox
            {
                Text = "Başka oyunculardan gelen resi kabul et",
                Location = new Point(438, 138),
                AutoSize = true,
                Checked = ResurrectPolicy.AcceptFromOthers,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Res_AcceptOthersCheck.CheckedChanged += (s, e) =>
            {
                ResurrectPolicy.AcceptFromOthers = PhBot_Res_AcceptOthersCheck.Checked;
            };
            p.Controls.Add(PhBot_Res_AcceptOthersCheck);

            PhBot_Res_AcceptPartyOnlyCheck = new CheckBox
            {
                Text = "Sadece parti",
                Location = new Point(458, 163),
                AutoSize = true,
                Checked = ResurrectPolicy.AcceptPartyOnly,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Res_AcceptPartyOnlyCheck.CheckedChanged += (s, e) =>
            {
                ResurrectPolicy.AcceptPartyOnly = PhBot_Res_AcceptPartyOnlyCheck.Checked;
            };
            p.Controls.Add(PhBot_Res_AcceptPartyOnlyCheck);

            Label lblResSkills = new Label
            {
                Text = "Canlandırma Becerisi:",
                Location = new Point(438, 195),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            p.Controls.Add(lblResSkills);

            PhBot_Res_SkillsList = new ListBox
            {
                Location = new Point(438, 220),
                Size = new Size(270, 155),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                IntegralHeight = false,
                SelectionMode = SelectionMode.MultiSimple
            };
            PhBot_Res_SkillsList.SelectedIndexChanged += (s, e) =>
            {
                ResurrectPolicy.SelectedResSkills.Clear();
                foreach (var it in PhBot_Res_SkillsList.SelectedItems)
                {
                    if (it is string sk) ResurrectPolicy.SelectedResSkills.Add(sk);
                }
            };
            p.Controls.Add(PhBot_Res_SkillsList);

            RefreshResurrectRegexList();
            PopulateResurrectNearbyPlayers();
            PopulateResurrectSkills();
        }

        private void RefreshResurrectRegexList()
        {
            if (PhBot_Res_RegexList == null) return;
            PhBot_Res_RegexList.Items.Clear();
            foreach (var r in ResurrectPolicy.ResurrectRegexList)
                PhBot_Res_RegexList.Items.Add(r);
        }

        private void PopulateResurrectNearbyPlayers()
        {
            if (PhBot_Res_NearbyList == null) return;
            PhBot_Res_NearbyList.Items.Clear();
            try
            {
                lock (InfoManager.Players)
                {
                    for (int i = 0; i < InfoManager.Players.Count; i++)
                    {
                        var pl = InfoManager.Players.GetAt(i);
                        if (pl != null && !string.IsNullOrEmpty(pl.Name))
                        {
                            if (!PhBot_Res_NearbyList.Items.Contains(pl.Name))
                                PhBot_Res_NearbyList.Items.Add(pl.Name);
                        }
                    }
                }
            }
            catch { }
        }

        private void PopulateResurrectSkills()
        {
            if (PhBot_Res_SkillsList == null) return;
            PhBot_Res_SkillsList.Items.Clear();
            try
            {
                if (Skills_lstvSkills != null)
                {
                    foreach (ListViewItem it in Skills_lstvSkills.Items)
                    {
                        if (it != null && !string.IsNullOrEmpty(it.Text))
                        {
                            string lower = it.Text.ToLowerInvariant();
                            if (lower.Contains("resurrect") || lower.Contains("rebirth") || lower.Contains("revive") || lower.Contains("soul") || lower.Contains("canlan"))
                            {
                                int idx = PhBot_Res_SkillsList.Items.Add(it.Text);
                                if (ResurrectPolicy.SelectedResSkills.Contains(it.Text))
                                    PhBot_Res_SkillsList.SetSelected(idx, true);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        #region TAB 5: LURE
        public void BuildPhBotLureTab(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.FromArgb(245, 247, 251);
            p.AutoScroll = true;

            Label lblWalk = new Label
            {
                Text = "Geri yürüme mesafesi:",
                Location = new Point(12, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(lblWalk);

            PhBot_Lure_WalkDistanceText = new TextBox
            {
                Text = LurePolicy.WalkBackDist.ToString(),
                Location = new Point(140, 12),
                Size = new Size(40, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_WalkDistanceText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_WalkDistanceText.Text, out int dist))
                {
                    LurePolicy.WalkBackDist = dist;
                    LurePolicy.WalkBackDistEnabled = dist > 0;
                }
            };
            p.Controls.Add(PhBot_Lure_WalkDistanceText);

            Label lblLureSkill = new Label
            {
                Text = "Lure Skili:",
                Location = new Point(200, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            p.Controls.Add(lblLureSkill);

            PhBot_Lure_SkillCombo = new ComboBox
            {
                Location = new Point(265, 12),
                Size = new Size(180, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_SkillCombo.SelectedIndexChanged += (s, e) =>
            {
                LurePolicy.LureSkillName = PhBot_Lure_SkillCombo.SelectedItem as string ?? "";
                LurePolicy.LureSkillEnabled = !string.IsNullOrEmpty(LurePolicy.LureSkillName);
            };
            p.Controls.Add(PhBot_Lure_SkillCombo);

            PhBot_Lure_BuffAtEndCheck = new CheckBox
            {
                Text = "Sonunda buff bas",
                Location = new Point(460, 14),
                AutoSize = true,
                Checked = LurePolicy.BuffAtLureEnd,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_BuffAtEndCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.BuffAtLureEnd = PhBot_Lure_BuffAtEndCheck.Checked;
            };
            p.Controls.Add(PhBot_Lure_BuffAtEndCheck);

            GroupBox grpStop = new GroupBox
            {
                Text = "Durdur",
                Location = new Point(12, 45),
                Size = new Size(410, 140),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            PhBot_Lure_StopIfPartyDeadCheck = new CheckBox
            {
                Text = "Parti üyesi ölürse",
                Location = new Point(15, 22),
                AutoSize = true,
                Checked = LurePolicy.StopDeadPartyEnabled,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_StopIfPartyDeadCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.StopDeadPartyEnabled = PhBot_Lure_StopIfPartyDeadCheck.Checked;
            };
            grpStop.Controls.Add(PhBot_Lure_StopIfPartyDeadCheck);

            PhBot_Lure_TownIfPartyLessCheck = new CheckBox
            {
                Text = "Parti üyesi sayısı şundan az ise şehre dön:",
                Location = new Point(15, 48),
                AutoSize = true,
                Checked = LurePolicy.StopTownLessEnabled,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_TownIfPartyLessCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.StopTownLessEnabled = PhBot_Lure_TownIfPartyLessCheck.Checked;
            };
            grpStop.Controls.Add(PhBot_Lure_TownIfPartyLessCheck);

            PhBot_Lure_TownIfPartyLessCount = new TextBox
            {
                Text = LurePolicy.StopTownLessCount.ToString(),
                Location = new Point(265, 45),
                Size = new Size(35, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_TownIfPartyLessCount.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_TownIfPartyLessCount.Text, out int cnt))
                    LurePolicy.StopTownLessCount = cnt;
            };
            grpStop.Controls.Add(PhBot_Lure_TownIfPartyLessCount);

            PhBot_Lure_StopGiantPartyCheck = new CheckBox
            {
                Text = "Eğer parti/dev sayısı şundan fazla ise:",
                Location = new Point(15, 76),
                AutoSize = true,
                Checked = LurePolicy.StopGiantPartyEnabled,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_StopGiantPartyCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.StopGiantPartyEnabled = PhBot_Lure_StopGiantPartyCheck.Checked;
            };
            grpStop.Controls.Add(PhBot_Lure_StopGiantPartyCheck);

            PhBot_Lure_StopGiantPartyCount = new TextBox
            {
                Text = LurePolicy.StopGiantPartyCount.ToString(),
                Location = new Point(245, 73),
                Size = new Size(35, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_StopGiantPartyCount.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_StopGiantPartyCount.Text, out int gcnt))
                    LurePolicy.StopGiantPartyCount = gcnt;
            };
            grpStop.Controls.Add(PhBot_Lure_StopGiantPartyCount);

            PhBot_Lure_StopMobCountCheck = new CheckBox
            {
                Text = "Canavar sayısı şundan fazla ise:",
                Location = new Point(15, 104),
                AutoSize = true,
                Checked = LurePolicy.StopMobCountEnabled,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_StopMobCountCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.StopMobCountEnabled = PhBot_Lure_StopMobCountCheck.Checked;
            };
            grpStop.Controls.Add(PhBot_Lure_StopMobCountCheck);

            PhBot_Lure_StopMobCount = new TextBox
            {
                Text = LurePolicy.StopMobCount.ToString(),
                Location = new Point(245, 101),
                Size = new Size(35, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_StopMobCount.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_StopMobCount.Text, out int mcnt))
                    LurePolicy.StopMobCount = mcnt;
            };
            grpStop.Controls.Add(PhBot_Lure_StopMobCount);

            p.Controls.Add(grpStop);

            PhBot_Lure_AttackLimitCheck = new CheckBox
            {
                Text = "Saldırı sınırı:",
                Location = new Point(15, 195),
                AutoSize = true,
                Checked = LurePolicy.AttackLimitEnabled,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_AttackLimitCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.AttackLimitEnabled = PhBot_Lure_AttackLimitCheck.Checked;
            };
            p.Controls.Add(PhBot_Lure_AttackLimitCheck);

            PhBot_Lure_AttackLimitText = new TextBox
            {
                Text = LurePolicy.AttackMobLimit.ToString(),
                Location = new Point(115, 192),
                Size = new Size(40, 23),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_AttackLimitText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_AttackLimitText.Text, out int sec))
                    LurePolicy.AttackMobLimit = sec;
            };
            p.Controls.Add(PhBot_Lure_AttackLimitText);

            Label lblEdge = new Label
            {
                Text = "Kenar gecikmesi:",
                Location = new Point(15, 227),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            p.Controls.Add(lblEdge);

            PhBot_Lure_EdgeDelayText = new TextBox
            {
                Text = LurePolicy.DelayEdgeMs.ToString(),
                Location = new Point(115, 224),
                Size = new Size(45, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_EdgeDelayText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_EdgeDelayText.Text, out int ed))
                    LurePolicy.DelayEdgeMs = ed;
            };
            p.Controls.Add(PhBot_Lure_EdgeDelayText);

            Label lblEdgeMs = new Label { Text = "ms", Location = new Point(163, 227), AutoSize = true };
            p.Controls.Add(lblEdgeMs);

            Label lblCenter = new Label
            {
                Text = "Merkez gecikmesi:",
                Location = new Point(200, 227),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            p.Controls.Add(lblCenter);

            PhBot_Lure_CenterDelayText = new TextBox
            {
                Text = LurePolicy.DelayCenterMs.ToString(),
                Location = new Point(310, 224),
                Size = new Size(45, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_CenterDelayText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_CenterDelayText.Text, out int cd))
                    LurePolicy.DelayCenterMs = cd;
            };
            p.Controls.Add(PhBot_Lure_CenterDelayText);

            Label lblCenterMs = new Label { Text = "ms", Location = new Point(358, 227), AutoSize = true };
            p.Controls.Add(lblCenterMs);

            Label lblHalf = new Label
            {
                Text = "Yarım gecikme:",
                Location = new Point(15, 257),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            p.Controls.Add(lblHalf);

            PhBot_Lure_HalfDelayText = new TextBox
            {
                Text = LurePolicy.DelayHalfMs.ToString(),
                Location = new Point(115, 254),
                Size = new Size(45, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_HalfDelayText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_HalfDelayText.Text, out int hd))
                    LurePolicy.DelayHalfMs = hd;
            };
            p.Controls.Add(PhBot_Lure_HalfDelayText);

            Label lblHalfMs = new Label { Text = "ms", Location = new Point(163, 257), AutoSize = true };
            p.Controls.Add(lblHalfMs);

            GroupBox grpScript = new GroupBox
            {
                Text = "Script",
                Location = new Point(12, 290),
                Size = new Size(410, 95),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            PhBot_Lure_ScriptPathText = new TextBox
            {
                Text = LurePolicy.ScriptPath,
                Location = new Point(15, 28),
                Size = new Size(295, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ReadOnly = true
            };
            grpScript.Controls.Add(PhBot_Lure_ScriptPathText);

            PhBot_Lure_BtnLoadScript = new Button
            {
                Text = "Yükle",
                Location = new Point(320, 26),
                Size = new Size(75, 26),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_BtnLoadScript.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Script Dosyaları (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        LurePolicy.ScriptPath = ofd.FileName;
                        LurePolicy.UseScript = true;
                        PhBot_Lure_ScriptPathText.Text = ofd.FileName;
                    }
                }
            };
            grpScript.Controls.Add(PhBot_Lure_BtnLoadScript);

            PhBot_Lure_BuffAfterScriptCheck = new CheckBox
            {
                Text = "Script bittikten sonra buff bas",
                Location = new Point(15, 60),
                AutoSize = true,
                Checked = LurePolicy.BuffAfterScriptCommand,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_BuffAfterScriptCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.BuffAfterScriptCommand = PhBot_Lure_BuffAfterScriptCheck.Checked;
            };
            grpScript.Controls.Add(PhBot_Lure_BuffAfterScriptCheck);

            p.Controls.Add(grpScript);

            PhBot_Lure_RandomWalkCheck = new CheckBox
            {
                Text = "Rastgele yürü",
                Location = new Point(440, 48),
                AutoSize = true,
                Checked = LurePolicy.RandomWalk,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_RandomWalkCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.RandomWalk = PhBot_Lure_RandomWalkCheck.Checked;
            };
            p.Controls.Add(PhBot_Lure_RandomWalkCheck);

            PhBot_Lure_SpawnsCheck = new CheckBox
            {
                Text = "Spawns",
                Location = new Point(440, 75),
                AutoSize = true,
                Checked = LurePolicy.WalkToSpawns,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_SpawnsCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.WalkToSpawns = PhBot_Lure_SpawnsCheck.Checked;
            };
            p.Controls.Add(PhBot_Lure_SpawnsCheck);

            PhBot_Lure_SmartCheck = new CheckBox
            {
                Text = "Akıllı",
                Location = new Point(440, 102),
                AutoSize = true,
                Checked = LurePolicy.SmartWalk,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            PhBot_Lure_SmartCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.SmartWalk = PhBot_Lure_SmartCheck.Checked;
            };
            p.Controls.Add(PhBot_Lure_SmartCheck);

            PhBot_Lure_StopIfPartyAwayCheck = new CheckBox
            {
                Text = "Parti üyesi şundan uzakta ise durdur:",
                Location = new Point(440, 130),
                AutoSize = true,
                Checked = LurePolicy.StopIfPartyAway,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_StopIfPartyAwayCheck.CheckedChanged += (s, e) =>
            {
                LurePolicy.StopIfPartyAway = PhBot_Lure_StopIfPartyAwayCheck.Checked;
            };
            p.Controls.Add(PhBot_Lure_StopIfPartyAwayCheck);

            PhBot_Lure_PartyAwayDistText = new TextBox
            {
                Text = LurePolicy.PartyMemberFarAwayDistance.ToString(),
                Location = new Point(660, 127),
                Size = new Size(35, 23),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };
            PhBot_Lure_PartyAwayDistText.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Lure_PartyAwayDistText.Text, out int dst))
                    LurePolicy.PartyMemberFarAwayDistance = dst;
            };
            p.Controls.Add(PhBot_Lure_PartyAwayDistText);

            Label lblPartyM = new Label { Text = "m", Location = new Point(700, 130), AutoSize = true };
            p.Controls.Add(lblPartyM);

            Label lblWhite = new Label
            {
                Text = "Beyaz Liste (Oyuncu Adları):",
                Location = new Point(440, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            p.Controls.Add(lblWhite);

            PhBot_Lure_Whitelist = new ListBox
            {
                Location = new Point(440, 185),
                Size = new Size(240, 190),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                IntegralHeight = false
            };
            p.Controls.Add(PhBot_Lure_Whitelist);

            PhBot_Lure_BtnRemove = new Button
            {
                Text = "K",
                Location = new Point(690, 220),
                Size = new Size(32, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Lure_BtnRemove.Click += (s, e) =>
            {
                if (PhBot_Lure_Whitelist.SelectedItem is string sel)
                {
                    LurePolicy.PartyNearWhitelist.Remove(sel);
                    RefreshLureWhitelist();
                }
            };
            p.Controls.Add(PhBot_Lure_BtnRemove);

            PhBot_Lure_BtnAdd = new Button
            {
                Text = "E",
                Location = new Point(690, 260),
                Size = new Size(32, 28),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            PhBot_Lure_BtnAdd.Click += (s, e) =>
            {
                string pName = PromptInput("Beyaz Listeye Oyuncu Ekle:", "Lure Beyaz Liste");
                if (!string.IsNullOrWhiteSpace(pName))
                {
                    pName = pName.Trim();
                    if (!LurePolicy.PartyNearWhitelist.Contains(pName))
                    {
                        LurePolicy.PartyNearWhitelist.Add(pName);
                        RefreshLureWhitelist();
                    }
                }
            };
            p.Controls.Add(PhBot_Lure_BtnAdd);

            PopulateLureSkills();
            RefreshLureWhitelist();
        }

        private void RefreshLureWhitelist()
        {
            if (PhBot_Lure_Whitelist == null) return;
            PhBot_Lure_Whitelist.Items.Clear();
            foreach (var p in LurePolicy.PartyNearWhitelist)
                PhBot_Lure_Whitelist.Items.Add(p);
        }

        private void PopulateLureSkills()
        {
            if (PhBot_Lure_SkillCombo == null) return;
            PhBot_Lure_SkillCombo.Items.Clear();
            PhBot_Lure_SkillCombo.Items.Add("");
            try
            {
                if (Skills_lstvSkills != null)
                {
                    foreach (ListViewItem it in Skills_lstvSkills.Items)
                    {
                        if (it != null && !string.IsNullOrEmpty(it.Text))
                        {
                            if (!PhBot_Lure_SkillCombo.Items.Contains(it.Text))
                                PhBot_Lure_SkillCombo.Items.Add(it.Text);
                        }
                    }
                }
            }
            catch { }

            if (!string.IsNullOrEmpty(LurePolicy.LureSkillName) && PhBot_Lure_SkillCombo.Items.Contains(LurePolicy.LureSkillName))
                PhBot_Lure_SkillCombo.SelectedItem = LurePolicy.LureSkillName;
            else if (PhBot_Lure_SkillCombo.Items.Count > 0)
                PhBot_Lure_SkillCombo.SelectedIndex = 0;
        }
        #endregion

        #region Helpers & Control Sync
        public void RefreshSkillsTabControls()
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(RefreshSkillsTabControls));
                    return;
                }

                PopulatePartyBuffSkills();
                RefreshPartyAssignmentsList();
                RefreshPartyOptPlayersList();
                RefreshResurrectRegexList();
                PopulateResurrectNearbyPlayers();
                PopulateResurrectSkills();
                PopulateLureSkills();
                RefreshLureWhitelist();
            }
            catch { }
        }

        private string PromptInput(string prompt, string title)
        {
            Form promptForm = new Form()
            {
                Width = 360,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 15, Text = prompt, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 40, Width = 300 };
            Button confirmation = new Button() { Text = "Tamam", Left = 150, Width = 80, Top = 75, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "İptal", Left = 240, Width = 80, Top = 75, DialogResult = DialogResult.Cancel };
            confirmation.Click += (sender, e) => { promptForm.Close(); };
            cancel.Click += (sender, e) => { promptForm.Close(); };
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);
            promptForm.Controls.Add(cancel);
            promptForm.Controls.Add(textLabel);
            promptForm.AcceptButton = confirmation;
            promptForm.CancelButton = cancel;

            return promptForm.ShowDialog(this) == DialogResult.OK ? textBox.Text : "";
        }
        #endregion
    }
}
