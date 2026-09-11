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
        // LOGIN — initial-startup_05.png: Login grubu (1..8 + 10..20),
        // Reduce Memory (9), Server Capacity (21).
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

                // Akış panelini çöz (gruplar panele döner).
                try
                {
                    Control flow = null;
                    foreach (Control c in host.Controls)
                    {
                        if (c is FlowLayoutPanel && c.Name == host.Name + "_Groups") { flow = c; break; }
                    }
                    if (flow != null)
                    {
                        var kids = new List<Control>();
                        foreach (Control k in flow.Controls) kids.Add(k);
                        foreach (Control k in kids)
                        {
                            try { flow.Controls.Remove(k); host.Controls.Add(k); } catch { }
                        }
                        flow.Visible = false;
                    }
                }
                catch { }
                try { if (Login_gbxAdvertising != null) Login_gbxAdvertising.Visible = false; } catch { }
                try { if (gbxStrategy != null) gbxStrategy.Visible = false; } catch { }

                // Connection grubunu çöz (içindekiler Login grubuna dizilir).
                try
                {
                    if (Login_gbxConnection != null && Login_gbxConnection.Parent == host)
                    {
                        var kids = new List<Control>();
                        foreach (Control k in Login_gbxConnection.Controls) kids.Add(k);
                        foreach (Control k in kids)
                        {
                            try { Login_gbxConnection.Controls.Remove(k); host.Controls.Add(k); } catch { }
                        }
                        Login_gbxConnection.Visible = false;
                    }
                }
                catch { }

                // Ana Login grubu (Sol Kolon: x=8, y=8, w=385, h=290)
                GroupBox login = Login_gbxLogin;
                int lx = 8, ly = 8, lw = 385, lh = 290;
                try
                {
                    if (login != null && login.Parent != host)
                    {
                        try { login.Parent.Controls.Remove(login); } catch { }
                        try { host.Controls.Add(login); } catch { }
                    }
                    if (login != null)
                    {
                        login.Text = "Login";
                        login.Font = PhBotFont();
                        login.ForeColor = Color.Black;
                        login.BackColor = Color.White;
                        login.Location = new Point(lx, ly);
                        login.Size = new Size(lw, lh);
                        login.Visible = true;
                    }
                }
                catch { }
                if (login == null) return;

                // Eski/çakışan kontrolleri gizle
                try { if (Login_btnStart != null) Login_btnStart.Visible = false; } catch { }
                try { if (Login_btnLauncher != null) Login_btnLauncher.Visible = false; } catch { }
                try { if (Login_rbnClient != null) Login_rbnClient.Visible = false; } catch { }
                try { if (Login_rbnClientless != null) Login_rbnClientless.Visible = false; } catch { }
                try { if (Login_cbxGoClientless != null) Login_cbxGoClientless.Visible = false; } catch { }
                try { if (Login_gbxCharacters != null) Login_gbxCharacters.Visible = false; } catch { }

                // Sol alt-kolon (x=10..235):
                int y = 16;
                const int lStep = 24;
                LoginRow(login, Login_lblUsername, "Username", Login_tbxUsername, 10, y, 120); y += lStep;
                LoginRow(login, Login_lblPassword, "Password", Login_tbxPassword, 10, y, 120); y += lStep;
                LoginRow(login, Login_lblServer, "Server", Login_cmbxServer, 10, y, 120); y += lStep;

                // Gateway
                PhBotLabel(login, "PhBot_LoginGwLbl", "Gateway", 10, y + 4);
                ComboBox gw = login.Controls["PhBot_LoginGw"] as ComboBox;
                if (gw == null)
                {
                    gw = new ComboBox();
                    gw.Name = "PhBot_LoginGw";
                    gw.DropDownStyle = ComboBoxStyle.DropDownList;
                    try { login.Controls.Add(gw); } catch { }
                }
                GrayCombo(gw);
                gw.Location = new Point(115, y); gw.Size = new Size(120, 22); gw.Visible = true;
                y += lStep;

                // Switch gateway after
                PhBotLabel(login, "PhBot_LoginSwLbl", "Switch gateway after", 10, y + 4);
                PhBotTodoNumber(login, "PhBot_LoginSw", 132, y, 35, "0");
                PhBotLabel(login, "PhBot_LoginSwU", "attempts", 172, y + 4);
                y += lStep;

                // SOCKS IP/Port
                PhBotLabel(login, "PhBot_SocksIpLbl", "SOCKS IP/Port", 10, y + 4);
                PhBotTodoNumber(login, "PhBot_SocksIp", 115, y, 75, "");
                try { (login.Controls["PhBot_SocksIp"] as TextBox).Size = new Size(75, 22); } catch { }
                PhBotTodoNumber(login, "PhBot_SocksPort", 193, y, 42, "0");
                y += lStep;

                // SOCKS User
                PhBotLabel(login, "PhBot_SocksUserLbl", "SOCKS User", 10, y + 4);
                PhBotTodoNumber(login, "PhBot_SocksUser", 115, y, 120, "");
                try { (login.Controls["PhBot_SocksUser"] as TextBox).Size = new Size(120, 22); } catch { }
                y += lStep;

                // SOCKS Pass
                PhBotLabel(login, "PhBot_SocksPassLbl", "SOCKS Pass", 10, y + 4);
                PhBotTodoNumber(login, "PhBot_SocksPass", 115, y, 120, "");
                try
                {
                    (login.Controls["PhBot_SocksPass"] as TextBox).Size = new Size(120, 22);
                    (login.Controls["PhBot_SocksPass"] as TextBox).UseSystemPasswordChar = true;
                }
                catch { }
                y += lStep;

                // Character
                LoginRow(login, Login_lblCharacter, "Character", Login_cmbxCharacter, 10, y, 120); y += lStep;

                // Captcha
                LoginRow(login, Login_lblCaptcha, "Captcha", Login_tbxCaptcha, 10, y, 120); y += lStep;

                // Saved Accounts
                try
                {
                    if (Login_lblAccount != null)
                    {
                        Login_lblAccount.Text = "Accounts:";
                        Login_lblAccount.Visible = true;
                        if (Login_lblAccount.Parent != login)
                        {
                            try { Login_lblAccount.Parent.Controls.Remove(Login_lblAccount); } catch { }
                            try { login.Controls.Add(Login_lblAccount); } catch { }
                        }
                        Login_lblAccount.Location = new Point(10, y + 4);
                    }
                    MoveToLogin(login, Login_cmbxSavedAccounts, 70, y, 95);
                    MoveToLogin(login, Login_btnSaveAccount, 168, y - 2, 48);
                    MoveToLogin(login, Login_btnDeleteAccount, 218, y - 2, 48);
                    MoveToLogin(login, Login_btnAccountSetup, 268, y - 2, 95);
                    if (Login_btnSaveAccount != null) Login_btnSaveAccount.Text = "Save";
                    if (Login_btnDeleteAccount != null) Login_btnDeleteAccount.Text = "Del";
                    if (Login_btnAccountSetup != null) Login_btnAccountSetup.Text = "Setup";
                }
                catch { }

                // Sağ alt-kolon (x=244..370):
                int rx = 244;
                MoveToLogin(login, Login_cbxUseReturnScroll, rx, 16, 0);
                SetCheckText(Login_cbxUseReturnScroll, "Return to town on login");

                try
                {
                    MoveToLogin(login, cbxGeneralAutoStart, rx, 40, 0);
                    SetCheckText(cbxGeneralAutoStart, "Start bot on login");
                }
                catch { }

                MoveToLogin(login, Login_cbxRelogin, rx, 64, 0);
                SetCheckText(Login_cbxRelogin, "Relog on disconnect");

                // Login / Logout butonları
                Button btnLogin = login.Controls["PhBot_LoginStartBtn"] as Button;
                if (btnLogin == null)
                {
                    btnLogin = new Button { Name = "PhBot_LoginStartBtn", Text = "Login", FlatStyle = FlatStyle.Flat };
                    btnLogin.Click += (s, e) => { try { if (Login_btnStart != null) Login_btnStart.PerformClick(); } catch { } };
                    login.Controls.Add(btnLogin);
                }
                btnLogin.SetBounds(rx, 88, 58, 24);
                btnLogin.Font = PhBotFont();
                btnLogin.BackColor = Color.FromArgb(246, 247, 248);
                btnLogin.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);

                Button btnLogout = login.Controls["PhBot_LoginLogoutBtn"] as Button;
                if (btnLogout == null)
                {
                    btnLogout = new Button { Name = "PhBot_LoginLogoutBtn", Text = "Logout", Enabled = false, FlatStyle = FlatStyle.Flat };
                    login.Controls.Add(btnLogout);
                }
                btnLogout.SetBounds(rx + 62, 88, 58, 24);
                btnLogout.Font = PhBotFont();
                btnLogout.BackColor = Color.FromArgb(246, 247, 248);
                btnLogout.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);

                PhBotTodoCheck(login, "PhBot_LoginJCP", "JCP", rx, 114, false);
                PhBotTodoCheck(login, "PhBot_LoginClient", "Client", rx, 138, false);
                PhBotTodoCheck(login, "PhBot_LoginCap4", "4", rx, 162, false);
                PhBotTodoCheck(login, "PhBot_LoginCap5", "5", rx + 32, 162, true);
                PhBotTodoCheck(login, "PhBot_LoginQueue", "Queue", rx + 66, 162, false);

                PhBotButton(login, "PhBot_SocksReset", "Reset", rx, 186, 50);
                PhBotTodoCheck(login, "PhBot_LoginHide", "Hide login", rx + 56, 188, false);

                PhBotTodoCheck(login, "PhBot_LoginCaptcha", "Captcha", rx, 214, true);
                PhBotTodoNumber(login, "PhBot_LoginCaptchaN", rx + 75, 212, 25, "1");
                PhBotTodoCheck(login, "PhBot_LoginXTrap", "Allow X-Trap", rx, 240, false);

                // Reduce Memory (Sol alt: x=8, y=302, w=145, h=50)
                GroupBox reduce = null;
                try
                {
                    foreach (Control c in host.Controls)
                    {
                        if (c is GroupBox && c.Name == "PhBot_ReduceMem") { reduce = (GroupBox)c; break; }
                    }
                    if (reduce == null)
                    {
                        reduce = new GroupBox { Name = "PhBot_ReduceMem", Text = "Reduce Memory", Font = PhBotFont(), ForeColor = Color.Black, BackColor = Color.White };
                        try { host.Controls.Add(reduce); } catch { }
                        PhBotTodoCheck(reduce, "PhBot_ReduceSilk", "Silkroad", 12, 20, false);
                    }
                    reduce.Location = new Point(8, 302);
                    reduce.Size = new Size(145, 50);
                    reduce.Visible = true;
                }
                catch { }

                // Sağ Kolon: Silkroad Seçici + Server Capacity
                int rightX = 400;
                int rightW = Math.Max(250, W - rightX - 8);

                // Silkroad Seçici (Sol altta, Reduce Memory'nin hemen sağında kompakt grup: 158, 302, 235, 50)
                try
                {
                    Control sel = null;
                    foreach (Control c in host.Controls)
                    {
                        if (c.Name == "XBotServerSelection") { sel = c; break; }
                    }
                    if (sel == null && Login_cmbxSilkroad != null)
                    {
                        sel = new GroupBox { Name = "XBotServerSelection", Text = "Silkroad", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                        foreach (Control c in new Control[] { Login_lblSilkroad, Login_cmbxSilkroad, Login_btnAddSilkroad })
                        {
                            if (c != null)
                            {
                                try { c.Parent?.Controls.Remove(c); } catch { }
                                sel.Controls.Add(c);
                            }
                        }
                        try { host.Controls.Add(sel); } catch { }
                    }
                    if (sel != null)
                    {
                        sel.Location = new Point(158, 302);
                        sel.Size = new Size(235, 50);
                        sel.Visible = true;
                        if (Login_lblSilkroad != null) { Login_lblSilkroad.Visible = false; }
                        if (Login_cmbxSilkroad != null) { Login_cmbxSilkroad.SetBounds(8, 18, 185, 24); Login_cmbxSilkroad.Visible = true; }
                        if (Login_btnAddSilkroad != null) { Login_btnAddSilkroad.SetBounds(197, 17, 28, 24); Login_btnAddSilkroad.Text = "..."; Login_btnAddSilkroad.Visible = true; }
                    }
                }
                catch { }

                // Server Capacity (tam sağ kolon: x=400, y=8, w=rightW, h=344)
                try
                {
                    if (Login_gbxServers != null)
                    {
                        if (Login_gbxServers.Parent != host)
                        {
                            try { Login_gbxServers.Parent.Controls.Remove(Login_gbxServers); } catch { }
                            try { host.Controls.Add(Login_gbxServers); } catch { }
                        }
                        Login_gbxServers.Text = "Server Capacity";
                        Login_gbxServers.Font = PhBotFont();
                        Login_gbxServers.ForeColor = Color.Black;
                        Login_gbxServers.BackColor = Color.White;
                        Login_gbxServers.Location = new Point(rightX, 8);
                        Login_gbxServers.Size = new Size(rightW, Math.Max(200, Math.Min(344, H - 16)));
                        Login_gbxServers.Visible = true;
                    }
                    if (Login_lstvServers != null)
                    {
                        Login_lstvServers.Location = new Point(8, 20);
                        Login_lstvServers.Size = new Size(Math.Max(180, Login_gbxServers.Width - 16), Math.Max(100, Login_gbxServers.Height - 28));
                        Login_lstvServers.Visible = true;
                        Classicize(Login_lstvServers);
                        if (Login_lstvServers.Columns.Count >= 3)
                        {
                            Login_lstvServers.Columns[0].Text = "Server";
                            Login_lstvServers.Columns[1].Text = "State";
                            Login_lstvServers.Columns[2].Text = "Capacity";
                            int usable = Login_lstvServers.ClientSize.Width - 4;
                            Login_lstvServers.Columns[0].Width = (int)(usable * 0.40);
                            Login_lstvServers.Columns[1].Width = (int)(usable * 0.30);
                            Login_lstvServers.Columns[2].Width = Math.Max(60, usable - Login_lstvServers.Columns[0].Width - Login_lstvServers.Columns[1].Width);
                        }
                    }
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("login inner: " + ex.Message); }
            finally { _innerLayout = false; }
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
