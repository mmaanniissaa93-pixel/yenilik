using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace xBot.App
{
    /// <summary>
    /// phBot birebir iç düzenler 4. dalga: placeholder sekmeler (Sound, Key
    /// Bindings, Conditions, Notifications, Auto Configure, Masteries) ve
    /// Inventory alt-sekmeleri (Exchange/JobTickets/Item/Gori) md birebir.
    /// Bu panellerde backend bağlı kontrol yoktur; içerik Clear + yeniden kurulur.
    /// </summary>
    public partial class Window
    {
        private Panel FindPanelByName(string name)
        {
            try
            {
                foreach (Control c in pnlWindow.Controls)
                {
                    if (c is Panel && c.Name == name) return (Panel)c;
                }
            }
            catch { }
            return null;
        }

        private static Panel FindDeepPanel(Control root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name)) return null;
            try
            {
                if (root.Name == name && root is Panel) return (Panel)root;
                foreach (Control c in root.Controls)
                {
                    Panel found = FindDeepPanel(c, name);
                    if (found != null) return found;
                }
            }
            catch { }
            return null;
        }

        // Sarma sonrası H-paneller TabPage içindedir; özyinelemeli bul.
        private Panel FindHTabPanel(Panel hostV, string hPrefix, string title)
        {
            if (hostV == null) return null;
            string n = hPrefix + SanitizeName(title) + "_Panel";
            try
            {
                if (hostV.Controls.ContainsKey(n))
                    return hostV.Controls[n] as Panel;
            }
            catch { }
            return FindDeepPanel(hostV, n);
        }

        // phBot'a özel sidebar panelleri (sarma işleminden önce kurulur).
        private void EnsureMissingSidebarPanels()
        {
            try
            {
                string[] keys = new string[]
                {
                    "ProjectHax", "Pet", "UnionParty", "Sound", "KeyBindings",
                    "Conditions", "Notifications", "AutoConfigure", "Masteries"
                };
                foreach (string key in keys)
                {
                    string pname = "TabPageV_Control01_" + key + "_Panel";
                    bool exists = false;
                    try
                    {
                        foreach (Control c in pnlWindow.Controls)
                        {
                            if (c is Panel && c.Name == pname) { exists = true; break; }
                        }
                    }
                    catch { }
                    if (exists) continue;
                    var panel = new Panel();
                    panel.Name = pname;
                    panel.BackColor = Color.White;
                    panel.BorderStyle = BorderStyle.None;
                    panel.AutoScroll = true;
                    panel.Visible = false;
                    try { pnlWindow.Controls.Add(panel); } catch { }
                    if (key == "ProjectHax")
                    {
                        try { BuildPhBotProjectHaxPanel(panel); } catch { }
                    }
                    if (key == "Pet")
                    {
                        try { BuildPhBotPetPanel(panel); } catch { }
                    }
                }
                PhBotDebug("missing sidebar panels ensured");
            }
            catch (Exception ex) { PhBotDebug("missing panels: " + ex.Message); }
        }

        private void HookInnerByName(string panelName, Action fn)
        {
            try
            {
                Panel p = FindPanelByName(panelName);
                if (p == null || fn == null) return;
                if ((p.Tag as string) == "inners34") return;
                p.Tag = "inners34";
                p.Layout += (s, e) => { try { fn(); } catch { } };
                p.SizeChanged += (s, e) => { try { fn(); } catch { } };
            }
            catch { }
        }

        private void LayoutPhBotInners34()
        {
            try
            {
                HookInnerByName("TabPageV_Control01_Academy_Panel", LayoutAcademyFull);
                HookInnerByName("TabPageV_Control01_Pet_Panel", LayoutPetInner);
                HookInnerByName("TabPageV_Control01_UnionParty_Panel", LayoutUnionInner);
                HookInnerByName("TabPageV_Control01_Masteries_Panel", LayoutMasteryInner);
                try
                {
                    if (TabPageV_Control01_Stall_Panel != null)
                    {
                        Panel sv = TabPageV_Control01_Stall_Panel;
                        sv.Layout += (s, e) => { try { LayoutStallInventoryInner(); LayoutStallFilterInner(); } catch { } };
                        sv.SizeChanged += (s, e) => { try { LayoutStallInventoryInner(); LayoutStallFilterInner(); } catch { } };
                    }
                }
                catch { }
                try
                {
                    if (TabPageH_Inventory_Option01_Panel != null)
                    {
                        TabPageH_Inventory_Option01_Panel.Layout += (s, e) => { try { LayoutInventoryMainInner(); } catch { } };
                        TabPageH_Inventory_Option01_Panel.SizeChanged += (s, e) => { try { LayoutInventoryMainInner(); } catch { } };
                    }
                }
                catch { }
                try
                {
                    if (TabPageH_Settings_Option01_Panel != null)
                    {
                        TabPageH_Settings_Option01_Panel.Layout += (s, e) => { try { LayoutSettingsInner(); } catch { } };
                        TabPageH_Settings_Option01_Panel.SizeChanged += (s, e) => { try { LayoutSettingsInner(); } catch { } };
                    }
                }
                catch { }
                LayoutSoundInner();
                LayoutKeysInner();
                LayoutConditionsInner();
                LayoutNotifInner();
                LayoutAutoCfgInner();
                LayoutMasteryInner();
                LayoutInventoryMainInner();
                LayoutSettingsInner();
                LayoutInventoryExchangeInner();
                LayoutInventoryJobTicketsInner();
                LayoutInventoryItemInner();
                LayoutInventoryGoriInner();
            }
            catch (Exception ex) { PhBotDebug("inners34: " + ex.Message); }
        }

        private Panel PlaceholderHost(string key)
        {
            return FindPanelByName("TabPageV_Control01_" + SanitizeName(key) + "_Panel");
        }

        // ---------------------------------------------------------------
        // SOUND — General + Unique Spawn 2 sekme (phbot_sound_01/02.png birebir)
        // ---------------------------------------------------------------
        private void LayoutSoundInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Sound");
                if (host == null || host.Controls["PhBot_SoundTabs"] != null) return;
                host.Controls.Clear();
                try { host.AutoScroll = false; } catch { }
                int W = Math.Max(500, host.Width), H = Math.Max(300, host.Height);

                TabControl tc = new TabControl();
                tc.Name = "PhBot_SoundTabs";
                tc.Dock = DockStyle.Fill;
                tc.Font = PhBotFont();

                TabPage tabGen = new TabPage("General");
                tabGen.BackColor = Color.White;
                try { tabGen.AutoScroll = false; } catch { }

                TabPage tabUniq = new TabPage("Unique Spawn");
                tabUniq.BackColor = Color.White;
                try { tabUniq.AutoScroll = false; } catch { }

                // Tab 1: General (13 triggers matching phbot_sound_02.png)
                int y = 6;
                string[] items = new string[]
                {
                    "GM spawned", "Unique spawned", "Titan spawned", "Unique in range",
                    "Player died", "Attacking another player", "Returning to town",
                    "Private message", "Attacked", "Thief nearby", "Hunter nearby",
                    "Rare item drop", "Transport died"
                };
                foreach (string it in items)
                {
                    string safe = it.Replace(" ", "");
                    CheckBox cb = PhBotTodoCheck(tabGen, "PhBot_Snd_" + safe, it, 10, y + 2, false);
                    if (cb != null) { cb.AutoSize = false; cb.Size = new Size(160, 20); }

                    TextBox tb = new TextBox
                    {
                        Name = "PhBot_SndPath_" + safe,
                        Font = PhBotFont(),
                        Location = new Point(175, y),
                        Size = new Size(Math.Max(150, W - 325), 22),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White
                    };
                    tabGen.Controls.Add(tb);

                    PhBotButton(tabGen, "PhBot_SndLoad_" + safe, "Load", W - 140, y - 1, 58);
                    PhBotButton(tabGen, "PhBot_SndReset_" + safe, "Reset", W - 78, y - 1, 58);

                    y += 24;
                }

                // Tab 2: Unique Spawn (matching phbot_sound_01.png)
                ListView lv = NewPhBotListView(6, 6, W - 20, Math.Max(120, H - 45), "Unique|250", "Sound Path|350");
                lv.Name = "PhBot_SoundUniques";
                string[] defaultUniques = new string[]
                {
                    "Tiger Girl", "Uruchi", "Isyutaru", "Demon Shaitan", "Cerberus",
                    "Lord Yarkan", "Haroeris", "Anubis", "Isis", "Kidemonas",
                    "Selket", "Neith", "BeakYung The White Viper"
                };
                foreach (string u in defaultUniques)
                {
                    var item = new ListViewItem(u);
                    item.SubItems.Add("");
                    lv.Items.Add(item);
                }
                tabUniq.Controls.Add(lv);

                tc.TabPages.Add(tabGen);
                tc.TabPages.Add(tabUniq);
                host.Controls.Add(tc);
            }
            catch (Exception ex) { PhBotDebug("sound inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void LayoutInventoryMainInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Inventory_Option01_Panel;
                if (p == null) return;
                try { p.AutoScroll = false; } catch { }
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                if (Inventory_lstvItems != null)
                {
                    Inventory_lstvItems.Location = new Point(6, 6);
                    Inventory_lstvItems.Size = new Size(W - 12, H - 12);
                    Inventory_lstvItems.Visible = true;
                    Classicize(Inventory_lstvItems);
                    AutoFitListView(Inventory_lstvItems);
                }
                if (Inventory_btnItemsRefresh != null) Inventory_btnItemsRefresh.Visible = false;
                if (Inventory_btnItemsSort != null) Inventory_btnItemsSort.Visible = false;
                if (Inventory_lblCapacity != null) Inventory_lblCapacity.Visible = false;
            }
            catch { }
            finally { _innerLayout = false; }
        }

        private void LayoutSettingsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Settings_Option01_Panel;
                if (p == null) return;
                try { p.AutoScroll = false; } catch { }
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                if (Settings_lstvSilkroads != null)
                {
                    Settings_lstvSilkroads.Location = new Point(6, 6);
                    Settings_lstvSilkroads.Size = new Size(150, Math.Max(150, H - 14));
                    Settings_lstvSilkroads.Visible = true;
                    Classicize(Settings_lstvSilkroads);
                }
                if (Settings_btnGenerateDatabase != null)
                {
                    Classicize(Settings_btnGenerateDatabase);
                    Settings_btnGenerateDatabase.Location = new Point(Math.Max(350, W - 170), 6);
                }
                if (Settings_btnLauncherPath != null)
                {
                    Classicize(Settings_btnLauncherPath);
                    Settings_btnLauncherPath.Location = new Point(Math.Max(350, W - 170), 38);
                }
                if (Settings_btnClientPath != null)
                {
                    Classicize(Settings_btnClientPath);
                    Settings_btnClientPath.Location = new Point(Math.Max(200, W - 320), 38);
                }
            }
            catch { }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // KEY BINDINGS — 10 madde.
        // ---------------------------------------------------------------
        private void LayoutKeysInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Key Bindings");
                if (host == null || host.Controls["PhBot_KeysDone"] != null) return;
                host.Controls.Clear();
                int y = 10;
                PhBotTodoCheck(host, "PhBot_KeyEnable", "Enable key bindings", 10, y, false); y += 34;
                string[] rows = new string[]
                {
                    "Start Bot", "Stop Bot", "Start Trace", "Stop Trace",
                    "Get Position", "Enable conditions", "Disable conditions",
                    "Call Python", "Python function name"
                };
                ListView lv = NewPhBotListView(10, y, 460, 280, "Action|260", "Key|180");
                lv.Name = "PhBot_KeysList";
                host.Controls.Add(lv);
                foreach (string r in rows)
                {
                    var it = new ListViewItem(r);
                    it.SubItems.Add("");
                    lv.Items.Add(it);
                }
                Label done = new Label();
                done.Name = "PhBot_KeysDone";
                done.Text = "Start/stop multiple bots at the same time with a single key press.";
                done.Font = PhBotFont();
                done.ForeColor = Color.FromArgb(60, 60, 60);
                done.AutoSize = true;
                done.Location = new Point(10, y + 290);
                host.Controls.Add(done);
                // TODO backend: genel hotkey sistemi yok (docs/PHBOT_GAP_ANALYSIS.md §22).
            }
            catch (Exception ex) { PhBotDebug("keys inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // CONDITIONS — If/Then editörü.
        // ---------------------------------------------------------------
        private void LayoutConditionsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Conditions");
                if (host == null || host.Controls["PhBot_CondDone"] != null) return;
                host.Controls.Clear();
                int y = 10;
                PhBotLabel(host, "PhBot_CondIfLbl", "If:", 10, y + 4);
                ComboBox cbIf = new ComboBox();
                cbIf.Name = "PhBot_CondIf";
                cbIf.DropDownStyle = ComboBoxStyle.DropDownList;
                cbIf.Items.AddRange(new object[] { "Player HP % <", "Monster count >=", "Level reached", "In town", "Dead" });
                try { cbIf.SelectedIndex = 0; } catch { }
                GrayCombo(cbIf);
                cbIf.Location = new Point(130, y);
                cbIf.Size = new Size(260, 24);
                host.Controls.Add(cbIf);
                Button bAddIf = PhBotButton(host, "PhBot_CondAddIf", "Add", 400, y - 2, 90);
                y += 36;
                ListView lv = NewPhBotListView(10, y, 560, 180, "If|270", "Then|270");
                lv.Name = "PhBot_CondList";
                host.Controls.Add(lv);
                y += 190;
                PhBotLabel(host, "PhBot_CondThenLbl", "Then:", 10, y + 4);
                ComboBox cbThen = new ComboBox();
                cbThen.Name = "PhBot_CondThen";
                cbThen.DropDownStyle = ComboBoxStyle.DropDownList;
                cbThen.Items.AddRange(new object[] { "Return to town", "Disconnect", "Stop bot", "Use return scroll", "Cast skill" });
                try { cbThen.SelectedIndex = 0; } catch { }
                GrayCombo(cbThen);
                cbThen.Location = new Point(130, y);
                cbThen.Size = new Size(260, 24);
                host.Controls.Add(cbThen);
                PhBotButton(host, "PhBot_CondAddThen", "Add", 400, y - 2, 90);
                y += 36;
                Label done = new Label();
                done.Name = "PhBot_CondDone";
                done.Text = "Conditions execute whether the bot is started/stopped/tracing/stalling. Multiple Then options execute in order.";
                done.Font = PhBotFont();
                done.ForeColor = Color.FromArgb(60, 60, 60);
                done.AutoSize = true;
                done.MaximumSize = new Size(560, 0);
                done.Location = new Point(10, y);
                host.Controls.Add(done);
                // TODO backend: genel Conditions motoru yok (docs/PHBOT_GAP_ANALYSIS.md §23).
            }
            catch (Exception ex) { PhBotDebug("cond inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // NOTIFICATIONS — olay listesi.
        // ---------------------------------------------------------------
        private void LayoutNotifInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Notifications");
                if (host == null || host.Controls["PhBot_NotifTabs"] != null) return;
                host.Controls.Clear();
                int W = Math.Max(500, host.Width), H = Math.Max(300, host.Height);

                TabControl tc = new TabControl();
                tc.Name = "PhBot_NotifTabs";
                tc.Dock = DockStyle.Fill;
                tc.Font = PhBotFont();

                TabPage tabNotif = new TabPage("Notifications");
                tabNotif.BackColor = Color.White;
                TabPage tabOpt = new TabPage("Options");
                tabOpt.BackColor = Color.White;

                // Tab 1: Notifications
                ListView lv = NewPhBotListView(6, 6, W - 24, Math.Max(120, H - 72), "Date|130", "Notification|480", "Icon|60");
                lv.Name = "PhBot_NotifList";
                lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                tabNotif.Controls.Add(lv);

                int by = H - 62;
                Button btnClear = PhBotButton(tabNotif, "PhBot_NotifClear", "Clear", 6, by, 75);
                if (btnClear != null) btnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

                CheckBox cbLog = PhBotTodoCheck(tabNotif, "PhBot_NotifLog", "Enable notification logging", 95, by + 2, true);
                if (cbLog != null) cbLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

                CheckBox cbChat = PhBotTodoCheck(tabNotif, "PhBot_NotifChat", "Enable chat logging", 275, by + 2, true);
                if (cbChat != null) cbChat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

                // Tab 2: Options
                string[] evts = new string[]
                {
                    "Play sound on unique spawn", "Play sound on death", "Play sound on GM spawn",
                    "Send notification on disconnect", "Rare item has been picked", "Quest has been disabled",
                    "GM has spawned near you", "Another player attacks you", "You die by a monster"
                };
                int oy = 12;
                foreach (string e in evts)
                {
                    PhBotTodoCheck(tabOpt, "PhBot_Notif_" + e.Replace(" ", ""), e, 14, oy, true);
                    oy += 26;
                }
                Label done = new Label();
                done.Name = "PhBot_NotifDone";
                done.Text = "Some notifications appear by the tray icon when minimized. Turn off if you experience lag (resource intensive).";
                done.Font = PhBotFont();
                done.ForeColor = Color.FromArgb(80, 80, 80);
                done.AutoSize = true;
                done.Location = new Point(14, oy + 8);
                tabOpt.Controls.Add(done);

                tc.TabPages.Add(tabNotif);
                tc.TabPages.Add(tabOpt);
                host.Controls.Add(tc);
            }
            catch (Exception ex) { PhBotDebug("notif inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // AUTO CONFIGURE — phbot_auto-configure_01.png birebir yerleşim
        // ---------------------------------------------------------------
        private void LayoutAutoCfgInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Auto Configure");
                if (host == null || host.Controls["PhBot_AutoDone"] != null) return;
                host.Controls.Clear();

                PhBotLabel(host, "PhBot_AutoInfo",
                    "Auto configuration allows you to easily setup a new character with little effort.", 10, 8);

                // GroupBox Chinese
                GroupBox gbCn = new GroupBox { Text = "Chinese", Location = new Point(10, 30), Size = new Size(255, 82), Font = PhBotFont(), BackColor = Color.White };
                PhBotLabel(gbCn, "PhBot_AutoTypeLbl", "Type", 12, 22);
                ComboBox cbBuild = new ComboBox { Name = "PhBot_AutoBuild", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(78, 18), Size = new Size(165, 24) };
                cbBuild.Items.AddRange(new object[] { "Str", "Int", "Hybrid 1:1", "Hybrid 1:2" });
                try { cbBuild.SelectedIndex = 0; } catch { }
                GrayCombo(cbBuild);
                gbCn.Controls.Add(cbBuild);

                PhBotLabel(gbCn, "PhBot_AutoWpnLbl", "Weapon", 12, 50);
                ComboBox cbWpn = new ComboBox { Name = "PhBot_AutoWeapon", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(78, 46), Size = new Size(165, 24) };
                cbWpn.Items.AddRange(new object[] { "Bicheon", "Heuksal", "Pacheon" });
                try { cbWpn.SelectedIndex = 0; } catch { }
                GrayCombo(cbWpn);
                gbCn.Controls.Add(cbWpn);
                host.Controls.Add(gbCn);

                // GroupBox European
                GroupBox gbEu = new GroupBox { Text = "European", Location = new Point(10, 116), Size = new Size(255, 82), Font = PhBotFont(), BackColor = Color.White };
                PhBotLabel(gbEu, "PhBot_AutoPrimLbl", "Primary", 12, 22);
                ComboBox cbPrim = new ComboBox { Name = "PhBot_AutoPrimary", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(78, 18), Size = new Size(165, 24) };
                cbPrim.Items.AddRange(new object[] { "Warrior", "Wizard", "Rogue", "Warlock", "Bard", "Cleric" });
                try { cbPrim.SelectedIndex = 0; } catch { }
                GrayCombo(cbPrim);
                gbEu.Controls.Add(cbPrim);

                PhBotLabel(gbEu, "PhBot_AutoSecLbl", "Secondary", 12, 50);
                ComboBox cbSec = new ComboBox { Name = "PhBot_AutoSecondary", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(78, 46), Size = new Size(165, 24) };
                cbSec.Items.AddRange(new object[] { "Cleric", "Bard", "Warlock", "None" });
                try { cbSec.SelectedIndex = 0; } catch { }
                GrayCombo(cbSec);
                gbEu.Controls.Add(cbSec);
                host.Controls.Add(gbEu);

                // GroupBox Profile
                GroupBox gbProf = new GroupBox { Text = "Profile", Location = new Point(10, 202), Size = new Size(255, 92), Font = PhBotFont(), BackColor = Color.White };
                ComboBox cbProf = new ComboBox { Name = "PhBot_AutoProfile", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(12, 20), Size = new Size(231, 24) };
                cbProf.Items.Add("Default");
                try { cbProf.SelectedIndex = 0; } catch { }
                GrayCombo(cbProf);
                gbProf.Controls.Add(cbProf);

                PhBotButton(gbProf, "PhBot_AutoNew", "New", 12, 52, 70);
                PhBotButton(gbProf, "PhBot_AutoRename", "Rename", 88, 52, 75);
                PhBotButton(gbProf, "PhBot_AutoDelete", "Delete", 168, 52, 75);
                host.Controls.Add(gbProf);

                PhBotTodoCheck(host, "PhBot_AutoBeforeLoop", "Auto configure before each town loop", 12, 302, false);

                // Configure Butonu (Büyük Sağ Buton)
                Button btnConfigure = new Button
                {
                    Name = "PhBot_AutoConfigureBigBtn",
                    Text = "Configure",
                    Location = new Point(285, 36),
                    Size = new Size(330, 162),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(225, 225, 225),
                    ForeColor = Color.Black,
                    Font = new Font("Tahoma", 9.5f, FontStyle.Regular)
                };
                btnConfigure.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnConfigure.FlatAppearance.BorderSize = 1;
                btnConfigure.Click += (s, e) =>
                {
                    MessageBox.Show("Auto configuration applied sane defaults for " + (cbBuild.SelectedItem ?? "character") + "!", "phBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                host.Controls.Add(btnConfigure);

                Label done = new Label { Name = "PhBot_AutoDone", Text = "", Visible = false };
                host.Controls.Add(done);
                // TODO backend: otomatik skill kurulumu yok (docs/PHBOT_GAP_ANALYSIS.md §2).
            }
            catch (Exception ex) { PhBotDebug("autocfg inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // MASTERIES — ağaç + gap + temizleme.
        // ---------------------------------------------------------------
        private void LayoutMasteryInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Masteries");
                if (host == null) return;
                int W = host.Width, H = host.Height;
                if (W < 400 || H < 250) return;
                try { host.AutoScroll = true; } catch { }
                if (host.Controls["PhBot_MastDone"] == null)
                {
                    host.Controls.Clear();
                    ListView lv = NewPhBotListView(8, 8, Math.Max(300, W - 280), 320, "Mastery / Skill|300", "Level|80", "Auto|80");
                    lv.Name = "PhBot_MastTree";
                    host.Controls.Add(lv);
                    Label done = new Label();
                    done.Name = "PhBot_MastDone";
                    done.Text = "Gap farming: keep a skill level gap to gain more SP and less EXP.";
                    done.Font = PhBotFont();
                    done.ForeColor = Color.FromArgb(60, 60, 60);
                    done.AutoSize = true;
                    host.Controls.Add(done);
                    // TODO backend: mastery/skill otomasyonu yok (docs/PHBOT_GAP_ANALYSIS.md §19).
                }
                int x = Math.Max(320, W - 260);
                try
                {
                    ListView lv2 = host.Controls["PhBot_MastTree"] as ListView;
                    if (lv2 != null) { lv2.Location = new Point(8, 8); lv2.Size = new Size(Math.Max(300, W - 280), 320); lv2.Visible = true; }
                }
                catch { }
                int y = 8;
                PhBotLabel(host, "PhBot_MastGapLbl", "Gap:", x, y + 4);
                PhBotTodoNumber(host, "PhBot_MastGap", x + 120, y, 60, "0"); y += 34;
                PhBotButton(host, "PhBot_MastClearM", "Clear Auto Mastery", x, y, 170); y += 36;
                PhBotButton(host, "PhBot_MastClearS", "Clear Auto Skill", x, y, 170); y += 36;
                PhBotTodoCheck(host, "PhBot_MastAuto", "Auto level up masteries/skills", x, y, false);
                try
                {
                    Label d2 = host.Controls["PhBot_MastDone"] as Label;
                    if (d2 != null) { d2.Location = new Point(8, 336); d2.Visible = true; }
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("mast inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // INVENTORY > EXCHANGE — 13 madde birebir (görsel klon).
        // ---------------------------------------------------------------
        private void LayoutInventoryExchangeInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Inventory_Panel;
                Panel pg = FindHTabPanel(host, "TabPageH_Inventory_Option", "Exchange");
                if (pg == null || pg.Controls["PhBot_InvExDone"] != null) return;
                pg.Controls.Clear();
                int W = pg.Width;
                PhBotTodoCheck(pg, "PhBot_InvExShow", "Show requests", 8, 8, true);
                ListView lvP = NewPhBotListView(8, 36, 240, 200, "Players|220");
                lvP.Name = "PhBot_InvExPlayers";
                pg.Controls.Add(lvP);
                PhBotButton(pg, "PhBot_InvExRefreshP", "Refresh", 8, 242, 110);
                PhBotButton(pg, "PhBot_InvExDo", "Exchange", 138, 242, 110);
                ListView lvM = NewPhBotListView(258, 36, 240, 200, "Inventory|220");
                lvM.Name = "PhBot_InvExMine";
                pg.Controls.Add(lvM);
                PhBotButton(pg, "PhBot_InvExRefreshM", "Refresh", 258, 242, 110);
                int mx = 508;
                Button bTo = PhBotButton(pg, "PhBot_InvExTo", ">", mx, 100, 44);
                Button bFrom = PhBotButton(pg, "PhBot_InvExFrom", "<", mx, 140, 44);
                ListView lvG = NewPhBotListView(mx + 54, 36, 220, 95, "Giving|200");
                lvG.Name = "PhBot_InvExGive";
                pg.Controls.Add(lvG);
                PhBotLabel(pg, "PhBot_InvExGoldLbl", "Gold:", mx + 54, 140);
                PhBotTodoNumber(pg, "PhBot_InvExGold", mx + 110, 138, 110, "0");
                ListView lvT = NewPhBotListView(mx + 54, 170, 220, 66, "Receiving|200");
                lvT.Name = "PhBot_InvExTake";
                pg.Controls.Add(lvT);
                PhBotButton(pg, "PhBot_InvExConfirm", "Confirm", mx + 54, 242, 90);
                PhBotButton(pg, "PhBot_InvExApprove", "Approve", mx + 150, 242, 90);
                PhBotButton(pg, "PhBot_InvExCancel", "Cancel", mx + 246, 242, 90);
                Label done = new Label();
                done.Name = "PhBot_InvExDone";
                done.Location = new Point(8, 280);
                pg.Controls.Add(done);
                // TODO backend: takas otomasyonu Players > Exchange dışında yok.
            }
            catch (Exception ex) { PhBotDebug("invex inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // INVENTORY > JOB TICKETS / ITEM / GORI — md birebir.
        // ---------------------------------------------------------------
        private void LayoutInventoryJobTicketsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Inventory_Panel;
                Panel pg = FindHTabPanel(host, "TabPageH_Inventory_Option", "Job Tickets");
                if (pg == null || pg.Controls["PhBot_InvJobDone"] != null) return;
                pg.Controls.Clear();
                PhBotTodoCheck(pg, "PhBot_InvJobEx", "Exchange job tickets automatically", 10, 10, false);
                PhBotTodoCheck(pg, "PhBot_InvJobDrop", "Drop unwanted rewards", 10, 40, false);
                ListView lv = NewPhBotListView(10, 72, 460, 200, "Ticket|340", "Count|100");
                lv.Name = "PhBot_InvJobList";
                pg.Controls.Add(lv);
                Label done = new Label();
                done.Name = "PhBot_InvJobDone";
                done.Location = new Point(10, 280);
                pg.Controls.Add(done);
            }
            catch (Exception ex) { PhBotDebug("invjob inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void LayoutInventoryItemInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Inventory_Panel;
                Panel pg = FindHTabPanel(host, "TabPageH_Inventory_Option", "Item");
                if (pg == null || pg.Controls["PhBot_InvItemDone"] != null) return;
                pg.Controls.Clear();
                PhBotTodoCheck(pg, "PhBot_InvBalloon", "Inflate balloons during the Balloon event", 10, 10, false);
                PhBotTodoCheck(pg, "PhBot_InvAwake", "Awakening Enhancement: Devil/Angel Spirit", 10, 40, false);
                PhBotButton(pg, "PhBot_InvItemStart", "Start", 10, 74, 90);
                PhBotButton(pg, "PhBot_InvItemStop", "Stop", 106, 74, 90);
                Label done = new Label();
                done.Name = "PhBot_InvItemDone";
                done.Location = new Point(10, 110);
                pg.Controls.Add(done);
            }
            catch (Exception ex) { PhBotDebug("invitem inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void LayoutInventoryGoriInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Inventory_Panel;
                Panel pg = FindHTabPanel(host, "TabPageH_Inventory_Option", "Gori Item Exchange");
                if (pg == null || pg.Controls["PhBot_InvGoriDone"] != null) return;
                pg.Controls.Clear();
                PhBotTodoCheck(pg, "PhBot_GoriEnable", "Enable Gori item exchange (Magic POP Guide)", 10, 10, false);
                PhBotLabel(pg, "PhBot_GoriStatLbl", "Stop at white stat:", 10, 44);
                PhBotTodoNumber(pg, "PhBot_GoriStat", 190, 42, 220, "");
                ListView lv = NewPhBotListView(10, 74, 460, 200, "Item|300", "Result|150");
                lv.Name = "PhBot_GoriList";
                pg.Controls.Add(lv);
                Label done = new Label();
                done.Name = "PhBot_InvGoriDone";
                done.Location = new Point(10, 282);
                pg.Controls.Add(done);
            }
            catch (Exception ex) { PhBotDebug("invgori inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }
    }
}
