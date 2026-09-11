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
        private Button EnsureExtraHTab(Panel hostV, Button anchor, string hPrefix, string key, string displayText = null)
        {
            if (hostV == null || anchor == null || anchor.Parent == null) return null;
            string title = displayText ?? key;
            string btnName = hPrefix + SanitizeName(key);
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
            else
            {
                b.Text = title;
                b.Size = new Size(Math.Max(70, TextRenderer.MeasureText(title, b.Font).Width + 20), 23);
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
                var orderedSet = new HashSet<string>(orderedNames, StringComparer.OrdinalIgnoreCase);
                foreach (Control c in strip.Controls)
                {
                    if (c is Button b && !orderedSet.Contains(b.Name))
                    {
                        b.Visible = false;
                    }
                }
            }
            catch { }
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
                var matches = hostV.Controls.Find(panelName, true);
                if (matches != null && matches.Length > 0 && matches[0] is Panel p)
                    return p;
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
                    "TabPageH_Character_Option02", pre + "Sockets", "TabPageH_Character_Option03",
                    pre + "PetReturn",
                    pre + "Berserk", pre + "MonsterPreferences", pre + "DevilsSpirit",
                    pre + "Scrolls", pre + "StatPoints", "TabPageH_Character_Option04"
                });

                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                if (TabPageH_Character_Option02 != null) TabPageH_Character_Option02.Text = isTR ? "Pot" : "Potions";
                if (TabPageH_Character_Option03 != null) TabPageH_Character_Option03.Text = isTR ? "Dönüş" : "Return";
                if (TabPageH_Character_Option04 != null) TabPageH_Character_Option04.Text = isTR ? "Diğerleri." : "Misc.";

                Control strip = TabPageH_Character_Option04.Parent;
                if (strip != null)
                {
                    Button bSock = strip.Controls[pre + "Sockets"] as Button;
                    if (bSock != null) bSock.Text = isTR ? "Socket" : "Sockets";
                    Button bPet = strip.Controls[pre + "PetReturn"] as Button;
                    if (bPet != null) bPet.Text = isTR ? "Pet Işınlama" : "Pet Return";
                    Button bZerk = strip.Controls[pre + "Berserk"] as Button;
                    if (bZerk != null) bZerk.Text = "Berserk";
                    Button bMobs = strip.Controls[pre + "MonsterPreferences"] as Button;
                    if (bMobs != null) bMobs.Text = isTR ? "Canavar Tercihleri" : "Monster Preferences";
                    Button bDev = strip.Controls[pre + "DevilsSpirit"] as Button;
                    if (bDev != null) bDev.Text = "Devil's Spirit";
                    Button bScr = strip.Controls[pre + "Scrolls"] as Button;
                    if (bScr != null) bScr.Text = isTR ? "Scrollar" : "Scrolls";
                    Button bStat = strip.Controls[pre + "StatPoints"] as Button;
                    if (bStat != null) bStat.Text = isTR ? "Stat Puanları" : "Stat Points";
                }

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
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.White;
            p.AutoScroll = false;

            int y = 14;
            AddPhBotCheck(p, "Socket Stone of Liberation", 20, y, false);

            string[] stones = new string[]
            {
                "Socket Stone of Stamina",
                "Socket Stone of Recovery",
                "Socket Stone of Magic Power",
                "Socket Stone of Block",
                "Socket Stone of Iron Wall"
            };

            for (int i = 0; i < stones.Length; i++)
            {
                y += 32;
                AddPhBotCheck(p, stones[i], 20, y, false);
                string stat = (i == 2) ? "MP <=" : "HP <=";
                var lbl = new Label { Text = stat, Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(220, y + 2) };
                var nud = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(265, y - 2), Size = new Size(48, 22), Value = 0 };
                var pct = new Label { Text = "%", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(318, y + 2) };
                p.Controls.AddRange(new Control[] { lbl, nud, pct });
            }

            y += 32;
            AddPhBotCheck(p, "Socket Stone of Concentration", 20, y, false);

            y += 32;
            AddPhBotCheck(p, "Socket Stone of Rapidity", 20, y, false);
        }

        private void BuildProtectionPetReturn(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.White;
            p.AutoScroll = false;
            bool isTR = LocalizationManager.CurrentLanguage == "TR";

            var gbx = new GroupBox
            {
                Text = isTR ? "Pot / Envanter" : "Potions / Inventory",
                Font = PhBotFont(),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Location = new Point(15, 12),
                Size = new Size(220, 270)
            };
            p.Controls.Add(gbx);

            var c1 = AddPhBotCheck(gbx, isTR ? "Pet recovery kit" : "Pet recovery kits", 15, 25, ProtectionManager.ReturnOutOfPetRecoveryKits);
            c1.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetRecoveryKits = c1.Checked;

            var c2 = AddPhBotCheck(gbx, isTR ? "Pet canlandırma" : "Pet revive", 15, 55, ProtectionManager.ReturnOutOfPetRevive);
            c2.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetRevive = c2.Checked;

            var c3 = AddPhBotCheck(gbx, isTR ? "Pet için HGP" : "Pet feed", 15, 85, ProtectionManager.ReturnOutOfPetFeed);
            c3.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetFeed = c3.Checked;

            var c4 = AddPhBotCheck(gbx, isTR ? "Anormal durum" : "Abnormal state", 15, 115, ProtectionManager.ReturnOutOfPetAbnormalPill);
            c4.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfPetAbnormalPill = c4.Checked;

            var c5 = AddPhBotCheck(gbx, isTR ? "Kervan recovery kiti" : "Transport recovery kits", 15, 145, ProtectionManager.ReturnOutOfTransportRecoveryKits);
            c5.CheckedChanged += (s, e) => ProtectionManager.ReturnOutOfTransportRecoveryKits = c5.Checked;

            var c6 = AddPhBotCheck(gbx, isTR ? "Grap pet dolu" : "Pick pet full", 15, 175, ProtectionManager.ReturnFullPetInventory);
            c6.CheckedChanged += (s, e) => ProtectionManager.ReturnFullPetInventory = c6.Checked;
        }

        private void BuildProtectionBerserk(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.White;
            p.AutoScroll = false;
            bool isTR = LocalizationManager.CurrentLanguage == "TR";

            var gbx = new GroupBox
            {
                Text = "Berserk",
                Font = PhBotFont(),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Location = new Point(15, 12),
                Size = new Size(340, 315)
            };
            p.Controls.Add(gbx);

            var cAtt = AddPhBotCheck(gbx, isTR ? "Saldıran canavar sayısı >" : "Monsters attacking >", 15, 18, CombatAIEngine.ZerkMonsterCountEnabled);
            var nAtt = new NumericUpDown
            {
                Font = PhBotFont(),
                BackColor = Color.White,
                Location = new Point(175, 16),
                Size = new Size(48, 22),
                Value = Math.Max(1, CombatAIEngine.ZerkMonsterCount > 0 ? CombatAIEngine.ZerkMonsterCount : 3)
            };
            nAtt.ValueChanged += (s, e) => {
                CombatAIEngine.ZerkMonsterCount = (int)nAtt.Value;
                Settings.SaveCharacterSettings();
            };
            cAtt.CheckedChanged += (s, e) => {
                CombatAIEngine.ZerkMonsterCountEnabled = cAtt.Checked;
                Settings.SaveCharacterSettings();
            };
            gbx.Controls.Add(nAtt);

            var cFull = AddPhBotCheck(gbx, isTR ? "Doldukça" : "Full", 15, 40, CombatAIEngine.ZerkWhenFull);
            cFull.CheckedChanged += (s, e) => { CombatAIEngine.ZerkWhenFull = cFull.Checked; Settings.SaveCharacterSettings(); };

            var cEven = AddPhBotCheck(gbx, isTR ? "Canavar saldırmasa bile" : "even if no monsters are attacking", 95, 40, CombatAIEngine.ZerkEvenIfNotAttacking);
            cEven.CheckedChanged += (s, e) => { CombatAIEngine.ZerkEvenIfNotAttacking = cEven.Checked; Settings.SaveCharacterSettings(); };

            Action<CheckBox, SRMob.Mob> bindMob = (cb, mt) => {
                cb.CheckedChanged += (s, e) => {
                    if (cb.Checked) CombatAIEngine.ZerkMobTypes.Add(mt);
                    else CombatAIEngine.ZerkMobTypes.Remove(mt);
                    Settings.SaveCharacterSettings();
                };
            };

            bindMob(AddPhBotCheck(gbx, "Giant", 15, 62, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.Giant)), SRMob.Mob.Giant);
            bindMob(AddPhBotCheck(gbx, isTR ? "Genel Parti" : "Party General", 15, 84, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.PartyGeneral)), SRMob.Mob.PartyGeneral);
            bindMob(AddPhBotCheck(gbx, isTR ? "Şampiyon Parti" : "Party Champion", 15, 106, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.PartyChampion)), SRMob.Mob.PartyChampion);
            bindMob(AddPhBotCheck(gbx, isTR ? "Giant Parti" : "Party Giant", 15, 128, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.PartyGiant)), SRMob.Mob.PartyGiant);
            bindMob(AddPhBotCheck(gbx, "Titan", 15, 150, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.Titan)), SRMob.Mob.Titan);
            bindMob(AddPhBotCheck(gbx, isTR ? "Elit" : "Elite", 15, 172, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.Elite)), SRMob.Mob.Elite);
            bindMob(AddPhBotCheck(gbx, isTR ? "Güçlü" : "Strong", 15, 194, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.Strong)), SRMob.Mob.Strong);
            bindMob(AddPhBotCheck(gbx, isTR ? "Etkinlik" : "Event", 15, 216, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.Event)), SRMob.Mob.Event);
            bindMob(AddPhBotCheck(gbx, "Unique", 15, 238, CombatAIEngine.ZerkMobTypes.Contains(SRMob.Mob.Unique)), SRMob.Mob.Unique);

            var cScript = AddPhBotCheck(gbx, isTR ? "Komut" : "Script", 15, 260, CombatAIEngine.ZerkInScript);
            cScript.CheckedChanged += (s, e) => { CombatAIEngine.ZerkInScript = cScript.Checked; Settings.SaveCharacterSettings(); };

            var cPillar = AddPhBotCheck(gbx, isTR ? "Pillar" : "Pillars", 15, 282, CombatAIEngine.ZerkPillars);
            cPillar.CheckedChanged += (s, e) => { CombatAIEngine.ZerkPillars = cPillar.Checked; Settings.SaveCharacterSettings(); };
        }

        private void BuildProtectionMonsterPrefs(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.White;
            p.AutoScroll = false;
            bool isTR = LocalizationManager.CurrentLanguage == "TR";

            int pw = p.Width > 200 ? p.Width : 620;
            int ph = p.Height > 200 ? p.Height : 380;

            var lv = NewPhBotListView(15, 15, pw - 60, ph - 70,
                (isTR ? "Tip" : "Type") + "|90",
                (isTR ? "No" : "ID") + "|70",
                (isTR ? "Adı" : "Name") + "|200",
                (isTR ? "Tercih" : "Preference") + "|160");
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            p.Controls.Add(lv);

            // Başlangıçta boş başlar, kullanıcının eklediği kuralları gösterir
            Action refreshList = () => {
                lv.Items.Clear();
                lock (CombatAIEngine.MonsterPreferences)
                {
                    foreach (var pEntry in CombatAIEngine.MonsterPreferences)
                    {
                        string colType = pEntry.MobType.HasValue ? pEntry.MobType.Value.ToString() : (isTR ? "Canavar" : "Monster");
                        string colNo = pEntry.MonsterId > 0 ? pEntry.MonsterId.ToString() : "";
                        string colName = !string.IsNullOrEmpty(pEntry.Name) ? pEntry.Name : (pEntry.MobType.HasValue ? pEntry.MobType.Value.ToString() : "");
                        string state = isTR ? "Hiçbiri" : "None";
                        if (pEntry.Preference == MonsterPreferenceType.Prefer) state = isTR ? "Tercih Et" : "Prefer";
                        else if (pEntry.Preference == MonsterPreferenceType.Avoid) state = isTR ? "Uzak Dur" : "Avoid";
                        else if (pEntry.Preference == MonsterPreferenceType.Ignore) state = isTR ? "Yoksay" : "Ignore";

                        var lvi = new ListViewItem(colType);
                        lvi.SubItems.Add(colNo);
                        lvi.SubItems.Add(colName);
                        lvi.SubItems.Add(state);
                        lvi.Tag = pEntry;
                        lv.Items.Add(lvi);
                    }
                }
            };
            refreshList();

            var btnUp = new Button { Text = "▲", Font = PhBotFont(), Size = new Size(26, 26), Location = new Point(pw - 38, 120), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var btnDown = new Button { Text = "▼", Font = PhBotFont(), Size = new Size(26, 26), Location = new Point(pw - 38, 155), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnUp.Click += (s, e) => {
                if (lv.SelectedIndices.Count > 0 && lv.SelectedIndices[0] > 0)
                {
                    int idx = lv.SelectedIndices[0];
                    lock (CombatAIEngine.MonsterPreferences)
                    {
                        var entry = CombatAIEngine.MonsterPreferences[idx];
                        CombatAIEngine.MonsterPreferences.RemoveAt(idx);
                        CombatAIEngine.MonsterPreferences.Insert(idx - 1, entry);
                    }
                    refreshList();
                    if (idx - 1 < lv.Items.Count)
                    {
                        lv.Items[idx - 1].Selected = true;
                        lv.Focus();
                    }
                    Settings.SaveCharacterSettings();
                }
            };
            btnDown.Click += (s, e) => {
                if (lv.SelectedIndices.Count > 0 && lv.SelectedIndices[0] < lv.Items.Count - 1)
                {
                    int idx = lv.SelectedIndices[0];
                    lock (CombatAIEngine.MonsterPreferences)
                    {
                        var entry = CombatAIEngine.MonsterPreferences[idx];
                        CombatAIEngine.MonsterPreferences.RemoveAt(idx);
                        CombatAIEngine.MonsterPreferences.Insert(idx + 1, entry);
                    }
                    refreshList();
                    if (idx + 1 < lv.Items.Count)
                    {
                        lv.Items[idx + 1].Selected = true;
                        lv.Focus();
                    }
                    Settings.SaveCharacterSettings();
                }
            };
            p.Controls.Add(btnUp);
            p.Controls.Add(btnDown);

            var cbSwitch = AddPhBotCheck(p, isTR ? "Listedeki pozisyona göre canavarı değiştirin" : "Switch monster based on position in the list", 15, ph - 46, CombatAIEngine.SwitchTargetByPosition);
            cbSwitch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cbSwitch.CheckedChanged += (s, e) => { CombatAIEngine.SwitchTargetByPosition = cbSwitch.Checked; Settings.SaveCharacterSettings(); };

            var cbQuest = AddPhBotCheck(p, isTR ? "Saldıran görev canavarlarına izin ver" : "Allow attacking quest monsters", 15, ph - 24, CombatAIEngine.AllowAttackingQuestMobs);
            cbQuest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cbQuest.CheckedChanged += (s, e) => { CombatAIEngine.AllowAttackingQuestMobs = cbQuest.Checked; Settings.SaveCharacterSettings(); };

            var ctx = new ContextMenuStrip();
            var mAdd = new ToolStripMenuItem(isTR ? "Ekle" : "Add");
            var mAddType = new ToolStripMenuItem(isTR ? "Tip" : "Type", null, (s, e) => {
                ShowMonsterTypeDialog(p.FindForm(), (chosenType) => {
                    SRMob.Mob? mobEnum = null;
                    if (Enum.TryParse<SRMob.Mob>(chosenType.Replace(" ", ""), out var parsedEnum))
                        mobEnum = parsedEnum;

                    var entry = new MonsterPreferenceEntry {
                        MobType = mobEnum,
                        MonsterId = 0,
                        Name = chosenType,
                        Preference = MonsterPreferenceType.None
                    };
                    lock (CombatAIEngine.MonsterPreferences)
                    {
                        CombatAIEngine.MonsterPreferences.Add(entry);
                    }
                    refreshList();
                    Settings.SaveCharacterSettings();
                });
            });
            var mAddMob = new ToolStripMenuItem(isTR ? "Canavar" : "Monster", null, (s, e) => {
                ShowMonsterSearchDialog(p.FindForm(), (mobId, mobName) => {
                    uint.TryParse(mobId, out uint idNum);
                    var entry = new MonsterPreferenceEntry {
                        MobType = null,
                        MonsterId = idNum,
                        Name = mobName,
                        Preference = MonsterPreferenceType.None
                    };
                    lock (CombatAIEngine.MonsterPreferences)
                    {
                        CombatAIEngine.MonsterPreferences.Add(entry);
                    }
                    refreshList();
                    Settings.SaveCharacterSettings();
                });
            });
            mAdd.DropDownItems.Add(mAddType);
            mAdd.DropDownItems.Add(mAddMob);
            ctx.Items.Add(mAdd);

            Action<MonsterPreferenceType> setPref = (pref) => {
                if (lv.SelectedItems.Count > 0)
                {
                    var it = lv.SelectedItems[0];
                    if (it.Tag is MonsterPreferenceEntry entry)
                    {
                        entry.Preference = pref;
                        refreshList();
                        Settings.SaveCharacterSettings();
                    }
                }
            };
            var mNone = new ToolStripMenuItem(isTR ? "Hiçbiri" : "None", null, (s, e) => setPref(MonsterPreferenceType.None));
            var mIgnore = new ToolStripMenuItem(isTR ? "Yoksay" : "Ignore", null, (s, e) => setPref(MonsterPreferenceType.Ignore));
            var mAvoid = new ToolStripMenuItem(isTR ? "Uzak Dur" : "Avoid", null, (s, e) => setPref(MonsterPreferenceType.Avoid));
            var mPrefer = new ToolStripMenuItem(isTR ? "Tercih Et" : "Prefer", null, (s, e) => setPref(MonsterPreferenceType.Prefer));
            var mSep = new ToolStripSeparator();
            var mRemove = new ToolStripMenuItem(isTR ? "Sil" : "Remove", null, (s, e) => {
                if (lv.SelectedItems.Count > 0)
                {
                    var it = lv.SelectedItems[0];
                    if (it.Tag is MonsterPreferenceEntry entry)
                    {
                        lock (CombatAIEngine.MonsterPreferences)
                        {
                            CombatAIEngine.MonsterPreferences.Remove(entry);
                        }
                        refreshList();
                        Settings.SaveCharacterSettings();
                    }
                }
            });
            ctx.Items.AddRange(new ToolStripItem[] { mNone, mIgnore, mAvoid, mPrefer, mSep, mRemove });
            lv.ContextMenuStrip = ctx;
        }

        private void ShowMonsterSearchDialog(Form parentForm, Action<string, string> onAdd)
        {
            try
            {
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                using (Form dlg = new Form())
                {
                    dlg.Text = "Monster Tercihleri";
                    dlg.Size = new Size(380, 440);
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dlg.MaximizeBox = false;
                    dlg.MinimizeBox = false;
                    dlg.BackColor = Color.White;

                    var lv = new ListView
                    {
                        Location = new Point(12, 12),
                        Size = new Size(340, 335),
                        View = View.Details,
                        FullRowSelect = true,
                        GridLines = true,
                        MultiSelect = false,
                        Font = PhBotFont()
                    };
                    lv.Columns.Add(isTR ? "No" : "ID", 70);
                    lv.Columns.Add(isTR ? "Adı" : "Name", 245);
                    dlg.Controls.Add(lv);

                    var txtSearch = new TextBox
                    {
                        Location = new Point(12, 360),
                        Size = new Size(245, 22),
                        Font = PhBotFont(),
                        BackColor = Color.White
                    };
                    var btnSearch = new Button
                    {
                        Text = isTR ? "Ara" : "Search",
                        Location = new Point(265, 358),
                        Size = new Size(85, 26),
                        Font = PhBotFont(),
                        UseVisualStyleBackColor = true
                    };
                    dlg.Controls.AddRange(new Control[] { txtSearch, btnSearch });

                    Action doSearch = () => {
                        lv.Items.Clear();
                        string q = txtSearch.Text.Trim();
                        var list = Game.DataManager.QueryMonsters(q);
                        if (list != null && list.Count > 0)
                        {
                            foreach (var row in list)
                            {
                                var lvi = new ListViewItem(row["id"] ?? "");
                                lvi.SubItems.Add(row["name"] ?? row["servername"] ?? "");
                                lv.Items.Add(lvi);
                            }
                        }
                        else
                        {
                            var samples = new string[] {
                                "6|Mangyang", "7|Mangyang", "8|Mangyang",
                                "10|Big White Spider", "11|Big White Spider", "12|Big White Spider",
                                "14|Small White Spider", "15|Small White Spider",
                                "18|Old Weasel", "19|Weasel",
                                "22|Water Ghost Slave", "23|Water Ghost",
                                "26|Broken Stone Ghost", "27|Stone Ghost",
                                "30|Tomb Stone", "31|Tomb Stone Ghost",
                                "35|Yeha Demon", "38|Bandit Bowman", "39|Bandit",
                                "43|Black Robber Bowman", "44|Black Robber",
                                "48|Tiger", "49|White Tiger",
                                "55|Chakji", "56|Chakji Worker",
                                "60|Ghost Bug", "61|Devil Bug"
                            };
                            foreach (var s in samples)
                            {
                                string[] parts = s.Split('|');
                                if (string.IsNullOrEmpty(q) || parts[1].IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    var lvi = new ListViewItem(parts[0]);
                                    lvi.SubItems.Add(parts[1]);
                                    lv.Items.Add(lvi);
                                }
                            }
                        }
                    };

                    btnSearch.Click += (s, e) => doSearch();
                    txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; doSearch(); } };

                    lv.DoubleClick += (s, e) => {
                        if (lv.SelectedItems.Count > 0)
                        {
                            string id = lv.SelectedItems[0].Text;
                            string name = lv.SelectedItems[0].SubItems.Count > 1 ? lv.SelectedItems[0].SubItems[1].Text : "";
                            onAdd(id, name);
                            dlg.DialogResult = DialogResult.OK;
                            dlg.Close();
                        }
                    };

                    doSearch();
                    dlg.ShowDialog(parentForm);
                }
            }
            catch (Exception ex) { PhBotDebug("Monster search dlg: " + ex.Message); }
        }

        private void ShowMonsterTypeDialog(Form parentForm, Action<string> onAdd)
        {
            try
            {
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                using (Form dlg = new Form())
                {
                    dlg.Text = isTR ? "Canavar Tipi" : "Monster Type";
                    dlg.Size = new Size(250, 130);
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dlg.MaximizeBox = false;
                    dlg.MinimizeBox = false;
                    dlg.BackColor = Color.White;

                    var cb = new ComboBox
                    {
                        Location = new Point(20, 18),
                        Size = new Size(195, 24),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = PhBotFont()
                    };
                    cb.Items.AddRange(new object[] {
                        "General", "Champion", "Giant", "Strong",
                        "Party General", "Party Champion", "Party Giant",
                        "Titan", "Elite", "Event"
                    });
                    cb.SelectedIndex = 0;
                    dlg.Controls.Add(cb);

                    var btnOk = new Button
                    {
                        Text = isTR ? "Tamam" : "OK",
                        Location = new Point(75, 54),
                        Size = new Size(85, 26),
                        Font = PhBotFont(),
                        DialogResult = DialogResult.OK,
                        UseVisualStyleBackColor = true
                    };
                    dlg.Controls.Add(btnOk);
                    dlg.AcceptButton = btnOk;

                    if (dlg.ShowDialog(parentForm) == DialogResult.OK && cb.SelectedItem != null)
                    {
                        onAdd(cb.SelectedItem.ToString());
                    }
                }
            }
            catch (Exception ex) { PhBotDebug("Monster type dlg: " + ex.Message); }
        }

        private void BuildProtectionDevilSpirit(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.White;
            p.AutoScroll = false;
            bool isTR = LocalizationManager.CurrentLanguage == "TR";

            var gbx = new GroupBox
            {
                Text = isTR ? "Canavarlar" : "Monsters",
                Font = PhBotFont(),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Location = new Point(15, 12),
                Size = new Size(340, 290)
            };
            p.Controls.Add(gbx);

            var cAtt = AddPhBotCheck(gbx, isTR ? "Saldıran canavar sayısı >" : "Monsters attacking >", 15, 18, SkillManager.DevilMonsterCountEnabled);
            var nAtt = new NumericUpDown
            {
                Font = PhBotFont(),
                BackColor = Color.White,
                Location = new Point(175, 16),
                Size = new Size(48, 22),
                Value = Math.Max(1, SkillManager.DevilMonsterCount > 0 ? SkillManager.DevilMonsterCount : 3)
            };
            nAtt.ValueChanged += (s, e) => {
                SkillManager.DevilMonsterCount = (int)nAtt.Value;
                Settings.SaveCharacterSettings();
            };
            cAtt.CheckedChanged += (s, e) => {
                SkillManager.DevilMonsterCountEnabled = cAtt.Checked;
                Settings.SaveCharacterSettings();
            };
            gbx.Controls.Add(nAtt);

            var cReady = AddPhBotCheck(gbx, isTR ? "Hazırsa" : "Ready", 15, 40, SkillManager.DevilWhenReady);
            cReady.CheckedChanged += (s, e) => {
                SkillManager.DevilWhenReady = cReady.Checked;
                SkillManager.UseDevilSpirit = cReady.Checked;
                Settings.SaveCharacterSettings();
                if (cReady.Checked) SkillManager.CheckDevilSpirit();
            };

            var cEven = AddPhBotCheck(gbx, isTR ? "Canavar saldırmasa bile" : "even if no monsters are attacking", 95, 40, SkillManager.DevilEvenIfNotAttacking);
            cEven.CheckedChanged += (s, e) => {
                SkillManager.DevilEvenIfNotAttacking = cEven.Checked;
                Settings.SaveCharacterSettings();
            };

            Action<CheckBox, SRMob.Mob> bindMob = (cb, mt) => {
                cb.CheckedChanged += (s, e) => {
                    if (cb.Checked) SkillManager.DevilMobTypes.Add(mt);
                    else SkillManager.DevilMobTypes.Remove(mt);
                    Settings.SaveCharacterSettings();
                };
            };

            bindMob(AddPhBotCheck(gbx, "Giant", 15, 62, SkillManager.DevilMobTypes.Contains(SRMob.Mob.Giant)), SRMob.Mob.Giant);
            bindMob(AddPhBotCheck(gbx, isTR ? "Genel Parti" : "Party General", 15, 84, SkillManager.DevilMobTypes.Contains(SRMob.Mob.PartyGeneral)), SRMob.Mob.PartyGeneral);
            bindMob(AddPhBotCheck(gbx, isTR ? "Şampiyon Parti" : "Party Champion", 15, 106, SkillManager.DevilMobTypes.Contains(SRMob.Mob.PartyChampion)), SRMob.Mob.PartyChampion);
            bindMob(AddPhBotCheck(gbx, isTR ? "Giant Parti" : "Party Giant", 15, 128, SkillManager.DevilMobTypes.Contains(SRMob.Mob.PartyGiant)), SRMob.Mob.PartyGiant);
            bindMob(AddPhBotCheck(gbx, "Titan", 15, 150, SkillManager.DevilMobTypes.Contains(SRMob.Mob.Titan)), SRMob.Mob.Titan);
            bindMob(AddPhBotCheck(gbx, isTR ? "Elit" : "Elite", 15, 172, SkillManager.DevilMobTypes.Contains(SRMob.Mob.Elite)), SRMob.Mob.Elite);
            bindMob(AddPhBotCheck(gbx, isTR ? "Güçlü" : "Strong", 15, 194, SkillManager.DevilMobTypes.Contains(SRMob.Mob.Strong)), SRMob.Mob.Strong);
            bindMob(AddPhBotCheck(gbx, isTR ? "Etkinlik" : "Event", 15, 216, SkillManager.DevilMobTypes.Contains(SRMob.Mob.Event)), SRMob.Mob.Event);
            bindMob(AddPhBotCheck(gbx, "Unique", 15, 238, SkillManager.DevilMobTypes.Contains(SRMob.Mob.Unique)), SRMob.Mob.Unique);

            var cPillar = AddPhBotCheck(gbx, isTR ? "Pillar" : "Pillars", 15, 260, SkillManager.DevilPillars);
            cPillar.CheckedChanged += (s, e) => {
                SkillManager.DevilPillars = cPillar.Checked;
                Settings.SaveCharacterSettings();
            };
        }

        private void BuildProtectionScrolls(Panel p)
        {
            if (p == null) return;
            p.Controls.Clear();
            p.BackColor = Color.White;
            p.AutoScroll = false;
            bool isTR = LocalizationManager.CurrentLanguage == "TR";

            int x1 = 20;
            AddPhBotCheck(p, "Accuracy", x1, 18, false);
            AddPhBotCheck(p, "Strength", x1, 44, false);
            AddPhBotCheck(p, "Intelligence", x1, 70, false);
            AddPhBotCheck(p, isTR ? "Hasar Arttırma" : "Damage Increase", x1, 96, false);
            AddPhBotCheck(p, isTR ? "Hasar Düşürme" : "Damage Absorption", x1, 122, false);
            AddPhBotCheck(p, isTR ? "HP Arttırma" : "HP Increase", x1, 148, false);
            AddPhBotCheck(p, isTR ? "MP Arttırma" : "MP Increase", x1, 174, false);
            AddPhBotCheck(p, "Subscription Card", x1, 200, false);
            AddPhBotCheck(p, "Skill Point", x1, 226, false);
            AddPhBotCheck(p, "Evasion", x1, 252, false);

            int x2 = 210;
            AddPhBotCheck(p, isTR ? "EXP Arttırma" : "EXP Increase", x2, 18, false);
            AddPhBotCheck(p, isTR ? "SP EXP Arttırma" : "SP EXP Increase", x2, 44, false);
            var cSpd = AddPhBotCheck(p, isTR ? "Hızlı Koşma" : "Speed", x2, 70, ProtectionManager.UseSpeedDrugs || ReturnToAreaPolicy.UseSpeedDrug);
            cSpd.CheckedChanged += (s, e) => {
                ProtectionManager.UseSpeedDrugs = cSpd.Checked;
                ReturnToAreaPolicy.UseSpeedDrug = cSpd.Checked;
            };
            AddPhBotCheck(p, "Trigger", x2, 96, false);
            AddPhBotCheck(p, isTR ? "EXP Kağıtları (Booster)" : "EXP Tickets", x2, 122, false);
            AddPhBotCheck(p, isTR ? "SP EXP Kağıtları (Booster)" : "SP EXP Tickets", x2, 148, false);

            var btnEn = new Button { Text = isTR ? "Etkinleştir" : "Enable", Font = PhBotFont(), Location = new Point(430, 16), Size = new Size(82, 26) };
            var btnDis = new Button { Text = isTR ? "Devre dışı bırak" : "Disable", Font = PhBotFont(), Location = new Point(516, 16), Size = new Size(110, 26) };
            var cbBotOnly = AddPhBotCheck(p, isTR ? "Sadece bottayken kullan" : "Only use while botting", 430, 52, false);
            var cbTraceOnly = AddPhBotCheck(p, isTR ? "Sadece takipteyken kullan" : "Only use while tracing", 430, 78, false);
            p.Controls.Add(btnEn);
            p.Controls.Add(btnDis);
        }

        private void BuildProtectionStats(Panel p)
        {
            if (p == null) return;
            try
            {
                p.Controls.Clear();
                p.BackColor = Color.White;
                p.AutoScroll = false;
                bool isTR = LocalizationManager.CurrentLanguage == "TR";

                // 1. Str [ 0 ] [ + ]
                var lStr = new Label { Text = "Str", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(25, 20) };
                var txtStr = new TextBox { Text = "0", Font = PhBotFont(), ReadOnly = true, BackColor = Color.White, ForeColor = Color.Black, Location = new Point(58, 18), Size = new Size(40, 22), TextAlign = HorizontalAlignment.Center };
                var btnAddStr = new Button { Text = "+", Font = PhBotFont(), Size = new Size(24, 22), Location = new Point(104, 18) };
                btnAddStr.Click += (s, e) => {
                    try { if (Character_btnAddSTR != null) Character_btnAddSTR.PerformClick(); } catch { }
                };

                // 2. Int [ 0 ] [ + ]
                var lInt = new Label { Text = "Int", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(25, 48) };
                var txtInt = new TextBox { Text = "0", Font = PhBotFont(), ReadOnly = true, BackColor = Color.White, ForeColor = Color.Black, Location = new Point(58, 46), Size = new Size(40, 22), TextAlign = HorizontalAlignment.Center };
                var btnAddInt = new Button { Text = "+", Font = PhBotFont(), Size = new Size(24, 22), Location = new Point(104, 46) };
                btnAddInt.Click += (s, e) => {
                    try { if (Character_btnAddINT != null) Character_btnAddINT.PerformClick(); } catch { }
                };

                // 3. Mevcut Gelişim Puanı: 0
                string availCount = "0";
                if (Character_lblStatPoints != null && !string.IsNullOrEmpty(Character_lblStatPoints.Text) && Character_lblStatPoints.Text != "- - -")
                    availCount = Character_lblStatPoints.Text.Trim();
                var lAvail = new Label { Text = (isTR ? "Mevcut Gelişim Puanı: " : "Stat points available: ") + availCount, Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(25, 82) };
                if (Character_lblStatPoints != null)
                {
                    Character_lblStatPoints.TextChanged += (s, e) => {
                        string val = Character_lblStatPoints.Text != "- - -" ? Character_lblStatPoints.Text.Trim() : "0";
                        lAvail.Text = (isTR ? "Mevcut Gelişim Puanı: " : "Stat points available: ") + val;
                    };
                }

                // 4. [ ] Otomatik gelişim puanı ver
                bool autoStat = Character_cbxAutoStat != null && Character_cbxAutoStat.Checked;
                var cbAuto = new CheckBox { Text = isTR ? "Otomatik gelişim puanı ver" : "Automatically add stat points", Checked = autoStat, Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(25, 114) };
                cbAuto.CheckedChanged += (s, e) => {
                    if (Character_cbxAutoStat != null) Character_cbxAutoStat.Checked = cbAuto.Checked;
                };

                // 5. Str [ 0 ]
                var lStrN = new Label { Text = "Str", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(25, 146) };
                var nStr = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black, Location = new Point(58, 144), Size = new Size(45, 22), Maximum = 3, Value = StatPointManager.TargetSTR };
                nStr.ValueChanged += (s, e) => {
                    StatPointManager.TargetSTR = (int)nStr.Value;
                    if (nudAutoStatSTR != null) try { nudAutoStatSTR.Value = nStr.Value; } catch { }
                };

                // 6. Int [ 0 ]
                var lIntN = new Label { Text = "Int", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(25, 176) };
                var nInt = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black, Location = new Point(58, 174), Size = new Size(45, 22), Maximum = 3, Value = StatPointManager.TargetINT };
                nInt.ValueChanged += (s, e) => {
                    StatPointManager.TargetINT = (int)nInt.Value;
                    if (nudAutoStatINT != null) try { nudAutoStatINT.Value = nInt.Value; } catch { }
                };

                p.Controls.AddRange(new Control[] { lStr, txtStr, btnAddStr, lInt, txtInt, btnAddInt, lAvail, cbAuto, lStrN, nStr, lIntN, nInt });
            }
            catch (Exception ex) { PhBotDebug("stats build: " + ex.Message); }
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
                bool isTR = LocalizationManager.CurrentLanguage == "TR";

                Control strip = TabPageH_Training_Option03.Parent;
                try { if (TabPageH_Training_Option03 != null) TabPageH_Training_Option03.Visible = false; } catch { }
                try
                {
                    if (btnTrainingCombat != null)
                    {
                        btnTrainingCombat.Visible = false;
                        if (strip != null && strip.Controls.Contains(btnTrainingCombat))
                            strip.Controls.Remove(btnTrainingCombat);
                    }
                    if (strip != null && strip.Controls.ContainsKey("TabPageH_Training_Option04"))
                    {
                        var bOld = strip.Controls["TabPageH_Training_Option04"];
                        bOld.Visible = false;
                        strip.Controls.Remove(bOld);
                    }
                    if (host != null && host.Controls.ContainsKey("TabPageH_Training_Option04_Panel"))
                    {
                        var pOld = host.Controls["TabPageH_Training_Option04_Panel"];
                        pOld.Visible = false;
                        host.Controls.Remove(pOld);
                    }
                }
                catch { }

                RenamePhBotSubTabs(TabPageH_Training_Option01, isTR ? "Kasılma" : "Training");
                RenamePhBotSubTabs(TabPageH_Training_Option02, isTR ? "Komut" : "Script");

                EnsureExtraHTab(host, TabPageH_Training_Option03, pre, "Conditions", isTR ? "Koşullar" : "Conditions");
                EnsureExtraHTab(host, TabPageH_Training_Option03, pre, "Collision", isTR ? "Çarpışma" : "Collision");
                EnsureExtraHTab(host, TabPageH_Training_Option03, pre, "Options", isTR ? "Seçenekler" : "Options");

                var order = new List<string>();
                order.Add("TabPageH_Training_Option01");
                order.Add(pre + "Conditions");
                order.Add(pre + "Collision");
                order.Add(pre + "Options");
                order.Add("TabPageH_Training_Option02");
                LayoutHStrip(strip, order.ToArray());

                BuildTrainingConditions(GetExtraHTabPanel(host, pre, "Conditions"));
                BuildTrainingCollision(GetExtraHTabPanel(host, pre, "Collision"));
                BuildTrainingSettings(GetExtraHTabPanel(host, pre, "Options"));
            }
            catch (Exception ex) { PhBotDebug("training tabs: " + ex.Message); }
        }

        private void EnsurePhBotPartyExtraTabs()
        {
            try
            {
                if (TabPageV_Control01_Party_Panel == null) return;
                Panel host = TabPageV_Control01_Party_Panel;
                if (TabPageH_Party_Option04 == null) return;
                const string pre = "TabPageH_Party_Option";

                EnsureExtraHTab(host, TabPageH_Party_Option04, pre, "Taxi Options");

                LayoutHStrip(TabPageH_Party_Option04.Parent, new string[]
                {
                    "TabPageH_Party_Option01", "TabPageH_Party_Option02",
                    "TabPageH_Party_Option03", "TabPageH_Party_Option04",
                    pre + "TaxiOptions"
                });
            }
            catch (Exception ex) { PhBotDebug("party tabs: " + ex.Message); }
        }

        private void BuildTrainingConditions(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            var btnClear = new Button();
            btnClear.Text = "Clear";
            btnClear.Font = PhBotFont();
            btnClear.Location = new Point(14, 14);
            btnClear.Size = new Size(80, 26);
            Classicize(btnClear);
            p.Controls.Add(btnClear);

            var lv = NewPhBotListView(105, 14, Math.Max(400, p.ClientSize.Width - 120), Math.Max(200, p.ClientSize.Height - 28), "Condition|220", "Change To|220");
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            lv.ContextMenuStrip = new ContextMenuStrip();

            var addMenu = new ToolStripMenuItem("Add");
            var itemLevel = new ToolStripMenuItem("Level");
            var itemTime = new ToolStripMenuItem("Time");
            var itemNotAttacked = new ToolStripMenuItem("Not Attacked");
            var itemScriptFinished = new ToolStripMenuItem("Script Finished");
            addMenu.DropDownItems.AddRange(new ToolStripItem[] { itemLevel, itemTime, itemNotAttacked, itemScriptFinished });

            var removeMenu = new ToolStripMenuItem("Remove");
            removeMenu.Click += (s, e) => {
                if (lv.SelectedItems.Count > 0)
                {
                    foreach (ListViewItem it in lv.SelectedItems)
                        lv.Items.Remove(it);
                }
            };

            EventHandler onAddCondition = (s, e) => {
                string condType = (s as ToolStripItem)?.Text ?? "Level";
                Form dlg = new Form();
                dlg.Text = "Condition";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ClientSize = new Size(250, 110);
                dlg.BackColor = SystemColors.Control;
                dlg.Font = PhBotFont();

                Label lbl = new Label();
                lbl.Text = "Select the condition type:";
                lbl.Location = new Point(12, 12);
                lbl.AutoSize = true;
                dlg.Controls.Add(lbl);

                ComboBox cb = new ComboBox();
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.Items.AddRange(new object[] { "Level", "Time", "Not Attacked", "Script Finished" });
                cb.SelectedItem = condType;
                cb.Location = new Point(14, 34);
                cb.Size = new Size(220, 22);
                dlg.Controls.Add(cb);

                Button btnOk = new Button();
                btnOk.Text = "OK";
                btnOk.Location = new Point(70, 70);
                btnOk.Size = new Size(75, 24);
                btnOk.DialogResult = DialogResult.OK;
                dlg.Controls.Add(btnOk);

                Button btnCancel = new Button();
                btnCancel.Text = "Cancel";
                btnCancel.Location = new Point(155, 70);
                btnCancel.Size = new Size(75, 24);
                btnCancel.DialogResult = DialogResult.Cancel;
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(p) == DialogResult.OK)
                {
                    string selected = cb.SelectedItem?.ToString() ?? "Level";
                    ListViewItem lvi = new ListViewItem(selected);
                    lvi.SubItems.Add("");
                    lv.Items.Add(lvi);
                }
            };

            itemLevel.Click += onAddCondition;
            itemTime.Click += onAddCondition;
            itemNotAttacked.Click += onAddCondition;
            itemScriptFinished.Click += onAddCondition;

            lv.ContextMenuStrip.Items.Add(addMenu);
            lv.ContextMenuStrip.Items.Add(removeMenu);

            btnClear.Click += (s, e) => {
                lv.Items.Clear();
            };

            p.Controls.Add(lv);
        }

        private void BuildTrainingCollision(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            bool isTR = LocalizationManager.CurrentLanguage == "TR";
            int y = 14;
            int step = 28;

            // media_1789163191778.png -> Çarpışma 8 checkbox
            var c1 = AddPhBotCheck(p, isTR ? "Kasılma alanında engel tespit etmeyi aktifleştir" : "Enable collision detection in the training area", 18, y, CollisionPolicy.EnableCollisionInTrainingArea);
            c1.Name = "PhBot_Collision_EnableInTraining";
            c1.CheckedChanged += (s, e) => { CollisionPolicy.EnableCollisionInTrainingArea = c1.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c2 = AddPhBotCheck(p, isTR ? "Engellerin etrafından dolaş" : "Navigate around obstacles", 18, y, CollisionPolicy.NavigateAroundObstacles);
            c2.Name = "PhBot_Collision_NavigateAround";
            c2.CheckedChanged += (s, e) => { CollisionPolicy.NavigateAroundObstacles = c2.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c3 = AddPhBotCheck(p, isTR ? "Eşyaları toplarken engellerin etrafından dolaş" : "Navigate around obstacles while picking items", 18, y, CollisionPolicy.NavigateAroundObstaclesWhilePicking);
            c3.Name = "PhBot_Collision_NavigateAroundPicking";
            c3.CheckedChanged += (s, e) => { CollisionPolicy.NavigateAroundObstaclesWhilePicking = c3.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c4 = AddPhBotCheck(p, isTR ? "Samarkand'a ışınlanmayı devre dışı bırak" : "Disable teleport to Samarkand", 18, y, CollisionPolicy.DisableTeleportSamarkand);
            c4.Name = "PhBot_Collision_DisableSamarkand";
            c4.CheckedChanged += (s, e) => { CollisionPolicy.DisableTeleportSamarkand = c4.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c5 = AddPhBotCheck(p, isTR ? "Alexandria'ya ışınlanmayı devre dışı bırak" : "Disable teleport to Alexandria", 18, y, CollisionPolicy.DisableTeleportAlexandria);
            c5.Name = "PhBot_Collision_DisableAlexandria";
            c5.CheckedChanged += (s, e) => { CollisionPolicy.DisableTeleportAlexandria = c5.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c6 = AddPhBotCheck(p, isTR ? "Guide/Advice NPCs Devre dışı bırak" : "Disable Guide/Advice NPCs", 18, y, CollisionPolicy.DisableGuideAdviceNPCs);
            c6.Name = "PhBot_Collision_DisableGuideAdvice";
            c6.CheckedChanged += (s, e) => { CollisionPolicy.DisableGuideAdviceNPCs = c6.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c7 = AddPhBotCheck(p, isTR ? "Taklamakan'ı devre dışı bırak" : "Disable Taklamakan", 18, y, CollisionPolicy.DisableTaklamakan);
            c7.Name = "PhBot_Collision_DisableTaklamakan";
            c7.CheckedChanged += (s, e) => { CollisionPolicy.DisableTaklamakan = c7.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c8 = AddPhBotCheck(p, isTR ? "Işınlanma seviyesini yoksay" : "Ignore teleport level", 18, y, CollisionPolicy.IgnoreTeleportLevel);
            c8.Name = "PhBot_Collision_IgnoreLevel";
            c8.CheckedChanged += (s, e) => { CollisionPolicy.IgnoreTeleportLevel = c8.Checked; Settings.SaveCharacterSettings(); };
        }

        public void RefreshCollisionControls()
        {
            try
            {
                Panel host = TabPageV_Control01_Training_Panel;
                if (host == null) return;
                Panel p = GetExtraHTabPanel(host, "TabPageH_Training_Option", "Collision");
                if (p == null) return;

                void SetCheck(string name, bool val)
                {
                    if (p.Controls.ContainsKey(name) && p.Controls[name] is CheckBox cb && cb.Checked != val)
                        cb.Checked = val;
                }

                SetCheck("PhBot_Collision_EnableInTraining", CollisionPolicy.EnableCollisionInTrainingArea);
                SetCheck("PhBot_Collision_NavigateAround", CollisionPolicy.NavigateAroundObstacles);
                SetCheck("PhBot_Collision_NavigateAroundPicking", CollisionPolicy.NavigateAroundObstaclesWhilePicking);
                SetCheck("PhBot_Collision_DisableSamarkand", CollisionPolicy.DisableTeleportSamarkand);
                SetCheck("PhBot_Collision_DisableAlexandria", CollisionPolicy.DisableTeleportAlexandria);
                SetCheck("PhBot_Collision_DisableGuideAdvice", CollisionPolicy.DisableGuideAdviceNPCs);
                SetCheck("PhBot_Collision_DisableTaklamakan", CollisionPolicy.DisableTaklamakan);
                SetCheck("PhBot_Collision_IgnoreLevel", CollisionPolicy.IgnoreTeleportLevel);
            }
            catch { }
        }

        private void BuildTrainingSettings(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            bool isTR = LocalizationManager.CurrentLanguage == "TR";
            int y = 12;
            const int step = 20;

            // Sol kolon 1..15 (media_1789163198989.png / media_1789167056612.png)
            var c1 = AddPhBotCheck(p, isTR ? "Kasılma alanında dolaşma" : "Don't walk around", 18, y, TrainingOptionsPolicy.DontWalkAroundTrainingArea);
            c1.Name = "PhBot_Training_DontWalkAround";
            c1.CheckedChanged += (s, e) => { TrainingOptionsPolicy.DontWalkAroundTrainingArea = c1.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c2 = AddPhBotCheck(p, isTR ? "Treasure Boxes kullan" : "Use Treasure Boxes", 18, y, TrainingOptionsPolicy.UseTreasureBoxes);
            c2.Name = "PhBot_Training_UseTreasureBoxes";
            c2.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseTreasureBoxes = c2.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c3 = AddPhBotCheck(p, isTR ? "Paskalya Yumurtası Etkinlik NPClerini kullan" : "Use Easter Egg event NPCs", 18, y, TrainingOptionsPolicy.UseEasterEggEventNpcs);
            c3.Name = "PhBot_Training_UseEasterEggEventNpcs";
            c3.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseEasterEggEventNpcs = c3.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c4 = AddPhBotCheck(p, isTR ? "Daha yüksek eşyaları otomatik kuşan" : "Equip better items", 18, y, TrainingOptionsPolicy.AutoEquipBetterItems);
            c4.Name = "PhBot_Training_AutoEquipBetterItems";
            c4.CheckedChanged += (s, e) => { TrainingOptionsPolicy.AutoEquipBetterItems = c4.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var c5 = AddPhBotCheck(p, isTR ? "Kasılma alanında çiçek kullan" : "Summon flowers in the training area", 18, y, TrainingOptionsPolicy.SummonFlowersInTrainingArea);
            c5.Name = "PhBot_Training_SummonFlowersInTrainingArea";
            c5.CheckedChanged += (s, e) => { TrainingOptionsPolicy.SummonFlowersInTrainingArea = c5.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cHammer = AddPhBotCheck(p, isTR ? "Repair Hammer kullan" : "Use a repair hammer", 18, y, TrainingOptionsPolicy.UseRepairHammer);
            cHammer.Name = "PhBot_Training_UseRepairHammer";
            cHammer.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseRepairHammer = cHammer.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cZerkPot = AddPhBotCheck(p, isTR ? "Berserker regeneration (zerk potu) kullan" : "Use berserker regeneration potions", 18, y, TrainingOptionsPolicy.UseZerkPotion);
            cZerkPot.Name = "PhBot_Training_UseZerkPotion";
            cZerkPot.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseZerkPotion = cZerkPot.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cEol = AddPhBotCheck(p, isTR ? "Energy of Life potu kullan" : "Use Energy of Life potions", 18, y, TrainingOptionsPolicy.UseEnergyOfLife);
            cEol.Name = "PhBot_Training_UseEnergyOfLife";
            cEol.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseEnergyOfLife = cEol.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cEolZerk = AddPhBotCheck(p, isTR ? "Energy of Life zerki doldur" : "Use Energy of Life berserk regeneration", 18, y, TrainingOptionsPolicy.UseEnergyOfLifeForZerk);
            cEolZerk.Name = "PhBot_Training_UseEnergyOfLifeForZerk";
            cEolZerk.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseEnergyOfLifeForZerk = cEolZerk.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cRevDeath = AddPhBotCheck(p, isTR ? "Öldüğün zaman reverse kullan" : "Use a reverse return scroll when you die", 18, y, TrainingOptionsPolicy.UseReverseOnDeath);
            cRevDeath.Name = "PhBot_Training_UseReverseOnDeath";
            cRevDeath.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseReverseOnDeath = cRevDeath.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cRevTown = AddPhBotCheck(p, isTR ? "Şehre döndükten sonra reverse kullan" : "Use a reverse return scroll after returning to town", 18, y, TrainingOptionsPolicy.UseReverseAfterTown);
            cRevTown.Name = "PhBot_Training_UseReverseAfterTown";
            cRevTown.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UseReverseAfterTown = cRevTown.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cPandoraSummon = AddPhBotCheck(p, isTR ? "Kasılma alanında Pandora box veya monster scroll kullan" : "Use monster summon scrolls & Pandora's Box in the training area", 18, y, TrainingOptionsPolicy.UsePandoraBoxOrMonsterScroll);
            cPandoraSummon.Name = "PhBot_Training_UsePandoraBoxOrMonsterScroll";
            cPandoraSummon.CheckedChanged += (s, e) => { TrainingOptionsPolicy.UsePandoraBoxOrMonsterScroll = cPandoraSummon.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cWaitStrong = AddPhBotCheck(p, isTR ? "Tekrar kullanmadan önce tüm Strong mobların ölmesini bekle" : "Wait for all strong monsters to be killed before summoning", 18, y, TrainingOptionsPolicy.WaitForStrongMobsBeforeSummon);
            cWaitStrong.Name = "PhBot_Training_WaitForStrongMobsBeforeSummon";
            cWaitStrong.CheckedChanged += (s, e) => { TrainingOptionsPolicy.WaitForStrongMobsBeforeSummon = cWaitStrong.Checked; Settings.SaveCharacterSettings(); };
            y += step;

            var cSpeed = AddPhBotCheck(p, isTR ? "Hızlı Koşma Kullan (Speed Drug)" : "Use speed drugs", 18, y, TrainingOptionsPolicy.UseSpeedDrugs);
            cSpeed.Name = "PhBot_Training_UseSpeedDrugs";
            cSpeed.CheckedChanged += (s, e) => {
                TrainingOptionsPolicy.UseSpeedDrugs = cSpeed.Checked;
                Settings.SaveCharacterSettings();
            };
            y += step;

            var cSpeedScript = AddPhBotCheck(p, isTR ? "Sadece alana giderken hızlı koşma kullan" : "Only use speed drugs in the script", 18, y, TrainingOptionsPolicy.SpeedDrugsOnlyInScript);
            cSpeedScript.Name = "PhBot_Training_SpeedDrugsOnlyInScript";
            cSpeedScript.CheckedChanged += (s, e) => {
                TrainingOptionsPolicy.SpeedDrugsOnlyInScript = cSpeedScript.Checked;
                Settings.SaveCharacterSettings();
            };

            // Sağ kolon kontrolleri (media_1789163198989.png / media_1789167056612.png)
            int rx = 420;
            var cStay = AddPhBotCheck(p, isTR ? "Training alanında kal" : "Stay in the training area", rx, 56, TrainingOptionsPolicy.StayInTrainingArea);
            cStay.Name = "PhBot_Training_StayInTrainingArea";
            cStay.CheckedChanged += (s, e) => { TrainingOptionsPolicy.StayInTrainingArea = cStay.Checked; Settings.SaveCharacterSettings(); };

            var cJustice = AddPhBotCheck(p, isTR ? "Statue of Justice mobuna saldır" : "Attack Statue of Justice", rx, 112, TrainingOptionsPolicy.AttackStatueOfJustice);
            cJustice.Name = "PhBot_Training_AttackStatueOfJustice";
            cJustice.CheckedChanged += (s, e) => { TrainingOptionsPolicy.AttackStatueOfJustice = cJustice.Checked; Settings.SaveCharacterSettings(); };

            var cMount = AddPhBotCheck(p, isTR ? "At çıkarma" : "Don't spawn horse", rx, 168, TrainingOptionsPolicy.DoNotSpawnMount);
            cMount.Name = "PhBot_Training_DoNotSpawnMount";
            cMount.CheckedChanged += (s, e) => { TrainingOptionsPolicy.DoNotSpawnMount = cMount.Checked; Settings.SaveCharacterSettings(); };

            var cLowMp = AddPhBotCheck(p, isTR ? "Düşük MP'de çekil" : "Withdraw on low MP", rx, 224, TrainingOptionsPolicy.WithdrawOnLowMp);
            cLowMp.Name = "PhBot_Training_WithdrawOnLowMp";
            cLowMp.CheckedChanged += (s, e) => { TrainingOptionsPolicy.WithdrawOnLowMp = cLowMp.Checked; Settings.SaveCharacterSettings(); };

            var numLowMp = new NumericUpDown { Name = "PhBot_Training_LowMpPercent", Font = PhBotFont(), BackColor = Color.White, Location = new Point(rx + 155, 222), Size = new Size(45, 22), Value = Math.Max(0, Math.Min(100, TrainingOptionsPolicy.LowMpPercent)), Maximum = 100 };
            numLowMp.ValueChanged += (s, e) => { TrainingOptionsPolicy.LowMpPercent = (int)numLowMp.Value; Settings.SaveCharacterSettings(); };
            var lblLowMp = new Label { Text = "%", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(rx + 205, 225) };
            p.Controls.AddRange(new Control[] { numLowMp, lblLowMp });

            var cPandora = AddPhBotCheck(p, isTR ? "Pandora sayısını koru" : "Preserve Pandora's count", rx, 250, TrainingOptionsPolicy.PreservePandora);
            cPandora.Name = "PhBot_Training_PreservePandora";
            cPandora.CheckedChanged += (s, e) => { TrainingOptionsPolicy.PreservePandora = cPandora.Checked; Settings.SaveCharacterSettings(); };

            var numPandora = new NumericUpDown { Name = "PhBot_Training_PreservePandoraCount", Font = PhBotFont(), BackColor = Color.White, Location = new Point(rx + 155, 248), Size = new Size(45, 22), Value = Math.Max(0, Math.Min(100, TrainingOptionsPolicy.PreservePandoraCount)), Maximum = 100 };
            numPandora.ValueChanged += (s, e) => { TrainingOptionsPolicy.PreservePandoraCount = (int)numPandora.Value; Settings.SaveCharacterSettings(); };
            p.Controls.Add(numPandora);
        }

        public void RefreshTrainingOptionsControls()
        {
            try
            {
                Panel host = TabPageV_Control01_Training_Panel;
                if (host == null) return;
                Panel p = GetExtraHTabPanel(host, "TabPageH_Training_Option", "Options");
                if (p == null) return;

                void SetCheck(string name, bool val)
                {
                    if (p.Controls.ContainsKey(name) && p.Controls[name] is CheckBox cb && cb.Checked != val)
                        cb.Checked = val;
                }
                void SetNum(string name, int val)
                {
                    if (p.Controls.ContainsKey(name) && p.Controls[name] is NumericUpDown nud && (int)nud.Value != val)
                        nud.Value = Math.Max(nud.Minimum, Math.Min(nud.Maximum, val));
                }

                SetCheck("PhBot_Training_DontWalkAround", TrainingOptionsPolicy.DontWalkAroundTrainingArea);
                SetCheck("PhBot_Training_UseTreasureBoxes", TrainingOptionsPolicy.UseTreasureBoxes);
                SetCheck("PhBot_Training_UseEasterEggEventNpcs", TrainingOptionsPolicy.UseEasterEggEventNpcs);
                SetCheck("PhBot_Training_AutoEquipBetterItems", TrainingOptionsPolicy.AutoEquipBetterItems);
                SetCheck("PhBot_Training_SummonFlowersInTrainingArea", TrainingOptionsPolicy.SummonFlowersInTrainingArea);
                SetCheck("PhBot_Training_UseRepairHammer", TrainingOptionsPolicy.UseRepairHammer);
                SetCheck("PhBot_Training_UseZerkPotion", TrainingOptionsPolicy.UseZerkPotion);
                SetCheck("PhBot_Training_UseEnergyOfLife", TrainingOptionsPolicy.UseEnergyOfLife);
                SetCheck("PhBot_Training_UseEnergyOfLifeForZerk", TrainingOptionsPolicy.UseEnergyOfLifeForZerk);
                SetCheck("PhBot_Training_UseReverseOnDeath", TrainingOptionsPolicy.UseReverseOnDeath);
                SetCheck("PhBot_Training_UseReverseAfterTown", TrainingOptionsPolicy.UseReverseAfterTown);
                SetCheck("PhBot_Training_UsePandoraBoxOrMonsterScroll", TrainingOptionsPolicy.UsePandoraBoxOrMonsterScroll);
                SetCheck("PhBot_Training_WaitForStrongMobsBeforeSummon", TrainingOptionsPolicy.WaitForStrongMobsBeforeSummon);
                SetCheck("PhBot_Training_UseSpeedDrugs", TrainingOptionsPolicy.UseSpeedDrugs);
                SetCheck("PhBot_Training_SpeedDrugsOnlyInScript", TrainingOptionsPolicy.SpeedDrugsOnlyInScript);

                SetCheck("PhBot_Training_StayInTrainingArea", TrainingOptionsPolicy.StayInTrainingArea);
                SetCheck("PhBot_Training_AttackStatueOfJustice", TrainingOptionsPolicy.AttackStatueOfJustice);
                SetCheck("PhBot_Training_DoNotSpawnMount", TrainingOptionsPolicy.DoNotSpawnMount);
                SetCheck("PhBot_Training_WithdrawOnLowMp", TrainingOptionsPolicy.WithdrawOnLowMp);
                SetNum("PhBot_Training_LowMpPercent", TrainingOptionsPolicy.LowMpPercent);
                SetCheck("PhBot_Training_PreservePandora", TrainingOptionsPolicy.PreservePandora);
                SetNum("PhBot_Training_PreservePandoraCount", TrainingOptionsPolicy.PreservePandoraCount);
            }
            catch { }
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
        // ADIM 5: Inventory alt-sekmeleri:
        // Inventory | Avatar | Job | Job Pouch | Pet | Transport | Storage | Guild | Exchange | Job Tickets | Item | Gori Item Exchange
        // ---------------------------------------------------------------
        private void EnsurePhBotInventoryExtraTabs()
        {
            try
            {
                if (TabPageV_Control01_Inventory_Panel == null) return;
                Panel host = TabPageV_Control01_Inventory_Panel;
                if (TabPageH_Inventory_Option04 == null) return;
                const string pre = "TabPageH_Inventory_Option";

                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Job");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Job Pouch");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Transport");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Guild");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Exchange");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Job Tickets");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Item");
                EnsureExtraHTab(host, TabPageH_Inventory_Option04, pre, "Gori Item Exchange");

                LayoutHStrip(TabPageH_Inventory_Option04.Parent, new string[]
                {
                    "TabPageH_Inventory_Option01",
                    "TabPageH_Inventory_Option02",
                    pre + "Job",
                    pre + "JobPouch",
                    "TabPageH_Inventory_Option04",
                    pre + "Transport",
                    "TabPageH_Inventory_Option03",
                    pre + "Guild",
                    pre + "Exchange",
                    pre + "JobTickets",
                    pre + "Item",
                    pre + "GoriItemExchange"
                });

                BuildInventoryGridTab(GetExtraHTabPanel(host, pre, "Job"), "Job");
                BuildInventoryGridTab(GetExtraHTabPanel(host, pre, "Job Pouch"), "Job Pouch");
                BuildInventoryGridTab(GetExtraHTabPanel(host, pre, "Transport"), "Transport");
                BuildInventoryGridTab(GetExtraHTabPanel(host, pre, "Guild"), "Guild");
                BuildInventoryExchange(GetExtraHTabPanel(host, pre, "Exchange"));
                BuildInventoryJobTickets(GetExtraHTabPanel(host, pre, "Job Tickets"));
                BuildInventoryItem(GetExtraHTabPanel(host, pre, "Item"));
                BuildInventoryGori(GetExtraHTabPanel(host, pre, "Gori Item Exchange"));
            }
            catch (Exception ex) { PhBotDebug("inventory tabs: " + ex.Message); }
        }

        private void BuildInventoryGridTab(Panel p, string typeName)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            var lv = NewPhBotListView(24, 6, Math.Max(300, p.ClientSize.Width - 110), Math.Max(150, p.ClientSize.Height - 45), "Slot|50", "Icon|50", "Item|250", "Quantity|75");
            lv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            p.Controls.Add(lv);

            var btnRefresh = new Button();
            btnRefresh.Text = "R\nE\nF\nR\nE\nS\nH";
            btnRefresh.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
            btnRefresh.Size = new Size(30, Math.Max(150, p.ClientSize.Height - 45));
            btnRefresh.Location = new Point(Math.Max(330, p.ClientSize.Width - 68), 6);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Classicize(btnRefresh);
            p.Controls.Add(btnRefresh);

            var btnSort = new Button();
            btnSort.Text = "S\nO\nR\nT";
            btnSort.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
            btnSort.Size = new Size(30, Math.Max(150, p.ClientSize.Height - 45));
            btnSort.Location = new Point(Math.Max(365, p.ClientSize.Width - 34), 6);
            btnSort.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Classicize(btnSort);
            p.Controls.Add(btnSort);

            var lblCount = new Label();
            lblCount.Text = "Item count: 0/0";
            lblCount.Font = PhBotFont();
            lblCount.AutoSize = true;
            lblCount.Location = new Point(24, Math.Max(160, p.ClientSize.Height - 30));
            lblCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            p.Controls.Add(lblCount);
        }

        private void BuildInventoryExchange(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            // phbot_inventory_04.png birebir
            AddPhBotCheck(p, "Show requests", 12, 8, true);

            var lblP = new Label { Text = "Players", Font = PhBotFont(), Location = new Point(12, 32), AutoSize = true };
            p.Controls.Add(lblP);
            var lvP = NewPhBotListView(12, 50, 115, 170, "Name|110");
            p.Controls.Add(lvP);
            var btnRefP = new Button { Text = "Refresh", Font = PhBotFont(), Location = new Point(12, 226), Size = new Size(58, 24) };
            Classicize(btnRefP);
            p.Controls.Add(btnRefP);
            var btnEx = new Button { Text = "Exchange", Font = PhBotFont(), Location = new Point(72, 226), Size = new Size(68, 24) };
            Classicize(btnEx);
            p.Controls.Add(btnEx);

            var lblInv = new Label { Text = "Inventory", Font = PhBotFont(), Location = new Point(135, 32), AutoSize = true };
            p.Controls.Add(lblInv);
            var lvInv = NewPhBotListView(135, 50, 140, 170, "Item|135");
            p.Controls.Add(lvInv);
            var btnRefInv = new Button { Text = "Refresh", Font = PhBotFont(), Location = new Point(135, 226), Size = new Size(140, 24) };
            Classicize(btnRefInv);
            p.Controls.Add(btnRefInv);

            var btnTo = new Button { Text = "►", Font = PhBotFont(), Location = new Point(282, 95), Size = new Size(28, 26) };
            Classicize(btnTo);
            p.Controls.Add(btnTo);
            var btnFrom = new Button { Text = "◄", Font = PhBotFont(), Location = new Point(282, 130), Size = new Size(28, 26) };
            Classicize(btnFrom);
            p.Controls.Add(btnFrom);

            var lblMy = new Label { Text = "My Items", Font = PhBotFont(), Location = new Point(318, 32), AutoSize = true };
            p.Controls.Add(lblMy);
            var lvMy = NewPhBotListView(318, 50, 140, 140, "Item|135");
            p.Controls.Add(lvMy);
            var txtGoldMy = new TextBox { Text = "0", Font = PhBotFont(), Location = new Point(318, 196), Size = new Size(122, 22), TextAlign = HorizontalAlignment.Right };
            p.Controls.Add(txtGoldMy);
            var lblG = new Label { Text = "G", Font = PhBotFont(), Location = new Point(442, 198), AutoSize = true };
            p.Controls.Add(lblG);

            var lblOther = new Label { Text = "Other Players Items", Font = PhBotFont(), Location = new Point(468, 32), AutoSize = true };
            p.Controls.Add(lblOther);
            var lvOther = NewPhBotListView(468, 50, 140, 140, "Item|135");
            p.Controls.Add(lvOther);
            var txtGoldOther = new TextBox { Text = "0", Font = PhBotFont(), Location = new Point(468, 196), Size = new Size(140, 22), TextAlign = HorizontalAlignment.Right };
            p.Controls.Add(txtGoldOther);

            var btnConfirm = new Button { Text = "Confirm", Font = PhBotFont(), Location = new Point(318, 226), Size = new Size(68, 24) };
            Classicize(btnConfirm);
            p.Controls.Add(btnConfirm);
            var btnApprove = new Button { Text = "Approve", Font = PhBotFont(), Location = new Point(390, 226), Size = new Size(68, 24) };
            Classicize(btnApprove);
            p.Controls.Add(btnApprove);
            var btnCancel = new Button { Text = "Cancel", Font = PhBotFont(), Location = new Point(468, 226), Size = new Size(68, 24) };
            Classicize(btnCancel);
            p.Controls.Add(btnCancel);
        }

        private void BuildInventoryJobTickets(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            // phbot_inventory_01.png birebir
            var gb = new GroupBox { Text = "Drop", Font = PhBotFont(), Location = new Point(14, 14), Size = new Size(270, 185) };
            AddPhBotCheck(gb, "Lowest quality", 20, 24, false);
            AddPhBotCheck(gb, "Low quality", 20, 56, false);
            AddPhBotCheck(gb, "Medium quality", 20, 88, false);
            AddPhBotCheck(gb, "High quality", 20, 120, false);
            AddPhBotCheck(gb, "Highest quality", 20, 152, false);
            p.Controls.Add(gb);

            var btnEx = new Button { Text = "Exchange", Font = PhBotFont(), Location = new Point(14, 210), Size = new Size(80, 26) };
            Classicize(btnEx);
            p.Controls.Add(btnEx);

            var btnCancel = new Button { Text = "Cancel", Font = PhBotFont(), Location = new Point(100, 210), Size = new Size(80, 26), Enabled = false };
            Classicize(btnCancel);
            p.Controls.Add(btnCancel);
        }

        private void BuildInventoryItem(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            // phbot_inventory_02.png birebir
            var gbInf = new GroupBox { Text = "Inflation Level", Font = PhBotFont(), Location = new Point(14, 14), Size = new Size(200, 75) };
            var cbInf = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont(), Location = new Point(16, 28), Size = new Size(168, 22) };
            cbInf.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            cbInf.SelectedIndex = 0;
            gbInf.Controls.Add(cbInf);
            p.Controls.Add(gbInf);

            var btnInfStart = new Button { Text = "Start", Font = PhBotFont(), Location = new Point(14, 98), Size = new Size(80, 26) };
            Classicize(btnInfStart);
            p.Controls.Add(btnInfStart);

            var btnInfStop = new Button { Text = "Stop", Font = PhBotFont(), Location = new Point(100, 98), Size = new Size(80, 26), Enabled = false };
            Classicize(btnInfStop);
            p.Controls.Add(btnInfStop);

            var gbAwk = new GroupBox { Text = "Awakening Enhancement", Font = PhBotFont(), Location = new Point(230, 14), Size = new Size(200, 75) };
            var cbAwk = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont(), Location = new Point(16, 28), Size = new Size(168, 22) };
            cbAwk.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            cbAwk.SelectedIndex = 0;
            gbAwk.Controls.Add(cbAwk);
            p.Controls.Add(gbAwk);

            var btnAwkStart = new Button { Text = "Start", Font = PhBotFont(), Location = new Point(230, 98), Size = new Size(80, 26) };
            Classicize(btnAwkStart);
            p.Controls.Add(btnAwkStart);

            var btnAwkStop = new Button { Text = "Stop", Font = PhBotFont(), Location = new Point(316, 98), Size = new Size(80, 26), Enabled = false };
            Classicize(btnAwkStop);
            p.Controls.Add(btnAwkStop);
        }

        private void BuildInventoryGori(Panel p)
        {
            if (p == null || p.Controls.Count > 0) return;
            p.AutoScroll = false;
            p.BackColor = Color.White;

            // phbot_inventory_03.png birebir
            var lblItem = new Label { Text = "Item", Font = PhBotFont(), Location = new Point(14, 14), AutoSize = true };
            p.Controls.Add(lblItem);
            var cbItem = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont(), Location = new Point(80, 11), Size = new Size(200, 22) };
            p.Controls.Add(cbItem);
            var btnR = new Button { Text = "R", Font = PhBotFont(), Location = new Point(285, 11), Size = new Size(28, 22) };
            Classicize(btnR);
            p.Controls.Add(btnR);

            var lblEx = new Label { Text = "Exchange", Font = PhBotFont(), Location = new Point(330, 14), AutoSize = true };
            p.Controls.Add(lblEx);
            var cbEx = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont(), Location = new Point(395, 11), Size = new Size(200, 22) };
            p.Controls.Add(cbEx);

            string[] leftStats = new string[] {
                "Physical Attack/Defense Power",
                "Magical Attack/Defense Power",
                "Durability",
                "Attack Rate",
                "Critical",
                "Physical Reinforce",
                "Magical Reinforce"
            };

            int y = 46;
            foreach (var stat in leftStats)
            {
                var lblStat = new Label { Text = stat, Font = PhBotFont(), Location = new Point(14, y + 2), AutoSize = true };
                p.Controls.Add(lblStat);
                var nud = new NumericUpDown { Font = PhBotFont(), Location = new Point(215, y), Size = new Size(60, 22), Maximum = 100, Value = 0, TextAlign = HorizontalAlignment.Center };
                p.Controls.Add(nud);
                var lblPct = new Label { Text = "%", Font = PhBotFont(), Location = new Point(280, y + 2), AutoSize = true };
                p.Controls.Add(lblPct);
                y += 30;
            }

            // Right column stats
            var lblPlus = new Label { Text = "Plus", Font = PhBotFont(), Location = new Point(330, 48), AutoSize = true };
            p.Controls.Add(lblPlus);
            var nudPlus = new NumericUpDown { Font = PhBotFont(), Location = new Point(450, 46), Size = new Size(60, 22), Maximum = 50, Value = 0, TextAlign = HorizontalAlignment.Center };
            p.Controls.Add(nudPlus);

            var lblParry = new Label { Text = "Parry Ratio", Font = PhBotFont(), Location = new Point(330, 78), AutoSize = true };
            p.Controls.Add(lblParry);
            var nudParry = new NumericUpDown { Font = PhBotFont(), Location = new Point(450, 76), Size = new Size(60, 22), Maximum = 100, Value = 0, TextAlign = HorizontalAlignment.Center };
            p.Controls.Add(nudParry);
            var lblParryPct = new Label { Text = "%", Font = PhBotFont(), Location = new Point(515, 78), AutoSize = true };
            p.Controls.Add(lblParryPct);

            AddPhBotCheck(p, "Stop when gold drops below", 330, 108, false);
            var nudGold = new NumericUpDown { Font = PhBotFont(), Location = new Point(510, 106), Size = new Size(120, 22), Maximum = 100000000000M, Value = 0 };
            p.Controls.Add(nudGold);

            AddPhBotCheck(p, "Stop when any item reaches a set stat", 330, 138, false);

            var btnStart = new Button { Text = "Start", Font = PhBotFont(), Location = new Point(14, 262), Size = new Size(80, 26) };
            Classicize(btnStart);
            p.Controls.Add(btnStart);

            var btnStop = new Button { Text = "Stop", Font = PhBotFont(), Location = new Point(100, 262), Size = new Size(80, 26), Enabled = false };
            Classicize(btnStop);
            p.Controls.Add(btnStop);

            var btnClear = new Button { Text = "Clear", Font = PhBotFont(), Location = new Point(186, 262), Size = new Size(80, 26) };
            Classicize(btnClear);
            p.Controls.Add(btnClear);

            var lblHint = new Label { Text = "* Setting the value to 0 will ignore the stat", Font = PhBotFont(), Location = new Point(330, 266), AutoSize = true, ForeColor = Color.FromArgb(60, 60, 60) };
            p.Controls.Add(lblHint);
        }
    }
}
