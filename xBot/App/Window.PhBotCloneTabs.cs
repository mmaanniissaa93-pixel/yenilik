using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    /// <summary>
    /// phBot birebir klon sekmeleri (2. faz).
    /// Referans: docs/phbot_ref/md/phbot_*.md (guide.phbot.org metin dökümleri).
    /// Yalnızca görsel yerleşim + mevcut panellerin taşınmasını yapar; yeni
    /// checkbox'lar backend'e bağlı değildir (TODO ile işaretlidir).
    /// Tüm metotlar idempotent'tir, ApplyPhBotClassicTheme içinden çağrılır.
    /// </summary>
    public partial class Window
    {
        // ---------------------------------------------------------------
        // Genel yardımcı: yatay (H) alt-sekeme yeni buton + panel ekler.
        // Kural TabPageH_Option_Click ile uyumludur: panel, butonun
        // ebeveyn şeridinin ebeveyni (V paneli) altında
        // buton.Adı + "_Panel" ismiyle durmalıdır.
        // ---------------------------------------------------------------
        private Button EnsureExtraHTab(Panel hostV, Button anchor, string hPrefix, string title)
        {
            if (hostV == null || anchor == null || anchor.Parent == null) return null;
            string btnName = hPrefix + SanitizeName(title);
            Control strip = anchor.Parent;

            Button b = null;
            try
            {
                if (strip.Controls.ContainsKey(btnName))
                    b = strip.Controls[btnName] as Button;
            }
            catch { }
            if (b == null)
            {
                b = new Button();
                b.Name = btnName;
                b.Text = title;
                b.Font = PhBotFont();
                b.ForeColor = Color.Black;
                b.BackColor = PhBotTabInactive;
                b.FlatStyle = FlatStyle.Standard;
                b.UseVisualStyleBackColor = true;
                b.Size = new Size(Math.Max(70, TextRenderer.MeasureText(title, b.Font).Width + 20), 23);
                try { b.Location = new Point(anchor.Right + 2, anchor.Top); } catch { b.Location = new Point(240, 2); }
                b.Click += TabPageH_Option_Click;
                b.Click += PhBotHTabVisual_Click;
                strip.Controls.Add(b);
            }

            string panelName = btnName + "_Panel";
            Panel p = null;
            try
            {
                if (hostV.Controls.ContainsKey(panelName))
                    p = hostV.Controls[panelName] as Panel;
            }
            catch { }
            if (p == null)
            {
                p = new Panel();
                p.Name = panelName;
                p.BackColor = Color.White;
                p.Visible = false;
                p.AutoScroll = true;
                try
                {
                    Control tpl = null;
                    try { tpl = hostV.Controls[anchor.Name + "_Panel"]; } catch { }
                    if (tpl != null)
                    {
                        p.Location = tpl.Location;
                        p.Size = tpl.Size;
                        p.Anchor = tpl.Anchor;
                    }
                    else
                    {
                        p.Location = new Point(3, 30);
                        p.Size = new Size(Math.Max(300, hostV.Width - 6), Math.Max(200, hostV.Height - 33));
                        p.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    }
                }
                catch { }
                hostV.Controls.Add(p);
            }
            return b;
        }

        /// <summary>
        /// Şeritteki TabPageH_ butonlarını verilen sırada dizer (LayoutSkillsStrip ile aynı mantık).
        /// </summary>
        private void LayoutHStrip(Control strip, string[] orderedNames)
        {
            if (strip == null || orderedNames == null) return;
            try { if (strip.Tag is TabControl) return; } catch { }
            try
            {
                int x = 2;
                int top = -1;
                foreach (string n in orderedNames)
                {
                    Button b = null;
                    try
                    {
                        if (strip.Controls.ContainsKey(n))
                            b = strip.Controls[n] as Button;
                    }
                    catch { }
                    if (b == null) continue;
                    try
                    {
                        if (top < 0) top = b.Top;
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
            }
            catch { }
        }

        private static ListView NewPhBotListView(int x, int y, int w, int h, params string[] columns)
        {
            var lv = new ListView();
            lv.View = View.Details;
            lv.FullRowSelect = true;
            lv.GridLines = true;
            lv.BackColor = Color.White;
            lv.ForeColor = Color.Black;
            lv.Location = new Point(x, y);
            lv.Size = new Size(w, h);
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            if (columns != null)
            {
                foreach (string c in columns)
                {
                    string[] s = c.Split('|');
                    int cw = 100;
                    try { if (s.Length > 1) cw = int.Parse(s[1]); } catch { }
                    lv.Columns.Add(s[0], cw);
                }
            }
            return lv;
        }

        private static Label NewPhBotNote(Control parent, string text, int x, int y)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Font = PhBotFont();
            lbl.ForeColor = Color.FromArgb(60, 60, 60);
            lbl.AutoSize = true;
            lbl.MaximumSize = new Size(700, 0);
            lbl.Location = new Point(x, y);
            parent.Controls.Add(lbl);
            return lbl;
        }

        private Panel GetExtraHTabPanel(Panel hostV, string hPrefix, string title)
        {
            if (hostV == null) return null;
            string panelName = hPrefix + SanitizeName(title) + "_Panel";
            try
            {
                if (hostV.Controls.ContainsKey(panelName))
                    return hostV.Controls[panelName] as Panel;
            }
            catch { }
            return null;
        }

        // ---------------------------------------------------------------
        // ADIM 1: gerçek panelleri sidebar placeholder'larına taşı.
        // RebuildPhBotSidebar, Trade/Quest için demo panel üretir; oysa
        // BuildTradeTab / BuildQuestTab çoktan gerçek paneli kurmuştu ve
        // isim çakışması yüzünden _Legacy'ye düşmüştü. İçeriği taşı.
        // ---------------------------------------------------------------
        private void WireRealSidebarPanels()
        {
            try
            {
                // --- Trade: gerçek içerik -> placeholder ---
                Panel tradeReal = null;
                try
                {
                    foreach (Control c in pnlWindow.Controls)
                    {
                        if (c is Panel && c.Name == "TabPageV_Control01_Trade_Panel_Legacy")
                        { tradeReal = (Panel)c; break; }
                    }
                }
                catch { }
                Panel tradePh = null;
                try
                {
                    foreach (Control c in pnlWindow.Controls)
                    {
                        if (c is Panel && c.Name == "TabPageV_Control01_Trade_Panel")
                        { tradePh = (Panel)c; break; }
                    }
                }
                catch { }
                if (tradeReal != null && tradePh != null && tradeReal != tradePh)
                {
                    try
                    {
                        var kids = new List<Control>();
                        foreach (Control k in tradeReal.Controls) kids.Add(k);
                        tradePh.Controls.Clear();
                        foreach (Control k in kids)
                        {
                            try { tradeReal.Controls.Remove(k); tradePh.Controls.Add(k); } catch { }
                        }
                        try { pnlWindow.Controls.Remove(tradeReal); tradeReal.Dispose(); } catch { }
                        PhBotDebug("trade wired: " + kids.Count + " controls moved");
                    }
                    catch (Exception ex) { PhBotDebug("trade wire: " + ex.Message); }
                }

                // --- Quest: gerçek questPanel -> placeholder ---
                Panel questReal = null;
                try
                {
                    foreach (Control c in pnlWindow.Controls)
                    {
                        if (c is Panel && c.Name == "TabPageV_Control01_Quest_Panel_Legacy")
                        { questReal = (Panel)c; break; }
                    }
                }
                catch { }
                Panel questPh = null;
                try
                {
                    foreach (Control c in pnlWindow.Controls)
                    {
                        if (c is Panel && c.Name == "TabPageV_Control01_Quest_Panel")
                        { questPh = (Panel)c; break; }
                    }
                }
                catch { }
                if (questPh != null)
                {
                    try
                    {
                        // Demo QuestPanel'i at (olay/timer bağlantısı yok)
                        var demo = new List<Control>();
                        foreach (Control k in questPh.Controls)
                        {
                            if (k is QuestPanel && k != questPanel) demo.Add(k);
                        }
                        foreach (Control d in demo)
                        {
                            try { questPh.Controls.Remove(d); d.Dispose(); } catch { }
                        }
                    }
                    catch { }
                    if (questReal != null && questReal != questPh && questPanel != null)
                    {
                        try
                        {
                            if (questPanel.Parent != null)
                            {
                                try { questPanel.Parent.Controls.Remove(questPanel); } catch { }
                            }
                            questPanel.Dock = DockStyle.Fill;
                            questPh.Controls.Add(questPanel);
                            try { pnlWindow.Controls.Remove(questReal); questReal.Dispose(); } catch { }
                            PhBotDebug("quest wired");
                        }
                        catch (Exception ex) { PhBotDebug("quest wire: " + ex.Message); }
                    }
                }

                // --- Pick Filter: gerçek sekmeler -> top-level placeholder ---
                try
                {
                    Panel pickPh = null;
                    foreach (Control c in pnlWindow.Controls)
                    {
                        if (c is Panel && c.Name == "TabPageV_Control01_PickFilter_Panel")
                        { pickPh = (Panel)c; break; }
                    }
                    if (pickPh != null && tabPickFilterRoot != null && tabPickFilterRoot.Parent != pickPh)
                    {
                        try
                        {
                            if (tabPickFilterRoot.Parent != null)
                            {
                                try { tabPickFilterRoot.Parent.Controls.Remove(tabPickFilterRoot); } catch { }
                            }
                            pickPh.Controls.Clear();
                            tabPickFilterRoot.Dock = DockStyle.Fill;
                            try { tabPickFilterRoot.Location = new Point(4, 4); } catch { }
                            pickPh.Controls.Add(tabPickFilterRoot);
                            PhBotDebug("pickfilter wired");
                        }
                        catch (Exception ex) { PhBotDebug("pick wire: " + ex.Message); }
                    }
                }
                catch (Exception ex) { PhBotDebug("pick wire outer: " + ex.Message); }
            }
            catch (Exception ex) { PhBotDebug("wire real: " + ex.Message); }
        }

        // ---------------------------------------------------------------
        // ADIM 2: Protection alt-sekmeleri.
        // phBot sırası: Potions | Return | Sockets | Pet Return | Berserk |
        // Monster Preferences | Devil's Spirit | Scrolls | Stats | Misc.
        // (Info xBot ekstrası olarak başta kalır.)
        // ---------------------------------------------------------------
        private void EnsurePhBotProtectionExtraTabs()
        {
            try
            {
                if (TabPageV_Control01_Character_Panel == null) return;
                Panel host = TabPageV_Control01_Character_Panel;
                if (TabPageH_Character_Option04 == null) return;
                const string pre = "TabPageH_Character_Option";

                string[] names = new string[]
                {
                    "Sockets", "Pet Return", "Berserk", "Monster Preferences",
                    "Devil's Spirit", "Scrolls", "Stat Points"
                };
                foreach (string n in names)
                    EnsureExtraHTab(host, TabPageH_Character_Option04, pre, n);

                LayoutHStrip(TabPageH_Character_Option04.Parent, new string[]
                {
                    "TabPageH_Character_Option02", "TabPageH_Character_Option03",
                    pre + "Sockets", pre + "PetReturn",
                    pre + "Berserk", pre + "MonsterPreferences", pre + "DevilsSpirit",
                    pre + "Scrolls", pre + "StatPoints", "TabPageH_Character_Option04"
                });

                BuildProtectionSockets(GetExtraHTabPanel(host, pre, "Sockets"));
                BuildProtectionPetReturn(GetExtraHTabPanel(host, pre, "Pet Return"));
                BuildProtectionBerserk(GetExtraHTabPanel(host, pre, "Berserk"));
                BuildProtectionMonsterPrefs(GetExtraHTabPanel(host, pre, "Monster Preferences"));
                BuildProtectionDevilSpirit(GetExtraHTabPanel(host, pre, "Devil's Spirit"));
                BuildProtectionScrolls(GetExtraHTabPanel(host, pre, "Scrolls"));
                BuildProtectionStats(GetExtraHTabPanel(host, pre, "Stat Points"));
            }
            catch (Exception ex) { PhBotDebug("protection tabs: " + ex.Message); }
        }

        private void BuildProtectionSockets(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            AddPhBotCheck(p, "Use gear socket skills automatically", 10, 10, false);
            AddPhBotCheck(p, "Change primary weapon if broken (same item type)", 10, 34, false);
            NewPhBotNote(p, "TODO backend: ProtectionManager'a socket + kırık silah takası kuralı eklenecek.", 10, 62);
        }

        private void BuildProtectionPetReturn(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            // phbot/protection.md -> Pet Return 1..6
            var c1 = AddPhBotCheck(p, "Return when out of pet recovery kits (fellow)", 10, 10, ProtectionManager.ReturnOutOfPetRecoveryKits);
            c1.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetRecoveryKits = c1.Checked;

            var c2 = AddPhBotCheck(p, "Return when out of pet revive items", 10, 34, ProtectionManager.ReturnOutOfPetRevive);
            c2.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetRevive = c2.Checked;

            var c3 = AddPhBotCheck(p, "Return when out of pet feed items", 10, 58, ProtectionManager.ReturnOutOfPetFeed);
            c3.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetFeed = c3.Checked;

            var c4 = AddPhBotCheck(p, "Return when out of pet abnormal state potions", 10, 82, ProtectionManager.ReturnOutOfPetAbnormalPill);
            c4.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetAbnormalPill = c4.Checked;

            var c5 = AddPhBotCheck(p, "Return when out of transport recovery kits (wolf)", 10, 106, ProtectionManager.ReturnOutOfTransportRecoveryKits);
            c5.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfTransportRecoveryKits = c5.Checked;

            var c6 = AddPhBotCheck(p, "Return when the pick pet is full", 10, 130, ProtectionManager.ReturnFullPetInventory);
            c6.CheckedChanged += (s, e) => ProtectionManager.ReturnFullPetInventory = c6.Checked;
        }

        private void BuildProtectionBerserk(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            var c1 = AddPhBotCheck(p, "Use berserk against preferred monster types", 10, 10, CombatAIEngine.ZerkRarityBased);
            c1.CheckedChanged += (s, e) => CombatAIEngine.ZerkRarityBased = c1.Checked;

            var n1 = AddPhBotLabeledNumber(p, "Minimum attackers to trigger berserk", 10, 38, CombatAIEngine.ZerkMonsterCount);
            n1.ValueChanged += (s, e) => {
                CombatAIEngine.ZerkMonsterCount = (int)n1.Value;
                CombatAIEngine.ZerkMonsterCountEnabled = n1.Value > 0;
            };

            var c2 = AddPhBotCheck(p, "Use berserk while executing the script", 10, 66, CombatAIEngine.ZerkInScript);
            c2.CheckedChanged += (s, e) => CombatAIEngine.ZerkInScript = c2.Checked;

            var c3 = AddPhBotCheck(p, "Use berserker regeneration potion", 10, 90, CombatAIEngine.UseZerkPotion);
            c3.CheckedChanged += (s, e) => CombatAIEngine.UseZerkPotion = c3.Checked;

            var c4 = AddPhBotCheck(p, "Use Energy of Life potions", 10, 114, CombatAIEngine.UseEnergyOfLife);
            c4.CheckedChanged += (s, e) => CombatAIEngine.UseEnergyOfLife = c4.Checked;

            AddPhBotCheck(p, "Use Energy of Life berserk regeneration", 10, 138, false);
        }

        private void BuildProtectionMonsterPrefs(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            var lv = NewPhBotListView(10, 10, 460, 220, "Preference|250", "Priority|100");
            p.Controls.Add(lv);

            Action refreshList = () => {
                lv.Items.Clear();
                foreach (SRMob.Mob mobType in Enum.GetValues(typeof(SRMob.Mob)))
                {
                    var rule = CombatAIEngine.GetRule(mobType);
                    string state = rule.Prefer ? "Prefer" : (rule.Avoid ? "Avoid" : (rule.Berserk ? "Berserk" : "Normal"));
                    var lvi = new ListViewItem(mobType.ToString());
                    lvi.SubItems.Add(state);
                    lvi.Tag = mobType;
                    lv.Items.Add(lvi);
                }
            };
            refreshList();

            var btnUp = new Button();
            btnUp.Text = "Prefer"; btnUp.Font = PhBotFont(); btnUp.Size = new Size(80, 26);
            btnUp.Location = new Point(480, 10); btnUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUp.Click += (s, e) => {
                if (lv.SelectedItems.Count > 0 && lv.SelectedItems[0].Tag is SRMob.Mob type)
                {
                    CombatAIEngine.SetRule(type, false, true, false);
                    refreshList();
                }
            };

            var btnDown = new Button();
            btnDown.Text = "Avoid"; btnDown.Font = PhBotFont(); btnDown.Size = new Size(80, 26);
            btnDown.Location = new Point(480, 42); btnDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDown.Click += (s, e) => {
                if (lv.SelectedItems.Count > 0 && lv.SelectedItems[0].Tag is SRMob.Mob type)
                {
                    CombatAIEngine.SetRule(type, true, false, false);
                    refreshList();
                }
            };

            var btnZerk = new Button();
            btnZerk.Text = "Berserk"; btnZerk.Font = PhBotFont(); btnZerk.Size = new Size(80, 26);
            btnZerk.Location = new Point(480, 74); btnZerk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnZerk.Click += (s, e) => {
                if (lv.SelectedItems.Count > 0 && lv.SelectedItems[0].Tag is SRMob.Mob type)
                {
                    CombatAIEngine.SetRule(type, false, false, true);
                    refreshList();
                }
            };

            var btnReset = new Button();
            btnReset.Text = "Normal"; btnReset.Font = PhBotFont(); btnReset.Size = new Size(80, 26);
            btnReset.Location = new Point(480, 106); btnReset.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnReset.Click += (s, e) => {
                if (lv.SelectedItems.Count > 0 && lv.SelectedItems[0].Tag is SRMob.Mob type)
                {
                    CombatAIEngine.SetRule(type, false, false, false);
                    refreshList();
                }
            };

            p.Controls.Add(btnUp); p.Controls.Add(btnDown); p.Controls.Add(btnZerk); p.Controls.Add(btnReset);
            AddPhBotCheck(p, "Switch monster based on position in the list", 10, 240, false);
        }

        private void BuildProtectionDevilSpirit(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            var c1 = AddPhBotCheck(p, "Use Devil's Spirit", 10, 10, SkillManager.UseDevilSpirit);
            c1.CheckedChanged += (s, e) => {
                SkillManager.UseDevilSpirit = c1.Checked;
                if (c1.Checked) SkillManager.CheckDevilSpirit();
            };

            var c2 = AddPhBotCheck(p, "Use Angel's Spirit", 10, 34, SkillManager.UseDevilSpirit);
            c2.CheckedChanged += (s, e) => {
                SkillManager.UseDevilSpirit = c2.Checked;
                if (c2.Checked) SkillManager.CheckDevilSpirit();
            };

            var n1 = AddPhBotLabeledNumber(p, "Use below HP %", 10, 62, SkillManager.DevilSpiritHPPercent);
            n1.ValueChanged += (s, e) => SkillManager.DevilSpiritHPPercent = (byte)n1.Value;

            var n2 = AddPhBotLabeledNumber(p, "Re-cast delay (s)", 10, 90, SkillManager.DevilSpiritDelaySeconds);
            n2.ValueChanged += (s, e) => SkillManager.DevilSpiritDelaySeconds = (int)n2.Value;
        }

        private void BuildProtectionScrolls(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            var c1 = AddPhBotCheck(p, "Use return scrolls", 10, 10, ProtectionManager.UseReturnScrolls);
            c1.CheckedChanged += (s, e) => ProtectionManager.UseReturnScrolls = c1.Checked;

            var c2 = AddPhBotCheck(p, "Use a reverse return scroll when you die", 10, 34, ProtectionManager.UseReverseOnDeath);
            c2.CheckedChanged += (s, e) => ProtectionManager.UseReverseOnDeath = c2.Checked;

            var c3 = AddPhBotCheck(p, "Use a reverse return scroll after returning to town", 10, 58, ProtectionManager.UseReverseAfterTown);
            c3.CheckedChanged += (s, e) => ProtectionManager.UseReverseAfterTown = c3.Checked;

            var c4 = AddPhBotCheck(p, "Use movement speed drugs", 10, 82, ProtectionManager.UseSpeedDrugs || ReturnToAreaPolicy.UseSpeedDrug);
            c4.CheckedChanged += (s, e) => {
                ProtectionManager.UseSpeedDrugs = c4.Checked;
                ReturnToAreaPolicy.UseSpeedDrug = c4.Checked;
            };

            var c5 = AddPhBotCheck(p, "Only use speed drugs in the script", 10, 106, ProtectionManager.SpeedDrugsOnlyInScript);
            c5.CheckedChanged += (s, e) => ProtectionManager.SpeedDrugsOnlyInScript = c5.Checked;

            var c6 = AddPhBotCheck(p, "Use a repair hammer instead of returning", 10, 130, ProtectionManager.UseRepairHammer);
            c6.CheckedChanged += (s, e) => ProtectionManager.UseRepairHammer = c6.Checked;
        }

        private void BuildProtectionStats(Panel p)
        {
            if (p == null) return;
            try
            {
                p.Controls.Clear();
                p.BackColor = Color.White;
                GroupBox gbx = Character_gbxStatPoints;
                if (gbx != null)
                {
                    if (gbx.Parent != null && gbx.Parent != p)
                        gbx.Parent.Controls.Remove(gbx);
                    gbx.Font = PhBotFont();
                    gbx.ForeColor = Color.Black;
                    gbx.BackColor = Color.White;
                    gbx.Location = new Point(14, 14);
                    gbx.Size = new Size(380, 240);
                    gbx.Text = "Stat Points";
                    p.Controls.Add(gbx);

                    if (Character_lblSTR != null) { Character_lblSTR.Text = "Str"; Character_lblSTR.Location = new Point(16, 28); }
                    if (Character_btnAddSTR != null) { Character_btnAddSTR.Text = "+"; Character_btnAddSTR.SetBounds(55, 24, 28, 24); }
                    if (Character_lblAddSTR != null) { Character_lblAddSTR.SetBounds(95, 24, 50, 24); }
                    if (Character_lblINT != null) { Character_lblINT.Text = "Int"; Character_lblINT.Location = new Point(16, 60); }
                    if (Character_btnAddINT != null) { Character_btnAddINT.Text = "+"; Character_btnAddINT.SetBounds(55, 56, 28, 24); }
                    if (Character_lblAddINT != null) { Character_lblAddINT.SetBounds(95, 56, 50, 24); }
                    if (Character_lblStatPoints != null) { Character_lblStatPoints.Location = new Point(16, 96); Character_lblStatPoints.AutoSize = true; }
                    if (Character_cbxAutoStat != null) { Character_cbxAutoStat.Text = "Automatically add stat points"; Character_cbxAutoStat.Location = new Point(16, 130); Character_cbxAutoStat.AutoSize = true; }
                    if (Character_rbnAutoSTR != null) { Character_rbnAutoSTR.Text = "Pure STR"; Character_rbnAutoSTR.Location = new Point(20, 160); Character_rbnAutoSTR.AutoSize = true; }
                    if (Character_rbnAutoINT != null) { Character_rbnAutoINT.Text = "Pure INT"; Character_rbnAutoINT.Location = new Point(120, 160); Character_rbnAutoINT.AutoSize = true; }
                }
            }
            catch (Exception ex) { PhBotDebug("stats move: " + ex.Message); }
        }

        // ---------------------------------------------------------------
        // ADIM 3: Training alt-sekmeleri: Conditions + Collision + Settings.
        // (Area | Script | Trace | Combat xBot içeriğidir, korunur.)
        // ---------------------------------------------------------------
        private void EnsurePhBotTrainingExtraTabs()
        {
            try
            {
                if (TabPageV_Control01_Training_Panel == null) return;
                Panel host = TabPageV_Control01_Training_Panel;
                if (TabPageH_Training_Option03 == null) return;
                const string pre = "TabPageH_Training_Option";

                // Trace içeriği Option03'te durur; etiketi geri al (Conditions ayrı sekmedir).
                try { RenamePhBotSubTabs(TabPageH_Training_Option03, "Trace"); } catch { }
                try
                {
                    if (btnTrainingCombat != null)
                        RenamePhBotSubTabs(btnTrainingCombat, "Combat");
                }
                catch { }

                EnsureExtraHTab(host, TabPageH_Training_Option03, pre, "Conditions");
                EnsureExtraHTab(host, TabPageH_Training_Option03, pre, "Collision");
                EnsureExtraHTab(host, TabPageH_Training_Option03, pre, "Settings");

                var order = new List<string>();
                order.Add("TabPageH_Training_Option01");
                order.Add("TabPageH_Training_Option02");
                order.Add("TabPageH_Training_Option03");
                try
                {
                    if (btnTrainingCombat != null) order.Add(btnTrainingCombat.Name);
                }
                catch { }
                order.Add(pre + "Conditions");
                order.Add(pre + "Collision");
                order.Add(pre + "Settings");
                Control strip = TabPageH_Training_Option03.Parent;
                LayoutHStrip(strip, order.ToArray());

                BuildTrainingConditions(GetExtraHTabPanel(host, pre, "Conditions"));
                BuildTrainingCollision(GetExtraHTabPanel(host, pre, "Collision"));
                BuildTrainingSettings(GetExtraHTabPanel(host, pre, "Settings"));
            }
            catch (Exception ex) { PhBotDebug("training tabs: " + ex.Message); }
        }

        private void BuildTrainingConditions(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            var lv = NewPhBotListView(10, 10, 420, 220, "Level|80", "Switch to|320");
            p.Controls.Add(lv);
            AddPhBotCheck(p, "Change training area on level up", 10, 240, false);
            NewPhBotNote(p, "Sağ tık -> Add ile seviye koşulu eklenir (TODO backend).", 10, 268);
        }

        private void BuildTrainingCollision(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            // phbot/training-area.md -> Collision 1..7
            AddPhBotCheck(p, "Enable collision detection in the training area", 10, 10, false);
            AddPhBotCheck(p, "Navigate around obstacles", 10, 34, false);
            AddPhBotCheck(p, "Navigate to item drops (NavMesh)", 10, 58, false);
            AddPhBotCheck(p, "Disable Samarkand teleport", 10, 82, false);
            AddPhBotCheck(p, "Disable Alexandria teleport", 10, 106, false);
            AddPhBotCheck(p, "Disable Guide/Advice NPCs", 10, 130, false);
            AddPhBotCheck(p, "Ignore teleport level", 10, 154, false);
            NewPhBotNote(p, "NavMesh verisi yoksa collision seçenekleri etkisizdir (TODO backend).", 10, 182);
        }

        private void BuildTrainingSettings(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            // phbot/training-area.md -> Settings 1..15 + Script 1..11
            var c1 = AddPhBotCheck(p, "Don't walk around in the training area", 10, 10, ReturnToAreaPolicy.DontWalkAroundTrainingArea);
            c1.CheckedChanged += (s, e) => ReturnToAreaPolicy.DontWalkAroundTrainingArea = c1.Checked;

            AddPhBotCheck(p, "Use Treasure Boxes (Jangan cave)", 10, 34, false);
            AddPhBotCheck(p, "Use Easter Egg event NPCs", 10, 58, false);
            AddPhBotCheck(p, "Equip better items (low level only!)", 10, 82, false);
            AddPhBotCheck(p, "Summon flowers in the training area", 10, 106, false);

            var cHammer = AddPhBotCheck(p, "Use a repair hammer", 10, 130, ProtectionManager.UseRepairHammer);
            cHammer.CheckedChanged += (s, e) => ProtectionManager.UseRepairHammer = cHammer.Checked;

            var cZerkPot = AddPhBotCheck(p, "Use berserker regeneration potions", 10, 154, CombatAIEngine.UseZerkPotion);
            cZerkPot.CheckedChanged += (s, e) => CombatAIEngine.UseZerkPotion = cZerkPot.Checked;

            var cEol = AddPhBotCheck(p, "Use Energy of Life potions", 10, 178, CombatAIEngine.UseEnergyOfLife);
            cEol.CheckedChanged += (s, e) => CombatAIEngine.UseEnergyOfLife = cEol.Checked;

            AddPhBotCheck(p, "Use monster summon scrolls & Pandora's Box", 10, 202, false);
            AddPhBotCheck(p, "Wait for strong monsters before next summon", 10, 226, false);

            var cSkipTown = AddPhBotCheck(p, "Skip town script entirely", 10, 250, ReturnToAreaPolicy.SkipTownScript);
            cSkipTown.CheckedChanged += (s, e) => ReturnToAreaPolicy.SkipTownScript = cSkipTown.Checked;

            var cContTown = AddPhBotCheck(p, "Continue town scripts after reconnect", 10, 274, ReturnToAreaPolicy.ContinueTownScript);
            cContTown.CheckedChanged += (s, e) => ReturnToAreaPolicy.ContinueTownScript = cContTown.Checked;

            var cRetStuck = AddPhBotCheck(p, "Return when can't continue script", 10, 298, ReturnToAreaPolicy.ReturnIfScriptStuck);
            cRetStuck.CheckedChanged += (s, e) => ReturnToAreaPolicy.ReturnIfScriptStuck = cRetStuck.Checked;

            var cAvoidStatue = AddPhBotCheck(p, "Avoid Statue of Justice in the script", 10, 322, ReturnToAreaPolicy.AvoidStatueOfJustice);
            cAvoidStatue.CheckedChanged += (s, e) => ReturnToAreaPolicy.AvoidStatueOfJustice = cAvoidStatue.Checked;

            var nWalkDelay = AddPhBotLabeledNumber(p, "Script walk delay (ms)", 10, 350, ReturnToAreaPolicy.ScriptWalkDelay);
            nWalkDelay.Maximum = 10000;
            nWalkDelay.ValueChanged += (s, e) => ReturnToAreaPolicy.ScriptWalkDelay = (int)nWalkDelay.Value;

            var nGoBack = AddPhBotLabeledNumber(p, "Go back if stuck after (s)", 10, 378, ReturnToAreaPolicy.StuckGoBackSeconds);
            nGoBack.Maximum = 600;
            nGoBack.ValueChanged += (s, e) => ReturnToAreaPolicy.StuckGoBackSeconds = (int)nGoBack.Value;

            var nReturnStuck = AddPhBotLabeledNumber(p, "Return if stuck in script after (s)", 10, 406, ReturnToAreaPolicy.StuckReturnSeconds);
            nReturnStuck.Maximum = 600;
            nReturnStuck.ValueChanged += (s, e) => ReturnToAreaPolicy.StuckReturnSeconds = (int)nReturnStuck.Value;
        }

        // ---------------------------------------------------------------
        // ADIM 4: Attack ekstra sekmeleri zaten var
        // (EnsurePhBotAttackExtraTabs: Party Options/Resurrect/Healing/Lure/Attack Log).
        // Burada yalnızca şerit sırası doğrulanır.
        // ---------------------------------------------------------------
        private void EnsurePhBotAttackTabs()
        {
            try
            {
                if (TabPageH_Skills_Option01 == null || TabPageH_Skills_Option01.Parent == null) return;
                LayoutHStrip(TabPageH_Skills_Option01.Parent, new string[]
                {
                    "TabPageH_Skills_Option01", "TabPageH_Skills_Option02", "TabPageH_Skills_Option03",
                    "TabPageH_Skills_OptionPartyOptions", "TabPageH_Skills_OptionResurrect",
                    "TabPageH_Skills_OptionHealing", "TabPageH_Skills_OptionLure",
                    "TabPageH_Skills_OptionAttackLog"
                });
            }
            catch (Exception ex) { PhBotDebug("attack tabs: " + ex.Message); }
        }

        // ---------------------------------------------------------------
        // ADIM 5: Inventory alt-sekmeleri: Exchange | Job Tickets | Item | Gori.
        // Mevcut: Inventory (eşya) | Avatar | Storage | Pet korunur; yalnızca
        // yanlış "Exchange" etiketi düzeltilir (içerik avatar listesidir).
        // ---------------------------------------------------------------
        private void EnsurePhBotInventoryExtraTabs()
        {
            try
            {
                if (TabPageV_Control01_Inventory_Panel == null) return;
                Panel host = TabPageV_Control01_Inventory_Panel;
                if (TabPageH_Inventory_Option04 == null) return;
                const string pre = "TabPageH_Inventory_Option";

                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Exchange");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Job Tickets");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Item");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Gori Item Exchange");

                LayoutHStrip(TabPageH_Inventory_Option04.Parent, new string[]
                {
                    "TabPageH_Inventory_Option01", pre + "Exchange", "TabPageH_Inventory_Option02",
                    "TabPageH_Inventory_Option03", "TabPageH_Inventory_Option04",
                    pre + "JobTickets", pre + "Item", pre + "GoriItemExchange"
                });

                BuildInventoryExchange(GetExtraHTabPanel(host, pre, "Exchange"));
                BuildInventoryJobTickets(GetExtraHTabPanel(host, pre, "Job Tickets"));
                BuildInventoryItem(GetExtraHTabPanel(host, pre, "Item"));
                BuildInventoryGori(GetExtraHTabPanel(host, pre, "Gori Item Exchange"));
            }
            catch (Exception ex) { PhBotDebug("inventory tabs: " + ex.Message); }
        }

        private void BuildInventoryExchange(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            // phbot/inventory.md -> Exchange 1..13 (görsel klon)
            var lvPlayers = NewPhBotListView(10, 10, 250, 180, "Players|170", "Level|70");
            p.Controls.Add(lvPlayers);
            var lvMine = NewPhBotListView(270, 10, 250, 180, "My items|170", "Qty|70");
            p.Controls.Add(lvMine);
            var lvTheir = NewPhBotListView(530, 10, 250, 180, "Their items|170", "Qty|70");
            p.Controls.Add(lvTheir);
            AddPhBotLabeledNumber(p, "Gold given", 10, 200, 0);
            var btnReq = new Button();
            btnReq.Text = "Exchange"; btnReq.Font = PhBotFont(); btnReq.Size = new Size(90, 26);
            btnReq.Location = new Point(10, 230);
            var btnConfirm = new Button();
            btnConfirm.Text = "Confirm"; btnConfirm.Font = PhBotFont(); btnConfirm.Size = new Size(90, 26);
            btnConfirm.Location = new Point(106, 230);
            var btnApprove = new Button();
            btnApprove.Text = "Approve"; btnApprove.Font = PhBotFont(); btnApprove.Size = new Size(90, 26);
            btnApprove.Location = new Point(202, 230);
            var btnCancel = new Button();
            btnCancel.Text = "Cancel"; btnCancel.Font = PhBotFont(); btnCancel.Size = new Size(90, 26);
            btnCancel.Location = new Point(298, 230);
            p.Controls.Add(btnReq); p.Controls.Add(btnConfirm); p.Controls.Add(btnApprove); p.Controls.Add(btnCancel);
            NewPhBotNote(p, "TODO backend: oyuncu takas paketlerine bağlanacak.", 10, 264);
        }

        private void BuildInventoryJobTickets(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            AddPhBotCheck(p, "Exchange job tickets automatically", 10, 10, false);
            AddPhBotCheck(p, "Drop unwanted job rewards", 10, 34, false);
            var lv = NewPhBotListView(10, 62, 460, 180, "Ticket|300", "Count|100");
            p.Controls.Add(lv);
            NewPhBotNote(p, "TODO backend: Job ticket exchange motoru yok.", 10, 252);
        }

        private void BuildInventoryItem(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            AddPhBotCheck(p, "Inflate balloons during the Balloon event", 10, 10, false);
            AddPhBotCheck(p, "Awakening Enhancement: Devil/Angel Spirit", 10, 34, false);
            var btnStart = new Button();
            btnStart.Text = "Start"; btnStart.Font = PhBotFont(); btnStart.Size = new Size(90, 26);
            btnStart.Location = new Point(10, 62);
            var btnStop = new Button();
            btnStop.Text = "Stop"; btnStop.Font = PhBotFont(); btnStop.Size = new Size(90, 26);
            btnStop.Location = new Point(106, 62);
            p.Controls.Add(btnStart); p.Controls.Add(btnStop);
            NewPhBotNote(p, "TODO backend: event otomasyonu yok.", 10, 98);
        }

        private void BuildInventoryGori(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            AddPhBotCheck(p, "Enable Gori item exchange (Magic POP)", 10, 10, false);
            var lbl = new Label();
            lbl.Text = "Stop at white stat:"; lbl.Font = PhBotFont(); lbl.AutoSize = true;
            lbl.Location = new Point(10, 40);
            p.Controls.Add(lbl);
            var tbx = new TextBox();
            tbx.Font = PhBotFont(); tbx.BackColor = Color.White;
            tbx.Location = new Point(130, 37); tbx.Size = new Size(220, 22);
            p.Controls.Add(tbx);
            var lv = NewPhBotListView(10, 68, 460, 180, "Item|300", "Result|150");
            p.Controls.Add(lv);
            NewPhBotNote(p, "TODO backend: Gori exchange motoru yok.", 10, 258);
        }
    }
}
