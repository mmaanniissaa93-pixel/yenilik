using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace xBot.App
{
    public partial class Window
    {
        private FormWindowState _trayRestoreState = FormWindowState.Normal;
        private ToolStripMenuItem _alwaysOnTopMenu;
        private TextBox _clientPathDisplay;
        private CheckBox _visibleClientMode, _visibleNoClientless, _visibleHideLogin;
        private bool _syncingLoginOptions;
        private bool _isInTray;
        private bool _isRestoringFromTray;
        private bool _isMinimizingToTray;
        private bool _initialFocusApplied;

        private void InitializeDesktopBehavior()
        {
            TopMost = false;
            _alwaysOnTopMenu = new ToolStripMenuItem("Her zaman üstte / Always on top")
                { CheckOnClick = true, Checked = false };
            _alwaysOnTopMenu.CheckedChanged += (s, e) => TopMost = _alwaysOnTopMenu.Checked;
            Menu_NotifyIcon.Items.Insert(0, _alwaysOnTopMenu);

            NotifyIcon.MouseClick += NotifyIcon_MouseClick;

            Shown += (s, e) =>
            {
                if (_initialFocusApplied) return;
                _initialFocusApplied = true;

                if (!_alwaysOnTopMenu.Checked)
                {
                    TopMost = true;
                    TopMost = false;
                }
                WinAPI.ShowWindow(Handle, WinAPI.SW_SHOWNORMAL);
                WinAPI.SetForegroundWindow(Handle);
                BringToFront();
                Activate();
            };

            Resize += (s, e) => {
                if (_isRestoringFromTray || _isMinimizingToTray) return;
                if (WindowState == FormWindowState.Minimized)
                {
                    MinimizeToTray();
                }
                else if (!_isInTray && WindowState != FormWindowState.Minimized)
                {
                    _trayRestoreState = WindowState;
                }
            };
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_isInTray || !Visible || WindowState == FormWindowState.Minimized)
                {
                    RestoreFromTray();
                }
                else
                {
                    int showCmd = (WindowState == FormWindowState.Maximized) ? WinAPI.SW_SHOWMAXIMIZED : WinAPI.SW_RESTORE;
                    WinAPI.ShowWindow(Handle, showCmd);
                    WinAPI.SetForegroundWindow(Handle);
                    BringToFront();
                    Activate();
                }
            }
        }

        private void MinimizeToTray()
        {
            if (_isInTray || _isMinimizingToTray || _isRestoringFromTray) return;
            _isMinimizingToTray = true;
            try
            {
                if (WindowState != FormWindowState.Minimized)
                {
                    _trayRestoreState = WindowState;
                }

                _isInTray = true;
                NotifyIcon.Visible = true;
                Hide();
                ShowInTaskbar = false;
                Menu_NotifyIcon_HideShow.Text = LocalizationManager.CurrentLanguage == "TR" ? "Göster" : "Show";
            }
            finally
            {
                _isMinimizingToTray = false;
            }
        }

        private void RestoreFromTray()
        {
            if (_isRestoringFromTray) return;
            _isRestoringFromTray = true;
            try
            {
                _isInTray = false;
                ShowInTaskbar = true;

                FormWindowState target = (_trayRestoreState == FormWindowState.Minimized)
                    ? FormWindowState.Normal
                    : _trayRestoreState;

                Show();
                WindowState = target;

                int showCmd = (target == FormWindowState.Maximized) ? WinAPI.SW_SHOWMAXIMIZED : WinAPI.SW_RESTORE;
                WinAPI.ShowWindow(Handle, showCmd);

                if (_alwaysOnTopMenu != null && _alwaysOnTopMenu.Checked)
                {
                    TopMost = true;
                }
                else
                {
                    TopMost = true;
                    TopMost = false;
                }

                WinAPI.SetForegroundWindow(Handle);
                BringToFront();
                Activate();

                Menu_NotifyIcon_HideShow.Text = LocalizationManager.CurrentLanguage == "TR" ? "Gizle" : "Hide";
            }
            finally
            {
                _isRestoringFromTray = false;
            }
        }

        private bool ChooseClientPath(ListViewItem profile)
        {
            if (profile == null)
            {
                Log("Client dosyası için önce bir Silkroad profili seçin.");
                return false;
            }
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "sro_client.exe seç / Select sro_client.exe";
                dialog.Filter = "Silkroad client (sro_client.exe)|sro_client.exe|Client (*.exe)|*.exe";
                dialog.CheckFileExists = true;
                string current = profile.SubItems.Count > 7 ? profile.SubItems[7].Tag as string : null;
                if (!string.IsNullOrWhiteSpace(current) && File.Exists(current)) dialog.FileName = current;
                if (dialog.ShowDialog(this) != DialogResult.OK) return false;
                SetProfileClientPath(profile, dialog.FileName);
                return true;
            }
        }

        private void SetProfileClientPath(ListViewItem profile, string path)
        {
            if (profile == null || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new ArgumentException("Client dosyası bulunamadı.");
            while (profile.SubItems.Count < 8) profile.SubItems.Add("");
            profile.SubItems[7].Tag = Path.GetFullPath(path);
            RefreshClientPathDisplay();
            Settings.SaveBotSettings();
        }

        private ListViewItem GetLoginProfile()
        {
            return string.IsNullOrWhiteSpace(Login_cmbxSilkroad.Text)
                ? null : Settings_lstvSilkroads.Items[Login_cmbxSilkroad.Text];
        }

        private void RefreshClientPathDisplay()
        {
            if (_clientPathDisplay == null) return;
            var profile = GetLoginProfile();
            _clientPathDisplay.Text = profile != null && profile.SubItems.Count > 7
                ? profile.SubItems[7].Tag as string ?? "" : "";
        }

        private void BuildVisibleClientPath(TabPage page)
        {
            PhBotLabel(page, "ClientProfileLabel", "Silkroad", 14, 17);
            page.Controls.Add(Login_cmbxSilkroad);
            Login_cmbxSilkroad.SetBounds(140, 14, 220, 24);
            Login_cmbxSilkroad.Visible = true;
            page.Controls.Add(Login_btnAddSilkroad);
            Login_btnAddSilkroad.SetBounds(370, 14, 36, 24);
            Login_btnAddSilkroad.Visible = true;
            PhBotLabel(page, "ClientPathLabel", "sro_client.exe", 14, 49);
            if (_clientPathDisplay == null)
            {
                _clientPathDisplay = new TextBox { Name = "ClientPathDisplay", ReadOnly = true };
                page.Controls.Add(_clientPathDisplay);
                var browse = new Button { Name = "BrowseLoginClient", Text = "Seç... / Browse..." };
                browse.SetBounds(510, 46, 110, 25);
                browse.Click += (s, e) => ChooseClientPath(GetLoginProfile());
                page.Controls.Add(browse);
                Login_cmbxSilkroad.SelectedIndexChanged += (s, e) => RefreshClientPathDisplay();
            }
            _clientPathDisplay.SetBounds(140, 46, 360, 24);
            RefreshClientPathDisplay();
        }

        private void BindVisibleLoginOptions(TabPage page)
        {
            if (_visibleClientMode == null)
            {
                _visibleClientMode = (CheckBox)page.Controls["PhBot_ClientMode"];
                _visibleNoClientless = (CheckBox)page.Controls["PhBot_NoClientless"];
                _visibleHideLogin = (CheckBox)page.Controls["PhBot_HideLogin"];
                _visibleClientMode.CheckedChanged += (s, e) => {
                    if (_syncingLoginOptions) return;
                    LoginStrategyManager.UseClient = _visibleClientMode.Checked;
                    Login_rbnClient.Checked = _visibleClientMode.Checked;
                    Login_rbnClientless.Checked = !_visibleClientMode.Checked;
                    Settings.SaveBotSettings();
                };
                _visibleNoClientless.CheckedChanged += (s, e) => {
                    if (_syncingLoginOptions) return;
                    LoginStrategyManager.StayConnected = !_visibleNoClientless.Checked;
                    cbxGeneralStayConnected.Checked = LoginStrategyManager.StayConnected;
                    Settings.SaveBotSettings();
                };
                _visibleHideLogin.CheckedChanged += (s, e) => {
                    if (_syncingLoginOptions) return;
                    LoginStrategyManager.HideLoginInfo = _visibleHideLogin.Checked;
                    Login_tbxUsername.UseSystemPasswordChar = _visibleHideLogin.Checked;
                    Login_tbxPassword.UseSystemPasswordChar = true;
                    Settings.SaveBotSettings();
                };
                Login_cbxUseReturnScroll.CheckedChanged += (s, e) => {
                    if (_syncingLoginOptions) return;
                    LoginStrategyManager.ReturnToTownOnLogin = Login_cbxUseReturnScroll.Checked;
                    Settings.SaveBotSettings();
                };
                Login_cbxRelogin.CheckedChanged += (s, e) => {
                    if (_syncingLoginOptions) return;
                    LoginStrategyManager.AutoRelogin = Login_cbxRelogin.Checked;
                    cbxGeneralAutoRelogin.Checked = Login_cbxRelogin.Checked;
                    Settings.SaveBotSettings();
                };
                Login_rbnClient.CheckedChanged += (s, e) => {
                    if (_syncingLoginOptions) return;
                    LoginStrategyManager.UseClient = Login_rbnClient.Checked;
                    SyncVisibleLoginOptions();
                };
                cbxGeneralAutoRelogin.CheckedChanged += (s, e) => SyncVisibleLoginOptions();
                cbxGeneralStayConnected.CheckedChanged += (s, e) => SyncVisibleLoginOptions();
            }
            SyncVisibleLoginOptions();
            // These controls have no corresponding runtime implementation.
            foreach (string name in new[] { "PhBot_LoginCheck", "PhBot_AllowXTrap", "PhBot_InstantAccess", "PhBot_BlockAfter" })
            {
                Control control = page.Controls[name];
                if (control == null) continue;
                control.Enabled = false;
                if (control is CheckBox check) check.Checked = false;
                ToolTips.SetToolTip(control, "Henüz uygulanmadı / Not implemented");
            }
            PhBotLabel(page, "LoginUnsupportedNote", LocalizationManager.CurrentLanguage == "TR"
                ? "Gri seçenekler henüz uygulanmadı." : "Grey options are not implemented yet.", 420, 326);
        }

        private void SyncVisibleLoginOptions()
        {
            if (_visibleClientMode == null || _syncingLoginOptions) return;
            _syncingLoginOptions = true;
            try
            {
                _visibleClientMode.Checked = LoginStrategyManager.UseClient;
                Login_rbnClient.Checked = LoginStrategyManager.UseClient;
                Login_rbnClientless.Checked = !LoginStrategyManager.UseClient;
                _visibleNoClientless.Checked = !LoginStrategyManager.StayConnected;
                Login_cbxRelogin.Checked = LoginStrategyManager.AutoRelogin;
                Login_cbxUseReturnScroll.Checked = LoginStrategyManager.ReturnToTownOnLogin;
                _visibleHideLogin.Checked = LoginStrategyManager.HideLoginInfo;
                Login_tbxUsername.UseSystemPasswordChar = LoginStrategyManager.HideLoginInfo;
                Login_tbxPassword.UseSystemPasswordChar = true;
            }
            finally { _syncingLoginOptions = false; }
        }
    }
}
