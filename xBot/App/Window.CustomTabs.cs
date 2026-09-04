using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using xBot.Game;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    public partial class Window
    {
        // Top Header Controls
        private Button btnLangTR;
        private Button btnLangEN;
        private Button btnQuickSave;
        private Button btnQuickHideClient;
        private Label lblHeaderLevel;
        private Label lblHeaderHP;
        private Label lblHeaderMP;

        // General & Login Strategy controls
        private CheckBox cbxGeneralAutoLogin;
        private CheckBox cbxGeneralStaticCaptcha;
        private TextBox tbxGeneralStaticCaptchaCode;
        private CheckBox cbxGeneralAutoStart;
        private CheckBox cbxGeneralAutoHide;
        private CheckBox cbxGeneralStayConnected;
        private NumericUpDown nudGeneralLoginDelay;
        private NumericUpDown nudGeneralWaitAfterDC;
        private RadioButton rbnGeneralFirstFound;
        private RadioButton rbnGeneralHighestLevel;

        // Protection controls added at runtime so they can be refreshed after
        // character settings are loaded.
        private CheckBox cbxProtectionSkillHP;
        private NumericUpDown nudProtectionSkillHP;
        private CheckBox cbxProtectionSkillMP;
        private NumericUpDown nudProtectionSkillMP;
        private CheckBox cbxProtectionCure;
        private CheckBox cbxProtectionPetRevive;
        private CheckBox cbxProtectionPetSummon;
        private CheckBox cbxProtectionNoArrows;
        private CheckBox cbxProtectionFullInventory;
        private CheckBox cbxProtectionFullPetInventory;
        private CheckBox cbxProtectionLowHP;
        private CheckBox cbxProtectionLowMP;
        private CheckBox cbxProtectionDurability;
        private CheckBox cbxProtectionLevelUp;
        private CheckBox cbxProtectionStopInTown;
        private CheckBox cbxProtectionDead;
        private NumericUpDown nudProtectionHP;
        private NumericUpDown nudProtectionMP;
        private NumericUpDown nudProtectionDurability;
        private NumericUpDown nudProtectionDeadDelay;

        // Auto Stat controls added at runtime
        private CheckBox cbxAutoStatEnabled;
        private NumericUpDown nudAutoStatSTR;
        private NumericUpDown nudAutoStatINT;
        private Label lblProtectionStatPointsRemain;

        // Item filter controls added at runtime.
        private NumericUpDown nudFilterMinDegree;
        private NumericUpDown nudFilterMaxDegree;
        private CheckBox cbxFilterSox;
        private CheckBox cbxFilterChina;
        private CheckBox cbxFilterEurope;
        private CheckBox cbxFilterMale;
        private CheckBox cbxFilterFemale;
        private TextBox tbxItemRuleName;
        private CheckBox cbxItemRulePickup;
        private CheckBox cbxItemRuleSell;
        private CheckBox cbxItemRuleStore;
        private ListView lstvItemRules;

        // Combat AI controls are hosted under Training > Combat AI.
        private Button btnTrainingCombat;
        private Panel pnlTrainingCombat;
        private CheckBox cbxCombatZerkFullHP;
        private CheckBox cbxCombatIgnorePillars;
        private CheckBox cbxCombatWeakerFirst;
        private CheckBox cbxCombatDoNotFollow;
        private CheckBox cbxCombatZerkCount;
        private CheckBox cbxCombatZerkAvoidance;
        private CheckBox cbxCombatZerkRarity;
        private NumericUpDown nudCombatZerkCount;
        private Label lblSkillRuntimeStatus;
        private ComboBox cmbxImbue;
        private bool refreshingImbueSkills;

        // Custom Panels
        public Panel pnlCustomGeneral;
        public Panel pnlCustomCombat;
        public Panel pnlCustomProtection;
        public Panel pnlCustomSkills;
        public Panel pnlCustomFilter;

        public void InitializeCustomUBOTFeatures()
        {
            try
            {
                LocalizationManager.OnLanguageChanged += ApplyLanguageToWindow;
                BuildHeaderWidgets();
                BuildGeneralTabWidgets();
                BuildCombatTabWidgets();
                BuildSkillsTabWidgets();
                BuildProtectionTabWidgets();
                BuildItemFilterWidgets();
                ApplyLanguageToWindow();
            }
            catch (Exception ex)
            {
                Log("[Custom UI Init Error] " + ex.Message);
            }
        }

        private void BuildHeaderWidgets()
        {
            if (this.pnlHeader == null) return;

            // Language switcher (TR / EN)
            btnLangTR = new Button();
            btnLangTR.Text = "TR";
            btnLangTR.Size = new Size(32, 22);
            btnLangTR.Location = new Point(pnlHeader.Width - 260, 4);
            btnLangTR.FlatStyle = FlatStyle.Flat;
            btnLangTR.FlatAppearance.BorderSize = 0;
            btnLangTR.BackColor = Color.FromArgb(0, 122, 204);
            btnLangTR.ForeColor = Color.White;
            btnLangTR.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLangTR.Click += (s, e) => { LocalizationManager.SetLanguage("TR"); Log("Dil Türkçe olarak ayarlandı."); };
            pnlHeader.Controls.Add(btnLangTR);

            btnLangEN = new Button();
            btnLangEN.Text = "EN";
            btnLangEN.Size = new Size(32, 22);
            btnLangEN.Location = new Point(pnlHeader.Width - 225, 4);
            btnLangEN.FlatStyle = FlatStyle.Flat;
            btnLangEN.FlatAppearance.BorderSize = 0;
            btnLangEN.BackColor = Color.FromArgb(50, 50, 55);
            btnLangEN.ForeColor = Color.LightGray;
            btnLangEN.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLangEN.Click += (s, e) => { LocalizationManager.SetLanguage("EN"); Log("Language set to English."); };
            pnlHeader.Controls.Add(btnLangEN);

            // Hide/Show client quick icon
            btnQuickHideClient = new Button();
            btnQuickHideClient.Text = "👁";
            btnQuickHideClient.Size = new Size(26, 22);
            btnQuickHideClient.Location = new Point(pnlHeader.Width - 190, 4);
            btnQuickHideClient.FlatStyle = FlatStyle.Flat;
            btnQuickHideClient.FlatAppearance.BorderSize = 0;
            btnQuickHideClient.BackColor = Color.FromArgb(45, 45, 48);
            btnQuickHideClient.ForeColor = Color.LightGray;
            btnQuickHideClient.Click += (s, e) =>
            {
                if (ClientManager.IsClientHidden)
                {
                    ClientManager.ShowClient();
                    Log("Game client visible.");
                }
                else
                {
                    ClientManager.HideClient();
                    Log("Game client hidden.");
                }
            };
            pnlHeader.Controls.Add(btnQuickHideClient);

            // Quick Save button
            btnQuickSave = new Button();
            btnQuickSave.Text = "💾";
            btnQuickSave.Size = new Size(26, 22);
            btnQuickSave.Location = new Point(pnlHeader.Width - 160, 4);
            btnQuickSave.FlatStyle = FlatStyle.Flat;
            btnQuickSave.FlatAppearance.BorderSize = 0;
            btnQuickSave.BackColor = Color.FromArgb(45, 45, 48);
            btnQuickSave.ForeColor = Color.LightGreen;
            btnQuickSave.Click += (s, e) =>
            {
                Settings.SaveBotSettings();
                Settings.SaveCharacterSettings();
                Log("All settings saved successfully.");
            };
            pnlHeader.Controls.Add(btnQuickSave);

            // Header mini stat labels
            lblHeaderLevel = new Label();
            lblHeaderLevel.Text = "LVL: --";
            lblHeaderLevel.ForeColor = Color.Gold;
            lblHeaderLevel.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblHeaderLevel.AutoSize = true;
            lblHeaderLevel.Location = new Point(pnlHeader.Width - 480, 8);
            pnlHeader.Controls.Add(lblHeaderLevel);

            lblHeaderHP = new Label();
            lblHeaderHP.Text = "HP: 100%";
            lblHeaderHP.ForeColor = Color.LimeGreen;
            lblHeaderHP.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblHeaderHP.AutoSize = true;
            lblHeaderHP.Location = new Point(pnlHeader.Width - 420, 8);
            pnlHeader.Controls.Add(lblHeaderHP);

            lblHeaderMP = new Label();
            lblHeaderMP.Text = "MP: 100%";
            lblHeaderMP.ForeColor = Color.DeepSkyBlue;
            lblHeaderMP.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblHeaderMP.AutoSize = true;
            lblHeaderMP.Location = new Point(pnlHeader.Width - 350, 8);
            pnlHeader.Controls.Add(lblHeaderMP);
        }

        private void BuildGeneralTabWidgets()
        {
            if (this.TabPageV_Control01_Login_Panel == null) return;

            // Hide unused advertising banner so the lower-right area is clean for strategy settings
            if (Login_gbxAdvertising != null)
                Login_gbxAdvertising.Visible = false;

            GroupBox gbxStrategy = new GroupBox();
            gbxStrategy.Text = "Giriş Akışı & Stratejisi (Login Flow & Strategy)";
            gbxStrategy.ForeColor = Color.FromArgb(0, 122, 204);
            gbxStrategy.Location = new Point(234, 184);
            gbxStrategy.Size = new Size(415, 182);
            gbxStrategy.Font = new Font("Segoe UI", 8.5F);

            cbxGeneralAutoLogin = new CheckBox { Text = "Otomatik Giriş", Location = new Point(12, 20), AutoSize = true, Checked = LoginStrategyManager.AutomatedLogin, ForeColor = Color.White };
            cbxGeneralAutoLogin.CheckedChanged += (s, e) => { LoginStrategyManager.AutomatedLogin = cbxGeneralAutoLogin.Checked; Settings.SaveBotSettings(); };

            cbxGeneralAutoStart = new CheckBox { Text = "Oyunda Botu Başlat", Location = new Point(140, 20), AutoSize = true, Checked = LoginStrategyManager.AutoStartBot, ForeColor = Color.White };
            cbxGeneralAutoStart.CheckedChanged += (s, e) => { LoginStrategyManager.AutoStartBot = cbxGeneralAutoStart.Checked; Settings.SaveBotSettings(); };

            cbxGeneralAutoHide = new CheckBox { Text = "İstemciyi Gizle", Location = new Point(285, 20), AutoSize = true, Checked = LoginStrategyManager.AutoHideClient, ForeColor = Color.White };
            cbxGeneralAutoHide.CheckedChanged += (s, e) => { LoginStrategyManager.AutoHideClient = cbxGeneralAutoHide.Checked; Settings.SaveBotSettings(); };

            cbxGeneralStaticCaptcha = new CheckBox { Text = "Sabit Captcha:", Location = new Point(12, 47), AutoSize = true, Checked = LoginStrategyManager.StaticCaptcha, ForeColor = Color.White };
            cbxGeneralStaticCaptcha.CheckedChanged += (s, e) => { LoginStrategyManager.StaticCaptcha = cbxGeneralStaticCaptcha.Checked; Settings.SaveBotSettings(); };

            tbxGeneralStaticCaptchaCode = new TextBox { Location = new Point(115, 45), Size = new Size(55, 21), BackColor = Color.FromArgb(45, 45, 48), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = LoginStrategyManager.StaticCaptchaCode ?? "" };
            tbxGeneralStaticCaptchaCode.TextChanged += (s, e) => { LoginStrategyManager.StaticCaptchaCode = tbxGeneralStaticCaptchaCode.Text; Settings.SaveBotSettings(); };
            if (ToolTips != null)
                ToolTips.SetToolTip(tbxGeneralStaticCaptchaCode, "Sabit captcha kodu (örn: 1234)");

            Label lblLoginDelay = new Label { Text = "Giriş Gecikmesi:", Location = new Point(185, 48), AutoSize = true, ForeColor = Color.LightGray };
            nudGeneralLoginDelay = new NumericUpDown { Location = new Point(280, 46), Size = new Size(42, 21), Minimum = 0, Maximum = 60, Value = LoginStrategyManager.LoginDelaySeconds };
            nudGeneralLoginDelay.ValueChanged += (s, e) => { LoginStrategyManager.LoginDelaySeconds = (int)nudGeneralLoginDelay.Value; Settings.SaveBotSettings(); };
            Label lblLoginDelaySec = new Label { Text = "sn", Location = new Point(325, 48), AutoSize = true, ForeColor = Color.LightGray };

            Label lblWaitDC = new Label { Text = "DC Bekleme:", Location = new Point(12, 75), AutoSize = true, ForeColor = Color.LightGray };
            nudGeneralWaitAfterDC = new NumericUpDown { Location = new Point(90, 73), Size = new Size(42, 21), Minimum = 0, Maximum = 60, Value = LoginStrategyManager.WaitAfterDCMinutes };
            nudGeneralWaitAfterDC.ValueChanged += (s, e) => { LoginStrategyManager.WaitAfterDCMinutes = (int)nudGeneralWaitAfterDC.Value; Settings.SaveBotSettings(); };
            Label lblWaitDCMin = new Label { Text = "dk", Location = new Point(135, 75), AutoSize = true, ForeColor = Color.LightGray };

            cbxGeneralStayConnected = new CheckBox { Text = "Çökme Koruması (Failover to Clientless)", Location = new Point(185, 74), AutoSize = true, Checked = LoginStrategyManager.StayConnected, ForeColor = Color.LightSkyBlue };
            cbxGeneralStayConnected.CheckedChanged += (s, e) => { LoginStrategyManager.StayConnected = cbxGeneralStayConnected.Checked; Settings.SaveBotSettings(); };
            if (ToolTips != null)
                ToolTips.SetToolTip(cbxGeneralStayConnected, "İstemci aniden çökerse sunucu bağlantısını koparmadan Clientless moda geçer");

            Label lblCharStrategy = new Label { Text = "Karakter:", Location = new Point(12, 105), AutoSize = true, ForeColor = Color.LightGray };
            rbnGeneralFirstFound = new RadioButton { Text = "İlk Karakter", Location = new Point(75, 103), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.FirstFound), ForeColor = Color.LightGray };
            rbnGeneralHighestLevel = new RadioButton { Text = "En Yüksek Seviyeli Karakter", Location = new Point(165, 103), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.HighestLevel), ForeColor = Color.Gold };

            rbnGeneralFirstFound.CheckedChanged += (s, e) => { if (rbnGeneralFirstFound.Checked) { LoginStrategyManager.Strategy = CharacterSelectionStrategy.FirstFound; Settings.SaveBotSettings(); } };
            rbnGeneralHighestLevel.CheckedChanged += (s, e) => { if (rbnGeneralHighestLevel.Checked) { LoginStrategyManager.Strategy = CharacterSelectionStrategy.HighestLevel; Settings.SaveBotSettings(); } };

            Label lblStrategyInfo = new Label
            {
                Text = "Giriş akışı, otomatik captcha, gecikmeler ve çökme koruması buradan yönetilir.",
                Location = new Point(12, 133),
                Size = new Size(390, 35),
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 7.5F)
            };

            gbxStrategy.Controls.AddRange(new Control[] {
                cbxGeneralAutoLogin, cbxGeneralAutoStart, cbxGeneralAutoHide,
                cbxGeneralStaticCaptcha, tbxGeneralStaticCaptchaCode,
                lblLoginDelay, nudGeneralLoginDelay, lblLoginDelaySec,
                lblWaitDC, nudGeneralWaitAfterDC, lblWaitDCMin,
                cbxGeneralStayConnected,
                lblCharStrategy, rbnGeneralFirstFound, rbnGeneralHighestLevel,
                lblStrategyInfo
            });
            TabPageV_Control01_Login_Panel.Controls.Add(gbxStrategy);
        }

        private void BuildCombatTabWidgets()
        {
            if (this.TabPageH_Training == null || this.TabPageV_Control01_Training_Panel == null
                || Combat_gbxAI == null || Combat_gbxMobFilter == null)
                return;

            EnsureTrainingCombatTab();

            // The designer groups used to live under Town. Re-parenting them
            // keeps the existing target filters while moving all combat UI to
            // the Training section requested by the user.
            Combat_gbxAI.Location = new Point(10, 8);
            Combat_gbxAI.Size = new Size(305, 325);
            Combat_gbxMobFilter.Location = new Point(325, 8);
            Combat_gbxMobFilter.Size = new Size(320, 325);
            pnlTrainingCombat.Controls.Add(Combat_gbxAI);
            pnlTrainingCombat.Controls.Add(Combat_gbxMobFilter);

            if (TabPageH_Town_Option02 != null)
                TabPageH_Town_Option02.Visible = false;
            if (TabPageH_Town_Option02_Panel != null)
                TabPageH_Town_Option02_Panel.Visible = false;
            ResizeTrainingTab(TabPageH_Town_Option01, 0, 328);
            ResizeTrainingTab(TabPageH_Town_Option03, 329, 328);

            // Keep the original controls readable and reserve the lower part
            // of the group for the advanced settings.
            if (Combat_lblInfo != null)
            {
                Combat_lblInfo.Location = new Point(15, 288);
                Combat_lblInfo.Size = new Size(275, 30);
                Combat_lblInfo.Text = "Öncelik, zerk ve alan dışı takip davranışı burada yönetilir.";
            }

            cbxCombatZerkFullHP = CreateCombatCheck("HP %100 iken berserk", new Point(15, 145), CombatAIEngine.ZerkWhenHPFull, value => CombatAIEngine.ZerkWhenHPFull = value);
            cbxCombatIgnorePillars = CreateCombatCheck("Dimension pillar yok say", new Point(15, 169), CombatAIEngine.IgnoreDimensionPillars, value => CombatAIEngine.IgnoreDimensionPillars = value);
            cbxCombatWeakerFirst = CreateCombatCheck("Önce zayıf mob", new Point(15, 193), CombatAIEngine.AttackWeakerFirst, value => CombatAIEngine.AttackWeakerFirst = value);
            cbxCombatDoNotFollow = CreateCombatCheck("Yarıçap dışını takip etme", new Point(15, 217), CombatAIEngine.DoNotFollowMobs, value => CombatAIEngine.DoNotFollowMobs = value);
            cbxCombatZerkCount = CreateCombatCheck("Mob sayısında zerk", new Point(15, 241), CombatAIEngine.ZerkMonsterCountEnabled, value => CombatAIEngine.ZerkMonsterCountEnabled = value);
            cbxCombatZerkCount.Size = new Size(125, 20);
            nudCombatZerkCount = new NumericUpDown
            {
                Location = new Point(145, 239),
                Size = new Size(45, 20),
                Minimum = 1,
                Maximum = 50,
                Value = Math.Max(1, Math.Min(50, CombatAIEngine.ZerkMonsterCount))
            };
            nudCombatZerkCount.ValueChanged += (s, e) => CombatAIEngine.ZerkMonsterCount = (int)nudCombatZerkCount.Value;
            cbxCombatZerkAvoidance = CreateCombatCheck("Avoidance zerk", new Point(15, 265), CombatAIEngine.ZerkAvoidanceBased, value => CombatAIEngine.ZerkAvoidanceBased = value);
            cbxCombatZerkAvoidance.Size = new Size(125, 20);
            cbxCombatZerkRarity = CreateCombatCheck("Rarity zerk", new Point(145, 265), CombatAIEngine.ZerkRarityBased, value => CombatAIEngine.ZerkRarityBased = value);
            cbxCombatZerkRarity.Size = new Size(110, 20);

            Combat_gbxAI.Controls.AddRange(new Control[] {
                cbxCombatZerkFullHP, cbxCombatIgnorePillars, cbxCombatWeakerFirst,
                cbxCombatDoNotFollow, cbxCombatZerkCount, nudCombatZerkCount,
                cbxCombatZerkAvoidance, cbxCombatZerkRarity
            });
        }

        private void EnsureTrainingCombatTab()
        {
            if (btnTrainingCombat != null && pnlTrainingCombat != null)
                return;

            const int tabWidth = 164;
            ResizeTrainingTab(TabPageH_Training_Option01, 0, tabWidth);
            ResizeTrainingTab(TabPageH_Training_Option02, tabWidth, tabWidth);
            ResizeTrainingTab(TabPageH_Training_Option03, tabWidth * 2, tabWidth);

            btnTrainingCombat = new Button
            {
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(45, 45, 48),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Microsoft Sans Serif", 13F, FontStyle.Regular, GraphicsUnit.Pixel),
                Location = new Point(tabWidth * 3, 0),
                Margin = new Padding(0),
                Name = "TabPageH_Training_Option04",
                Size = new Size(tabWidth, 26),
                TabIndex = 15,
                Tag = "Source Sans Pro",
                Text = "Combat AI",
                UseVisualStyleBackColor = false
            };
            btnTrainingCombat.FlatAppearance.BorderSize = 0;
            btnTrainingCombat.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 122, 204);
            btnTrainingCombat.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 151, 234);
            btnTrainingCombat.Click += TabPageH_Option_Click;
            TabPageH_Training.Controls.Add(btnTrainingCombat);

            pnlTrainingCombat = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 27),
                Name = "TabPageH_Training_Option04_Panel",
                Size = new Size(657, 345),
                TabIndex = 28,
                Visible = false
            };
            TabPageV_Control01_Training_Panel.Controls.Add(pnlTrainingCombat);
        }

        private void ResizeTrainingTab(Button tab, int x, int width)
        {
            if (tab == null)
                return;

            tab.Location = new Point(x, 0);
            tab.Size = new Size(width, tab.Height);
        }

        private CheckBox CreateCombatCheck(string text, Point location, bool isChecked, Action<bool> changed)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = text,
                Location = location,
                AutoSize = false,
                Size = new Size(275, 20),
                Checked = isChecked,
                ForeColor = Color.White
            };
            checkBox.CheckedChanged += (s, e) => changed(checkBox.Checked);
            return checkBox;
        }

        private void BuildSkillsTabWidgets()
        {
            if (this.TabPageV_Control01_Skills_Panel == null) return;

            Skills_cbxCastInOrder.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            Skills_cbxCastInOrder.ForeColor = Color.White;
            Skills_cbxCastInOrder.Checked = SkillManager.InOrderCombo;
            Skills_cbxCastInOrder.CheckedChanged += (s, e) =>
            {
                SkillManager.InOrderCombo = Skills_cbxCastInOrder.Checked;
                Settings.SaveCharacterSettings();
            };

            // Move Up and Down buttons
            Button btnSkillUp = new Button { Text = "▲", Size = new Size(30, 26), Location = new Point(350, 290), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(0, 122, 204) };
            btnSkillUp.Click += (s, e) => SkillManager.MoveSelectedItemUp(Skills_lstvSkills);

            Button btnSkillDown = new Button { Text = "▼", Size = new Size(30, 26), Location = new Point(385, 290), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(0, 122, 204) };
            btnSkillDown.Click += (s, e) => SkillManager.MoveSelectedItemDown(Skills_lstvSkills);

            // Imbue Selector
            Label lblImbue = new Label { Text = "Çin Imbue:", Location = new Point(285, 222), AutoSize = true, ForeColor = Color.LightGray };
            cmbxImbue = new ComboBox { Location = new Point(355, 219), Size = new Size(100, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbxImbue.DropDownWidth = 260;
            cmbxImbue.SelectedIndexChanged += (s, e) =>
            {
                if (refreshingImbueSkills)
                    return;

                SkillManager.ImbueSkillOption selected = cmbxImbue.SelectedItem as SkillManager.ImbueSkillOption;
                if (selected == null)
                    return;

                SkillManager.SelectedImbueSkillId = selected.SkillId;
                SkillManager.SelectedImbue = selected.Element;
                Settings.SaveCharacterSettings();
            };

            // No attack checkbox
            CheckBox cbxNoAttack = new CheckBox { Text = "Saldırı Yapma (No Attack / Support)", Location = new Point(10, 218), AutoSize = true, Checked = SkillManager.NoAttackMode, ForeColor = Color.White };
            cbxNoAttack.CheckedChanged += (s, e) => { SkillManager.NoAttackMode = cbxNoAttack.Checked; Settings.SaveCharacterSettings(); };

            // Devil Spirit checkbox
            CheckBox cbxDevil = new CheckBox { Text = "Devil Spirit Becerisi Kullan", Location = new Point(10, 245), AutoSize = true, Checked = SkillManager.UseDevilSpirit, ForeColor = Color.White };
            cbxDevil.CheckedChanged += (s, e) => { SkillManager.UseDevilSpirit = cbxDevil.Checked; Settings.SaveCharacterSettings(); };

            lblSkillRuntimeStatus = new Label
            {
                AutoEllipsis = true,
                Location = new Point(10, 292),
                Size = new Size(330, 22),
                ForeColor = Color.LightSkyBlue,
                Text = "Skill durumu: Hazır"
            };
            if (ToolTips != null)
                ToolTips.SetToolTip(lblSkillRuntimeStatus, "Son skill denemesinin sonucu ve fallback durumu");

            TabPageH_Skills_Option01_Panel.Controls.AddRange(new Control[]
            {
                btnSkillUp, btnSkillDown, lblImbue, cmbxImbue, cbxNoAttack, cbxDevil,
                lblSkillRuntimeStatus
            });
            RefreshImbueSkillList();
        }

        private void RefreshImbueSkillList()
        {
            if (cmbxImbue == null)
                return;

            System.Collections.Generic.List<SkillManager.ImbueSkillOption> options = SkillManager.GetAvailableImbueSkills();
            SkillManager.ImbueSkillOption selected = options.FirstOrDefault(option => option.SkillId == SkillManager.SelectedImbueSkillId);
            if (selected == null)
            {
                string legacyElement = SkillManager.NormalizeImbueSelection(SkillManager.SelectedImbue);
                selected = options.FirstOrDefault(option => option.Element == legacyElement) ?? options.FirstOrDefault();
            }

            refreshingImbueSkills = true;
            try
            {
                cmbxImbue.BeginUpdate();
                cmbxImbue.Items.Clear();
                foreach (SkillManager.ImbueSkillOption option in options)
                    cmbxImbue.Items.Add(option);

                cmbxImbue.Enabled = options.Count > 0;
                if (selected != null)
                {
                    cmbxImbue.SelectedItem = selected;
                    SkillManager.SelectedImbueSkillId = selected.SkillId;
                    SkillManager.SelectedImbue = selected.Element;
                }
                else
                {
                    cmbxImbue.Text = "Çin imbue bulunamadı";
                }
            }
            finally
            {
                cmbxImbue.EndUpdate();
                refreshingImbueSkills = false;
            }
        }

        private void BuildProtectionTabWidgets()
        {
            if (this.TabPageV_Control01_Character_Panel == null) return;

            GroupBox gbxSkillPet = new GroupBox
            {
                Text = "Skill ve Pet Koruması",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(10, 10),
                Size = new Size(305, 145),
                Font = new Font("Segoe UI", 8.5F)
            };

            cbxProtectionSkillHP = CreateProtectionCheck("HP < %:", new Point(12, 22), ProtectionManager.UseSkillHP, value => { ProtectionManager.UseSkillHP = value; Settings.SaveCharacterSettings(); });
            cbxProtectionSkillHP.Size = new Size(80, 20);
            nudProtectionSkillHP = CreateProtectionNumber(new Point(95, 21), 1, 99, ProtectionManager.SkillHPPercent, value => { ProtectionManager.SkillHPPercent = (byte)value; Settings.SaveCharacterSettings(); });
            nudProtectionSkillHP.Size = new Size(42, 20);
            Label lblSkillHPSuffix = new Label { Text = "% ise skill bas", Location = new Point(140, 23), AutoSize = true, ForeColor = Color.LightGray };

            cbxProtectionSkillMP = CreateProtectionCheck("MP < %:", new Point(12, 46), ProtectionManager.UseSkillMP, value => { ProtectionManager.UseSkillMP = value; Settings.SaveCharacterSettings(); });
            cbxProtectionSkillMP.Size = new Size(80, 20);
            nudProtectionSkillMP = CreateProtectionNumber(new Point(95, 45), 1, 99, ProtectionManager.SkillMPPercent, value => { ProtectionManager.SkillMPPercent = (byte)value; Settings.SaveCharacterSettings(); });
            nudProtectionSkillMP.Size = new Size(42, 20);
            Label lblSkillMPSuffix = new Label { Text = "% ise skill bas", Location = new Point(140, 47), AutoSize = true, ForeColor = Color.LightGray };

            cbxProtectionCure = CreateProtectionCheck("Kötü durumu skill ile temizle", new Point(12, 70), ProtectionManager.UseSkillBadStatus, value => { ProtectionManager.UseSkillBadStatus = value; Settings.SaveCharacterSettings(); });
            cbxProtectionPetRevive = CreateProtectionCheck("Ölen peti dirilt (Grass of Life)", new Point(12, 94), ProtectionManager.RevivePet, value => { ProtectionManager.RevivePet = value; Settings.SaveCharacterSettings(); });
            cbxProtectionPetSummon = CreateProtectionCheck("Peti otomatik çağır", new Point(12, 118), ProtectionManager.AutoSummonPet, value => { ProtectionManager.AutoSummonPet = value; Settings.SaveCharacterSettings(); });

            gbxSkillPet.Controls.AddRange(new Control[] {
                cbxProtectionSkillHP, nudProtectionSkillHP, lblSkillHPSuffix,
                cbxProtectionSkillMP, nudProtectionSkillMP, lblSkillMPSuffix,
                cbxProtectionCure, cbxProtectionPetRevive, cbxProtectionPetSummon
            });

            GroupBox gbxAutoStat = new GroupBox
            {
                Text = "Otomatik Stat Dağıtımı (Auto Stat)",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(10, 160),
                Size = new Size(305, 175),
                Font = new Font("Segoe UI", 8.5F)
            };

            cbxAutoStatEnabled = new CheckBox
            {
                Text = "Level atlayınca otomatik stat dağıt",
                Location = new Point(12, 22),
                AutoSize = true,
                Checked = StatPointManager.AutoDistributeEnabled,
                ForeColor = Color.White
            };
            cbxAutoStatEnabled.CheckedChanged += (s, e) =>
            {
                StatPointManager.AutoDistributeEnabled = cbxAutoStatEnabled.Checked;
                if (Character_cbxAutoStat != null && Character_cbxAutoStat.Checked != cbxAutoStatEnabled.Checked)
                    Character_cbxAutoStat.Checked = cbxAutoStatEnabled.Checked;
                Settings.SaveCharacterSettings();
            };

            Label lblStr = new Label { Text = "STR:", Location = new Point(12, 50), AutoSize = true, ForeColor = Color.LightSkyBlue };
            nudAutoStatSTR = new NumericUpDown { Location = new Point(48, 48), Size = new Size(42, 20), Minimum = 0, Maximum = 3, Value = StatPointManager.TargetSTR };
            nudAutoStatSTR.ValueChanged += (s, e) =>
            {
                StatPointManager.TargetSTR = (int)nudAutoStatSTR.Value;
                Settings.SaveCharacterSettings();
            };

            Label lblInt = new Label { Text = "INT:", Location = new Point(105, 50), AutoSize = true, ForeColor = Color.Gold };
            nudAutoStatINT = new NumericUpDown { Location = new Point(140, 48), Size = new Size(42, 20), Minimum = 0, Maximum = 3, Value = StatPointManager.TargetINT };
            nudAutoStatINT.ValueChanged += (s, e) =>
            {
                StatPointManager.TargetINT = (int)nudAutoStatINT.Value;
                Settings.SaveCharacterSettings();
            };

            Button btnDistributeNow = new Button
            {
                Text = "Puanları Dağıt",
                Location = new Point(190, 46),
                Size = new Size(105, 24),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            btnDistributeNow.FlatAppearance.BorderSize = 0;
            btnDistributeNow.Click += (s, e) =>
            {
                StatPointManager.CheckAndDistribute();
                UpdateProtectionStatus();
            };

            Label lblStatGuidance = new Label
            {
                Text = "Her seviyede gelen 3 stat puanı seçilen oranda verilir.\n• 0 STR / 3 INT = Pure INT\n• 3 STR / 0 INT = Pure STR\n• 1 STR / 2 INT = Hibrit",
                Location = new Point(12, 78),
                Size = new Size(280, 58),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 7.5F)
            };

            Label lblRemainInfo = new Label { Text = "Kalan Boş Stat Puanı:", Location = new Point(12, 145), AutoSize = true, ForeColor = Color.LightSkyBlue };
            lblProtectionStatPointsRemain = new Label { Text = "--", Location = new Point(140, 145), AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };

            gbxAutoStat.Controls.AddRange(new Control[] {
                cbxAutoStatEnabled, lblStr, nudAutoStatSTR, lblInt, nudAutoStatINT,
                btnDistributeNow, lblStatGuidance, lblRemainInfo, lblProtectionStatPointsRemain
            });

            // Wire Info tab's existing auto-stat controls so both tabs stay in sync
            if (Character_cbxAutoStat != null)
            {
                Character_cbxAutoStat.Checked = StatPointManager.AutoDistributeEnabled;
                Character_cbxAutoStat.CheckedChanged += (s, e) =>
                {
                    StatPointManager.AutoDistributeEnabled = Character_cbxAutoStat.Checked;
                    if (cbxAutoStatEnabled != null && cbxAutoStatEnabled.Checked != Character_cbxAutoStat.Checked)
                        cbxAutoStatEnabled.Checked = Character_cbxAutoStat.Checked;
                    Settings.SaveCharacterSettings();
                };
            }
            if (Character_rbnAutoSTR != null)
            {
                Character_rbnAutoSTR.Checked = (StatPointManager.TargetSTR > StatPointManager.TargetINT);
                Character_rbnAutoSTR.CheckedChanged += (s, e) =>
                {
                    if (Character_rbnAutoSTR.Checked)
                    {
                        StatPointManager.TargetSTR = 3;
                        StatPointManager.TargetINT = 0;
                        if (nudAutoStatSTR != null) nudAutoStatSTR.Value = 3;
                        if (nudAutoStatINT != null) nudAutoStatINT.Value = 0;
                        Settings.SaveCharacterSettings();
                    }
                };
            }
            if (Character_rbnAutoINT != null)
            {
                Character_rbnAutoINT.Checked = (StatPointManager.TargetINT >= StatPointManager.TargetSTR);
                Character_rbnAutoINT.CheckedChanged += (s, e) =>
                {
                    if (Character_rbnAutoINT.Checked)
                    {
                        StatPointManager.TargetSTR = 0;
                        StatPointManager.TargetINT = 3;
                        if (nudAutoStatSTR != null) nudAutoStatSTR.Value = 0;
                        if (nudAutoStatINT != null) nudAutoStatINT.Value = 3;
                        Settings.SaveCharacterSettings();
                    }
                };
            }

            GroupBox gbxReturn = new GroupBox
            {
                Text = "Kasabaya Dönüş Koşulları",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(325, 10),
                Size = new Size(320, 180),
                Font = new Font("Segoe UI", 8.5F)
            };

            cbxProtectionNoArrows = CreateProtectionCheck("Ok / bolt bitince dön", new Point(12, 22), ProtectionManager.ReturnNoArrows, value => { ProtectionManager.ReturnNoArrows = value; Settings.SaveCharacterSettings(); });
            cbxProtectionFullInventory = CreateProtectionCheck("Çanta dolunca dön", new Point(12, 46), ProtectionManager.ReturnFullInventory, value => { ProtectionManager.ReturnFullInventory = value; Settings.SaveCharacterSettings(); });
            cbxProtectionFullPetInventory = CreateProtectionCheck("Pet çantası dolunca dön", new Point(12, 70), ProtectionManager.ReturnFullPetInventory, value => { ProtectionManager.ReturnFullPetInventory = value; Settings.SaveCharacterSettings(); });
            cbxProtectionLowHP = CreateProtectionCheck("HP stoğu düşük", new Point(12, 94), ProtectionManager.ReturnHPLow, value => { ProtectionManager.ReturnHPLow = value; Settings.SaveCharacterSettings(); });
            cbxProtectionLowMP = CreateProtectionCheck("MP stoğu düşük", new Point(12, 118), ProtectionManager.ReturnMPLow, value => { ProtectionManager.ReturnMPLow = value; Settings.SaveCharacterSettings(); });

            cbxProtectionDurability = CreateProtectionCheck("Durability düşük", new Point(165, 22), ProtectionManager.ReturnDurabilityLow, value => { ProtectionManager.ReturnDurabilityLow = value; Settings.SaveCharacterSettings(); });
            cbxProtectionLevelUp = CreateProtectionCheck("Level-up sonrası dön", new Point(165, 46), ProtectionManager.ReturnLevelUp, value => { ProtectionManager.ReturnLevelUp = value; Settings.SaveCharacterSettings(); });
            cbxProtectionStopInTown = CreateProtectionCheck("Şehirde botu durdur", new Point(165, 70), ProtectionManager.StopBotInTown, value => { ProtectionManager.StopBotInTown = value; Settings.SaveCharacterSettings(); });
            cbxProtectionDead = CreateProtectionCheck("Ölüm sonrası dön", new Point(165, 94), ProtectionManager.ReturnDeadWithDelay, value => { ProtectionManager.ReturnDeadWithDelay = value; Settings.SaveCharacterSettings(); });

            cbxProtectionLowHP.Size = new Size(110, 20);
            cbxProtectionLowMP.Size = new Size(110, 20);
            cbxProtectionDurability.Size = new Size(90, 20);
            cbxProtectionLevelUp.Size = new Size(130, 20);
            cbxProtectionStopInTown.Size = new Size(130, 20);
            cbxProtectionDead.Size = new Size(100, 20);

            nudProtectionHP = CreateProtectionNumber(new Point(125, 93), 0, 100, ProtectionManager.HPLowThreshold, value => { ProtectionManager.HPLowThreshold = value; Settings.SaveCharacterSettings(); });
            nudProtectionMP = CreateProtectionNumber(new Point(125, 117), 0, 100, ProtectionManager.MPLowThreshold, value => { ProtectionManager.MPLowThreshold = value; Settings.SaveCharacterSettings(); });
            nudProtectionDurability = CreateProtectionNumber(new Point(260, 21), 0, 100, ProtectionManager.DurabilityLowThreshold, value => { ProtectionManager.DurabilityLowThreshold = value; Settings.SaveCharacterSettings(); });
            nudProtectionDurability.Size = new Size(40, 20);

            Label lblDeadDelay = new Label { Text = "Gecikme (sn):", Location = new Point(165, 120), AutoSize = true, ForeColor = Color.LightGray };
            nudProtectionDeadDelay = CreateProtectionNumber(new Point(250, 117), 0, 3600, ProtectionManager.DeadDelaySeconds, value => { ProtectionManager.DeadDelaySeconds = value; Settings.SaveCharacterSettings(); });
            gbxReturn.Controls.AddRange(new Control[] {
                cbxProtectionNoArrows, cbxProtectionFullInventory, cbxProtectionFullPetInventory,
                cbxProtectionLowHP, cbxProtectionLowMP, cbxProtectionDurability, cbxProtectionLevelUp,
                cbxProtectionStopInTown, cbxProtectionDead, nudProtectionHP, nudProtectionMP,
                nudProtectionDurability, lblDeadDelay, nudProtectionDeadDelay
            });

            GroupBox gbxProtectionSummary = new GroupBox
            {
                Text = "Hızlı Eylemler & Durum",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(325, 200),
                Size = new Size(320, 135),
                Font = new Font("Segoe UI", 8.5F)
            };

            Button btnManualPetProtection = new Button
            {
                Text = "Peti Kontrol Et (Dirilt/Çağır)",
                Location = new Point(15, 24),
                Size = new Size(185, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204)
            };
            btnManualPetProtection.FlatAppearance.BorderSize = 0;
            btnManualPetProtection.Click += (s, e) =>
            {
                ProtectionManager.CheckPetProtection();
                Log("Pet kontrolü tetiklendi.");
            };

            Button btnSaveProtSettings = new Button
            {
                Text = "Kaydet",
                Location = new Point(210, 24),
                Size = new Size(95, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 137, 70)
            };
            btnSaveProtSettings.FlatAppearance.BorderSize = 0;
            btnSaveProtSettings.Click += (s, e) =>
            {
                Settings.SaveCharacterSettings();
                Log("Koruma ve Stat ayarları kaydedildi.");
            };

            Label lblProtectionOverview = new Label
            {
                Text = "Karakter koruması canlı döngüde çalışır. HP/MP durumunda acil skill kullanımı, ölen petin Grass of Life ile diriltilmesi ve şehre dönüş koşulları otomatik izlenir.",
                Location = new Point(15, 58),
                Size = new Size(290, 65),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 7.5F)
            };

            gbxProtectionSummary.Controls.AddRange(new Control[] {
                btnManualPetProtection, btnSaveProtSettings, lblProtectionOverview
            });

            if (TabPageH_Character_Option03 != null)
                TabPageH_Character_Option03.Text = "Koruma";
            if (TabPageH_Character_Option03_Panel != null)
                TabPageH_Character_Option03_Panel.Controls.AddRange(new Control[] { gbxSkillPet, gbxAutoStat, gbxReturn, gbxProtectionSummary });
        }

        private CheckBox CreateProtectionCheck(string text, Point location, bool isChecked, Action<bool> changed)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = text,
                Location = location,
                AutoSize = false,
                Size = new Size(150, 20),
                Checked = isChecked,
                ForeColor = Color.White
            };
            checkBox.CheckedChanged += (s, e) => changed(checkBox.Checked);
            return checkBox;
        }

        private NumericUpDown CreateProtectionNumber(Point location, int minimum, int maximum, int value, Action<int> changed)
        {
            NumericUpDown number = new NumericUpDown
            {
                Location = location,
                Size = new Size(45, 20),
                Minimum = minimum,
                Maximum = maximum,
                Value = Math.Max(minimum, Math.Min(maximum, value))
            };
            number.ValueChanged += (s, e) => changed((int)number.Value);
            return number;
        }

        private void BuildItemFilterWidgets()
        {
            if (this.TabPageH_Town_Option03_Panel == null) return;

            // The designer pickup group occupies the left column. The advanced
            // filter and item-rule editor use the right column instead of
            // overlapping the existing controls.
            if (Filter_gbxPick != null)
            {
                Filter_gbxPick.Location = new Point(10, 10);
                Filter_gbxPick.Size = new Size(300, 320);
            }
            if (Filter_cbxPickElixirStone != null)
                Filter_cbxPickElixirStone.Size = new Size(260, 20);
            if (Filter_cbxPickMaterials != null)
                Filter_cbxPickMaterials.Size = new Size(250, 20);
            if (Filter_cbxUsePet != null)
                Filter_cbxUsePet.Size = new Size(260, 20);
            if (Filter_lblInfo != null)
            {
                Filter_lblInfo.Location = new Point(25, 245);
                Filter_lblInfo.Size = new Size(260, 60);
            }

            GroupBox gbxFilterRules = new GroupBox
            {
                Text = "Gelişmiş Eşya Filtresi",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(320, 10),
                Size = new Size(325, 150),
                Font = new Font("Segoe UI", 8.5F)
            };

            Label lblDeg = new Label { Text = "Degree:", Location = new Point(12, 25), AutoSize = true, ForeColor = Color.White };
            nudFilterMinDegree = new NumericUpDown { Location = new Point(62, 23), Size = new Size(42, 20), Minimum = 1, Maximum = 16, Value = ItemFilterManager.MinDegree };
            nudFilterMinDegree.ValueChanged += (s, e) => ItemFilterManager.MinDegree = (int)nudFilterMinDegree.Value;
            Label lblTo = new Label { Text = "-", Location = new Point(108, 25), AutoSize = true, ForeColor = Color.White };
            nudFilterMaxDegree = new NumericUpDown { Location = new Point(120, 23), Size = new Size(42, 20), Minimum = 1, Maximum = 16, Value = ItemFilterManager.MaxDegree };
            nudFilterMaxDegree.ValueChanged += (s, e) => ItemFilterManager.MaxDegree = (int)nudFilterMaxDegree.Value;

            cbxFilterSox = CreateFilterCheck("Sadece SoX", new Point(175, 23), ItemFilterManager.OnlySox, value => ItemFilterManager.OnlySox = value, Color.Gold);
            cbxFilterChina = CreateFilterCheck("China", new Point(12, 53), ItemFilterManager.FilterChina, value => ItemFilterManager.FilterChina = value, Color.White);
            cbxFilterEurope = CreateFilterCheck("Europe", new Point(82, 53), ItemFilterManager.FilterEurope, value => ItemFilterManager.FilterEurope = value, Color.White);
            cbxFilterMale = CreateFilterCheck("Male", new Point(155, 53), ItemFilterManager.FilterMale, value => ItemFilterManager.FilterMale = value, Color.White);
            cbxFilterFemale = CreateFilterCheck("Female", new Point(215, 53), ItemFilterManager.FilterFemale, value => ItemFilterManager.FilterFemale = value, Color.White);

            Label lblFilterInfo = new Label
            {
                Text = "Global filtreler yalnızca ekipmana uygulanır; açık item kuralları her zaman önceliklidir.",
                Location = new Point(12, 83),
                Size = new Size(295, 42),
                ForeColor = Color.LightGray
            };
            gbxFilterRules.Controls.AddRange(new Control[] { lblDeg, nudFilterMinDegree, lblTo, nudFilterMaxDegree, cbxFilterSox, cbxFilterChina, cbxFilterEurope, cbxFilterMale, cbxFilterFemale, lblFilterInfo });

            GroupBox gbxItemRules = new GroupBox
            {
                Text = "Item Bazlı Pickup / Sell / Store Kuralları",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(320, 170),
                Size = new Size(325, 160),
                Font = new Font("Segoe UI", 8.5F)
            };

            tbxItemRuleName = new TextBox { Location = new Point(12, 22), Size = new Size(155, 21) };
            if (ToolTips != null)
                ToolTips.SetToolTip(tbxItemRuleName, "Item adı veya servername");
            Button btnSaveRule = new Button { Text = "Kaydet", Location = new Point(173, 20), Size = new Size(65, 24), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(0, 122, 204) };
            Button btnRemoveRule = new Button { Text = "Sil", Location = new Point(243, 20), Size = new Size(65, 24), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(80, 80, 85) };

            cbxItemRulePickup = CreateRuleCheck("Pickup", new Point(12, 51), true);
            cbxItemRuleSell = CreateRuleCheck("Sell", new Point(102, 51), false);
            cbxItemRuleStore = CreateRuleCheck("Store", new Point(180, 51), false);

            lstvItemRules = new ListView
            {
                Location = new Point(12, 76),
                Size = new Size(296, 70),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                BackColor = Color.FromArgb(34, 34, 38),
                ForeColor = Color.White
            };
            lstvItemRules.Columns.Add("Item", 210);
            lstvItemRules.Columns.Add("P/S/St", 70);
            lstvItemRules.SelectedIndexChanged += (s, e) => LoadSelectedItemRule();
            btnSaveRule.Click += (s, e) => SaveItemRule();
            btnRemoveRule.Click += (s, e) => RemoveSelectedItemRule();

            gbxItemRules.Controls.AddRange(new Control[] { tbxItemRuleName, btnSaveRule, btnRemoveRule, cbxItemRulePickup, cbxItemRuleSell, cbxItemRuleStore, lstvItemRules });
            TabPageH_Town_Option03_Panel.Controls.AddRange(new Control[] { gbxFilterRules, gbxItemRules });
            RefreshItemRuleList();
        }

        private CheckBox CreateFilterCheck(string text, Point location, bool isChecked, Action<bool> changed, Color foreColor)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Checked = isChecked,
                ForeColor = foreColor
            };
            checkBox.CheckedChanged += (s, e) => changed(checkBox.Checked);
            return checkBox;
        }

        private CheckBox CreateRuleCheck(string text, Point location, bool isChecked)
        {
            return new CheckBox { Text = text, Location = location, AutoSize = true, Checked = isChecked, ForeColor = Color.White };
        }

        private void RefreshItemRuleList()
        {
            if (lstvItemRules == null)
                return;

            lstvItemRules.BeginUpdate();
            try
            {
                lstvItemRules.Items.Clear();
                foreach (ItemFilterRule rule in ItemFilterManager.GetAllRules())
                {
                    ListViewItem row = new ListViewItem(rule.ItemName);
                    row.SubItems.Add((rule.Pickup ? "P" : "-") + "/" + (rule.Sell ? "S" : "-") + "/" + (rule.Store ? "St" : "-"));
                    row.Tag = rule.ItemName;
                    lstvItemRules.Items.Add(row);
                }
            }
            finally
            {
                lstvItemRules.EndUpdate();
            }
        }

        private void LoadSelectedItemRule()
        {
            if (lstvItemRules == null || lstvItemRules.SelectedItems.Count == 0)
                return;

            string itemName = lstvItemRules.SelectedItems[0].Tag as string;
            ItemFilterRule rule = ItemFilterManager.GetRule(itemName);
            if (rule == null)
                return;

            tbxItemRuleName.Text = rule.ItemName;
            cbxItemRulePickup.Checked = rule.Pickup;
            cbxItemRuleSell.Checked = rule.Sell;
            cbxItemRuleStore.Checked = rule.Store;
        }

        private void SaveItemRule()
        {
            if (tbxItemRuleName == null || string.IsNullOrWhiteSpace(tbxItemRuleName.Text))
                return;

            string itemName = tbxItemRuleName.Text.Trim();
            ItemFilterManager.SetRule(itemName, cbxItemRulePickup.Checked, cbxItemRuleSell.Checked, cbxItemRuleStore.Checked);
            RefreshItemRuleList();
            SelectItemRule(itemName);
            Log("Item filter rule saved: " + itemName);
        }

        private void RemoveSelectedItemRule()
        {
            if (lstvItemRules == null || lstvItemRules.SelectedItems.Count == 0)
                return;

            string itemName = lstvItemRules.SelectedItems[0].Tag as string;
            ItemFilterManager.RemoveRule(itemName);
            tbxItemRuleName.Clear();
            RefreshItemRuleList();
            Log("Item filter rule removed: " + itemName);
        }

        private void SelectItemRule(string itemName)
        {
            if (lstvItemRules == null)
                return;

            foreach (ListViewItem row in lstvItemRules.Items)
            {
                if (string.Equals(row.Tag as string, itemName, StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                    row.EnsureVisible();
                    break;
                }
            }
        }

        public void RefreshCustomSettingsWidgets()
        {
            this.InvokeIfRequired(() =>
            {
                // General & Login Strategy
                SetProtectionCheck(cbxGeneralAutoLogin, LoginStrategyManager.AutomatedLogin);
                SetProtectionCheck(cbxGeneralStaticCaptcha, LoginStrategyManager.StaticCaptcha);
                if (tbxGeneralStaticCaptchaCode != null && tbxGeneralStaticCaptchaCode.Text != (LoginStrategyManager.StaticCaptchaCode ?? ""))
                    tbxGeneralStaticCaptchaCode.Text = LoginStrategyManager.StaticCaptchaCode ?? "";
                SetProtectionNumber(nudGeneralLoginDelay, LoginStrategyManager.LoginDelaySeconds);
                SetProtectionNumber(nudGeneralWaitAfterDC, LoginStrategyManager.WaitAfterDCMinutes);
                SetProtectionCheck(cbxGeneralAutoStart, LoginStrategyManager.AutoStartBot);
                SetProtectionCheck(cbxGeneralAutoHide, LoginStrategyManager.AutoHideClient);
                SetProtectionCheck(cbxGeneralStayConnected, LoginStrategyManager.StayConnected);
                if (rbnGeneralFirstFound != null)
                    rbnGeneralFirstFound.Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.FirstFound);
                if (rbnGeneralHighestLevel != null)
                    rbnGeneralHighestLevel.Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.HighestLevel);

                // Protection & Healing
                SetProtectionCheck(cbxProtectionSkillHP, ProtectionManager.UseSkillHP);
                SetProtectionNumber(nudProtectionSkillHP, ProtectionManager.SkillHPPercent);
                SetProtectionCheck(cbxProtectionSkillMP, ProtectionManager.UseSkillMP);
                SetProtectionNumber(nudProtectionSkillMP, ProtectionManager.SkillMPPercent);
                SetProtectionCheck(cbxProtectionCure, ProtectionManager.UseSkillBadStatus);
                SetProtectionCheck(cbxProtectionPetRevive, ProtectionManager.RevivePet);
                SetProtectionCheck(cbxProtectionPetSummon, ProtectionManager.AutoSummonPet);
                SetProtectionCheck(cbxProtectionNoArrows, ProtectionManager.ReturnNoArrows);
                SetProtectionCheck(cbxProtectionFullInventory, ProtectionManager.ReturnFullInventory);
                SetProtectionCheck(cbxProtectionFullPetInventory, ProtectionManager.ReturnFullPetInventory);
                SetProtectionCheck(cbxProtectionLowHP, ProtectionManager.ReturnHPLow);
                SetProtectionCheck(cbxProtectionLowMP, ProtectionManager.ReturnMPLow);
                SetProtectionCheck(cbxProtectionDurability, ProtectionManager.ReturnDurabilityLow);
                SetProtectionCheck(cbxProtectionLevelUp, ProtectionManager.ReturnLevelUp);
                SetProtectionCheck(cbxProtectionStopInTown, ProtectionManager.StopBotInTown);
                SetProtectionCheck(cbxProtectionDead, ProtectionManager.ReturnDeadWithDelay);
                SetProtectionNumber(nudProtectionHP, ProtectionManager.HPLowThreshold);
                SetProtectionNumber(nudProtectionMP, ProtectionManager.MPLowThreshold);
                SetProtectionNumber(nudProtectionDurability, ProtectionManager.DurabilityLowThreshold);
                SetProtectionNumber(nudProtectionDeadDelay, ProtectionManager.DeadDelaySeconds);

                // Auto Stat Points
                SetProtectionCheck(cbxAutoStatEnabled, StatPointManager.AutoDistributeEnabled);
                SetProtectionNumber(nudAutoStatSTR, StatPointManager.TargetSTR);
                SetProtectionNumber(nudAutoStatINT, StatPointManager.TargetINT);
                if (Character_cbxAutoStat != null && Character_cbxAutoStat.Checked != StatPointManager.AutoDistributeEnabled)
                    Character_cbxAutoStat.Checked = StatPointManager.AutoDistributeEnabled;
                if (Character_rbnAutoSTR != null)
                    Character_rbnAutoSTR.Checked = (StatPointManager.TargetSTR > StatPointManager.TargetINT);
                if (Character_rbnAutoINT != null)
                    Character_rbnAutoINT.Checked = (StatPointManager.TargetINT >= StatPointManager.TargetSTR);
                UpdateProtectionStatus();

                // Item Filter
                SetProtectionNumber(nudFilterMinDegree, ItemFilterManager.MinDegree);
                SetProtectionNumber(nudFilterMaxDegree, ItemFilterManager.MaxDegree);
                SetProtectionCheck(cbxFilterSox, ItemFilterManager.OnlySox);
                SetProtectionCheck(cbxFilterChina, ItemFilterManager.FilterChina);
                SetProtectionCheck(cbxFilterEurope, ItemFilterManager.FilterEurope);
                SetProtectionCheck(cbxFilterMale, ItemFilterManager.FilterMale);
                SetProtectionCheck(cbxFilterFemale, ItemFilterManager.FilterFemale);

                // Skills
                SetProtectionCheck(Skills_cbxCastInOrder, SkillManager.InOrderCombo);
                if (cmbxImbue != null)
                    RefreshImbueSkillList();
                UpdateSkillRuntimeStatus();

                // Combat AI
                SetProtectionCheck(cbxCombatZerkFullHP, CombatAIEngine.ZerkWhenHPFull);
                SetProtectionCheck(cbxCombatIgnorePillars, CombatAIEngine.IgnoreDimensionPillars);
                SetProtectionCheck(cbxCombatWeakerFirst, CombatAIEngine.AttackWeakerFirst);
                SetProtectionCheck(cbxCombatDoNotFollow, CombatAIEngine.DoNotFollowMobs);
                SetProtectionCheck(cbxCombatZerkCount, CombatAIEngine.ZerkMonsterCountEnabled);
                SetProtectionCheck(cbxCombatZerkAvoidance, CombatAIEngine.ZerkAvoidanceBased);
                SetProtectionCheck(cbxCombatZerkRarity, CombatAIEngine.ZerkRarityBased);
                SetProtectionNumber(nudCombatZerkCount, CombatAIEngine.ZerkMonsterCount);

                RefreshItemRuleList();
            });
        }

        public void UpdateProtectionStatus()
        {
            if (lblProtectionStatPointsRemain != null)
            {
                lblProtectionStatPointsRemain.InvokeIfRequired(() =>
                {
                    if (InfoManager.Character != null)
                        lblProtectionStatPointsRemain.Text = InfoManager.Character.StatPoints.ToString();
                    else
                        lblProtectionStatPointsRemain.Text = "--";
                });
            }
        }

        public void UpdateSkillRuntimeStatus()
        {
            if (lblSkillRuntimeStatus == null)
                return;

            string skill = string.IsNullOrEmpty(SkillManager.LastCastSkill) ? "-" : SkillManager.LastCastSkill;
            string text = "Skill: " + skill + " | " + SkillManager.LastCastStatus;
            Color color = SkillManager.LastCastStatus == "Başarılı"
                ? Color.LightGreen
                : (SkillManager.ConsecutiveCastFailures > 0 ? Color.Khaki : Color.LightSkyBlue);

            lblSkillRuntimeStatus.InvokeIfRequired(() =>
            {
                lblSkillRuntimeStatus.Text = text;
                lblSkillRuntimeStatus.ForeColor = color;
            });
        }

        private void SetProtectionCheck(CheckBox checkBox, bool value)
        {
            if (checkBox != null && checkBox.Checked != value)
                checkBox.Checked = value;
        }

        private void SetProtectionNumber(NumericUpDown number, int value)
        {
            if (number == null)
                return;

            decimal safeValue = Math.Max(number.Minimum, Math.Min(number.Maximum, value));
            if (number.Value != safeValue)
                number.Value = safeValue;
        }

        public void UpdateHeaderStats()
        {
            try
            {
                if (InfoManager.Character == null) return;

                if (lblHeaderLevel != null)
                {
                    lblHeaderLevel.InvokeIfRequired(() => {
                        lblHeaderLevel.Text = "LVL: " + InfoManager.Character.Level;
                    });
                }
                if (lblHeaderHP != null)
                {
                    lblHeaderHP.InvokeIfRequired(() => {
                        lblHeaderHP.Text = "HP: " + InfoManager.Character.GetHPPercent() + "%";
                    });
                }
                if (lblHeaderMP != null)
                {
                    lblHeaderMP.InvokeIfRequired(() => {
                        lblHeaderMP.Text = "MP: " + InfoManager.Character.GetMPPercent() + "%";
                    });
                }

                UpdateProtectionStatus();
            }
            catch { }
        }

        public void ApplyLanguageToWindow()
        {
            try
            {
                this.InvokeIfRequired(() =>
                {
                    bool isTR = LocalizationManager.CurrentLanguage == "TR";
                    if (btnLangTR != null)
                    {
                        btnLangTR.BackColor = isTR ? Color.FromArgb(0, 122, 204) : Color.FromArgb(50, 50, 55);
                        btnLangTR.ForeColor = isTR ? Color.White : Color.LightGray;
                    }
                    if (btnLangEN != null)
                    {
                        btnLangEN.BackColor = !isTR ? Color.FromArgb(0, 122, 204) : Color.FromArgb(50, 50, 55);
                        btnLangEN.ForeColor = !isTR ? Color.White : Color.LightGray;
                    }

                    if (this.TabPageV_Control01_Login != null) this.TabPageV_Control01_Login.Text = LocalizationManager.Get("UI_General", "General");
                    if (this.TabPageV_Control01_Training != null) this.TabPageV_Control01_Training.Text = LocalizationManager.Get("UI_Training", "Training");
                    if (this.TabPageV_Control01_Skills != null) this.TabPageV_Control01_Skills.Text = LocalizationManager.Get("UI_Skills", "Skills");
                    if (this.TabPageV_Control01_Character != null) this.TabPageV_Control01_Character.Text = LocalizationManager.Get("UI_Protection", "Protection");
                    if (this.TabPageV_Control01_Party != null) this.TabPageV_Control01_Party.Text = LocalizationManager.Get("UI_Party", "Party");
                    if (this.TabPageV_Control01_Inventory != null) this.TabPageV_Control01_Inventory.Text = LocalizationManager.Get("UI_Inventory", "Inventory");
                    if (this.TabPageV_Control01_Town != null) this.TabPageV_Control01_Town.Text = LocalizationManager.Get("UI_Items", "Town & Items");
                    if (this.TabPageV_Control01_Chat != null) this.TabPageV_Control01_Chat.Text = LocalizationManager.Get("UI_Chat", "Chat");

                    if (this.TabPageH_Character_Option03 != null)
                        this.TabPageH_Character_Option03.Text = LocalizationManager.Get("UI_Protection", "Protection");
                    if (this.TabPageH_Town_Option03 != null)
                        this.TabPageH_Town_Option03.Text = LocalizationManager.Get("UI_ItemFilter", "Item Filter");
                    if (this.Skills_cbxCastInOrder != null)
                        this.Skills_cbxCastInOrder.Text = LocalizationManager.Get("UI_InOrder", "Cast skills in order");

                    if (this.lblBotState != null && this.lblBotState.Text.Contains("Ready"))
                        this.lblBotState.Text = LocalizationManager.Get("UI_WaitingForChar", "Ready");
                });
            }
            catch { }
        }
    }
}
