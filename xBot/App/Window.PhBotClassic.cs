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

        private static readonly Font _phBotDefaultFont = CreatePhBotFont();

        private static Font CreatePhBotFont()
        {
            try { return new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point); }
            catch { return SystemFonts.DefaultFont; }
        }

        private static Font PhBotFont()
        {
            return _phBotDefaultFont;
        }

        private void PhBotDebug(string msg)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PhBot] " + msg);
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
        public void ApplyPhBotClassicTheme()
        {
            ApplyXBotReferenceLayout();
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

                    if (!string.IsNullOrEmpty(customStatus))
                    {
                        if (customStatus.StartsWith("Casting", StringComparison.OrdinalIgnoreCase) ||
                            customStatus.Contains("skill") ||
                            customStatus.Contains("ms)...") ||
                            customStatus.Contains("Attack loop") ||
                            customStatus.Contains("Sorting") ||
                            customStatus.Contains("Weapon Swap") ||
                            customStatus.Contains("Checking current"))
                        {
                            customStatus = null;
                        }
                    }

                    string status;
                    if (!string.IsNullOrEmpty(customStatus))
                    {
                        status = customStatus;
                    }
                    else
                    {
                        bool isConnected = false;
                        try { isConnected = InfoManager.inGame; } catch { }
                        bool isBotting = false;
                        try { isBotting = Bot.Get != null && Bot.Get.isBotting; } catch { }

                        if (isBotting)
                            status = isTR ? "Bot Çalışıyor" : "Botting";
                        else if (isConnected)
                            status = isTR ? "Bağlandı" : "Connected";
                        else
                            status = isTR ? "Bağlantı kesildi" : "Disconnected";
                    }
                    this.Text = string.Format("xBot - {0} - {1}", charName, status);
                });
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // Yardımcı metotlar
        // ---------------------------------------------------------------

        private static string SanitizeName(string s)
        {
            string r = "";
            foreach (char c in s)
                r += (char.IsLetterOrDigit(c) ? c.ToString() : "");
            return string.IsNullOrEmpty(r) ? "Item" : r;
        }

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
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                RenamePhBotSubTabs(TabPageH_Skills_Option01, isTR ? "Saldırı" : "Attack");
                RenamePhBotSubTabs(TabPageH_Skills_Option02, isTR ? "Bufflar" : "Buffs");
                RenamePhBotSubTabs(TabPageH_Skills_Option03, isTR ? "Parti" : "Party");

                Control stripSkills = TabPageH_Skills_Option01 != null ? TabPageH_Skills_Option01.Parent : null;
                if (stripSkills != null)
                {
                    Button bOpt = stripSkills.Controls["TabPageH_Skills_OptionPartyOptions"] as Button;
                    if (bOpt != null) RenamePhBotSubTabs(bOpt, isTR ? "Parti Ayarları" : "Party Options");

                    Button bRes = stripSkills.Controls["TabPageH_Skills_OptionResurrect"] as Button;
                    if (bRes != null) RenamePhBotSubTabs(bRes, isTR ? "Canlandır" : "Resurrect");

                    Button bHeal = stripSkills.Controls["TabPageH_Skills_OptionHealing"] as Button;
                    if (bHeal != null) RenamePhBotSubTabs(bHeal, isTR ? "İyileştirme" : "Healing");

                    Button bLure = stripSkills.Controls["TabPageH_Skills_OptionLure"] as Button;
                    if (bLure != null) RenamePhBotSubTabs(bLure, "Lure");

                    Button bLog = stripSkills.Controls["TabPageH_Skills_OptionAttackLog"] as Button;
                    if (bLog != null) RenamePhBotSubTabs(bLog, isTR ? "Saldırı Günlüğü" : "Attack Log");
                }
            }
            catch { }
            try
            {
                // Protection -> Character paneli
                // Pot | Socket | Dönüş | Pet Işınlama | Berserk | Canavar Tercihleri | Devil's Spirit | Scrollar | Stat Puanları | Diğerleri.
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                RenamePhBotSubTabs(TabPageH_Character_Option01, isTR ? "Bilgi" : "Info");
                RenamePhBotSubTabs(TabPageH_Character_Option02, isTR ? "Pot" : "Potions");
                RenamePhBotSubTabs(TabPageH_Character_Option03, isTR ? "Dönüş" : "Return");
                RenamePhBotSubTabs(TabPageH_Character_Option04, isTR ? "Diğerleri." : "Misc.");

                Control strip = TabPageH_Character_Option04 != null ? TabPageH_Character_Option04.Parent : null;
                if (strip != null)
                {
                    Button bSock = strip.Controls["TabPageH_Character_OptionSockets"] as Button;
                    if (bSock != null) RenamePhBotSubTabs(bSock, isTR ? "Socket" : "Sockets");

                    Button bPet = strip.Controls["TabPageH_Character_OptionPetReturn"] as Button;
                    if (bPet != null) RenamePhBotSubTabs(bPet, isTR ? "Pet Işınlama" : "Pet Return");

                    Button bZerk = strip.Controls["TabPageH_Character_OptionBerserk"] as Button;
                    if (bZerk != null) RenamePhBotSubTabs(bZerk, "Berserk");

                    Button bMobs = strip.Controls["TabPageH_Character_OptionMonsterPreferences"] as Button;
                    if (bMobs != null) RenamePhBotSubTabs(bMobs, isTR ? "Canavar Tercihleri" : "Monster Preferences");

                    Button bDev = strip.Controls["TabPageH_Character_OptionDevilsSpirit"] as Button;
                    if (bDev != null) RenamePhBotSubTabs(bDev, "Devil's Spirit");

                    Button bScr = strip.Controls["TabPageH_Character_OptionScrolls"] as Button;
                    if (bScr != null) RenamePhBotSubTabs(bScr, isTR ? "Scrollar" : "Scrolls");

                    Button bStat = strip.Controls["TabPageH_Character_OptionStatPoints"] as Button;
                    if (bStat != null) RenamePhBotSubTabs(bStat, isTR ? "Stat Puanları" : "Stat Points");
                }
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

                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                foreach (string name in extra)
                {
                    string btnName = "TabPageH_Skills_Option" + SanitizeName(name);
                    if (host.Controls.Find(btnName + "_Panel", true).Length > 0) continue;
                    if (strip.Controls.ContainsKey(btnName)) continue;

                    string trText = name;
                    if (name == "Party Options") trText = "Parti Ayarları";
                    else if (name == "Resurrect") trText = "Canlandır";
                    else if (name == "Healing") trText = "İyileştirme";
                    else if (name == "Lure") trText = "Lure";
                    else if (name == "Attack Log") trText = "Saldırı Günlüğü";

                    var b = new Button();
                    b.Name = btnName;
                    b.Text = isTR ? trText : name;
                    b.Font = PhBotFont();
                    b.ForeColor = Color.Black;
                    b.BackColor = PhBotTabInactive;
                    b.FlatStyle = FlatStyle.Standard;
                    b.UseVisualStyleBackColor = true;
                    b.Size = new Size(Math.Max(80, TextRenderer.MeasureText(b.Text, b.Font).Width + 20), 23);
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

                // Tab 3: Parti sekmesi (görsel referansa birebir boşluk doldurma)
                if (TabPageH_Skills_Option03_Panel != null && TabPageH_Skills_Option03_Panel.Controls.Count == 0)
                {
                    BuildPhBotPartyTab(TabPageH_Skills_Option03_Panel);
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
                BuildPhBotResurrectTab(p);
            }
            else if (name == "Healing")
            {
                AddPhBotLabeledNumber(p, "Heal party member when HP % <", 10, 10, 70);
                AddPhBotLabeledNumber(p, "Group heal when avg HP % <", 10, 38, 60);
                var chkSelf = new CheckBox
                {
                    Text = "Heal self when HP % <",
                    Font = PhBotFont(),
                    AutoSize = true,
                    Location = new Point(10, 68),
                    Checked = ProtectionManager.UseSkillHP
                };
                chkSelf.CheckedChanged += (s, e) => {
                    ProtectionManager.UseSkillHP = chkSelf.Checked;
                    if (cbxProtectionSkillHP != null && cbxProtectionSkillHP.Checked != chkSelf.Checked)
                        cbxProtectionSkillHP.Checked = chkSelf.Checked;
                    Settings.SaveCharacterSettings();
                };
                var nudSelf = new NumericUpDown
                {
                    Font = PhBotFont(),
                    Location = new Point(160, 66),
                    Size = new Size(50, 22),
                    Minimum = 1,
                    Maximum = 99,
                    Value = Math.Max(1, Math.Min(99, (int)ProtectionManager.SkillHPPercent))
                };
                nudSelf.ValueChanged += (s, e) => {
                    ProtectionManager.SkillHPPercent = (byte)nudSelf.Value;
                    if (nudProtectionSkillHP != null && nudProtectionSkillHP.Value != nudSelf.Value)
                        nudProtectionSkillHP.Value = nudSelf.Value;
                    Settings.SaveCharacterSettings();
                };
                p.Controls.Add(chkSelf);
                p.Controls.Add(nudSelf);
                var lv = new ListView();
                lv.View = View.Details; lv.FullRowSelect = true; lv.GridLines = true; lv.BackColor = Color.White;
                lv.Location = new Point(10, 100); lv.Size = new Size(420, 170);
                lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                lv.Columns.Add("Heal skill", 250); lv.Columns.Add("Target", 150);
                p.Controls.Add(lv);
            }
            else if (name == "Lure")
            {
                BuildPhBotLureTab(p);
            }
            else // Party Options
            {
                BuildPhBotPartyOptionsTab(p);
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
