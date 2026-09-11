using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game;

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
                HookInnerByName("TabPageV_Control01_AutoConfigure_Panel", LayoutAutoCfgInner);
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

                TabControl tc = new TabControl();
                tc.Name = "PhBot_SoundTabs";
                tc.Dock = DockStyle.Fill;
                tc.Font = PhBotFont();

                TabPage tabGen = new TabPage("General");
                tabGen.BackColor = Color.White;
                try { tabGen.AutoScroll = true; } catch { }

                TabPage tabUniq = new TabPage("Unique Spawn");
                tabUniq.BackColor = Color.White;
                try { tabUniq.AutoScroll = false; } catch { }

                // Tab 1: General (13 triggers matching phbot_sound_02.png)
                int y = 8;
                string[] items = new string[]
                {
                    "GM spawned", "Unique spawned", "Titan spawned", "Unique in range",
                    "Player died", "Attacking another player", "Returning to town",
                    "Private message", "Attacked", "Thief nearby", "Hunter nearby",
                    "Rare item drop", "Transport died"
                };
                var textboxes = new List<TextBox>();
                var loadBtns = new List<Button>();
                var resetBtns = new List<Button>();
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
                        Size = new Size(350, 22),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White
                    };
                    tabGen.Controls.Add(tb);
                    textboxes.Add(tb);

                    Button bLoad = PhBotButton(tabGen, "PhBot_SndLoad_" + safe, "Load", 535, y - 1, 58);
                    Button bReset = PhBotButton(tabGen, "PhBot_SndReset_" + safe, "Reset", 598, y - 1, 58);
                    if (bLoad != null) loadBtns.Add(bLoad);
                    if (bReset != null) resetBtns.Add(bReset);

                    y += 24;
                }

                Action updateGenLayout = () =>
                {
                    int cw = tabGen.ClientSize.Width;
                    if (cw < 350) return;
                    int btnResetX = cw - 70;
                    int btnLoadX = btnResetX - 62;
                    int tbW = Math.Max(120, btnLoadX - 185);
                    for (int i = 0; i < textboxes.Count; i++)
                    {
                        textboxes[i].Width = tbW;
                        if (i < loadBtns.Count) loadBtns[i].Left = btnLoadX;
                        if (i < resetBtns.Count) resetBtns[i].Left = btnResetX;
                    }
                };
                tabGen.Resize += (s, e) => updateGenLayout();
                updateGenLayout();

                // Tab 2: Unique Spawn (matching phbot_sound_01.png)
                ListView lv = new ListView
                {
                    Name = "PhBot_SoundUniques",
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    Font = PhBotFont(),
                    Dock = DockStyle.Fill
                };
                lv.Columns.Add("Unique", 250);
                lv.Columns.Add("Sound Path", 450);
                Action updateLvCols = () =>
                {
                    if (lv.ClientSize.Width > 300)
                    {
                        lv.Columns[0].Width = 220;
                        lv.Columns[1].Width = Math.Max(150, lv.ClientSize.Width - 225);
                    }
                };
                lv.Resize += (s, e) => updateLvCols();

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
                updateLvCols();

                tc.TabPages.Add(tabGen);
                tc.TabPages.Add(tabUniq);
                host.Controls.Add(tc);
            }
            catch (Exception ex) { PhBotDebug("snd inner: " + ex.Message); }
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

                int rightBtnsW = 75;
                int bottomH = 32;

                Label lblPlayer = p.Controls["PhBot_InvPlayerLbl"] as Label;
                if (lblPlayer == null)
                {
                    lblPlayer = new Label
                    {
                        Name = "PhBot_InvPlayerLbl",
                        Text = "P\nL\nA\nY\nE\nR",
                        Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                        ForeColor = Color.FromArgb(80, 80, 80),
                        TextAlign = ContentAlignment.MiddleCenter,
                        AutoSize = false,
                        Location = new Point(4, 120),
                        Size = new Size(18, 120)
                    };
                    p.Controls.Add(lblPlayer);
                }
                lblPlayer.Visible = true;

                if (Inventory_lstvItems != null)
                {
                    Inventory_lstvItems.Location = new Point(24, 6);
                    Inventory_lstvItems.Size = new Size(W - rightBtnsW - 32, H - bottomH - 12);
                    Inventory_lstvItems.Visible = true;
                    Classicize(Inventory_lstvItems);
                    if (Inventory_lstvItems.Columns.Count != 4 || Inventory_lstvItems.Columns[1].Text != "Icon")
                    {
                        Inventory_lstvItems.Columns.Clear();
                        Inventory_lstvItems.Columns.Add("Slot", 50);
                        Inventory_lstvItems.Columns.Add("Icon", 50);
                        int itemColW = Math.Max(200, (W - rightBtnsW - 32) - 50 - 50 - 75 - 20);
                        Inventory_lstvItems.Columns.Add("Item", itemColW);
                        Inventory_lstvItems.Columns.Add("Quantity", 75);
                    }
                }

                // Sağ dikey butonlar: REFRESH ve SORT
                if (Inventory_btnItemsRefresh != null)
                {
                    Inventory_btnItemsRefresh.Text = "R\nE\nF\nR\nE\nS\nH";
                    Inventory_btnItemsRefresh.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
                    Inventory_btnItemsRefresh.Location = new Point(W - 68, 6);
                    Inventory_btnItemsRefresh.Size = new Size(30, H - bottomH - 12);
                    Inventory_btnItemsRefresh.Visible = true;
                    Classicize(Inventory_btnItemsRefresh);
                }
                if (Inventory_btnItemsSort != null)
                {
                    Inventory_btnItemsSort.Text = "S\nO\nR\nT";
                    Inventory_btnItemsSort.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
                    Inventory_btnItemsSort.Location = new Point(W - 34, 6);
                    Inventory_btnItemsSort.Size = new Size(30, H - bottomH - 12);
                    Inventory_btnItemsSort.Visible = true;
                    Classicize(Inventory_btnItemsSort);
                }

                // Alt çubuk: Item count ve Gold
                if (Inventory_lblCapacity != null)
                {
                    Inventory_lblCapacity.Text = "Item count: " + (Inventory_lstvItems != null ? Inventory_lstvItems.Items.Count : 0) + "/109";
                    Inventory_lblCapacity.Font = PhBotFont();
                    Inventory_lblCapacity.ForeColor = Color.Black;
                    Inventory_lblCapacity.Location = new Point(8, H - 24);
                    Inventory_lblCapacity.AutoSize = true;
                    Inventory_lblCapacity.Visible = true;
                }

                Label lblGold = p.Controls["PhBot_InvGoldLbl"] as Label;
                if (lblGold == null)
                {
                    lblGold = new Label
                    {
                        Name = "PhBot_InvGoldLbl",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        TextAlign = ContentAlignment.MiddleRight,
                        AutoSize = false
                    };
                    p.Controls.Add(lblGold);
                }
                lblGold.Text = (InfoManager.Character != null ? InfoManager.Character.Gold.ToString("N0") : "0");
                lblGold.Location = new Point(W - 220, H - 24);
                lblGold.Size = new Size(210, 20);
                lblGold.Visible = true;
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
        // KEY BINDINGS — phbot_key-bindings_01.png birebir yerleşim
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
                host.BackColor = Color.White;
                try { host.AutoScroll = true; } catch { }

                CheckBox cbEnable = PhBotTodoCheck(host, "PhBot_KeyEnable", "Enable key bindings", 12, 12, false);

                string[] standardActions = new string[]
                {
                    "Start bot", "Stop bot", "Start trace", "Stop trace",
                    "Get position", "Enable conditions", "Disable conditions"
                };

                int y = 44;
                for (int i = 0; i < standardActions.Length; i++)
                {
                    string action = standardActions[i];
                    string safe = action.Replace(" ", "");
                    Label lbl = new Label
                    {
                        Name = "PhBot_KeyLbl_" + safe,
                        Text = action,
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        Location = new Point(12, y + 4),
                        AutoSize = true
                    };
                    host.Controls.Add(lbl);

                    TextBox tb = new TextBox
                    {
                        Name = "PhBot_KeyBox_" + safe,
                        Text = "Press shortcut",
                        Font = PhBotFont(),
                        ForeColor = Color.FromArgb(70, 70, 70),
                        Location = new Point(135, y),
                        Size = new Size(95, 23),
                        ReadOnly = true,
                        TextAlign = HorizontalAlignment.Center,
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    host.Controls.Add(tb);

                    PhBotButton(host, "PhBot_KeyReset_" + safe, "Reset", 238, y - 1, 55);

                    if (i == 1 || i == 3 || i == 4 || i == 6)
                        y += 34;
                    else
                        y += 28;
                }

                // 3 rows of Call Python
                for (int p = 1; p <= 3; p++)
                {
                    Label lbl = new Label
                    {
                        Name = "PhBot_KeyLbl_Py" + p,
                        Text = "Call Python",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        Location = new Point(12, y + 4),
                        AutoSize = true
                    };
                    host.Controls.Add(lbl);

                    TextBox tb = new TextBox
                    {
                        Name = "PhBot_KeyBox_Py" + p,
                        Text = "Press shortcut",
                        Font = PhBotFont(),
                        ForeColor = Color.FromArgb(70, 70, 70),
                        Location = new Point(135, y),
                        Size = new Size(95, 23),
                        ReadOnly = true,
                        TextAlign = HorizontalAlignment.Center,
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    host.Controls.Add(tb);

                    PhBotButton(host, "PhBot_KeyReset_Py" + p, "Reset", 238, y - 1, 55);

                    TextBox tbScript = new TextBox
                    {
                        Name = "PhBot_KeyPyScript_" + p,
                        Font = PhBotFont(),
                        Location = new Point(302, y),
                        Size = new Size(165, 23),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    host.Controls.Add(tbScript);

                    y += 28;
                }

                Label done = new Label { Name = "PhBot_KeysDone", Text = "", Visible = false };
                host.Controls.Add(done);
            }
            catch (Exception ex) { PhBotDebug("keys inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // CONDITIONS — phbot_conditions_01.png birebir 2 alt-sekme (Conditions, Builder)
        // ---------------------------------------------------------------
        private void LayoutConditionsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Conditions");
                if (host == null || host.Controls["PhBot_ConditionsTabs"] != null) return;
                host.Controls.Clear();
                host.BackColor = Color.White;
                int W = Math.Max(500, host.Width), H = Math.Max(300, host.Height);

                TabControl tc = new TabControl
                {
                    Name = "PhBot_ConditionsTabs",
                    Dock = DockStyle.Fill,
                    Font = PhBotFont()
                };

                TabPage tabCond = new TabPage("Conditions") { BackColor = Color.White };
                TabPage tabBuild = new TabPage("Builder") { BackColor = Color.White };

                // Tab 1: Conditions list + reorder buttons
                ListBox lbxCond = new ListBox
                {
                    Name = "PhBot_ConditionsListBox",
                    Font = PhBotFont(),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    ItemHeight = 20
                };
                ContextMenuStrip cms = new ContextMenuStrip { Font = PhBotFont() };
                cms.Items.Add("Disable");
                cms.Items.Add("Edit");
                cms.Items.Add("Delete");
                cms.Items.Add("Clear");
                lbxCond.ContextMenuStrip = cms;
                lbxCond.Items.Add("if (In town && Time Elapsed (seconds) > 300) { Chat party ( hello! ); }");
                tabCond.Controls.Add(lbxCond);

                Button btnUp = new Button
                {
                    Name = "PhBot_CondBtnUp",
                    Text = "▲",
                    Size = new Size(26, 24),
                    FlatStyle = FlatStyle.Standard,
                    UseVisualStyleBackColor = true,
                    Font = new Font("Segoe UI Symbol", 8.5f)
                };
                Button btnDown = new Button
                {
                    Name = "PhBot_CondBtnDown",
                    Text = "▼",
                    Size = new Size(26, 24),
                    FlatStyle = FlatStyle.Standard,
                    UseVisualStyleBackColor = true,
                    Font = new Font("Segoe UI Symbol", 8.5f)
                };
                tabCond.Controls.Add(btnUp);
                tabCond.Controls.Add(btnDown);

                Action updateCondLayout = () =>
                {
                    int cw = tabCond.ClientSize.Width, ch = tabCond.ClientSize.Height;
                    if (cw < 100 || ch < 50) return;
                    btnUp.Location = new Point(cw - 34, ch / 2 - 28);
                    btnDown.Location = new Point(cw - 34, ch / 2 + 2);
                    lbxCond.Location = new Point(8, 8);
                    lbxCond.Size = new Size(Math.Max(50, cw - 46), Math.Max(40, ch - 16));
                };
                tabCond.Resize += (s, e) => updateCondLayout();
                tc.SelectedIndexChanged += (s, e) => updateCondLayout();
                updateCondLayout();

                // Tab 2: Builder
                int by = 12;
                PhBotLabel(tabBuild, "PhBot_BldIfLbl", "If:", 12, by + 4);
                ComboBox cbBldIf = new ComboBox
                {
                    Name = "PhBot_BldIf",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(60, by),
                    Size = new Size(240, 24)
                };
                cbBldIf.Items.AddRange(new object[] { "In town", "Dead", "HP < %", "MP < %", "Monster count >=", "Time elapsed (seconds) >" });
                try { cbBldIf.SelectedIndex = 0; } catch { }
                GrayCombo(cbBldIf);
                tabBuild.Controls.Add(cbBldIf);
                TextBox tbBldIfVal = new TextBox { Name = "PhBot_BldIfVal", Location = new Point(308, by), Size = new Size(70, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                tabBuild.Controls.Add(tbBldIfVal);
                by += 38;

                PhBotLabel(tabBuild, "PhBot_BldThenLbl", "Then:", 12, by + 4);
                ComboBox cbBldThen = new ComboBox
                {
                    Name = "PhBot_BldThen",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(60, by),
                    Size = new Size(240, 24)
                };
                cbBldThen.Items.AddRange(new object[] { "Chat party", "Return to town", "Stop bot", "Disconnect", "Cast skill" });
                try { cbBldThen.SelectedIndex = 0; } catch { }
                GrayCombo(cbBldThen);
                tabBuild.Controls.Add(cbBldThen);
                TextBox tbBldThenVal = new TextBox { Name = "PhBot_BldThenVal", Location = new Point(308, by), Size = new Size(180, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                tabBuild.Controls.Add(tbBldThenVal);
                by += 38;

                PhBotLabel(tabBuild, "PhBot_BldElseLbl", "Else:", 12, by + 4);
                ComboBox cbBldElse = new ComboBox
                {
                    Name = "PhBot_BldElse",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(60, by),
                    Size = new Size(240, 24)
                };
                cbBldElse.Items.AddRange(new object[] { "None", "Chat party", "Return to town", "Stop bot" });
                try { cbBldElse.SelectedIndex = 0; } catch { }
                GrayCombo(cbBldElse);
                tabBuild.Controls.Add(cbBldElse);
                TextBox tbBldElseVal = new TextBox { Name = "PhBot_BldElseVal", Location = new Point(308, by), Size = new Size(180, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                tabBuild.Controls.Add(tbBldElseVal);
                by += 42;

                PhBotButton(tabBuild, "PhBot_BldAdd", "Add Condition", 60, by, 110);
                PhBotButton(tabBuild, "PhBot_BldReset", "Reset", 178, by, 75);

                tc.TabPages.Add(tabCond);
                tc.TabPages.Add(tabBuild);
                host.Controls.Add(tc);
            }
            catch (Exception ex) { PhBotDebug("cond inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // NOTIFICATIONS — phbot_notifications_01.png birebir
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

                // Tab 1: Notifications (exact columns matching phbot_notifications_01.png)
                ListView lv = NewPhBotListView(8, 8, W - 20, Math.Max(120, H - 75), "Date|130", "Notification|480", "Icon|80");
                lv.Name = "PhBot_NotifList";
                tabNotif.Controls.Add(lv);

                int by = H - 60;
                Button btnClear = PhBotButton(tabNotif, "PhBot_NotifClear", "Clear", 8, by, 75);
                CheckBox cbLog = PhBotTodoCheck(tabNotif, "PhBot_NotifLog", "Enable notification logging", 100, by + 4, true);
                CheckBox cbChat = PhBotTodoCheck(tabNotif, "PhBot_NotifChat", "Enable chat logging", 295, by + 4, true);

                Action relayoutNotif = () => {
                    int th = tabNotif.ClientSize.Height;
                    int tw = tabNotif.ClientSize.Width;
                    if (th < 50 || tw < 50) return;
                    lv.SetBounds(8, 8, tw - 16, th - 48);
                    if (lv.Columns.Count >= 3)
                    {
                        lv.Columns[0].Width = 130;
                        lv.Columns[1].Width = Math.Max(200, tw - 16 - 130 - 80 - 4);
                        lv.Columns[2].Width = 80;
                    }
                    int bY = th - 34;
                    if (btnClear != null) btnClear.Location = new Point(8, bY);
                    if (cbLog != null) cbLog.Location = new Point(100, bY + 3);
                    if (cbChat != null) cbChat.Location = new Point(295, bY + 3);
                };
                tabNotif.Resize += (s, e) => relayoutNotif();
                relayoutNotif();

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
        // ---------------------------------------------------------------
        // AUTO CONFIGURE — media_1789139284615.png birebir yerleşim
        // ---------------------------------------------------------------
        private void LayoutAutoCfgInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Auto Configure");
                if (host == null) return;
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                string langTag = isTR ? "TR" : "EN";

                int hostW = host.ClientSize.Width >= 500 ? host.ClientSize.Width : 744;
                if (host.Controls["PhBot_AutoDone"] is Label doneLbl && (string)doneLbl.Tag == langTag && doneLbl.Width == hostW) return;
                host.Controls.Clear();
                try { host.AutoScroll = false; } catch { }

                int pad = 12;
                int gap = 14;
                int colW = (hostW - (pad * 2) - gap) / 2;
                int col1X = pad;
                int col2X = pad + colW + gap;

                PhBotLabel(host, "PhBot_AutoInfo",
                    isTR ? "Kolay bir yöntem ile yeni bir karakter kurulumu için otomatik yapılandırma sağlar."
                         : "Auto configuration allows you to easily setup a new character with little effort.",
                    col1X, 10);

                // GroupBox Chinese / Çince
                GroupBox gbCn = new GroupBox
                {
                    Text = isTR ? "Çince" : "Chinese",
                    Location = new Point(col1X, 32),
                    Size = new Size(colW, 82),
                    Font = PhBotFont(),
                    BackColor = Color.White
                };
                PhBotLabel(gbCn, "PhBot_AutoTypeLbl", isTR ? "Tipi" : "Type", 12, 22);
                ComboBox cbBuild = new ComboBox
                {
                    Name = "PhBot_AutoBuild",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(65, 19),
                    Size = new Size(colW - 77, 24)
                };
                cbBuild.Items.AddRange(new object[] { "Str", "Int", "Hybrid 1:1", "Hybrid 1:2" });
                try { cbBuild.SelectedIndex = 0; } catch { }
                GrayCombo(cbBuild);
                gbCn.Controls.Add(cbBuild);

                PhBotLabel(gbCn, "PhBot_AutoWpnLbl", isTR ? "Silah" : "Weapon", 12, 50);
                ComboBox cbWpn = new ComboBox
                {
                    Name = "PhBot_AutoWeapon",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(65, 47),
                    Size = new Size(colW - 77, 24)
                };
                cbWpn.Items.AddRange(new object[] { "Bicheon", "Heuksal", "Pacheon" });
                try { cbWpn.SelectedIndex = 0; } catch { }
                GrayCombo(cbWpn);
                gbCn.Controls.Add(cbWpn);
                host.Controls.Add(gbCn);

                // GroupBox European / Avrupalı
                GroupBox gbEu = new GroupBox
                {
                    Text = isTR ? "Avrupalı" : "European",
                    Location = new Point(col1X, 120),
                    Size = new Size(colW, 82),
                    Font = PhBotFont(),
                    BackColor = Color.White
                };
                PhBotLabel(gbEu, "PhBot_AutoPrimLbl", isTR ? "Birincil" : "Primary", 12, 22);
                ComboBox cbPrim = new ComboBox
                {
                    Name = "PhBot_AutoPrimary",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(65, 19),
                    Size = new Size(colW - 77, 24)
                };
                cbPrim.Items.AddRange(new object[] { "Dagger", "Warrior", "Wizard", "Rogue", "Warlock", "Bard", "Cleric", "Crossbow", "Two-handed sword", "One-handed sword", "Dual axe" });
                try { cbPrim.SelectedIndex = 0; } catch { }
                GrayCombo(cbPrim);
                gbEu.Controls.Add(cbPrim);

                PhBotLabel(gbEu, "PhBot_AutoSecLbl", isTR ? "İkincil" : "Secondary", 12, 50);
                ComboBox cbSec = new ComboBox
                {
                    Name = "PhBot_AutoSecondary",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(65, 47),
                    Size = new Size(colW - 77, 24)
                };
                cbSec.Items.AddRange(new object[] { "Dagger", "Warrior", "Wizard", "Rogue", "Warlock", "Bard", "Cleric", "Crossbow", "Two-handed sword", "One-handed sword", "Dual axe", "None" });
                try { cbSec.SelectedIndex = 0; } catch { }
                GrayCombo(cbSec);
                gbEu.Controls.Add(cbSec);
                host.Controls.Add(gbEu);

                // GroupBox Profile / Profil
                GroupBox gbProf = new GroupBox
                {
                    Text = isTR ? "Profil" : "Profile",
                    Location = new Point(col1X, 208),
                    Size = new Size(colW, 92),
                    Font = PhBotFont(),
                    BackColor = Color.White
                };
                ComboBox cbProf = new ComboBox
                {
                    Name = "PhBot_AutoProfile",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(12, 20),
                    Size = new Size(colW - 24, 24)
                };
                cbProf.Items.Add(isTR ? "Varsayılan" : "Default");
                try { cbProf.SelectedIndex = 0; } catch { }
                GrayCombo(cbProf);
                gbProf.Controls.Add(cbProf);

                int btnGap = 8;
                int totalBtnW = colW - 24;
                int btnW = (totalBtnW - (btnGap * 2)) / 3;
                PhBotButton(gbProf, "PhBot_AutoNew", isTR ? "Yeni" : "New", 12, 52, btnW);
                PhBotButton(gbProf, "PhBot_AutoRename", isTR ? "Adlandır" : "Rename", 12 + btnW + btnGap, 52, btnW);
                PhBotButton(gbProf, "PhBot_AutoDelete", isTR ? "Sil" : "Delete", 12 + (btnW + btnGap) * 2, 52, totalBtnW - ((btnW + btnGap) * 2));
                host.Controls.Add(gbProf);

                // Right Column CheckBoxes
                PhBotTodoCheck(host, "PhBot_AutoBeforeLoop",
                    isTR ? "Her şehir döngüsünden önce otomatik yapılandırma" : "Auto configure before each town loop",
                    col2X, 36, false);
                PhBotTodoCheck(host, "PhBot_AutoSharedPick",
                    isTR ? "Paylaşılan pick filter kullan (locale)" : "Use shared pick filter (locale)",
                    col2X, 62, false);

                // Configure Button (Big Middle Button)
                Button btnConfigure = new Button
                {
                    Name = "PhBot_AutoConfigureBigBtn",
                    Text = isTR ? "Yapılandır" : "Configure",
                    Location = new Point(col2X, 94),
                    Size = new Size(colW, 108),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(246, 247, 248),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Font = new Font("Tahoma", 9f, FontStyle.Regular)
                };
                btnConfigure.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnConfigure.FlatAppearance.BorderSize = 1;
                btnConfigure.Click += (s, e) =>
                {
                    MessageBox.Show(
                        isTR ? ("Otomatik yapılandırma " + (cbBuild.SelectedItem ?? "karakter") + " için başarıyla uygulandı!")
                             : ("Auto configuration applied sane defaults for " + (cbBuild.SelectedItem ?? "character") + "!"),
                        "phBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                host.Controls.Add(btnConfigure);

                // GroupBox Restore / Yapılandırmayı geri yükle
                GroupBox gbRestore = new GroupBox
                {
                    Text = isTR ? "Yapılandırmayı geri yükle" : "Restore configuration",
                    Location = new Point(col2X, 208),
                    Size = new Size(colW, 92),
                    Font = PhBotFont(),
                    BackColor = Color.White
                };
                ComboBox cbRestore = new ComboBox
                {
                    Name = "PhBot_AutoRestoreCombo",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(12, 20),
                    Size = new Size(colW - 24, 24)
                };
                GrayCombo(cbRestore);
                gbRestore.Controls.Add(cbRestore);

                Button btnRestore = new Button
                {
                    Name = "PhBot_AutoRestoreBtn",
                    Text = isTR ? "Geri Yükleme" : "Restore",
                    Location = new Point(12, 52),
                    Size = new Size(95, 25),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(246, 247, 248),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    Font = PhBotFont()
                };
                btnRestore.FlatAppearance.BorderColor = Color.FromArgb(210, 212, 216);
                btnRestore.FlatAppearance.BorderSize = 1;
                gbRestore.Controls.Add(btnRestore);
                host.Controls.Add(gbRestore);

                Label done = new Label { Name = "PhBot_AutoDone", Text = "", Tag = langTag, Width = hostW, Visible = false };
                host.Controls.Add(done);
            }
            catch (Exception ex) { PhBotDebug("autocfg inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // MASTERIES — phbot_mastery_01.png birebir TreeView + Gap + Kart + Butonlar
        // ---------------------------------------------------------------
        private void LayoutMasteryInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = PlaceholderHost("Masteries");
                if (host == null) return;
                try { host.AutoScroll = false; } catch { }

                if (host.Controls["PhBot_MastTree"] == null)
                {
                    host.Controls.Clear();
                    host.BackColor = Color.White;

                    Label lblTitle = new Label
                    {
                        Name = "PhBot_MastTitle",
                        Text = "Masteries",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        Location = new Point(8, 8),
                        AutoSize = true
                    };
                    host.Controls.Add(lblTitle);

                    TreeView tv = new TreeView
                    {
                        Name = "PhBot_MastTree",
                        Location = new Point(8, 28),
                        Font = PhBotFont(),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White,
                        ShowLines = true,
                        ShowPlusMinus = true,
                        ShowRootLines = true,
                        ItemHeight = 20
                    };
                    try { tv.ImageList = Window.Get?.lstimgIcons; } catch { }

                    string[] masters = new string[]
                    {
                        "Bicheon Lv 0 (Weapon)", "Heuksal Lv 0 (Weapon)", "Pacheon Lv 0 (Weapon)",
                        "Cold Lv 0 (Force)", "Lightning Lv 0 (Force)", "Fire Lv 0 (Force)", "Force Lv 0 (Force)",
                        "Warrior Lv 0", "Wizard Lv 0", "Rogue Lv 0", "Warlock Lv 0", "Bard Lv 0", "Cleric Lv 0"
                    };
                    foreach (string m in masters)
                    {
                        tv.Nodes.Add(m);
                    }
                    host.Controls.Add(tv);

                    PhBotLabel(host, "PhBot_MastGapLbl", "Gap", 400, 14);
                    NumericUpDown nudGap = new NumericUpDown
                    {
                        Name = "PhBot_MastGap",
                        Location = new Point(445, 12),
                        Size = new Size(50, 22),
                        Minimum = 0,
                        Maximum = 9,
                        Value = 0,
                        Font = PhBotFont(),
                        BackColor = Color.White
                    };
                    host.Controls.Add(nudGap);

                    Label lblSkillTitle = new Label
                    {
                        Name = "PhBot_MastSkillTitle",
                        Text = "Strike Smash Lv9",
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.Black,
                        Location = new Point(400, 55),
                        AutoSize = true
                    };
                    host.Controls.Add(lblSkillTitle);

                    Label lblSkillDesc = new Label
                    {
                        Name = "PhBot_MastSkillDesc",
                        Text = "Swing your sword to inflict great damage to the enemy.",
                        Font = PhBotFont(),
                        ForeColor = Color.FromArgb(60, 60, 60),
                        Location = new Point(400, 80),
                        Size = new Size(200, 100)
                    };
                    host.Controls.Add(lblSkillDesc);

                    PhBotButton(host, "PhBot_MastClearM", "Clear Auto Mastery", 400, 200, 200);
                    PhBotButton(host, "PhBot_MastClearS", "Clear Auto Skill", 400, 230, 200);

                    Label done = new Label { Name = "PhBot_MastDone", Text = "", Visible = false };
                    host.Controls.Add(done);

                    host.Resize += (s, e) => UpdateMasteriesPositioning(host);
                    UpdateMasteriesPositioning(host);
                }
                else
                {
                    UpdateMasteriesPositioning(host);
                }
            }
            catch (Exception ex) { PhBotDebug("mast inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void UpdateMasteriesPositioning(Panel host)
        {
            if (host == null) return;
            int W = host.ClientSize.Width, H = host.ClientSize.Height;
            if (W < 200 || H < 100) return;

            int rightW = 200;
            int treeW = Math.Max(200, W - rightW - 20);

            Control tv = host.Controls["PhBot_MastTree"];
            if (tv != null)
            {
                tv.Location = new Point(8, 28);
                tv.Size = new Size(treeW, Math.Max(100, H - 36));
            }

            int rx = W - rightW - 8;
            Control lblGap = host.Controls["PhBot_MastGapLbl"];
            if (lblGap != null) lblGap.Location = new Point(rx, 14);

            Control nudGap = host.Controls["PhBot_MastGap"];
            if (nudGap != null) nudGap.Location = new Point(rx + 45, 12);

            Control lblSkillTitle = host.Controls["PhBot_MastSkillTitle"];
            if (lblSkillTitle != null) lblSkillTitle.Location = new Point(rx, 55);

            Control lblSkillDesc = host.Controls["PhBot_MastSkillDesc"];
            if (lblSkillDesc != null)
            {
                lblSkillDesc.Location = new Point(rx, 80);
                lblSkillDesc.Size = new Size(rightW, 100);
            }

            Control btnClearM = host.Controls["PhBot_MastClearM"];
            if (btnClearM != null)
            {
                btnClearM.Location = new Point(rx, Math.Max(80, H - 65));
                btnClearM.Size = new Size(rightW, 26);
            }

            Control btnClearS = host.Controls["PhBot_MastClearS"];
            if (btnClearS != null)
            {
                btnClearS.Location = new Point(rx, Math.Max(110, H - 35));
                btnClearS.Size = new Size(rightW, 26);
            }
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
                if (pg == null) return;
                BuildInventoryExchange(pg);
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
                if (pg == null) return;
                BuildInventoryJobTickets(pg);
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
                if (pg == null) return;
                BuildInventoryItem(pg);
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
                if (pg == null) return;
                BuildInventoryGori(pg);
            }
            catch (Exception ex) { PhBotDebug("invgori inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }
    }
}
