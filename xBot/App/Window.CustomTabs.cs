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

            GroupBox gbxStrategy = new GroupBox();
            gbxStrategy.Text = "Giriş Akışı & Stratejisi (Login Flow & Strategy)";
            gbxStrategy.ForeColor = Color.FromArgb(0, 122, 204);
            gbxStrategy.Location = new Point(235, 31);
            gbxStrategy.Size = new Size(410, 183);
            gbxStrategy.Font = new Font("Segoe UI", 8.5F);

            CheckBox cbxAutoLogin = new CheckBox { Text = "Otomatik Giriş (Automated Login)", Location = new Point(12, 22), AutoSize = true, Checked = LoginStrategyManager.AutomatedLogin, ForeColor = Color.White };
            cbxAutoLogin.CheckedChanged += (s, e) => LoginStrategyManager.AutomatedLogin = cbxAutoLogin.Checked;

            CheckBox cbxStaticCaptcha = new CheckBox { Text = "Sabit Captcha", Location = new Point(12, 45), AutoSize = true, Checked = LoginStrategyManager.StaticCaptcha, ForeColor = Color.White };
            cbxStaticCaptcha.CheckedChanged += (s, e) => LoginStrategyManager.StaticCaptcha = cbxStaticCaptcha.Checked;

            CheckBox cbxAutoStart = new CheckBox { Text = "Oyuna Girince Botu Başlat", Location = new Point(12, 68), AutoSize = true, Checked = LoginStrategyManager.AutoStartBot, ForeColor = Color.White };
            cbxAutoStart.CheckedChanged += (s, e) => LoginStrategyManager.AutoStartBot = cbxAutoStart.Checked;

            CheckBox cbxAutoHide = new CheckBox { Text = "İstemciyi Otomatik Gizle", Location = new Point(12, 91), AutoSize = true, Checked = LoginStrategyManager.AutoHideClient, ForeColor = Color.White };
            cbxAutoHide.CheckedChanged += (s, e) => LoginStrategyManager.AutoHideClient = cbxAutoHide.Checked;

            CheckBox cbxStayConnected = new CheckBox { Text = "Çökme Koruması (Failover to Clientless)", Location = new Point(12, 114), AutoSize = true, Checked = LoginStrategyManager.StayConnected, ForeColor = Color.LightSkyBlue };
            cbxStayConnected.CheckedChanged += (s, e) => LoginStrategyManager.StayConnected = cbxStayConnected.Checked;

            RadioButton rbnFirstFound = new RadioButton { Text = "İlk Karakter", Location = new Point(12, 142), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.FirstFound), ForeColor = Color.LightGray };
            RadioButton rbnHighestLevel = new RadioButton { Text = "En Yüksek Seviyeli Karakter", Location = new Point(140, 142), AutoSize = true, Checked = (LoginStrategyManager.Strategy == CharacterSelectionStrategy.HighestLevel), ForeColor = Color.Gold };

            rbnFirstFound.CheckedChanged += (s, e) => { if (rbnFirstFound.Checked) LoginStrategyManager.Strategy = CharacterSelectionStrategy.FirstFound; };
            rbnHighestLevel.CheckedChanged += (s, e) => { if (rbnHighestLevel.Checked) LoginStrategyManager.Strategy = CharacterSelectionStrategy.HighestLevel; };

            gbxStrategy.Controls.AddRange(new Control[] { cbxAutoLogin, cbxStaticCaptcha, cbxAutoStart, cbxAutoHide, cbxStayConnected, rbnFirstFound, rbnHighestLevel });
            TabPageV_Control01_Login_Panel.Controls.Add(gbxStrategy);
        }

        private void BuildCombatTabWidgets()
        {
            if (this.TabPageH_Town_Option02_Panel == null) return;

            // Add custom avoidance / zerk controls to Combat panel
            if (Combat_gbxAI != null)
            {
                CheckBox cbxZerkFullHP = new CheckBox { Text = "HP %100 Olduğunda Berserk Bas", Location = new Point(15, 145), AutoSize = true, Checked = CombatAIEngine.ZerkWhenHPFull, ForeColor = Color.White };
                cbxZerkFullHP.CheckedChanged += (s, e) => CombatAIEngine.ZerkWhenHPFull = cbxZerkFullHP.Checked;

                CheckBox cbxDimension = new CheckBox { Text = "Dimension Pillar Sütunlarını Yok Say", Location = new Point(15, 175), AutoSize = true, Checked = CombatAIEngine.IgnoreDimensionPillars, ForeColor = Color.White };
                cbxDimension.CheckedChanged += (s, e) => CombatAIEngine.IgnoreDimensionPillars = cbxDimension.Checked;

                CheckBox cbxWeakerFirst = new CheckBox { Text = "Önce Zayıf Moblara Saldır (Weaker First)", Location = new Point(15, 205), AutoSize = true, Checked = CombatAIEngine.AttackWeakerFirst, ForeColor = Color.White };
                cbxWeakerFirst.CheckedChanged += (s, e) => CombatAIEngine.AttackWeakerFirst = cbxWeakerFirst.Checked;

                CheckBox cbxDoNotFollow = new CheckBox { Text = "Alanın Dışındaki Mobu Takip Etme", Location = new Point(15, 235), AutoSize = true, Checked = CombatAIEngine.DoNotFollowMobs, ForeColor = Color.White };
                cbxDoNotFollow.CheckedChanged += (s, e) => CombatAIEngine.DoNotFollowMobs = cbxDoNotFollow.Checked;

                Combat_gbxAI.Controls.AddRange(new Control[] { cbxZerkFullHP, cbxDimension, cbxWeakerFirst, cbxDoNotFollow });
            }
        }

        private void BuildSkillsTabWidgets()
        {
            if (this.TabPageV_Control01_Skills_Panel == null) return;

            // Move Up and Down buttons
            Button btnSkillUp = new Button { Text = "▲", Size = new Size(30, 26), Location = new Point(400, 310), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(0, 122, 204) };
            btnSkillUp.Click += (s, e) => SkillManager.MoveSelectedItemUp(Skills_lstvSkills);

            Button btnSkillDown = new Button { Text = "▼", Size = new Size(30, 26), Location = new Point(435, 310), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Color.FromArgb(0, 122, 204) };
            btnSkillDown.Click += (s, e) => SkillManager.MoveSelectedItemDown(Skills_lstvSkills);

            // Imbue Selector
            Label lblImbue = new Label { Text = "Imbue (El Yakma):", Location = new Point(475, 315), AutoSize = true, ForeColor = Color.LightGray };
            ComboBox cmbxImbue = new ComboBox { Location = new Point(590, 312), Size = new Size(80, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbxImbue.Items.AddRange(new object[] { "None", "Fire", "Cold", "Lightning" });
            cmbxImbue.SelectedItem = SkillManager.SelectedImbue;
            cmbxImbue.SelectedIndexChanged += (s, e) => SkillManager.SelectedImbue = cmbxImbue.SelectedItem.ToString();

            // No attack checkbox
            CheckBox cbxNoAttack = new CheckBox { Text = "Saldırı Yapma (No Attack / Support)", Location = new Point(400, 345), AutoSize = true, Checked = SkillManager.NoAttackMode, ForeColor = Color.White };
            cbxNoAttack.CheckedChanged += (s, e) => SkillManager.NoAttackMode = cbxNoAttack.Checked;

            // Devil Spirit checkbox
            CheckBox cbxDevil = new CheckBox { Text = "Devil Spirit Becerisi Kullan", Location = new Point(400, 370), AutoSize = true, Checked = SkillManager.UseDevilSpirit, ForeColor = Color.White };
            cbxDevil.CheckedChanged += (s, e) => SkillManager.UseDevilSpirit = cbxDevil.Checked;

            TabPageV_Control01_Skills_Panel.Controls.AddRange(new Control[] { btnSkillUp, btnSkillDown, lblImbue, cmbxImbue, cbxNoAttack, cbxDevil });
        }

        private void BuildProtectionTabWidgets()
        {
            if (this.TabPageV_Control01_Character_Panel == null) return;

            GroupBox gbxProtExtra = new GroupBox();
            gbxProtExtra.Text = "Beceriyle İyileşme & Gelişmiş Koruma (Skill Protection)";
            gbxProtExtra.ForeColor = Color.FromArgb(0, 122, 204);
            gbxProtExtra.Location = new Point(340, 6);
            gbxProtExtra.Size = new Size(305, 335);
            gbxProtExtra.Font = new Font("Segoe UI", 8.5F);

            CheckBox cbxSkillHP = new CheckBox { Text = "HP < %50 ise Beceriyle Doldur", Location = new Point(12, 22), AutoSize = true, Checked = ProtectionManager.UseSkillHP, ForeColor = Color.White };
            cbxSkillHP.CheckedChanged += (s, e) => ProtectionManager.UseSkillHP = cbxSkillHP.Checked;

            CheckBox cbxSkillMP = new CheckBox { Text = "MP < %40 ise Beceriyle Doldur", Location = new Point(12, 48), AutoSize = true, Checked = ProtectionManager.UseSkillMP, ForeColor = Color.White };
            cbxSkillMP.CheckedChanged += (s, e) => ProtectionManager.UseSkillMP = cbxSkillMP.Checked;

            CheckBox cbxSkillCure = new CheckBox { Text = "Kötü Durumu Beceriyle Tedavi Et", Location = new Point(12, 74), AutoSize = true, Checked = ProtectionManager.UseSkillBadStatus, ForeColor = Color.White };
            cbxSkillCure.CheckedChanged += (s, e) => ProtectionManager.UseSkillBadStatus = cbxSkillCure.Checked;

            CheckBox cbxPetRevive = new CheckBox { Text = "Ölen Peti Dirilt (Grass of Life)", Location = new Point(12, 100), AutoSize = true, Checked = ProtectionManager.RevivePet, ForeColor = Color.White };
            cbxPetRevive.CheckedChanged += (s, e) => ProtectionManager.RevivePet = cbxPetRevive.Checked;

            CheckBox cbxPetSummon = new CheckBox { Text = "Peti Otomatik Geri Çağır", Location = new Point(12, 126), AutoSize = true, Checked = ProtectionManager.AutoSummonPet, ForeColor = Color.White };
            cbxPetSummon.CheckedChanged += (s, e) => ProtectionManager.AutoSummonPet = cbxPetSummon.Checked;

            CheckBox cbxNoArrows = new CheckBox { Text = "Ok / Bolt Bittiğinde Şehre Dön", Location = new Point(12, 152), AutoSize = true, Checked = ProtectionManager.ReturnNoArrows, ForeColor = Color.White };
            cbxNoArrows.CheckedChanged += (s, e) => ProtectionManager.ReturnNoArrows = cbxNoArrows.Checked;

            CheckBox cbxFullInv = new CheckBox { Text = "Çanta Dolduğunda Şehre Dön", Location = new Point(12, 178), AutoSize = true, Checked = ProtectionManager.ReturnFullInventory, ForeColor = Color.White };
            cbxFullInv.CheckedChanged += (s, e) => ProtectionManager.ReturnFullInventory = cbxFullInv.Checked;

            CheckBox cbxDurability = new CheckBox { Text = "Dayanıklılık (Durability) Azalınca Dön", Location = new Point(12, 204), AutoSize = true, Checked = ProtectionManager.ReturnDurabilityLow, ForeColor = Color.White };
            cbxDurability.CheckedChanged += (s, e) => ProtectionManager.ReturnDurabilityLow = cbxDurability.Checked;

            CheckBox cbxAutoStats = new CheckBox { Text = "Level Atlayınca Statları Otomatik Ver", Location = new Point(12, 235), AutoSize = true, Checked = StatPointManager.AutoDistributeEnabled, ForeColor = Color.Gold };
            cbxAutoStats.CheckedChanged += (s, e) => StatPointManager.AutoDistributeEnabled = cbxAutoStats.Checked;

            Label lblStr = new Label { Text = "STR:", Location = new Point(12, 265), AutoSize = true, ForeColor = Color.LightGray };
            NumericUpDown nudStr = new NumericUpDown { Location = new Point(45, 263), Size = new Size(40, 20), Maximum = 3, Minimum = 0, Value = StatPointManager.TargetSTR };
            nudStr.ValueChanged += (s, e) => StatPointManager.TargetSTR = (int)nudStr.Value;

            Label lblInt = new Label { Text = "INT:", Location = new Point(100, 265), AutoSize = true, ForeColor = Color.LightGray };
            NumericUpDown nudInt = new NumericUpDown { Location = new Point(130, 263), Size = new Size(40, 20), Maximum = 3, Minimum = 0, Value = StatPointManager.TargetINT };
            nudInt.ValueChanged += (s, e) => StatPointManager.TargetINT = (int)nudInt.Value;

            gbxProtExtra.Controls.AddRange(new Control[] {
                cbxSkillHP, cbxSkillMP, cbxSkillCure, cbxPetRevive, cbxPetSummon,
                cbxNoArrows, cbxFullInv, cbxDurability, cbxAutoStats, lblStr, nudStr, lblInt, nudInt
            });

            TabPageV_Control01_Character_Panel.Controls.Add(gbxProtExtra);
        }

        private void BuildItemFilterWidgets()
        {
            if (this.TabPageH_Town_Option03_Panel == null) return;

            GroupBox gbxFilterRules = new GroupBox();
            gbxFilterRules.Text = "Gelişmiş Eşya Filtreleme & Kural Motoru (Item Filter)";
            gbxFilterRules.ForeColor = Color.FromArgb(0, 122, 204);
            gbxFilterRules.Location = new Point(15, 170);
            gbxFilterRules.Size = new Size(625, 160);
            gbxFilterRules.Font = new Font("Segoe UI", 8.5F);

            Label lblDeg = new Label { Text = "Degree Aralığı:", Location = new Point(15, 25), AutoSize = true, ForeColor = Color.White };
            NumericUpDown nudMinDeg = new NumericUpDown { Location = new Point(110, 23), Size = new Size(40, 20), Minimum = 1, Maximum = 16, Value = ItemFilterManager.MinDegree };
            nudMinDeg.ValueChanged += (s, e) => ItemFilterManager.MinDegree = (int)nudMinDeg.Value;

            Label lblTo = new Label { Text = "~", Location = new Point(155, 25), AutoSize = true, ForeColor = Color.White };
            NumericUpDown nudMaxDeg = new NumericUpDown { Location = new Point(175, 23), Size = new Size(40, 20), Minimum = 1, Maximum = 16, Value = ItemFilterManager.MaxDegree };
            nudMaxDeg.ValueChanged += (s, e) => ItemFilterManager.MaxDegree = (int)nudMaxDeg.Value;

            CheckBox cbxSox = new CheckBox { Text = "Sadece SoX (SOS/SOM/SUN) Eşyaları", Location = new Point(235, 24), AutoSize = true, Checked = ItemFilterManager.OnlySox, ForeColor = Color.Gold };
            cbxSox.CheckedChanged += (s, e) => ItemFilterManager.OnlySox = cbxSox.Checked;

            CheckBox cbxChina = new CheckBox { Text = "China", Location = new Point(15, 55), AutoSize = true, Checked = ItemFilterManager.FilterChina, ForeColor = Color.White };
            cbxChina.CheckedChanged += (s, e) => ItemFilterManager.FilterChina = cbxChina.Checked;

            CheckBox cbxEu = new CheckBox { Text = "Europe", Location = new Point(80, 55), AutoSize = true, Checked = ItemFilterManager.FilterEurope, ForeColor = Color.White };
            cbxEu.CheckedChanged += (s, e) => ItemFilterManager.FilterEurope = cbxEu.Checked;

            CheckBox cbxMale = new CheckBox { Text = "Male", Location = new Point(155, 55), AutoSize = true, Checked = ItemFilterManager.FilterMale, ForeColor = Color.White };
            cbxMale.CheckedChanged += (s, e) => ItemFilterManager.FilterMale = cbxMale.Checked;

            CheckBox cbxFemale = new CheckBox { Text = "Female", Location = new Point(215, 55), AutoSize = true, Checked = ItemFilterManager.FilterFemale, ForeColor = Color.White };
            cbxFemale.CheckedChanged += (s, e) => ItemFilterManager.FilterFemale = cbxFemale.Checked;

            Label lblInfo = new Label
            {
                Text = "Filtrelenen eşyalar yerdeki droplardan otomatik toplanır, şehir döngüsünde ayara göre depoya yatırılır veya NPC'ye satılır.",
                Location = new Point(15, 88),
                Size = new Size(590, 30),
                ForeColor = Color.LightGray
            };

            gbxFilterRules.Controls.AddRange(new Control[] { lblDeg, nudMinDeg, lblTo, nudMaxDeg, cbxSox, cbxChina, cbxEu, cbxMale, cbxFemale, lblInfo });
            TabPageH_Town_Option03_Panel.Controls.Add(gbxFilterRules);
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
            }
            catch { }
        }

        public void ApplyLanguageToWindow()
        {
            try
            {
                this.InvokeIfRequired(() =>
                {
                    if (this.TabPageV_Control01_Login != null) this.TabPageV_Control01_Login.Text = LocalizationManager.Get("UI_General", "General");
                    if (this.TabPageV_Control01_Training != null) this.TabPageV_Control01_Training.Text = LocalizationManager.Get("UI_Training", "Training");
                    if (this.TabPageV_Control01_Skills != null) this.TabPageV_Control01_Skills.Text = LocalizationManager.Get("UI_Skills", "Skills");
                    if (this.TabPageV_Control01_Character != null) this.TabPageV_Control01_Character.Text = LocalizationManager.Get("UI_Protection", "Protection");
                    if (this.TabPageV_Control01_Party != null) this.TabPageV_Control01_Party.Text = LocalizationManager.Get("UI_Party", "Party");
                    if (this.TabPageV_Control01_Inventory != null) this.TabPageV_Control01_Inventory.Text = LocalizationManager.Get("UI_Inventory", "Inventory");
                    if (this.TabPageV_Control01_Town != null) this.TabPageV_Control01_Town.Text = LocalizationManager.Get("UI_Items", "Town & Items");
                    if (this.TabPageV_Control01_Chat != null) this.TabPageV_Control01_Chat.Text = LocalizationManager.Get("UI_Chat", "Chat");

                    if (this.lblBotState != null && this.lblBotState.Text.Contains("Ready"))
                        this.lblBotState.Text = LocalizationManager.Get("UI_WaitingForChar", "Ready");
                });
            }
            catch { }
        }
    }
}
