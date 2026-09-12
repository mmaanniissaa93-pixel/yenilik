using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace xBot.App
{
    /// <summary>
    /// phBot birebir iç düzenler 5. dalga: Login + Training metinleri.
    /// Referans: guide/initial-startup_05.png (Login 1..21) + initial-startup.md.
    /// </summary>
    public partial class Window
    {
        private void LayoutPhBotInners5()
        {
            try
            {
                HookInner(TabPageV_Control01_Login_Panel, LayoutLoginInner);
                HookInner(TabPageV_Control01_Minimap_Panel, LayoutMapInner);
                HookInner(TabPageV_Control01_GameInfo_Panel, LayoutGameInfoInner);
                HookInner(TabPageV_Control01_Alchemy_Panel, LayoutAlchemyInner);
                HookInner(TabPageV_Control01_Trade_Panel, LayoutTradeInner);
                LayoutLoginInner();
                LayoutAlchemyInner();
                LayoutTrainingAreaTexts();
                LayoutMapInner();
                LayoutGameInfoInner();
                LayoutTradeInner();
            }
            catch (Exception ex) { PhBotDebug("inners5: " + ex.Message); }
        }

        private static void LoginRow(GroupBox login, Label lbl, string text, Control input, int x, int y, int w)
        {
            try
            {
                if (lbl != null)
                {
                    if (login != null && lbl.Parent != login)
                    {
                        try { lbl.Parent?.Controls.Remove(lbl); } catch { }
                        try { login.Controls.Add(lbl); } catch { }
                    }
                    lbl.Text = text;
                    lbl.Font = PhBotFont();
                    lbl.ForeColor = Color.Black;
                    lbl.AutoSize = true;
                    lbl.Location = new Point(x, y + 4);
                    lbl.Visible = true;
                    lbl.BringToFront();
                    try { lbl.Anchor = AnchorStyles.Top | AnchorStyles.Left; } catch { }
                }
                if (input != null)
                {
                    if (login != null && input.Parent != login)
                    {
                        try { input.Parent?.Controls.Remove(input); } catch { }
                        try { login.Controls.Add(input); } catch { }
                    }
                    input.Font = PhBotFont();
                    input.BackColor = Color.White;
                    input.ForeColor = Color.Black;
                    input.Location = new Point(x + 105, y);
                    input.Size = new Size(w, 22);
                    input.Visible = true;
                    input.BringToFront();
                    try { input.Anchor = AnchorStyles.Top | AnchorStyles.Left; } catch { }
                }
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // LOGIN / SILKROAD BAĞLANTISI (phBot v33.6.3 1:1)
        // Sub-tabs: Bağlan | Ayarlar | Credentials
        // ---------------------------------------------------------------
        private void LayoutLoginInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Login_Panel;
                if (host == null) return;
                int W = host.Width, H = host.Height;
                if (W < 200 || H < 100) return;
                try { host.AutoScroll = false; } catch { }

                bool isTR = LocalizationManager.CurrentLanguage == "TR";

                // Eski grupları gizle
                try { if (Login_gbxAdvertising != null) Login_gbxAdvertising.Visible = false; } catch { }
                try { if (gbxStrategy != null) gbxStrategy.Visible = false; } catch { }
                try { if (Login_gbxConnection != null) Login_gbxConnection.Visible = false; } catch { }
                try { if (Login_gbxLogin != null) Login_gbxLogin.Visible = false; } catch { }
                try { if (Login_gbxServers != null) Login_gbxServers.Visible = false; } catch { }
                try { if (Login_gbxCharacters != null) Login_gbxCharacters.Visible = false; } catch { }

                // TabControl'ü al veya oluştur
                TabControl tabLoginRoot = host.Controls["tabLoginRoot"] as TabControl;
                if (tabLoginRoot == null)
                {
                    tabLoginRoot = new TabControl
                    {
                        Name = "tabLoginRoot",
                        Dock = DockStyle.Fill,
                        Font = PhBotFont(),
                        Padding = new Point(8, 3)
                    };
                    host.Controls.Add(tabLoginRoot);
                }
                tabLoginRoot.Visible = true;
                tabLoginRoot.BringToFront();

                // Host üzerindeki diğer tüm kontrolleri gizle (sadece tabLoginRoot açık kalsın)
                foreach (Control c in host.Controls)
                {
                    if (c != tabLoginRoot)
                    {
                        try { c.Visible = false; } catch { }
                    }
                }

                // 3 Alt Sekme (Bağlan, Ayarlar, Credentials)
                TabPage pageConnect = null;
                TabPage pageSettings = null;
                TabPage pageCredentials = null;

                foreach (TabPage p in tabLoginRoot.TabPages)
                {
                    if (p.Name == "pageConnect") pageConnect = p;
                    else if (p.Name == "pageSettings") pageSettings = p;
                    else if (p.Name == "pageCredentials") pageCredentials = p;
                }

                if (pageConnect == null)
                {
                    pageConnect = new TabPage { Name = "pageConnect", BackColor = Color.White, AutoScroll = true };
                    tabLoginRoot.TabPages.Add(pageConnect);
                }
                if (pageSettings == null)
                {
                    pageSettings = new TabPage { Name = "pageSettings", BackColor = Color.White, AutoScroll = true };
                    tabLoginRoot.TabPages.Add(pageSettings);
                }
                if (pageCredentials == null)
                {
                    pageCredentials = new TabPage { Name = "pageCredentials", BackColor = Color.White, AutoScroll = true };
                    tabLoginRoot.TabPages.Add(pageCredentials);
                }

                pageConnect.Text = isTR ? "Bağlan" : "Connect";
                pageSettings.Text = isTR ? "Ayarlar" : "Settings";
                pageCredentials.Text = "Credentials";

                // =======================================================
                // TAB 1: BAĞLAN (CONNECT)
                // =======================================================
                int lx = 14;
                int ix = 150;
                int iw = 165;
                int y = 14;
                const int rStep = 28;

                // 1. Saved login
                PhBotLabel(pageConnect, "lblSavedLogin", "Saved login", lx, y + 3);
                ComboBox cbSavedLogin = pageConnect.Controls["PhBot_LoginSavedLogins"] as ComboBox;
                if (cbSavedLogin == null)
                {
                    cbSavedLogin = new ComboBox { Name = "PhBot_LoginSavedLogins", DropDownStyle = ComboBoxStyle.DropDownList };
                    cbSavedLogin.SelectedIndexChanged += OnPhBotSavedLoginSelected;
                    pageConnect.Controls.Add(cbSavedLogin);
                }
                cbSavedLogin.Font = PhBotFont();
                cbSavedLogin.BackColor = Color.White;
                cbSavedLogin.ForeColor = Color.Black;
                cbSavedLogin.SetBounds(ix, y, iw, 22);
                cbSavedLogin.Visible = true;
                y += rStep;

                // 2. Kullanıcı Adı
                PhBotLabel(pageConnect, "lblLoginUser", isTR ? "Kullanıcı Adı" : "Username", lx, y + 3);
                if (Login_tbxUsername != null)
                {
                    if (Login_tbxUsername.Parent != pageConnect)
                    {
                        try { Login_tbxUsername.Parent?.Controls.Remove(Login_tbxUsername); } catch { }
                        pageConnect.Controls.Add(Login_tbxUsername);
                    }
                    Login_tbxUsername.Font = PhBotFont();
                    Login_tbxUsername.BackColor = Color.White;
                    Login_tbxUsername.ForeColor = Color.Black;
                    Login_tbxUsername.SetBounds(ix, y, iw, 22);
                    Login_tbxUsername.Visible = true;
                }
                y += rStep;

                // 3. Şifre
                PhBotLabel(pageConnect, "lblLoginPass", isTR ? "Şifre" : "Password", lx, y + 3);
                if (Login_tbxPassword != null)
                {
                    if (Login_tbxPassword.Parent != pageConnect)
                    {
                        try { Login_tbxPassword.Parent?.Controls.Remove(Login_tbxPassword); } catch { }
                        pageConnect.Controls.Add(Login_tbxPassword);
                    }
                    Login_tbxPassword.Font = PhBotFont();
                    Login_tbxPassword.BackColor = Color.White;
                    Login_tbxPassword.ForeColor = Color.Black;
                    Login_tbxPassword.UseSystemPasswordChar = true;
                    Login_tbxPassword.SetBounds(ix, y, iw, 22);
                    Login_tbxPassword.Visible = true;
                }
                y += rStep;

                // 4. Sunucu + Bağlan + Çıkış yap
                PhBotLabel(pageConnect, "lblLoginServer", isTR ? "Sunucu" : "Server", lx, y + 3);
                if (Login_cmbxServer != null)
                {
                    if (Login_cmbxServer.Parent != pageConnect)
                    {
                        try { Login_cmbxServer.Parent?.Controls.Remove(Login_cmbxServer); } catch { }
                        pageConnect.Controls.Add(Login_cmbxServer);
                    }
                    Login_cmbxServer.Font = PhBotFont();
                    Login_cmbxServer.BackColor = Color.White;
                    Login_cmbxServer.ForeColor = Color.Black;
                    Login_cmbxServer.DropDownStyle = ComboBoxStyle.DropDownList;
                    Login_cmbxServer.SetBounds(ix, y, 115, 22);
                    Login_cmbxServer.Visible = true;
                }

                if (Login_btnStart != null)
                {
                    if (Login_btnStart.Parent != pageConnect)
                    {
                        try { Login_btnStart.Parent?.Controls.Remove(Login_btnStart); } catch { }
                        pageConnect.Controls.Add(Login_btnStart);
                    }
                    Login_btnStart.Font = PhBotFont();
                    Login_btnStart.FlatStyle = FlatStyle.Flat;
                    Login_btnStart.BackColor = Color.FromArgb(246, 247, 248);
                    Login_btnStart.ForeColor = Color.Black;
                    Login_btnStart.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                    Login_btnStart.SetBounds(ix + 120, y - 1, 60, 24);
                    bool connectionIdle = true;
                    try { connectionIdle = Bot.Get.Proxy == null || !Bot.Get.Proxy.isRunning; } catch { }
                    if (connectionIdle)
                    {
                        Login_btnStart.Text = isTR ? "Bağlan" : "Connect";
                    }
                    Login_btnStart.Visible = true;
                }

                Button btnLogout = pageConnect.Controls["btnPhBotLogout"] as Button;
                if (btnLogout == null)
                {
                    btnLogout = new Button { Name = "btnPhBotLogout", Enabled = false, FlatStyle = FlatStyle.Flat };
                    btnLogout.Click += (s, e) => {
                        try {
                            if (Bot.Get.Proxy != null && Bot.Get.Proxy.isRunning && Login_btnStart != null)
                                Control_Click(Login_btnStart, EventArgs.Empty);
                        } catch { }
                    };
                    pageConnect.Controls.Add(btnLogout);
                }
                btnLogout.Font = PhBotFont();
                btnLogout.BackColor = Color.FromArgb(246, 247, 248);
                btnLogout.ForeColor = Color.Black;
                btnLogout.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnLogout.SetBounds(ix + 185, y - 1, 68, 24);
                btnLogout.Text = isTR ? "Çıkış yap" : "Logout";
                btnLogout.Visible = true;
                y += rStep;

                // 5. Gateway
                PhBotLabel(pageConnect, "lblLoginGw", "Gateway", lx, y + 3);
                ComboBox cbGw = pageConnect.Controls["PhBot_LoginGw"] as ComboBox;
                if (cbGw == null)
                {
                    cbGw = new ComboBox { Name = "PhBot_LoginGw", DropDownStyle = ComboBoxStyle.DropDownList };
                    cbGw.Items.AddRange(new object[] { "94.199.103.68", "127.0.0.1" });
                    if (cbGw.Items.Count > 0) cbGw.SelectedIndex = 0;
                    pageConnect.Controls.Add(cbGw);
                }
                cbGw.Font = PhBotFont();
                cbGw.BackColor = Color.White;
                cbGw.ForeColor = Color.Black;
                cbGw.SetBounds(ix, y, iw, 22);
                cbGw.Visible = true;
                y += rStep;

                // 6. Şu kadar denemeden sonra [ 0 ] gateway değiştir
                PhBotLabel(pageConnect, "lblSwGw1", isTR ? "Şu kadar denemeden sonra" : "Switch gateway after", lx, y + 3);
                TextBox txtSw = PhBotTodoNumber(pageConnect, "PhBot_LoginSw", 152, y, 42, "0");
                txtSw.TextAlign = HorizontalAlignment.Center;
                PhBotLabel(pageConnect, "lblSwGw2", isTR ? "gateway değiştir" : "attempts", 200, y + 3);
                y += rStep;

                // 7. SOCKS IP/Port + 4 + 5
                PhBotLabel(pageConnect, "lblSocksIp", "SOCKS IP/Port", lx, y + 3);
                TextBox tSocksIp = PhBotTodoNumber(pageConnect, "PhBot_SocksIp", ix, y, 98, "");
                tSocksIp.TextAlign = HorizontalAlignment.Left;
                TextBox tSocksPort = PhBotTodoNumber(pageConnect, "PhBot_SocksPort", ix + 104, y, 46, "0");
                tSocksPort.TextAlign = HorizontalAlignment.Center;

                RadioButton rb4 = pageConnect.Controls["PhBot_Socks4"] as RadioButton;
                if (rb4 == null)
                {
                    rb4 = new RadioButton { Name = "PhBot_Socks4", Text = "4", AutoSize = true, Font = PhBotFont() };
                    pageConnect.Controls.Add(rb4);
                }
                rb4.SetBounds(ix + 156, y + 2, 38, 20);
                rb4.Visible = true;

                RadioButton rb5 = pageConnect.Controls["PhBot_Socks5"] as RadioButton;
                if (rb5 == null)
                {
                    rb5 = new RadioButton { Name = "PhBot_Socks5", Text = "5", AutoSize = true, Checked = true, Font = PhBotFont() };
                    pageConnect.Controls.Add(rb5);
                }
                rb5.SetBounds(ix + 198, y + 2, 38, 20);
                rb5.Visible = true;
                y += rStep;

                // 8. SOCKS Kullanıcı
                PhBotLabel(pageConnect, "lblSocksUser", isTR ? "SOCKS Kullanıcı" : "SOCKS User", lx, y + 3);
                TextBox tSocksUser = PhBotTodoNumber(pageConnect, "PhBot_SocksUser", ix, y, iw, "");
                tSocksUser.TextAlign = HorizontalAlignment.Left;
                y += rStep;

                // 9. SOCKS Şifre + Sıfırla
                PhBotLabel(pageConnect, "lblSocksPass", isTR ? "SOCKS Şifre" : "SOCKS Pass", lx, y + 3);
                TextBox tSocksPass = PhBotTodoNumber(pageConnect, "PhBot_SocksPass", ix, y, 105, "");
                tSocksPass.UseSystemPasswordChar = true;
                tSocksPass.TextAlign = HorizontalAlignment.Left;

                Button btnReset = pageConnect.Controls["btnSocksReset"] as Button;
                if (btnReset == null)
                {
                    btnReset = new Button { Name = "btnSocksReset", FlatStyle = FlatStyle.Flat };
                    btnReset.Click += (s, e) => {
                        try {
                            tSocksIp.Text = "";
                            tSocksPort.Text = "0";
                            tSocksUser.Text = "";
                            tSocksPass.Text = "";
                        } catch { }
                    };
                    pageConnect.Controls.Add(btnReset);
                }
                btnReset.Font = PhBotFont();
                btnReset.BackColor = Color.FromArgb(246, 247, 248);
                btnReset.ForeColor = Color.Black;
                btnReset.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnReset.SetBounds(ix + 112, y - 1, 60, 24);
                btnReset.Text = isTR ? "Sıfırla" : "Reset";
                btnReset.Visible = true;

                // -------------------------------------------------------
                // Right Side: Server List + Checkboxes
                // -------------------------------------------------------
                int rightX = 420;
                int rightW = Math.Max(300, W - rightX - 24);

                if (Login_lstvServers != null)
                {
                    if (Login_lstvServers.Parent != pageConnect)
                    {
                        try { Login_lstvServers.Parent?.Controls.Remove(Login_lstvServers); } catch { }
                        pageConnect.Controls.Add(Login_lstvServers);
                    }
                    Login_lstvServers.Location = new Point(rightX, 14);
                    Login_lstvServers.Size = new Size(rightW, 105);
                    Login_lstvServers.BackColor = Color.White;
                    Login_lstvServers.ForeColor = Color.Black;
                    Login_lstvServers.BorderStyle = BorderStyle.FixedSingle;
                    Login_lstvServers.Font = PhBotFont();
                    Login_lstvServers.View = View.Details;
                    Login_lstvServers.FullRowSelect = true;
                    Login_lstvServers.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                    Login_lstvServers.Visible = true;

                    if (Login_lstvServers.Columns.Count >= 3)
                    {
                        Login_lstvServers.Columns[0].Text = isTR ? "Sunucu" : "Server";
                        Login_lstvServers.Columns[1].Text = isTR ? "Durum" : "State";
                        Login_lstvServers.Columns[2].Text = isTR ? "Kapasite" : "Capacity";
                        int usable = Login_lstvServers.ClientSize.Width - 4;
                        Login_lstvServers.Columns[0].Width = (int)(usable * 0.40);
                        Login_lstvServers.Columns[1].Width = (int)(usable * 0.30);
                        Login_lstvServers.Columns[2].Width = Math.Max(60, usable - Login_lstvServers.Columns[0].Width - Login_lstvServers.Columns[1].Width);
                    }
                }

                // Checkboxes below server list
                int col1X = rightX;
                int col2X = rightX + 220;
                int cy = 126;
                const int cStep = 22;

                // Col 1:
                MoveToLogin(pageConnect, Login_cbxUseReturnScroll, col1X, cy);
                SetCheckText(Login_cbxUseReturnScroll, isTR ? "Giriş Yapınca Şehre Dön" : "Return to town on login");
                cy += cStep;

                MoveToLogin(pageConnect, cbxGeneralAutoLogin, col1X, cy);
                SetCheckText(cbxGeneralAutoLogin, isTR ? "Otomatik Giriş Yap" : "Auto login");
                cy += cStep;

                MoveToLogin(pageConnect, cbxGeneralAutoStart, col1X, cy);
                SetCheckText(cbxGeneralAutoStart, isTR ? "Giriş Yapınca Botu Başlat" : "Start bot on login");
                cy += cStep;

                MoveToLogin(pageConnect, Login_cbxRelogin, col1X, cy);
                SetCheckText(Login_cbxRelogin, isTR ? "Tekrar Bağlan" : "Relog");
                if (nudGeneralLoginDelay != null)
                {
                    if (nudGeneralLoginDelay.Parent != pageConnect)
                    {
                        try { nudGeneralLoginDelay.Parent?.Controls.Remove(nudGeneralLoginDelay); } catch { }
                        pageConnect.Controls.Add(nudGeneralLoginDelay);
                    }
                    nudGeneralLoginDelay.Font = PhBotFont();
                    nudGeneralLoginDelay.BackColor = Color.White;
                    nudGeneralLoginDelay.ForeColor = Color.Black;
                    nudGeneralLoginDelay.SetBounds(col1X + (isTR ? 104 : 64), cy, 42, 22);
                    nudGeneralLoginDelay.Visible = true;
                }
                PhBotLabel(pageConnect, "lblRelogSec", "s", col1X + (isTR ? 150 : 110), cy + 2);
                cy += cStep;

                PhBotTodoCheck(pageConnect, "PhBot_LoginCheck", isTR ? "Giriş kontrolü" : "Login check", col1X, cy, false);
                cy += cStep;

                PhBotTodoCheck(pageConnect, "PhBot_NoClientless", isTR ? "Clientless yok" : "No clientless", col1X, cy, false);
                cy += cStep + 2;

                PhBotLabel(pageConnect, "lblWaitDelay", isTR ? "DC sonrası bekle" : "Wait after DC", col1X, cy + 2);
                if (nudGeneralWaitAfterDC != null)
                {
                    if (nudGeneralWaitAfterDC.Parent != pageConnect)
                    {
                        try { nudGeneralWaitAfterDC.Parent?.Controls.Remove(nudGeneralWaitAfterDC); } catch { }
                        pageConnect.Controls.Add(nudGeneralWaitAfterDC);
                    }
                    nudGeneralWaitAfterDC.Font = PhBotFont();
                    nudGeneralWaitAfterDC.BackColor = Color.White;
                    nudGeneralWaitAfterDC.ForeColor = Color.Black;
                    nudGeneralWaitAfterDC.SetBounds(col1X + (isTR ? 112 : 88), cy, 42, 22);
                    nudGeneralWaitAfterDC.Visible = true;
                }
                PhBotLabel(pageConnect, "lblWaitMin", isTR ? "dk" : "min", col1X + (isTR ? 158 : 134), cy + 2);
                cy += cStep + 2;

                PhBotLabel(pageConnect, "lblBlockAfter", isTR ? "Sonra engelle" : "Block after", col1X, cy + 2);
                PhBotTodoNumber(pageConnect, "PhBot_BlockAfter", col1X + (isTR ? 84 : 74), cy, 44, "200").TextAlign = HorizontalAlignment.Center;
                PhBotLabel(pageConnect, "lblBlockQueue", isTR ? "Sıra denemesi" : "Queue attempts", col1X + (isTR ? 132 : 122), cy + 2);

                // Col 2:
                int cy2 = 126;
                PhBotTodoCheck(pageConnect, "PhBot_ClientMode", "Client", col2X, cy2, true);
                cy2 += cStep;
                PhBotTodoCheck(pageConnect, "PhBot_HideLogin", isTR ? "Bilgileri gizle" : "Hide login info", col2X, cy2, false);
                cy2 += cStep;
                PhBotTodoCheck(pageConnect, "PhBot_AllowXTrap", isTR ? "X-Tap'a İzin Ver" : "Allow X-Trap", col2X, cy2, false);
                cy2 += cStep;
                PhBotTodoCheck(pageConnect, "PhBot_InstantAccess", isTR ? "Anında erişim" : "Instant access", col2X, cy2, false);
                BindVisibleLoginOptions(pageConnect);

                // =======================================================
                // TAB 2: AYARLAR (SETTINGS)
                // =======================================================
                int ay = 92;
                const int aStep = 26;

                PhBotLabel(pageSettings, "lblCharSel", isTR ? "Karakter seçimi gecikmesi" : "Character select delay", 14, ay + 3);
                PhBotTodoNumber(pageSettings, "PhBot_CharSelectDelay", 170, ay, 40, "5").TextAlign = HorizontalAlignment.Center;
                PhBotLabel(pageSettings, "lblCharSelSec", "s", 216, ay + 3);
                ay += aStep + 4;

                PhBotTodoCheck(pageSettings, "PhBot_ReduceMemSilk", isTR ? "Silkroad bellek kullanımını azalt" : "Reduce Silkroad memory usage", 14, ay, false);
                ay += aStep;

                PhBotTodoCheck(pageSettings, "PhBot_LowLatency", isTR ? "Düşük gecikme" : "Low latency", 14, ay, false);
                ay += aStep;

                PhBotTodoCheck(pageSettings, "PhBot_CaptchaCheck", "Captcha", 14, ay, true);
                if (Login_tbxCaptcha != null)
                {
                    if (Login_tbxCaptcha.Parent != pageSettings)
                    {
                        try { Login_tbxCaptcha.Parent?.Controls.Remove(Login_tbxCaptcha); } catch { }
                        pageSettings.Controls.Add(Login_tbxCaptcha);
                    }
                    Login_tbxCaptcha.Font = PhBotFont();
                    Login_tbxCaptcha.BackColor = Color.White;
                    Login_tbxCaptcha.ForeColor = Color.Black;
                    Login_tbxCaptcha.TextAlign = HorizontalAlignment.Center;
                    Login_tbxCaptcha.UseSystemPasswordChar = false;
                    Login_tbxCaptcha.PasswordChar = '\0';
                    if (string.IsNullOrWhiteSpace(Login_tbxCaptcha.Text)) Login_tbxCaptcha.Text = "1";
                    Login_tbxCaptcha.SetBounds(95, ay - 1, 40, 22);
                    Login_tbxCaptcha.Visible = true;
                }
                else
                {
                    PhBotTodoNumber(pageSettings, "PhBot_CaptchaNum", 95, ay - 1, 40, "1").TextAlign = HorizontalAlignment.Center;
                }
                ay += aStep;

                PhBotTodoCheck(pageSettings, "PhBot_Noel", "Noel", 14, ay, false);
                ay += aStep;

                PhBotTodoCheck(pageSettings, "PhBot_Queue", isTR ? "Kuyruk" : "Queue", 14, ay, false);
                ay += aStep;

                PhBotTodoCheck(pageSettings, "PhBot_IdleDisconnect", isTR ? "Boşta kopmayı kapat" : "Disable idle disconnect", 14, ay, false);
                PhBotTodoNumber(pageSettings, "PhBot_IdleMinutes", 160, ay - 1, 40, "10").TextAlign = HorizontalAlignment.Center;
                PhBotLabel(pageSettings, "lblIdleMin", isTR ? "Dk" : "Min", 206, ay + 2);
                ay += aStep + 16;

                // Keep the selected profile and client executable reachable.
                if (Login_cmbxSilkroad != null)
                {
                    if (Login_cmbxSilkroad.Parent != pageSettings)
                    {
                        try { Login_cmbxSilkroad.Parent?.Controls.Remove(Login_cmbxSilkroad); } catch { }
                        pageSettings.Controls.Add(Login_cmbxSilkroad);
                    }
                    Login_cmbxSilkroad.Visible = true;
                }
                if (Login_btnAddSilkroad != null)
                {
                    if (Login_btnAddSilkroad.Parent != pageSettings)
                    {
                        try { Login_btnAddSilkroad.Parent?.Controls.Remove(Login_btnAddSilkroad); } catch { }
                        pageSettings.Controls.Add(Login_btnAddSilkroad);
                    }
                    Login_btnAddSilkroad.Visible = true;
                }
                BuildVisibleClientPath(pageSettings);

                // =======================================================
                // TAB 3: CREDENTIALS
                // =======================================================
                PhBotLabel(pageCredentials, "lblCredSavedTitle", "Saved logins", 14, 14);

                ListBox lbCred = pageCredentials.Controls["PhBot_CredList"] as ListBox;
                if (lbCred == null)
                {
                    lbCred = new ListBox { Name = "PhBot_CredList", BorderStyle = BorderStyle.FixedSingle };
                    lbCred.SelectedIndexChanged += OnPhBotCredListSelectionChanged;
                    pageCredentials.Controls.Add(lbCred);
                }
                lbCred.Font = PhBotFont();
                lbCred.BackColor = Color.White;
                lbCred.ForeColor = Color.Black;
                lbCred.SetBounds(14, 34, 160, 205);
                lbCred.Visible = true;

                Button btnNew = pageCredentials.Controls["btnCredNew"] as Button;
                if (btnNew == null)
                {
                    btnNew = new Button { Name = "btnCredNew", FlatStyle = FlatStyle.Flat };
                    btnNew.Click += OnPhBotCredNewClicked;
                    pageCredentials.Controls.Add(btnNew);
                }
                btnNew.Font = PhBotFont();
                btnNew.BackColor = Color.FromArgb(246, 247, 248);
                btnNew.ForeColor = Color.Black;
                btnNew.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnNew.SetBounds(14, 246, 76, 25);
                btnNew.Text = "New";
                btnNew.Visible = true;

                Button btnDel = pageCredentials.Controls["btnCredDelete"] as Button;
                if (btnDel == null)
                {
                    btnDel = new Button { Name = "btnCredDelete", FlatStyle = FlatStyle.Flat };
                    btnDel.Click += OnPhBotCredDeleteClicked;
                    pageCredentials.Controls.Add(btnDel);
                }
                btnDel.Font = PhBotFont();
                btnDel.BackColor = Color.FromArgb(246, 247, 248);
                btnDel.ForeColor = Color.Black;
                btnDel.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnDel.SetBounds(98, 246, 76, 25);
                btnDel.Text = "Delete";
                btnDel.Visible = true;

                // Right fields of Credentials
                int cry = 34;
                const int crStep = 28;

                PhBotLabel(pageCredentials, "lblCredName", "Name", 195, cry + 3);
                TextBox tCredName = PhBotTodoNumber(pageCredentials, "PhBot_CredName", 280, cry, 180, "");
                tCredName.TextAlign = HorizontalAlignment.Left;
                cry += crStep;

                PhBotLabel(pageCredentials, "lblCredUser", isTR ? "Kullanıcı Adı" : "Username", 195, cry + 3);
                TextBox tCredUser = PhBotTodoNumber(pageCredentials, "PhBot_CredUser", 280, cry, 180, "");
                tCredUser.TextAlign = HorizontalAlignment.Left;
                cry += crStep;

                PhBotLabel(pageCredentials, "lblCredPass", isTR ? "Şifre" : "Password", 195, cry + 3);
                TextBox tCredPass = PhBotTodoNumber(pageCredentials, "PhBot_CredPass", 280, cry, 180, "");
                tCredPass.UseSystemPasswordChar = true;
                tCredPass.TextAlign = HorizontalAlignment.Left;
                cry += crStep;

                PhBotLabel(pageCredentials, "lblCredPin", "Passcode", 195, cry + 3);
                TextBox tCredPin = PhBotTodoNumber(pageCredentials, "PhBot_CredPin", 280, cry, 180, "");
                tCredPin.UseSystemPasswordChar = true;
                tCredPin.TextAlign = HorizontalAlignment.Left;
                cry += crStep;

                PhBotLabel(pageCredentials, "lblCredServer", isTR ? "Sunucu" : "Server", 195, cry + 3);
                ComboBox cbCredServer = pageCredentials.Controls["PhBot_CredServer"] as ComboBox;
                if (cbCredServer == null)
                {
                    cbCredServer = new ComboBox { Name = "PhBot_CredServer", DropDownStyle = ComboBoxStyle.DropDownList };
                    pageCredentials.Controls.Add(cbCredServer);
                }
                cbCredServer.Font = PhBotFont();
                cbCredServer.BackColor = Color.White;
                cbCredServer.ForeColor = Color.Black;
                cbCredServer.SetBounds(280, cry, 180, 22);
                cbCredServer.Visible = true;
                if (Login_cmbxServer != null && Login_cmbxServer.Items.Count > 0 && cbCredServer.Items.Count == 0)
                {
                    foreach (var it in Login_cmbxServer.Items) cbCredServer.Items.Add(it);
                }
                cry += crStep;

                PhBotLabel(pageCredentials, "lblCredChar", "Character", 195, cry + 3);
                TextBox tCredChar = PhBotTodoNumber(pageCredentials, "PhBot_CredChar", 280, cry, 180, "");
                tCredChar.TextAlign = HorizontalAlignment.Left;
                cry += crStep;

                CheckBox chkJCP = PhBotTodoCheck(pageCredentials, "PhBot_CredJCP", "JCP (JC Planet)", 280, cry, false);
                cry += crStep + 6;

                Button btnSave = pageCredentials.Controls["btnCredSave"] as Button;
                if (btnSave == null)
                {
                    btnSave = new Button { Name = "btnCredSave", FlatStyle = FlatStyle.Flat };
                    btnSave.Click += OnPhBotCredSaveClicked;
                    pageCredentials.Controls.Add(btnSave);
                }
                btnSave.Font = PhBotFont();
                btnSave.BackColor = Color.FromArgb(246, 247, 248);
                btnSave.ForeColor = Color.Black;
                btnSave.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnSave.SetBounds(280, cry, 75, 25);
                btnSave.Text = "Save";
                btnSave.Visible = true;

                // UI senkronizasyonu
                RefreshPhBotCredentialsUI();
            }
            catch (Exception ex) { PhBotDebug("login inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void OnPhBotSavedLoginSelected(object sender, EventArgs e)
        {
            try
            {
                ComboBox cb = sender as ComboBox;
                if (cb == null || cb.SelectedIndex <= 0) return;
                if (cb.SelectedItem is SavedAccount acc)
                {
                    AccountManager.SelectedAccountUsername = acc.Username;
                    ApplySavedAccountToInputs(acc);
                    if (Login_tbxUsername != null) Login_tbxUsername.Text = acc.Username;
                    if (Login_tbxPassword != null) Login_tbxPassword.Text = acc.Password;
                    if (Login_cmbxServer != null && !string.IsNullOrWhiteSpace(acc.Server))
                    {
                        Login_cmbxServer.Text = acc.Server;
                        xBot.Game.InfoManager.ServerName = acc.Server;
                    }
                    if (Login_cmbxCharacter != null && !string.IsNullOrWhiteSpace(acc.Character))
                        Login_cmbxCharacter.Text = acc.Character;

                    Panel host = TabPageV_Control01_Login_Panel;
                    if (host != null)
                    {
                        Control[] fIp = host.Controls.Find("PhBot_SocksIp", true);
                        if (fIp.Length > 0 && fIp[0] is TextBox tIp) tIp.Text = acc.ProxyHost;

                        Control[] fPort = host.Controls.Find("PhBot_SocksPort", true);
                        if (fPort.Length > 0 && fPort[0] is TextBox tPort) tPort.Text = acc.ProxyPort > 0 ? acc.ProxyPort.ToString() : "0";

                        Control[] fUser = host.Controls.Find("PhBot_SocksUser", true);
                        if (fUser.Length > 0 && fUser[0] is TextBox tUser) tUser.Text = acc.ProxyUsername;

                        Control[] fPass = host.Controls.Find("PhBot_SocksPass", true);
                        if (fPass.Length > 0 && fPass[0] is TextBox tPass) tPass.Text = acc.ProxyPassword;
                    }
                }
            }
            catch { }
        }

        private void OnPhBotCredListSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ListBox lb = sender as ListBox;
                if (lb == null) return;
                if (lb.SelectedItem is SavedAccount acc)
                {
                    Panel host = TabPageV_Control01_Login_Panel;
                    if (host == null) return;

                    Control[] fName = host.Controls.Find("PhBot_CredName", true);
                    if (fName.Length > 0 && fName[0] is TextBox tName) tName.Text = acc.ProfileName;

                    Control[] fUser = host.Controls.Find("PhBot_CredUser", true);
                    if (fUser.Length > 0 && fUser[0] is TextBox tUser) tUser.Text = acc.Username;

                    Control[] fPass = host.Controls.Find("PhBot_CredPass", true);
                    if (fPass.Length > 0 && fPass[0] is TextBox tPass) tPass.Text = acc.Password;

                    Control[] fPin = host.Controls.Find("PhBot_CredPin", true);
                    if (fPin.Length > 0 && fPin[0] is TextBox tPin) tPin.Text = acc.SecondaryPasscode;

                    Control[] fServer = host.Controls.Find("PhBot_CredServer", true);
                    if (fServer.Length > 0 && fServer[0] is ComboBox cbServer && !string.IsNullOrWhiteSpace(acc.Server)) cbServer.Text = acc.Server;

                    Control[] fChar = host.Controls.Find("PhBot_CredChar", true);
                    if (fChar.Length > 0 && fChar[0] is TextBox tChar) tChar.Text = acc.Character;

                    Control[] fJCP = host.Controls.Find("PhBot_CredJCP", true);
                    if (fJCP.Length > 0 && fJCP[0] is CheckBox chkJCP) chkJCP.Checked = acc.IsJCP;
                }
            }
            catch { }
        }

        private void OnPhBotCredNewClicked(object sender, EventArgs e)
        {
            try
            {
                Panel host = TabPageV_Control01_Login_Panel;
                if (host == null) return;

                Control[] fList = host.Controls.Find("PhBot_CredList", true);
                if (fList.Length > 0 && fList[0] is ListBox lb) lb.SelectedIndex = -1;

                Control[] fName = host.Controls.Find("PhBot_CredName", true);
                if (fName.Length > 0 && fName[0] is TextBox tName) { tName.Text = ""; tName.Focus(); }

                Control[] fUser = host.Controls.Find("PhBot_CredUser", true);
                if (fUser.Length > 0 && fUser[0] is TextBox tUser) tUser.Text = "";

                Control[] fPass = host.Controls.Find("PhBot_CredPass", true);
                if (fPass.Length > 0 && fPass[0] is TextBox tPass) tPass.Text = "";

                Control[] fPin = host.Controls.Find("PhBot_CredPin", true);
                if (fPin.Length > 0 && fPin[0] is TextBox tPin) tPin.Text = "";

                Control[] fServer = host.Controls.Find("PhBot_CredServer", true);
                if (fServer.Length > 0 && fServer[0] is ComboBox cbServer) cbServer.SelectedIndex = -1;

                Control[] fChar = host.Controls.Find("PhBot_CredChar", true);
                if (fChar.Length > 0 && fChar[0] is TextBox tChar) tChar.Text = "";

                Control[] fJCP = host.Controls.Find("PhBot_CredJCP", true);
                if (fJCP.Length > 0 && fJCP[0] is CheckBox chkJCP) chkJCP.Checked = false;
            }
            catch { }
        }

        private void OnPhBotCredDeleteClicked(object sender, EventArgs e)
        {
            try
            {
                Panel host = TabPageV_Control01_Login_Panel;
                if (host == null) return;

                Control[] fList = host.Controls.Find("PhBot_CredList", true);
                if (fList.Length > 0 && fList[0] is ListBox lb && lb.SelectedItem is SavedAccount acc)
                {
                    AccountManager.DeleteAccount(acc.Username);
                    RefreshPhBotCredentialsUI();
                    OnPhBotCredNewClicked(null, null);
                }
            }
            catch { }
        }

        private void OnPhBotCredSaveClicked(object sender, EventArgs e)
        {
            try
            {
                Panel host = TabPageV_Control01_Login_Panel;
                if (host == null) return;

                Control[] fName = host.Controls.Find("PhBot_CredName", true);
                Control[] fUser = host.Controls.Find("PhBot_CredUser", true);
                Control[] fPass = host.Controls.Find("PhBot_CredPass", true);
                Control[] fPin = host.Controls.Find("PhBot_CredPin", true);
                Control[] fServer = host.Controls.Find("PhBot_CredServer", true);
                Control[] fChar = host.Controls.Find("PhBot_CredChar", true);
                Control[] fJCP = host.Controls.Find("PhBot_CredJCP", true);

                string user = (fUser.Length > 0 && fUser[0] is TextBox tu) ? tu.Text.Trim() : "";
                if (string.IsNullOrWhiteSpace(user))
                {
                    Log("[Hesap] Kaydetmek için lütfen Kullanıcı Adı girin.", Theme.LogLevel.Warning);
                    return;
                }

                string name = (fName.Length > 0 && fName[0] is TextBox tn) ? tn.Text.Trim() : "";
                string pass = (fPass.Length > 0 && fPass[0] is TextBox tp) ? tp.Text : "";
                string pin = (fPin.Length > 0 && fPin[0] is TextBox tpi) ? tpi.Text : "";
                string srv = (fServer.Length > 0 && fServer[0] is ComboBox cs) ? cs.Text.Trim() : "";
                string chr = (fChar.Length > 0 && fChar[0] is TextBox tc) ? tc.Text.Trim() : "";
                bool isJcp = (fJCP.Length > 0 && fJCP[0] is CheckBox cj) ? cj.Checked : false;

                SavedAccount acc = new SavedAccount
                {
                    ProfileName = name,
                    Username = user,
                    Password = pass,
                    SecondaryPasscode = pin,
                    Server = srv,
                    Character = chr,
                    IsJCP = isJcp,
                    Silkroad = Login_cmbxSilkroad?.Text?.Trim() ?? ""
                };

                AccountManager.SaveAccount(acc);
                RefreshPhBotCredentialsUI();
                Log($"[Hesap] '{user}' hesabı kaydedildi.");
            }
            catch { }
        }

        public void RefreshPhBotCredentialsUI()
        {
            try
            {
                Panel host = TabPageV_Control01_Login_Panel;
                if (host == null) return;

                // 1. PhBot_CredList
                Control[] fList = host.Controls.Find("PhBot_CredList", true);
                if (fList.Length > 0 && fList[0] is ListBox lb)
                {
                    lb.BeginUpdate();
                    lb.Items.Clear();
                    foreach (var a in AccountManager.Accounts) lb.Items.Add(a);
                    lb.EndUpdate();
                }

                // 2. PhBot_LoginSavedLogins
                Control[] fCombo = host.Controls.Find("PhBot_LoginSavedLogins", true);
                if (fCombo.Length > 0 && fCombo[0] is ComboBox cb)
                {
                    cb.BeginUpdate();
                    cb.Items.Clear();
                    cb.Items.Add("(None)");
                    int sel = 0;
                    for (int i = 0; i < AccountManager.Accounts.Count; i++)
                    {
                        var a = AccountManager.Accounts[i];
                        cb.Items.Add(a);
                        if (!string.IsNullOrEmpty(AccountManager.SelectedAccountUsername) &&
                            a.Username.Equals(AccountManager.SelectedAccountUsername, StringComparison.OrdinalIgnoreCase))
                        {
                            sel = i + 1;
                        }
                    }
                    if (cb.Items.Count > 0 && sel < cb.Items.Count) cb.SelectedIndex = sel;
                    cb.EndUpdate();
                }

                // 3. PhBot_CredServer sync from Login_cmbxServer
                Control[] fCredServer = host.Controls.Find("PhBot_CredServer", true);
                if (fCredServer.Length > 0 && fCredServer[0] is ComboBox cbCredServer && Login_cmbxServer != null)
                {
                    if (Login_cmbxServer.Items.Count > 0 && cbCredServer.Items.Count == 0)
                    {
                        foreach (var it in Login_cmbxServer.Items) cbCredServer.Items.Add(it);
                    }
                }
            }
            catch { }
        }

        private void MoveToLogin(Control container, Control c, int x, int y, int w = 0)
        {
            if (c == null || container == null) return;
            try
            {
                if (c.Parent != container)
                {
                    try { c.Parent?.Controls.Remove(c); } catch { }
                    try { container.Controls.Add(c); } catch { }
                }
                Classicize(c);
                if (c is CheckBox || c is RadioButton)
                {
                    (c as ButtonBase).AutoSize = true;
                }
                else if (w > 0)
                {
                    c.Size = new Size(w, c.Height);
                }
                c.Location = new Point(x, y);
                c.Visible = true;
                try { c.Anchor = AnchorStyles.Top | AnchorStyles.Left; } catch { }
            }
            catch { }
        }

        private void LayoutGameInfoInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageV_Control01_GameInfo_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                if (GameInfo_tvwObjects != null)
                {
                    GameInfo_tvwObjects.Location = new Point(4, 4);
                    GameInfo_tvwObjects.Size = new Size(W - 8, H - 40);
                    GameInfo_tvwObjects.BackColor = Color.White;
                    GameInfo_tvwObjects.ForeColor = Color.Black;
                    GameInfo_tvwObjects.BorderStyle = BorderStyle.FixedSingle;
                    GameInfo_tvwObjects.Font = PhBotFont();
                    GameInfo_tvwObjects.Visible = true;
                }

                int bY = H - 30;
                if (GameInfo_lblServerTime != null)
                {
                    GameInfo_lblServerTime.Font = PhBotFont();
                    GameInfo_lblServerTime.ForeColor = Color.Black;
                    GameInfo_lblServerTime.Location = new Point(6, bY + 3);
                    GameInfo_lblServerTime.AutoSize = true;
                    GameInfo_lblServerTime.Visible = true;
                }
                if (GameInfo_tbxServerTime != null)
                {
                    GameInfo_tbxServerTime.Font = PhBotFont();
                    GameInfo_tbxServerTime.BackColor = Color.White;
                    GameInfo_tbxServerTime.ForeColor = Color.Black;
                    GameInfo_tbxServerTime.Location = new Point(80, bY);
                    GameInfo_tbxServerTime.Size = new Size(100, 22);
                    GameInfo_tbxServerTime.Visible = true;
                }

                Control[] infoChecks = new Control[] { GameInfo_cbxPlayer, GameInfo_cbxPet, GameInfo_cbxMob, GameInfo_cbxNPC, GameInfo_cbxDrop, GameInfo_cbxOthers };
                int cbX = 188;
                for (int i = 0; i < infoChecks.Length; i++)
                {
                    var cb = infoChecks[i] as CheckBox;
                    if (cb != null)
                    {
                        cb.Font = PhBotFont();
                        cb.ForeColor = Color.Black;
                        cb.Location = new Point(cbX, bY + 2);
                        cb.AutoSize = true;
                        cb.Visible = true;
                        cbX += cb.PreferredSize.Width + 6;
                    }
                }
            }
            catch (Exception ex) { PhBotDebug("gameinfo inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }


        // Yerleşim sonrası kardeş çakışmalarını loglar (göz kararı yerine veri).
        private void ReportOverlaps(Control parent, string tag)
        {
            try
            {
                if (parent == null) return;
                var kids = new List<Control>();
                foreach (Control k in parent.Controls)
                {
                    if (k.Visible && k.Width > 2 && k.Height > 2) kids.Add(k);
                }
                for (int i = 0; i < kids.Count; i++)
                {
                    for (int j = i + 1; j < kids.Count; j++)
                    {
                        Control a = kids[i], b = kids[j];
                        if (a.Left < b.Left + b.Width && b.Left < a.Left + a.Width &&
                            a.Top < b.Top + b.Height && b.Top < a.Top + a.Height)
                        {
                            PhBotDebug("OVERLAP[" + tag + "] " + a.Name + " " + a.Bounds +
                                " vs " + b.Name + " " + b.Bounds);
                        }
                    }
                }
            }
            catch { }
        }

        private static string CtrlBounds(Control c)
        {
            try
            {
                if (c == null) return "null";
                return c.Name + " loc=" + c.Location + " size=" + c.Size +
                    " vis=" + c.Visible + " anchor=" + c.Anchor;
            }
            catch { return "?"; }
        }

        private void MoveToLogin(GroupBox login, Control c, int x, int y, int w)
        {
            if (c == null || login == null) return;
            try
            {
                if (c.Parent != login)
                {
                    try { c.Parent.Controls.Remove(c); } catch { }
                    try { login.Controls.Add(c); } catch { }
                }
                Classicize(c);
                if (c is CheckBox || c is RadioButton)
                {
                    (c as ButtonBase).AutoSize = true;
                }
                else if (w > 0)
                {
                    c.Size = new Size(w, c.Height);
                }
                c.Location = new Point(x, y);
                c.Visible = true;
                try { c.Anchor = AnchorStyles.Top | AnchorStyles.Left; } catch { }
            }
            catch { }
        }

        private void SetCheckText(CheckBox cbx, string text)
        {
            if (cbx == null) return;
            try
            {
                cbx.Text = text;
                cbx.Font = PhBotFont();
                cbx.ForeColor = Color.Black;
                cbx.AutoSize = true;
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // TRAINING metinleri + Pick Radius (yerleşim shell'indedir).
        // ---------------------------------------------------------------
        private void LayoutTrainingAreaTexts()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                try { if (Training_btnGetCoordinates != null) Training_btnGetCoordinates.Text = "Get Position"; } catch { }
                try
                {
                    GroupBox details = null;
                    Panel p = TabPageH_Training_Option01_Panel;
                    if (p != null)
                    {
                        foreach (Control c in p.Controls)
                        {
                            if (c is GroupBox && c.Name == "XBotAreaDetails") { details = (GroupBox)c; break; }
                        }
                    }
                    if (details != null && details.Controls["PhBot_PickRadius"] == null)
                    {
                        Label pl = new Label();
                        pl.Name = "PhBot_PickRadiusLbl";
                        pl.Text = "Pick Radius:";
                        pl.Font = PhBotFont();
                        pl.ForeColor = Color.Black;
                        pl.AutoSize = true;
                        pl.Location = new Point(340, 104);
                        details.Controls.Add(pl);
                        TextBox pb = new TextBox();
                        pb.Name = "PhBot_PickRadius";
                        pb.Font = PhBotFont();
                        pb.BackColor = Color.White;
                        pb.Location = new Point(415, 100);
                        pb.Size = new Size(60, 26);
                        pb.Text = "30";
                        details.Controls.Add(pb);
                        // TODO backend: pick radius politikası.
                    }
                }
                catch (Exception ex) { PhBotDebug("pickradius: " + ex.Message); }
            }
            catch (Exception ex) { PhBotDebug("traintexts: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // MAP — docs/phbot_ref/guide/phbot_map_01.png
        // Üst: Harita (Minimap_pnlMap)
        // Alt: HP / MP, EXP / Job EXP, Level / Job Level + Polygon butonları
        // ---------------------------------------------------------------
        private void LayoutMapInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageV_Control01_Minimap_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 400 || H < 250) return;

                const int bottomH = 95;
                int mapH = Math.Max(120, H - bottomH);

                if (Minimap_pnlMap != null)
                {
                    Minimap_pnlMap.Location = new Point(0, 0);
                    Minimap_pnlMap.Size = new Size(W, mapH);
                    Minimap_pnlMap.Visible = true;
                }

                if (Minimap_panelCoords != null)
                {
                    Minimap_panelCoords.Location = new Point(Math.Max(10, W - 320), 6);
                    Minimap_panelCoords.BringToFront();
                }

                if (Minimap_tbrZoom != null)
                {
                    Minimap_tbrZoom.Location = new Point(Math.Max(10, W - 36), 45);
                    Minimap_tbrZoom.Size = new Size(30, Math.Min(160, Math.Max(60, mapH - 55)));
                    Minimap_tbrZoom.BringToFront();
                }

                // Alt panel kontrolleri
                int baseY = mapH + 8;

                // Row 1: HP / EXP / Level
                PhBotLabel(p, "PhBot_MapHPLbl", "HP", 15, baseY + 2);
                if (Character_pgbHP != null)
                {
                    if (Character_pgbHP.Parent != p)
                    {
                        try { Character_pgbHP.Parent.Controls.Remove(Character_pgbHP); } catch { }
                        try { p.Controls.Add(Character_pgbHP); } catch { }
                    }
                    Character_pgbHP.TrackColor = Color.FromArgb(16, 172, 36);
                    Character_pgbHP.BackColor = Color.FromArgb(16, 172, 36);
                    Character_pgbHP.ForeColor = Color.White;
                    Character_pgbHP.Display = xGraphics.xProgressBarDisplay.Values;
                    Place(Character_pgbHP, 55, baseY, 150, 20);
                    Character_pgbHP.BringToFront();
                }

                PhBotLabel(p, "PhBot_MapExpLbl", "EXP", 225, baseY + 2);
                if (Character_pgbExp != null)
                {
                    if (Character_pgbExp.Parent != p)
                    {
                        try { Character_pgbExp.Parent.Controls.Remove(Character_pgbExp); } catch { }
                        try { p.Controls.Add(Character_pgbExp); } catch { }
                    }
                    Character_pgbExp.TrackColor = Color.FromArgb(235, 235, 235);
                    Character_pgbExp.BackColor = Color.FromArgb(16, 172, 36);
                    Character_pgbExp.ForeColor = Color.Black;
                    Character_pgbExp.Display = xGraphics.xProgressBarDisplay.Percentage;
                    Place(Character_pgbExp, 280, baseY, 150, 20);
                    Character_pgbExp.BringToFront();
                }

                PhBotLabel(p, "PhBot_MapLvLbl", "Level", 450, baseY + 2);
                if (Character_lblLevel != null)
                {
                    if (Character_lblLevel.Parent != p)
                    {
                        try { Character_lblLevel.Parent.Controls.Remove(Character_lblLevel); } catch { }
                        try { p.Controls.Add(Character_lblLevel); } catch { }
                    }
                    Character_lblLevel.Font = PhBotFont();
                    Character_lblLevel.ForeColor = Color.Black;
                    Character_lblLevel.Location = new Point(510, baseY + 2);
                    Character_lblLevel.Visible = true;
                    Character_lblLevel.BringToFront();
                }

                // Row 2: MP / Job EXP / Job Level
                int r2Y = baseY + 26;
                PhBotLabel(p, "PhBot_MapMPLbl", "MP", 15, r2Y + 2);
                if (Character_pgbMP != null)
                {
                    if (Character_pgbMP.Parent != p)
                    {
                        try { Character_pgbMP.Parent.Controls.Remove(Character_pgbMP); } catch { }
                        try { p.Controls.Add(Character_pgbMP); } catch { }
                    }
                    Character_pgbMP.TrackColor = Color.FromArgb(16, 172, 36);
                    Character_pgbMP.BackColor = Color.FromArgb(16, 172, 36);
                    Character_pgbMP.ForeColor = Color.White;
                    Character_pgbMP.Display = xGraphics.xProgressBarDisplay.Values;
                    Place(Character_pgbMP, 55, r2Y, 150, 20);
                    Character_pgbMP.BringToFront();
                }

                PhBotLabel(p, "PhBot_MapJobExpLbl", "Job EXP", 225, r2Y + 2);
                if (Character_pgbJobExp != null)
                {
                    if (Character_pgbJobExp.Parent != p)
                    {
                        try { Character_pgbJobExp.Parent.Controls.Remove(Character_pgbJobExp); } catch { }
                        try { p.Controls.Add(Character_pgbJobExp); } catch { }
                    }
                    Character_pgbJobExp.TrackColor = Color.FromArgb(235, 235, 235);
                    Character_pgbJobExp.BackColor = Color.FromArgb(16, 172, 36);
                    Character_pgbJobExp.ForeColor = Color.Black;
                    Character_pgbJobExp.Display = xGraphics.xProgressBarDisplay.Percentage;
                    Place(Character_pgbJobExp, 280, r2Y, 150, 20);
                    Character_pgbJobExp.BringToFront();
                }

                PhBotLabel(p, "PhBot_MapJobLvLbl", "Job Level", 450, r2Y + 2);
                if (Character_lblJobLevel != null)
                {
                    if (Character_lblJobLevel.Parent != p)
                    {
                        try { Character_lblJobLevel.Parent.Controls.Remove(Character_lblJobLevel); } catch { }
                        try { p.Controls.Add(Character_lblJobLevel); } catch { }
                    }
                    Character_lblJobLevel.Font = PhBotFont();
                    Character_lblJobLevel.ForeColor = Color.Black;
                    Character_lblJobLevel.Location = new Point(510, r2Y + 2);
                    Character_lblJobLevel.Visible = true;
                    Character_lblJobLevel.BringToFront();
                }

                // Row 3: Buttons Record Polygon / Stop Recording / Clear
                int r3Y = r2Y + 28;
                PhBotButton(p, "PhBot_MapRecPoly", "Record Polygon", 15, r3Y, 110);
                Button btnStop = PhBotButton(p, "PhBot_MapStopPoly", "Stop Recording", 130, r3Y, 110);
                if (btnStop != null) btnStop.Enabled = false;
                PhBotButton(p, "PhBot_MapClearPoly", "Clear", 245, r3Y, 70);
            }
            catch (Exception ex) { PhBotDebug("map inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // ALCHEMY — guide/phbot_alchemy_01..07.png:
        // Gear (Plus, Attribute, Alchemic, Dismantle, Disjoint) | Shining Stone | Tablet
        // ---------------------------------------------------------------
        private void LayoutAlchemyInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Alchemy_Panel;
                if (host == null) return;
                int W = host.Width, H = host.Height;
                if (W < 200 || H < 100) return;
                try { host.AutoScroll = false; } catch { }

                // Eski kartları gizle
                try { if (gbxAlchemySettings != null) gbxAlchemySettings.Visible = false; } catch { }
                try { if (gbxAlchemyActions != null) gbxAlchemyActions.Visible = false; } catch { }

                // Ana phBot TabControl: Gear | Shining Stone | Tablet
                TabControl tabRoot = host.Controls["PhBot_AlchemyTabRoot"] as TabControl;
                if (tabRoot == null)
                {
                    tabRoot = new TabControl { Name = "PhBot_AlchemyTabRoot", Dock = DockStyle.Fill, Font = PhBotFont() };
                    TabPage tabGear = new TabPage { Name = "PhBot_AlcTabGear", Text = "Gear", BackColor = Color.White };
                    TabPage tabShining = new TabPage { Name = "PhBot_AlcTabShining", Text = "Shining Stone", BackColor = Color.White };
                    TabPage tabTablet = new TabPage { Name = "PhBot_AlcTabTablet", Text = "Tablet", BackColor = Color.White };
                    tabRoot.TabPages.AddRange(new TabPage[] { tabGear, tabShining, tabTablet });
                    host.Controls.Add(tabRoot);
                }
                tabRoot.Visible = true;
                tabRoot.BringToFront();

                TabPage pGear = tabRoot.TabPages["PhBot_AlcTabGear"];
                TabPage pShining = tabRoot.TabPages["PhBot_AlcTabShining"];
                TabPage pTablet = tabRoot.TabPages["PhBot_AlcTabTablet"];

                // ---- GEAR TAB ----
                if (pGear != null)
                {
                    int gw = pGear.Width, gh = pGear.Height;
                    if (gw < 200) gw = W - 16;
                    if (gh < 100) gh = H - 32;

                    // Item Satırı
                    PhBotLabel(pGear, "PhBot_AlcItemLbl", "Item", 14, 18);
                    if (cmbxAlchemyItems != null)
                    {
                        if (cmbxAlchemyItems.Parent != pGear)
                        {
                            try { cmbxAlchemyItems.Parent.Controls.Remove(cmbxAlchemyItems); } catch { }
                            pGear.Controls.Add(cmbxAlchemyItems);
                        }
                        cmbxAlchemyItems.Location = new Point(65, 14);
                        cmbxAlchemyItems.Size = new Size(304, 24);
                        cmbxAlchemyItems.Font = PhBotFont();
                        cmbxAlchemyItems.BackColor = Color.White;
                        cmbxAlchemyItems.ForeColor = Color.Black;
                        cmbxAlchemyItems.DropDownStyle = ComboBoxStyle.DropDownList;
                        cmbxAlchemyItems.Visible = true;
                    }
                    if (btnAlchemyRefreshItems != null)
                    {
                        btnAlchemyRefreshItems.Visible = false;
                    }

                    // Alt Sekmeler: Plus | Attribute | Alchemic | Dismantle | Disjoint
                    TabControl gearSub = pGear.Controls["PhBot_GearSubTabs"] as TabControl;
                    if (gearSub == null)
                    {
                        gearSub = new TabControl { Name = "PhBot_GearSubTabs", Location = new Point(14, 46), Size = new Size(355, 260), Font = PhBotFont() };
                        TabPage subPlus = new TabPage { Name = "PhBot_SubPlus", Text = "Plus", BackColor = Color.White };
                        TabPage subAttr = new TabPage { Name = "PhBot_SubAttr", Text = "Attribute", BackColor = Color.White };
                        TabPage subAlch = new TabPage { Name = "PhBot_SubAlch", Text = "Alchemic", BackColor = Color.White };
                        TabPage subDism = new TabPage { Name = "PhBot_SubDism", Text = "Dismantle", BackColor = Color.White };
                        TabPage subDisj = new TabPage { Name = "PhBot_SubDisj", Text = "Disjoint", BackColor = Color.White };
                        gearSub.TabPages.AddRange(new TabPage[] { subPlus, subAttr, subAlch, subDism, subDisj });
                        pGear.Controls.Add(gearSub);
                    }
                    gearSub.Location = new Point(14, 46);
                    gearSub.Size = new Size(355, 260);
                    gearSub.Visible = true;

                    // Plus Sekmesi
                    TabPage sp = gearSub.TabPages["PhBot_SubPlus"];
                    if (sp != null)
                    {
                        PhBotLabel(sp, "PhBot_PlusStopLbl", "Stop at", 10, 14);
                        if (nudAlchemyTargetPlus != null)
                        {
                            if (nudAlchemyTargetPlus.Parent != sp)
                            {
                                try { nudAlchemyTargetPlus.Parent.Controls.Remove(nudAlchemyTargetPlus); } catch { }
                                sp.Controls.Add(nudAlchemyTargetPlus);
                            }
                            nudAlchemyTargetPlus.Location = new Point(105, 12);
                            nudAlchemyTargetPlus.Size = new Size(48, 22);
                            nudAlchemyTargetPlus.Font = PhBotFont();
                            nudAlchemyTargetPlus.BackColor = Color.White;
                            nudAlchemyTargetPlus.ForeColor = Color.Black;
                            nudAlchemyTargetPlus.Visible = true;
                        }
                        PhBotLabel(sp, "PhBot_PlusStopUnit", "+0", 158, 14);

                        PhBotLabel(sp, "PhBot_PlusSuccLbl", "Success delay", 10, 38);
                        if (nudAlchemyDelay != null)
                        {
                            if (nudAlchemyDelay.Parent != sp)
                            {
                                try { nudAlchemyDelay.Parent.Controls.Remove(nudAlchemyDelay); } catch { }
                                sp.Controls.Add(nudAlchemyDelay);
                            }
                            nudAlchemyDelay.Location = new Point(105, 36);
                            nudAlchemyDelay.Size = new Size(48, 22);
                            nudAlchemyDelay.Font = PhBotFont();
                            nudAlchemyDelay.BackColor = Color.White;
                            nudAlchemyDelay.ForeColor = Color.Black;
                            nudAlchemyDelay.Visible = true;
                        }
                        PhBotLabel(sp, "PhBot_PlusSuccUnit", "ms    0%", 158, 38);

                        PhBotLabel(sp, "PhBot_PlusFailLbl", "Failure delay", 10, 62);
                        PhBotTodoNumber(sp, "PhBot_PlusFailN", 105, 60, 48, "5000");
                        PhBotLabel(sp, "PhBot_PlusFailUnit", "ms", 158, 62);

                        // Lucky Powder
                        int cy = 86;
                        if (cbxAlchemyUsePowder != null)
                        {
                            if (cbxAlchemyUsePowder.Parent != sp)
                            {
                                try { cbxAlchemyUsePowder.Parent.Controls.Remove(cbxAlchemyUsePowder); } catch { }
                                sp.Controls.Add(cbxAlchemyUsePowder);
                            }
                            cbxAlchemyUsePowder.Text = "Lucky powder";
                            cbxAlchemyUsePowder.Font = PhBotFont();
                            cbxAlchemyUsePowder.ForeColor = Color.Black;
                            cbxAlchemyUsePowder.Location = new Point(10, cy);
                            cbxAlchemyUsePowder.AutoSize = true;
                            cbxAlchemyUsePowder.Visible = true;
                        }
                        PhBotLabel(sp, "PhBot_PowderPlus", "+", 98, cy + 2);
                        PhBotTodoNumber(sp, "PhBot_PowderN", 108, cy, 26, "0");
                        ComboBox cPowder = sp.Controls["PhBot_PowderAuto"] as ComboBox;
                        if (cPowder == null)
                        {
                            cPowder = new ComboBox { Name = "PhBot_PowderAuto", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                            cPowder.Items.Add("Auto");
                            cPowder.SelectedIndex = 0;
                            sp.Controls.Add(cPowder);
                        }
                        cPowder.Location = new Point(138, cy); cPowder.Size = new Size(50, 22); cPowder.Visible = true;

                        cy = 108;
                        PhBotTodoCheck(sp, "PhBot_Astral", "Astral", 10, cy, false);
                        PhBotLabel(sp, "PhBot_AstralPlus", "+", 98, cy + 2);
                        PhBotTodoNumber(sp, "PhBot_AstralN", 108, cy, 26, "0");
                        PhBotTodoCheck(sp, "PhBot_MoveOnFail", "Move on after plus failure", 180, cy, false);

                        cy = 130;
                        PhBotTodoCheck(sp, "PhBot_Steady", "Steady", 10, cy, false);
                        PhBotLabel(sp, "PhBot_SteadyPlus", "+", 98, cy + 2);
                        PhBotTodoNumber(sp, "PhBot_SteadyN", 108, cy, 26, "0");
                        PhBotTodoCheck(sp, "PhBot_StopDestroyed", "Stop all if destroyed", 180, cy, false);

                        cy = 152;
                        PhBotTodoCheck(sp, "PhBot_Immortal", "Immortal", 10, cy, false);
                        PhBotLabel(sp, "PhBot_ImmortalPlus", "+", 98, cy + 2);
                        PhBotTodoNumber(sp, "PhBot_ImmortalN", 108, cy, 26, "0");

                        cy = 174;
                        PhBotTodoCheck(sp, "PhBot_LuckyStone", "Lucky stone", 10, cy, false);
                        PhBotLabel(sp, "PhBot_LuckyStonePlus", "+", 98, cy + 2);
                        PhBotTodoNumber(sp, "PhBot_LuckyStoneN", 108, cy, 26, "0");

                        cy = 196;
                        PhBotTodoCheck(sp, "PhBot_StopAttempt", "Stop after attempt", 10, cy, false);
                        if (nudAlchemyMaxAttempts != null)
                        {
                            if (nudAlchemyMaxAttempts.Parent != sp)
                            {
                                try { nudAlchemyMaxAttempts.Parent.Controls.Remove(nudAlchemyMaxAttempts); } catch { }
                                sp.Controls.Add(nudAlchemyMaxAttempts);
                            }
                            nudAlchemyMaxAttempts.Location = new Point(130, cy - 1);
                            nudAlchemyMaxAttempts.Size = new Size(50, 22);
                            nudAlchemyMaxAttempts.Font = PhBotFont();
                            nudAlchemyMaxAttempts.BackColor = Color.White;
                            nudAlchemyMaxAttempts.ForeColor = Color.Black;
                            nudAlchemyMaxAttempts.Visible = true;
                        }
                    }

                    // Alt Butonlar (Add All Items | Add | Cancel | Start)
                    int by = 310;
                    PhBotButton(pGear, "PhBot_AlcAddAll", "Add All Items", 14, by, 84);
                    PhBotButton(pGear, "PhBot_AlcAdd", "Add", 102, by, 50);
                    PhBotButton(pGear, "PhBot_AlcCancel", "Cancel", 156, by, 54);

                    if (btnAlchemyStart != null)
                    {
                        if (btnAlchemyStart.Parent != pGear)
                        {
                            try { btnAlchemyStart.Parent.Controls.Remove(btnAlchemyStart); } catch { }
                            pGear.Controls.Add(btnAlchemyStart);
                        }
                        btnAlchemyStart.Text = "Start";
                        btnAlchemyStart.Font = PhBotFont();
                        btnAlchemyStart.Location = new Point(214, by);
                        btnAlchemyStart.Size = new Size(54, 25);
                        btnAlchemyStart.Visible = true;
                    }
                    if (btnAlchemyStop != null)
                    {
                        btnAlchemyStop.Visible = false;
                    }

                    // Dismantle sekmesi uyarısı
                    TabPage sd = gearSub.TabPages["PhBot_SubDism"];
                    if (sd != null)
                    {
                        Label dWarn = sd.Controls["PhBot_DismWarn"] as Label;
                        if (dWarn == null)
                        {
                            dWarn = new Label
                            {
                                Name = "PhBot_DismWarn",
                                Text = "* This tab allows you to completely destroy items in\nyour inventory.\n\nYou have been warned.",
                                ForeColor = Color.Red,
                                Font = PhBotFont(),
                                Location = new Point(12, 20),
                                AutoSize = true
                            };
                            sd.Controls.Add(dWarn);
                        }
                        dWarn.Visible = true;
                    }

                    // Sağ Kolon: Alchemy Listesi (Type | Item)
                    int rightX = 380;
                    int rightW = Math.Max(200, gw - rightX - 10);
                    ListView lv = pGear.Controls["PhBot_AlcListView"] as ListView;
                    if (lv == null)
                    {
                        lv = NewPhBotListView(rightX, 14, rightW, Math.Max(200, gh - 24), "Type|90", "Item|220");
                        lv.Name = "PhBot_AlcListView";
                        pGear.Controls.Add(lv);
                    }
                    lv.Location = new Point(rightX, 14);
                    lv.Size = new Size(rightW, Math.Max(200, gh - 24));
                    lv.Visible = true;
                }

                // ---- SHINING STONE TAB ----
                if (pShining != null)
                {
                    ComboBox cmbShine = pShining.Controls["PhBot_ShineCmb"] as ComboBox;
                    if (cmbShine == null)
                    {
                        cmbShine = new ComboBox { Name = "PhBot_ShineCmb", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                        cmbShine.Items.Add("Shining Stone");
                        cmbShine.SelectedIndex = 0;
                        pShining.Controls.Add(cmbShine);
                    }
                    cmbShine.Location = new Point(14, 18); cmbShine.Size = new Size(200, 24); cmbShine.Visible = true;
                    PhBotLabel(pShining, "PhBot_BlueStoneLbl", "Blue Stone:           0", 14, 60);
                    PhBotLabel(pShining, "PhBot_BlackStoneLbl", "Black Stone:          0", 14, 96);
                    PhBotButton(pShining, "PhBot_ShineCraft", "Craft All", 14, 140, 75);
                    PhBotButton(pShining, "PhBot_ShineCancel", "Cancel", 94, 140, 75);
                }

                // ---- TABLET TAB ----
                if (pTablet != null)
                {
                    ComboBox cmbTab = pTablet.Controls["PhBot_TabletCmb"] as ComboBox;
                    if (cmbTab == null)
                    {
                        cmbTab = new ComboBox { Name = "PhBot_TabletCmb", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                        cmbTab.Items.Add("None");
                        cmbTab.SelectedIndex = 0;
                        pTablet.Controls.Add(cmbTab);
                    }
                    cmbTab.Location = new Point(14, 18); cmbTab.Size = new Size(200, 24); cmbTab.Visible = true;
                    PhBotLabel(pTablet, "PhBot_EarthLbl", "Earth           0", 14, 60);
                    PhBotLabel(pTablet, "PhBot_WindLbl", "Wind            0", 14, 92);
                    PhBotLabel(pTablet, "PhBot_FireLbl", "Fire            0", 14, 124);
                    PhBotLabel(pTablet, "PhBot_WaterLbl", "Water           0", 14, 156);
                    PhBotButton(pTablet, "PhBot_TabStart", "Start", 14, 200, 75);
                    PhBotButton(pTablet, "PhBot_TabStop", "Stop", 94, 200, 75);
                }
            }
            catch (Exception ex) { PhBotDebug("alchemy inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // TRADE — phbot_trade_01.png, 02.png, 03.png: Loop | Items | Options
        // ---------------------------------------------------------------
        private void LayoutTradeInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Trade_Panel;
                if (host == null) return;
                int W = host.Width, H = host.Height;
                if (W < 200 || H < 100) return;

                TabControl tabs = host.Controls["PhBot_TradeTabs"] as TabControl;
                if (tabs == null)
                {
                    tabs = new TabControl { Name = "PhBot_TradeTabs", Font = PhBotFont(), Dock = DockStyle.Fill };
                    var tpLoop = new TabPage("Loop") { Name = "PhBot_TradeLoop", BackColor = Color.White };
                    var tpItems = new TabPage("Items") { Name = "PhBot_TradeItems", BackColor = Color.White };
                    var tpOptions = new TabPage("Options") { Name = "PhBot_TradeOptions", BackColor = Color.White };
                    tabs.TabPages.Add(tpLoop);
                    tabs.TabPages.Add(tpItems);
                    tabs.TabPages.Add(tpOptions);
                    host.Controls.Add(tabs);
                }
                tabs.BringToFront();

                TabPage pLoop = tabs.TabPages["PhBot_TradeLoop"];
                TabPage pItems = tabs.TabPages["PhBot_TradeItems"];
                TabPage pOptions = tabs.TabPages["PhBot_TradeOptions"];

                // ---- LOOP TAB ----
                if (pLoop != null)
                {
                    int pw = pLoop.Width, ph = pLoop.Height;
                    if (pw < 200) pw = W;
                    if (ph < 100) ph = H - 35;

                    // Start
                    PhBotLabel(pLoop, "PhBot_TradeStartLbl", "Start", 14, 14);
                    ComboBox cmbStart = pLoop.Controls["PhBot_TradeStartCmb"] as ComboBox;
                    if (cmbStart == null)
                    {
                        cmbStart = new ComboBox { Name = "PhBot_TradeStartCmb", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                        cmbStart.Items.AddRange(new object[] { "Jangan", "Donwhang", "Hotan", "Samarkand", "Constantinople", "Alexandria" });
                        cmbStart.SelectedIndex = 0;
                        pLoop.Controls.Add(cmbStart);
                    }
                    cmbStart.Location = new Point(14, 34);
                    cmbStart.Size = new Size(180, 24);
                    cmbStart.Visible = true;

                    // End
                    PhBotLabel(pLoop, "PhBot_TradeEndLbl", "End", 14, 68);
                    ComboBox cmbEnd = pLoop.Controls["PhBot_TradeEndCmb"] as ComboBox;
                    if (cmbEnd == null)
                    {
                        cmbEnd = new ComboBox { Name = "PhBot_TradeEndCmb", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                        cmbEnd.Items.AddRange(new object[] { "Jangan", "Donwhang", "Hotan", "Samarkand", "Constantinople", "Alexandria" });
                        cmbEnd.SelectedIndex = 2; // Hotan
                        pLoop.Controls.Add(cmbEnd);
                    }
                    cmbEnd.Location = new Point(14, 88);
                    cmbEnd.Size = new Size(180, 24);
                    cmbEnd.Visible = true;

                    // Transport
                    PhBotLabel(pLoop, "PhBot_TradeTransLbl", "Transport", 14, 122);
                    ComboBox cmbTrans = pLoop.Controls["PhBot_TradeTransCmb"] as ComboBox;
                    if (cmbTrans == null)
                    {
                        cmbTrans = new ComboBox { Name = "PhBot_TradeTransCmb", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                        cmbTrans.Items.AddRange(new object[] { "Bactrian camel", "Horse", "Camel", "Elephant" });
                        cmbTrans.SelectedIndex = 0;
                        pLoop.Controls.Add(cmbTrans);
                    }
                    cmbTrans.Location = new Point(14, 142);
                    cmbTrans.Size = new Size(180, 24);
                    cmbTrans.Visible = true;

                    // Star
                    PhBotLabel(pLoop, "PhBot_TradeStarLbl", "Star", 14, 176);
                    ComboBox cmbStar = pLoop.Controls["PhBot_TradeStarCmb"] as ComboBox;
                    if (cmbStar == null)
                    {
                        cmbStar = new ComboBox { Name = "PhBot_TradeStarCmb", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                        cmbStar.Items.AddRange(new object[] { "Use quantity", "1 Star", "2 Star", "3 Star", "4 Star", "5 Star" });
                        cmbStar.SelectedIndex = 0;
                        pLoop.Controls.Add(cmbStar);
                    }
                    cmbStar.Location = new Point(14, 196);
                    cmbStar.Size = new Size(120, 24);
                    cmbStar.Visible = true;

                    // Add Button
                    Button btnAdd = pLoop.Controls["PhBot_TradeAdd"] as Button;
                    if (btnAdd == null)
                    {
                        btnAdd = new Button { Name = "PhBot_TradeAdd", Text = "Add", Font = PhBotFont(), Size = new Size(180, 28) };
                        pLoop.Controls.Add(btnAdd);
                    }
                    btnAdd.Location = new Point(14, 234);
                    btnAdd.Visible = true;
                    Classicize(btnAdd);

                    // Start / Stop Buttons
                    Button btnStart = pLoop.Controls["PhBot_TradeStart"] as Button;
                    if (btnStart == null)
                    {
                        btnStart = new Button { Name = "PhBot_TradeStart", Text = "Start", Font = PhBotFont(), Size = new Size(85, 28) };
                        btnStart.Click += (s, e) => {
                            try { StartTradeLoopFromUi(); } catch { }
                        };
                        pLoop.Controls.Add(btnStart);
                    }
                    btnStart.Location = new Point(14, Math.Max(280, ph - 40));
                    btnStart.Visible = true;
                    Classicize(btnStart);

                    Button btnStop = pLoop.Controls["PhBot_TradeStop"] as Button;
                    if (btnStop == null)
                    {
                        btnStop = new Button { Name = "PhBot_TradeStop", Text = "Stop", Font = PhBotFont(), Size = new Size(85, 28) };
                        btnStop.Click += (s, e) => {
                            try { TradeLoopManager.CancelRequest(); Bot.Get.Stop(); } catch { }
                        };
                        pLoop.Controls.Add(btnStop);
                    }
                    btnStop.Location = new Point(105, Math.Max(280, ph - 40));
                    btnStop.Visible = true;
                    Classicize(btnStop);

                    // Right Side: Routes ListView
                    if (Trade_lstvRoutes != null)
                    {
                        if (Trade_lstvRoutes.Parent != pLoop)
                        {
                            try { Trade_lstvRoutes.Parent?.Controls.Remove(Trade_lstvRoutes); } catch { }
                            pLoop.Controls.Add(Trade_lstvRoutes);
                        }
                        Trade_lstvRoutes.Location = new Point(206, 14);
                        Trade_lstvRoutes.Size = new Size(Math.Max(200, pw - 220), Math.Max(200, ph - 28));
                        Trade_lstvRoutes.Visible = true;
                        Classicize(Trade_lstvRoutes);
                        Trade_lstvRoutes.Columns.Clear();
                        int colRouteW = Math.Max(100, (Trade_lstvRoutes.ClientSize.Width - 4) / 4);
                        Trade_lstvRoutes.Columns.Add("Start", colRouteW);
                        Trade_lstvRoutes.Columns.Add("End", colRouteW);
                        Trade_lstvRoutes.Columns.Add("Transport", colRouteW);
                        Trade_lstvRoutes.Columns.Add("Star", Math.Max(100, Trade_lstvRoutes.ClientSize.Width - colRouteW * 3 - 4));
                    }
                }

                // ---- ITEMS TAB ----
                if (pItems != null)
                {
                    int pw = pItems.Width, ph = pItems.Height;
                    if (pw < 200) pw = W;
                    if (ph < 100) ph = H - 35;

                    ListView lvItems = pItems.Controls["PhBot_TradeItemsList"] as ListView;
                    if (lvItems == null)
                    {
                        lvItems = NewPhBotListView(14, 14, pw - 28, Math.Max(200, ph - 28), "ID|70", "Icon|60", "Name|280", "Buy|60", "Quantity|80");
                        lvItems.Name = "PhBot_TradeItemsList";
                        pItems.Controls.Add(lvItems);
                    }
                    lvItems.Location = new Point(14, 14);
                    lvItems.Size = new Size(pw - 28, Math.Max(200, ph - 28));
                    lvItems.Visible = true;
                    if (lvItems.Columns.Count >= 5)
                    {
                        lvItems.Columns[0].Width = 70;
                        lvItems.Columns[1].Width = 60;
                        int remN = lvItems.ClientSize.Width - 70 - 60 - 60 - 80 - 4;
                        lvItems.Columns[2].Width = Math.Max(200, remN);
                        lvItems.Columns[3].Width = 60;
                        lvItems.Columns[4].Width = 80;
                    }
                    Classicize(lvItems);
                }

                // ---- OPTIONS TAB ----
                if (pOptions != null)
                {
                    // [ ] Repeat trade loop [ 0 ]
                    EnsureBoxCheck(pOptions, "PhBot_TradeRepeat", "Repeat trade loop", 14, 14);
                    TextBox tbxRep = pOptions.Controls["PhBot_TradeRepeatN"] as TextBox;
                    if (tbxRep == null)
                    {
                        tbxRep = new TextBox { Name = "PhBot_TradeRepeatN", Text = "0", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                        pOptions.Controls.Add(tbxRep);
                    }
                    tbxRep.Location = new Point(230, 12);
                    tbxRep.Size = new Size(50, 22);
                    tbxRep.Visible = true;

                    // [ ] Attack spawned thieves [ 25 ]
                    EnsureBoxCheck(pOptions, "PhBot_TradeAttackThieves", "Attack spawned thieves", 14, 46);
                    TextBox tbxAtk = pOptions.Controls["PhBot_TradeAttackRadius"] as TextBox;
                    if (tbxAtk == null)
                    {
                        tbxAtk = new TextBox { Name = "PhBot_TradeAttackRadius", Text = "25", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                        pOptions.Controls.Add(tbxAtk);
                    }
                    tbxAtk.Location = new Point(230, 44);
                    tbxAtk.Size = new Size(50, 22);
                    tbxAtk.Visible = true;

                    // [ ] Use return scroll after loop completes
                    EnsureBoxCheck(pOptions, "PhBot_TradeReturnScroll", "Use return scroll after loop completes", 14, 78);

                    // [ ] Skip town loop
                    EnsureBoxCheck(pOptions, "PhBot_TradeSkipTown", "Skip town loop", 14, 110);

                    // (o) Stay on transport
                    // ( ) Stay off transport
                    // ( ) Remount transport
                    RadioButton rbnStay = pOptions.Controls["PhBot_TradeStayOn"] as RadioButton;
                    if (rbnStay == null)
                    {
                        rbnStay = new RadioButton { Name = "PhBot_TradeStayOn", Text = "Stay on transport", Font = PhBotFont(), ForeColor = Color.Black, Checked = true, AutoSize = true };
                        pOptions.Controls.Add(rbnStay);
                    }
                    rbnStay.Location = new Point(14, 142);
                    rbnStay.Visible = true;

                    RadioButton rbnStayOff = pOptions.Controls["PhBot_TradeStayOff"] as RadioButton;
                    if (rbnStayOff == null)
                    {
                        rbnStayOff = new RadioButton { Name = "PhBot_TradeStayOff", Text = "Stay off transport", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true };
                        pOptions.Controls.Add(rbnStayOff);
                    }
                    rbnStayOff.Location = new Point(14, 174);
                    rbnStayOff.Visible = true;

                    RadioButton rbnRemount = pOptions.Controls["PhBot_TradeRemount"] as RadioButton;
                    if (rbnRemount == null)
                    {
                        rbnRemount = new RadioButton { Name = "PhBot_TradeRemount", Text = "Remount transport", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true };
                        pOptions.Controls.Add(rbnRemount);
                    }
                    rbnRemount.Location = new Point(14, 206);
                    rbnRemount.Visible = true;
                }

                // Hide custom panels from Window.CustomTabs.cs
                if (Trade_pnlLoop != null) Trade_pnlLoop.Visible = false;
                if (Trade_pnlItems != null) Trade_pnlItems.Visible = false;
                if (Trade_pnlOptions != null) Trade_pnlOptions.Visible = false;
                Control tabStrip = host.Controls["TradeTabs"];
                if (tabStrip != null) tabStrip.Visible = false;
            }
            catch (Exception ex) { PhBotDebug("trade inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }
    }
}
