using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace xBot.App
{
    public partial class Window
    {
        // The reference supplies the visual structure. The controls, settings and
        // event handlers belong to xBot and retain their original object identity.
        private readonly Dictionary<Control, TabPage> _referencePages = new Dictionary<Control, TabPage>();
        private readonly List<Panel> _referenceViews = new List<Panel>();
        private FlowLayoutPanel _referenceTools;
        private bool _referenceLayingOut;
        private readonly Font _referenceFont = new Font("Segoe UI", 9F);
        private int _sidebarScrollOffset = 0;
        private int _sidebarTotalHeight = 0;

        private void Sidebar_MouseWheel(object sender, MouseEventArgs e)
        {
            if (TabPageV_Control01 == null) return;
            int viewH = TabPageV_Control01.ClientSize.Height;
            int maxScroll = Math.Max(0, _sidebarTotalHeight - viewH);
            if (maxScroll <= 0) return;
            int delta = (e.Delta / 120) * 28;
            int newOffset = Math.Max(0, Math.Min(maxScroll, _sidebarScrollOffset - delta));
            if (newOffset != _sidebarScrollOffset)
            {
                _sidebarScrollOffset = newOffset;
                UpdateSidebarItemPositions();
                TabPageV_Control01.Invalidate();
            }
        }

        private void UpdateSidebarItemPositions()
        {
            if (TabPageV_Control01 == null) return;
            TabPageV_Control01.SuspendLayout();
            foreach (Control c in TabPageV_Control01.Controls)
            {
                if (c.Tag is int origY)
                {
                    c.Top = origY - _sidebarScrollOffset;
                }
            }
            TabPageV_Control01.ResumeLayout(false);
        }

        private void ApplyXBotReferenceLayout()
        {
            if (_phBotClassicApplied) return;
            SuspendLayout();
            try
            {
                FormBorderStyle = FormBorderStyle.FixedSingle;
                MaximizeBox = false;
                MinimizeBox = ControlBox = true;
                Size = new Size(975, 526);
                MinimumSize = MaximumSize = new Size(975, 526);
                StartPosition = FormStartPosition.CenterScreen;
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
                BackColor = PhBotBg;
                ForeColor = Color.Black;
                pnlWindow.Dock = DockStyle.Fill;
                pnlWindow.SizeChanged += (s, e) => LayoutXBotReference();
                pnlWindow.BorderStyle = BorderStyle.None;
                pnlHeader.Visible = false;
                btnBotStart.Visible = btnClientOptions.Visible = btnAnalyzer.Visible = false;
                // Modern temanın yüzen alt şeridi klasik görünümde içeriğin üstüne biner.
                try
                {
                    foreach (Control c in pnlWindow.Controls)
                    {
                        if (c is Theme.ModernStatusStrip) c.Visible = false;
                    }
                }
                catch { }
                if (lblBotState != null)
                {
                    lblBotState.Visible = false;
                    try { this.Controls.Remove(lblBotState); } catch { }
                }
                BuildAlchemyTab();
                BuildTargetAssistTab();
                pnlWindow.Controls.Add(TabPageV_Control01_Alchemy_Panel);
                pnlWindow.Controls.Add(TabPageV_Control01_TargetAssist_Panel);
                // phBot birebir: H-etiketleri düzelt, eksik alt-sekmeleri aç,
                // phBot'a özel sidebar panellerini kur (sarma işleminden önce).
                try { RenamePhBotSubTabs(); } catch { }
                try { EnsurePhBotAttackExtraTabs(); } catch { }
                try { EnsurePhBotProtectionExtraTabs(); } catch { }
                try { EnsurePhBotTrainingExtraTabs(); } catch { }
                try { EnsurePhBotInventoryExtraTabs(); } catch { }
                try { EnsurePhBotStallExtraTabs(); } catch { }
                try { EnsureMissingSidebarPanels(); } catch { }
                BuildXBotReferenceSidebar();
                foreach (Panel view in _referenceViews)
                {
                    foreach (Panel strip in view.Controls.OfType<Panel>().ToArray())
                    {
                        if (strip.Controls.OfType<Button>().Any(b => b.Name.StartsWith("TabPageH_")))
                            WrapXBotReferenceTabs(view, strip);
                    }
                }
                EnsurePhBotBottomArea();
                StyleXBotReference(pnlWindow);
                PrepareXBotReferenceContent();
                // phBot birebir iç düzenler (aynı-nesne-taşıma; bağlı kontroller korunur).
                try { LayoutPhBotInners(); } catch { }
                try { LayoutPhBotInners2(); } catch { }
                try { LayoutPhBotInners3(); } catch { }
                try { LayoutPhBotInners34(); } catch { }
                try { LayoutPhBotInners5(); } catch { }
                _phBotClassicApplied = true;
                LayoutXBotReference();
                SizeChanged += (s, e) => LayoutXBotReference();
                LocalizationManager.OnLanguageChanged += RefreshReferenceLanguage;
                Disposed += (s, e) => {
                    LocalizationManager.OnLanguageChanged -= RefreshReferenceLanguage;
                    _referenceFont.Dispose();
                };
                try
                {
                    using (var bmp = new Bitmap(16, 16))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.Transparent);
                            using (Font pf = new Font("Tahoma", 8f, FontStyle.Bold, GraphicsUnit.Point))
                            using (SolidBrush pb = new SolidBrush(Color.FromArgb(200, 30, 30)))
                            using (SolidBrush xb = new SolidBrush(Color.FromArgb(30, 30, 30)))
                            {
                                g.DrawString("x", pf, pb, -2, 0);
                                g.DrawString("B", pf, xb, 6, 2);
                            }
                        }
                        IntPtr hIcon = bmp.GetHicon();
                        this.Icon = Icon.FromHandle(hIcon);
                    }
                }
                catch { }
                UpdatePhBotTitle();
                try
                {
                    string time = DateTime.Now.ToString("HH:mm:ss");
                    rtbxLogs.Clear();
                    rtbxLogs.AppendText(string.Format("[{0}] Welcome to xBot\r\n", time));
                }
                catch { }
                Control initNav = TabPageV_Control01.Controls["TabPageV_Control01_ProjectHax"] ?? TabPageV_Control01.Controls["TabPageV_Control01_Login"];
                if (initNav != null) TabPageV_Option_Click(initNav, EventArgs.Empty);
            }
            finally { ResumeLayout(true); LayoutXBotReference(); }
        }

        private void BuildXBotReferenceSidebar()
        {
            bool isTR = LocalizationManager.CurrentLanguage == "TR";
            var titles = isTR ? new Dictionary<string, string> {
                { "ProjectHax", "xBot" },
                { "GameInfo", "İstatistikler" }, { "Login", "Silkroad Bağlantısı" },
                { "AutoConfigure", "Otomatik Yapılandırma" }, { "TargetAssist", "Yardımcı" },
                { "Notifications", "Bildirimler" },
                { "Character", "Koruma" }, { "Town", "Şehir" },
                { "Training", "Kasılma Alanı" }, { "Skills", "Saldırı" },
                { "Pet", "Pet" }, { "Party", "Parti" },
                { "UnionParty", "Birlik Partisi" }, { "PickFilter", "Toplama Filtresi" },
                { "Quest", "Görev" }, { "Players", "Oyuncular" },
                { "Guild", "Lonca" }, { "Academy", "Akademi" },
                { "Inventory", "Envanter" }, { "Stall", "Tezgah" },
                { "Trade", "Kervan" }, { "Alchemy", "Simya" },
                { "Masteries", "Ustalıklar" }, { "Chat", "Sohbet" },
                { "Minimap", "Harita" }, { "Sound", "Ses" },
                { "KeyBindings", "Kısayollar" }, { "Conditions", "Koşullar" },
                { "Settings", "Eklentiler" }
            } : new Dictionary<string, string> {
                { "ProjectHax", "xBot" },
                { "GameInfo", "Statistics" }, { "Login", "Silkroad Login" },
                { "AutoConfigure", "Auto Configure" }, { "TargetAssist", "Assistant" },
                { "Notifications", "Notifications" },
                { "Character", "Protection" }, { "Town", "Town" },
                { "Training", "Training Area" }, { "Skills", "Attack" },
                { "Pet", "Pet" }, { "Party", "Party" },
                { "UnionParty", "Union Party" }, { "PickFilter", "Pick Filter" },
                { "Quest", "Quest" }, { "Players", "Players" },
                { "Guild", "Guild" }, { "Academy", "Academy" },
                { "Inventory", "Inventory" }, { "Stall", "Stall" },
                { "Trade", "Trade" }, { "Alchemy", "Alchemy" },
                { "Masteries", "Masteries" }, { "Chat", "Chat" },
                { "Minimap", "Map" }, { "Sound", "Sound" },
                { "KeyBindings", "Key Bindings" }, { "Conditions", "Conditions" },
                { "Settings", "Plugins" }
            };
            var oldButtons = TabPageV_Control01.Controls.OfType<Button>().ToDictionary(b => b.Name);
            var oldIcons = TabPageV_Control01.Controls.OfType<Label>().ToDictionary(b => b.Name);
            if (tabPickFilterRoot != null)
            {
                var pick = new Panel { Name = "TabPageV_Control01_PickFilter_Panel", Visible = false };
                pick.Controls.Add(tabPickFilterRoot);
                tabPickFilterRoot.Dock = DockStyle.Fill;
                pnlWindow.Controls.Add(pick);
            }
            string[] order = {
                "ProjectHax", "GameInfo", "Login", "AutoConfigure", "TargetAssist", "Notifications",
                "Character", "Town", "Training", "Skills", "Pet", "Party",
                "UnionParty", "PickFilter", "Quest", "Players", "Guild", "Academy",
                "Inventory", "Stall", "Trade", "Alchemy", "Masteries",
                "Chat", "Minimap", "Sound", "KeyBindings",
                "Conditions", "Settings"
            };
            var views = pnlWindow.Controls.OfType<Panel>()
                .Where(p => p.Name.StartsWith("TabPageV_Control01_") && p.Name.EndsWith("_Panel")).ToList();
            views.Sort((a, b) => {
                int ai = Array.IndexOf(order, a.Name.Replace("TabPageV_Control01_", "").Replace("_Panel", ""));
                int bi = Array.IndexOf(order, b.Name.Replace("TabPageV_Control01_", "").Replace("_Panel", ""));
                return (ai < 0 ? 100 : ai).CompareTo(bi < 0 ? 100 : bi);
            });
            TabPageV_Control01.Controls.Clear();
            TabPageV_Control01.Tag = null;
            TabPageV_Control01.AutoScroll = false;
            TabPageV_Control01.BorderStyle = BorderStyle.None;
            TabPageV_Control01.BackColor = Color.White;
            TabPageV_Control01.MouseWheel -= Sidebar_MouseWheel;
            TabPageV_Control01.MouseWheel += Sidebar_MouseWheel;
            TabPageV_Control01.Paint += (s, e) =>
            {
                // 1px sağ ayrım çizgisi
                using (var p = new Pen(Color.FromArgb(220, 220, 220)))
                {
                    e.Graphics.DrawLine(p, TabPageV_Control01.Width - 1, 0, TabPageV_Control01.Width - 1, TabPageV_Control01.Height);
                }
                // Seçili öğe mavi dikey göstergesi (X=3, Y=ortalanmış, W=3, H=16)
                var selected = TabPageV_Control01.Tag as List<Control>;
                if (selected != null && selected.Count > 0)
                {
                    Control sel = selected[0];
                    int barY = sel.Top + (sel.Height - 16) / 2;
                    using (var b = new SolidBrush(Color.FromArgb(0, 103, 192)))
                    using (var path = Theme.DarkTheme.CreateRoundedRectangle(new Rectangle(3, barY, 3, 16), 1))
                    {
                        e.Graphics.FillPath(b, path);
                    }
                }
                // phBot zarif 2px overlay kaydırma çubuğu (X=Width-2, W=2)
                int viewH = TabPageV_Control01.ClientSize.Height;
                if (_sidebarTotalHeight > viewH && viewH > 0)
                {
                    int maxScroll = _sidebarTotalHeight - viewH;
                    int thumbH = Math.Max(28, (viewH * viewH) / _sidebarTotalHeight);
                    int thumbY = (_sidebarScrollOffset * (viewH - thumbH)) / maxScroll;
                    using (var b = new SolidBrush(Color.FromArgb(141, 141, 141)))
                    {
                        e.Graphics.FillRectangle(b, TabPageV_Control01.Width - 2, thumbY, 2, thumbH);
                    }
                }
            };
            TabPageV_ColorSelected = PhBotSelect;
            TabPageV_ColorHover = PhBotHover;
            int y = 6;
            foreach (Panel view in views)
            {
                string key = view.Name.Replace("TabPageV_Control01_", "").Replace("_Panel", "");
                if (Array.IndexOf(order, key) < 0) continue;
                string name = view.Name.Substring(0, view.Name.Length - 6);
                string title;
                if (!titles.TryGetValue(key, out title))
                    title = key == "PickFilter" ? "Pick Filter" : key;
                Button button;
                bool existingButton = oldButtons.TryGetValue(name, out button);
                if (!existingButton) button = new Button();
                button.Name = name; button.Text = title;
                button.SetBounds(28, y, 148, 26);
                button.TextAlign = ContentAlignment.MiddleLeft;
                button.FlatStyle = FlatStyle.Flat; button.BackColor = Color.White;
                button.TabIndex = _referenceViews.Count;
                button.Visible = true;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = PhBotHover;
                button.FlatAppearance.MouseDownBackColor = PhBotSelect;
                button.Tag = y;
                button.MouseWheel -= Sidebar_MouseWheel;
                button.MouseWheel += Sidebar_MouseWheel;
                Label icon;
                bool existingIcon = oldIcons.TryGetValue(name + "_Icon", out icon);
                if (!existingIcon) icon = new Label();
                icon.Name = name + "_Icon"; icon.Text = "";
                icon.Image = PhBotIcons.Get(title);
                icon.SetBounds(8, y, 18, 26);
                icon.BackColor = Color.White; icon.Visible = true;
                icon.Tag = y;
                icon.MouseWheel -= Sidebar_MouseWheel;
                icon.MouseWheel += Sidebar_MouseWheel;
                foreach (Control item in new Control[] { button, icon })
                {
                    if ((item == button && !existingButton) || (item == icon && !existingIcon))
                    {
                        item.Click += TabPageV_Option_Click;
                        item.MouseEnter += TabPageV_Option_MouseEnter;
                        item.MouseLeave += TabPageV_Option_MouseLeave;
                    }
                    TabPageV_Control01.Controls.Add(item);
                }
                view.Visible = false;
                view.BorderStyle = BorderStyle.None;
                _referenceViews.Add(view);
                y += 27;
            }
            _sidebarTotalHeight = y + 6;
        }

        private void WrapXBotReferenceTabs(Panel view, Control strip)
        {
            if (strip == null || strip.Tag is TabControl) return;
            // Players ve Guild phBot'ta tek paneldir (üstte sub-tab barındırmaz).
            if (view == TabPageV_Control01_Players_Panel || view == TabPageV_Control01_Guild_Panel)
            {
                strip.Visible = false;
                Panel mainP = (view == TabPageV_Control01_Players_Panel) ? TabPageH_Players_Option01_Panel : TabPageH_Guild_Option01_Panel;
                if (mainP != null)
                {
                    mainP.Dock = DockStyle.Fill;
                    mainP.Visible = true;
                    mainP.BringToFront();
                }
                return;
            }

            var buttons = strip.Controls.OfType<Button>().Where(b => b.Name.StartsWith("TabPageH_"))
                .OrderBy(b => b.Left).ToList();

            if (view == TabPageV_Control01_Character_Panel)
            {
                string[] protOrder = { "Potions", "Sockets", "Return", "Pet Return", "Berserk", "Monster Preferences", "Devil's Spirit", "Scrolls", "Stat Points", "Misc." };
                buttons.Sort((a, b) => {
                    int ai = Array.IndexOf(protOrder, a.Text);
                    int bi = Array.IndexOf(protOrder, b.Text);
                    return (ai < 0 ? 100 : ai).CompareTo(bi < 0 ? 100 : bi);
                });
            }
            else if (view == TabPageV_Control01_Skills_Panel)
            {
                string[] skillOrder = { "Attack", "Buffs", "Party", "Party Options", "Resurrect", "Healing", "Lure", "Attack Log" };
                buttons.Sort((a, b) => {
                    int ai = Array.IndexOf(skillOrder, a.Text);
                    int bi = Array.IndexOf(skillOrder, b.Text);
                    return (ai < 0 ? 100 : ai).CompareTo(bi < 0 ? 100 : bi);
                });
            }

            var tabs = new TabControl { Name = strip.Name + "_Tabs", Font = PhBotFont(),
                Padding = new Point(8, 3), Multiline = false };
            var selected = strip.Tag as Control;
            foreach (Button button in buttons)
            {
                var panel = view.Controls[button.Name + "_Panel"] as Panel;
                if (panel == null) continue;
                // Pick Filter now has its own navigation entry.
                if (button == TabPageH_Town_Option03) { panel.Visible = false; continue; }
                // Protection Info sekmesi phBot'ta yoktur (genel statlar Map sekmesindedir).
                if (button == TabPageH_Character_Option01) { panel.Visible = false; continue; }

                var page = new TabPage(button.Text) { Tag = button, BackColor = Color.White, Padding = new Padding(8) };
                tabs.TabPages.Add(page);
                _referencePages.Add(button, page);
                panel.Dock = DockStyle.Fill;
                panel.BorderStyle = BorderStyle.None;
                panel.AutoScroll = true;
                page.Controls.Add(panel);
                panel.Visible = true;
                if (button == selected) tabs.SelectedTab = page;
            }
            if (tabs.TabCount == 0) { tabs.Dispose(); return; }
            if (tabs.SelectedTab == null && tabs.TabPages.Count > 0) tabs.SelectedIndex = 0;
            strip.Tag = tabs;
            strip.Visible = false;
            view.Controls.Add(tabs);
            tabs.BringToFront();

            if (view == TabPageV_Control01_Skills_Panel && Skills_lstvSkills != null)
            {
                Skills_lstvSkills.Parent = view;
                Skills_lstvSkills.SetBounds(0, 0, 220, view.Height);
                Skills_lstvSkills.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                Skills_lstvSkills.BringToFront();
            }

            // Real TabPages own their contents, so both mouse and keyboard tab
            // changes show exactly one page, even while the old strip is hidden.
            tabs.SelectedIndexChanged += (s, e) => {
                if (view == TabPageV_Control01_Skills_Panel) LayoutXBotSkillPages();
            };
        }

        private void BuildXBotReferenceTools()
        {
            _referenceTools = new FlowLayoutPanel { Name = "XBotTools", WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, AutoScroll = false };
            pnlWindow.Controls.Add(_referenceTools);
            var tools = new[] { btnLangTR, btnLangEN, btnQuickSave, btnCommandCenter, btnAnalyzer };
            string[] labels = { "TR", "EN", "Save", "Command Center", "Analyzer" };
            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] == null) continue;
                _referenceTools.Controls.Add(tools[i]);
                tools[i].Text = labels[i];
                tools[i].AutoSize = true;
                tools[i].Size = new Size(i < 2 ? 34 : 75, 26);
                tools[i].Margin = new Padding(0, 0, 6, 0);
                tools[i].Visible = true;
            }
        }

        private void RefreshReferenceLanguage()
        {
            foreach (var entry in _referencePages) entry.Value.Text = entry.Key.Text;
            StyleXBotReference(pnlWindow);
            PreparePotionCaptions();
            RefreshSidebarTitles();
            // Reapply the active row after recoloring the navigation list.
            var selected = TabPageV_Control01.Tag as List<Control>;
            if (selected != null) foreach (Control c in selected) c.BackColor = PhBotSelect;
            bool isTR = LocalizationManager.CurrentLanguage == "TR";
            if (_phBotBtnLaunch != null) _phBotBtnLaunch.Text = isTR ? "Clienti Başlat" : "Launch";
            if (_phBotBtnStart != null) _phBotBtnStart.Text = isTR ? "Botu Başlat" : "Start Bot";
            if (_phBotBtnClientless != null) _phBotBtnClientless.Text = isTR ? "Clienti Kapat" : "Kill Client";
            if (_phBotBtnStop != null) _phBotBtnStop.Text = isTR ? "Botu Durdur" : "Stop Bot";
            if (_phBotBtnHide != null) _phBotBtnHide.Text = isTR ? "Clienti Gizle" : "Hide Client";
            if (_phBotBtnReturn != null) _phBotBtnReturn.Text = isTR ? "Şehre Dön" : "Return Scroll";
            if (_phBotCopyright != null) _phBotCopyright.Text = "©2026 xBot";
            UpdatePhBotTitle();
            LayoutXBotReference();
        }

        // ApplyLanguageToWindow eski dikey buton metinlerini ezdiği için
        // sidebar başlıkları burada phBot kanonik isimlerine sabitlenir.
        private void RefreshSidebarTitles()
        {
            try
            {
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                var titles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                    { "TabPageV_Control01_ProjectHax", "xBot" },
                    { "TabPageV_Control01_GameInfo", isTR ? "İstatistikler" : "Statistics" },
                    { "TabPageV_Control01_Login", isTR ? "Silkroad Bağlantısı" : "Silkroad Login" },
                    { "TabPageV_Control01_AutoConfigure", isTR ? "Otomatik Yapılandırma" : "Auto Configure" },
                    { "TabPageV_Control01_TargetAssist", isTR ? "Yardımcı" : "Assistant" },
                    { "TabPageV_Control01_Notifications", isTR ? "Bildirimler" : "Notifications" },
                    { "TabPageV_Control01_Character", isTR ? "Koruma" : "Protection" },
                    { "TabPageV_Control01_Town", isTR ? "Şehir" : "Town" },
                    { "TabPageV_Control01_Training", isTR ? "Kasılma Alanı" : "Training Area" },
                    { "TabPageV_Control01_Skills", isTR ? "Saldırı" : "Attack" },
                    { "TabPageV_Control01_Pet", "Pet" },
                    { "TabPageV_Control01_Party", isTR ? "Parti" : "Party" },
                    { "TabPageV_Control01_UnionParty", isTR ? "Birlik Partisi" : "Union Party" },
                    { "TabPageV_Control01_PickFilter", isTR ? "Toplama Filtresi" : "Pick Filter" },
                    { "TabPageV_Control01_Quest", isTR ? "Görev" : "Quest" },
                    { "TabPageV_Control01_Players", isTR ? "Oyuncular" : "Players" },
                    { "TabPageV_Control01_Guild", isTR ? "Lonca" : "Guild" },
                    { "TabPageV_Control01_Academy", isTR ? "Akademi" : "Academy" },
                    { "TabPageV_Control01_Inventory", isTR ? "Envanter" : "Inventory" },
                    { "TabPageV_Control01_Stall", isTR ? "Tezgah" : "Stall" },
                    { "TabPageV_Control01_Trade", isTR ? "Kervan" : "Trade" },
                    { "TabPageV_Control01_Alchemy", isTR ? "Simya" : "Alchemy" },
                    { "TabPageV_Control01_Masteries", isTR ? "Ustalıklar" : "Masteries" },
                    { "TabPageV_Control01_Chat", isTR ? "Sohbet" : "Chat" },
                    { "TabPageV_Control01_Minimap", isTR ? "Harita" : "Map" },
                    { "TabPageV_Control01_Sound", isTR ? "Ses" : "Sound" },
                    { "TabPageV_Control01_KeyBindings", isTR ? "Kısayollar" : "Key Bindings" },
                    { "TabPageV_Control01_Conditions", isTR ? "Koşullar" : "Conditions" },
                    { "TabPageV_Control01_Settings", isTR ? "Eklentiler" : "Plugins" }
                };
                if (TabPageV_Control01 != null)
                {
                    foreach (Control c in TabPageV_Control01.Controls)
                    {
                        Button b = c as Button;
                        if (b == null) continue;
                        string t;
                        if (titles.TryGetValue(b.Name, out t))
                        {
                            try { if (b.Text != t) b.Text = t; } catch { }
                            try
                            {
                                Label icon = TabPageV_Control01.Controls[b.Name + "_Icon"] as Label;
                                if (icon != null) icon.Image = PhBotIcons.Get(t);
                            }
                            catch { }
                        }
                    }
                }
                // Protection > Return sekmesi dil değişiminde "Protection"a dönmesin.
                try
                {
                    if (TabPageH_Character_Option03 != null && TabPageH_Character_Option03.Text != "Return")
                        TabPageH_Character_Option03.Text = "Return";
                }
                catch { }
                // Hesap kombo ilk öğesi + gecikme birimi dile uysun.
                try
                {
                    if (Login_cmbxSavedAccounts != null && Login_cmbxSavedAccounts.Items.Count > 0)
                    {
                        Login_cmbxSavedAccounts.Items[0] = isTR ? "[Yeni / Özel Hesap]" : "[New / Custom Account]";
                    }
                }
                catch { }
                try
                {
                    if (lblLoginDelaySec != null)
                        lblLoginDelaySec.Text = isTR ? "sn" : "sec";
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("sidebar titles: " + ex.Message); }
        }

        private void StyleXBotReference(Control control)
        {
            if (control is QuestPanel) return; // Quest already supplies its light table theme.
            if (control.Name != null && (control.Name.StartsWith("ProjectHax_pnlCloud") || control.Name.StartsWith("ProjectHax_pnlSocial") || control.Name.StartsWith("ProjectHax_btnSocial_") || control.Name == "ProjectHax_btnCloud" || control.Name == "ProjectHax_lblCloudTitle" || control.Name == "ProjectHax_lblCloudSubtitle"))
                return;
            if (control.Parent != null && (control.Parent.Name == "ProjectHax_pnlCloud" || control.Parent.Name == "ProjectHax_pnlSocial"))
                return;
            control.Font = _referenceFont;
            control.ForeColor = Color.Black;
            control.BackColor = control == pnlWindow || control == _phBotActionsPanel || control == _referenceTools
                ? PhBotBg : Color.White;
            var button = control as Button;
            if (button != null)
            {
                bool sidebar = button.Parent == TabPageV_Control01;
                if (sidebar)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.BackColor = Color.White;
                }
                else if (button.Parent == _phBotActionsPanel || (button.Name != null && button.Name.StartsWith("ProjectHax_")))
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                    button.BackColor = Color.FromArgb(246, 247, 248);
                    button.ForeColor = Color.FromArgb(30, 30, 30);
                }
                else
                {
                    button.FlatStyle = FlatStyle.Standard;
                    button.UseVisualStyleBackColor = true;
                    button.BackColor = PhBotBg;
                }
                button.Padding = Padding.Empty;
            }
            var check = control as CheckBox;
            if (check != null) { check.FlatStyle = FlatStyle.Standard; check.Padding = Padding.Empty; }
            var radio = control as RadioButton;
            if (radio != null) { radio.FlatStyle = FlatStyle.Standard; radio.Padding = Padding.Empty; }
            var list = control as ListView;
            if (list != null) {
                list.OwnerDraw = false;
                list.BorderStyle = BorderStyle.FixedSingle;
                list.FullRowSelect = true;
                list.HideSelection = false;
                list.GridLines = list.Columns.Count > 1;
            }
            var input = control as TextBox;
            if (input != null) input.BorderStyle = BorderStyle.FixedSingle;
            var combo = control as ComboBox;
            if (combo != null) combo.FlatStyle = FlatStyle.Standard;
            foreach (Control child in control.Controls) StyleXBotReference(child);
        }

        private void LayoutXBotReference()
        {
            if (!_phBotClassicApplied || _referenceLayingOut) return;
            _referenceLayingOut = true;
            try
            {
                int w = pnlWindow.ClientSize.Width, h = pnlWindow.ClientSize.Height;
                int sidebar = 195;
                const int margin = 8;
                int x = 205;
                int contentW = Math.Max(500, w - x - margin);
                const int bottom = 85; // exact 3-row button grid height
                int logY = h - bottom - margin - 20;
                int contentH = logY - margin - 4;

                TabPageV_Control01.SetBounds(0, 0, sidebar, h);
                TabPageV_Control01.BorderStyle = BorderStyle.None;
                TabPageV_Control01.BackColor = Color.White;

                foreach (Control c in TabPageV_Control01.Controls)
                {
                    if (c is Button)
                    {
                        c.Left = 28;
                        c.Width = 148;
                    }
                    else if (c is Label)
                    {
                        c.Left = 8;
                        c.Width = 18;
                    }
                }
                UpdateSidebarItemPositions();

                foreach (Panel view in _referenceViews)
                {
                    view.SetBounds(x, margin, contentW, contentH);
                    foreach (TabControl tabs in view.Controls.OfType<TabControl>())
                    {
                        int left = (view == TabPageV_Control01_Skills_Panel) ? 185 : 0;
                        tabs.SetBounds(left, 0, contentW - left, contentH);
                    }
                    if (view == TabPageV_Control01_Skills_Panel && Skills_lstvSkills != null)
                    {
                        Skills_lstvSkills.SetBounds(0, 0, 180, contentH);
                    }
                }

                const int actionsW = 212;

                rtbxLogs.SetBounds(x, logY, contentW - actionsW - 10, bottom);
                rtbxLogs.BorderStyle = BorderStyle.FixedSingle;
                rtbxLogs.DetectUrls = false;
                rtbxLogs.BackColor = Color.White;
                rtbxLogs.ForeColor = Color.Black;
                rtbxLogs.Visible = true;
                rtbxLogs.BringToFront();

                if (_referenceTools != null) _referenceTools.Visible = false;
                _phBotActionsPanel.SetBounds(w - margin - actionsW, logY, actionsW, bottom + 24);
                Button[] actions = { _phBotBtnLaunch, _phBotBtnStart, _phBotBtnClientless, _phBotBtnStop, _phBotBtnHide, _phBotBtnReturn };
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i].SetBounds((i % 2) * 107, (i / 2) * 29, 103, 27);
                    actions[i].Font = new Font("Tahoma", 7.5f, FontStyle.Regular);
                    actions[i].FlatStyle = FlatStyle.Flat;
                    actions[i].FlatAppearance.BorderSize = 1;
                    actions[i].FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                    actions[i].BackColor = Color.FromArgb(246, 247, 248);
                    actions[i].ForeColor = Color.FromArgb(30, 30, 30);
                    actions[i].Padding = Padding.Empty;
                    actions[i].Margin = Padding.Empty;
                }
                _phBotCopyright.Text = "©2026 xBot";
                _phBotCopyright.SetBounds(0, 88, actionsW, 16);
                _phBotCopyright.TextAlign = ContentAlignment.MiddleCenter;
                _phBotCopyright.Font = new Font("Tahoma", 7.5f, FontStyle.Regular);
                _phBotCopyright.ForeColor = Color.FromArgb(60, 60, 60);
                LayoutXBotSkillPages();
            }
            finally { _referenceLayingOut = false; }
        }

        private void PrepareXBotReferenceContent()
        {
            // Existing settings groups keep their data bindings. A wrapping flow
            // provides natural reading order and scrolling on smaller windows.
            // Login ve Alchemy sekmeleri phBot koordinat düzenindedir (LayoutLoginInner, LayoutAlchemyInner);
            // akış yerleşimi tablo ve grup düzenini bozduğu için akışa alınmaz.
            FlowReferenceGroups(TabPageH_Character_Option01_Panel);
            FlowReferenceGroups(TabPageH_Character_Option04_Panel);
            PrepareXBotPotions();
            PrepareXBotTraining();
            PrepareXBotPickFilter();
            TabPageV_Control01_Town_Panel.Controls[TabPageH_Town_Option03.Name + "_Panel"].Visible = false;
            foreach (var entry in _referencePages)
            {
                var pagePanel = entry.Value.Controls.OfType<Panel>().FirstOrDefault();
                if (pagePanel == null || pagePanel == TabPageH_Skills_Option01_Panel || pagePanel == TabPageH_Skills_Option02_Panel) continue;
                var lists = pagePanel.Controls.OfType<ListView>().ToArray();
                if (lists.Length == 1 && lists[0].Width > 450)
                    lists[0].Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            }
            TabPageH_Skills_Option01_Panel.SizeChanged += (s, e) => LayoutXBotSkillPages();
            TabPageH_Skills_Option02_Panel.SizeChanged += (s, e) => LayoutXBotSkillPages();
        }

        private void PrepareXBotLogin()
        {
            var selector = new GroupBox { Name = "XBotServerSelection", Text = "Silkroad", Size = new Size(415, 80), Font = _referenceFont, BackColor = Color.White };
            foreach (Control c in new Control[] { Login_lblSilkroad, Login_cmbxSilkroad, Login_btnAddSilkroad })
                selector.Controls.Add(c);
            Login_lblSilkroad.SetBounds(12, 24, 108, 26);
            Login_cmbxSilkroad.SetBounds(124, 24, 232, 26);
            Login_btnAddSilkroad.SetBounds(364, 24, 32, 26);
            Login_btnAddSilkroad.Text = "...";
            TabPageV_Control01_Login_Panel.Controls.Add(selector);
            // Sort before the server list when the wrapping layout is built.
            selector.Location = new Point(0, -1);
            Login_gbxLogin.Size = new Size(350, 330);
            Login_lblAccount.SetBounds(12, 24, 320, 22);
            Login_cmbxSavedAccounts.SetBounds(12, 50, 320, 26);
            Button[] accountButtons = { Login_btnSaveAccount, Login_btnDeleteAccount, Login_btnAccountSetup };
            string[] captions = { "Save", "Delete", "Account Setup" };
            for (int i = 0; i < accountButtons.Length; i++)
            {
                accountButtons[i].Text = captions[i];
                accountButtons[i].SetBounds(12 + i * 108, 84, 102, 28);
            }
            Control[] labels = { Login_lblUsername, Login_lblPassword, Login_lblCaptcha, Login_lblServer, Login_lblCharacter };
            Control[] inputs = { Login_tbxUsername, Login_tbxPassword, Login_tbxCaptcha, Login_cmbxServer, Login_cmbxCharacter };
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].SetBounds(12, 128 + i * 36, 95, 26);
                inputs[i].SetBounds(114, 128 + i * 36, 218, 26);
            }
            // At most two columns at laptop sizes; each group remains readable.
            Login_gbxConnection.Size = new Size(350, 220);
        }

        private void PrepareXBotPickFilter()
        {
            if (lstPickItems == null) return;
            var page = tabPickFilterRoot.TabPages[0];
            var footer = new Panel { Name = "XBotPickSearch", Dock = DockStyle.Bottom, Height = 72, BackColor = Color.White };
            foreach (Control c in page.Controls.Cast<Control>().Where(c => c != lstPickItems).ToArray())
            {
                int y = c.Top - 204;
                c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                footer.Controls.Add(c);
                c.Top = Math.Max(0, y);
                if (c is Label && c.Text.Contains("SOX"))
                {
                    c.Size = new Size(Math.Max(200, footer.Width - 235), 36);
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                }
            }
            if (txtPickSearch != null)
            {
                txtPickSearch.Location = new Point(330, 4);
                txtPickSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
            page.Controls.Add(footer);
            page.Padding = new Padding(6);
            lstPickItems.Dock = DockStyle.Fill;
            lstPickItems.BringToFront();
            footer.Resize += (s, e) =>
            {
                if (txtPickSearch != null) txtPickSearch.Width = Math.Max(120, footer.Width - 336);
                var btnReset = footer.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Reset");
                var btnUpdate = footer.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Update");
                var btnClear = footer.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Clear");
                if (btnReset != null) { btnReset.Location = new Point(footer.Width - 76, 32); btnReset.BringToFront(); }
                if (btnUpdate != null) { btnUpdate.Location = new Point(footer.Width - 152, 32); btnUpdate.BringToFront(); }
                if (btnClear != null) { btnClear.Location = new Point(footer.Width - 228, 32); btnClear.BringToFront(); }
                var lblNote = footer.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("SOX"));
                if (lblNote != null)
                {
                    lblNote.Width = Math.Max(200, footer.Width - 235);
                    lblNote.SendToBack();
                }
            };
        }

        private void FlowReferenceGroups(Panel host)
        {
            var groups = host.Controls.Cast<Control>().Where(c => c is GroupBox || c is Theme.ModernCard)
                .OrderBy(c => c.Top).ThenBy(c => c.Left).ToArray();
            if (groups.Length == 0) return;
            var flow = new FlowLayoutPanel { Name = host.Name + "_Groups", Dock = DockStyle.Fill,
                AutoScroll = true, BackColor = Color.White, Padding = new Padding(8), WrapContents = true };
            foreach (Control group in groups)
            {
                if (group == Login_gbxAdvertising || group == Login_gbxCharacters) continue;
                group.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                group.Margin = new Padding(0, 0, 14, 14);
                flow.Controls.Add(group);
            }
            host.Controls.Add(flow);
            flow.BringToFront();
        }

        private void PrepareXBotPotions()
        {
            // phBot birebir iç düzen Window.PhBotInner.LayoutPotionsInner'a taşındı
            // (17 satır + yüzde/gecikme kutuları). Çift yerleşimi engellemek için
            // burası bilerek boştur.
            return;
        }

        private void PreparePotionCaptions()
        {
            // Bkz. PrepareXBotPotions.
            return;
        }

        private void PrepareXBotTraining()
        {
            // phBot birebir iç düzen Window.PhBotInner2.LayoutTrainingAreaInner'a taşındı.
            // Çift yerleşimi ve koordinat ezilmesini engellemek için burası bilerek boştur.
            return;
        }

        private void LayoutXBotSkillPages()
        {
            // phBot birebir iç düzen Window.PhBotInner.LayoutAttackInner /
            // LayoutBuffsInner'a taşındı (tam sağ sütun + Imbue). Çift
            // yerleşimi engellemek için burası bilerek boştur.
            return;
        }
    }
}
