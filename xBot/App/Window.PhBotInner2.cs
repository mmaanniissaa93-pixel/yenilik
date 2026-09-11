using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    /// <summary>
    /// phBot birebir iç düzenler 2. dalga: Town (Buy/Options), Training
    /// (Area/Script), Party (Party/Accept-Matching-Taxi).
    /// Referans: docs/phbot_ref/md/phbot_town.md, phbot_training-area.md,
    /// phbot_party.md + town_buy.png. Aynı-nesne-taşıma kuralı geçerlidir.
    /// </summary>
    public partial class Window
    {
        private void LayoutPhBotInners2()
        {
            try
            {
                HookInner(TabPageH_Town_Option01_Panel, LayoutTownBuyInner);
                HookInner(TabPageH_Town_Option02_Panel, LayoutTownOptionsInner);
                HookInner(TabPageH_Training_Option01_Panel, LayoutTrainingAreaInner);
                HookInner(TabPageH_Training_Option02_Panel, LayoutTrainingScriptInner);
                HookInner(TabPageH_Character_Option03_Panel, LayoutProtectionReturnInner);
                HookInner(TabPageH_Character_Option04_Panel, LayoutProtectionMiscInner);
                HookInner(TabPageH_Party_Option01_Panel, LayoutPartyMainInner);
                HookInner(TabPageH_Party_Option02_Panel, LayoutPartyAcceptInner);
                HookInner(TabPageH_Party_Option03_Panel, LayoutPartyMatchingInner);
                HookInner(TabPageH_Party_Option04_Panel, LayoutPartyTaxiInner);
                Panel pTaxiOpt = GetExtraHTabPanel(TabPageV_Control01_Party_Panel, "TabPageH_Party_Option", "Taxi Options");
                if (pTaxiOpt != null) HookInner(pTaxiOpt, LayoutPartyTaxiOptionsInner);
                LayoutTownBuyInner();
                LayoutTownOptionsInner();
                LayoutTrainingAreaInner();
                LayoutTrainingScriptInner();
                LayoutProtectionReturnInner();
                LayoutProtectionMiscInner();
                LayoutPartyMainInner();
                LayoutPartyAcceptInner();
                LayoutPartyMatchingInner();
                LayoutPartyTaxiInner();
                LayoutPartyTaxiOptionsInner();
            }
            catch (Exception ex) { PhBotDebug("inners2: " + ex.Message); }
        }

        private void HookInner(Panel p, Action fn)
        {
            if (p == null || fn == null) return;
            try
            {
                p.Layout += (s, e) => { try { fn(); } catch { } };
                p.SizeChanged += (s, e) => { try { fn(); } catch { } };
            }
            catch { }
        }

        private static void MoveTo(Control c, Control parent, int x, int y)
        {
            if (c == null || parent == null) return;
            try
            {
                if (c.Parent != parent)
                {
                    try { c.Parent.Controls.Remove(c); } catch { }
                    try { parent.Controls.Add(c); } catch { }
                }
                c.Location = new Point(x, y);
                c.Visible = true;
            }
            catch { }
        }

        private static Button PhBotButton(Control p, string name, string text, int x, int y, int w)
        {
            if (p == null) return null;
            Button b = null;
            try
            {
                if (p.Controls.ContainsKey(name))
                    b = p.Controls[name] as Button;
            }
            catch { }
            if (b == null)
            {
                b = new Button();
                b.Name = name;
                try { p.Controls.Add(b); } catch { }
            }
            try
            {
                b.Text = text;
                b.Font = PhBotFont();
                b.ForeColor = Color.Black;
                b.BackColor = SystemColors.Control;
                b.FlatStyle = FlatStyle.Standard;
                b.UseVisualStyleBackColor = true;
                b.Location = new Point(x, y);
                b.Size = new Size(w, 28);
                b.Visible = true;
            }
            catch { }
            return b;
        }

        // ---------------------------------------------------------------
        // TOWN > BUY — town_buy.png: ID/☑/Icon/Name/Level/Quantity +
        // search + Search/Clear/Refresh/Reset + token notu.
        // ---------------------------------------------------------------
        private void LayoutTownBuyInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Town_Option01_Panel;
                Panel opt2 = TabPageH_Town_Option02_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;
                try { p.AutoScroll = false; } catch { }

                // Lojistik kutuları Options sekmesine taşınır (town loop ayarlarıdır).
                if (opt2 != null)
                {
                    try
                    {
                        if (Town_gbxLogistics != null && Town_gbxLogistics.Parent == p)
                        {
                            try { p.Controls.Remove(Town_gbxLogistics); } catch { }
                            try { opt2.Controls.Add(Town_gbxLogistics); } catch { }
                        }
                        if (Town_gbxAutoBuy != null && Town_gbxAutoBuy.Parent == p)
                        {
                            try { p.Controls.Remove(Town_gbxAutoBuy); } catch { }
                            try { opt2.Controls.Add(Town_gbxAutoBuy); } catch { }
                        }
                    }
                    catch { }
                }

                ListView lv = p.Controls["PhBot_BuyList"] as ListView;
                if (lv == null)
                {
                    lv = new ListView();
                    lv.Name = "PhBot_BuyList";
                    lv.View = View.Details;
                    lv.FullRowSelect = true;
                    lv.GridLines = true;
                    lv.CheckBoxes = true;
                    lv.BackColor = Color.White;
                    lv.ForeColor = Color.Black;
                    lv.Font = PhBotFont();
                    try { lv.BorderStyle = BorderStyle.FixedSingle; } catch { }
                    lv.Columns.Add("", 34);
                    lv.Columns.Add("ID", 50);
                    lv.Columns.Add("Icon", 60);
                    lv.Columns.Add("Name", 420);
                    lv.Columns.Add("Level", 70);
                    lv.Columns.Add("Quantity", 90);
                    string[][] demo = new string[][]
                    {
                        new string[] { "4", "HP Recovery Herb" },
                        new string[] { "5", "HP Recovery Potion (Small)" },
                        new string[] { "6", "HP Recovery Potion (Medium)" },
                        new string[] { "7", "HP Recovery Potion (Large)" },
                        new string[] { "8", "HP Recovery Potion (X-Large)" },
                        new string[] { "9", "HP Recovery Grain (Small)" },
                    };
                    foreach (string[] d in demo)
                    {
                        var it = new ListViewItem("");
                        it.SubItems.Add(d[0]);
                        it.SubItems.Add("");
                        it.SubItems.Add(d[1]);
                        it.SubItems.Add("0");
                        it.SubItems.Add("0");
                        lv.Items.Add(it);
                    }
                    try { p.Controls.Add(lv); } catch { }
                    // TODO backend: NPC satın alma kataloğuna bağlanacak (docs/PHBOT_GAP_ANALYSIS.md §5).
                }
                try
                {
                    lv.Location = new Point(8, 8);
                    lv.Size = new Size(W - 16, H - 118);
                    lv.Visible = true;
                    try { lv.Columns[3].Width = Math.Max(200, W - 16 - 34 - 50 - 60 - 70 - 90 - 4); } catch { }
                }
                catch { }

                TextBox search = p.Controls["PhBot_BuySearch"] as TextBox;
                if (search == null)
                {
                    search = new TextBox();
                    search.Name = "PhBot_BuySearch";
                    search.Font = PhBotFont();
                    search.BackColor = Color.White;
                    try { p.Controls.Add(search); } catch { }
                    search.KeyDown += (s, e) =>
                    {
                        try
                        {
                            if (e.KeyCode == Keys.Enter)
                            {
                                e.SuppressKeyPress = true;
                                FilterBuyList();
                            }
                        }
                        catch { }
                    };
                }
                int rowY = H - 102;
                try
                {
                    search.Location = new Point(8, rowY);
                    search.Size = new Size(Math.Max(200, W - 420), 26);
                    search.Visible = true;
                }
                catch { }
                int bx = W - 404;
                Button bSearch = PhBotButton(p, "PhBot_BuyBtnSearch", "Search", bx, rowY - 1, 92); bx += 100;
                Button bClear = PhBotButton(p, "PhBot_BuyBtnClear", "Clear", bx, rowY - 1, 92); bx += 100;
                Button bRefresh = PhBotButton(p, "PhBot_BuyBtnRefresh", "Refresh", bx, rowY - 1, 92); bx += 100;
                PhBotButton(p, "PhBot_BuyBtnReset", "Reset", bx, rowY - 1, 92);
                try
                {
                    bSearch.Click -= FilterBuyListClick;
                    bSearch.Click += FilterBuyListClick;
                    bClear.Click -= ClearBuyListClick;
                    bClear.Click += ClearBuyListClick;
                }
                catch { }
                Label note = p.Controls["PhBot_BuyNote"] as Label;
                if (note == null)
                {
                    note = new Label();
                    note.Name = "PhBot_BuyNote";
                    note.Font = PhBotFont();
                    note.ForeColor = Color.FromArgb(60, 60, 60);
                    note.AutoSize = true;
                    note.Text = "* Token potions/arrows/bolts will be automatically bought if you are level < 70";
                    try { p.Controls.Add(note); } catch { }
                }
                try { note.Location = new Point(8, rowY + 30); note.Visible = true; }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("townbuy inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void FilterBuyListClick(object sender, EventArgs e) { try { FilterBuyList(); } catch { } }

        private void FilterBuyList()
        {
            try
            {
                Panel p = TabPageH_Town_Option01_Panel;
                if (p == null) return;
                ListView lv = p.Controls["PhBot_BuyList"] as ListView;
                TextBox search = p.Controls["PhBot_BuySearch"] as TextBox;
                if (lv == null || search == null) return;
                string q = (search.Text ?? "").Trim().ToLowerInvariant();
                lv.BeginUpdate();
                try
                {
                    foreach (ListViewItem it in lv.Items)
                    {
                        string name = it.SubItems.Count > 3 ? it.SubItems[3].Text ?? "" : "";
                        it.Selected = q.Length > 0 && name.ToLowerInvariant().Contains(q);
                        if (it.Selected) it.EnsureVisible();
                    }
                }
                finally { lv.EndUpdate(); }
            }
            catch { }
        }

        private void ClearBuyListClick(object sender, EventArgs e)
        {
            try
            {
                Panel p = TabPageH_Town_Option01_Panel;
                if (p == null) return;
                TextBox search = p.Controls["PhBot_BuySearch"] as TextBox;
                ListView lv = p.Controls["PhBot_BuyList"] as ListView;
                if (search != null) search.Text = "";
                if (lv != null)
                {
                    foreach (ListViewItem it in lv.Items) it.Selected = false;
                }
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // TOWN > OPTIONS — town.md: 7 madde + lojistik kutuları.
        // Savaş kutuları Training > Combat'a taşınır.
        // ---------------------------------------------------------------
        private void LayoutTownOptionsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Town_Option02_Panel;
                Panel opt1 = TabPageH_Town_Option01_Panel;
                if (p == null) return;
                int W = p.Width;
                if (W < 200) return;
                try { p.AutoScroll = true; } catch { }

                // Savaş kutuları gerçek evine (Training > Combat).
                try
                {
                    Panel combat = pnlTrainingCombat;
                    if (combat != null)
                    {
                        Control[] movers = new Control[] { Combat_gbxAI, Combat_gbxMobFilter };
                        foreach (Control m in movers)
                        {
                            if (m == null || m.Parent != p) continue;
                            int ny = 8;
                            try
                            {
                                foreach (Control k in combat.Controls)
                                {
                                    try { ny = Math.Max(ny, k.Bottom + 8); } catch { }
                                }
                            }
                            catch { }
                            try { p.Controls.Remove(m); } catch { }
                            try { combat.Controls.Add(m); } catch { }
                            try { m.Location = new Point(8, ny); } catch { }
                            try { m.Visible = true; } catch { }
                        }
                    }
                }
                catch (Exception ex) { PhBotDebug("town combat move: " + ex.Message); }

                // Eski Türkçe kutuları gizle
                if (Town_gbxLogistics != null) Town_gbxLogistics.Visible = false;
                if (Town_gbxAutoBuy != null) Town_gbxAutoBuy.Visible = false;

                // phBot Town > Options 8 maddesi (phbot_town_01.png birebir)
                int y = 14;
                // 1. Skip guild storage if a guild member is nearby
                EnsureBoxCheck(p, "PhBot_TownSkipGuild", "Skip guild storage if a guild member is nearby", 14, y);
                y += 26;

                // 2. Guild storage enter retry [ 3 ]
                PhBotLabel(p, "PhBot_TownGuildRetryLbl", "Guild storage enter retry", 14, y + 2);
                TextBox tbxRetry = p.Controls["PhBot_TownGuildRetryN"] as TextBox;
                if (tbxRetry == null)
                {
                    tbxRetry = new TextBox { Name = "PhBot_TownGuildRetryN", Text = "3", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    p.Controls.Add(tbxRetry);
                }
                tbxRetry.Location = new Point(205, y);
                tbxRetry.Size = new Size(35, 22);
                tbxRetry.Visible = true;
                y += 26;

                // 3. Stop bot if the inventory is full when buying items
                EnsureBoxCheck(p, "PhBot_TownStopFull", "Stop bot if the inventory is full when buying items", 14, y);
                y += 26;

                // 4. Do not cast speed or noise during the script
                EnsureBoxCheck(p, "PhBot_TownNoSpeed", "Do not cast speed or noise during the script", 14, y);
                y += 26;

                // 5. Sell excess potions
                EnsureBoxCheck(p, "PhBot_TownSellExcess", "Sell excess potions", 14, y);
                y += 26;

                // 6. Drop items in town
                EnsureBoxCheck(p, "PhBot_TownDrop", "Drop items in town", 14, y);
                y += 26;

                // 7. Randomize walk coordinates [ 2 ]
                EnsureBoxCheck(p, "PhBot_TownRandWalk", "Randomize walk coordinates", 14, y);
                TextBox tbxRand = p.Controls["PhBot_TownRandWalkN"] as TextBox;
                if (tbxRand == null)
                {
                    tbxRand = new TextBox { Name = "PhBot_TownRandWalkN", Text = "2", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    p.Controls.Add(tbxRand);
                }
                tbxRand.Location = new Point(205, y);
                tbxRand.Size = new Size(35, 22);
                tbxRand.Visible = true;
                y += 26;

                // 8. Do not use low level town scripts
                EnsureBoxCheck(p, "PhBot_TownNoLowScript", "Do not use low level town scripts", 14, y);
            }
            catch (Exception ex) { PhBotDebug("townopt inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private int TownOptCheck(Panel p, string name, string text, int y, bool check, int num = -1)
        {
            try
            {
                PhBotTodoCheck(p, name, text, 8, y, check);
                if (num >= 0)
                    PhBotTodoNumber(p, name + "N", 330, y - 2, 60, num.ToString());
                return y + 30;
            }
            catch { return y + 30; }
        }

        // ---------------------------------------------------------------
        // TRAINING > TRAINING — phbot_training-area_07.png:
        // Sol kolon: Create, Get Position, Walk, koordinat etiketi.
        // Sağ kolon: Training_lstvAreas (Name, File, Radius, Pick Radius, Type).
        // ---------------------------------------------------------------
        private void LayoutTrainingAreaInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Training_Option01_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                // Eski koordinat kutularını ana görünümden gizle
                Control[] toHide = new Control[] {
                    Training_lblRegion, Training_tbxRegion, Training_lblX, Training_tbxX,
                    Training_lblY, Training_tbxY, Training_lblZ, Training_tbxZ,
                    Training_lblRadius, Training_tbxRadius, Training_lblScriptPath,
                    Training_tbxScriptPath, Training_btnLoadScriptPath, Training_cbxWalkToCenter,
                    gbxReturnToArea, p.Controls["PhBot_PickRadiusLbl"], p.Controls["PhBot_PickRadius"]
                };
                foreach (Control c in toHide)
                {
                    if (c != null) c.Visible = false;
                }

                // Sol butonlar: Create, Get Position, Walk
                Button btnCreate = p.Controls["PhBot_TrainCreate"] as Button;
                if (btnCreate == null)
                {
                    btnCreate = new Button();
                    btnCreate.Name = "PhBot_TrainCreate";
                    btnCreate.Text = "Create";
                    btnCreate.Click += (s, e) => {
                        try { Control_Click(this.Menu_lstvArea_Add, null); } catch { }
                    };
                    try { p.Controls.Add(btnCreate); } catch { }
                }
                Classicize(btnCreate);
                btnCreate.Location = new Point(14, 14);
                btnCreate.Size = new Size(96, 26);
                btnCreate.Visible = true;

                if (Training_btnGetCoordinates != null)
                {
                    Training_btnGetCoordinates.Text = "Get Position";
                    Classicize(Training_btnGetCoordinates);
                    Training_btnGetCoordinates.Location = new Point(14, 46);
                    Training_btnGetCoordinates.Size = new Size(96, 26);
                    Training_btnGetCoordinates.Visible = true;
                }

                Button btnWalk = p.Controls["PhBot_TrainWalk"] as Button;
                if (btnWalk == null)
                {
                    btnWalk = new Button();
                    btnWalk.Name = "PhBot_TrainWalk";
                    btnWalk.Text = "Walk";
                    btnWalk.Click += (s, e) => {
                        try
                        {
                            if (Training_lstvAreas != null && Training_lstvAreas.SelectedItems.Count > 0)
                            {
                                var sel = Training_lstvAreas.SelectedItems[0];
                                int x = Convert.ToInt32(sel.SubItems[2].Tag ?? 0);
                                int y = Convert.ToInt32(sel.SubItems[3].Tag ?? 0);
                                if (x != 0 || y != 0)
                                {
                                    Bot.Get.MoveTo(new SRCoord(x, y));
                                }
                            }
                        }
                        catch { }
                    };
                    try { p.Controls.Add(btnWalk); } catch { }
                }
                Classicize(btnWalk);
                btnWalk.Location = new Point(14, 86);
                btnWalk.Size = new Size(96, 26);
                btnWalk.Visible = true;

                // Sol alt koordinat etiketi: "999999, 999999" (X, Y)
                Label lblPos = p.Controls["PhBot_TrainPosLbl"] as Label;
                if (lblPos == null)
                {
                    lblPos = new Label();
                    lblPos.Name = "PhBot_TrainPosLbl";
                    lblPos.Font = PhBotFont();
                    lblPos.ForeColor = Color.Black;
                    lblPos.AutoSize = true;
                    lblPos.Text = "0, 0";
                    try { p.Controls.Add(lblPos); } catch { }
                }
                lblPos.Location = new Point(14, Math.Max(120, H - 30));
                lblPos.Visible = true;
                try
                {
                    if (InfoManager.Character != null)
                    {
                        var cp = InfoManager.Character.GetRealtimePosition();
                        if (cp != null) lblPos.Text = cp.X + ", " + cp.Y;
                    }
                }
                catch { }

                // Sağ liste: Name, File, Radius, Pick Radius, Type
                if (Training_lstvAreas != null)
                {
                    Training_lstvAreas.Location = new Point(120, 14);
                    Training_lstvAreas.Size = new Size(Math.Max(300, W - 128), Math.Max(150, H - 28));
                    Training_lstvAreas.Columns.Clear();
                    Training_lstvAreas.Columns.Add("Name", 120);
                    Training_lstvAreas.Columns.Add("File", 120);
                    Training_lstvAreas.Columns.Add("Radius", 65);
                    Training_lstvAreas.Columns.Add("Pick Radius", 75);
                    Training_lstvAreas.Columns.Add("Type", 65);
                    Training_lstvAreas.Visible = true;
                    Classicize(Training_lstvAreas);
                }
            }
            catch (Exception ex) { PhBotDebug("trainarea inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // TRAINING > SCRIPT — Creator + Record + Output.
        // ---------------------------------------------------------------
        // ---------------------------------------------------------------
        // TRAINING > SCRIPT — phbot_training-area_01.png
        // ---------------------------------------------------------------
        private void LayoutTrainingScriptInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Training_Option02_Panel;
                if (p == null) return;
                p.BackColor = Color.White;
                p.AutoScroll = false;
                try { if (groupBox2 != null) groupBox2.Visible = false; } catch { }
                try { if (Training_gbxRecord != null) Training_gbxRecord.Visible = false; } catch { }
                try { if (Training_gbxOutput != null) Training_gbxOutput.Visible = false; } catch { }

                if (p.Controls.ContainsKey("PhBot_TrScript_SkipTown")) return;

                int y = 14;
                var cSkip = AddPhBotCheck(p, "Skip town script entirely", 20, y, ReturnToAreaPolicy.SkipTownScript);
                cSkip.Name = "PhBot_TrScript_SkipTown";
                cSkip.CheckedChanged += (s, e) => ReturnToAreaPolicy.SkipTownScript = cSkip.Checked;
                y += 24;

                var cCont = AddPhBotCheck(p, "Continue town scripts", 20, y, ReturnToAreaPolicy.ContinueTownScript);
                cCont.CheckedChanged += (s, e) => ReturnToAreaPolicy.ContinueTownScript = cCont.Checked;
                y += 24;

                var cRet = AddPhBotCheck(p, "Return when can't continue script", 20, y, ReturnToAreaPolicy.ReturnIfScriptStuck);
                cRet.CheckedChanged += (s, e) => ReturnToAreaPolicy.ReturnIfScriptStuck = cRet.Checked;
                y += 24;

                var cAvoid = AddPhBotCheck(p, "Avoid Statue of Justice in script", 20, y, ReturnToAreaPolicy.AvoidStatueOfJustice);
                cAvoid.CheckedChanged += (s, e) => ReturnToAreaPolicy.AvoidStatueOfJustice = cAvoid.Checked;
                y += 24;

                AddPhBotCheck(p, "Script walk delay", 20, y, false);
                var nWalk = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(280, y - 2), Size = new Size(60, 22), Maximum = 10000, Value = 1000 };
                var lWalk = new Label { Text = "ms", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(345, y + 2) };
                p.Controls.AddRange(new Control[] { nWalk, lWalk });
                y += 24;

                AddPhBotCheck(p, "Go back a coordinate if stuck after", 20, y, true);
                var nStuck = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(280, y - 2), Size = new Size(60, 22), Value = 15 };
                var lStuck = new Label { Text = "s", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(345, y + 2) };
                p.Controls.AddRange(new Control[] { nStuck, lStuck });
                y += 24;

                AddPhBotCheck(p, "Return if stuck in script after", 20, y, false);
                var nRetStuck = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(280, y - 2), Size = new Size(60, 22), Value = 90 };
                var lRetStuck = new Label { Text = "s", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(345, y + 2) };
                p.Controls.AddRange(new Control[] { nRetStuck, lRetStuck });
                y += 24;

                AddPhBotCheck(p, "Ride fellow pet to training area", 20, y, false);
                y += 22;

                AddPhBotCheck(p, "Remount in caves", 40, y, false);
                y += 22;

                AddPhBotCheck(p, "Use town NPC teleporter for last recall", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Use town NPC teleporter for last death", 20, y, false);
            }
            catch (Exception ex) { PhBotDebug("trainscript inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PROTECTION > RETURN — phbot_protection_04.png
        // ---------------------------------------------------------------
        private void LayoutProtectionReturnInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Character_Option03_Panel;
                if (p == null) return;
                p.BackColor = Color.White;
                p.AutoScroll = false;
                try { if (gbxProtectionSkillPet != null) gbxProtectionSkillPet.Visible = false; } catch { }
                try { if (gbxProtectionAutoStat != null) gbxProtectionAutoStat.Visible = false; } catch { }
                try { if (gbxProtectionReturn != null) gbxProtectionReturn.Visible = false; } catch { }
                try { if (gbxProtectionSummary != null) gbxProtectionSummary.Visible = false; } catch { }

                if (p.Controls.ContainsKey("PhBot_ProtRet_GbxPotions")) return;

                // 1. Potions / Inventory
                var gbxPot = new GroupBox
                {
                    Name = "PhBot_ProtRet_GbxPotions",
                    Text = "Potions / Inventory",
                    Font = PhBotFont(),
                    ForeColor = Color.Black,
                    BackColor = Color.White,
                    Location = new Point(10, 8),
                    Size = new Size(185, 292)
                };
                p.Controls.Add(gbxPot);

                int py = 16;
                var cHP = AddPhBotCheck(gbxPot, "HP potions <=", 10, py, ProtectionManager.ReturnHPLow);
                var nHP = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(120, py - 2), Size = new Size(50, 22), Value = ProtectionManager.HPLowThreshold };
                nHP.ValueChanged += (s, e) => { ProtectionManager.HPLowThreshold = (int)nHP.Value; Settings.SaveCharacterSettings(); };
                cHP.CheckedChanged += (s, e) => { ProtectionManager.ReturnHPLow = cHP.Checked; Settings.SaveCharacterSettings(); };
                gbxPot.Controls.Add(nHP);
                py += 22;

                var cMP = AddPhBotCheck(gbxPot, "<= 5 MP potions left", 10, py, ProtectionManager.ReturnMPLow);
                cMP.CheckedChanged += (s, e) => { ProtectionManager.ReturnMPLow = cMP.Checked; Settings.SaveCharacterSettings(); };
                py += 22;

                var cUniv = AddPhBotCheck(gbxPot, "Out of universal pills", 10, py, false);
                py += 22;

                var cVig = AddPhBotCheck(gbxPot, "Out of vigors", 10, py, false);
                py += 22;

                var cArr = AddPhBotCheck(gbxPot, "Out of arrows/bolts", 10, py, ProtectionManager.ReturnNoArrows);
                cArr.CheckedChanged += (s, e) => { ProtectionManager.ReturnNoArrows = cArr.Checked; Settings.SaveCharacterSettings(); };
                py += 22;

                var cSpd = AddPhBotCheck(gbxPot, "Out of speed scrolls", 10, py, false);
                py += 22;

                var cFull = AddPhBotCheck(gbxPot, "Inventory is full", 10, py, ProtectionManager.ReturnFullInventory);
                cFull.CheckedChanged += (s, e) => { ProtectionManager.ReturnFullInventory = cFull.Checked; Settings.SaveCharacterSettings(); };
                py += 22;

                var cWpn = AddPhBotCheck(gbxPot, "Weapon durability", 10, py, ProtectionManager.ReturnDurabilityLow);
                cWpn.CheckedChanged += (s, e) => { ProtectionManager.ReturnDurabilityLow = cWpn.Checked; Settings.SaveCharacterSettings(); };
                py += 22;

                var cShld = AddPhBotCheck(gbxPot, "Shield durability", 10, py, false);
                py += 22;

                var cArmr = AddPhBotCheck(gbxPot, "Armor durability", 10, py, false);
                py += 22;

                AddPhBotCheck(gbxPot, "Union Party Tickets", 10, py, false);
                py += 22;

                AddPhBotCheck(gbxPot, "Energy of Life", 10, py, false);

                // 2. Time
                var gbxTime = new GroupBox
                {
                    Name = "PhBot_ProtRet_GbxTime",
                    Text = "Time",
                    Font = PhBotFont(),
                    ForeColor = Color.Black,
                    BackColor = Color.White,
                    Location = new Point(202, 8),
                    Size = new Size(250, 292)
                };
                p.Controls.Add(gbxTime);

                int ty = 16;
                AddPhBotCheck(gbxTime, "Return before next hour", 10, ty, false);
                var nHour = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(180, ty - 2), Size = new Size(50, 22), Value = 10 };
                gbxTime.Controls.Add(nHour);
                ty += 24;

                AddPhBotCheck(gbxTime, "Return every", 10, ty, false);
                var nEvery = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(115, ty - 2), Size = new Size(55, 22), Maximum = 99999, Value = 1440 };
                var lEvery = new Label { Text = "minutes", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(175, ty + 2) };
                gbxTime.Controls.AddRange(new Control[] { nEvery, lEvery });
                ty += 24;

                AddPhBotCheck(gbxTime, "Return at", 10, ty, false);
                var dtAt = new DateTimePicker { Font = PhBotFont(), Format = DateTimePickerFormat.Time, ShowUpDown = true, Location = new Point(105, ty - 2), Size = new Size(80, 22) };
                var cbStop = AddPhBotCheck(gbxTime, "stop", 192, ty, false);
                gbxTime.Controls.Add(dtAt);
                ty += 24;

                AddPhBotCheck(gbxTime, "Return/disconnect", 10, ty, false);
                var nRetDisc = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(135, ty - 2), Size = new Size(48, 22), Maximum = 99999, Value = 300 };
                var lRetDisc = new Label { Text = "minutes", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(188, ty + 2) };
                gbxTime.Controls.AddRange(new Control[] { nRetDisc, lRetDisc });
                ty += 24;

                AddPhBotCheck(gbxTime, "Styria (iSRO)", 10, ty, false);
                ty += 24;

                AddPhBotCheck(gbxTime, "Return if less than", 10, ty, false);
                var nPty = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(145, ty - 2), Size = new Size(45, 22), Value = 0 };
                var lPty = new Label { Text = "players are in the party", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(100, ty + 20) };
                gbxTime.Controls.AddRange(new Control[] { nPty, lPty });
                ty += 40;

                AddPhBotCheck(gbxTime, "Disconnect every", 10, ty, false);
                var nDisc = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(135, ty - 2), Size = new Size(48, 22), Maximum = 99999, Value = 300 };
                var lDisc = new Label { Text = "minutes", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(188, ty + 2) };
                gbxTime.Controls.AddRange(new Control[] { nDisc, lDisc });
                ty += 24;

                AddPhBotCheck(gbxTime, "Reset play time (VTC)", 10, ty, false);
                var nReset = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(135, ty - 2), Size = new Size(45, 22), Value = 50 };
                var lReset = new Label { Text = "EXP %", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(186, ty + 2) };
                gbxTime.Controls.AddRange(new Control[] { nReset, lReset });

                // 3. Training
                var gbxTr = new GroupBox
                {
                    Name = "PhBot_ProtRet_GbxTraining",
                    Text = "Training",
                    Font = PhBotFont(),
                    ForeColor = Color.Black,
                    BackColor = Color.White,
                    Location = new Point(458, 8),
                    Size = new Size(260, 292)
                };
                p.Controls.Add(gbxTr);

                int ry = 16;
                var cDead = AddPhBotCheck(gbxTr, "Dead", 10, ry, ProtectionManager.ReturnDeadWithDelay);
                cDead.CheckedChanged += (s, e) => { ProtectionManager.ReturnDeadWithDelay = cDead.Checked; Settings.SaveCharacterSettings(); };
                ry += 22;

                AddPhBotCheck(gbxTr, "Not resurrected", 10, ry, false);
                var nRess = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(135, ry - 2), Size = new Size(45, 22), Value = 1 };
                var lRess = new Label { Text = "minutes", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(185, ry + 2) };
                gbxTr.Controls.AddRange(new Control[] { nRess, lRess });
                ry += 22;

                var cOutside = AddPhBotCheck(gbxTr, "Return immediately if dead outside of\nthe training area", 10, ry, false);
                cOutside.AutoSize = false;
                cOutside.Size = new Size(245, 30);
                ry += 32;

                AddPhBotCheck(gbxTr, "Resurrection scroll", 10, ry, false);
                var nRessSc = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(135, ry - 2), Size = new Size(45, 22), Value = 60 };
                var lRessSc = new Label { Text = "seconds", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(185, ry + 2) };
                gbxTr.Controls.AddRange(new Control[] { nRessSc, lRessSc });
                ry += 22;

                AddPhBotCheck(gbxTr, "Not attacked", 10, ry, false);
                var nNotAtt = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(135, ry - 2), Size = new Size(45, 22), Value = 10 };
                var lNotAtt = new Label { Text = "minutes", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(185, ry + 2) };
                gbxTr.Controls.AddRange(new Control[] { nNotAtt, lNotAtt });
                ry += 22;

                AddPhBotCheck(gbxTr, "Unique spawns near you", 10, ry, false);
                ry += 22;

                AddPhBotCheck(gbxTr, "Job transport dies", 10, ry, false);
                ry += 22;

                AddPhBotCheck(gbxTr, "Pink status", 10, ry, false);
                ry += 22;

                AddPhBotCheck(gbxTr, "Murder status (disconnect)", 10, ry, false);
                ry += 22;

                AddPhBotCheck(gbxTr, "Change primary weapon if broken", 10, ry, false);
            }
            catch (Exception ex) { PhBotDebug("protection return inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PROTECTION > MISC. — phbot_protection_05.png
        // ---------------------------------------------------------------
        private void LayoutProtectionMiscInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Character_Option04_Panel;
                if (p == null) return;
                p.BackColor = Color.White;
                p.AutoScroll = false;
                try { if (Character_cbxPVPModeUseShield != null) Character_cbxPVPModeUseShield.Visible = false; } catch { }
                try { if (Character_cbxPVPMode != null) Character_cbxPVPMode.Visible = false; } catch { }
                try { if (Character_cbxRefuseExchange != null) Character_cbxRefuseExchange.Visible = false; } catch { }
                try { if (Character_cbxApproveExchange != null) Character_cbxApproveExchange.Visible = false; } catch { }
                try { if (Character_cbxConfirmExchange != null) Character_cbxConfirmExchange.Visible = false; } catch { }
                try { if (Character_cbxAcceptExchangeLeaderOnly != null) Character_cbxAcceptExchangeLeaderOnly.Visible = false; } catch { }
                try { if (Character_cbxAcceptExchange != null) Character_cbxAcceptExchange.Visible = false; } catch { }
                try { if (Character_cbxAcceptRessPartyOnly != null) Character_cbxAcceptRessPartyOnly.Visible = false; } catch { }
                try { if (Character_cbxAcceptRess != null) Character_cbxAcceptRess.Visible = false; } catch { }

                if (p.Controls.ContainsKey("PhBot_ProtMisc_DieStop")) return;

                int y = 14;
                var cDie = AddPhBotCheck(p, "Stop bot when you die >", 20, y, false);
                cDie.Name = "PhBot_ProtMisc_DieStop";
                var nDie = new NumericUpDown { Font = PhBotFont(), BackColor = Color.White, Location = new Point(205, y - 2), Size = new Size(50, 22), Value = 0 };
                p.Controls.Add(nDie);
                y += 24;

                AddPhBotCheck(p, "Stop if you die by a Statue of Justice", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Stop bot at 50% EXP (SilkroadR)", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Stop bot at 0% EXP (SilkroadR)", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Remove berserk from the game", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Remove invisible from the game", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Re-spawn the job transport if it dies", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Disconnect if a GM spawns", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Disconnect if a player attacks you while botting", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Stop if a thief player is found in town", 20, y, false);
                y += 24;

                AddPhBotCheck(p, "Only repair equipped items", 20, y, false);
            }
            catch (Exception ex) { PhBotDebug("protection misc inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > PARTY — phbot_party_04.png
        // ---------------------------------------------------------------
        private void LayoutPartyMainInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Party_Option01_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;
                try
                {
                    if (Party_lblCurrentSetup != null) Party_lblCurrentSetup.Visible = false;

                    if (Party_lstvPartyMembers != null)
                    {
                        Party_lstvPartyMembers.Location = new Point(8, 8);
                        Party_lstvPartyMembers.Size = new Size(Math.Max(300, W - 16), Math.Max(100, H - 42));
                        Party_lstvPartyMembers.Visible = true;
                        Classicize(Party_lstvPartyMembers);
                        Party_lstvPartyMembers.Columns.Clear();
                        Party_lstvPartyMembers.Columns.Add("Player", 150);
                        Party_lstvPartyMembers.Columns.Add("Guild", 120);
                        Party_lstvPartyMembers.Columns.Add("Level", 65);
                        Party_lstvPartyMembers.Columns.Add("HP / MP", 120);
                        Party_lstvPartyMembers.Columns.Add("Location", 140);
                        Party_lstvPartyMembers.Columns.Add("Invite", 80);
                    }

                    if (Party_cbxShowFGWInvites != null)
                    {
                        Party_cbxShowFGWInvites.Text = "Show Forgotten World summons";
                        Party_cbxShowFGWInvites.Font = PhBotFont();
                        Party_cbxShowFGWInvites.ForeColor = Color.Black;
                        Party_cbxShowFGWInvites.AutoSize = true;
                        Party_cbxShowFGWInvites.Location = new Point(8, Math.Max(110, H - 28));
                        Party_cbxShowFGWInvites.Visible = true;
                    }
                }
                catch (Exception ex) { PhBotDebug("partymain place: " + ex.Message); }
            }
            catch (Exception ex) { PhBotDebug("partymain inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > OPTIONS (Accept/Invite) — phbot_party_01.png
        // ---------------------------------------------------------------
        private void LayoutPartyAcceptInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Party_Option02_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;
                try { p.AutoScroll = false; } catch { }

                // Grup 1: Accept / Invite (Sol taraf)
                int leftW = 350;
                GroupBox gbAccept = p.Controls["PhBot_PartyGbAccept"] as GroupBox;
                if (gbAccept == null)
                {
                    gbAccept = new GroupBox { Name = "PhBot_PartyGbAccept", Text = "Accept / Invite", Font = PhBotFont(), ForeColor = Color.Black };
                    p.Controls.Add(gbAccept);
                }
                gbAccept.Location = new Point(8, 8);
                gbAccept.Size = new Size(leftW, H - 16);
                gbAccept.Visible = true;

                // 1. Grup (Panel): Accept all party invites / Accept party invites from list + delay
                Panel pnlAcceptRadios = gbAccept.Controls["PhBot_PartyPnlAcceptRadios"] as Panel;
                if (pnlAcceptRadios == null)
                {
                    pnlAcceptRadios = new Panel { Name = "PhBot_PartyPnlAcceptRadios", BackColor = Color.Transparent, BorderStyle = BorderStyle.None, Location = new Point(10, 18), Size = new Size(leftW - 20, 44) };
                    gbAccept.Controls.Add(pnlAcceptRadios);

                    var rbAcceptAll = new RadioButton { Name = "PhBot_PartyRbAcceptAll", Text = "Accept all party invites", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(0, 0), FlatStyle = FlatStyle.Standard, UseVisualStyleBackColor = true };
                    var rbAcceptList = new RadioButton { Name = "PhBot_PartyRbAcceptList", Text = "Accept party invites from list", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(0, 22), FlatStyle = FlatStyle.Standard, UseVisualStyleBackColor = true };
                    var lblDelay = new Label { Name = "PhBot_PartyDelayLbl", Text = "Accept delay", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(180, 2) };
                    var tbxDelay = new TextBox { Name = "PhBot_PartyDelay", Text = "0", Font = PhBotFont(), Size = new Size(30, 20), Location = new Point(255, 0), BackColor = Color.White, ForeColor = Color.Black };
                    var lblSec = new Label { Name = "PhBot_PartySecLbl", Text = "s", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(290, 2) };

                    rbAcceptAll.CheckedChanged += (s, e) => {
                        if (Party_cbxAcceptAll != null) Party_cbxAcceptAll.Checked = rbAcceptAll.Checked;
                        if (Party_cbxAcceptPartyList != null) Party_cbxAcceptPartyList.Checked = !rbAcceptAll.Checked;
                    };
                    rbAcceptAll.Checked = Party_cbxAcceptAll != null ? Party_cbxAcceptAll.Checked : true;
                    rbAcceptList.Checked = !rbAcceptAll.Checked;

                    pnlAcceptRadios.Controls.Add(rbAcceptAll);
                    pnlAcceptRadios.Controls.Add(rbAcceptList);
                    pnlAcceptRadios.Controls.Add(lblDelay);
                    pnlAcceptRadios.Controls.Add(tbxDelay);
                    pnlAcceptRadios.Controls.Add(lblSec);
                }
                pnlAcceptRadios.Location = new Point(10, 18);
                pnlAcceptRadios.Size = new Size(leftW - 20, 44);
                pnlAcceptRadios.Visible = true;

                // 2. Grup (Panel): Invite all players / Invite only from the list
                Panel pnlInviteRadios = gbAccept.Controls["PhBot_PartyPnlInviteRadios"] as Panel;
                if (pnlInviteRadios == null)
                {
                    pnlInviteRadios = new Panel { Name = "PhBot_PartyPnlInviteRadios", BackColor = Color.Transparent, BorderStyle = BorderStyle.None, Location = new Point(10, 66), Size = new Size(leftW - 20, 44) };
                    gbAccept.Controls.Add(pnlInviteRadios);

                    var rbInviteAll = new RadioButton { Name = "PhBot_PartyRbInviteAll", Text = "Invite all players", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(0, 0), FlatStyle = FlatStyle.Standard, UseVisualStyleBackColor = true };
                    var rbInviteList = new RadioButton { Name = "PhBot_PartyRbInviteList", Text = "Invite only from the list", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(0, 22), FlatStyle = FlatStyle.Standard, UseVisualStyleBackColor = true };

                    rbInviteAll.CheckedChanged += (s, e) => {
                        if (Party_cbxInviteAll != null) Party_cbxInviteAll.Checked = rbInviteAll.Checked;
                        if (Party_cbxInvitePartyList != null) Party_cbxInvitePartyList.Checked = !rbInviteAll.Checked;
                    };
                    rbInviteAll.Checked = Party_cbxInviteAll != null ? Party_cbxInviteAll.Checked : true;
                    rbInviteList.Checked = !rbInviteAll.Checked;

                    pnlInviteRadios.Controls.Add(rbInviteAll);
                    pnlInviteRadios.Controls.Add(rbInviteList);
                }
                pnlInviteRadios.Location = new Point(10, 66);
                pnlInviteRadios.Size = new Size(leftW - 20, 44);
                pnlInviteRadios.Visible = true;

                // 3. Grup (Panel): Leave if leader is not in the list / Leave if leader is in the training area
                Panel pnlLeaveRadios = gbAccept.Controls["PhBot_PartyPnlLeaveRadios"] as Panel;
                if (pnlLeaveRadios == null)
                {
                    pnlLeaveRadios = new Panel { Name = "PhBot_PartyPnlLeaveRadios", BackColor = Color.Transparent, BorderStyle = BorderStyle.None, Location = new Point(10, 114), Size = new Size(leftW - 20, 44) };
                    gbAccept.Controls.Add(pnlLeaveRadios);

                    var rbLeaveNotList = new RadioButton { Name = "PhBot_PartyRbLeaveNotList", Text = "Leave if leader is not in the list", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(0, 0), FlatStyle = FlatStyle.Standard, UseVisualStyleBackColor = true };
                    var rbLeaveInArea = new RadioButton { Name = "PhBot_PartyRbLeaveInArea", Text = "Leave if leader is in the training area", Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, Location = new Point(0, 22), FlatStyle = FlatStyle.Standard, UseVisualStyleBackColor = true };

                    rbLeaveNotList.CheckedChanged += (s, e) => {
                        if (Party_cbxLeavePartyNoneLeader != null) Party_cbxLeavePartyNoneLeader.Checked = rbLeaveNotList.Checked;
                    };
                    rbLeaveNotList.Checked = Party_cbxLeavePartyNoneLeader != null ? Party_cbxLeavePartyNoneLeader.Checked : true;
                    rbLeaveInArea.Checked = !rbLeaveNotList.Checked;

                    pnlLeaveRadios.Controls.Add(rbLeaveNotList);
                    pnlLeaveRadios.Controls.Add(rbLeaveInArea);
                }
                pnlLeaveRadios.Location = new Point(10, 114);
                pnlLeaveRadios.Size = new Size(leftW - 20, 44);
                pnlLeaveRadios.Visible = true;

                // Checkboxlar: 7..11
                int y = 162;
                y = PlaceInBox(gbAccept, Party_cbxSetupMasterInvite, "Invite other players to the party (enables invite)", 10, y);
                EnsureBoxCheck(gbAccept, "PhBot_PartyOnlyArea", "Only accept invites at the training area", 10, y);
                y += 22;
                y = PlaceInBox(gbAccept, Party_cbxAcceptOnlyPartySetup, "Accept party invites from other players", 10, y);
                y = PlaceInBox(gbAccept, Party_cbxActivateLeaderCommands, "Allow other party members to invite players", 10, y);
                y = PlaceInBox(gbAccept, Party_cbxRefuseInvitations, "Refuse invites from players not in the list", 10, y);

                // Hide original legacy checkboxes that are now represented by radio panels
                if (Party_cbxAcceptAll != null) Party_cbxAcceptAll.Visible = false;
                if (Party_cbxAcceptPartyList != null) Party_cbxAcceptPartyList.Visible = false;
                if (Party_cbxInviteAll != null) Party_cbxInviteAll.Visible = false;
                if (Party_cbxInvitePartyList != null) Party_cbxInvitePartyList.Visible = false;
                if (Party_cbxLeavePartyNoneLeader != null) Party_cbxLeavePartyNoneLeader.Visible = false;

                // ComboBox at y = 276
                ComboBox cmbPartyType = gbAccept.Controls["PhBot_PartyTypeCmb"] as ComboBox;
                if (cmbPartyType == null)
                {
                    cmbPartyType = new ComboBox { Name = "PhBot_PartyTypeCmb", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont() };
                    cmbPartyType.Items.Add("Experience Distribution, Item Distribution");
                    cmbPartyType.Items.Add("Experience Distribution, Item Individual");
                    cmbPartyType.Items.Add("Experience Individual, Item Distribution");
                    cmbPartyType.Items.Add("Experience Individual, Item Individual");
                    cmbPartyType.SelectedIndex = 0;
                    gbAccept.Controls.Add(cmbPartyType);
                }
                cmbPartyType.Location = new Point(10, Math.Min(276, gbAccept.Height - 32));
                cmbPartyType.Size = new Size(leftW - 20, 24);
                cmbPartyType.Visible = true;

                // Grup 2 & 3: Party Leader List ve Accept / Invite List (Regex)
                int remW = W - leftW - 24;
                int colW = Math.Max(180, remW / 2);

                // Grup 2: Party Leader List
                GroupBox gbLeader = p.Controls["PhBot_PartyGbLeader"] as GroupBox;
                if (gbLeader == null)
                {
                    gbLeader = new GroupBox { Name = "PhBot_PartyGbLeader", Text = "Party Leader List", Font = PhBotFont(), ForeColor = Color.Black };
                    p.Controls.Add(gbLeader);
                }
                gbLeader.Location = new Point(leftW + 14, 8);
                gbLeader.Size = new Size(colW, H - 16);
                gbLeader.Visible = true;

                if (Party_lstvLeaderList != null)
                {
                    if (Party_lstvLeaderList.Parent != gbLeader)
                    {
                        try { Party_lstvLeaderList.Parent?.Controls.Remove(Party_lstvLeaderList); } catch { }
                        gbLeader.Controls.Add(Party_lstvLeaderList);
                    }
                    Party_lstvLeaderList.Location = new Point(8, 20);
                    Party_lstvLeaderList.Size = new Size(colW - 44, gbLeader.Height - 28);
                    Party_lstvLeaderList.Visible = true;
                    Party_lstvLeaderList.HeaderStyle = ColumnHeaderStyle.None;
                    if (Party_lstvLeaderList.Columns.Count > 0)
                        Party_lstvLeaderList.Columns[0].Width = Party_lstvLeaderList.ClientSize.Width - 4;
                    Classicize(Party_lstvLeaderList);
                }
                if (Party_btnAddLeader != null)
                {
                    if (Party_btnAddLeader.Parent != gbLeader)
                    {
                        try { Party_btnAddLeader.Parent?.Controls.Remove(Party_btnAddLeader); } catch { }
                        gbLeader.Controls.Add(Party_btnAddLeader);
                    }
                    Party_btnAddLeader.Text = "A";
                    Party_btnAddLeader.Location = new Point(colW - 32, 20);
                    Party_btnAddLeader.Size = new Size(24, 24);
                    Party_btnAddLeader.Visible = true;
                    Classicize(Party_btnAddLeader);
                }
                Button btnRemLeader = gbLeader.Controls["PhBot_PartyRemLeader"] as Button;
                if (btnRemLeader == null)
                {
                    btnRemLeader = new Button { Name = "PhBot_PartyRemLeader", Text = "R", Font = PhBotFont(), Size = new Size(24, 24), Location = new Point(colW - 32, 48) };
                    btnRemLeader.Click += (s, e) => {
                        try
                        {
                            if (Party_lstvLeaderList != null && Party_lstvLeaderList.SelectedItems.Count > 0)
                                Party_lstvLeaderList.Items.Remove(Party_lstvLeaderList.SelectedItems[0]);
                        }
                        catch { }
                    };
                    gbLeader.Controls.Add(btnRemLeader);
                }
                btnRemLeader.Location = new Point(colW - 32, 48);
                btnRemLeader.Visible = true;
                Classicize(btnRemLeader);

                // Grup 3: Accept / Invite List (Regex)
                int rightX = leftW + 14 + colW + 8;
                int rightW = Math.Max(180, W - rightX - 8);
                GroupBox gbInvite = p.Controls["PhBot_PartyGbInvite"] as GroupBox;
                if (gbInvite == null)
                {
                    gbInvite = new GroupBox { Name = "PhBot_PartyGbInvite", Text = "Accept / Invite List (Regex)", Font = PhBotFont(), ForeColor = Color.Black };
                    p.Controls.Add(gbInvite);
                }
                gbInvite.Location = new Point(rightX, 8);
                gbInvite.Size = new Size(rightW, H - 16);
                gbInvite.Visible = true;

                if (Party_lstvPartyList != null)
                {
                    if (Party_lstvPartyList.Parent != gbInvite)
                    {
                        try { Party_lstvPartyList.Parent?.Controls.Remove(Party_lstvPartyList); } catch { }
                        gbInvite.Controls.Add(Party_lstvPartyList);
                    }
                    Party_lstvPartyList.Location = new Point(8, 20);
                    Party_lstvPartyList.Size = new Size(rightW - 44, gbInvite.Height - 28);
                    Party_lstvPartyList.Visible = true;
                    Party_lstvPartyList.HeaderStyle = ColumnHeaderStyle.None;
                    if (Party_lstvPartyList.Columns.Count > 0)
                        Party_lstvPartyList.Columns[0].Width = Party_lstvPartyList.ClientSize.Width - 4;
                    Classicize(Party_lstvPartyList);
                }
                if (Party_btnAddPlayer != null)
                {
                    if (Party_btnAddPlayer.Parent != gbInvite)
                    {
                        try { Party_btnAddPlayer.Parent?.Controls.Remove(Party_btnAddPlayer); } catch { }
                        gbInvite.Controls.Add(Party_btnAddPlayer);
                    }
                    Party_btnAddPlayer.Text = "A";
                    Party_btnAddPlayer.Location = new Point(rightW - 32, 20);
                    Party_btnAddPlayer.Size = new Size(24, 24);
                    Party_btnAddPlayer.Visible = true;
                    Classicize(Party_btnAddPlayer);
                }
                Button btnRemPlayer = gbInvite.Controls["PhBot_PartyRemPlayer"] as Button;
                if (btnRemPlayer == null)
                {
                    btnRemPlayer = new Button { Name = "PhBot_PartyRemPlayer", Text = "R", Font = PhBotFont(), Size = new Size(24, 24), Location = new Point(rightW - 32, 48) };
                    btnRemPlayer.Click += (s, e) => {
                        try
                        {
                            if (Party_lstvPartyList != null && Party_lstvPartyList.SelectedItems.Count > 0)
                                Party_lstvPartyList.Items.Remove(Party_lstvPartyList.SelectedItems[0]);
                        }
                        catch { }
                    };
                    gbInvite.Controls.Add(btnRemPlayer);
                }
                btnRemPlayer.Location = new Point(rightW - 32, 48);
                btnRemPlayer.Visible = true;
                Classicize(btnRemPlayer);

                // Eski gizlenecek kutular
                if (Party_gbxSetup != null) Party_gbxSetup.Visible = false;
                if (Party_gbxAcceptInvite != null) Party_gbxAcceptInvite.Visible = false;
                if (Party_gbxLeaderList != null) Party_gbxLeaderList.Visible = false;
                if (Party_gbxPlayerList != null) Party_gbxPlayerList.Visible = false;
                if (Party_tbxLeader != null) Party_tbxLeader.Visible = false;
                if (Party_tbxPlayer != null) Party_tbxPlayer.Visible = false;
                if (Party_pnlSetupItem != null) Party_pnlSetupItem.Visible = false;
                if (Party_pnlSetupExp != null) Party_pnlSetupExp.Visible = false;
            }
            catch (Exception ex) { PhBotDebug("partyaccept inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > MATCHING — phbot_party_05.png
        // ---------------------------------------------------------------
        private void LayoutPartyMatchingInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Party_Option03_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                // Title: [textbox]
                PhBotLabel(p, "PhBot_MatchTitleLbl", "Title", 14, 16);
                TextBox tbxTitle = p.Controls["PhBot_MatchTitle"] as TextBox;
                if (tbxTitle == null)
                {
                    tbxTitle = new TextBox { Name = "PhBot_MatchTitle", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    p.Controls.Add(tbxTitle);
                }
                tbxTitle.Location = new Point(55, 14);
                tbxTitle.Size = new Size(320, 22);
                tbxTitle.Visible = true;

                // Level [textbox] to [textbox]  [x] Auto form party matching
                PhBotLabel(p, "PhBot_MatchLevelLbl", "Level", 14, 48);
                TextBox tbxLvlMin = p.Controls["PhBot_MatchLvlMin"] as TextBox;
                if (tbxLvlMin == null)
                {
                    tbxLvlMin = new TextBox { Name = "PhBot_MatchLvlMin", Text = "1", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    p.Controls.Add(tbxLvlMin);
                }
                tbxLvlMin.Location = new Point(55, 46);
                tbxLvlMin.Size = new Size(110, 22);
                tbxLvlMin.Visible = true;

                PhBotLabel(p, "PhBot_MatchToLbl", "to", 175, 48);
                TextBox tbxLvlMax = p.Controls["PhBot_MatchLvlMax"] as TextBox;
                if (tbxLvlMax == null)
                {
                    tbxLvlMax = new TextBox { Name = "PhBot_MatchLvlMax", Text = "140", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    p.Controls.Add(tbxLvlMax);
                }
                tbxLvlMax.Location = new Point(195, 46);
                tbxLvlMax.Size = new Size(110, 22);
                tbxLvlMax.Visible = true;

                EnsureBoxCheck(p, "PhBot_AutoFormMatch", "Auto form party matching", 318, 48);

                // View Party Matching button
                if (Party_btnRefreshMatch != null)
                {
                    Party_btnRefreshMatch.Text = "View Party Matching";
                    Party_btnRefreshMatch.Location = new Point(14, 82);
                    Party_btnRefreshMatch.Size = new Size(150, 26);
                    Party_btnRefreshMatch.Visible = true;
                    Classicize(Party_btnRefreshMatch);
                }

                // Join party # [textbox] [Join]
                if (Party_lblJoinToNumber != null)
                {
                    Party_lblJoinToNumber.Text = "Join party #";
                    Party_lblJoinToNumber.Font = PhBotFont();
                    Party_lblJoinToNumber.ForeColor = Color.Black;
                    Party_lblJoinToNumber.Location = new Point(14, 134);
                    Party_lblJoinToNumber.AutoSize = true;
                    Party_lblJoinToNumber.Visible = true;
                }
                if (Party_tbxJoinToNumber != null)
                {
                    Party_tbxJoinToNumber.Font = PhBotFont();
                    Party_tbxJoinToNumber.BackColor = Color.White;
                    Party_tbxJoinToNumber.ForeColor = Color.Black;
                    Party_tbxJoinToNumber.Location = new Point(130, 132);
                    Party_tbxJoinToNumber.Size = new Size(105, 22);
                    Party_tbxJoinToNumber.Visible = true;
                }
                if (Party_btnJoinMatch != null)
                {
                    Party_btnJoinMatch.Text = "Join";
                    Party_btnJoinMatch.Location = new Point(245, 130);
                    Party_btnJoinMatch.Size = new Size(95, 26);
                    Party_btnJoinMatch.Visible = true;
                    Classicize(Party_btnJoinMatch);
                }

                // [ ] Join by name [textbox]
                EnsureBoxCheck(p, "PhBot_JoinByName", "Join by name", 14, 178);
                TextBox tbxJoinName = p.Controls["PhBot_JoinByNameTbx"] as TextBox;
                if (tbxJoinName == null)
                {
                    tbxJoinName = new TextBox { Name = "PhBot_JoinByNameTbx", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    p.Controls.Add(tbxJoinName);
                }
                tbxJoinName.Location = new Point(130, 176);
                tbxJoinName.Size = new Size(145, 22);
                tbxJoinName.Visible = true;

                // [ ] Join by title [textbox]
                EnsureBoxCheck(p, "PhBot_JoinByTitle", "Join by title", 14, 218);
                TextBox tbxJoinTitle = p.Controls["PhBot_JoinByTitleTbx"] as TextBox;
                if (tbxJoinTitle == null)
                {
                    tbxJoinTitle = new TextBox { Name = "PhBot_JoinByTitleTbx", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    p.Controls.Add(tbxJoinTitle);
                }
                tbxJoinTitle.Location = new Point(130, 216);
                tbxJoinTitle.Size = new Size(245, 22);
                tbxJoinTitle.Visible = true;

                // Hide listview or legacy elements that belong to popup
                if (Party_lstvPartyMatch != null) Party_lstvPartyMatch.Visible = false;
                if (Party_pnlAutoFormMatch != null) Party_pnlAutoFormMatch.Visible = false;
                if (Party_lblPageNumber != null) Party_lblPageNumber.Visible = false;
                if (Party_btnLastPage != null) Party_btnLastPage.Visible = false;
                if (Party_btnNextPage != null) Party_btnNextPage.Visible = false;
            }
            catch (Exception ex) { PhBotDebug("partymatch inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > TAXI — phbot_party_06.png
        // ---------------------------------------------------------------
        private void LayoutPartyTaxiInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Party_Option04_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                ListView lv = p.Controls["PhBot_TaxiList"] as ListView;
                if (lv == null)
                {
                    lv = NewPhBotListView(8, 8, W - 16, H - 16, "Player|320", "Time Left|250");
                    lv.Name = "PhBot_TaxiList";
                    p.Controls.Add(lv);
                }
                lv.Location = new Point(8, 8);
                lv.Size = new Size(W - 16, H - 16);
                lv.Visible = true;
                if (lv.Columns.Count >= 2)
                {
                    lv.Columns[0].Width = 320;
                    lv.Columns[1].Width = Math.Max(200, lv.ClientSize.Width - 324);
                }
                Classicize(lv);
            }
            catch (Exception ex) { PhBotDebug("partytaxi inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > TAXI OPTIONS — phbot_party_02.png
        // ---------------------------------------------------------------
        private void LayoutPartyTaxiOptionsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = GetExtraHTabPanel(TabPageV_Control01_Party_Panel, "TabPageH_Party_Option", "Taxi Options");
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;
                try { p.AutoScroll = false; } catch { }

                int colW = Math.Min(400, (W - 24) / 2);

                // Sol GroupBox: Settings
                GroupBox gbSettings = p.Controls["PhBot_TaxiGbSettings"] as GroupBox;
                if (gbSettings == null)
                {
                    gbSettings = new GroupBox { Name = "PhBot_TaxiGbSettings", Text = "Settings", Font = PhBotFont(), ForeColor = Color.Black };
                    p.Controls.Add(gbSettings);
                }
                gbSettings.Location = new Point(8, 8);
                gbSettings.Size = new Size(colW, H - 16);
                gbSettings.Visible = true;

                // Cost per hour [textbox]
                PhBotLabel(gbSettings, "PhBot_TaxiCostLbl", "Cost per hour", 12, 24);
                TextBox tbxCost = gbSettings.Controls["PhBot_TaxiCost"] as TextBox;
                if (tbxCost == null)
                {
                    tbxCost = new TextBox { Name = "PhBot_TaxiCost", Text = "1000000", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    gbSettings.Controls.Add(tbxCost);
                }
                tbxCost.Location = new Point(95, 22);
                tbxCost.Size = new Size(colW - 110, 22);
                tbxCost.Visible = true;

                // [ ] Kick players from party when time is up
                EnsureBoxCheck(gbSettings, "PhBot_TaxiKick", "Kick players from party when time is up", 12, 54);
                // [ ] Add party matching joins to taxi
                EnsureBoxCheck(gbSettings, "PhBot_TaxiMatch", "Add party matching joins to taxi", 12, 82);

                // [ ] Blacklist after [ 1 ] kicks
                EnsureBoxCheck(gbSettings, "PhBot_TaxiBlKicks", "Blacklist after", 12, 114);
                TextBox tbxBlKicksN = gbSettings.Controls["PhBot_TaxiBlKicksN"] as TextBox;
                if (tbxBlKicksN == null)
                {
                    tbxBlKicksN = new TextBox { Name = "PhBot_TaxiBlKicksN", Text = "1", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    gbSettings.Controls.Add(tbxBlKicksN);
                }
                tbxBlKicksN.Location = new Point(125, 112);
                tbxBlKicksN.Size = new Size(160, 22);
                tbxBlKicksN.Visible = true;
                PhBotLabel(gbSettings, "PhBot_TaxiKicksLbl", "kicks", 290, 114);

                // Blacklist [textbox] [listview] [A] [R]
                PhBotLabel(gbSettings, "PhBot_TaxiBlTitle", "Blacklist", 12, 150);
                TextBox tbxBlAdd = gbSettings.Controls["PhBot_TaxiBlAdd"] as TextBox;
                if (tbxBlAdd == null)
                {
                    tbxBlAdd = new TextBox { Name = "PhBot_TaxiBlAdd", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    gbSettings.Controls.Add(tbxBlAdd);
                }
                tbxBlAdd.Location = new Point(12, 172);
                tbxBlAdd.Size = new Size(180, 22);
                tbxBlAdd.Visible = true;

                ListView lvBl = gbSettings.Controls["PhBot_TaxiBlList"] as ListView;
                if (lvBl == null)
                {
                    lvBl = NewPhBotListView(12, 202, 180, 160, "Name|160");
                    lvBl.Name = "PhBot_TaxiBlList";
                    gbSettings.Controls.Add(lvBl);
                }
                lvBl.Location = new Point(12, 202);
                lvBl.Size = new Size(180, Math.Max(120, gbSettings.Height - 212));
                lvBl.Visible = true;
                lvBl.HeaderStyle = ColumnHeaderStyle.None;
                if (lvBl.Columns.Count > 0) lvBl.Columns[0].Width = lvBl.ClientSize.Width - 4;
                Classicize(lvBl);

                Button btnBlA = gbSettings.Controls["PhBot_TaxiBlA"] as Button;
                if (btnBlA == null)
                {
                    btnBlA = new Button { Name = "PhBot_TaxiBlA", Text = "A", Font = PhBotFont(), Size = new Size(32, 26), Location = new Point(202, 212) };
                    btnBlA.Click += (s, e) => {
                        if (!string.IsNullOrWhiteSpace(tbxBlAdd.Text))
                        {
                            lvBl.Items.Add(tbxBlAdd.Text.Trim());
                            tbxBlAdd.Clear();
                        }
                    };
                    gbSettings.Controls.Add(btnBlA);
                }
                btnBlA.Location = new Point(202, 212);
                btnBlA.Visible = true;
                Classicize(btnBlA);

                Button btnBlR = gbSettings.Controls["PhBot_TaxiBlR"] as Button;
                if (btnBlR == null)
                {
                    btnBlR = new Button { Name = "PhBot_TaxiBlR", Text = "R", Font = PhBotFont(), Size = new Size(32, 26), Location = new Point(202, 248) };
                    btnBlR.Click += (s, e) => {
                        if (lvBl.SelectedItems.Count > 0) lvBl.Items.Remove(lvBl.SelectedItems[0]);
                    };
                    gbSettings.Controls.Add(btnBlR);
                }
                btnBlR.Location = new Point(202, 248);
                btnBlR.Visible = true;
                Classicize(btnBlR);

                // Sağ GroupBox: Messages
                int rightX = colW + 16;
                int rightW = Math.Max(colW, W - rightX - 8);
                GroupBox gbMessages = p.Controls["PhBot_TaxiGbMessages"] as GroupBox;
                if (gbMessages == null)
                {
                    gbMessages = new GroupBox { Name = "PhBot_TaxiGbMessages", Text = "Messages", Font = PhBotFont(), ForeColor = Color.Black };
                    p.Controls.Add(gbMessages);
                }
                gbMessages.Location = new Point(rightX, 8);
                gbMessages.Size = new Size(rightW, H - 16);
                gbMessages.Visible = true;

                // Exchange message
                PhBotLabel(gbMessages, "PhBot_TaxiExMsgLbl", "Exchange", 12, 24);
                TextBox tbxExMsg = gbMessages.Controls["PhBot_TaxiExMsg"] as TextBox;
                if (tbxExMsg == null)
                {
                    tbxExMsg = new TextBox { Name = "PhBot_TaxiExMsg", Text = "Please exchange me and pay %0 gold for the taxi.", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    gbMessages.Controls.Add(tbxExMsg);
                }
                tbxExMsg.Location = new Point(12, 48);
                tbxExMsg.Size = new Size(rightW - 24, 22);
                tbxExMsg.Visible = true;

                // Kick message
                PhBotLabel(gbMessages, "PhBot_TaxiKickMsgLbl", "Kick", 12, 84);
                TextBox tbxKickMsg = gbMessages.Controls["PhBot_TaxiKickMsg"] as TextBox;
                if (tbxKickMsg == null)
                {
                    tbxKickMsg = new TextBox { Name = "PhBot_TaxiKickMsg", Text = "You have %0 minutes to do so and then you will be kicked.", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    gbMessages.Controls.Add(tbxKickMsg);
                }
                tbxKickMsg.Location = new Point(12, 108);
                tbxKickMsg.Size = new Size(rightW - 24, 22);
                tbxKickMsg.Visible = true;

                // Delay in minutes
                PhBotLabel(gbMessages, "PhBot_TaxiDelayLbl", "Delay in minutes", 12, 144);
                TextBox tbxDelay = gbMessages.Controls["PhBot_TaxiDelay"] as TextBox;
                if (tbxDelay == null)
                {
                    tbxDelay = new TextBox { Name = "PhBot_TaxiDelay", Text = "5", Font = PhBotFont(), BackColor = Color.White, ForeColor = Color.Black };
                    gbMessages.Controls.Add(tbxDelay);
                }
                tbxDelay.Location = new Point(12, 168);
                tbxDelay.Size = new Size(rightW - 24, 22);
                tbxDelay.Visible = true;
            }
            catch (Exception ex) { PhBotDebug("partytaxioptions inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private static int PlaceInBox(GroupBox gb, Control c, string text, int x, int y)
        {
            if (c == null) return y;
            if (c.Parent != gb)
            {
                try { c.Parent?.Controls.Remove(c); } catch { }
                gb.Controls.Add(c);
            }
            if (text != null) c.Text = text;
            c.Font = PhBotFont();
            c.ForeColor = Color.Black;
            if (c is ButtonBase bb)
            {
                bb.FlatStyle = FlatStyle.Standard;
                bb.UseVisualStyleBackColor = true;
            }
            c.AutoSize = true;
            c.Location = new Point(x, y);
            c.Visible = true;
            return y + 22;
        }

        private static CheckBox EnsureBoxCheck(Control parent, string name, string text, int x, int y)
        {
            CheckBox cb = parent.Controls[name] as CheckBox;
            if (cb == null)
            {
                cb = new CheckBox { Name = name, Text = text, Font = PhBotFont(), ForeColor = Color.Black, AutoSize = true, FlatStyle = FlatStyle.Standard, UseVisualStyleBackColor = true };
                parent.Controls.Add(cb);
            }
            cb.Text = text;
            cb.FlatStyle = FlatStyle.Standard;
            cb.UseVisualStyleBackColor = true;
            cb.AutoSize = true;
            cb.Location = new Point(x, y);
            cb.Visible = true;
            return cb;
        }
    }
}
