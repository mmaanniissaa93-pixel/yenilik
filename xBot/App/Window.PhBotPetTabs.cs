using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

namespace xBot.App
{
    public partial class Window
    {
        #region Saldırı Peti Kontrolleri
        private CheckBox PhBot_Pet_UseAttackPet;
        private CheckBox PhBot_Pet_DontAttackMonsters;
        private CheckBox PhBot_Pet_AutoSummon;
        private CheckBox PhBot_Pet_OnlyInTown;
        private CheckBox PhBot_Pet_OnlyAtTrainingArea;
        private CheckBox PhBot_Pet_AutoRevive;
        private TextBox PhBot_Pet_AutoReviveMaxCount;
        private CheckBox PhBot_Pet_OnlyIfHpPotion;
        private CheckBox PhBot_Pet_ReturnTownWhenDies;
        private CheckBox PhBot_Pet_ReturnTownOnlyIfNoRevive;
        private CheckBox PhBot_Pet_ProtectPet;
        private CheckBox PhBot_Pet_AutoRecallStuck;
        private TextBox PhBot_Pet_AttackRadius;
        private Button PhBot_Pet_BtnRecall;
        private Button PhBot_Pet_BtnDismiss;

        private Label PhBot_Pet_AtkNameVal;
        private Label PhBot_Pet_AtkLevelVal;
        private ProgressBar PhBot_Pet_AtkHpBar;
        private Label PhBot_Pet_AtkHpPct;
        private Label PhBot_Pet_AtkHpVal;
        private ProgressBar PhBot_Pet_AtkHgpBar;
        private Label PhBot_Pet_AtkHgpPct;
        private Label PhBot_Pet_AtkHgpVal;
        private ProgressBar PhBot_Pet_AtkExpBar;
        private Label PhBot_Pet_AtkExpPct;

        private Label PhBot_Pet_AtkStatusVal;
        private Label PhBot_Pet_AtkPositionVal;
        private Label PhBot_Pet_AtkReviveWithVal;
        private Label PhBot_Pet_AtkRecoveryVal;
        private Label PhBot_Pet_AtkHgpFoodVal;
        #endregion

        #region Kervan Kontrolleri
        private Button PhBot_Pet_BtnTerminateCaravan;
        private Label PhBot_Pet_CaravanTypeVal;
        private Label PhBot_Pet_CaravanNameVal;
        private Label PhBot_Pet_CaravanMountedVal;
        private Label PhBot_Pet_CaravanXVal;
        private Label PhBot_Pet_CaravanYVal;
        private ProgressBar PhBot_Pet_CaravanHpBar;
        private Label PhBot_Pet_CaravanHpPct;
        #endregion

        #region Toplama Peti Kontrolleri
        private Button PhBot_Pet_BtnPickRecall;
        private Button PhBot_Pet_BtnPickDismiss;
        private Label PhBot_Pet_PickNameVal;
        private Label PhBot_Pet_PickXVal;
        private Label PhBot_Pet_PickYVal;
        private Label PhBot_Pet_PickRemainingTimeVal;
        private Label PhBot_Pet_PickSlotsVal;
        private Label PhBot_Pet_PickAutoPickVal;
        private ListView PhBot_Pet_PickInventory;
        private Button PhBot_Pet_BtnMoveSelected;
        private Button PhBot_Pet_BtnOrganize;
        #endregion

        #region Fellow Kontrolleri
        private CheckBox PhBot_Pet_FellowUnique;
        private CheckBox PhBot_Pet_FellowReady;
        private CheckBox PhBot_Pet_FellowGrowth;
        private CheckBox PhBot_Pet_FellowSpRecall;

        private Label PhBot_Pet_FellowNameVal;
        private Label PhBot_Pet_FellowLevelVal;
        private Label PhBot_Pet_FellowStoredSpVal;
        private ProgressBar PhBot_Pet_FellowHpBar;
        private Label PhBot_Pet_FellowHpPct;
        private ProgressBar PhBot_Pet_FellowHgpBar;
        private Label PhBot_Pet_FellowHgpPct;
        private ProgressBar PhBot_Pet_FellowExpBar;
        private Label PhBot_Pet_FellowExpPct;

        private Label PhBot_Pet_FellowStrVal;
        private Label PhBot_Pet_FellowIntVal;
        private Label PhBot_Pet_FellowPhysAtkVal;
        private Label PhBot_Pet_FellowMagAtkVal;
        private Label PhBot_Pet_FellowPhysDefVal;
        private Label PhBot_Pet_FellowMagDefVal;
        private Label PhBot_Pet_FellowPhysAbsVal;
        private Label PhBot_Pet_FellowMagAbsVal;
        private Label PhBot_Pet_FellowAtkRateVal;
        private Label PhBot_Pet_FellowParryVal;
        private Label PhBot_Pet_FellowBlockVal;
        private Label PhBot_Pet_FellowCritVal;

        private ListView PhBot_Pet_FellowArmorList;
        private ListView PhBot_Pet_FellowSkillsList;
        #endregion

        private TabControl PhBot_Pet_MainTabs;

        public Panel BuildPhBotPetPanel(Panel panel)
        {
            if (panel == null) return null;
            panel.SuspendLayout();
            panel.Controls.Clear();
            panel.BackColor = Color.FromArgb(245, 247, 251);
            panel.AutoScroll = true;

            PhBot_Pet_MainTabs = new TabControl
            {
                Name = "PhBot_Pet_MainTabs",
                Dock = DockStyle.Fill,
                Font = PhBotFont()
            };

            // 4 Alt Sekme (Birebir phBot sırası ve başlıkları)
            TabPage tabAttack = new TabPage("Saldırı Peti") { BackColor = Color.White };
            TabPage tabCaravan = new TabPage("Kervan") { BackColor = Color.White };
            TabPage tabPick = new TabPage("Toplama") { BackColor = Color.White };
            TabPage tabFellow = new TabPage("Fellow") { BackColor = Color.White };

            BuildAttackPetSubTab(tabAttack);
            BuildCaravanSubTab(tabCaravan);
            BuildPickPetSubTab(tabPick);
            BuildFellowSubTab(tabFellow);

            PhBot_Pet_MainTabs.TabPages.Add(tabAttack);
            PhBot_Pet_MainTabs.TabPages.Add(tabCaravan);
            PhBot_Pet_MainTabs.TabPages.Add(tabPick);
            PhBot_Pet_MainTabs.TabPages.Add(tabFellow);

            panel.Controls.Add(PhBot_Pet_MainTabs);
            panel.ResumeLayout(true);

            UpdatePetRuntimeStatus();
            return panel;
        }

        #region Alt Sekme 1: Saldırı Peti (media_1789185928301.png)
        private void BuildAttackPetSubTab(TabPage page)
        {
            page.AutoScroll = true;

            // Sağ üst eylem butonları (Recall, Peti kapat)
            PhBot_Pet_BtnRecall = new Button
            {
                Name = "PhBot_Pet_BtnRecall",
                Text = "Recall",
                Location = new Point(620, 10),
                Size = new Size(72, 24),
                Font = PhBotFont(),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            PhBot_Pet_BtnRecall.Click += (s, e) => RecallAttackPet();
            page.Controls.Add(PhBot_Pet_BtnRecall);

            PhBot_Pet_BtnDismiss = new Button
            {
                Name = "PhBot_Pet_BtnDismiss",
                Text = "Peti kapat",
                Location = new Point(700, 10),
                Size = new Size(72, 24),
                Font = PhBotFont(),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            PhBot_Pet_BtnDismiss.Click += (s, e) => DismissAttackPet();
            page.Controls.Add(PhBot_Pet_BtnDismiss);

            // Satır 1
            PhBot_Pet_UseAttackPet = new CheckBox
            {
                Text = "Saldırı petini kullan",
                Location = new Point(16, 12),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.UseAttackPet
            };
            PhBot_Pet_UseAttackPet.CheckedChanged += (s, e) => { PetPolicy.UseAttackPet = PhBot_Pet_UseAttackPet.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_UseAttackPet);

            PhBot_Pet_DontAttackMonsters = new CheckBox
            {
                Text = "Canavara saldırma",
                Location = new Point(190, 12),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.DontAttackMonsters
            };
            PhBot_Pet_DontAttackMonsters.CheckedChanged += (s, e) => { PetPolicy.DontAttackMonsters = PhBot_Pet_DontAttackMonsters.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_DontAttackMonsters);

            // Satır 2
            PhBot_Pet_AutoSummon = new CheckBox
            {
                Text = "Saldırı petini oto çağır",
                Location = new Point(16, 38),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.AutoSummonAttackPet
            };
            PhBot_Pet_AutoSummon.CheckedChanged += (s, e) => { PetPolicy.AutoSummonAttackPet = PhBot_Pet_AutoSummon.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_AutoSummon);

            PhBot_Pet_OnlyInTown = new CheckBox
            {
                Text = "Sadece şehirde",
                Location = new Point(200, 38),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.OnlyInTown
            };
            PhBot_Pet_OnlyInTown.CheckedChanged += (s, e) => { PetPolicy.OnlyInTown = PhBot_Pet_OnlyInTown.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_OnlyInTown);

            PhBot_Pet_OnlyAtTrainingArea = new CheckBox
            {
                Text = "Sadece kasılma alanında",
                Location = new Point(320, 38),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.OnlyAtTrainingArea
            };
            PhBot_Pet_OnlyAtTrainingArea.CheckedChanged += (s, e) => { PetPolicy.OnlyAtTrainingArea = PhBot_Pet_OnlyAtTrainingArea.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_OnlyAtTrainingArea);

            // Satır 3
            PhBot_Pet_AutoRevive = new CheckBox
            {
                Text = "Peti otomatik canlandır",
                Location = new Point(16, 64),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.AutoReviveAttackPet
            };
            PhBot_Pet_AutoRevive.CheckedChanged += (s, e) => { PetPolicy.AutoReviveAttackPet = PhBot_Pet_AutoRevive.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_AutoRevive);

            PhBot_Pet_AutoReviveMaxCount = new TextBox
            {
                Text = PetPolicy.AutoReviveMaxCount.ToString(),
                Location = new Point(185, 62),
                Size = new Size(36, 20),
                Font = PhBotFont(),
                TextAlign = HorizontalAlignment.Center
            };
            PhBot_Pet_AutoReviveMaxCount.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Pet_AutoReviveMaxCount.Text, out int val))
                {
                    PetPolicy.AutoReviveMaxCount = val;
                    Settings.SaveCharacterSettings();
                }
            };
            page.Controls.Add(PhBot_Pet_AutoReviveMaxCount);

            Label lblMaxTimes = new Label
            {
                Text = "Maximum (Sınırsız için 0 )",
                Location = new Point(226, 65),
                AutoSize = true,
                Font = PhBotFont()
            };
            page.Controls.Add(lblMaxTimes);

            PhBot_Pet_OnlyIfHpPotion = new CheckBox
            {
                Text = "sadece HP potu mevcutsa",
                Location = new Point(410, 64),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.OnlyIfHpPotionPresent
            };
            PhBot_Pet_OnlyIfHpPotion.CheckedChanged += (s, e) => { PetPolicy.OnlyIfHpPotionPresent = PhBot_Pet_OnlyIfHpPotion.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_OnlyIfHpPotion);

            // Satır 4
            PhBot_Pet_ReturnTownWhenDies = new CheckBox
            {
                Text = "Saldırı peti ölünce şehre dön",
                Location = new Point(16, 90),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.ReturnTownWhenAttackPetDies
            };
            PhBot_Pet_ReturnTownWhenDies.CheckedChanged += (s, e) => { PetPolicy.ReturnTownWhenAttackPetDies = PhBot_Pet_ReturnTownWhenDies.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_ReturnTownWhenDies);

            PhBot_Pet_ReturnTownOnlyIfNoRevive = new CheckBox
            {
                Text = "sadece pet canlandırması biterse şehre dön",
                Location = new Point(240, 90),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.ReturnTownOnlyIfNoRevive
            };
            PhBot_Pet_ReturnTownOnlyIfNoRevive.CheckedChanged += (s, e) => { PetPolicy.ReturnTownOnlyIfNoRevive = PhBot_Pet_ReturnTownOnlyIfNoRevive.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_ReturnTownOnlyIfNoRevive);

            // Satır 5
            PhBot_Pet_ProtectPet = new CheckBox
            {
                Text = "Saldırı petini koru",
                Location = new Point(16, 116),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.ProtectAttackPet
            };
            PhBot_Pet_ProtectPet.CheckedChanged += (s, e) => { PetPolicy.ProtectAttackPet = PhBot_Pet_ProtectPet.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_ProtectPet);

            PhBot_Pet_AutoRecallStuck = new CheckBox
            {
                Text = "Auto-recall stuck attack pet",
                Location = new Point(190, 116),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.AutoRecallStuckAttackPet
            };
            PhBot_Pet_AutoRecallStuck.CheckedChanged += (s, e) => { PetPolicy.AutoRecallStuckAttackPet = PhBot_Pet_AutoRecallStuck.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_AutoRecallStuck);

            // Satır 6: Saldırı yarıçapı
            Label lblRadius = new Label
            {
                Text = "Saldırı yarıçapı",
                Location = new Point(16, 144),
                AutoSize = true,
                Font = PhBotFont()
            };
            page.Controls.Add(lblRadius);

            PhBot_Pet_AttackRadius = new TextBox
            {
                Text = PetPolicy.AttackRadius.ToString(),
                Location = new Point(110, 141),
                Size = new Size(40, 20),
                Font = PhBotFont(),
                TextAlign = HorizontalAlignment.Center
            };
            PhBot_Pet_AttackRadius.TextChanged += (s, e) =>
            {
                if (int.TryParse(PhBot_Pet_AttackRadius.Text, out int r))
                {
                    PetPolicy.AttackRadius = r;
                    Settings.SaveCharacterSettings();
                }
            };
            page.Controls.Add(PhBot_Pet_AttackRadius);

            // Canlı Göstergeler (Sol Kolon)
            int yStart = 175;
            Label lblName = new Label { Text = "İsimi", Location = new Point(16, yStart), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkNameVal = new Label { Text = "İsimsiz", Location = new Point(70, yStart), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblName); page.Controls.Add(PhBot_Pet_AtkNameVal);

            Label lblLevel = new Label { Text = "Seviye", Location = new Point(16, yStart + 24), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkLevelVal = new Label { Text = "0", Location = new Point(70, yStart + 24), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblLevel); page.Controls.Add(PhBot_Pet_AtkLevelVal);

            Label lblHp = new Label { Text = "HP", Location = new Point(16, yStart + 48), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkHpBar = new ProgressBar { Location = new Point(65, yStart + 48), Size = new Size(135, 14), Minimum = 0, Maximum = 100, Value = 0 };
            PhBot_Pet_AtkHpPct = new Label { Text = "0%", Location = new Point(205, yStart + 48), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkHpVal = new Label { Text = " - ", Location = new Point(255, yStart + 48), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblHp); page.Controls.Add(PhBot_Pet_AtkHpBar); page.Controls.Add(PhBot_Pet_AtkHpPct); page.Controls.Add(PhBot_Pet_AtkHpVal);

            Label lblHgp = new Label { Text = "HGP", Location = new Point(16, yStart + 72), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkHgpBar = new ProgressBar { Location = new Point(65, yStart + 72), Size = new Size(135, 14), Minimum = 0, Maximum = 100, Value = 0 };
            PhBot_Pet_AtkHgpPct = new Label { Text = "0%", Location = new Point(205, yStart + 72), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkHgpVal = new Label { Text = " - ", Location = new Point(255, yStart + 72), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblHgp); page.Controls.Add(PhBot_Pet_AtkHgpBar); page.Controls.Add(PhBot_Pet_AtkHgpPct); page.Controls.Add(PhBot_Pet_AtkHgpVal);

            Label lblExp = new Label { Text = "EXP", Location = new Point(16, yStart + 96), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkExpBar = new ProgressBar { Location = new Point(65, yStart + 96), Size = new Size(135, 14), Minimum = 0, Maximum = 100, Value = 0 };
            PhBot_Pet_AtkExpPct = new Label { Text = "0%", Location = new Point(205, yStart + 96), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblExp); page.Controls.Add(PhBot_Pet_AtkExpBar); page.Controls.Add(PhBot_Pet_AtkExpPct);

            // Canlı Göstergeler (Sağ Kolon)
            int rx = 420;
            int rv = 510;
            Label lblStat = new Label { Text = "Status", Location = new Point(rx, yStart), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkStatusVal = new Label { Text = " - ", Location = new Point(rv, yStart), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblStat); page.Controls.Add(PhBot_Pet_AtkStatusVal);

            Label lblPos = new Label { Text = "Position", Location = new Point(rx, yStart + 24), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkPositionVal = new Label { Text = " - ", Location = new Point(rv, yStart + 24), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblPos); page.Controls.Add(PhBot_Pet_AtkPositionVal);

            Label lblRev = new Label { Text = "Revive with", Location = new Point(rx, yStart + 48), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkReviveWithVal = new Label { Text = " - ", Location = new Point(rv, yStart + 48), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblRev); page.Controls.Add(PhBot_Pet_AtkReviveWithVal);

            Label lblRec = new Label { Text = "Recovery", Location = new Point(rx, yStart + 72), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkRecoveryVal = new Label { Text = " - ", Location = new Point(rv, yStart + 72), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblRec); page.Controls.Add(PhBot_Pet_AtkRecoveryVal);

            Label lblFood = new Label { Text = "HGP food", Location = new Point(rx, yStart + 96), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_AtkHgpFoodVal = new Label { Text = " - ", Location = new Point(rv, yStart + 96), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblFood); page.Controls.Add(PhBot_Pet_AtkHgpFoodVal);

            // Alt Bilgi Notu
            Label lblNote = new Label
            {
                Text = "* Envanterinizde bir saldırı peti olduğundan emin olun",
                Location = new Point(16, 335),
                AutoSize = true,
                Font = PhBotFont(),
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            page.Controls.Add(lblNote);
        }
        #endregion

        #region Alt Sekme 2: Kervan (media_1789185934646.png)
        private void BuildCaravanSubTab(TabPage page)
        {
            page.AutoScroll = true;

            PhBot_Pet_BtnTerminateCaravan = new Button
            {
                Name = "PhBot_Pet_BtnTerminateCaravan",
                Text = "Sonlandır",
                Location = new Point(700, 10),
                Size = new Size(72, 24),
                Font = PhBotFont(),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            PhBot_Pet_BtnTerminateCaravan.Click += (s, e) => TerminateCaravanPet();
            page.Controls.Add(PhBot_Pet_BtnTerminateCaravan);

            int y = 20;
            Label lblType = new Label { Text = "Tipi", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_CaravanTypeVal = new Label { Text = " - ", Location = new Point(70, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblType); page.Controls.Add(PhBot_Pet_CaravanTypeVal);

            y += 26;
            Label lblName = new Label { Text = "İsimi", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_CaravanNameVal = new Label { Text = " - ", Location = new Point(70, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblName); page.Controls.Add(PhBot_Pet_CaravanNameVal);

            y += 26;
            Label lblRide = new Label { Text = "Binek", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_CaravanMountedVal = new Label { Text = "Hayır", Location = new Point(70, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblRide); page.Controls.Add(PhBot_Pet_CaravanMountedVal);

            y += 26;
            Label lblX = new Label { Text = "X", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_CaravanXVal = new Label { Text = "0", Location = new Point(70, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblX); page.Controls.Add(PhBot_Pet_CaravanXVal);

            y += 26;
            Label lblY = new Label { Text = "Y", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_CaravanYVal = new Label { Text = "0", Location = new Point(70, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblY); page.Controls.Add(PhBot_Pet_CaravanYVal);

            y += 28;
            Label lblHp = new Label { Text = "HP", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_CaravanHpBar = new ProgressBar
            {
                Location = new Point(70, y),
                Size = new Size(500, 14),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            PhBot_Pet_CaravanHpPct = new Label
            {
                Text = "0%",
                Location = new Point(576, y),
                AutoSize = true,
                Font = PhBotFont(),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            page.Controls.Add(lblHp); page.Controls.Add(PhBot_Pet_CaravanHpBar); page.Controls.Add(PhBot_Pet_CaravanHpPct);
        }
        #endregion

        #region Alt Sekme 3: Toplama (media_1789185942242.png)
        private void BuildPickPetSubTab(TabPage page)
        {
            page.AutoScroll = true;

            PhBot_Pet_BtnPickRecall = new Button
            {
                Name = "PhBot_Pet_BtnPickRecall",
                Text = "Recall",
                Location = new Point(620, 10),
                Size = new Size(72, 24),
                Font = PhBotFont(),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            PhBot_Pet_BtnPickRecall.Click += (s, e) => RecallPickPet();
            page.Controls.Add(PhBot_Pet_BtnPickRecall);

            PhBot_Pet_BtnPickDismiss = new Button
            {
                Name = "PhBot_Pet_BtnPickDismiss",
                Text = "Peti kapat",
                Location = new Point(700, 10),
                Size = new Size(72, 24),
                Font = PhBotFont(),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            PhBot_Pet_BtnPickDismiss.Click += (s, e) => DismissPickPet();
            page.Controls.Add(PhBot_Pet_BtnPickDismiss);

            int y = 14;
            Label lblName = new Label { Text = "İsimi", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_PickNameVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblName); page.Controls.Add(PhBot_Pet_PickNameVal);

            y += 24;
            Label lblX = new Label { Text = "X", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_PickXVal = new Label { Text = "0", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblX); page.Controls.Add(PhBot_Pet_PickXVal);

            y += 24;
            Label lblY = new Label { Text = "Y", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_PickYVal = new Label { Text = "0", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblY); page.Controls.Add(PhBot_Pet_PickYVal);

            y += 24;
            Label lblTime = new Label { Text = "Kalan süresi", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_PickRemainingTimeVal = new Label { Text = "N/A", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblTime); page.Controls.Add(PhBot_Pet_PickRemainingTimeVal);

            y += 24;
            Label lblSlots = new Label { Text = "Slots", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_PickSlotsVal = new Label { Text = "0/0", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblSlots); page.Controls.Add(PhBot_Pet_PickSlotsVal);

            y += 24;
            Label lblAuto = new Label { Text = "Auto-pick", Location = new Point(16, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_PickAutoPickVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            page.Controls.Add(lblAuto); page.Controls.Add(PhBot_Pet_PickAutoPickVal);

            // GroupBox: Inventory
            GroupBox grpInv = new GroupBox
            {
                Text = "Inventory",
                Location = new Point(16, 160),
                Size = new Size(756, 215),
                Font = PhBotFont(),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            PhBot_Pet_PickInventory = new ListView
            {
                Location = new Point(10, 20),
                Size = new Size(736, 150),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = PhBotFont(),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            PhBot_Pet_PickInventory.Columns.Add("Miktar", 60);
            PhBot_Pet_PickInventory.Columns.Add("Resim", 55);
            PhBot_Pet_PickInventory.Columns.Add("Eşya", 480);
            PhBot_Pet_PickInventory.Columns.Add("Miktar", 100);
            grpInv.Controls.Add(PhBot_Pet_PickInventory);

            PhBot_Pet_BtnMoveSelected = new Button
            {
                Text = "Move selected",
                Location = new Point(10, 178),
                Size = new Size(100, 26),
                Font = PhBotFont(),
                BackColor = Color.White
            };
            PhBot_Pet_BtnMoveSelected.Click += (s, e) => MoveSelectedPickPetItems();
            grpInv.Controls.Add(PhBot_Pet_BtnMoveSelected);

            PhBot_Pet_BtnOrganize = new Button
            {
                Text = "Organize",
                Location = new Point(118, 178),
                Size = new Size(90, 26),
                Font = PhBotFont(),
                BackColor = Color.White
            };
            PhBot_Pet_BtnOrganize.Click += (s, e) => OrganizePickPetInventory();
            grpInv.Controls.Add(PhBot_Pet_BtnOrganize);

            page.Controls.Add(grpInv);
        }
        #endregion

        #region Alt Sekme 4: Fellow (media_1789185948832.png)
        private void BuildFellowSubTab(TabPage page)
        {
            page.AutoScroll = true;

            // Üst CheckBox'lar
            PhBot_Pet_FellowUnique = new CheckBox
            {
                Text = "Fellow petlerin özgün(unique) becerilerini kullan",
                Location = new Point(16, 12),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.UseFellowUniqueSkills
            };
            PhBot_Pet_FellowUnique.CheckedChanged += (s, e) => { PetPolicy.UseFellowUniqueSkills = PhBot_Pet_FellowUnique.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_FellowUnique);

            PhBot_Pet_FellowGrowth = new CheckBox
            {
                Text = "Potion of Growth kullan",
                Location = new Point(380, 12),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.UsePotionOfGrowth
            };
            PhBot_Pet_FellowGrowth.CheckedChanged += (s, e) => { PetPolicy.UsePotionOfGrowth = PhBot_Pet_FellowGrowth.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_FellowGrowth);

            PhBot_Pet_FellowReady = new CheckBox
            {
                Text = "Fellow petlerin hazır becerilerini kullan",
                Location = new Point(16, 38),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.UseFellowReadySkills
            };
            PhBot_Pet_FellowReady.CheckedChanged += (s, e) => { PetPolicy.UseFellowReadySkills = PhBot_Pet_FellowReady.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_FellowReady);

            PhBot_Pet_FellowSpRecall = new CheckBox
            {
                Text = "Şehirdeyken fellow pet yeteneğini (SP Recall)'i kullan",
                Location = new Point(380, 38),
                AutoSize = true,
                Font = PhBotFont(),
                Checked = PetPolicy.UseFellowSpRecallInTown
            };
            PhBot_Pet_FellowSpRecall.CheckedChanged += (s, e) => { PetPolicy.UseFellowSpRecallInTown = PhBot_Pet_FellowSpRecall.Checked; Settings.SaveCharacterSettings(); };
            page.Controls.Add(PhBot_Pet_FellowSpRecall);

            // GroupBox: Stats (Sol)
            GroupBox grpStats = new GroupBox
            {
                Text = "Stats",
                Location = new Point(16, 68),
                Size = new Size(345, 305),
                Font = PhBotFont()
            };

            int y = 20;
            Label lblName = new Label { Text = "İsimi", Location = new Point(12, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowNameVal = new Label { Text = "No fellow", Location = new Point(85, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblName); grpStats.Controls.Add(PhBot_Pet_FellowNameVal);

            y += 22;
            Label lblLevel = new Label { Text = "Seviye", Location = new Point(12, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowLevelVal = new Label { Text = " - ", Location = new Point(85, y), AutoSize = true, Font = PhBotFont() };
            Label lblSp = new Label { Text = "Stored SP", Location = new Point(160, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowStoredSpVal = new Label { Text = " - ", Location = new Point(230, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblLevel); grpStats.Controls.Add(PhBot_Pet_FellowLevelVal);
            grpStats.Controls.Add(lblSp); grpStats.Controls.Add(PhBot_Pet_FellowStoredSpVal);

            y += 24;
            Label lblHp = new Label { Text = "HP", Location = new Point(12, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowHpBar = new ProgressBar { Location = new Point(85, y), Size = new Size(185, 12), Minimum = 0, Maximum = 100, Value = 0 };
            PhBot_Pet_FellowHpPct = new Label { Text = "0%", Location = new Point(275, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblHp); grpStats.Controls.Add(PhBot_Pet_FellowHpBar); grpStats.Controls.Add(PhBot_Pet_FellowHpPct);

            y += 22;
            Label lblHgp = new Label { Text = "HGP", Location = new Point(12, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowHgpBar = new ProgressBar { Location = new Point(85, y), Size = new Size(185, 12), Minimum = 0, Maximum = 100, Value = 0 };
            PhBot_Pet_FellowHgpPct = new Label { Text = "0%", Location = new Point(275, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblHgp); grpStats.Controls.Add(PhBot_Pet_FellowHgpBar); grpStats.Controls.Add(PhBot_Pet_FellowHgpPct);

            y += 22;
            Label lblExp = new Label { Text = "EXP", Location = new Point(12, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowExpBar = new ProgressBar { Location = new Point(85, y), Size = new Size(185, 12), Minimum = 0, Maximum = 100, Value = 0 };
            PhBot_Pet_FellowExpPct = new Label { Text = "0%", Location = new Point(275, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblExp); grpStats.Controls.Add(PhBot_Pet_FellowExpBar); grpStats.Controls.Add(PhBot_Pet_FellowExpPct);

            y += 26;
            // 2 Sütun Nitelikler
            int c1 = 12, c2 = 165;
            Label lblStr = new Label { Text = "STR", Location = new Point(c1, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowStrVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            Label lblInt = new Label { Text = "INT", Location = new Point(c2, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowIntVal = new Label { Text = " - ", Location = new Point(255, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblStr); grpStats.Controls.Add(PhBot_Pet_FellowStrVal);
            grpStats.Controls.Add(lblInt); grpStats.Controls.Add(PhBot_Pet_FellowIntVal);

            y += 20;
            Label lblPa = new Label { Text = "Phys. Attack", Location = new Point(c1, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowPhysAtkVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblPa); grpStats.Controls.Add(PhBot_Pet_FellowPhysAtkVal);

            y += 20;
            Label lblMa = new Label { Text = "Mag. Attack", Location = new Point(c1, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowMagAtkVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblMa); grpStats.Controls.Add(PhBot_Pet_FellowMagAtkVal);

            y += 20;
            Label lblPd = new Label { Text = "Phys. Defense", Location = new Point(c1, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowPhysDefVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            Label lblMd = new Label { Text = "Mag. Defense", Location = new Point(c2, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowMagDefVal = new Label { Text = " - ", Location = new Point(255, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblPd); grpStats.Controls.Add(PhBot_Pet_FellowPhysDefVal);
            grpStats.Controls.Add(lblMd); grpStats.Controls.Add(PhBot_Pet_FellowMagDefVal);

            y += 20;
            Label lblPab = new Label { Text = "Phys. Absorption", Location = new Point(c1, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowPhysAbsVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            Label lblMab = new Label { Text = "Mag. Absorption", Location = new Point(c2, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowMagAbsVal = new Label { Text = " - ", Location = new Point(255, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblPab); grpStats.Controls.Add(PhBot_Pet_FellowPhysAbsVal);
            grpStats.Controls.Add(lblMab); grpStats.Controls.Add(PhBot_Pet_FellowMagAbsVal);

            y += 20;
            Label lblAr = new Label { Text = "Attack Rate", Location = new Point(c1, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowAtkRateVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            Label lblPr = new Label { Text = "Parry Ratio", Location = new Point(c2, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowParryVal = new Label { Text = " - ", Location = new Point(255, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblAr); grpStats.Controls.Add(PhBot_Pet_FellowAtkRateVal);
            grpStats.Controls.Add(lblPr); grpStats.Controls.Add(PhBot_Pet_FellowParryVal);

            y += 20;
            Label lblBr = new Label { Text = "Block Ratio", Location = new Point(c1, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowBlockVal = new Label { Text = " - ", Location = new Point(100, y), AutoSize = true, Font = PhBotFont() };
            Label lblCrit = new Label { Text = "Critical", Location = new Point(c2, y), AutoSize = true, Font = PhBotFont() };
            PhBot_Pet_FellowCritVal = new Label { Text = " - ", Location = new Point(255, y), AutoSize = true, Font = PhBotFont() };
            grpStats.Controls.Add(lblBr); grpStats.Controls.Add(PhBot_Pet_FellowBlockVal);
            grpStats.Controls.Add(lblCrit); grpStats.Controls.Add(PhBot_Pet_FellowCritVal);

            page.Controls.Add(grpStats);

            // GroupBox: Equipped armor (Sağ Üst)
            GroupBox grpArmor = new GroupBox
            {
                Text = "Equipped armor",
                Location = new Point(375, 68),
                Size = new Size(397, 140),
                Font = PhBotFont(),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            PhBot_Pet_FellowArmorList = new ListView
            {
                Location = new Point(10, 20),
                Size = new Size(377, 110),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = PhBotFont(),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            PhBot_Pet_FellowArmorList.Columns.Add("Slot", 60);
            PhBot_Pet_FellowArmorList.Columns.Add("Icon", 50);
            PhBot_Pet_FellowArmorList.Columns.Add("Item", 180);
            PhBot_Pet_FellowArmorList.Columns.Add("Expires", 75);
            grpArmor.Controls.Add(PhBot_Pet_FellowArmorList);
            page.Controls.Add(grpArmor);

            // GroupBox: Skills (Sağ Alt)
            GroupBox grpSkills = new GroupBox
            {
                Text = "Skills",
                Location = new Point(375, 215),
                Size = new Size(397, 158),
                Font = PhBotFont(),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            PhBot_Pet_FellowSkillsList = new ListView
            {
                Location = new Point(10, 20),
                Size = new Size(377, 128),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = PhBotFont(),
                CheckBoxes = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            PhBot_Pet_FellowSkillsList.Columns.Add("On", 40);
            PhBot_Pet_FellowSkillsList.Columns.Add("Icon", 50);
            PhBot_Pet_FellowSkillsList.Columns.Add("Skill", 140);
            PhBot_Pet_FellowSkillsList.Columns.Add("Status", 65);
            PhBot_Pet_FellowSkillsList.Columns.Add("Kalan süresi", 75);
            grpSkills.Controls.Add(PhBot_Pet_FellowSkillsList);
            page.Controls.Add(grpSkills);

            // Alt Not
            Label lblFellowNote = new Label
            {
                Text = "* The options above only cast while clientless; the skill list works either way",
                Location = new Point(16, 382),
                AutoSize = true,
                Font = PhBotFont(),
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            page.Controls.Add(lblFellowNote);
        }
        #endregion

        #region Buton Aksiyonları
        private void RecallAttackPet()
        {
            try
            {
                SRCoService pet = InfoManager.MyPets?.Find(p => p != null && p.isAttackPet() && !Script.IsLikelyFellow(p));
                if (pet == null)
                    pet = InfoManager.MyPets?.Find(p => p != null && p.isAttackPet());

                if (pet != null && InfoManager.Character != null)
                {
                    SRCoord myPos = InfoManager.Character.GetRealtimePosition();
                    PacketBuilder.MoveTo(myPos, pet.UniqueID);
                    LogProcess($"Saldırı peti çağrılıyor (Recall) [{pet.Name}]...");
                }
                else
                {
                    LogProcess("Recall: Çağrılmış saldırı peti bulunamadı.", ProcessState.Warning);
                }
            }
            catch (Exception ex)
            {
                LogProcess("Recall hatası: " + ex.Message, ProcessState.Error);
            }
        }

        private void DismissAttackPet()
        {
            try
            {
                SRCoService pet = InfoManager.MyPets?.Find(p => p != null && p.isAttackPet() && !Script.IsLikelyFellow(p));
                if (pet == null)
                    pet = InfoManager.MyPets?.Find(p => p != null && p.isAttackPet());

                if (pet != null)
                {
                    PacketBuilder.UnsummonPet(pet.UniqueID);
                    LogProcess($"Saldırı peti kapatılıyor [{pet.Name}]...");
                }
                else
                {
                    LogProcess("Kapat: Çağrılmış saldırı peti bulunamadı.", ProcessState.Warning);
                }
            }
            catch (Exception ex)
            {
                LogProcess("Pet kapatma hatası: " + ex.Message, ProcessState.Error);
            }
        }

        private void TerminateCaravanPet()
        {
            try
            {
                SRCoService pet = InfoManager.MyPets?.Find(p => p != null && (p.isTransport() || p.isHorse()));
                if (pet != null)
                {
                    PacketBuilder.UnsummonPet(pet.UniqueID);
                    LogProcess($"Kervan/Binek taşıtı sonlandırılıyor [{pet.Name}]...");
                }
                else
                {
                    LogProcess("Sonlandır: Aktif kervan/binek bulunamadı.", ProcessState.Warning);
                }
            }
            catch (Exception ex)
            {
                LogProcess("Kervan sonlandırma hatası: " + ex.Message, ProcessState.Error);
            }
        }

        private void RecallPickPet()
        {
            try
            {
                SRCoService pet = InfoManager.MyPets?.Find(p => p != null && p.isPickPet());
                if (pet != null && InfoManager.Character != null)
                {
                    SRCoord myPos = InfoManager.Character.GetRealtimePosition();
                    PacketBuilder.MoveTo(myPos, pet.UniqueID);
                    LogProcess($"Toplama peti çağrılıyor (Recall) [{pet.Name}]...");
                }
                else
                {
                    LogProcess("Recall: Çağrılmış toplama peti bulunamadı.", ProcessState.Warning);
                }
            }
            catch (Exception ex)
            {
                LogProcess("Pick pet recall hatası: " + ex.Message, ProcessState.Error);
            }
        }

        private void DismissPickPet()
        {
            try
            {
                SRCoService pet = InfoManager.MyPets?.Find(p => p != null && p.isPickPet());
                if (pet != null)
                {
                    PacketBuilder.UnsummonPet(pet.UniqueID);
                    LogProcess($"Toplama peti kapatılıyor [{pet.Name}]...");
                }
                else
                {
                    LogProcess("Kapat: Çağrılmış toplama peti bulunamadı.", ProcessState.Warning);
                }
            }
            catch (Exception ex)
            {
                LogProcess("Pick pet kapatma hatası: " + ex.Message, ProcessState.Error);
            }
        }

        private void MoveSelectedPickPetItems()
        {
            try
            {
                SRCoService pet = InfoManager.MyPets?.Find(p => p != null && p.isPickPet());
                if (pet == null || pet.Inventory == null)
                {
                    LogProcess("Move selected: Toplama peti envanteri bulunamadı.", ProcessState.Warning);
                    return;
                }
                if (PhBot_Pet_PickInventory == null || PhBot_Pet_PickInventory.SelectedItems.Count == 0)
                {
                    LogProcess("Move selected: Lütfen aktarılacak eşyaları seçin.", ProcessState.Warning);
                    return;
                }

                var chr = InfoManager.Character;
                if (chr == null || chr.Inventory == null) return;

                int moved = 0;
                foreach (ListViewItem item in PhBot_Pet_PickInventory.SelectedItems)
                {
                    if (item.Tag is byte petSlot)
                    {
                        var pItem = pet.Inventory[petSlot];
                        if (pItem == null) continue;

                        int emptySlot = -1;
                        for (byte s = 13; s < chr.Inventory.Capacity; s++)
                        {
                            if (chr.Inventory[s] == null)
                            {
                                emptySlot = s;
                                break;
                            }
                        }
                        if (emptySlot == -1)
                        {
                            LogProcess("Move selected: Karakter envanteri dolu!", ProcessState.Warning);
                            break;
                        }

                        // Envantere taşı
                        PacketBuilder.MoveItem(petSlot, (byte)emptySlot, SRTypes.InventoryItemMovement.InventoryToInventory);
                        moved++;
                        System.Threading.Thread.Sleep(80);
                    }
                }
                LogProcess($"Move selected: {moved} adet eşya toplama petinden karaktere aktarıldı.");
            }
            catch (Exception ex)
            {
                LogProcess("Move selected hatası: " + ex.Message, ProcessState.Error);
            }
        }

        private void OrganizePickPetInventory()
        {
            try
            {
                SRCoService pet = InfoManager.MyPets?.Find(p => p != null && p.isPickPet());
                if (pet == null || pet.Inventory == null)
                {
                    LogProcess("Organize: Toplama peti envanteri bulunamadı.", ProcessState.Warning);
                    return;
                }
                LogProcess("Toplama peti envanteri düzenleniyor...");
                UpdatePickPetInventoryUI(pet);
            }
            catch (Exception ex)
            {
                LogProcess("Organize hatası: " + ex.Message, ProcessState.Error);
            }
        }
        #endregion

        #region Canlı UI Güncellemesi (UpdatePetRuntimeStatus)
        public void UpdatePetRuntimeStatus()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired)
            {
                try { this.BeginInvoke((MethodInvoker)UpdatePetRuntimeStatus); } catch { }
                return;
            }

            try
            {
                var pets = InfoManager.MyPets;
                var chr = InfoManager.Character;
                SRCoord myPos = chr?.GetRealtimePosition();

                // 1. Saldırı Peti
                SRCoService atkPet = pets?.Find(p => p != null && p.isAttackPet() && !Script.IsLikelyFellow(p));
                if (atkPet == null) atkPet = pets?.Find(p => p != null && p.isAttackPet());
                if (atkPet != null && PhBot_Pet_AtkNameVal != null)
                {
                    PhBot_Pet_AtkNameVal.Text = string.IsNullOrEmpty(atkPet.Name) ? "İsimsiz" : atkPet.Name;
                    int lvl = 0;
                    if (atkPet is SRAttackPet sap)
                    {
                        lvl = sap.Level;
                        int hgpPct = Math.Max(0, Math.Min(100, (int)(sap.HGP * 0.01)));
                        if (PhBot_Pet_AtkHgpBar != null) PhBot_Pet_AtkHgpBar.Value = hgpPct;
                        if (PhBot_Pet_AtkHgpPct != null) PhBot_Pet_AtkHgpPct.Text = $"{hgpPct}%";
                        if (PhBot_Pet_AtkHgpVal != null) PhBot_Pet_AtkHgpVal.Text = $"{sap.HGP} / 10000";
                    }
                    if (PhBot_Pet_AtkLevelVal != null) PhBot_Pet_AtkLevelVal.Text = lvl.ToString();

                    int hpPct = atkPet.GetHPPercent();
                    if (PhBot_Pet_AtkHpBar != null) PhBot_Pet_AtkHpBar.Value = Math.Max(0, Math.Min(100, hpPct));
                    if (PhBot_Pet_AtkHpPct != null) PhBot_Pet_AtkHpPct.Text = $"{hpPct}%";
                    if (PhBot_Pet_AtkHpVal != null) PhBot_Pet_AtkHpVal.Text = $"{atkPet.HP} / {atkPet.HPMax}";

                    if (PhBot_Pet_AtkStatusVal != null)
                        PhBot_Pet_AtkStatusVal.Text = atkPet.HP == 0 ? "Ölü" : "Aktif";

                    SRCoord pPos = atkPet.GetRealtimePosition();
                    if (PhBot_Pet_AtkPositionVal != null)
                    {
                        if (pPos != null && myPos != null)
                            PhBot_Pet_AtkPositionVal.Text = $"X: {pPos.PosX:F0}, Y: {pPos.PosY:F0} ({myPos.DistanceTo(pPos):F1}m)";
                        else
                            PhBot_Pet_AtkPositionVal.Text = " - ";
                    }

                    // Envanter kontrolü: Revive with, Recovery, HGP food
                    UpdatePetConsumableCounts();
                }
                else if (PhBot_Pet_AtkNameVal != null)
                {
                    PhBot_Pet_AtkNameVal.Text = "İsimsiz";
                    if (PhBot_Pet_AtkLevelVal != null) PhBot_Pet_AtkLevelVal.Text = "0";
                    if (PhBot_Pet_AtkHpBar != null) PhBot_Pet_AtkHpBar.Value = 0;
                    if (PhBot_Pet_AtkHpPct != null) PhBot_Pet_AtkHpPct.Text = "0%";
                    if (PhBot_Pet_AtkHpVal != null) PhBot_Pet_AtkHpVal.Text = " - ";
                    if (PhBot_Pet_AtkHgpBar != null) PhBot_Pet_AtkHgpBar.Value = 0;
                    if (PhBot_Pet_AtkHgpPct != null) PhBot_Pet_AtkHgpPct.Text = "0%";
                    if (PhBot_Pet_AtkHgpVal != null) PhBot_Pet_AtkHgpVal.Text = " - ";
                    if (PhBot_Pet_AtkStatusVal != null) PhBot_Pet_AtkStatusVal.Text = " - ";
                    if (PhBot_Pet_AtkPositionVal != null) PhBot_Pet_AtkPositionVal.Text = " - ";
                    UpdatePetConsumableCounts();
                }

                // 2. Kervan Peti
                SRCoService caravan = pets?.Find(p => p != null && (p.isTransport() || p.isHorse()));
                if (caravan != null && PhBot_Pet_CaravanTypeVal != null)
                {
                    PhBot_Pet_CaravanTypeVal.Text = caravan.isHorse() ? "Binek Atı" : "Kervan Taşıtı";
                    PhBot_Pet_CaravanNameVal.Text = string.IsNullOrEmpty(caravan.Name) ? (caravan.isHorse() ? "At" : "Taşıt") : caravan.Name;
                    PhBot_Pet_CaravanMountedVal.Text = (chr != null && chr.isRiding) ? "Evet" : "Hayır";
                    SRCoord cPos = caravan.GetRealtimePosition();
                    if (cPos != null)
                    {
                        PhBot_Pet_CaravanXVal.Text = ((int)cPos.PosX).ToString();
                        PhBot_Pet_CaravanYVal.Text = ((int)cPos.PosY).ToString();
                    }
                    int cHp = caravan.GetHPPercent();
                    if (PhBot_Pet_CaravanHpBar != null) PhBot_Pet_CaravanHpBar.Value = Math.Max(0, Math.Min(100, cHp));
                    if (PhBot_Pet_CaravanHpPct != null) PhBot_Pet_CaravanHpPct.Text = $"{cHp}%";
                }
                else if (PhBot_Pet_CaravanTypeVal != null)
                {
                    PhBot_Pet_CaravanTypeVal.Text = " - ";
                    PhBot_Pet_CaravanNameVal.Text = " - ";
                    PhBot_Pet_CaravanMountedVal.Text = "Hayır";
                    PhBot_Pet_CaravanXVal.Text = "0";
                    PhBot_Pet_CaravanYVal.Text = "0";
                    if (PhBot_Pet_CaravanHpBar != null) PhBot_Pet_CaravanHpBar.Value = 0;
                    if (PhBot_Pet_CaravanHpPct != null) PhBot_Pet_CaravanHpPct.Text = "0%";
                }

                // 3. Toplama Peti
                SRCoService pickPet = pets?.Find(p => p != null && p.isPickPet());
                if (pickPet != null && PhBot_Pet_PickNameVal != null)
                {
                    PhBot_Pet_PickNameVal.Text = string.IsNullOrEmpty(pickPet.Name) ? "Toplama Peti" : pickPet.Name;
                    SRCoord pPos = pickPet.GetRealtimePosition();
                    if (pPos != null)
                    {
                        PhBot_Pet_PickXVal.Text = ((int)pPos.PosX).ToString();
                        PhBot_Pet_PickYVal.Text = ((int)pPos.PosY).ToString();
                    }
                    PhBot_Pet_PickRemainingTimeVal.Text = "28 Gün";
                    if (pickPet.Inventory != null)
                    {
                        int used = 0;
                        for (int s = 0; s < pickPet.Inventory.Capacity; s++)
                            if (pickPet.Inventory[(byte)s] != null) used++;
                        PhBot_Pet_PickSlotsVal.Text = $"{used}/{pickPet.Inventory.Capacity}";
                    }
                    else
                    {
                        PhBot_Pet_PickSlotsVal.Text = "0/0";
                    }
                    PhBot_Pet_PickAutoPickVal.Text = "Aktif";
                    UpdatePickPetInventoryUI(pickPet);
                }
                else if (PhBot_Pet_PickNameVal != null)
                {
                    PhBot_Pet_PickNameVal.Text = " - ";
                    PhBot_Pet_PickXVal.Text = "0";
                    PhBot_Pet_PickYVal.Text = "0";
                    PhBot_Pet_PickRemainingTimeVal.Text = "N/A";
                    PhBot_Pet_PickSlotsVal.Text = "0/0";
                    PhBot_Pet_PickAutoPickVal.Text = " - ";
                }

                // 4. Fellow Pet
                SRCoService fellow = pets?.Find(p => p != null && Script.IsLikelyFellow(p));
                if (fellow != null && PhBot_Pet_FellowNameVal != null)
                {
                    PhBot_Pet_FellowNameVal.Text = string.IsNullOrEmpty(fellow.Name) ? "Fellow" : fellow.Name;
                    if (fellow is SRAttackPet sfp)
                    {
                        PhBot_Pet_FellowLevelVal.Text = sfp.Level.ToString();
                        int fHgp = Math.Max(0, Math.Min(100, (int)(sfp.HGP * 0.01)));
                        if (PhBot_Pet_FellowHgpBar != null) PhBot_Pet_FellowHgpBar.Value = fHgp;
                        if (PhBot_Pet_FellowHgpPct != null) PhBot_Pet_FellowHgpPct.Text = $"{fHgp}%";
                    }
                    int fHp = fellow.GetHPPercent();
                    if (PhBot_Pet_FellowHpBar != null) PhBot_Pet_FellowHpBar.Value = Math.Max(0, Math.Min(100, fHp));
                    if (PhBot_Pet_FellowHpPct != null) PhBot_Pet_FellowHpPct.Text = $"{fHp}%";
                }
                else if (PhBot_Pet_FellowNameVal != null)
                {
                    PhBot_Pet_FellowNameVal.Text = "No fellow";
                    PhBot_Pet_FellowLevelVal.Text = " - ";
                    PhBot_Pet_FellowStoredSpVal.Text = " - ";
                    if (PhBot_Pet_FellowHpBar != null) PhBot_Pet_FellowHpBar.Value = 0;
                    if (PhBot_Pet_FellowHpPct != null) PhBot_Pet_FellowHpPct.Text = "0%";
                    if (PhBot_Pet_FellowHgpBar != null) PhBot_Pet_FellowHgpBar.Value = 0;
                    if (PhBot_Pet_FellowHgpPct != null) PhBot_Pet_FellowHgpPct.Text = "0%";
                }
            }
            catch { }
        }

        private void UpdatePetConsumableCounts()
        {
            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null) return;

            int reviveCount = 0;
            string reviveName = "-";
            int recovCount = 0;
            string recovName = "-";
            int foodCount = 0;
            string foodName = "-";

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var item = chr.Inventory[s];
                if (item == null) continue;
                string sn = (item.ServerName ?? "").ToUpperInvariant();

                // Revive item (Grass of life, etc.)
                if (sn.Contains("REBIRTH") || sn.Contains("REVIVE") || sn.Contains("RESURRECT") || sn.Contains("LIFE"))
                {
                    reviveCount += Math.Max(1, (int)item.Quantity);
                    reviveName = item.Name ?? "Canlandırma";
                }
                // Pet HP Recovery kit
                if (sn.Contains("RECOVERY") || sn.Contains("POTION_COS") || sn.Contains("HP_RECOVERY"))
                {
                    recovCount += Math.Max(1, (int)item.Quantity);
                    recovName = item.Name ?? "Pet HP Potu";
                }
                // Pet HGP Food
                if (sn.Contains("HGP") || sn.Contains("FEED") || sn.Contains("POUCH") || sn.Contains("FOOD"))
                {
                    foodCount += Math.Max(1, (int)item.Quantity);
                    foodName = item.Name ?? "Pet Yemi";
                }
            }

            if (PhBot_Pet_AtkReviveWithVal != null)
                PhBot_Pet_AtkReviveWithVal.Text = reviveCount > 0 ? $"{reviveName} ({reviveCount})" : " - ";
            if (PhBot_Pet_AtkRecoveryVal != null)
                PhBot_Pet_AtkRecoveryVal.Text = recovCount > 0 ? $"{recovName} ({recovCount})" : " - ";
            if (PhBot_Pet_AtkHgpFoodVal != null)
                PhBot_Pet_AtkHgpFoodVal.Text = foodCount > 0 ? $"{foodName} ({foodCount})" : " - ";
        }

        private void UpdatePickPetInventoryUI(SRCoService pickPet)
        {
            if (PhBot_Pet_PickInventory == null || pickPet == null || pickPet.Inventory == null) return;
            try
            {
                PhBot_Pet_PickInventory.BeginUpdate();
                PhBot_Pet_PickInventory.Items.Clear();

                for (byte s = 0; s < pickPet.Inventory.Capacity; s++)
                {
                    var item = pickPet.Inventory[s];
                    if (item == null) continue;

                    ListViewItem lvi = new ListViewItem((s + 1).ToString());
                    lvi.Tag = s;
                    lvi.SubItems.Add(""); // Resim
                    lvi.SubItems.Add(item.Name ?? item.ServerName ?? "Eşya");
                    lvi.SubItems.Add(item.Quantity.ToString());

                    PhBot_Pet_PickInventory.Items.Add(lvi);
                }
            }
            catch { }
            finally
            {
                PhBot_Pet_PickInventory.EndUpdate();
            }
        }

        public void RefreshPetSettingsControls()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired)
            {
                try { this.BeginInvoke((MethodInvoker)RefreshPetSettingsControls); } catch { }
                return;
            }

            try
            {
                if (PhBot_Pet_UseAttackPet != null) PhBot_Pet_UseAttackPet.Checked = PetPolicy.UseAttackPet;
                if (PhBot_Pet_DontAttackMonsters != null) PhBot_Pet_DontAttackMonsters.Checked = PetPolicy.DontAttackMonsters;
                if (PhBot_Pet_AutoSummon != null) PhBot_Pet_AutoSummon.Checked = PetPolicy.AutoSummonAttackPet;
                if (PhBot_Pet_OnlyInTown != null) PhBot_Pet_OnlyInTown.Checked = PetPolicy.OnlyInTown;
                if (PhBot_Pet_OnlyAtTrainingArea != null) PhBot_Pet_OnlyAtTrainingArea.Checked = PetPolicy.OnlyAtTrainingArea;
                if (PhBot_Pet_AutoRevive != null) PhBot_Pet_AutoRevive.Checked = PetPolicy.AutoReviveAttackPet;
                if (PhBot_Pet_AutoReviveMaxCount != null) PhBot_Pet_AutoReviveMaxCount.Text = PetPolicy.AutoReviveMaxCount.ToString();
                if (PhBot_Pet_OnlyIfHpPotion != null) PhBot_Pet_OnlyIfHpPotion.Checked = PetPolicy.OnlyIfHpPotionPresent;
                if (PhBot_Pet_ReturnTownWhenDies != null) PhBot_Pet_ReturnTownWhenDies.Checked = PetPolicy.ReturnTownWhenAttackPetDies;
                if (PhBot_Pet_ReturnTownOnlyIfNoRevive != null) PhBot_Pet_ReturnTownOnlyIfNoRevive.Checked = PetPolicy.ReturnTownOnlyIfNoRevive;
                if (PhBot_Pet_ProtectPet != null) PhBot_Pet_ProtectPet.Checked = PetPolicy.ProtectAttackPet;
                if (PhBot_Pet_AutoRecallStuck != null) PhBot_Pet_AutoRecallStuck.Checked = PetPolicy.AutoRecallStuckAttackPet;
                if (PhBot_Pet_AttackRadius != null) PhBot_Pet_AttackRadius.Text = PetPolicy.AttackRadius.ToString();

                if (PhBot_Pet_FellowUnique != null) PhBot_Pet_FellowUnique.Checked = PetPolicy.UseFellowUniqueSkills;
                if (PhBot_Pet_FellowGrowth != null) PhBot_Pet_FellowGrowth.Checked = PetPolicy.UsePotionOfGrowth;
                if (PhBot_Pet_FellowReady != null) PhBot_Pet_FellowReady.Checked = PetPolicy.UseFellowReadySkills;
                if (PhBot_Pet_FellowSpRecall != null) PhBot_Pet_FellowSpRecall.Checked = PetPolicy.UseFellowSpRecallInTown;

                UpdatePetRuntimeStatus();
            }
            catch { }
        }
        #endregion
    }
}
