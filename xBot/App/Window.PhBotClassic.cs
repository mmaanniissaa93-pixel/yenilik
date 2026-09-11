using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using xBot.Game;

namespace xBot.App
{
    /// <summary>
    /// phBot v27.x birebir klasik arayüz klonu.
    /// Referans: guide.phbot.org ekran görüntüleri (Attack, Buffs, Protection/Potions,
    /// Town/Buy, Pick Filter). Amaç: sol ikonlu beyaz sidebar, üstte klasik sub-tablar,
    /// altta beyaz log + sağda 6'lı buton grid (Launch/Start Bot/Go Clientless/Stop Bot/
    /// Hide Client/Return Scroll) ve açık gri (SystemColors.Control) klasik WinForms görünümü.
    /// Sadece görsel düzen + yeniden adlandırma yapar, bot mantığına dokunmaz.
    /// </summary>
    public partial class Window
    {
        public static bool UsePhBotClassic = true;

        private static bool? _phBotTestMode;
        internal static bool PhBotTestMode
        {
            get
            {
                if (_phBotTestMode == null)
                {
                    bool t = false;
                    try
                    {
                        foreach (string a in Environment.GetCommandLineArgs())
                        {
                            if (a != null && a.Equals("--phbot-test", StringComparison.OrdinalIgnoreCase))
                            {
                                t = true;
                                break;
                            }
                        }
                    }
                    catch { }
                    _phBotTestMode = t;
                }
                return _phBotTestMode.Value;
            }
        }

        private bool _phBotClassicApplied = false;
        private Panel _phBotActionsPanel;
        private Button _phBotBtnLaunch;
        private Button _phBotBtnStart;
        private Button _phBotBtnClientless;
        private Button _phBotBtnStop;
        private Button _phBotBtnHide;
        private Button _phBotBtnReturn;
        private Label _phBotCopyright;

        private static readonly Color PhBotBg = Color.FromArgb(243, 243, 243);
        private static readonly Color PhBotPanelWhite = Color.White;
        private static readonly Color PhBotSelect = Color.FromArgb(245, 245, 245);
        private static readonly Color PhBotHover = Color.FromArgb(249, 249, 249);
        private static readonly Color PhBotTabInactive = Color.FromArgb(232, 232, 232);
        private static readonly Color PhBotBorder = Color.FromArgb(215, 218, 222);

        private static Font PhBotFont()
        {
            try { return new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point); }
            catch { return SystemFonts.DefaultFont; }
        }

        private void PhBotDebug(string msg)
        {
            try
            {
                System.IO.File.AppendAllText(
                    @"C:\Users\auguu\Desktop\xBot-WinForms\docs\phbot_ref\phbot_debug.txt",
                    DateTime.Now.ToString("HH:mm:ss") + " " + msg + Environment.NewLine);
            }
            catch { }
        }

        private void DumpViewState(string view) { }

        private void AutoFitListView(ListView lv)
        {
            if (lv == null || lv.Columns.Count == 0) return;
            try
            {
                int totalWidth = lv.ClientSize.Width;
                if (totalWidth <= 0) return;
                if (lv.Columns.Count == 1)
                {
                    lv.Columns[0].Width = Math.Max(50, totalWidth - 4);
                    return;
                }
                int otherColumnsWidth = 0;
                for (int i = 0; i < lv.Columns.Count - 1; i++)
                    otherColumnsWidth += lv.Columns[i].Width;
                int remaining = totalWidth - otherColumnsWidth - 4;
                if (remaining > 50)
                    lv.Columns[lv.Columns.Count - 1].Width = remaining;
            }
            catch { }
        }

        /// <summary>
        /// Constructor sırasında çalışmış olabilecek ModernTheme etkilerini geri alır:
        /// modern sidebar gizlenir, GroupBox'lara eklenen koyu kart Paint hook'ları
        /// sökülüp başlıklar geri yüklenir, koyu özel kontroller (ModernCard/Toggle/
        /// Button/TextBox) klasik karşılıklarına çevrilir.
        /// </summary>
        private void NeutralizeModernTheme()
        {
            try { if (TabPageV_Control01 != null) TabPageV_Control01.Visible = true; } catch { }
        }

        public void ApplyPhBotClassicTheme()
        {
            ApplyXBotReferenceLayout();
        }

        private Timer _phBotCapTimer;
        private int _phBotCapStep;

        private void PhBotSelfCapture_Shown(object sender, EventArgs e)
        {
            try { Shown -= PhBotSelfCapture_Shown; } catch { }
            _phBotCapStep = 0;
            _phBotCapTimer = new Timer();
            _phBotCapTimer.Interval = 1500;
            _phBotCapTimer.Tick += PhBotSelfCapture_Tick;
            _phBotCapTimer.Start();
        }

        private void PhBotSelfCapture_Tick(object sender, EventArgs e)
        {
            try
            {
                string dir = @"C:\Users\auguu\Desktop\xBot-WinForms\docs\phbot_ref\";
                string[] views = new string[] { "SilkroadLogin", "Attack", "AttackResurrect", "Protection", "Town", "TrainingArea", "Party", "Inventory", "PickFilter" };
                if (_phBotCapStep < views.Length)
                {
                    string v = views[_phBotCapStep];
                    try
                    {
                        if (v == "AttackResurrect")
                        {
                            Control atk = TabPageV_Control01.Controls["TabPageV_Control01_Attack"];
                            if (atk != null) TabPageV_Option_Click(atk, EventArgs.Empty);
                            Control res = null;
                            try
                            {
                                foreach (Control s in TabPageH_Skills_Option01.Parent.Controls)
                                {
                                    if (s.Name == "TabPageH_Skills_OptionResurrect") { res = s; break; }
                                }
                            }
                            catch { }
                            if (res != null) TabPageH_Option_Click(res, EventArgs.Empty);
                        }
                        else
                        {
                            Control btn = TabPageV_Control01.Controls["TabPageV_Control01_" + v];
                            if (btn != null) TabPageV_Option_Click(btn, EventArgs.Empty);
                        }
                        if (v == "Attack" && TabPageH_Skills_Option01 != null)
                            TabPageH_Option_Click(TabPageH_Skills_Option01, EventArgs.Empty);
                        if (v == "Town" && TabPageH_Town_Option01 != null)
                            TabPageH_Option_Click(TabPageH_Town_Option01, EventArgs.Empty);
                        if (v == "Protection" && TabPageH_Character_Option02 != null)
                            TabPageH_Option_Click(TabPageH_Character_Option02, EventArgs.Empty);
                        try
                        {
                            foreach (Control cc in pnlWindow.Controls)
                            {
                                Panel pp = cc as Panel;
                                if (pp != null && pp.Name == "TabPageV_Control01_" + v + "_Panel")
                                {
                                    pp.BringToFront();
                                    break;
                                }
                            }
                        }
                        catch { }
                        Refresh();
                        Application.DoEvents();
                        try { DumpViewState(v); } catch { }
                        try
                        {
                            if (v == "Town")
                            {
                                DumpPanelDeep("TabPageH_Town", 1);
                                DumpPanelDeep("TabPageH_Town_Option01_Panel", 1);
                            }
                            if (v == "Attack") DumpPanelDeep("TabPageH_Skills", 1);
                            if (v == "PickFilter") DumpPanelDeep("PhBot_PickFilterTabs", 2);
                        }
                        catch { }
                        using (Bitmap bmp = new Bitmap(ClientSize.Width, ClientSize.Height))
                        {
                            DrawToBitmap(bmp, new Rectangle(Point.Empty, ClientSize));
                            bmp.Save(dir + "self_" + v + ".png",
                                System.Drawing.Imaging.ImageFormat.Png);
                        }
                        try { PhBotCaptureScreen(v, dir); } catch (Exception ex) { PhBotDebug("screen outer " + v + ": " + ex.Message); }
                    }
                    catch (Exception ex) { PhBotDebug("cap " + v + ": " + ex.Message); }
                    _phBotCapStep++;
                }
                else
                {
                    _phBotCapTimer.Stop();
                    try
                    {
                        Control def = TabPageV_Control01.Controls["TabPageV_Control01_SilkroadLogin"];
                        if (def != null) TabPageV_Option_Click(def, EventArgs.Empty);
                    }
                    catch { }
                    PhBotDebug("capture done");
                    if (PhBotTestMode)
                    {
                        // Test örneği kendi kendine kapanır, ortalıkta kalmaz
                        try
                        {
                            var exitTimer = new Timer();
                            exitTimer.Interval = 800;
                            exitTimer.Tick += delegate
                            {
                                try { exitTimer.Stop(); } catch { }
                                try { Application.Exit(); } catch { }
                            };
                            exitTimer.Start();
                        }
                        catch { try { Application.Exit(); } catch { } }
                    }
                }
            }
            catch { }
        }

        private void PhBotCaptureScreen(string v, string dir)
        {
            try
            {
                bool wasTop = TopMost;
                TopMost = true;
                Activate();
                BringToFront();
                Refresh();
                Application.DoEvents();
                System.Threading.Thread.Sleep(450);
                Rectangle r = RectangleToScreen(ClientRectangle);
                if (r.Width > 0 && r.Height > 0)
                {
                    using (Bitmap bmp = new Bitmap(r.Width, r.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.CopyFromScreen(r.Location, Point.Empty, r.Size);
                        }
                        bmp.Save(dir + "selfscreen_" + v + ".png",
                            System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                TopMost = wasTop;
            }
            catch (Exception ex) { PhBotDebug("screen " + v + ": " + ex.Message); }
        }

        private void WindowPhBotClassic_SizeChanged(object sender, EventArgs e)
        {
            if (!_phBotClassicApplied) return;
            try { LayoutPhBotClassic(); } catch { }
            try { LayoutTrainingAreaClassic(); } catch { }
        }

        private void UpdatePhBotTitle(string customStatus = null)
        {
            try
            {
                this.InvokeIfRequired(() => {
                    bool isTR = LocalizationManager.CurrentLanguage == "TR";
                    string charName = "N/A";
                    try
                    {
                        if (InfoManager.Character != null && !string.IsNullOrEmpty(InfoManager.Character.Name))
                            charName = InfoManager.Character.Name;
                    }
                    catch { }

                    string status;
                    if (!string.IsNullOrEmpty(customStatus))
                    {
                        status = customStatus;
                    }
                    else
                    {
                        bool isConnected = false;
                        try { isConnected = InfoManager.inGame || (Bot.Get != null && Bot.Get.isBotting); } catch { }
                        status = isConnected ? (isTR ? "Bağlandı" : "Connected") : (isTR ? "Bağlantı kesildi" : "Disconnected");
                    }
                    this.Text = string.Format("xBot - {0} - {1}", charName, status);
                });
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // Sidebar
        // ---------------------------------------------------------------
        private void RebuildPhBotSidebar()
        {
            if (TabPageV_Control01 == null || pnlWindow == null) return;

            var existing = new Dictionary<string, Panel>(StringComparer.OrdinalIgnoreCase);
            try
            {
                AddExisting(existing, "Statistics", TabPageV_Control01_GameInfo_Panel);
                AddExisting(existing, "Silkroad Login", TabPageV_Control01_Login_Panel);
                AddExisting(existing, "Protection", TabPageV_Control01_Character_Panel);
                AddExisting(existing, "Town", TabPageV_Control01_Town_Panel);
                AddExisting(existing, "Training Area", TabPageV_Control01_Training_Panel);
                AddExisting(existing, "Attack", TabPageV_Control01_Skills_Panel);
                AddExisting(existing, "Party", TabPageV_Control01_Party_Panel);
                AddExisting(existing, "Players", TabPageV_Control01_Players_Panel);
                AddExisting(existing, "Guild", TabPageV_Control01_Guild_Panel);
                AddExisting(existing, "Academy", TabPageV_Control01_Academy_Panel);
                AddExisting(existing, "Inventory", TabPageV_Control01_Inventory_Panel);
                AddExisting(existing, "Stall", TabPageV_Control01_Stall_Panel);
                AddExisting(existing, "Chat", TabPageV_Control01_Chat_Panel);
                AddExisting(existing, "Map", TabPageV_Control01_Minimap_Panel);
                AddExisting(existing, "Plugins", TabPageV_Control01_Settings_Panel);
                try
                {
                    if (TabPageV_Control01_Alchemy_Panel != null)
                        AddExisting(existing, "Alchemy", TabPageV_Control01_Alchemy_Panel);
                }
                catch { }
            }
            catch { }

            // phBot sırası (ekran görüntülerinden birebir)
            string[] order = new string[]
            {
                "ProjectHax", "Statistics", "Silkroad Login", "Auto Configure",
                "Notifications", "Protection", "Town", "Training Area",
                "Attack", "Pet", "Party", "Union Party",
                "Pick Filter", "Quest", "Players", "Guild",
                "Academy", "Inventory", "Stall", "Trade",
                "Alchemy", "Masteries", "Chat", "Map",
                "Sound", "Key Bindings", "Conditions", "Plugins"
            };

            TabPageV_Control01.SuspendLayout();
            TabPageV_Control01.Controls.Clear();
            TabPageV_Control01.BackColor = PhBotPanelWhite;
            TabPageV_Control01.ForeColor = Color.Black;
            TabPageV_Control01.BorderStyle = BorderStyle.FixedSingle;
            TabPageV_Control01.Tag = null;
            try { TabPageV_Control01.AutoScroll = true; } catch { }

            TabPageV_ColorSelected = PhBotSelect;
            TabPageV_ColorHover = PhBotHover;

            int y = 2;
            const int itemH = 26;
            foreach (string key in order)
            {
                bool isPx = (key == "ProjectHax");
                Panel panel = null;
                if (!existing.TryGetValue(key, out panel) || panel == null)
                {
                    panel = BuildPhBotPlaceholderPanel(key);
                }
                // Aynı isimde eski/kör panel varsa kaldır (isim çakışması TabPageV_Option_Click'i bozar)
                try
                {
                    string canon = "TabPageV_Control01_" + SanitizeName(key) + "_Panel";
                    var dupes = new List<Control>();
                    foreach (Control cc in pnlWindow.Controls)
                    {
                        if (cc is Panel && cc.Name == canon && cc != panel)
                            dupes.Add(cc);
                    }
                    foreach (Control d in dupes)
                    {
                        try { pnlWindow.Controls.Remove(d); } catch { }
                        d.Name = canon + "_Legacy";
                    }
                }
                catch { }

                panel.Visible = false;
                panel.BackColor = PhBotBg;
                try { panel.BorderStyle = BorderStyle.FixedSingle; } catch { }
                if (panel.Parent != pnlWindow)
                {
                    try { pnlWindow.Controls.Add(panel); } catch { }
                }
                panel.Name = "TabPageV_Control01_" + SanitizeName(key) + "_Panel";

                string baseName = "TabPageV_Control01_" + SanitizeName(key);

                var lblIcon = new Label();
                lblIcon.Name = baseName + "_Icon";
                lblIcon.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point);
                lblIcon.ForeColor = Color.FromArgb(60, 60, 60);
                lblIcon.BackColor = PhBotPanelWhite;
                lblIcon.TextAlign = ContentAlignment.MiddleCenter;
                lblIcon.Location = new Point(2, y);
                lblIcon.Size = new Size(28, itemH);
                lblIcon.Cursor = Cursors.Hand;
                if (isPx)
                {
                    lblIcon.Text = "Px";
                }
                else
                {
                    lblIcon.Text = "";
                    try
                    {
                        Image glyph = PhBotIcons.Get(key);
                        if (glyph != null)
                        {
                            lblIcon.Image = glyph;
                            lblIcon.ImageAlign = ContentAlignment.MiddleCenter;
                        }
                        else
                        {
                            lblIcon.Text = "•";
                            lblIcon.Font = new Font("Tahoma", 10f, FontStyle.Regular, GraphicsUnit.Point);
                        }
                    }
                    catch { lblIcon.Text = "•"; }
                }

                var btn = new Button();
                btn.Name = baseName;
                btn.Text = key;
                btn.Font = PhBotFont();
                btn.ForeColor = Color.Black;
                btn.BackColor = PhBotPanelWhite;
                btn.FlatStyle = FlatStyle.Flat;
                try { btn.FlatAppearance.BorderSize = 0; } catch { }
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Location = new Point(28, y);
                btn.Size = new Size(134, itemH);
                btn.Cursor = Cursors.Hand;
                btn.UseVisualStyleBackColor = false;
                btn.Tag = panel;

                lblIcon.Click += TabPageV_Option_Click;
                btn.Click += TabPageV_Option_Click;
                lblIcon.MouseEnter += TabPageV_Option_MouseEnter;
                btn.MouseEnter += TabPageV_Option_MouseEnter;
                lblIcon.MouseLeave += TabPageV_Option_MouseLeave;
                btn.MouseLeave += TabPageV_Option_MouseLeave;

                TabPageV_Control01.Controls.Add(lblIcon);
                TabPageV_Control01.Controls.Add(btn);

                y += itemH;
            }

            // Eski referans butonlar silindi; TabPageH_Option_Click için gerekli
            // yatay sekmeler panellerin içinde duruyor, etkilenmez.
            TabPageV_Control01.ResumeLayout(false);

            // Varsayılan seçim: Silkroad Login (eski davranışla aynı)
            try
            {
                Control def = TabPageV_Control01.Controls["TabPageV_Control01_SilkroadLogin"];
                if (def != null) TabPageV_Option_Click(def, EventArgs.Empty);
            }
            catch (Exception ex) { PhBotDebug("select: " + ex.Message); }
            PhBotDebug("sidebar rebuilt: " + TabPageV_Control01.Controls.Count);
        }

        private static void AddExisting(Dictionary<string, Panel> map, string key, Panel panel)
        {
            if (panel != null && !map.ContainsKey(key))
                map[key] = panel;
        }

        private static string SanitizeName(string s)
        {
            string r = "";
            foreach (char c in s)
                r += (char.IsLetterOrDigit(c) ? c.ToString() : "");
            return string.IsNullOrEmpty(r) ? "Item" : r;
        }

        private Panel BuildPhBotPlaceholderPanel(string key)
        {
            var panel = new Panel();
            panel.Name = "TabPageV_Control01_" + SanitizeName(key) + "_Panel";
            panel.BackColor = PhBotBg;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.AutoScroll = true;

            if (key == "Pick Filter")
                return BuildPhBotPickFilterPanel(panel);
            if (key == "Pet")
                return BuildPhBotPetPanel(panel);
            if (key == "Union Party")
                return BuildPhBotUnionPanel(panel);
            if (key == "ProjectHax")
                return BuildPhBotProjectHaxPanel(panel);
            if (key == "Auto Configure")
                return BuildPhBotAutoConfigurePanel(panel);
            if (key == "Notifications" || key == "Sound")
                return BuildPhBotNotificationsPanel(panel, key);
            if (key == "Quest")
                return BuildPhBotQuestPanel(panel);
            if (key == "Trade")
                return BuildPhBotTradePanel(panel);
            if (key == "Masteries")
                return BuildPhBotMasteriesPanel(panel);
            if (key == "Key Bindings" || key == "Conditions")
                return BuildPhBotSimpleListPanel(panel, key);

            var info = new Label();
            info.Text = key + "  •  phBot klasik görünüm";
            info.Font = new Font(PhBotFont(), FontStyle.Bold);
            info.ForeColor = Color.Black;
            info.BackColor = Color.Transparent;
            info.AutoSize = true;
            info.Location = new Point(10, 8);
            panel.Controls.Add(info);

            var desc = new Label();
            desc.Text = "Bu bölüm phBot ile aynı yerleşimle gösterilir. İlgili xBot yöneticisi buraya bağlanır.";
            desc.Font = PhBotFont();
            desc.ForeColor = Color.FromArgb(60, 60, 60);
            desc.AutoSize = true;
            desc.Location = new Point(10, 30);
            panel.Controls.Add(desc);
            return panel;
        }

        /// <summary>
        /// Klasik sekme görünümü: seçili H sekmesi beyaz, diğerleri açık gri.
        /// TabPageH_Option_Click sonradan renkleri ezdiği için ayrıca Click'e takılır.
        /// </summary>
        private void PhBotHTabVisual_Click(object sender, EventArgs e)
        {
            try
            {
                Button b = sender as Button;
                if (b == null || b.Parent == null) return;
                foreach (Control s in b.Parent.Controls)
                {
                    Button sb = s as Button;
                    if (sb != null && sb.Name.StartsWith("TabPageH_") && !sb.Name.EndsWith("_Panel"))
                        sb.BackColor = (sb == b) ? Color.White : PhBotTabInactive;
                }
            }
            catch { }
        }

        private void FixHTabVisuals()
        {
            try
            {
                var strips = new List<Control>();
                ForEachControl(pnlWindow, delegate(Control c)
                {
                    Button b = c as Button;
                    if (b != null && b.Name.StartsWith("TabPageH_") && !b.Name.EndsWith("_Panel"))
                    {
                        // Gerçek TabControl ile sarılmış şeritler native görünür; dokunma.
                        try { if (b.Parent != null && b.Parent.Tag is TabControl) return; } catch { }
                        try { b.Click -= PhBotHTabVisual_Click; } catch { }
                        b.Click += PhBotHTabVisual_Click;
                        if (b.Parent != null && !strips.Contains(b.Parent))
                            strips.Add(b.Parent);
                    }
                });
                foreach (Control strip in strips)
                {
                    try
                    {
                        Control sel = strip.Tag as Control;
                        foreach (Control s in strip.Controls)
                        {
                            Button sb = s as Button;
                            if (sb != null && sb.Name.StartsWith("TabPageH_") && !sb.Name.EndsWith("_Panel"))
                            {
                                // Modern tema bazı yatay sekmeleri gizlemiş olabilir; klasikte hepsi görünür
                                // (Town>Toplama Filtresi Pick Filter'a taşındığı için gizli kalır).
                                if (sb.Name != "TabPageH_Town_Option03")
                                    sb.Visible = true;
                                sb.BackColor = (sb == sel) ? Color.White : PhBotTabInactive;
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex) { PhBotDebug("htab: " + ex.Message); }
        }

        private static void ForEachControl(Control parent, Action<Control> act)
        {
            if (parent == null || act == null) return;
            foreach (Control c in parent.Controls)
            {
                try { act(c); } catch { }
                if (c.Controls.Count > 0)
                    ForEachControl(c, act);
            }
        }

        private void PhBotDiagCounts(string stage)
        {
            try
            {
                int gbx = 0, cards = 0, toggles = 0, mbtns = 0, mtxt = 0, lvOwner = 0, lvTotal = 0, darkPanels = 0;
                ForEachControl(pnlWindow, delegate(Control c)
                {
                    if (c is GroupBox) gbx++;
                    string fn = "";
                    try { fn = c.GetType().FullName; } catch { }
                    if (fn == "xBot.App.Theme.ModernCard") cards++;
                    else if (fn == "xBot.App.Theme.ModernToggle") toggles++;
                    else if (fn == "xBot.App.Theme.ModernButton") mbtns++;
                    else if (fn == "xBot.App.Theme.ModernTextBox") mtxt++;
                    ListView lv = c as ListView;
                    if (lv != null)
                    {
                        lvTotal++;
                        try { if (lv.OwnerDraw) lvOwner++; } catch { }
                    }
                    Panel p = c as Panel;
                    if (p != null && p != pnlWindow && p != _phBotActionsPanel)
                    {
                        try { if (p.BackColor.GetBrightness() < 0.35f) darkPanels++; } catch { }
                    }
                });
                PhBotDebug(stage + " gbx=" + gbx + " cards=" + cards + " toggles=" + toggles +
                    " mbtns=" + mbtns + " mtxt=" + mtxt + " lv=" + lvTotal + "/" + lvOwner +
                    " darkPanels=" + darkPanels);
            }
            catch { }
        }

        private void DumpPaintHooks()
        {
            try
            {
                object paintKey = null;
                foreach (Type t in new Type[] { typeof(Control), typeof(GroupBox), typeof(Panel), typeof(ButtonBase) })
                {
                    try
                    {
                        var kf = t.GetField("EventPaint",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy);
                        if (kf != null) { paintKey = kf.GetValue(null); break; }
                    }
                    catch { }
                }
                PhBotDebug("paintkey=" + (paintKey == null ? "null" : "found"));
                if (paintKey == null) return;
                var evProp = typeof(System.ComponentModel.Component).GetProperty("Events",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                ForEachControl(pnlWindow, delegate(Control c)
                {
                    try
                    {
                        if (!(c is GroupBox) && !(c is Panel) && !(c is RadioButton) && !(c is CheckBox))
                            return;
                        var list = evProp.GetValue(c, null) as System.ComponentModel.EventHandlerList;
                        Delegate d = list != null ? list[paintKey] : null;
                        if (d != null)
                        {
                            string methods = "";
                            try
                            {
                                foreach (var h in d.GetInvocationList())
                                {
                                    try { methods += (h.Method.DeclaringType != null ? h.Method.DeclaringType.Name : "?") + "." + h.Method.Name + ";"; }
                                    catch { }
                                }
                            }
                            catch { }
                            PhBotDebug("HOOK: " + c.GetType().Name + " " + c.Name + " n=" + d.GetInvocationList().Length + " " + methods);
                        }
                    }
                    catch { }
                });
                // Alan referansları görünen örneklerle aynı mı?
                try
                {
                    ForEachControl(pnlWindow, delegate(Control c)
                    {
                        try
                        {
                            GroupBox g = c as GroupBox;
                            if (g == null || g.Name == null || !g.Name.StartsWith("Login_gbx")) return;
                            bool isField = object.ReferenceEquals(g, Login_gbxConnection) ||
                                object.ReferenceEquals(g, Login_gbxLogin) ||
                                object.ReferenceEquals(g, Login_gbxServers) ||
                                object.ReferenceEquals(g, Login_gbxCharacters) ||
                                object.ReferenceEquals(g, Login_gbxAdvertising);
                            PhBotDebug("loginbox: " + g.Name + " isField=" + isField + " hash=" + g.GetHashCode() +
                                " parent=" + (g.Parent != null ? g.Parent.Name : "?"));
                        }
                        catch { }
                    });
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("dumphooks: " + ex.Message); }
        }

        private void DumpPanelDeep(string panelName, int maxDepth)
        {
            try
            {
                Control root = null;
                ForEachControl(pnlWindow, delegate(Control c)
                {
                    if (root == null && c.Name == panelName)
                        root = c;
                });
                if (root == null) { PhBotDebug("deep " + panelName + ": NOT FOUND"); return; }
                DumpDeepInner(root, 0, maxDepth);
            }
            catch (Exception ex) { PhBotDebug("deep: " + ex.Message); }
        }

        private void DumpDeepInner(Control c, int depth, int maxDepth)
        {
            int count = -1;
            try { count = c.Controls.Count; } catch (Exception ex) { PhBotDebug("count fail: " + ex.GetType().Name); }
            try
            {
                string extra = "";
                try
                {
                    Button b = c as Button;
                    if (b != null) extra = " text=[" + b.Text + "]";
                    ComboBox cb = c as ComboBox;
                    if (cb != null) extra = " items=" + cb.Items.Count + " text=[" + cb.Text + "]";
                    TextBox tb = c as TextBox;
                    if (tb != null) extra = " text=[" + tb.Text + "]";
                }
                catch { }
                PhBotDebug(new string(' ', depth * 2) + c.GetType().Name + " " + c.Name +
                    " vis=" + c.Visible + " count=" + count + " disposed=" + c.IsDisposed +
                    " loc=" + c.Location + " size=" + c.Size +
                    " bg=" + c.BackColor.ToArgb().ToString("X8") + extra);
                if (depth < maxDepth && count > 0)
                {
                    for (int i = 0; i < c.Controls.Count; i++)
                    {
                        try { DumpDeepInner(c.Controls[i], depth + 1, maxDepth); }
                        catch (Exception ex) { PhBotDebug("child dump fail idx=" + i + ": " + ex.GetType().Name + " " + ex.Message); }
                    }
                }
            }
            catch (Exception ex) { PhBotDebug("dumpinner fail: " + ex.GetType().Name + " " + ex.Message); }
        }

        private void DumpDuplicatePanels()
        {
            try
            {
                var counts = new Dictionary<string, int>();
                var hashes = new Dictionary<string, string>();
                ForEachControl(pnlWindow, delegate(Control c)
                {
                    try
                    {
                        Panel p = c as Panel;
                        if (p == null) return;
                        if (!p.Name.StartsWith("TabPageV_Control01_") || !p.Name.EndsWith("_Panel")) return;
                        if (!counts.ContainsKey(p.Name)) { counts[p.Name] = 0; hashes[p.Name] = ""; }
                        counts[p.Name]++;
                        hashes[p.Name] += p.GetHashCode() + "(vis=" + p.Visible + ",kids=" + p.Controls.Count + ") ";
                    }
                    catch { }
                });
                foreach (var kv in counts)
                    PhBotDebug("panelname: " + kv.Key + " n=" + kv.Value + " " + hashes[kv.Key]);
            }
            catch (Exception ex) { PhBotDebug("dupdump: " + ex.Message); }
        }

        private void LayoutSkillsStrip()
        {
            try
            {
                if (TabPageH_Skills_Option01 == null || TabPageH_Skills_Option01.Parent == null) return;
                Control strip = TabPageH_Skills_Option01.Parent;
                try { if (strip != null && strip.Tag is TabControl) return; } catch { }
                var ordered = new List<Button>();
                string[] want = new string[]
                {
                    "TabPageH_Skills_Option01", "TabPageH_Skills_Option02", "TabPageH_Skills_Option03",
                    "TabPageH_Skills_OptionPartyOptions", "TabPageH_Skills_OptionResurrect",
                    "TabPageH_Skills_OptionHealing", "TabPageH_Skills_OptionLure",
                    "TabPageH_Skills_OptionAttackLog"
                };
                foreach (string n in want)
                {
                    try
                    {
                        Control f = null;
                        foreach (Control s in strip.Controls)
                        {
                            if (s.Name == n) { f = s; break; }
                        }
                        Button b = f as Button;
                        if (b != null) ordered.Add(b);
                    }
                    catch { }
                }
                int x = 2;
                int top = TabPageH_Skills_Option01.Top;
                foreach (Button b in ordered)
                {
                    try
                    {
                        int w = TextRenderer.MeasureText(b.Text, b.Font).Width + 12;
                        if (w < 56) w = 56;
                        b.Location = new Point(x, top);
                        b.Size = new Size(w, 23);
                        b.Visible = true;
                        x += w + 2;
                    }
                    catch { }
                }
                try
                {
                    Panel sp = strip as Panel;
                    if (sp != null) sp.AutoScroll = false;
                }
                catch { }
                PhBotDebug("skills strip laid out: " + ordered.Count + " tabs, width=" + x);
            }
            catch (Exception ex) { PhBotDebug("skillsstrip: " + ex.Message); }
        }

        // ---------------------------------------------------------------
        // Sub-tab isimleri (birebir phBot)
        // ---------------------------------------------------------------
        private void RenamePhBotSubTabs(Button b, string text)
        {
            if (b == null) return;
            b.Text = text;
            b.Font = PhBotFont();
            b.ForeColor = Color.Black;
        }

        private void RenamePhBotSubTabs()
        {
            try
            {
                // Attack -> Skills paneli: phBot sırası
                // Attack | Buffs | Party | Party Options | Resurrect | Healing | Lure | Attack Log
                RenamePhBotSubTabs(TabPageH_Skills_Option01, "Attack");
                RenamePhBotSubTabs(TabPageH_Skills_Option02, "Buffs");
                RenamePhBotSubTabs(TabPageH_Skills_Option03, "Party");
            }
            catch { }
            try
            {
                // Protection -> Character paneli
                // Potions | Sockets | Return | Pet Return | Berserk | Monster Preferences | Devil's Spirit | Scrolls | Stat Points | Misc.
                RenamePhBotSubTabs(TabPageH_Character_Option01, "Info");
                RenamePhBotSubTabs(TabPageH_Character_Option02, "Potions");
                RenamePhBotSubTabs(TabPageH_Character_Option03, "Return");
                RenamePhBotSubTabs(TabPageH_Character_Option04, "Misc.");
            }
            catch { }
            try
            {
                // Town: Buy | Options
                RenamePhBotSubTabs(TabPageH_Town_Option01, "Buy");
                RenamePhBotSubTabs(TabPageH_Town_Option02, "Options");
                if (TabPageH_Town_Option03 != null) TabPageH_Town_Option03.Visible = false;
            }
            catch { }
            try
            {
                // Training: Training Area | Script | Trace | Combat (xBot) + Conditions/Collision/Settings (phBot).
                // Trace içeriği Option03'te durur; etiketi Conditions yapmak içeriği
                // yanlış etiketlerdi, o yüzden Trace korunur (EnsurePhBotTrainingExtraTabs
                // Conditions/Collision/Settings'i ayrı sekmeler olarak ekler).
                RenamePhBotSubTabs(TabPageH_Training_Option01, "Training");
                RenamePhBotSubTabs(TabPageH_Training_Option02, "Script");
                RenamePhBotSubTabs(TabPageH_Training_Option03, "Trace");
            }
            catch { }
            try
            {
                // Party: Party | Options | Matching | Taxi | Taxi Options
                RenamePhBotSubTabs(TabPageH_Party_Option01, "Party");
                RenamePhBotSubTabs(TabPageH_Party_Option02, "Options");
                RenamePhBotSubTabs(TabPageH_Party_Option03, "Matching");
                RenamePhBotSubTabs(TabPageH_Party_Option04, "Taxi");
            }
            catch { }
            try
            {
                // Inventory: 01 eşya, 03 depo, 04 pet listeleridir. 02 avatar
                // listesidir ("Exchange" etiketi içerikle uyuşmuyordu);
                // gerçek Exchange/JobTickets/Item/Gori sekmeleri
                // EnsurePhBotInventoryExtraTabs ile eklenir.
                RenamePhBotSubTabs(TabPageH_Inventory_Option01, "Inventory");
                RenamePhBotSubTabs(TabPageH_Inventory_Option02, "Avatar");
                RenamePhBotSubTabs(TabPageH_Inventory_Option03, "Storage");
                RenamePhBotSubTabs(TabPageH_Inventory_Option04, "Pet");
            }
            catch { }
            try
            {
                RenamePhBotSubTabs(TabPageH_Stall_Option01, "Stall");
                RenamePhBotSubTabs(TabPageH_Stall_Option02, "Options");
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // Attack ekstra sekmeler: Party Options / Resurrect / Healing / Lure / Attack Log
        // ---------------------------------------------------------------
        private void EnsurePhBotAttackExtraTabs()
        {
            try
            {
                if (TabPageV_Control01_Skills_Panel == null) return;
                Panel host = TabPageV_Control01_Skills_Panel;
                // Yatay buton şeridi: ilk butonun 부모 paneli
                Control strip = TabPageH_Skills_Option01 != null ? TabPageH_Skills_Option01.Parent : null;
                if (strip == null) return;

                string[] extra = new string[] { "Party Options", "Resurrect", "Healing", "Lure", "Attack Log" };
                int x = 0;
                try
                {
                    if (TabPageH_Skills_Option03 != null)
                        x = TabPageH_Skills_Option03.Right + 2;
                    else
                        x = 240;
                }
                catch { x = 240; }

                foreach (string name in extra)
                {
                    string btnName = "TabPageH_Skills_Option" + SanitizeName(name);
                    if (host.Controls.Find(btnName + "_Panel", true).Length > 0) continue;
                    if (strip.Controls.ContainsKey(btnName)) continue;

                    var b = new Button();
                    b.Name = btnName;
                    b.Text = name;
                    b.Font = PhBotFont();
                    b.ForeColor = Color.Black;
                    b.BackColor = PhBotTabInactive;
                    b.FlatStyle = FlatStyle.Standard;
                    b.UseVisualStyleBackColor = true;
                    b.Size = new Size(Math.Max(80, TextRenderer.MeasureText(name, b.Font).Width + 20), 23);
                    b.Location = new Point(x, TabPageH_Skills_Option01 != null ? TabPageH_Skills_Option01.Top : 2);
                    b.Click += TabPageH_Option_Click;
                    b.Click += PhBotHTabVisual_Click;
                    strip.Controls.Add(b);
                    x += b.Width + 2;

                    var p = new Panel();
                    p.Name = btnName + "_Panel";
                    p.BackColor = PhBotBg;
                    p.Visible = false;
                    // Kardeş H panellerinden birinin boyut/konumunu kopyala
                    try
                    {
                        if (TabPageH_Skills_Option01_Panel != null)
                        {
                            p.Location = TabPageH_Skills_Option01_Panel.Location;
                            p.Size = TabPageH_Skills_Option01_Panel.Size;
                            p.Anchor = TabPageH_Skills_Option01_Panel.Anchor;
                        }
                        else
                        {
                            p.Location = new Point(3, 30);
                            p.Size = new Size(host.Width - 6, host.Height - 33);
                            p.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        }
                    }
                    catch { }
                    host.Controls.Add(p);
                    BuildPhBotAttackExtraContent(name, p);
                }
            }
            catch { }
        }

        private void BuildPhBotAttackExtraContent(string name, Panel p)
        {
            p.AutoScroll = true;
            if (name == "Attack Log")
            {
                var lv = new ListView();
                lv.Name = "PhBot_AttackLog";
                lv.View = View.Details;
                lv.FullRowSelect = true;
                lv.GridLines = true;
                lv.BackColor = Color.White;
                lv.Location = new Point(6, 6);
                lv.Size = new Size(Math.Max(400, p.Width - 12), Math.Max(200, p.Height - 12));
                lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                lv.Columns.Add("Time", 70);
                lv.Columns.Add("Attacker", 150);
                lv.Columns.Add("Info", 300);
                p.Controls.Add(lv);
                var lbl = new Label();
                lbl.Text = "Size saldıran oyuncular burada listelenir (Job Cave / PK takibi).";
                lbl.Font = PhBotFont();
                lbl.AutoSize = true;
                lbl.Location = new Point(6, Math.Max(220, p.Height - 40));
                lbl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                p.Controls.Add(lbl);
            }
            else if (name == "Resurrect")
            {
                AddPhBotCheck(p, "Resurrect all party members", 10, 10, true);
                AddPhBotCheck(p, "Auto accept res from other players", 10, 34, true);
                AddPhBotCheck(p, "Only accept res from party members", 10, 58, true);
                AddPhBotLabeledNumber(p, "Resurrect radius", 10, 84, 50);
                AddPhBotLabeledNumber(p, "Resurrect delay (s)", 10, 112, 5);
                AddPhBotLabeledNumber(p, "Resurrect try limit", 10, 140, 3);
                var lv = new ListView();
                lv.View = View.Details; lv.FullRowSelect = true; lv.GridLines = true; lv.BackColor = Color.White;
                lv.Location = new Point(300, 10); lv.Size = new Size(300, 160);
                lv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                lv.Columns.Add("Resurrect list", 180); lv.Columns.Add("Status", 110);
                p.Controls.Add(lv);
                var note = new Label();
                note.Text = "Revive çalışması için resurrect skili Buffs listesinde olmalı.";
                note.Font = PhBotFont(); note.AutoSize = true; note.Location = new Point(10, 170);
                p.Controls.Add(note);
            }
            else if (name == "Healing")
            {
                AddPhBotLabeledNumber(p, "Heal party member when HP % <", 10, 10, 70);
                AddPhBotLabeledNumber(p, "Group heal when avg HP % <", 10, 38, 60);
                AddPhBotLabeledNumber(p, "Heal self when HP % <", 10, 66, 50);
                var lv = new ListView();
                lv.View = View.Details; lv.FullRowSelect = true; lv.GridLines = true; lv.BackColor = Color.White;
                lv.Location = new Point(10, 96); lv.Size = new Size(420, 170);
                lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                lv.Columns.Add("Heal skill", 250); lv.Columns.Add("Target", 150);
                p.Controls.Add(lv);
            }
            else if (name == "Lure")
            {
                AddPhBotLabeledNumber(p, "Walk X spaces then back", 10, 10, 30);
                AddPhBotCheck(p, "Buff only after lure script ends", 10, 38, false);
                AddPhBotCheck(p, "Stop luring if party members are not near", 10, 62, true);
                AddPhBotLabeledNumber(p, "Stop if dead party members >", 10, 88, 2);
                AddPhBotLabeledNumber(p, "Stop if monsters in area >", 10, 116, 20);
                AddPhBotLabeledNumber(p, "Delay at edges (ms)", 10, 144, 2000);
                AddPhBotLabeledNumber(p, "Delay at center (ms)", 10, 172, 2000);
                var lbl = new Label();
                lbl.Text = "Cast: Howling Shout (önerilen)";
                lbl.Font = PhBotFont(); lbl.AutoSize = true; lbl.Location = new Point(10, 200);
                p.Controls.Add(lbl);
            }
            else // Party Options
            {
                AddPhBotLabeledNumber(p, "Party buffing radius", 10, 10, 30);
                AddPhBotCheck(p, "Walk back to center after buffing", 10, 38, true);
                AddPhBotCheck(p, "Buff all nearby players (önerilmez)", 10, 62, false);
                AddPhBotCheck(p, "Start/stop party buffing if player nearby", 10, 86, false);
            }
        }

        // ---------------------------------------------------------------
        // Pick Filter sidebar paneli (ekran görüntüsü birebir)
        // ---------------------------------------------------------------
        private void EnsurePhBotPickFilterTab()
        {
            // Sidebar rebuild sırasında zaten oluşturuldu (BuildPhBotPickFilterPanel).
            // Eski Town>Toplama Filtresi sekmesi gizlendi, veri kaybı yok.
        }

        private Panel BuildPhBotPickFilterPanel(Panel panel)
        {
            panel.Controls.Clear();
            var tabs = new TabControl();
            tabs.Name = "PhBot_PickFilterTabs";
            tabs.Location = new Point(4, 4);
            tabs.Size = new Size(800, 480);
            tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabs.Font = PhBotFont();

            var tpFilter = new TabPage("Pick Filter");
            tpFilter.BackColor = PhBotBg;
            var lv = new ListView();
            lv.Name = "PhBot_PickList";
            lv.View = View.Details;
            lv.FullRowSelect = true;
            lv.GridLines = true;
            lv.BackColor = Color.White;
            lv.Location = new Point(6, 6);
            lv.Size = new Size(770, 330);
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lv.Columns.Add("ID", 50);
            lv.Columns.Add("Icon", 50);
            lv.Columns.Add("Name", 260);
            lv.Columns.Add("Level", 60);
            lv.Columns.Add("Pick", 60);
            lv.Columns.Add("Pet", 60);
            lv.Columns.Add("Sell", 60);
            lv.Columns.Add("Store", 60);
            lv.Columns.Add("Store Guild", 90);
            string[] demo = new string[]
            {
                "4|HP Recovery Herb|0", "5|HP Recovery Potion (Small)|0",
                "6|HP Recovery Potion (Medium)|0", "7|HP Recovery Potion (Large)|0",
                "8|HP Recovery Potion (X-Large)|0", "9|HP Recovery Grain (Small)|0",
                "11|MP Recovery Herb|0", "12|MP Recovery Potion (Small)|0",
                "13|MP Recovery Potion (Medium)|0", "14|MP Recovery Potion (Large)|0"
            };
            foreach (string d in demo)
            {
                string[] s = d.Split('|');
                var it = new ListViewItem(s[0]);
                it.SubItems.Add("");
                it.SubItems.Add(s[1]);
                it.SubItems.Add(s[2]);
                it.SubItems.Add("No"); it.SubItems.Add("No"); it.SubItems.Add("No"); it.SubItems.Add("No"); it.SubItems.Add("No");
                lv.Items.Add(it);
            }
            tpFilter.Controls.Add(lv);

            var cbx1 = new ComboBox();
            cbx1.Items.AddRange(new object[] { "Potion", "Weapon", "Armor", "Accessory", "Avatar", "Quest", "All" });
            cbx1.SelectedIndex = 0; cbx1.DropDownStyle = ComboBoxStyle.DropDownList;
            cbx1.Location = new Point(6, 344); cbx1.Size = new Size(140, 22);
            cbx1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            tpFilter.Controls.Add(cbx1);
            var cbx2 = new ComboBox();
            cbx2.Items.AddRange(new object[] { "All", "1 Degree", "5 Degree", "9 Degree", "SoX" });
            cbx2.SelectedIndex = 0; cbx2.DropDownStyle = ComboBoxStyle.DropDownList;
            cbx2.Location = new Point(152, 344); cbx2.Size = new Size(120, 22);
            cbx2.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            tpFilter.Controls.Add(cbx2);
            var tbxSearch = new TextBox();
            tbxSearch.Location = new Point(278, 344); tbxSearch.Size = new Size(280, 22);
            tbxSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            tpFilter.Controls.Add(tbxSearch);

            var btnClear = new Button(); btnClear.Text = "Clear"; btnClear.Size = new Size(80, 24);
            btnClear.Location = new Point(566, 372); btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            var btnUpdate = new Button(); btnUpdate.Text = "Update"; btnUpdate.Size = new Size(80, 24);
            btnUpdate.Location = new Point(652, 372); btnUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            var btnReset = new Button(); btnReset.Text = "Reset"; btnReset.Size = new Size(80, 24);
            btnReset.Location = new Point(738, 372); btnReset.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            tpFilter.Controls.Add(btnClear); tpFilter.Controls.Add(btnUpdate); tpFilter.Controls.Add(btnReset);

            var warn = new Label();
            warn.Text = "* SOX items will not be sold, and your primary/secondary weapon will not be sold/stored";
            warn.Font = PhBotFont(); warn.AutoSize = true; warn.Location = new Point(6, 376);
            warn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            tpFilter.Controls.Add(warn);

            tabs.TabPages.Add(tpFilter);
            tabs.TabPages.Add(new TabPage("Options") { BackColor = PhBotBg });
            tabs.TabPages.Add(new TabPage("Store Gold") { BackColor = PhBotBg });
            tabs.TabPages.Add(new TabPage("Dismantle") { BackColor = PhBotBg });
            FillPhBotPickOptions((TabPage)tabs.TabPages[1]);
            FillPhBotStoreGold((TabPage)tabs.TabPages[2]);
            FillPhBotDismantle((TabPage)tabs.TabPages[3]);

            panel.Controls.Add(tabs);
            return panel;
        }

        private void FillPhBotPickOptions(TabPage tp)
        {
            string[] opts = new string[]
            {
                "Pick items first", "Use pick pet", "Pick other player's items",
                "Pick party items", "Don't pick items", "Allow selling of all item types",
                "Only pick rare or blue items", "Only store rare or blue items",
                "Pick Arrows/Bolts", "Pick with character if pet is unsummoned or full",
                "Do not move pet items except for storing/selling"
            };
            int y = 10;
            foreach (string o in opts)
            {
                var c = new CheckBox();
                c.Text = o; c.Font = PhBotFont(); c.AutoSize = true; c.Location = new Point(10, y);
                tp.Controls.Add(c); y += 24;
            }
        }

        private void FillPhBotStoreGold(TabPage tp)
        {
            AddPhBotLabeledNumber(tp, "Gold keep amount", 10, 10, 100000);
            AddPhBotCheck(tp, "Take gold from storage", 10, 38, false);
            AddPhBotCheck(tp, "Take gold from guild storage", 10, 62, false);
            AddPhBotLabeledNumber(tp, "Store gold in storage (0 = no limit)", 10, 88, 0);
            AddPhBotLabeledNumber(tp, "Store gold in guild storage (0 = no limit)", 10, 116, 0);
        }

        private void FillPhBotDismantle(TabPage tp)
        {
            var lbl = new Label();
            lbl.Text = "Belirli degree/tip itemleri NPC önünde dismantle eder.";
            lbl.Font = PhBotFont(); lbl.AutoSize = true; lbl.Location = new Point(10, 10);
            tp.Controls.Add(lbl);
            var lv = new ListView();
            lv.View = View.Details; lv.FullRowSelect = true; lv.GridLines = true; lv.BackColor = Color.White;
            lv.Location = new Point(10, 36); lv.Size = new Size(500, 250);
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lv.Columns.Add("Degree", 80); lv.Columns.Add("Type", 150); lv.Columns.Add("NPC", 200);
            tp.Controls.Add(lv);
        }

        // ---------------------------------------------------------------
        // Placeholder paneller
        // ---------------------------------------------------------------
        private Panel BuildPhBotPetPanel(Panel panel)
        {
            var tabs = new TabControl();
            tabs.Font = PhBotFont();
            tabs.Location = new Point(4, 4); tabs.Size = new Size(800, 300);
            tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabs.TabPages.Add(new TabPage("Fellow") { BackColor = PhBotBg });
            tabs.TabPages.Add(new TabPage("Pick Pet") { BackColor = PhBotBg });
            tabs.TabPages.Add(new TabPage("Transport") { BackColor = PhBotBg });
            AddPhBotCheck((Control)tabs.TabPages[0], "Summon fellow pet in training area", 10, 10, true);
            AddPhBotCheck((Control)tabs.TabPages[0], "Use pet recovery kits", 10, 34, true);
            AddPhBotCheck((Control)tabs.TabPages[1], "Use pick pet", 10, 10, true);
            AddPhBotCheck((Control)tabs.TabPages[1], "Pick with character if pet full", 10, 34, true);
            AddPhBotCheck((Control)tabs.TabPages[2], "Respawn job transport if dead", 10, 10, false);
            panel.Controls.Add(tabs);
            return panel;
        }

        private Panel BuildPhBotUnionPanel(Panel panel)
        {
            var lv = new ListView();
            lv.View = View.Details; lv.FullRowSelect = true; lv.GridLines = true; lv.BackColor = Color.White;
            lv.Location = new Point(6, 6); lv.Size = new Size(500, 300);
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lv.Columns.Add("Union member", 200); lv.Columns.Add("Level", 70); lv.Columns.Add("Guild", 200);
            panel.Controls.Add(lv);
            AddPhBotCheck(panel, "Accept union invites from list", 520, 10, true);
            AddPhBotCheck(panel, "Invite only from list", 520, 34, false);
            return panel;
        }

        private Panel BuildPhBotProjectHaxPanel(Panel panel)
        {
            panel.Controls.Clear();
            panel.BackColor = PhBotBg;

            bool isTR = LocalizationManager.CurrentLanguage == "TR";

            // 1. Üç açılır kutu (sağ üst): Dil, Tema, Yazı Boyutu
            var cmbxLang = new ComboBox
            {
                Name = "ProjectHax_cmbxLang",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = PhBotFont(),
                FlatStyle = FlatStyle.Popup,
                BackColor = Color.White,
                Size = new Size(80, 22)
            };
            cmbxLang.Items.AddRange(new object[] { "Türkçe", "English" });
            cmbxLang.SelectedIndex = isTR ? 0 : 1;

            var cmbxTheme = new ComboBox
            {
                Name = "ProjectHax_cmbxTheme",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = PhBotFont(),
                FlatStyle = FlatStyle.Popup,
                BackColor = Color.White,
                Size = new Size(70, 22)
            };
            cmbxTheme.Items.AddRange(isTR ? new object[] { "Beyaz", "Koyu" } : new object[] { "White", "Dark" });
            cmbxTheme.SelectedIndex = 0;

            var cmbxFontSize = new ComboBox
            {
                Name = "ProjectHax_cmbxFontSize",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = PhBotFont(),
                FlatStyle = FlatStyle.Popup,
                BackColor = Color.White,
                Size = new Size(46, 22)
            };
            cmbxFontSize.Items.AddRange(new object[] { "8", "9", "10", "11", "12" });
            cmbxFontSize.SelectedIndex = 0;

            panel.Controls.Add(cmbxLang);
            panel.Controls.Add(cmbxTheme);
            panel.Controls.Add(cmbxFontSize);

            // 2. xBot Giriş Grubu (birebir ölçüler: 244 x 140)
            var gbxLogin = new GroupBox
            {
                Name = "ProjectHax_gbxLogin",
                Text = isTR ? "xBot Giriş" : "xBot Login",
                Font = PhBotFont(),
                BackColor = Color.Transparent,
                Size = new Size(244, 140)
            };

            var lblUser = new Label { Name = "ProjectHax_lblUser", Text = isTR ? "Kullanıcı" : "Username", Font = PhBotFont(), AutoSize = true, Location = new Point(12, 24) };
            var tbxUser = new TextBox { Name = "ProjectHax_tbxUser", Text = "", Font = PhBotFont(), Location = new Point(78, 22), Size = new Size(152, 22), BorderStyle = BorderStyle.FixedSingle };
            var lblPass = new Label { Name = "ProjectHax_lblPass", Text = isTR ? "Şifre" : "Password", Font = PhBotFont(), AutoSize = true, Location = new Point(12, 54) };
            var tbxPass = new TextBox { Name = "ProjectHax_tbxPass", Font = PhBotFont(), Location = new Point(78, 52), Size = new Size(152, 22), BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            var btnLogin = new Button { Name = "ProjectHax_btnLogin", Text = isTR ? "Bağlan" : "Login", Font = PhBotFont(), Location = new Point(12, 86), Size = new Size(218, 26), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogin.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);

            gbxLogin.Controls.Add(lblUser);
            gbxLogin.Controls.Add(tbxUser);
            gbxLogin.Controls.Add(lblPass);
            gbxLogin.Controls.Add(tbxPass);
            gbxLogin.Controls.Add(btnLogin);
            panel.Controls.Add(gbxLogin);

            // Dil Değiştirme
            cmbxLang.SelectedIndexChanged += (s, e) =>
            {
                string lang = cmbxLang.SelectedIndex == 0 ? "TR" : "EN";
                if (LocalizationManager.CurrentLanguage != lang)
                {
                    LocalizationManager.SetLanguage(lang);
                    bool tr = (lang == "TR");
                    gbxLogin.Text = tr ? "xBot Giriş" : "xBot Login";
                    lblUser.Text = tr ? "Kullanıcı" : "Username";
                    lblPass.Text = tr ? "Şifre" : "Password";
                    btnLogin.Text = tr ? "Bağlan" : "Login";
                    cmbxTheme.Items.Clear();
                    cmbxTheme.Items.AddRange(tr ? new object[] { "Beyaz", "Koyu" } : new object[] { "White", "Dark" });
                    cmbxTheme.SelectedIndex = 0;
                }
            };

            Action reposition = () =>
            {
                int w = panel.ClientSize.Width, h = panel.ClientSize.Height;
                cmbxLang.Location = new Point(Math.Max(10, w - 210), 10);
                cmbxTheme.Location = new Point(Math.Max(10, w - 124), 10);
                cmbxFontSize.Location = new Point(Math.Max(10, w - 48), 10);

                gbxLogin.Location = new Point(Math.Max(20, (w - gbxLogin.Width) / 2), Math.Max(30, (h - gbxLogin.Height) / 2 - 10));
            };
            panel.SizeChanged += (s, e) => reposition();
            reposition();

            return panel;
        }

        private Panel BuildPhBotAutoConfigurePanel(Panel panel)
        {
            return panel;
        }

        private Panel BuildPhBotNotificationsPanel(Panel panel, string key)
        {
            AddPhBotCheck(panel, "Play sound on unique spawn", 10, 10, true);
            AddPhBotCheck(panel, "Play sound on death", 10, 34, true);
            AddPhBotCheck(panel, "Play sound on GM spawn", 10, 58, true);
            AddPhBotCheck(panel, "Send notification on disconnect", 10, 82, false);
            return panel;
        }

        private Panel BuildPhBotQuestPanel(Panel panel)
        {
            try
            {
                var qp = new QuestPanel();
                qp.Dock = DockStyle.Fill;
                panel.Controls.Add(qp);
            }
            catch
            {
                AddPhBotCheck(panel, "Auto complete level quests", 10, 10, false);
                AddPhBotCheck(panel, "Auto complete job quests", 10, 34, false);
            }
            return panel;
        }

        private Panel BuildPhBotTradePanel(Panel panel)
        {
            var lbl = new Label();
            lbl.Text = "Trade route / stall alım-satım döngüsü. Detay için Stall sekmesine de bakın.";
            lbl.Font = PhBotFont(); lbl.AutoSize = true; lbl.Location = new Point(10, 10);
            panel.Controls.Add(lbl);
            AddPhBotCheck(panel, "Enable trade loop", 10, 36, false);
            AddPhBotLabeledNumber(panel, "Buy quantity", 10, 62, 100);
            return panel;
        }

        private Panel BuildPhBotMasteriesPanel(Panel panel)
        {
            var lv = new ListView();
            lv.View = View.Details; lv.FullRowSelect = true; lv.GridLines = true; lv.BackColor = Color.White;
            lv.Location = new Point(6, 6); lv.Size = new Size(500, 300);
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lv.Columns.Add("Mastery", 250); lv.Columns.Add("Level", 80); lv.Columns.Add("Auto up", 80);
            panel.Controls.Add(lv);
            AddPhBotCheck(panel, "Auto level up masteries", 520, 10, true);
            AddPhBotLabeledNumber(panel, "STR per level", 520, 38, 1);
            AddPhBotLabeledNumber(panel, "INT per level", 520, 66, 1);
            return panel;
        }

        private Panel BuildPhBotSimpleListPanel(Panel panel, string key)
        {
            var lv = new ListView();
            lv.View = View.Details; lv.FullRowSelect = true; lv.GridLines = true; lv.BackColor = Color.White;
            lv.Location = new Point(6, 6); lv.Size = new Size(600, 320);
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            if (key == "Conditions")
            {
                lv.Columns.Add("Level", 80); lv.Columns.Add("Switch to", 250);
                AddPhBotCheck(panel, "Change training area on level up", 10, 336, false);
            }
            else
            {
                lv.Columns.Add("Action", 250); lv.Columns.Add("Key", 120);
            }
            panel.Controls.Add(lv);
            return panel;
        }

        private void LayoutTrainingAreaClassic()
        {
            try
            {
                if (TabPageH_Training_Option01_Panel == null) return;
                Panel host = TabPageH_Training_Option01_Panel;
                int W = host.Width;
                int H = host.Height;
                if (W < 100 || H < 100) return;
                try
                {
                    if (Training_lstvAreas != null)
                    {
                        Training_lstvAreas.Location = new Point(6, 6);
                        Training_lstvAreas.Size = new Size(300, Math.Max(200, H - 12));
                        Training_lstvAreas.Visible = true;
                        try
                        {
                            if (Training_lstvAreas.Columns.Count > 0)
                                Training_lstvAreas.Columns[0].Width = 294;
                        }
                        catch { }
                    }
                }
                catch { }
                try
                {
                    Control details = null;
                    foreach (Control k in host.Controls)
                    {
                        if (k.Name == "gbxAreaDetails") { details = k; break; }
                    }
                    if (details != null)
                    {
                        details.Location = new Point(312, 6);
                        details.Size = new Size(Math.Max(200, W - 318), Math.Max(200, H - 12));
                        details.Visible = true;
                        details.BringToFront();
                        foreach (Control k in details.Controls)
                        {
                            try { k.Visible = true; } catch { }
                        }
                        PhBotDebug("training details: vis=" + details.Visible +
                            " loc=" + details.Location + " size=" + details.Size +
                            " kids=" + details.Controls.Count);
                    }
                    else
                    {
                        PhBotDebug("training details: NOT FOUND");
                    }
                }
                catch (Exception ex) { PhBotDebug("training details: " + ex.Message); }
                try { host.AutoScroll = true; } catch { }
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // Alt bölüm: beyaz log + 6 buton
        // ---------------------------------------------------------------
        private void EnsurePhBotBottomArea()
        {
            if (pnlWindow == null) return;

            if (_phBotActionsPanel == null)
            {
                _phBotActionsPanel = new Panel();
                _phBotActionsPanel.Name = "PhBotActions";
                _phBotActionsPanel.BackColor = PhBotBg;
                pnlWindow.Controls.Add(_phBotActionsPanel);

                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                _phBotBtnLaunch = NewPhBotActionButton("PhBotLaunch", isTR ? "Clienti Başlat" : "Launch");
                _phBotBtnStart = NewPhBotActionButton("PhBotStart", isTR ? "Botu Başlat" : "Start Bot");
                _phBotBtnClientless = NewPhBotActionButton("PhBotClientless", isTR ? "Clienti Kapat" : "Kill Client");
                _phBotBtnStop = NewPhBotActionButton("PhBotStop", isTR ? "Botu Durdur" : "Stop Bot");
                _phBotBtnHide = NewPhBotActionButton("PhBotHide", isTR ? "Clienti Gizle" : "Hide Client");
                _phBotBtnReturn = NewPhBotActionButton("PhBotReturn", isTR ? "Şehre Dön" : "Return Scroll");

                _phBotBtnLaunch.Click += (s, e) => { LaunchClientWithLoader(); };
                _phBotBtnStart.Click += (s, e) => { try { if (!Bot.Get.isBotting) Bot.Get.Start(); } catch { } try { UpdatePhBotTitle(); } catch { } };
                _phBotBtnStop.Click += (s, e) => { try { if (Bot.Get.isBotting) Bot.Get.Stop(); } catch { } try { UpdatePhBotTitle(); } catch { } };
                _phBotBtnClientless.Click += (s, e) => { try { if (Menu_btnClientOptions_GoClientless != null) Menu_btnClientOptions_GoClientless.PerformClick(); } catch { } };
                _phBotBtnHide.Click += (s, e) => { try { if (Menu_btnClientOptions_ShowHide != null) Menu_btnClientOptions_ShowHide.PerformClick(); } catch { } };
                _phBotBtnReturn.Click += (s, e) =>
                {
                    try { Bot.Get.UseReturnScroll(); }
                    catch
                    {
                        try { MessageBox.Show(this, "Return Scroll komutu gönderilemedi (oyuna bağlı değil).", ProductName); }
                        catch { }
                    }
                };

                _phBotActionsPanel.Controls.AddRange(new Control[]
                {
                    _phBotBtnLaunch, _phBotBtnStart,
                    _phBotBtnClientless, _phBotBtnStop,
                    _phBotBtnHide, _phBotBtnReturn
                });

                _phBotCopyright = new Label();
                _phBotCopyright.Font = PhBotFont();
                _phBotCopyright.ForeColor = Color.FromArgb(60, 60, 60);
                _phBotCopyright.TextAlign = ContentAlignment.MiddleCenter;
                _phBotCopyright.Text = "©2026 xBot";
                _phBotActionsPanel.Controls.Add(_phBotCopyright);
            }
        }

        private Button NewPhBotActionButton(string name, string text)
        {
            var b = new Button();
            b.Name = name;
            b.Text = text;
            b.Font = PhBotFont();
            b.ForeColor = Color.Black;
            b.BackColor = PhBotBg;
            b.FlatStyle = FlatStyle.Standard;
            b.UseVisualStyleBackColor = true;
            b.Size = new Size(110, 26);
            return b;
        }

        private void LayoutPhBotClassic()
        {
            if (pnlWindow == null) return;
            try
            {
                const int margin = 4;
                const int sidebarW = 188;
                const int bottomH = 158;

                if (TabPageV_Control01 != null)
                {
                    TabPageV_Control01.Location = new Point(margin, margin);
                    // phBot'taki gibi tam boy sidebar (log yalnızca içerik alanının altındadır)
                    TabPageV_Control01.Size = new Size(sidebarW, Math.Max(200, pnlWindow.Height - margin * 2));
                    TabPageV_Control01.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                    // Buton genişliklerini sidebar'a uydur
                    foreach (Control c in TabPageV_Control01.Controls)
                    {
                        if (c is Button)
                            c.Width = sidebarW - 34;
                    }
                }

                int contentX = margin + sidebarW + margin;
                int contentW = Math.Max(300, pnlWindow.Width - contentX - margin);
                int contentH = Math.Max(150, pnlWindow.Height - bottomH - margin * 3);

                foreach (Control c in pnlWindow.Controls)
                {
                    if (c is Panel && c.Name.StartsWith("TabPageV_Control01_") && c.Name.EndsWith("_Panel"))
                    {
                        c.Location = new Point(contentX, margin);
                        c.Size = new Size(contentW, contentH);
                        c.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    }
                }

                int logY = margin + contentH + margin;
                int logH = bottomH - 8;
                int actionsW = 280;
                if (rtbxLogs != null)
                {
                    rtbxLogs.Location = new Point(contentX, logY);
                    rtbxLogs.Size = new Size(Math.Max(200, contentW - actionsW - margin), logH);
                    rtbxLogs.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    rtbxLogs.BackColor = Color.White;
                    rtbxLogs.ForeColor = Color.Black;
                    rtbxLogs.BorderStyle = BorderStyle.FixedSingle;
                    try { rtbxLogs.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point); } catch { }
                }

                if (_phBotActionsPanel != null)
                {
                    _phBotActionsPanel.Location = new Point(contentX + contentW - actionsW, logY);
                    _phBotActionsPanel.Size = new Size(actionsW, logH);
                    _phBotActionsPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    _phBotActionsPanel.BackColor = PhBotBg;

                    int bw = 132, bh = 30, gap = 8;
                    _phBotBtnLaunch.Location = new Point(0, 0); _phBotBtnLaunch.Size = new Size(bw, bh);
                    _phBotBtnStart.Location = new Point(bw + gap, 0); _phBotBtnStart.Size = new Size(bw, bh);
                    _phBotBtnClientless.Location = new Point(0, bh + 6); _phBotBtnClientless.Size = new Size(bw, bh);
                    _phBotBtnStop.Location = new Point(bw + gap, bh + 6); _phBotBtnStop.Size = new Size(bw, bh);
                    _phBotBtnHide.Location = new Point(0, (bh + 6) * 2); _phBotBtnHide.Size = new Size(bw, bh);
                    _phBotBtnReturn.Location = new Point(bw + gap, (bh + 6) * 2); _phBotBtnReturn.Size = new Size(bw, bh);
                    _phBotCopyright.Location = new Point(0, (bh + 6) * 3 + 2);
                    _phBotCopyright.Size = new Size(actionsW, 18);
                }
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // Klasik stil
        // ---------------------------------------------------------------
        private void StyleClassicRecursive(Control parent)
        {
            if (parent == null) return;
            // QuestPanel gibi özel boyalı kontrollers hariç
            if (parent is QuestPanel) return;
            StyleClassicSingle(parent);
            foreach (Control c in parent.Controls)
                StyleClassicRecursive(c);
        }

        private void StyleClassicSingle(Control c)
        {
            if (c == null) return;
            try
            {
                if (c == TabPageV_Control01 || c == _phBotActionsPanel) { /* renkler zaten verildi */ }
                if (c is Button)
                {
                    var b = (Button)c;
                    // Sidebar butonları flat-beyaz kalır (phBot liste görünümü)
                    bool isSidebar = b.Parent == TabPageV_Control01;
                    bool isHTab = b.Name.StartsWith("TabPageH_");
                    if (isSidebar)
                    {
                        b.FlatStyle = FlatStyle.Flat;
                        b.BackColor = PhBotPanelWhite;
                        b.ForeColor = Color.Black;
                        try { b.FlatAppearance.BorderSize = 0; } catch { }
                    }
                    else if (isHTab)
                    {
                        b.FlatStyle = FlatStyle.Standard;
                        b.BackColor = PhBotTabInactive;
                        b.ForeColor = Color.Black;
                        b.UseVisualStyleBackColor = true;
                    }
                    else if (b.Name.StartsWith("PhBot"))
                    {
                        // aksiyon butonları klasik gri
                    }
                    else
                    {
                        b.FlatStyle = FlatStyle.Standard;
                        b.BackColor = PhBotBg;
                        b.ForeColor = Color.Black;
                        b.UseVisualStyleBackColor = true;
                    }
                    if (b.Font == null || b.Font.Size > 10f) b.Font = PhBotFont();
                    else
                    {
                        try { b.Font = PhBotFont(); } catch { }
                    }
                }
                else if (c is CheckBox || c is RadioButton)
                {
                    c.ForeColor = Color.Black;
                    try { c.Font = PhBotFont(); } catch { }
                    try
                    {
                        Color bg = c.BackColor;
                        if (bg.GetBrightness() < 0.35f)
                        {
                            Color pbg = (c.Parent != null) ? c.Parent.BackColor : PhBotBg;
                            c.BackColor = (pbg.GetBrightness() < 0.35f) ? PhBotBg : pbg;
                        }
                    }
                    catch { }
                }
                else if (c is Label)
                {
                    var l = (Label)c;
                    if (l.Parent == TabPageV_Control01)
                    {
                        l.BackColor = PhBotPanelWhite;
                        l.ForeColor = Color.Black;
                    }
                    else
                    {
                        try
                        {
                            Color own = l.BackColor;
                            Color eff = (own.A == 0 || own == Color.Transparent)
                                ? (l.Parent != null ? l.Parent.BackColor : PhBotBg)
                                : own;
                            if (eff.GetBrightness() > 0.5f)
                            {
                                if (l.ForeColor.GetBrightness() > 0.75f)
                                    l.ForeColor = Color.Black;
                                if (own.GetBrightness() < 0.35f && own.A != 0)
                                    l.BackColor = Color.Transparent;
                            }
                        }
                        catch { }
                    }
                }
                else if (c is TextBox || c is ComboBox || c is NumericUpDown || c is RichTextBox || c is ListBox)
                {
                    c.BackColor = Color.White;
                    c.ForeColor = Color.Black;
                    try { c.Font = PhBotFont(); } catch { }
                }
                else if (c is ListView)
                {
                    var lv = (ListView)c;
                    try { lv.OwnerDraw = false; } catch { }
                    lv.BackColor = Color.White;
                    lv.ForeColor = Color.Black;
                    try { lv.Font = PhBotFont(); } catch { }
                    lv.FullRowSelect = true;
                    lv.GridLines = true;
                    try { lv.BorderStyle = BorderStyle.FixedSingle; } catch { }
                }
                else if (c is GroupBox)
                {
                    c.ForeColor = Color.Black;
                    c.BackColor = PhBotBg;
                    try { c.Font = PhBotFont(); } catch { }
                }
                else if (c is TabControl)
                {
                    c.BackColor = PhBotBg;
                    c.ForeColor = Color.Black;
                }
                else if (c is TabPage)
                {
                    c.BackColor = PhBotBg;
                    c.ForeColor = Color.Black;
                }
                else if (c is Panel)
                {
                    var p = (Panel)c;
                    if (p == pnlWindow || p == _phBotActionsPanel) return;
                    if (p.Parent == TabPageV_Control01) return;
                    try
                    {
                        // Yatay sekme şeritleri her zaman açık gri (mavi seçim rengi kalmasın)
                        if (p.Name.StartsWith("TabPageH_") && !p.Name.Contains("_Option"))
                        {
                            p.BackColor = PhBotBg;
                            return;
                        }
                        if (p.BackColor.GetBrightness() < 0.35f)
                            p.BackColor = PhBotBg;
                    }
                    catch { }
                }
                else if (c is ProgressBar)
                {
                    // klasik kalır
                }
            }
            catch { }
        }

        private static CheckBox AddPhBotCheck(Control parent, string text, int x, int y, bool check)
        {
            var c = new CheckBox();
            c.Text = text;
            c.Font = PhBotFont();
            c.ForeColor = Color.Black;
            c.AutoSize = true;
            c.UseMnemonic = false;
            c.Location = new Point(x, y);
            c.Checked = check;
            parent.Controls.Add(c);
            return c;
        }

        private static NumericUpDown AddPhBotLabeledNumber(Control parent, string label, int x, int y, int value)
        {
            var l = new Label();
            l.Text = label;
            l.Font = PhBotFont();
            l.ForeColor = Color.Black;
            l.AutoSize = true;
            l.Location = new Point(x, y + 3);
            parent.Controls.Add(l);
            var n = new NumericUpDown();
            n.Value = Math.Max(n.Minimum, Math.Min(n.Maximum, value));
            n.Font = PhBotFont();
            n.BackColor = Color.White;
            n.Location = new Point(x + Math.Max(180, TextRenderer.MeasureText(label, l.Font).Width + 10), y);
            n.Size = new Size(70, 22);
            parent.Controls.Add(n);
            return n;
        }
    }
}
