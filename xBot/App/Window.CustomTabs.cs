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
        private Button btnCommandCenter;
        private Label lblHeaderLevel;
        private Label lblHeaderHP;
        private Label lblHeaderMP;

        // Saved Accounts controls
        public Label Login_lblAccount;
        public ComboBox Login_cmbxSavedAccounts;
        public Button Login_btnSaveAccount;
        public Button Login_btnDeleteAccount;
        public Button Login_btnAccountSetup;
        private bool _isPopulatingSavedAccounts = false;

        // Alchemy controls
        public Panel TabPageV_Control01_Alchemy_Panel;
        private Theme.ModernCard gbxAlchemySettings;
        private Theme.ModernCard gbxAlchemyActions;
        private ComboBox cmbxAlchemyItems;
        private Button btnAlchemyRefreshItems;
        private NumericUpDown nudAlchemyTargetSlot;
        private NumericUpDown nudAlchemyTargetPlus;
        private CheckBox cbxAlchemyUsePowder;
        private NumericUpDown nudAlchemyMaxAttempts;
        private NumericUpDown nudAlchemyDelay;
        private Label lblAlchemyItemInfo;
        private Label lblAlchemyElixirInfo;
        private Label lblAlchemyPowderInfo;
        private Button btnAlchemyStart;
        private Button btnAlchemyStop;
        private Button btnAlchemyReset;
        private Label lblAlchemyStatus;
        private Label lblAlchemyAttempts;
        private Label lblAlchemySuccess;
        private Label lblAlchemyFailed;
        private ListBox lbxAlchemyLog;

        // Target Assist controls
        public Panel TabPageV_Control01_TargetAssist_Panel;
        private Theme.ModernCard gbxTargetAssistSettings;
        private Theme.ModernCard gbxTargetAssistFilters;
        private Theme.ModernCard gbxTargetAssistLists;
        private Label lblTargetAssistRuntimeStatus;
        private Theme.ModernToggle togTargetAssistEnabled;
        private NumericUpDown nudTargetAssistMaxRange;
        private ComboBox cmbxTargetAssistRoleMode;
        private TextBox tbxTargetAssistCycleKey;
        private Button btnTargetAssistCaptureKey;
        private Theme.ModernToggle togTargetAssistIncludeDead;
        private Theme.ModernToggle togTargetAssistIgnoreSnow;
        private Theme.ModernToggle togTargetAssistIgnoreBloody;
        private Theme.ModernToggle togTargetAssistOnlyCustom;
        private TextBox tbxTargetAssistIgnoredGuildInput;
        private Button btnTargetAssistAddGuild;
        private ListBox lbxTargetAssistIgnoredGuilds;
        private Button btnTargetAssistRemoveGuild;
        private Label lblTargetAssistGuildsMeta;
        private TextBox tbxTargetAssistCustomPlayerInput;
        private Button btnTargetAssistAddPlayer;
        private ListBox lbxTargetAssistCustomPlayers;
        private Button btnTargetAssistRemovePlayer;
        private Label lblTargetAssistPlayersMeta;
        private Label lblTargetAssistFooterHelp;
        private Button btnTargetAssistSave;
        private bool _isCapturingCycleKey = false;
        private Timer _targetAssistUiTimer;

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
        private Theme.ModernCard gbxStrategy;
        private Label lblGeneralLoginDelay;
        private Label lblLoginDelaySec;
        private Label lblGeneralWaitDC;
        private Label lblWaitDCMin;
        private Label lblGeneralCharStrategy;
        private Label lblGeneralStrategyInfo;

        // Protection controls added at runtime so they can be refreshed after
        // character settings are loaded.
        private GroupBox gbxProtectionSkillPet;
        private CheckBox cbxProtectionSkillHP;
        private NumericUpDown nudProtectionSkillHP;
        private Label lblProtectionSkillHPSuffix;
        private CheckBox cbxProtectionSkillMP;
        private NumericUpDown nudProtectionSkillMP;
        private Label lblProtectionSkillMPSuffix;
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
        private Label lblProtectionDeadDelay;
        private GroupBox gbxProtectionReturn;
        private GroupBox gbxProtectionSummary;
        private Button btnProtectionManualPet;
        private Button btnProtectionSave;
        private Label lblProtectionOverview;

        // Auto Stat controls added at runtime
        private GroupBox gbxProtectionAutoStat;
        private CheckBox cbxAutoStatEnabled;
        private NumericUpDown nudAutoStatSTR;
        private NumericUpDown nudAutoStatINT;
        private Label lblProtectionStr;
        private Label lblProtectionInt;
        private Button btnProtectionDistributeNow;
        private Label lblProtectionStatGuidance;
        private Label lblProtectionRemainInfo;
        private Label lblProtectionStatPointsRemain;

        // Item filter controls added at runtime.
        private GroupBox gbxItemFilterRules;
        private Label lblFilterDegree;
        private Label lblFilterInfo;
        private GroupBox gbxItemFilterRulesCustom;
        private Button btnItemFilterSaveRule;
        private Button btnItemFilterRemoveRule;
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
        private Label lblSkillImbue;
        private CheckBox cbxSkillNoAttack;
        private CheckBox cbxSkillDevil;
        private bool refreshingImbueSkills;
        private System.Windows.Forms.Timer automatedLoginTimer;

        // Alchemy promoted labels
        private Label lblAlchemySelect;
        private Label lblAlchemySlot;
        private Label lblAlchemyPlus;
        private Label lblAlchemyMaxAttempts;
        private Label lblAlchemyDelay;
        private Label lblAlchemyLogHead;

        // Target Assist promoted labels
        private Label lblTargetAssistEnabled;
        private Label lblTargetAssistMaxRange;
        private Label lblTargetAssistRoleMode;
        private Label lblTargetAssistCycleKey;
        private Label lblTargetAssistGuildHead;
        private Label lblTargetAssistPlayerHead;

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
                StartGameInfoLiveTimer();
                ApplyLanguageToWindow();
                ApplyModernTheme();
            }
            catch (Exception ex)
            {
                Log("[Custom UI Init Error] " + ex.Message);
            }
        }
        private Timer _gameInfoLiveTimer;
        private void StartGameInfoLiveTimer()
        {
            if (_gameInfoLiveTimer != null) return;
            _gameInfoLiveTimer = new Timer();
            _gameInfoLiveTimer.Interval = 2000;
            _gameInfoLiveTimer.Tick += (s, e) => {
                try
                {
                    if (!InfoManager.inGame) return;
                    if (TabPageV_Control01_GameInfo_Panel == null || !TabPageV_Control01_GameInfo_Panel.Visible) return;
                    WinAPI.InvokeIfRequired(GameInfo_tbxServerTime, () => {
                        try { GameInfo_tbxServerTime.Text = InfoManager.GetServerTime().ToString("HH:mm:ss | dd/MM/yyyy"); } catch { }
                    });
                }
                catch { }
            };
            _gameInfoLiveTimer.Start();
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

            // Command Center button
            btnCommandCenter = new Button();
            btnCommandCenter.Text = "KOMUT";
            btnCommandCenter.Size = new Size(62, 22);
            btnCommandCenter.Location = new Point(pnlHeader.Width - 330, 4);
            btnCommandCenter.FlatStyle = FlatStyle.Flat;
            btnCommandCenter.FlatAppearance.BorderSize = 0;
            btnCommandCenter.BackColor = Color.FromArgb(45, 45, 48);
            btnCommandCenter.ForeColor = Color.FromArgb(56, 189, 248);
            btnCommandCenter.Click += (s, e) =>
            {
                using (var form = new CommandCenter.CommandCenterForm())
                {
                    form.ShowDialog(this);
                }
            };
            pnlHeader.Controls.Add(btnCommandCenter);

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

            gbxStrategy = new Theme.ModernCard();
            gbxStrategy.TitleText = LocalizationManager.Get("UI_StrategyCard_Title", "GİRİŞ STRATEJİSİ VE OTOMASYON");
            gbxStrategy.Location = new Point(432, 344);
            gbxStrategy.Size = new Size(772, 338);
            gbxStrategy.Font = Theme.DarkTheme.FontBody;

            cbxGeneralAutoLogin = new CheckBox { Text = LocalizationManager.Get("UI_AutoLogin", "Otomatik Giriş Yap"), Location = new Point(16, 36), AutoSize = true, Checked = LoginStrategyManager.AutomatedLogin, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralAutoLogin.CheckedChanged += (s, e) =>
            {
                LoginStrategyManager.AutomatedLogin = cbxGeneralAutoLogin.Checked;
                Settings.SaveBotSettings();
            };

            cbxGeneralAutoStart = new CheckBox { Text = LocalizationManager.Get("UI_AutoStartBot", "Oyuna Girince Botu Otomatik Başlat"), Location = new Point(220, 36), AutoSize = true, Checked = LoginStrategyManager.AutoStartBot, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralAutoStart.CheckedChanged += (s, e) => { LoginStrategyManager.AutoStartBot = cbxGeneralAutoStart.Checked; Settings.SaveBotSettings(); };

            cbxGeneralAutoHide = new CheckBox { Text = LocalizationManager.Get("UI_AutoHideClient", "Silkroad İstemcisini Gizle"), Location = new Point(490, 36), AutoSize = true, Checked = LoginStrategyManager.AutoHideClient, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralAutoHide.CheckedChanged += (s, e) => { LoginStrategyManager.AutoHideClient = cbxGeneralAutoHide.Checked; Settings.SaveBotSettings(); };

            cbxGeneralStaticCaptcha = new CheckBox { Text = LocalizationManager.Get("UI_StaticCaptcha", "Sabit Captcha Kodu:"), Location = new Point(16, 82), AutoSize = true, Checked = LoginStrategyManager.StaticCaptcha, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralStaticCaptcha.CheckedChanged += (s, e) => { LoginStrategyManager.StaticCaptcha = cbxGeneralStaticCaptcha.Checked; Settings.SaveBotSettings(); };

            tbxGeneralStaticCaptchaCode = new TextBox { Location = new Point(190, 80), Size = new Size(65, 24), BackColor = Theme.DarkTheme.BgInput, ForeColor = Theme.DarkTheme.TextPrimary, BorderStyle = BorderStyle.FixedSingle, Text = LoginStrategyManager.StaticCaptchaCode ?? "" };
            tbxGeneralStaticCaptchaCode.TextChanged += (s, e) => { LoginStrategyManager.StaticCaptchaCode = tbxGeneralStaticCaptchaCode.Text; Settings.SaveBotSettings(); };
            if (ToolTips != null)
                ToolTips.SetToolTip(tbxGeneralStaticCaptchaCode, "Sabit captcha kodu (örn: 1234)");

            lblGeneralLoginDelay = new Label { Text = LocalizationManager.Get("UI_LoginDelay", "Giriş Gecikmesi:"), Location = new Point(280, 84), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };
            nudGeneralLoginDelay = new NumericUpDown { Location = new Point(400, 80), Size = new Size(55, 24), Minimum = 0, Maximum = 60, Value = LoginStrategyManager.LoginDelaySeconds, BackColor = Theme.DarkTheme.BgInput, ForeColor = Theme.DarkTheme.TextPrimary };
            nudGeneralLoginDelay.ValueChanged += (s, e) => { LoginStrategyManager.LoginDelaySeconds = (int)nudGeneralLoginDelay.Value; Settings.SaveBotSettings(); };
            lblLoginDelaySec = new Label { Text = "sn", Location = new Point(460, 84), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };

            lblGeneralWaitDC = new Label { Text = LocalizationManager.Get("UI_WaitAfterDC", "DC Sonrası Bekleme:"), Location = new Point(16, 130), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };
            nudGeneralWaitAfterDC = new NumericUpDown { Location = new Point(190, 126), Size = new Size(65, 24), Minimum = 0, Maximum = 60, Value = LoginStrategyManager.WaitAfterDCMinutes, BackColor = Theme.DarkTheme.BgInput, ForeColor = Theme.DarkTheme.TextPrimary };
            nudGeneralWaitAfterDC.ValueChanged += (s, e) => { LoginStrategyManager.WaitAfterDCMinutes = (int)nudGeneralWaitAfterDC.Value; Settings.SaveBotSettings(); };
            lblWaitDCMin = new Label { Text = "dk", Location = new Point(260, 130), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };

            cbxGeneralStayConnected = new CheckBox { Text = LocalizationManager.Get("UI_StayConnected", "Bağlantıyı Koru (Çökme / DC Koruması)"), Location = new Point(300, 128), AutoSize = true, Checked = LoginStrategyManager.StayConnected, ForeColor = Theme.DarkTheme.InfoBlue };
            cbxGeneralStayConnected.CheckedChanged += (s, e) => { LoginStrategyManager.StayConnected = cbxGeneralStayConnected.Checked; Settings.SaveBotSettings(); };
            if (ToolTips != null)
                ToolTips.SetToolTip(cbxGeneralStayConnected, "İstemci aniden çökerse sunucu bağlantısını koparmadan Clientless moda geçer");

            lblGeneralCharStrategy = new Label { Text = LocalizationManager.Get("UI_CharSelectStrategy", "Karakter Seçim Stratejisi:"), Location = new Point(16, 178), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };
            rbnGeneralFirstFound = new RadioButton { Text = LocalizationManager.Get("UI_StrategyFirst", "İlk Bulunan Karakter"), Location = new Point(180, 176), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.FirstFound), ForeColor = Theme.DarkTheme.TextPrimary };
            rbnGeneralHighestLevel = new RadioButton { Text = LocalizationManager.Get("UI_StrategyHighest", "En Yüksek Seviyeli Karakter"), Location = new Point(410, 176), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.HighestLevel), ForeColor = Theme.DarkTheme.Warning };

            rbnGeneralFirstFound.CheckedChanged += (s, e) => { if (rbnGeneralFirstFound.Checked) { LoginStrategyManager.Strategy = CharacterSelectionStrategy.FirstFound; Settings.SaveBotSettings(); } };
            rbnGeneralHighestLevel.CheckedChanged += (s, e) => { if (rbnGeneralHighestLevel.Checked) { LoginStrategyManager.Strategy = CharacterSelectionStrategy.HighestLevel; Settings.SaveBotSettings(); } };

            lblGeneralStrategyInfo = new Label
            {
                Text = LocalizationManager.Get("UI_StrategyCard_Hint", "Kayıtlı hesabı seçip START butonuna bastığınızda bot oyuna otomatik giriş yapacaktır.\nDC sonrası bekleme ve çökme koruması bağlantıyı otomatik olarak canlı tutar."),
                Location = new Point(16, 226),
                Size = new Size(736, 50),
                ForeColor = Theme.DarkTheme.TextFaint,
                Font = Theme.DarkTheme.FontCaption
            };

            gbxStrategy.Controls.AddRange(new Control[] {
                cbxGeneralAutoLogin, cbxGeneralAutoStart, cbxGeneralAutoHide,
                cbxGeneralStaticCaptcha, tbxGeneralStaticCaptchaCode,
                lblGeneralLoginDelay, nudGeneralLoginDelay, lblLoginDelaySec,
                lblGeneralWaitDC, nudGeneralWaitAfterDC, lblWaitDCMin,
                cbxGeneralStayConnected,
                lblGeneralCharStrategy, rbnGeneralFirstFound, rbnGeneralHighestLevel,
                lblGeneralStrategyInfo
            });
            TabPageV_Control01_Login_Panel.Controls.Add(gbxStrategy);

            gbxStrategy.Layout += (s, e) => RepositionStrategyCardControls();
            RepositionStrategyCardControls();

            // Saved Accounts Controls inside Login_gbxLogin
            if (Login_gbxLogin != null)
            {
                Login_lblAccount = new Label
                {
                    Name = "Login_lblAccount",
                    Text = "Hesap:",
                    ForeColor = Theme.DarkTheme.TextMuted,
                    AutoSize = true
                };

                Login_cmbxSavedAccounts = new ComboBox
                {
                    Name = "Login_cmbxSavedAccounts",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.DarkTheme.BgInput,
                    ForeColor = Theme.DarkTheme.TextPrimary,
                    Font = Theme.DarkTheme.FontBody
                };

                Login_btnSaveAccount = new Button
                {
                    Name = "Login_btnSaveAccount",
                    Text = "+",
                    Font = Theme.DarkTheme.FontBodyBold,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.DarkTheme.BgInput,
                    ForeColor = Theme.DarkTheme.Success,
                    Cursor = Cursors.Hand
                };
                Login_btnSaveAccount.FlatAppearance.BorderSize = 1;
                Login_btnSaveAccount.FlatAppearance.BorderColor = Theme.DarkTheme.BorderSubtle;

                Login_btnDeleteAccount = new Button
                {
                    Name = "Login_btnDeleteAccount",
                    Text = "✕",
                    Font = Theme.DarkTheme.FontBodyBold,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.DarkTheme.BgInput,
                    ForeColor = Theme.DarkTheme.Danger,
                    Cursor = Cursors.Hand
                };
                Login_btnDeleteAccount.FlatAppearance.BorderSize = 1;
                Login_btnDeleteAccount.FlatAppearance.BorderColor = Theme.DarkTheme.BorderSubtle;

                Login_btnAccountSetup = new Button
                {
                    Name = "Login_btnAccountSetup",
                    Text = "⚙",
                    Font = Theme.DarkTheme.FontBodyBold,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.DarkTheme.BgInput,
                    ForeColor = Theme.DarkTheme.Accent,
                    Cursor = Cursors.Hand
                };
                Login_btnAccountSetup.FlatAppearance.BorderSize = 1;
                Login_btnAccountSetup.FlatAppearance.BorderColor = Theme.DarkTheme.BorderSubtle;

                if (ToolTips != null)
                {
                    ToolTips.SetToolTip(Login_btnSaveAccount, "Geçerli hesap bilgilerini kaydet / güncelle");
                    ToolTips.SetToolTip(Login_btnDeleteAccount, "Seçili hesabı listeden sil");
                    ToolTips.SetToolTip(Login_btnAccountSetup, "Hesap Yönetimi ve PIN Ayarları (Account Setup)");
                    ToolTips.SetToolTip(Login_cmbxSavedAccounts, "Kayıtlı hesaplar arasından seçim yapın");
                }

                Login_cmbxSavedAccounts.SelectedIndexChanged += (s, e) => OnSavedAccountSelected();
                Login_btnSaveAccount.Click += (s, e) => OnSaveAccountClicked();
                Login_btnDeleteAccount.Click += (s, e) => OnDeleteAccountClicked();
                Login_btnAccountSetup.Click += (s, e) => OpenAccountSetupDialog();

                Login_gbxLogin.Controls.Add(Login_lblAccount);
                Login_gbxLogin.Controls.Add(Login_cmbxSavedAccounts);
                Login_gbxLogin.Controls.Add(Login_btnAccountSetup);
                Login_gbxLogin.Controls.Add(Login_btnSaveAccount);
                Login_gbxLogin.Controls.Add(Login_btnDeleteAccount);
            }
        }

        public void RepositionStrategyCardControls()
        {
            if (gbxStrategy == null) return;
            int cardW = gbxStrategy.Width;

            // Row 1: Checkboxes
            if (cbxGeneralAutoLogin != null) { cbxGeneralAutoLogin.Location = new Point(16, 36); cbxGeneralAutoLogin.AutoSize = true; }
            if (cbxGeneralAutoStart != null) { cbxGeneralAutoStart.Location = new Point(220, 36); cbxGeneralAutoStart.AutoSize = true; }
            if (cbxGeneralAutoHide != null) { cbxGeneralAutoHide.Location = new Point(490, 36); cbxGeneralAutoHide.AutoSize = true; }

            // Row 2: Captcha and Login Delay
            if (cbxGeneralStaticCaptcha != null) { cbxGeneralStaticCaptcha.Location = new Point(16, 82); cbxGeneralStaticCaptcha.AutoSize = true; }
            if (tbxGeneralStaticCaptchaCode != null) { tbxGeneralStaticCaptchaCode.Location = new Point(190, 80); tbxGeneralStaticCaptchaCode.Size = new Size(65, 24); }
            if (lblGeneralLoginDelay != null) { lblGeneralLoginDelay.Location = new Point(280, 84); lblGeneralLoginDelay.AutoSize = true; }
            if (nudGeneralLoginDelay != null) { nudGeneralLoginDelay.Location = new Point(400, 80); nudGeneralLoginDelay.Size = new Size(55, 24); }
            if (lblLoginDelaySec != null) { lblLoginDelaySec.Location = new Point(460, 84); lblLoginDelaySec.AutoSize = true; }

            // Row 3: DC Wait and Failover Protection
            if (lblGeneralWaitDC != null) { lblGeneralWaitDC.Location = new Point(16, 130); lblGeneralWaitDC.AutoSize = true; }
            if (nudGeneralWaitAfterDC != null) { nudGeneralWaitAfterDC.Location = new Point(190, 126); nudGeneralWaitAfterDC.Size = new Size(65, 24); }
            if (lblWaitDCMin != null) { lblWaitDCMin.Location = new Point(260, 130); lblWaitDCMin.AutoSize = true; }
            if (cbxGeneralStayConnected != null) { cbxGeneralStayConnected.Location = new Point(300, 128); cbxGeneralStayConnected.AutoSize = true; }

            // Row 4: Character Selection Strategy
            if (lblGeneralCharStrategy != null) { lblGeneralCharStrategy.Location = new Point(16, 178); lblGeneralCharStrategy.AutoSize = true; }
            if (rbnGeneralFirstFound != null) { rbnGeneralFirstFound.Location = new Point(180, 176); rbnGeneralFirstFound.AutoSize = true; }
            if (rbnGeneralHighestLevel != null) { rbnGeneralHighestLevel.Location = new Point(410, 176); rbnGeneralHighestLevel.AutoSize = true; }

            // Row 5: Strategy Info Box
            if (lblGeneralStrategyInfo != null)
            {
                lblGeneralStrategyInfo.Location = new Point(16, 226);
                lblGeneralStrategyInfo.Size = new Size(cardW - 32, 50);
            }
        }

        public void OpenAccountSetupDialog()
        {
            using (var dlg = new AccountSetupForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    PopulateSavedAccounts(AccountManager.SelectedAccountUsername);
                }
            }
        }

        public void PopulateSavedAccounts(string selectUsername = null)
        {
            if (Login_cmbxSavedAccounts == null) return;

            _isPopulatingSavedAccounts = true;
            try
            {
                Login_cmbxSavedAccounts.Items.Clear();
                Login_cmbxSavedAccounts.Items.Add("[Yeni / Özel Hesap]");

                int selectIdx = 0;
                string targetUser = selectUsername ?? AccountManager.SelectedAccountUsername;

                for (int i = 0; i < AccountManager.Accounts.Count; i++)
                {
                    var acc = AccountManager.Accounts[i];
                    Login_cmbxSavedAccounts.Items.Add(acc);
                    if (!string.IsNullOrEmpty(targetUser) && acc.Username.Equals(targetUser, StringComparison.OrdinalIgnoreCase))
                    {
                        selectIdx = i + 1; // +1 for [Yeni / Özel Hesap]
                    }
                }

                if (Login_cmbxSavedAccounts.Items.Count > 0)
                {
                    Login_cmbxSavedAccounts.SelectedIndex = selectIdx;
                }
            }
            finally
            {
                _isPopulatingSavedAccounts = false;
            }

            if (Login_cmbxSavedAccounts != null && Login_cmbxSavedAccounts.SelectedItem is SavedAccount selAcc)
            {
                ApplySavedAccountToInputs(selAcc);
            }
        }

        private void OnSavedAccountSelected()
        {
            if (_isPopulatingSavedAccounts || Login_cmbxSavedAccounts == null) return;

            if (Login_cmbxSavedAccounts.SelectedItem is SavedAccount acc)
            {
                AccountManager.SelectedAccountUsername = acc.Username;
                Settings.SaveBotSettings();
                ApplySavedAccountToInputs(acc);
                Log($"[Hesap] '{acc.Username}' seçildi. Client başlatıldığında otomatik giriş yapılacak.");
            }
        }

        private void ApplySavedAccountToInputs(SavedAccount acc)
        {
            if (acc == null) return;

            if (Login_tbxUsername != null)
            {
                Login_tbxUsername.Text = acc.Username;
                Login_tbxUsername.SelectionStart = 0;
            }
            if (Login_tbxPassword != null)
            {
                Login_tbxPassword.Text = acc.Password;
                Login_tbxPassword.SelectionStart = 0;
            }

            if (!string.IsNullOrWhiteSpace(acc.Silkroad) && Login_cmbxSilkroad != null)
            {
                if (Login_cmbxSilkroad.Items.Contains(acc.Silkroad))
                    Login_cmbxSilkroad.Text = acc.Silkroad;
            }

            if (!string.IsNullOrWhiteSpace(acc.Server))
            {
                if (Login_cmbxServer != null)
                {
                    Login_cmbxServer.Tag = acc.Server;
                    Login_cmbxServer.Text = acc.Server;
                }
                InfoManager.ServerName = acc.Server;
            }

            if (!string.IsNullOrWhiteSpace(acc.Character))
            {
                if (Login_cmbxCharacter != null)
                {
                    Login_cmbxCharacter.Tag = acc.Character;
                    Login_cmbxCharacter.Text = acc.Character;
                }
            }
        }

        private void OnSaveAccountClicked()
        {
            if (Login_tbxUsername == null || string.IsNullOrWhiteSpace(Login_tbxUsername.Text))
            {
                Log("[Hesap Uyarısı] Kaydetmek için lütfen bir kullanıcı adı (ID) girin.", Theme.LogLevel.Warning);
                return;
            }

            string user = Login_tbxUsername.Text.Trim();
            string pass = Login_tbxPassword?.Text ?? string.Empty;
            string server = Login_cmbxServer?.Text?.Trim() ?? string.Empty;
            string character = Login_cmbxCharacter?.Text?.Trim() ?? string.Empty;
            string sro = Login_cmbxSilkroad?.Text?.Trim() ?? string.Empty;

            SavedAccount existing = AccountManager.GetAccount(user);
            string pin = existing?.SecondaryPasscode ?? string.Empty;

            SavedAccount acc = new SavedAccount
            {
                Username = user,
                Password = pass,
                Server = server,
                Character = character,
                Silkroad = sro,
                SecondaryPasscode = pin
            };

            AccountManager.SaveAccount(acc);
            PopulateSavedAccounts(acc.Username);
            Log($"[Hesap] '{acc.Username}' başarıyla kaydedildi.");
        }

        private void OnDeleteAccountClicked()
        {
            string targetUser = string.Empty;
            if (Login_cmbxSavedAccounts?.SelectedItem is SavedAccount sa)
            {
                targetUser = sa.Username;
            }
            else if (Login_tbxUsername != null && !string.IsNullOrWhiteSpace(Login_tbxUsername.Text))
            {
                targetUser = Login_tbxUsername.Text.Trim();
            }

            if (string.IsNullOrEmpty(targetUser))
            {
                Log("[Hesap Uyarısı] Silinecek bir hesap seçilmedi.", Theme.LogLevel.Warning);
                return;
            }

            if (AccountManager.DeleteAccount(targetUser))
            {
                PopulateSavedAccounts();
                if (Login_tbxUsername != null) Login_tbxUsername.Text = string.Empty;
                if (Login_tbxPassword != null) Login_tbxPassword.Text = string.Empty;
                Log($"[Hesap] '{targetUser}' başarıyla silindi.");
            }
            else
            {
                Log($"[Hesap] '{targetUser}' kayıtlı hesaplar arasında bulunamadı.", Theme.LogLevel.Warning);
            }
        }

        private void ScheduleAutomatedLogin()
        {
            // Only automated command-line launches (/sro ... /user ... /pass ...)
            // should programmatically auto-click the START button.
            // In regular UI usage, the user selects their account and clicks START.
            if (!Bot.Get.hasAutoLoginMode
                || InfoManager.inGame
                || Login_btnStart == null
                || Login_btnStart.Text != "START"
                || !Login_btnStart.Enabled
                || Login_cmbxSilkroad == null
                || string.IsNullOrWhiteSpace(Login_cmbxSilkroad.Text)
                || Login_tbxUsername == null
                || string.IsNullOrWhiteSpace(Login_tbxUsername.Text)
                || Login_tbxPassword == null
                || string.IsNullOrWhiteSpace(Login_tbxPassword.Text))
            {
                if (automatedLoginTimer != null)
                    automatedLoginTimer.Stop();
                return;
            }

            if (automatedLoginTimer == null)
            {
                automatedLoginTimer = new System.Windows.Forms.Timer();
                automatedLoginTimer.Tick += (s, e) =>
                {
                    automatedLoginTimer.Stop();
                    if (Bot.Get.hasAutoLoginMode && Login_btnStart.Text == "START")
                    {
                        Bot.Get.LoggedFromBot = true;
                        Log("Otomatik komut satırı girişi başlatılıyor...");
                        Control_Click(Login_btnStart, null);
                    }
                };
            }

            automatedLoginTimer.Interval = Math.Max(1, LoginStrategyManager.LoginDelaySeconds * 1000);
            automatedLoginTimer.Stop();
            automatedLoginTimer.Start();
            LogProcess("Otomatik giriş " + LoginStrategyManager.LoginDelaySeconds + " sn sonra başlayacak...");
        }

        public void BuildAlchemyTab()
        {
            if (TabPageV_Control01_Alchemy_Panel != null)
                return;

            TabPageV_Control01_Alchemy_Panel = new Panel
            {
                Name = "TabPageV_Control01_Alchemy_Panel",
                BackColor = Theme.DarkTheme.BgDark,
                BorderStyle = BorderStyle.None,
                Visible = false
            };

            int cardW = 370;
            int cardH = 404;

            // -------------------------------------------------------------
            // Card 1: Simya Ayarları (Left)
            // -------------------------------------------------------------
            gbxAlchemySettings = new Theme.ModernCard
            {
                Name = "gbxAlchemySettings",
                TitleText = "SİMYA (+ BASMA) AYARLARI",
                Location = new Point(4, 4),
                Size = new Size(cardW, cardH),
                CardColor = Theme.DarkTheme.BgCard,
                BorderColor = Theme.DarkTheme.BorderSubtle,
                BorderRadius = 8
            };

            lblAlchemySelect = new Label
            {
                Text = "Hedef Ekipman (Envanter):",
                Location = new Point(14, 44),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontCaption
            };

            cmbxAlchemyItems = new ComboBox
            {
                Name = "cmbxAlchemyItems",
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(14, 64),
                Size = new Size(296, 24),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody
            };

            btnAlchemyRefreshItems = new Button
            {
                Name = "btnAlchemyRefreshItems",
                Text = "↻",
                Location = new Point(314, 64),
                Size = new Size(40, 24),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.Accent,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontBodyBold,
                Cursor = Cursors.Hand
            };
            btnAlchemyRefreshItems.FlatAppearance.BorderSize = 1;
            btnAlchemyRefreshItems.FlatAppearance.BorderColor = Theme.DarkTheme.BorderSubtle;
            if (ToolTips != null)
            {
                ToolTips.SetToolTip(btnAlchemyRefreshItems, "Envanterdeki ekipmanları ve malzemeleri yenile");
            }

            lblAlchemySlot = new Label
            {
                Text = "Envanter Slotu (13-76):",
                Location = new Point(14, 98),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontBody
            };

            nudAlchemyTargetSlot = new NumericUpDown
            {
                Name = "nudAlchemyTargetSlot",
                Location = new Point(234, 96),
                Size = new Size(120, 22),
                Minimum = 13,
                Maximum = 76,
                Value = Math.Max(13, Math.Min(76, (decimal)AlchemyManager.TargetSlot)),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblAlchemyPlus = new Label
            {
                Text = "Hedef Artı Değeri (+):",
                Location = new Point(14, 130),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontBody
            };

            nudAlchemyTargetPlus = new NumericUpDown
            {
                Name = "nudAlchemyTargetPlus",
                Location = new Point(234, 128),
                Size = new Size(120, 22),
                Minimum = 1,
                Maximum = 15,
                Value = Math.Max(1, Math.Min(15, (decimal)AlchemyManager.TargetPlus)),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };

            cbxAlchemyUsePowder = new CheckBox
            {
                Name = "cbxAlchemyUsePowder",
                Text = "Lucky Powder Kullan (Otomatik Degree)",
                Checked = AlchemyManager.UseLuckyPowder,
                Location = new Point(14, 162),
                Size = new Size(340, 24),
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody
            };

            lblAlchemyMaxAttempts = new Label
            {
                Text = "Maksimum Deneme Sınırı:",
                Location = new Point(14, 196),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontBody
            };

            nudAlchemyMaxAttempts = new NumericUpDown
            {
                Name = "nudAlchemyMaxAttempts",
                Location = new Point(234, 194),
                Size = new Size(120, 22),
                Minimum = 1,
                Maximum = 500,
                Value = Math.Max(1, Math.Min(500, (decimal)AlchemyManager.MaxAttempts)),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblAlchemyDelay = new Label
            {
                Text = "Deneme Gecikmesi (ms):",
                Location = new Point(14, 228),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontBody
            };

            nudAlchemyDelay = new NumericUpDown
            {
                Name = "nudAlchemyDelay",
                Location = new Point(234, 226),
                Size = new Size(120, 22),
                Minimum = 500,
                Maximum = 10000,
                Increment = 250,
                Value = Math.Max(500, Math.Min(10000, (decimal)AlchemyManager.DelayMs)),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblAlchemyItemInfo = new Label
            {
                Text = "Seçili: [Slot " + nudAlchemyTargetSlot.Value + "]",
                Location = new Point(14, 270),
                Size = new Size(340, 20),
                ForeColor = Theme.DarkTheme.Accent,
                Font = Theme.DarkTheme.FontBodyBold
            };

            lblAlchemyElixirInfo = new Label
            {
                Text = "Gereken Elixir: -",
                Location = new Point(14, 298),
                Size = new Size(340, 20),
                ForeColor = Theme.DarkTheme.TextSecondary,
                Font = Theme.DarkTheme.FontBody
            };

            lblAlchemyPowderInfo = new Label
            {
                Text = "Gereken Powder: -",
                Location = new Point(14, 326),
                Size = new Size(340, 20),
                ForeColor = Theme.DarkTheme.TextSecondary,
                Font = Theme.DarkTheme.FontBody
            };

            gbxAlchemySettings.Controls.Add(lblAlchemySelect);
            gbxAlchemySettings.Controls.Add(cmbxAlchemyItems);
            gbxAlchemySettings.Controls.Add(btnAlchemyRefreshItems);
            gbxAlchemySettings.Controls.Add(lblAlchemySlot);
            gbxAlchemySettings.Controls.Add(nudAlchemyTargetSlot);
            gbxAlchemySettings.Controls.Add(lblAlchemyPlus);
            gbxAlchemySettings.Controls.Add(nudAlchemyTargetPlus);
            gbxAlchemySettings.Controls.Add(cbxAlchemyUsePowder);
            gbxAlchemySettings.Controls.Add(lblAlchemyMaxAttempts);
            gbxAlchemySettings.Controls.Add(nudAlchemyMaxAttempts);
            gbxAlchemySettings.Controls.Add(lblAlchemyDelay);
            gbxAlchemySettings.Controls.Add(nudAlchemyDelay);
            gbxAlchemySettings.Controls.Add(lblAlchemyItemInfo);
            gbxAlchemySettings.Controls.Add(lblAlchemyElixirInfo);
            gbxAlchemySettings.Controls.Add(lblAlchemyPowderInfo);

            // -------------------------------------------------------------
            // Card 2: Aksiyonlar ve Canlı Durum (Right)
            // -------------------------------------------------------------
            gbxAlchemyActions = new Theme.ModernCard
            {
                Name = "gbxAlchemyActions",
                TitleText = "İŞLEMLER VE CANLI DURUM",
                Location = new Point(cardW + 14, 4),
                Size = new Size(cardW, cardH),
                CardColor = Theme.DarkTheme.BgCard,
                BorderColor = Theme.DarkTheme.BorderSubtle,
                BorderRadius = 8
            };

            btnAlchemyStart = new Button
            {
                Name = "btnAlchemyStart",
                Text = "▶ Simyayı Başlat",
                Location = new Point(14, 44),
                Size = new Size(160, 34),
                BackColor = Theme.DarkTheme.Success,
                ForeColor = Color.White,
                Font = Theme.DarkTheme.FontBodyBold,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAlchemyStart.FlatAppearance.BorderSize = 0;

            btnAlchemyStop = new Button
            {
                Name = "btnAlchemyStop",
                Text = "⏹ Durdur",
                Location = new Point(184, 44),
                Size = new Size(160, 34),
                BackColor = Theme.DarkTheme.Danger,
                ForeColor = Color.White,
                Font = Theme.DarkTheme.FontBodyBold,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAlchemyStop.FlatAppearance.BorderSize = 0;

            btnAlchemyReset = new Button
            {
                Name = "btnAlchemyReset",
                Text = "↺ Sayaçları Sıfırla",
                Location = new Point(14, 86),
                Size = new Size(140, 26),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextSecondary,
                Font = Theme.DarkTheme.FontCaption,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAlchemyReset.FlatAppearance.BorderSize = 1;
            btnAlchemyReset.FlatAppearance.BorderColor = Theme.DarkTheme.BorderSubtle;

            lblAlchemyStatus = new Label
            {
                Text = "Durum: " + AlchemyManager.LastStatus,
                Location = new Point(14, 122),
                Size = new Size(330, 20),
                ForeColor = Theme.DarkTheme.InfoBlue,
                Font = Theme.DarkTheme.FontBodyBold
            };

            lblAlchemyAttempts = new Label
            {
                Text = "Deneme: 0 / " + nudAlchemyMaxAttempts.Value,
                Location = new Point(14, 148),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBodyBold
            };

            lblAlchemySuccess = new Label
            {
                Text = "Başarılı: 0",
                Location = new Point(150, 148),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.Success,
                Font = Theme.DarkTheme.FontBodyBold
            };

            lblAlchemyFailed = new Label
            {
                Text = "Başarısız: 0",
                Location = new Point(250, 148),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.Danger,
                Font = Theme.DarkTheme.FontBodyBold
            };

            lblAlchemyLogHead = new Label
            {
                Text = "Simya Olay Günlüğü:",
                Location = new Point(14, 178),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontCaption
            };

            lbxAlchemyLog = new ListBox
            {
                Name = "lbxAlchemyLog",
                Location = new Point(14, 198),
                Size = new Size(342, 192),
                BackColor = Theme.DarkTheme.BgDark,
                ForeColor = Theme.DarkTheme.TextSecondary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = Theme.DarkTheme.FontConsole
            };

            gbxAlchemyActions.Controls.Add(btnAlchemyStart);
            gbxAlchemyActions.Controls.Add(btnAlchemyStop);
            gbxAlchemyActions.Controls.Add(btnAlchemyReset);
            gbxAlchemyActions.Controls.Add(lblAlchemyStatus);
            gbxAlchemyActions.Controls.Add(lblAlchemyAttempts);
            gbxAlchemyActions.Controls.Add(lblAlchemySuccess);
            gbxAlchemyActions.Controls.Add(lblAlchemyFailed);
            gbxAlchemyActions.Controls.Add(lblAlchemyLogHead);
            gbxAlchemyActions.Controls.Add(lbxAlchemyLog);

            TabPageV_Control01_Alchemy_Panel.Controls.Add(gbxAlchemySettings);
            TabPageV_Control01_Alchemy_Panel.Controls.Add(gbxAlchemyActions);

            // Hook Event Listeners
            btnAlchemyRefreshItems.Click += (s, e) => RefreshAlchemyInventoryItems();

            cmbxAlchemyItems.SelectedIndexChanged += (s, e) =>
            {
                if (cmbxAlchemyItems.SelectedItem is AlchemyManager.UpgradeableItemInfo item)
                {
                    nudAlchemyTargetSlot.Value = item.Slot;
                    UpdateAlchemyMaterialPreview(item);
                }
            };

            nudAlchemyTargetSlot.ValueChanged += (s, e) =>
            {
                AlchemyManager.TargetSlot = (byte)nudAlchemyTargetSlot.Value;
                lblAlchemyItemInfo.Text = $"Seçili: [Slot {nudAlchemyTargetSlot.Value}]";
            };

            nudAlchemyTargetPlus.ValueChanged += (s, e) =>
            {
                AlchemyManager.TargetPlus = (byte)nudAlchemyTargetPlus.Value;
            };

            cbxAlchemyUsePowder.CheckedChanged += (s, e) =>
            {
                AlchemyManager.UseLuckyPowder = cbxAlchemyUsePowder.Checked;
            };

            nudAlchemyMaxAttempts.ValueChanged += (s, e) =>
            {
                AlchemyManager.MaxAttempts = (int)nudAlchemyMaxAttempts.Value;
            };

            nudAlchemyDelay.ValueChanged += (s, e) =>
            {
                AlchemyManager.DelayMs = (int)nudAlchemyDelay.Value;
            };

            btnAlchemyStart.Click += (s, e) =>
            {
                byte slot = (byte)nudAlchemyTargetSlot.Value;
                byte targetPlus = (byte)nudAlchemyTargetPlus.Value;
                bool usePowder = cbxAlchemyUsePowder.Checked;
                int maxAttempts = (int)nudAlchemyMaxAttempts.Value;
                int delayMs = (int)nudAlchemyDelay.Value;

                AlchemyManager.Start(slot, targetPlus, usePowder, maxAttempts, delayMs);
                Settings.SaveBotSettings();
            };

            btnAlchemyStop.Click += (s, e) =>
            {
                AlchemyManager.Stop();
            };

            btnAlchemyReset.Click += (s, e) =>
            {
                AlchemyManager.ResetCounters();
            };

            AlchemyManager.OnStatusChanged += () =>
            {
                if (TabPageV_Control01_Alchemy_Panel.IsDisposed) return;
                this.InvokeIfRequired(() =>
                {
                    lblAlchemyStatus.Text = "Durum: " + AlchemyManager.LastStatus;
                    lblAlchemyAttempts.Text = $"Deneme: {AlchemyManager.CurrentAttempts} / {AlchemyManager.MaxAttempts}";
                    lblAlchemySuccess.Text = $"Başarılı: {AlchemyManager.SuccessCount}";
                    lblAlchemyFailed.Text = $"Başarısız: {AlchemyManager.FailCount}";
                });
            };

            AlchemyManager.OnLogMessage += (msg) =>
            {
                if (TabPageV_Control01_Alchemy_Panel.IsDisposed) return;
                this.InvokeIfRequired(() =>
                {
                    lbxAlchemyLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
                    lbxAlchemyLog.TopIndex = Math.Max(0, lbxAlchemyLog.Items.Count - 1);
                });
            };
        }

        private void RefreshAlchemyInventoryItems()
        {
            if (cmbxAlchemyItems == null) return;
            cmbxAlchemyItems.Items.Clear();

            var items = AlchemyManager.GetUpgradeableInventoryItems();
            foreach (var itm in items)
            {
                cmbxAlchemyItems.Items.Add(itm);
            }

            if (cmbxAlchemyItems.Items.Count > 0)
            {
                cmbxAlchemyItems.SelectedIndex = 0;
            }
            else
            {
                lblAlchemyItemInfo.Text = "Envanterde yükseltilebilir eşya bulunamadı.";
                lblAlchemyElixirInfo.Text = "Gereken Elixir: -";
                lblAlchemyPowderInfo.Text = "Gereken Powder: -";
            }
        }

        private void UpdateAlchemyMaterialPreview(AlchemyManager.UpgradeableItemInfo item)
        {
            if (item == null) return;
            int elixirCount = AlchemyManager.CountElixirs(item.RequiredElixir);
            int powderCount = AlchemyManager.CountLuckyPowder(item.Degree);

            lblAlchemyItemInfo.Text = $"Seçili: {item.Name} (+{item.Plus}) - D{item.Degree}";
            lblAlchemyElixirInfo.Text = $"Gereken: Elixir {item.RequiredElixir} (Envanterde: {elixirCount} adet)";
            lblAlchemyPowderInfo.Text = $"Gereken: Lucky Powder D{item.Degree} (Envanterde: {powderCount} adet)";
        }

        public void BuildTargetAssistTab()
        {
            if (TabPageV_Control01_TargetAssist_Panel != null)
                return;

            TargetAssistManager.Initialize();

            TabPageV_Control01_TargetAssist_Panel = new Panel
            {
                Name = "TabPageV_Control01_TargetAssist_Panel",
                BackColor = Theme.DarkTheme.BgDark,
                BorderStyle = BorderStyle.None,
                Visible = false,
                AutoScroll = true
            };

            // Top Pill Bar
            Panel pnlTopBar = new Panel
            {
                Location = new Point(4, 4),
                Size = new Size(740, 30),
                BackColor = Color.Transparent
            };
            Button btnPill = new Button
            {
                Text = "Target Assist",
                Location = new Point(0, 0),
                Size = new Size(115, 26),
                BackColor = Theme.DarkTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontBodyBold,
                Cursor = Cursors.Hand
            };
            btnPill.FlatAppearance.BorderSize = 0;
            pnlTopBar.Controls.Add(btnPill);
            TabPageV_Control01_TargetAssist_Panel.Controls.Add(pnlTopBar);

            // -------------------------------------------------------------
            // Card 1: Core Settings (Top-Left)
            // -------------------------------------------------------------
            gbxTargetAssistSettings = new Theme.ModernCard
            {
                Name = "gbxTargetAssistSettings",
                TitleText = "CORE SETTINGS",
                Location = new Point(4, 38),
                Size = new Size(370, 172),
                CardColor = Theme.DarkTheme.BgCard,
                BorderColor = Theme.DarkTheme.BorderSubtle,
                BorderRadius = 8
            };

            lblTargetAssistRuntimeStatus = new Label
            {
                Text = "No target candidates in range.",
                Location = new Point(14, 34),
                Size = new Size(342, 18),
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontCaption
            };

            lblTargetAssistEnabled = new Label
            {
                Text = "Enabled",
                Location = new Point(14, 56),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody
            };
            togTargetAssistEnabled = new Theme.ModernToggle
            {
                Location = new Point(140, 54),
                Size = new Size(50, 22),
                Checked = TargetAssistManager.Enabled
            };

            lblTargetAssistMaxRange = new Label
            {
                Text = "Max range",
                Location = new Point(14, 84),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody
            };
            nudTargetAssistMaxRange = new NumericUpDown
            {
                Location = new Point(140, 82),
                Size = new Size(216, 22),
                Minimum = 5,
                Maximum = 400,
                DecimalPlaces = 1,
                Increment = 1,
                Value = (decimal)TargetAssistManager.MaxRange,
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblTargetAssistRoleMode = new Label
            {
                Text = "Role mode",
                Location = new Point(14, 112),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody
            };
            cmbxTargetAssistRoleMode = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(140, 110),
                Size = new Size(216, 24),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody
            };
            cmbxTargetAssistRoleMode.Items.AddRange(new object[] { "civil", "thief", "hunterTrader" });
            cmbxTargetAssistRoleMode.SelectedIndex = Math.Max(0, Math.Min(2, (int)TargetAssistManager.RoleMode));

            lblTargetAssistCycleKey = new Label
            {
                Text = "Target cycle key",
                Location = new Point(14, 140),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody
            };
            tbxTargetAssistCycleKey = new TextBox
            {
                Location = new Point(140, 138),
                Size = new Size(116, 22),
                ReadOnly = true,
                Text = TargetAssistManager.TargetCycleKey,
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            btnTargetAssistCaptureKey = new Button
            {
                Text = "[ Capture ]",
                Location = new Point(262, 138),
                Size = new Size(94, 23),
                BackColor = Theme.DarkTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontCaptionBold,
                Cursor = Cursors.Hand
            };
            btnTargetAssistCaptureKey.FlatAppearance.BorderSize = 0;

            gbxTargetAssistSettings.Controls.Add(lblTargetAssistRuntimeStatus);
            gbxTargetAssistSettings.Controls.Add(lblTargetAssistEnabled);
            gbxTargetAssistSettings.Controls.Add(togTargetAssistEnabled);
            gbxTargetAssistSettings.Controls.Add(lblTargetAssistMaxRange);
            gbxTargetAssistSettings.Controls.Add(nudTargetAssistMaxRange);
            gbxTargetAssistSettings.Controls.Add(lblTargetAssistRoleMode);
            gbxTargetAssistSettings.Controls.Add(cmbxTargetAssistRoleMode);
            gbxTargetAssistSettings.Controls.Add(lblTargetAssistCycleKey);
            gbxTargetAssistSettings.Controls.Add(tbxTargetAssistCycleKey);
            gbxTargetAssistSettings.Controls.Add(btnTargetAssistCaptureKey);
            TabPageV_Control01_TargetAssist_Panel.Controls.Add(gbxTargetAssistSettings);

            // -------------------------------------------------------------
            // Card 2: Target Filters (Top-Right)
            // -------------------------------------------------------------
            gbxTargetAssistFilters = new Theme.ModernCard
            {
                Name = "gbxTargetAssistFilters",
                TitleText = "TARGET FILTERS",
                Location = new Point(382, 38),
                Size = new Size(358, 172),
                CardColor = Theme.DarkTheme.BgCard,
                BorderColor = Theme.DarkTheme.BorderSubtle,
                BorderRadius = 8
            };

            togTargetAssistIncludeDead = new Theme.ModernToggle
            {
                Text = "Include dead targets",
                Location = new Point(14, 46),
                Size = new Size(160, 24),
                Checked = TargetAssistManager.IncludeDeadTargets,
                Font = Theme.DarkTheme.FontBody
            };

            togTargetAssistIgnoreBloody = new Theme.ModernToggle
            {
                Text = "Ignore bloody storm targets",
                Location = new Point(180, 46),
                Size = new Size(170, 24),
                Checked = TargetAssistManager.IgnoreBloodyStormTargets,
                Font = Theme.DarkTheme.FontBody
            };

            togTargetAssistIgnoreSnow = new Theme.ModernToggle
            {
                Text = "Ignore snow shield targets",
                Location = new Point(14, 98),
                Size = new Size(160, 24),
                Checked = TargetAssistManager.IgnoreSnowShieldTargets,
                Font = Theme.DarkTheme.FontBody
            };

            togTargetAssistOnlyCustom = new Theme.ModernToggle
            {
                Text = "Only custom players",
                Location = new Point(180, 98),
                Size = new Size(170, 24),
                Checked = TargetAssistManager.OnlyCustomPlayers,
                Font = Theme.DarkTheme.FontBody
            };

            gbxTargetAssistFilters.Controls.Add(togTargetAssistIncludeDead);
            gbxTargetAssistFilters.Controls.Add(togTargetAssistIgnoreBloody);
            gbxTargetAssistFilters.Controls.Add(togTargetAssistIgnoreSnow);
            gbxTargetAssistFilters.Controls.Add(togTargetAssistOnlyCustom);
            TabPageV_Control01_TargetAssist_Panel.Controls.Add(gbxTargetAssistFilters);

            // -------------------------------------------------------------
            // Card 3: Custom Target Lists (Bottom)
            // -------------------------------------------------------------
            gbxTargetAssistLists = new Theme.ModernCard
            {
                Name = "gbxTargetAssistLists",
                TitleText = "CUSTOM TARGET LISTS",
                Location = new Point(4, 216),
                Size = new Size(736, 162),
                CardColor = Theme.DarkTheme.BgCard,
                BorderColor = Theme.DarkTheme.BorderSubtle,
                BorderRadius = 8
            };

            // Left: Ignored Guilds
            lblTargetAssistGuildHead = new Label
            {
                Text = "Ignored Guilds",
                Location = new Point(14, 34),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBodyBold
            };
            lblTargetAssistGuildsMeta = new Label
            {
                Text = "0 guilds",
                Location = new Point(220, 34),
                Size = new Size(130, 18),
                TextAlign = ContentAlignment.TopRight,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontCaption
            };
            tbxTargetAssistIgnoredGuildInput = new TextBox
            {
                Location = new Point(14, 54),
                Size = new Size(250, 22),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            btnTargetAssistAddGuild = new Button
            {
                Text = "+ Add",
                Location = new Point(270, 53),
                Size = new Size(80, 24),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.Accent,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontCaptionBold,
                Cursor = Cursors.Hand
            };
            btnTargetAssistAddGuild.FlatAppearance.BorderSize = 1;
            btnTargetAssistAddGuild.FlatAppearance.BorderColor = Theme.DarkTheme.BorderSubtle;

            lbxTargetAssistIgnoredGuilds = new ListBox
            {
                Location = new Point(14, 82),
                Size = new Size(250, 68),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            btnTargetAssistRemoveGuild = new Button
            {
                Text = "- Remove",
                Location = new Point(270, 82),
                Size = new Size(80, 24),
                BackColor = Color.FromArgb(120, 30, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontCaptionBold,
                Cursor = Cursors.Hand
            };
            btnTargetAssistRemoveGuild.FlatAppearance.BorderSize = 0;

            // Right: Custom Players
            lblTargetAssistPlayerHead = new Label
            {
                Text = "Custom Players",
                Location = new Point(380, 34),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBodyBold
            };
            lblTargetAssistPlayersMeta = new Label
            {
                Text = "0 players",
                Location = new Point(586, 34),
                Size = new Size(130, 18),
                TextAlign = ContentAlignment.TopRight,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontCaption
            };
            tbxTargetAssistCustomPlayerInput = new TextBox
            {
                Location = new Point(380, 54),
                Size = new Size(250, 22),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            btnTargetAssistAddPlayer = new Button
            {
                Text = "+ Add",
                Location = new Point(636, 53),
                Size = new Size(80, 24),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.Accent,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontCaptionBold,
                Cursor = Cursors.Hand
            };
            btnTargetAssistAddPlayer.FlatAppearance.BorderSize = 1;
            btnTargetAssistAddPlayer.FlatAppearance.BorderColor = Theme.DarkTheme.BorderSubtle;

            lbxTargetAssistCustomPlayers = new ListBox
            {
                Location = new Point(380, 82),
                Size = new Size(250, 68),
                BackColor = Theme.DarkTheme.BgInput,
                ForeColor = Theme.DarkTheme.TextPrimary,
                Font = Theme.DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };
            btnTargetAssistRemovePlayer = new Button
            {
                Text = "- Remove",
                Location = new Point(636, 82),
                Size = new Size(80, 24),
                BackColor = Color.FromArgb(120, 30, 30),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontCaptionBold,
                Cursor = Cursors.Hand
            };
            btnTargetAssistRemovePlayer.FlatAppearance.BorderSize = 0;

            gbxTargetAssistLists.Controls.Add(lblTargetAssistGuildHead);
            gbxTargetAssistLists.Controls.Add(lblTargetAssistGuildsMeta);
            gbxTargetAssistLists.Controls.Add(tbxTargetAssistIgnoredGuildInput);
            gbxTargetAssistLists.Controls.Add(btnTargetAssistAddGuild);
            gbxTargetAssistLists.Controls.Add(lbxTargetAssistIgnoredGuilds);
            gbxTargetAssistLists.Controls.Add(btnTargetAssistRemoveGuild);

            gbxTargetAssistLists.Controls.Add(lblTargetAssistPlayerHead);
            gbxTargetAssistLists.Controls.Add(lblTargetAssistPlayersMeta);
            gbxTargetAssistLists.Controls.Add(tbxTargetAssistCustomPlayerInput);
            gbxTargetAssistLists.Controls.Add(btnTargetAssistAddPlayer);
            gbxTargetAssistLists.Controls.Add(lbxTargetAssistCustomPlayers);
            gbxTargetAssistLists.Controls.Add(btnTargetAssistRemovePlayer);

            TabPageV_Control01_TargetAssist_Panel.Controls.Add(gbxTargetAssistLists);

            // -------------------------------------------------------------
            // Footer Row: Help + Save
            // -------------------------------------------------------------
            lblTargetAssistFooterHelp = new Label
            {
                Text = "Changes apply instantly.",
                Location = new Point(14, 386),
                AutoSize = true,
                ForeColor = Theme.DarkTheme.TextMuted,
                Font = Theme.DarkTheme.FontCaption
            };

            btnTargetAssistSave = new Button
            {
                Text = "Save",
                Location = new Point(640, 382),
                Size = new Size(100, 28),
                BackColor = Theme.DarkTheme.Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Theme.DarkTheme.FontBodyBold,
                Cursor = Cursors.Hand
            };
            btnTargetAssistSave.FlatAppearance.BorderSize = 0;

            TabPageV_Control01_TargetAssist_Panel.Controls.Add(lblTargetAssistFooterHelp);
            TabPageV_Control01_TargetAssist_Panel.Controls.Add(btnTargetAssistSave);

            // Hook Event Listeners
            HookTargetAssistEvents();
            RefreshTargetAssistControls();
        }

        private void HookTargetAssistEvents()
        {
            togTargetAssistEnabled.CheckedChanged += (s, e) => TargetAssistManager.Enabled = togTargetAssistEnabled.Checked;
            nudTargetAssistMaxRange.ValueChanged += (s, e) => TargetAssistManager.MaxRange = (double)nudTargetAssistMaxRange.Value;
            cmbxTargetAssistRoleMode.SelectedIndexChanged += (s, e) =>
            {
                if (cmbxTargetAssistRoleMode.SelectedIndex >= 0)
                    TargetAssistManager.RoleMode = (TargetAssistRoleMode)cmbxTargetAssistRoleMode.SelectedIndex;
            };

            togTargetAssistIncludeDead.CheckedChanged += (s, e) => TargetAssistManager.IncludeDeadTargets = togTargetAssistIncludeDead.Checked;
            togTargetAssistIgnoreBloody.CheckedChanged += (s, e) => TargetAssistManager.IgnoreBloodyStormTargets = togTargetAssistIgnoreBloody.Checked;
            togTargetAssistIgnoreSnow.CheckedChanged += (s, e) => TargetAssistManager.IgnoreSnowShieldTargets = togTargetAssistIgnoreSnow.Checked;
            togTargetAssistOnlyCustom.CheckedChanged += (s, e) => TargetAssistManager.OnlyCustomPlayers = togTargetAssistOnlyCustom.Checked;

            // Capture Hotkey
            btnTargetAssistCaptureKey.Click += (s, e) =>
            {
                _isCapturingCycleKey = true;
                btnTargetAssistCaptureKey.Text = "[ Press key ]";
                btnTargetAssistCaptureKey.BackColor = Theme.DarkTheme.AccentHover;
                tbxTargetAssistCycleKey.Focus();
            };

            KeyEventHandler handleKeyCapture = (s, e) =>
            {
                if (!_isCapturingCycleKey) return;
                e.SuppressKeyPress = true;
                _isCapturingCycleKey = false;
                string keyName = e.KeyCode.ToString();
                TargetAssistManager.TargetCycleKey = keyName;
                tbxTargetAssistCycleKey.Text = keyName;
                btnTargetAssistCaptureKey.Text = "[ Capture ]";
                btnTargetAssistCaptureKey.BackColor = Theme.DarkTheme.Accent;
            };

            tbxTargetAssistCycleKey.KeyDown += handleKeyCapture;
            btnTargetAssistCaptureKey.KeyDown += handleKeyCapture;

            // Add/Remove Guilds
            btnTargetAssistAddGuild.Click += (s, e) =>
            {
                string guild = tbxTargetAssistIgnoredGuildInput.Text != null ? tbxTargetAssistIgnoredGuildInput.Text.Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(guild) && !TargetAssistManager.IgnoredGuilds.Contains(guild))
                {
                    TargetAssistManager.IgnoredGuilds.Add(guild);
                    lbxTargetAssistIgnoredGuilds.Items.Add(guild);
                    tbxTargetAssistIgnoredGuildInput.Clear();
                    lblTargetAssistGuildsMeta.Text = lbxTargetAssistIgnoredGuilds.Items.Count + " guilds";
                }
            };

            btnTargetAssistRemoveGuild.Click += (s, e) =>
            {
                if (lbxTargetAssistIgnoredGuilds.SelectedItem is string guild)
                {
                    TargetAssistManager.IgnoredGuilds.Remove(guild);
                    lbxTargetAssistIgnoredGuilds.Items.Remove(guild);
                    lblTargetAssistGuildsMeta.Text = lbxTargetAssistIgnoredGuilds.Items.Count + " guilds";
                }
            };

            // Add/Remove Players
            btnTargetAssistAddPlayer.Click += (s, e) =>
            {
                string player = tbxTargetAssistCustomPlayerInput.Text != null ? tbxTargetAssistCustomPlayerInput.Text.Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(player) && !TargetAssistManager.CustomPlayers.Contains(player))
                {
                    TargetAssistManager.CustomPlayers.Add(player);
                    lbxTargetAssistCustomPlayers.Items.Add(player);
                    tbxTargetAssistCustomPlayerInput.Clear();
                    lblTargetAssistPlayersMeta.Text = lbxTargetAssistCustomPlayers.Items.Count + " players";
                }
            };

            btnTargetAssistRemovePlayer.Click += (s, e) =>
            {
                if (lbxTargetAssistCustomPlayers.SelectedItem is string player)
                {
                    TargetAssistManager.CustomPlayers.Remove(player);
                    lbxTargetAssistCustomPlayers.Items.Remove(player);
                    lblTargetAssistPlayersMeta.Text = lbxTargetAssistCustomPlayers.Items.Count + " players";
                }
            };

            // Save
            btnTargetAssistSave.Click += (s, e) =>
            {
                TargetAssistManager.Enabled = togTargetAssistEnabled.Checked;
                TargetAssistManager.MaxRange = (double)nudTargetAssistMaxRange.Value;
                if (cmbxTargetAssistRoleMode.SelectedIndex >= 0)
                    TargetAssistManager.RoleMode = (TargetAssistRoleMode)cmbxTargetAssistRoleMode.SelectedIndex;
                TargetAssistManager.TargetCycleKey = tbxTargetAssistCycleKey.Text;
                TargetAssistManager.IncludeDeadTargets = togTargetAssistIncludeDead.Checked;
                TargetAssistManager.IgnoreBloodyStormTargets = togTargetAssistIgnoreBloody.Checked;
                TargetAssistManager.IgnoreSnowShieldTargets = togTargetAssistIgnoreSnow.Checked;
                TargetAssistManager.OnlyCustomPlayers = togTargetAssistOnlyCustom.Checked;

                Settings.SaveBotSettings();
                LogProcess("[Target Assist] Ayarlar başarıyla kaydedildi.");
            };

            // Live status update timer
            if (_targetAssistUiTimer == null)
            {
                _targetAssistUiTimer = new Timer();
                _targetAssistUiTimer.Interval = 500;
                _targetAssistUiTimer.Tick += (s, e) => UpdateTargetAssistStatusLabel();
                _targetAssistUiTimer.Start();
            }
        }

        public void RefreshTargetAssistControls()
        {
            if (TabPageV_Control01_TargetAssist_Panel == null) return;

            togTargetAssistEnabled.Checked = TargetAssistManager.Enabled;
            nudTargetAssistMaxRange.Value = (decimal)TargetAssistManager.MaxRange;
            cmbxTargetAssistRoleMode.SelectedIndex = Math.Max(0, Math.Min(2, (int)TargetAssistManager.RoleMode));
            tbxTargetAssistCycleKey.Text = TargetAssistManager.TargetCycleKey;

            togTargetAssistIncludeDead.Checked = TargetAssistManager.IncludeDeadTargets;
            togTargetAssistIgnoreBloody.Checked = TargetAssistManager.IgnoreBloodyStormTargets;
            togTargetAssistIgnoreSnow.Checked = TargetAssistManager.IgnoreSnowShieldTargets;
            togTargetAssistOnlyCustom.Checked = TargetAssistManager.OnlyCustomPlayers;

            lbxTargetAssistIgnoredGuilds.Items.Clear();
            foreach (var g in TargetAssistManager.IgnoredGuilds)
                lbxTargetAssistIgnoredGuilds.Items.Add(g);
            lblTargetAssistGuildsMeta.Text = lbxTargetAssistIgnoredGuilds.Items.Count + " guilds";

            lbxTargetAssistCustomPlayers.Items.Clear();
            foreach (var p in TargetAssistManager.CustomPlayers)
                lbxTargetAssistCustomPlayers.Items.Add(p);
            lblTargetAssistPlayersMeta.Text = lbxTargetAssistCustomPlayers.Items.Count + " players";

            UpdateTargetAssistStatusLabel();
        }

        private void UpdateTargetAssistStatusLabel()
        {
            if (lblTargetAssistRuntimeStatus == null || TabPageV_Control01_TargetAssist_Panel == null || !TabPageV_Control01_TargetAssist_Panel.Visible)
                return;

            try
            {
                var info = TargetAssistManager.GetStatusInfo();
                string status = TargetAssistPolicy.FormatCandidateStatus(info.count, info.nearestName, info.nearestDistance);
                if (lblTargetAssistRuntimeStatus.Text != status)
                    lblTargetAssistRuntimeStatus.Text = status;
            }
            catch { }
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

            Skills_cbxCastInOrder.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            Skills_cbxCastInOrder.ForeColor = Color.White;
            Skills_cbxCastInOrder.Checked = SkillManager.InOrderCombo;
            Skills_cbxCastInOrder.CheckedChanged += (s, e) =>
            {
                SkillManager.InOrderCombo = Skills_cbxCastInOrder.Checked;
                Settings.SaveCharacterSettings();
            };

            // Move Up and Down buttons for the attack skill list
            // Their exact X/Y will be set dynamically by ApplyModernContentPanels;
            // they are placed here with placeholder position and resized after layout.
            Button btnSkillUp = new Button
            {
                Text = "▲", Size = new Size(34, 28),
                Location = new Point(498, 4),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 100, 180)
            };
            btnSkillUp.FlatAppearance.BorderSize = 0;
            btnSkillUp.Click += (s, e) =>
            {
                // Target the currently visible attack list
                ListView activeList = Skills_cmbxAttackMobType?.Tag as ListView;
                if (activeList != null)
                {
                    SkillManager.MoveSelectedItemUp(activeList);
                    Settings.SaveCharacterSettings();
                }
                else
                {
                    SkillManager.MoveSelectedItemUp(Skills_lstvSkills);
                }
            };

            Button btnSkillDown = new Button
            {
                Text = "▼", Size = new Size(34, 28),
                Location = new Point(536, 4),
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 100, 180)
            };
            btnSkillDown.FlatAppearance.BorderSize = 0;
            btnSkillDown.Click += (s, e) =>
            {
                ListView activeList = Skills_cmbxAttackMobType?.Tag as ListView;
                if (activeList != null)
                {
                    SkillManager.MoveSelectedItemDown(activeList);
                    Settings.SaveCharacterSettings();
                }
                else
                {
                    SkillManager.MoveSelectedItemDown(Skills_lstvSkills);
                }
            };

            // Imbue Selector — placed in the bottom area of Option01 panel
            lblSkillImbue = new Label
            {
                Text = LocalizationManager.Get("UI_Imbue", "Imbue:"),
                Location = new Point(6, -1000), // will be repositioned by layout
                AutoSize = true, ForeColor = Color.LightGray
            };
            cmbxImbue = new ComboBox { Location = new Point(76, -1000), Size = new Size(190, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbxImbue.DropDownWidth = 320;
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
            cbxSkillNoAttack = new CheckBox
            {
                Text = LocalizationManager.Get("UI_NoAttack", "No Attack / Support"),
                Location = new Point(290, -1000), AutoSize = true,
                Checked = SkillManager.NoAttackMode, ForeColor = Color.White
            };
            cbxSkillNoAttack.CheckedChanged += (s, e) => { SkillManager.NoAttackMode = cbxSkillNoAttack.Checked; Settings.SaveCharacterSettings(); };

            // Devil Spirit checkbox
            cbxSkillDevil = new CheckBox
            {
                Text = LocalizationManager.Get("UI_DevilSkill", "Devil Spirit Becerisi"),
                Location = new Point(480, -1000), AutoSize = true,
                Checked = SkillManager.UseDevilSpirit, ForeColor = Color.White
            };
            cbxSkillDevil.CheckedChanged += (s, e) => { SkillManager.UseDevilSpirit = cbxSkillDevil.Checked; Settings.SaveCharacterSettings(); };

            lblSkillRuntimeStatus = new Label
            {
                AutoEllipsis = true,
                Location = new Point(6, -1000),
                Size = new Size(600, 20),
                ForeColor = Color.LightSkyBlue,
                Text = "Skill durumu: Hazır"
            };
            if (ToolTips != null)
                ToolTips.SetToolTip(lblSkillRuntimeStatus, "Son skill denemesinin sonucu ve fallback durumu");

            TabPageH_Skills_Option01_Panel.Controls.AddRange(new Control[]
            {
                btnSkillUp, btnSkillDown, lblSkillImbue, cmbxImbue, cbxSkillNoAttack, cbxSkillDevil,
                lblSkillRuntimeStatus
            });

            // Reposition the dynamically-added controls after layout is applied
            TabPageH_Skills_Option01_Panel.Layout += (s, e) => RepositionSkillPanelControls();
            RepositionSkillPanelControls();

            RefreshImbueSkillList();
        }

        private void RepositionSkillPanelControls()
        {
            if (TabPageH_Skills_Option01_Panel == null) return;
            int panelW = TabPageH_Skills_Option01_Panel.Width;
            int panelH = TabPageH_Skills_Option01_Panel.Height;
            // Approximate list bottom
            int listH = panelH - 120;
            int bottomRow1 = listH + 38;
            int bottomRow2 = bottomRow1 + 28;
            int bottomRow3 = bottomRow2 + 28;

            if (lblSkillImbue != null)    lblSkillImbue.Location    = new Point(6, bottomRow1 + 4);
            if (cmbxImbue != null)        cmbxImbue.Location        = new Point(6 + (lblSkillImbue?.Width ?? 56) + 6, bottomRow1);
            if (cbxSkillNoAttack != null) cbxSkillNoAttack.Location = new Point(6, bottomRow2);
            if (cbxSkillDevil != null)    cbxSkillDevil.Location    = new Point(250, bottomRow2);
            if (lblSkillRuntimeStatus != null)
            {
                lblSkillRuntimeStatus.Location = new Point(6, bottomRow3);
                lblSkillRuntimeStatus.Size = new Size(panelW - 12, 20);
            }

            // ▲ ▼ buttons — placed right after MobType combobox
            // Their location is managed by ApplyModernContentPanels and this helper
            // just positions them based on panel width if they exist.
            foreach (Control c in TabPageH_Skills_Option01_Panel.Controls)
            {
                if (c is Button btn && (btn.Text == "▲" || btn.Text == "▼"))
                {
                    btn.Location = btn.Text == "▲"
                        ? new Point(270, 4)
                        : new Point(308, 4);
                }
            }
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

            gbxProtectionSkillPet = new GroupBox
            {
                Text = LocalizationManager.Get("UI_Prot_Recovery", "Skill ve Pet Koruması"),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(10, 10),
                Size = new Size(305, 145),
                Font = new Font("Segoe UI", 8.5F)
            };

            cbxProtectionSkillHP = CreateProtectionCheck("HP < %:", new Point(12, 22), ProtectionManager.UseSkillHP, value => { ProtectionManager.UseSkillHP = value; Settings.SaveCharacterSettings(); });
            cbxProtectionSkillHP.Size = new Size(80, 20);
            nudProtectionSkillHP = CreateProtectionNumber(new Point(95, 21), 1, 99, ProtectionManager.SkillHPPercent, value => { ProtectionManager.SkillHPPercent = (byte)value; Settings.SaveCharacterSettings(); });
            nudProtectionSkillHP.Size = new Size(42, 20);
            lblProtectionSkillHPSuffix = new Label { Text = LocalizationManager.CurrentLanguage == "TR" ? "% ise skill bas" : "% then cast heal", Location = new Point(140, 23), AutoSize = true, ForeColor = Color.LightGray };

            cbxProtectionSkillMP = CreateProtectionCheck("MP < %:", new Point(12, 46), ProtectionManager.UseSkillMP, value => { ProtectionManager.UseSkillMP = value; Settings.SaveCharacterSettings(); });
            cbxProtectionSkillMP.Size = new Size(80, 20);
            nudProtectionSkillMP = CreateProtectionNumber(new Point(95, 45), 1, 99, ProtectionManager.SkillMPPercent, value => { ProtectionManager.SkillMPPercent = (byte)value; Settings.SaveCharacterSettings(); });
            nudProtectionSkillMP.Size = new Size(42, 20);
            lblProtectionSkillMPSuffix = new Label { Text = LocalizationManager.CurrentLanguage == "TR" ? "% ise skill bas" : "% then cast mana", Location = new Point(140, 47), AutoSize = true, ForeColor = Color.LightGray };

            cbxProtectionCure = CreateProtectionCheck("Kötü durumu skill ile temizle", new Point(12, 70), ProtectionManager.UseSkillBadStatus, value => { ProtectionManager.UseSkillBadStatus = value; Settings.SaveCharacterSettings(); });
            cbxProtectionPetRevive = CreateProtectionCheck("Ölen peti dirilt (Grass of Life)", new Point(12, 94), ProtectionManager.RevivePet, value => { ProtectionManager.RevivePet = value; Settings.SaveCharacterSettings(); });
            cbxProtectionPetSummon = CreateProtectionCheck("Peti otomatik çağır", new Point(12, 118), ProtectionManager.AutoSummonPet, value => { ProtectionManager.AutoSummonPet = value; Settings.SaveCharacterSettings(); });

            gbxProtectionSkillPet.Controls.AddRange(new Control[] {
                cbxProtectionSkillHP, nudProtectionSkillHP, lblProtectionSkillHPSuffix,
                cbxProtectionSkillMP, nudProtectionSkillMP, lblProtectionSkillMPSuffix,
                cbxProtectionCure, cbxProtectionPetRevive, cbxProtectionPetSummon
            });

            gbxProtectionAutoStat = new GroupBox
            {
                Text = LocalizationManager.Get("UI_Prot_AutoStat", "Otomatik Stat Dağıtımı (Auto Stat)"),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(10, 160),
                Size = new Size(305, 175),
                Font = new Font("Segoe UI", 8.5F)
            };

            cbxAutoStatEnabled = new CheckBox
            {
                Text = LocalizationManager.Get("UI_AutoStat", "Level atlayınca otomatik stat dağıt"),
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

            lblProtectionStr = new Label { Text = "STR:", Location = new Point(12, 50), AutoSize = true, ForeColor = Color.LightSkyBlue };
            nudAutoStatSTR = new NumericUpDown { Location = new Point(48, 48), Size = new Size(42, 20), Minimum = 0, Maximum = 3, Value = StatPointManager.TargetSTR };
            nudAutoStatSTR.ValueChanged += (s, e) =>
            {
                StatPointManager.TargetSTR = (int)nudAutoStatSTR.Value;
                Settings.SaveCharacterSettings();
            };

            lblProtectionInt = new Label { Text = "INT:", Location = new Point(105, 50), AutoSize = true, ForeColor = Color.Gold };
            nudAutoStatINT = new NumericUpDown { Location = new Point(140, 48), Size = new Size(42, 20), Minimum = 0, Maximum = 3, Value = StatPointManager.TargetINT };
            nudAutoStatINT.ValueChanged += (s, e) =>
            {
                StatPointManager.TargetINT = (int)nudAutoStatINT.Value;
                Settings.SaveCharacterSettings();
            };

            btnProtectionDistributeNow = new Button
            {
                Text = LocalizationManager.CurrentLanguage == "TR" ? "Puanları Dağıt" : "Distribute Now",
                Location = new Point(190, 46),
                Size = new Size(105, 24),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold)
            };
            btnProtectionDistributeNow.FlatAppearance.BorderSize = 0;
            btnProtectionDistributeNow.Click += (s, e) =>
            {
                StatPointManager.CheckAndDistribute();
                UpdateProtectionStatus();
            };

            lblProtectionStatGuidance = new Label
            {
                Text = LocalizationManager.CurrentLanguage == "TR"
                    ? "Her seviyede gelen 3 stat puanı seçilen oranda verilir.\n• 0 STR / 3 INT = Pure INT\n• 3 STR / 0 INT = Pure STR\n• 1 STR / 2 INT = Hibrit"
                    : "3 stat points per level are distributed as selected.\n• 0 STR / 3 INT = Pure INT\n• 3 STR / 0 INT = Pure STR\n• 1 STR / 2 INT = Hybrid",
                Location = new Point(12, 78),
                Size = new Size(280, 58),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 7.5F)
            };

            lblProtectionRemainInfo = new Label { Text = LocalizationManager.Get("UI_StatRemain", "Kalan Boş Stat Puanı:"), Location = new Point(12, 145), AutoSize = true, ForeColor = Color.LightSkyBlue };
            lblProtectionStatPointsRemain = new Label { Text = "--", Location = new Point(140, 145), AutoSize = true, ForeColor = Color.Gold, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };

            gbxProtectionAutoStat.Controls.AddRange(new Control[] {
                cbxAutoStatEnabled, lblProtectionStr, nudAutoStatSTR, lblProtectionInt, nudAutoStatINT,
                btnProtectionDistributeNow, lblProtectionStatGuidance, lblProtectionRemainInfo, lblProtectionStatPointsRemain
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

            gbxProtectionReturn = new GroupBox
            {
                Text = LocalizationManager.Get("UI_Prot_Town", "Kasabaya Dönüş Koşulları"),
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

            nudProtectionHP = CreateProtectionNumber(new Point(125, 93), 0, 100, ProtectionManager.HPLowThreshold, value => { ProtectionManager.HPLowThreshold = value; Settings.SaveCharacterSettings(); });
            nudProtectionMP = CreateProtectionNumber(new Point(125, 117), 0, 100, ProtectionManager.MPLowThreshold, value => { ProtectionManager.MPLowThreshold = value; Settings.SaveCharacterSettings(); });
            nudProtectionDurability = CreateProtectionNumber(new Point(260, 21), 0, 100, ProtectionManager.DurabilityLowThreshold, value => { ProtectionManager.DurabilityLowThreshold = value; Settings.SaveCharacterSettings(); });
            nudProtectionDurability.Size = new Size(40, 20);

            lblProtectionDeadDelay = new Label { Text = LocalizationManager.Get("UI_DeadDelay", "Gecikme (sn):"), Location = new Point(165, 120), AutoSize = true, ForeColor = Color.LightGray };
            nudProtectionDeadDelay = CreateProtectionNumber(new Point(250, 117), 0, 3600, ProtectionManager.DeadDelaySeconds, value => { ProtectionManager.DeadDelaySeconds = value; Settings.SaveCharacterSettings(); });
            gbxProtectionReturn.Controls.AddRange(new Control[] {
                cbxProtectionNoArrows, cbxProtectionFullInventory, cbxProtectionFullPetInventory,
                cbxProtectionLowHP, cbxProtectionLowMP, cbxProtectionDurability, cbxProtectionLevelUp,
                cbxProtectionStopInTown, cbxProtectionDead, nudProtectionHP, nudProtectionMP,
                nudProtectionDurability, lblProtectionDeadDelay, nudProtectionDeadDelay
            });

            gbxProtectionSummary = new GroupBox
            {
                Text = LocalizationManager.CurrentLanguage == "TR" ? "Hızlı Eylemler & Durum" : "Quick Actions & Status",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(325, 200),
                Size = new Size(320, 135),
                Font = new Font("Segoe UI", 8.5F)
            };

            btnProtectionManualPet = new Button
            {
                Text = LocalizationManager.CurrentLanguage == "TR" ? "Peti Kontrol Et (Dirilt/Çağır)" : "Check Pet (Revive/Summon)",
                Location = new Point(15, 24),
                Size = new Size(185, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204)
            };
            btnProtectionManualPet.FlatAppearance.BorderSize = 0;
            btnProtectionManualPet.Click += (s, e) =>
            {
                ProtectionManager.CheckPetProtection();
                Log("Pet kontrolü tetiklendi.");
            };

            btnProtectionSave = new Button
            {
                Text = LocalizationManager.Get("UI_Save", "Kaydet"),
                Location = new Point(210, 24),
                Size = new Size(95, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 137, 70)
            };
            btnProtectionSave.FlatAppearance.BorderSize = 0;
            btnProtectionSave.Click += (s, e) =>
            {
                Settings.SaveCharacterSettings();
                Log("Koruma ve Stat ayarları kaydedildi.");
            };

            lblProtectionOverview = new Label
            {
                Text = LocalizationManager.CurrentLanguage == "TR"
                    ? "Karakter koruması canlı döngüde çalışır. HP/MP durumunda acil skill kullanımı, ölen petin Grass of Life ile diriltilmesi ve şehre dönüş koşulları otomatik izlenir."
                    : "Protection runs on real-time loops. Emergency healing skills, Grass of Life pet revival, and town return triggers are monitored dynamically.",
                Location = new Point(15, 58),
                Size = new Size(290, 65),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 7.5F)
            };

            gbxProtectionSummary.Controls.AddRange(new Control[] {
                btnProtectionManualPet, btnProtectionSave, lblProtectionOverview
            });

            if (TabPageH_Character_Option03 != null)
                TabPageH_Character_Option03.Text = LocalizationManager.Get("UI_Protection", "Protection");
            if (TabPageH_Character_Option03_Panel != null)
                TabPageH_Character_Option03_Panel.Controls.AddRange(new Control[] { gbxProtectionSkillPet, gbxProtectionAutoStat, gbxProtectionReturn, gbxProtectionSummary });
        }

        private CheckBox CreateProtectionCheck(string text, Point location, bool isChecked, Action<bool> changed)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = text,
                Location = location,
                AutoSize = true,
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

            gbxItemFilterRules = new GroupBox
            {
                Text = LocalizationManager.Get("UI_ItemFilter", "Gelişmiş Eşya Filtresi"),
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(320, 10),
                Size = new Size(325, 150),
                Font = new Font("Segoe UI", 8.5F)
            };

            lblFilterDegree = new Label { Text = LocalizationManager.Get("UI_DegreeRange", "Degree:"), Location = new Point(12, 25), AutoSize = true, ForeColor = Color.White };
            nudFilterMinDegree = new NumericUpDown { Location = new Point(62, 23), Size = new Size(42, 20), Minimum = 1, Maximum = 16, Value = ItemFilterManager.MinDegree };
            nudFilterMinDegree.ValueChanged += (s, e) => ItemFilterManager.MinDegree = (int)nudFilterMinDegree.Value;
            Label lblTo = new Label { Text = "-", Location = new Point(108, 25), AutoSize = true, ForeColor = Color.White };
            nudFilterMaxDegree = new NumericUpDown { Location = new Point(120, 23), Size = new Size(42, 20), Minimum = 1, Maximum = 16, Value = ItemFilterManager.MaxDegree };
            nudFilterMaxDegree.ValueChanged += (s, e) => ItemFilterManager.MaxDegree = (int)nudFilterMaxDegree.Value;

            cbxFilterSox = CreateFilterCheck(LocalizationManager.Get("UI_SoxPrint", "Sadece SoX"), new Point(175, 23), ItemFilterManager.OnlySox, value => ItemFilterManager.OnlySox = value, Color.Gold);
            cbxFilterChina = CreateFilterCheck(LocalizationManager.Get("UI_FilterChina", "China"), new Point(12, 53), ItemFilterManager.FilterChina, value => ItemFilterManager.FilterChina = value, Color.White);
            cbxFilterEurope = CreateFilterCheck(LocalizationManager.Get("UI_FilterEurope", "Europe"), new Point(82, 53), ItemFilterManager.FilterEurope, value => ItemFilterManager.FilterEurope = value, Color.White);
            cbxFilterMale = CreateFilterCheck(LocalizationManager.Get("UI_FilterMale", "Male"), new Point(155, 53), ItemFilterManager.FilterMale, value => ItemFilterManager.FilterMale = value, Color.White);
            cbxFilterFemale = CreateFilterCheck(LocalizationManager.Get("UI_FilterFemale", "Female"), new Point(215, 53), ItemFilterManager.FilterFemale, value => ItemFilterManager.FilterFemale = value, Color.White);

            lblFilterInfo = new Label
            {
                Text = LocalizationManager.CurrentLanguage == "TR"
                    ? "Global filtreler yalnızca ekipmana uygulanır; açık item kuralları her zaman önceliklidir."
                    : "Global filters only apply to equipment; specific item rules always have priority.",
                Location = new Point(12, 83),
                Size = new Size(295, 42),
                ForeColor = Color.LightGray
            };
            gbxItemFilterRules.Controls.AddRange(new Control[] { lblFilterDegree, nudFilterMinDegree, lblTo, nudFilterMaxDegree, cbxFilterSox, cbxFilterChina, cbxFilterEurope, cbxFilterMale, cbxFilterFemale, lblFilterInfo });

            gbxItemFilterRulesCustom = new GroupBox
            {
                Text = LocalizationManager.CurrentLanguage == "TR" ? "Item Bazlı Pickup / Sell / Store Kuralları" : "Per-Item Pickup / Sell / Store Rules",
                ForeColor = Color.FromArgb(0, 122, 204),
                Location = new Point(320, 170),
                Size = new Size(325, 160),
                Font = new Font("Segoe UI", 8.5F)
            };

            tbxItemRuleName = new TextBox { Location = new Point(12, 22), Size = new Size(155, 21) };
            if (ToolTips != null)
                ToolTips.SetToolTip(tbxItemRuleName, "Item adı veya servername");
            btnItemFilterSaveRule = new Button { Text = LocalizationManager.Get("UI_AddRule", "Kaydet"), Location = new Point(173, 20), Size = new Size(65, 24), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(0, 122, 204) };
            btnItemFilterRemoveRule = new Button { Text = LocalizationManager.Get("UI_DeleteAccount", "Sil"), Location = new Point(243, 20), Size = new Size(65, 24), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(80, 80, 85) };

            cbxItemRulePickup = CreateRuleCheck(LocalizationManager.Get("UI_AddPickup", "Pickup"), new Point(12, 51), true);
            cbxItemRuleSell = CreateRuleCheck(LocalizationManager.Get("UI_AddSell", "Sell"), new Point(102, 51), false);
            cbxItemRuleStore = CreateRuleCheck(LocalizationManager.Get("UI_AddStore", "Store"), new Point(180, 51), false);

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
            btnItemFilterSaveRule.Click += (s, e) => SaveItemRule();
            btnItemFilterRemoveRule.Click += (s, e) => RemoveSelectedItemRule();

            gbxItemFilterRulesCustom.Controls.AddRange(new Control[] { tbxItemRuleName, btnItemFilterSaveRule, btnItemFilterRemoveRule, cbxItemRulePickup, cbxItemRuleSell, cbxItemRuleStore, lstvItemRules });
            TabPageH_Town_Option03_Panel.Controls.AddRange(new Control[] { gbxItemFilterRules, gbxItemFilterRulesCustom });
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
                UpdateModernStatusBars();
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

                    // Header Controls
                    if (btnQuickSave != null)
                    {
                        btnQuickSave.Text = isTR ? "KAYDET" : "SAVE";
                        if (ToolTips != null) ToolTips.SetToolTip(btnQuickSave, LocalizationManager.Get("UI_Save", "Save Settings"));
                    }
                    if (btnQuickHideClient != null)
                    {
                        btnQuickHideClient.Text = isTR ? (ClientManager.IsClientHidden ? "GÖSTER" : "GİZLE") : (ClientManager.IsClientHidden ? "SHOW" : "HIDE");
                        if (ToolTips != null) ToolTips.SetToolTip(btnQuickHideClient, LocalizationManager.Get("UI_HideClient", "Hide Client"));
                    }
                    if (btnCommandCenter != null)
                    {
                        btnCommandCenter.Text = isTR ? "KOMUT" : "CMD";
                        if (ToolTips != null) ToolTips.SetToolTip(btnCommandCenter, LocalizationManager.Get("UI_CommandCenter", "Command Center"));
                    }

                    // Modern Sidebar Categories and Items
                    if (modernSidebar != null)
                    {
                        string catBot = LocalizationManager.Get("UI_Cat_BotSettings", "BOT SETTINGS");
                        string catCom = LocalizationManager.Get("UI_Cat_Community", "COMMUNITY");
                        string catSys = LocalizationManager.Get("UI_Cat_System", "SYSTEM");

                        modernSidebar.UpdateItemText("Login", LocalizationManager.Get("UI_Tab_Login", "General / Login"), catBot);
                        modernSidebar.UpdateItemText("Training", LocalizationManager.Get("UI_Tab_Training", "Training"), catBot);
                        modernSidebar.UpdateItemText("Skills", LocalizationManager.Get("UI_Tab_Skills", "Skills"), catBot);
                        modernSidebar.UpdateItemText("Character", LocalizationManager.Get("UI_Tab_Character", "Protection"), catBot);
                        modernSidebar.UpdateItemText("Town", LocalizationManager.Get("UI_Tab_Town", "Town & Items"), catBot);
                        modernSidebar.UpdateItemText("Alchemy", LocalizationManager.Get("UI_Tab_Alchemy", "Alchemy (+ Fuse)"), catBot);
                        modernSidebar.UpdateItemText("TargetAssist", LocalizationManager.Get("UI_Tab_TargetAssist", "Target Assist"), catBot);

                        modernSidebar.UpdateItemText("Inventory", LocalizationManager.Get("UI_Tab_Inventory", "Inventory"), catCom);
                        modernSidebar.UpdateItemText("Party", LocalizationManager.Get("UI_Tab_Party", "Party"), catCom);
                        modernSidebar.UpdateItemText("Guild", LocalizationManager.Get("UI_Tab_Guild", "Guild"), catCom);
                        modernSidebar.UpdateItemText("Academy", LocalizationManager.Get("UI_Tab_Academy", "Academy"), catCom);
                        modernSidebar.UpdateItemText("Players", LocalizationManager.Get("UI_Tab_Players", "Players"), catCom);
                        modernSidebar.UpdateItemText("Chat", LocalizationManager.Get("UI_Tab_Chat", "Chat"), catCom);
                        modernSidebar.UpdateItemText("Stall", LocalizationManager.Get("UI_Tab_Stall", "Stall"), catCom);

                        modernSidebar.UpdateItemText("Minimap", LocalizationManager.Get("UI_Tab_Minimap", "Minimap"), catSys);
                        modernSidebar.UpdateItemText("GameInfo", LocalizationManager.Get("UI_Tab_GameInfo", "Game Info"), catSys);
                        modernSidebar.UpdateItemText("Settings", LocalizationManager.Get("UI_Tab_Settings", "Settings"), catSys);
                    }

                    // Classic TabPages (fallback)
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

                    // General & Login Strategy
                    if (gbxStrategy != null) gbxStrategy.TitleText = LocalizationManager.Get("UI_StrategyCard_Title", "LOGIN STRATEGY & AUTOMATION");
                    if (cbxGeneralAutoLogin != null) cbxGeneralAutoLogin.Text = LocalizationManager.Get("UI_AutomatedLogin", "Automated Login");
                    if (cbxGeneralAutoStart != null) cbxGeneralAutoStart.Text = LocalizationManager.Get("UI_AutoStartBot", "Auto Start Bot on In-Game");
                    if (cbxGeneralAutoHide != null) cbxGeneralAutoHide.Text = LocalizationManager.Get("UI_AutoHideClient", "Auto Hide Silkroad Client");
                    if (cbxGeneralStaticCaptcha != null) cbxGeneralStaticCaptcha.Text = LocalizationManager.Get("UI_StaticCaptcha", "Use Static Captcha Code");
                    if (cbxGeneralStayConnected != null) cbxGeneralStayConnected.Text = LocalizationManager.Get("UI_StayConnected", "Stay Connected (Crash/DC Failover)");
                    if (rbnGeneralFirstFound != null) rbnGeneralFirstFound.Text = LocalizationManager.Get("UI_StrategyFirst", "First Found Character");
                    if (rbnGeneralHighestLevel != null) rbnGeneralHighestLevel.Text = LocalizationManager.Get("UI_StrategyHighest", "Highest Level Character");
                    if (Login_lblAccount != null) Login_lblAccount.Text = LocalizationManager.Get("UI_SavedAccounts", "Saved Accounts:");
                    if (lblGeneralLoginDelay != null) lblGeneralLoginDelay.Text = LocalizationManager.Get("UI_LoginDelay", "Login Delay (sec):");
                    if (lblGeneralWaitDC != null) lblGeneralWaitDC.Text = LocalizationManager.Get("UI_WaitAfterDC", "Wait After DC (min):");
                    if (lblGeneralCharStrategy != null) lblGeneralCharStrategy.Text = LocalizationManager.Get("UI_CharSelectStrategy", "Character Selection Strategy:");
                    if (lblGeneralStrategyInfo != null)
                        lblGeneralStrategyInfo.Text = isTR
                            ? "Kayıtlı hesabı seçip START butonuna bastığınızda bot oyuna otomatik giriş yapacaktır."
                            : "Select a saved account and press START to automatically log into the game.";
                    if (ToolTips != null)
                    {
                        if (Login_btnSaveAccount != null) ToolTips.SetToolTip(Login_btnSaveAccount, LocalizationManager.Get("UI_SaveAccount", "Save Account"));
                        if (Login_btnDeleteAccount != null) ToolTips.SetToolTip(Login_btnDeleteAccount, LocalizationManager.Get("UI_DeleteAccount", "Delete"));
                        if (Login_btnAccountSetup != null) ToolTips.SetToolTip(Login_btnAccountSetup, LocalizationManager.Get("UI_AccountSetupTip", "Advanced Account & PIN Setup"));
                    }

                    // Combat AI & Training
                    if (btnTrainingCombat != null) btnTrainingCombat.Text = LocalizationManager.Get("UI_CombatAI_Tab", "Combat AI");
                    if (Combat_gbxAI != null) Combat_gbxAI.Text = LocalizationManager.Get("UI_CombatAI_Title", "COMBAT AI");
                    if (cbxCombatZerkFullHP != null) cbxCombatZerkFullHP.Text = LocalizationManager.Get("UI_ZerkHPFull", "Berserk When HP is Full");
                    if (cbxCombatIgnorePillars != null) cbxCombatIgnorePillars.Text = LocalizationManager.Get("UI_IgnoreDimensionPillars", "Ignore Dimension Pillars");
                    if (cbxCombatWeakerFirst != null) cbxCombatWeakerFirst.Text = LocalizationManager.Get("UI_AttackWeakerFirst", "Attack Weaker Mobs First");
                    if (cbxCombatDoNotFollow != null) cbxCombatDoNotFollow.Text = LocalizationManager.Get("UI_DoNotFollowMobs", "Do Not Follow Mobs Beyond Radius");
                    if (cbxCombatZerkCount != null) cbxCombatZerkCount.Text = LocalizationManager.Get("UI_ZerkMobCount", "Berserk When Surrounding Mobs >=");
                    if (cbxCombatZerkAvoidance != null) cbxCombatZerkAvoidance.Text = LocalizationManager.Get("UI_ZerkAvoidance", "Avoidance Based Berserk");
                    if (cbxCombatZerkRarity != null) cbxCombatZerkRarity.Text = LocalizationManager.Get("UI_ZerkRarity", "Berserk on Champion/Giant/Leader");

                    // Skills
                    if (Skills_cbxCastInOrder != null) Skills_cbxCastInOrder.Text = LocalizationManager.Get("UI_InOrder", "Cast Skills in Order (Combo)");
                    if (cbxSkillNoAttack != null) cbxSkillNoAttack.Text = LocalizationManager.Get("UI_NoAttack", "No Attack (Buff/Follow Only)");
                    if (cbxSkillDevil != null) cbxSkillDevil.Text = LocalizationManager.Get("UI_DevilSkill", "Use Malicious Devil Skill");
                    if (lblSkillImbue != null) lblSkillImbue.Text = LocalizationManager.Get("UI_Imbue", "Use Imbue Skill") + ":";

                    // Protection
                    if (gbxProtectionSkillPet != null) gbxProtectionSkillPet.Text = LocalizationManager.Get("UI_Prot_Recovery", "HEALTH / MANA / RECOVERY");
                    if (cbxProtectionSkillHP != null) cbxProtectionSkillHP.Text = LocalizationManager.Get("UI_SkillHP", "Heal Skill if HP < %");
                    if (lblProtectionSkillHPSuffix != null) lblProtectionSkillHPSuffix.Text = isTR ? "% ise skill bas" : "% then cast heal";
                    if (cbxProtectionSkillMP != null) cbxProtectionSkillMP.Text = LocalizationManager.Get("UI_SkillMP", "Restore Skill if MP < %");
                    if (lblProtectionSkillMPSuffix != null) lblProtectionSkillMPSuffix.Text = isTR ? "% ise skill bas" : "% then cast mana";
                    if (cbxProtectionCure != null) cbxProtectionCure.Text = LocalizationManager.Get("UI_SkillCure", "Cure Bad Status with Skill");
                    if (cbxProtectionPetRevive != null) cbxProtectionPetRevive.Text = LocalizationManager.Get("UI_PetRevive", "Auto Revive Pet (Grass of Life)");
                    if (cbxProtectionPetSummon != null) cbxProtectionPetSummon.Text = LocalizationManager.Get("UI_PetAutoSummon", "Auto Summon Pet");

                    if (gbxProtectionAutoStat != null) gbxProtectionAutoStat.Text = LocalizationManager.Get("UI_Prot_AutoStat", "AUTO STAT DISTRIBUTION");
                    if (cbxAutoStatEnabled != null) cbxAutoStatEnabled.Text = LocalizationManager.Get("UI_AutoStat", "Auto Distribute Stat Points");
                    if (btnProtectionDistributeNow != null) btnProtectionDistributeNow.Text = isTR ? "Puanları Dağıt" : "Distribute Now";
                    if (lblProtectionStatGuidance != null)
                        lblProtectionStatGuidance.Text = isTR
                            ? "Her seviyede gelen 3 stat puanı seçilen oranda verilir.\n• 0 STR / 3 INT = Pure INT\n• 3 STR / 0 INT = Pure STR\n• 1 STR / 2 INT = Hibrit"
                            : "3 stat points per level are distributed as selected.\n• 0 STR / 3 INT = Pure INT\n• 3 STR / 0 INT = Pure STR\n• 1 STR / 2 INT = Hybrid";
                    if (lblProtectionRemainInfo != null) lblProtectionRemainInfo.Text = LocalizationManager.Get("UI_StatRemain", "Remaining Stat Points:");

                    if (gbxProtectionReturn != null) gbxProtectionReturn.Text = LocalizationManager.Get("UI_Prot_Town", "RETURN TO TOWN TRIGGERS");
                    if (cbxProtectionNoArrows != null) cbxProtectionNoArrows.Text = LocalizationManager.Get("UI_NoArrows", "No Arrows / Bolts Left");
                    if (cbxProtectionFullInventory != null) cbxProtectionFullInventory.Text = LocalizationManager.Get("UI_FullInventory", "Inventory Full");
                    if (cbxProtectionFullPetInventory != null) cbxProtectionFullPetInventory.Text = LocalizationManager.Get("UI_FullPetInventory", "Pet Inventory Full");
                    if (cbxProtectionLowHP != null) cbxProtectionLowHP.Text = LocalizationManager.Get("UI_HPLow", "HP Potions Low (<=)");
                    if (cbxProtectionLowMP != null) cbxProtectionLowMP.Text = LocalizationManager.Get("UI_MPLow", "MP Potions Low (<=)");
                    if (cbxProtectionDurability != null) cbxProtectionDurability.Text = LocalizationManager.Get("UI_DurabilityLow", "Equipment Durability Low (<=)");
                    if (cbxProtectionLevelUp != null) cbxProtectionLevelUp.Text = LocalizationManager.Get("UI_LevelUp", "Return to Town on Level Up");
                    if (cbxProtectionStopInTown != null) cbxProtectionStopInTown.Text = LocalizationManager.Get("UI_StopInTown", "Stop Bot when in Town");
                    if (cbxProtectionDead != null) cbxProtectionDead.Text = LocalizationManager.Get("UI_BackToTown", "Back to Town");
                    if (lblProtectionDeadDelay != null) lblProtectionDeadDelay.Text = LocalizationManager.Get("UI_DeadDelay", "Death delay (sec):");

                    if (gbxProtectionSummary != null) gbxProtectionSummary.Text = isTR ? "Hızlı Eylemler & Durum" : "Quick Actions & Status";
                    if (btnProtectionManualPet != null) btnProtectionManualPet.Text = isTR ? "Peti Kontrol Et (Dirilt/Çağır)" : "Check Pet (Revive/Summon)";
                    if (btnProtectionSave != null) btnProtectionSave.Text = LocalizationManager.Get("UI_Save", "Save Settings");
                    if (lblProtectionOverview != null)
                        lblProtectionOverview.Text = isTR
                            ? "Karakter koruması canlı döngüde çalışır. HP/MP durumunda acil skill kullanımı, ölen petin Grass of Life ile diriltilmesi ve şehre dönüş koşulları otomatik izlenir."
                            : "Protection runs on real-time loops. Emergency healing skills, Grass of Life pet revival, and town return triggers are monitored dynamically.";

                    // Item Filter & Rules
                    if (gbxItemFilterRules != null) gbxItemFilterRules.Text = LocalizationManager.Get("UI_ItemFilter", "Item Filter & Rules");
                    if (lblFilterDegree != null) lblFilterDegree.Text = LocalizationManager.Get("UI_DegreeRange", "Degree Range:");
                    if (cbxFilterSox != null) cbxFilterSox.Text = LocalizationManager.Get("UI_SoxPrint", "Only SoX (SOS / SOM / SUN / Nova)");
                    if (cbxFilterChina != null) cbxFilterChina.Text = LocalizationManager.Get("UI_FilterChina", "Chinese Equipment");
                    if (cbxFilterEurope != null) cbxFilterEurope.Text = LocalizationManager.Get("UI_FilterEurope", "European Equipment");
                    if (cbxFilterMale != null) cbxFilterMale.Text = LocalizationManager.Get("UI_FilterMale", "Male Equipment");
                    if (cbxFilterFemale != null) cbxFilterFemale.Text = LocalizationManager.Get("UI_FilterFemale", "Female Equipment");
                    if (lblFilterInfo != null)
                        lblFilterInfo.Text = isTR
                            ? "Global filtreler yalnızca ekipmana uygulanır; açık item kuralları her zaman önceliklidir."
                            : "Global filters only apply to equipment; specific item rules always have priority.";
                    if (gbxItemFilterRulesCustom != null)
                        gbxItemFilterRulesCustom.Text = isTR ? "Item Bazlı Pickup / Sell / Store Kuralları" : "Per-Item Pickup / Sell / Store Rules";
                    if (btnItemFilterSaveRule != null) btnItemFilterSaveRule.Text = LocalizationManager.Get("UI_AddRule", "Add Rule");
                    if (btnItemFilterRemoveRule != null) btnItemFilterRemoveRule.Text = LocalizationManager.Get("UI_DeleteAccount", "Delete");
                    if (cbxItemRulePickup != null) cbxItemRulePickup.Text = LocalizationManager.Get("UI_AddPickup", "Pick");
                    if (cbxItemRuleSell != null) cbxItemRuleSell.Text = LocalizationManager.Get("UI_AddSell", "Sell");
                    if (cbxItemRuleStore != null) cbxItemRuleStore.Text = LocalizationManager.Get("UI_AddStore", "Store");

                    // Alchemy (+ Fuse)
                    if (gbxAlchemySettings != null) gbxAlchemySettings.TitleText = LocalizationManager.Get("UI_Alchemy_CardSettings", "ALCHEMY (+ FUSE) SETTINGS");
                    if (gbxAlchemyActions != null) gbxAlchemyActions.TitleText = LocalizationManager.Get("UI_Alchemy_CardActions", "ALCHEMY ACTIONS & LIVE STATUS");
                    if (lblAlchemySelect != null) lblAlchemySelect.Text = LocalizationManager.Get("UI_Alchemy_TargetItem", "Target Equipment (Inventory):");
                    if (lblAlchemySlot != null) lblAlchemySlot.Text = LocalizationManager.Get("UI_Alchemy_TargetSlot", "Inventory Slot (13-76):");
                    if (lblAlchemyPlus != null) lblAlchemyPlus.Text = LocalizationManager.Get("UI_Alchemy_TargetPlus", "Target Plus (+1 to +15):");
                    if (cbxAlchemyUsePowder != null) cbxAlchemyUsePowder.Text = LocalizationManager.Get("UI_Alchemy_UsePowder", "Use Lucky Powder (Auto-Match)");
                    if (lblAlchemyMaxAttempts != null) lblAlchemyMaxAttempts.Text = LocalizationManager.Get("UI_Alchemy_MaxAttempts", "Maximum Attempts Limit:");
                    if (lblAlchemyDelay != null) lblAlchemyDelay.Text = LocalizationManager.Get("UI_Alchemy_Delay", "Action Delay (ms):");
                    if (btnAlchemyStart != null) btnAlchemyStart.Text = LocalizationManager.Get("UI_Alchemy_Start", "▶ Start Alchemy");
                    if (btnAlchemyStop != null) btnAlchemyStop.Text = LocalizationManager.Get("UI_Alchemy_Stop", "⏹ Stop");
                    if (btnAlchemyReset != null) btnAlchemyReset.Text = LocalizationManager.Get("UI_Alchemy_Reset", "↺ Reset Counters");
                    if (lblAlchemyLogHead != null) lblAlchemyLogHead.Text = LocalizationManager.Get("UI_Alchemy_LogHead", "Live Alchemy Log:");
                    if (lblAlchemyStatus != null)
                        lblAlchemyStatus.Text = LocalizationManager.Get("UI_Alchemy_StatusPrefix", "Status: ") + AlchemyManager.LastStatus;
                    if (lblAlchemyAttempts != null)
                        lblAlchemyAttempts.Text = LocalizationManager.Get("UI_Alchemy_AttemptsPrefix", "Attempts: ") + $"{AlchemyManager.CurrentAttempts} / {AlchemyManager.MaxAttempts}";
                    if (lblAlchemySuccess != null)
                        lblAlchemySuccess.Text = LocalizationManager.Get("UI_Alchemy_SuccessPrefix", "Success: ") + AlchemyManager.SuccessCount;
                    if (lblAlchemyFailed != null)
                        lblAlchemyFailed.Text = LocalizationManager.Get("UI_Alchemy_FailedPrefix", "Failed: ") + AlchemyManager.FailCount;

                    // Target Assist
                    if (gbxTargetAssistSettings != null) gbxTargetAssistSettings.TitleText = LocalizationManager.Get("UI_TA_CardSettings", "CORE SETTINGS");
                    if (gbxTargetAssistFilters != null) gbxTargetAssistFilters.TitleText = LocalizationManager.Get("UI_TA_CardFilters", "TARGET FILTERS");
                    if (gbxTargetAssistLists != null) gbxTargetAssistLists.TitleText = LocalizationManager.Get("UI_TA_CardLists", "CUSTOM TARGET LISTS");
                    if (lblTargetAssistEnabled != null) lblTargetAssistEnabled.Text = LocalizationManager.Get("UI_TA_Enabled", "Enabled");
                    if (lblTargetAssistMaxRange != null) lblTargetAssistMaxRange.Text = LocalizationManager.Get("UI_TA_MaxRange", "Max range");
                    if (lblTargetAssistRoleMode != null) lblTargetAssistRoleMode.Text = LocalizationManager.Get("UI_TA_RoleMode", "Role mode");
                    if (lblTargetAssistCycleKey != null) lblTargetAssistCycleKey.Text = LocalizationManager.Get("UI_TA_CycleKey", "Target cycle key");
                    if (btnTargetAssistCaptureKey != null)
                        btnTargetAssistCaptureKey.Text = _isCapturingCycleKey ? LocalizationManager.Get("UI_TA_PressKey", "[ Press key ]") : LocalizationManager.Get("UI_TA_Capture", "[ Capture ]");
                    if (togTargetAssistIncludeDead != null) togTargetAssistIncludeDead.Text = LocalizationManager.Get("UI_TA_IncludeDead", "Include dead targets");
                    if (togTargetAssistIgnoreBloody != null) togTargetAssistIgnoreBloody.Text = LocalizationManager.Get("UI_TA_IgnoreBloody", "Ignore bloody storm targets");
                    if (togTargetAssistIgnoreSnow != null) togTargetAssistIgnoreSnow.Text = LocalizationManager.Get("UI_TA_IgnoreSnow", "Ignore snow shield targets");
                    if (togTargetAssistOnlyCustom != null) togTargetAssistOnlyCustom.Text = LocalizationManager.Get("UI_TA_OnlyCustom", "Only custom players");
                    if (lblTargetAssistGuildHead != null) lblTargetAssistGuildHead.Text = LocalizationManager.Get("UI_TA_IgnoredGuilds", "Ignored Guilds");
                    if (lblTargetAssistPlayerHead != null) lblTargetAssistPlayerHead.Text = LocalizationManager.Get("UI_TA_CustomPlayers", "Custom Players");
                    if (btnTargetAssistAddGuild != null) btnTargetAssistAddGuild.Text = LocalizationManager.Get("UI_TA_Add", "+ Add");
                    if (btnTargetAssistRemoveGuild != null) btnTargetAssistRemoveGuild.Text = LocalizationManager.Get("UI_TA_Remove", "- Remove");
                    if (btnTargetAssistAddPlayer != null) btnTargetAssistAddPlayer.Text = LocalizationManager.Get("UI_TA_Add", "+ Add");
                    if (btnTargetAssistRemovePlayer != null) btnTargetAssistRemovePlayer.Text = LocalizationManager.Get("UI_TA_Remove", "- Remove");
                    if (lblTargetAssistFooterHelp != null) lblTargetAssistFooterHelp.Text = LocalizationManager.Get("UI_TA_Footer", "Changes apply instantly.");
                    if (btnTargetAssistSave != null) btnTargetAssistSave.Text = LocalizationManager.Get("UI_TA_Save", "Save");

                    if (this.lblBotState != null && this.lblBotState.Text.Contains("Ready"))
                        this.lblBotState.Text = LocalizationManager.Get("UI_WaitingForChar", "Ready");
                });
            }
            catch { }
        }
    }
}
