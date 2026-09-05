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
        private System.Windows.Forms.Timer automatedLoginTimer;

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
                ApplyModernTheme();
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
            gbxStrategy.TitleText = "Giriş Akışı & Stratejisi (Login Flow & Strategy)";
            gbxStrategy.Location = new Point(252, 208);
            gbxStrategy.Size = new Size(519, 196);
            gbxStrategy.Font = Theme.DarkTheme.FontBody;

            cbxGeneralAutoLogin = new CheckBox { Text = "Otomatik Giriş", Location = new Point(14, 30), AutoSize = true, Checked = LoginStrategyManager.AutomatedLogin, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralAutoLogin.CheckedChanged += (s, e) =>
            {
                LoginStrategyManager.AutomatedLogin = cbxGeneralAutoLogin.Checked;
                Settings.SaveBotSettings();
            };

            cbxGeneralAutoStart = new CheckBox { Text = "Oyunda Botu Başlat", Location = new Point(140, 30), AutoSize = true, Checked = LoginStrategyManager.AutoStartBot, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralAutoStart.CheckedChanged += (s, e) => { LoginStrategyManager.AutoStartBot = cbxGeneralAutoStart.Checked; Settings.SaveBotSettings(); };

            cbxGeneralAutoHide = new CheckBox { Text = "İstemciyi Gizle", Location = new Point(290, 30), AutoSize = true, Checked = LoginStrategyManager.AutoHideClient, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralAutoHide.CheckedChanged += (s, e) => { LoginStrategyManager.AutoHideClient = cbxGeneralAutoHide.Checked; Settings.SaveBotSettings(); };

            cbxGeneralStaticCaptcha = new CheckBox { Text = "Sabit Captcha:", Location = new Point(14, 58), AutoSize = true, Checked = LoginStrategyManager.StaticCaptcha, ForeColor = Theme.DarkTheme.TextPrimary };
            cbxGeneralStaticCaptcha.CheckedChanged += (s, e) => { LoginStrategyManager.StaticCaptcha = cbxGeneralStaticCaptcha.Checked; Settings.SaveBotSettings(); };

            tbxGeneralStaticCaptchaCode = new TextBox { Location = new Point(126, 56), Size = new Size(50, 21), BackColor = Theme.DarkTheme.BgInput, ForeColor = Theme.DarkTheme.TextPrimary, BorderStyle = BorderStyle.FixedSingle, Text = LoginStrategyManager.StaticCaptchaCode ?? "" };
            tbxGeneralStaticCaptchaCode.TextChanged += (s, e) => { LoginStrategyManager.StaticCaptchaCode = tbxGeneralStaticCaptchaCode.Text; Settings.SaveBotSettings(); };
            if (ToolTips != null)
                ToolTips.SetToolTip(tbxGeneralStaticCaptchaCode, "Sabit captcha kodu (örn: 1234)");

            Label lblLoginDelay = new Label { Text = "Giriş Gecikmesi:", Location = new Point(185, 58), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };
            nudGeneralLoginDelay = new NumericUpDown { Location = new Point(280, 56), Size = new Size(45, 21), Minimum = 0, Maximum = 60, Value = LoginStrategyManager.LoginDelaySeconds, BackColor = Theme.DarkTheme.BgInput, ForeColor = Theme.DarkTheme.TextPrimary };
            nudGeneralLoginDelay.ValueChanged += (s, e) => { LoginStrategyManager.LoginDelaySeconds = (int)nudGeneralLoginDelay.Value; Settings.SaveBotSettings(); };
            Label lblLoginDelaySec = new Label { Text = "sn", Location = new Point(330, 58), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };

            Label lblWaitDC = new Label { Text = "DC Bekleme:", Location = new Point(14, 88), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };
            nudGeneralWaitAfterDC = new NumericUpDown { Location = new Point(95, 86), Size = new Size(45, 21), Minimum = 0, Maximum = 60, Value = LoginStrategyManager.WaitAfterDCMinutes, BackColor = Theme.DarkTheme.BgInput, ForeColor = Theme.DarkTheme.TextPrimary };
            nudGeneralWaitAfterDC.ValueChanged += (s, e) => { LoginStrategyManager.WaitAfterDCMinutes = (int)nudGeneralWaitAfterDC.Value; Settings.SaveBotSettings(); };
            Label lblWaitDCMin = new Label { Text = "dk", Location = new Point(145, 88), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };

            cbxGeneralStayConnected = new CheckBox { Text = "Çökme Koruması (Failover)", Location = new Point(185, 88), AutoSize = true, Checked = LoginStrategyManager.StayConnected, ForeColor = Theme.DarkTheme.InfoBlue };
            cbxGeneralStayConnected.CheckedChanged += (s, e) => { LoginStrategyManager.StayConnected = cbxGeneralStayConnected.Checked; Settings.SaveBotSettings(); };
            if (ToolTips != null)
                ToolTips.SetToolTip(cbxGeneralStayConnected, "İstemci aniden çökerse sunucu bağlantısını koparmadan Clientless moda geçer");

            Label lblCharStrategy = new Label { Text = "Karakter:", Location = new Point(14, 118), AutoSize = true, ForeColor = Theme.DarkTheme.TextMuted };
            rbnGeneralFirstFound = new RadioButton { Text = "İlk Karakter", Location = new Point(75, 118), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.FirstFound), ForeColor = Theme.DarkTheme.TextPrimary };
            rbnGeneralHighestLevel = new RadioButton { Text = "En Yüksek Seviyeli Karakter", Location = new Point(170, 118), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.HighestLevel), ForeColor = Theme.DarkTheme.Warning };

            rbnGeneralFirstFound.CheckedChanged += (s, e) => { if (rbnGeneralFirstFound.Checked) { LoginStrategyManager.Strategy = CharacterSelectionStrategy.FirstFound; Settings.SaveBotSettings(); } };
            rbnGeneralHighestLevel.CheckedChanged += (s, e) => { if (rbnGeneralHighestLevel.Checked) { LoginStrategyManager.Strategy = CharacterSelectionStrategy.HighestLevel; Settings.SaveBotSettings(); } };

            Label lblStrategyInfo = new Label
            {
                Text = "Kayıtlı hesabı seçip START butonuna bastığınızda bot oyuna otomatik giriş yapacaktır.",
                Location = new Point(14, 148),
                Size = new Size(490, 36),
                ForeColor = Theme.DarkTheme.TextFaint,
                Font = Theme.DarkTheme.FontCaption
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

            Label lblSelect = new Label
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
                Text = "🔄",
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

            Label lblSlot = new Label
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

            Label lblPlus = new Label
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

            Label lblMax = new Label
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

            Label lblDelay = new Label
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

            gbxAlchemySettings.Controls.Add(lblSelect);
            gbxAlchemySettings.Controls.Add(cmbxAlchemyItems);
            gbxAlchemySettings.Controls.Add(btnAlchemyRefreshItems);
            gbxAlchemySettings.Controls.Add(lblSlot);
            gbxAlchemySettings.Controls.Add(nudAlchemyTargetSlot);
            gbxAlchemySettings.Controls.Add(lblPlus);
            gbxAlchemySettings.Controls.Add(nudAlchemyTargetPlus);
            gbxAlchemySettings.Controls.Add(cbxAlchemyUsePowder);
            gbxAlchemySettings.Controls.Add(lblMax);
            gbxAlchemySettings.Controls.Add(nudAlchemyMaxAttempts);
            gbxAlchemySettings.Controls.Add(lblDelay);
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

            Label lblLogHead = new Label
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
            gbxAlchemyActions.Controls.Add(lblLogHead);
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
