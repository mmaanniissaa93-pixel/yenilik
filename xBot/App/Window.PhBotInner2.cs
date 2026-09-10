using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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
                HookInner(TabPageH_Party_Option01_Panel, LayoutPartyMainInner);
                HookInner(TabPageH_Party_Option02_Panel, LayoutPartyAcceptInner);
                HookInner(TabPageH_Party_Option03_Panel, LayoutPartyMatchingInner);
                HookInner(TabPageH_Party_Option04_Panel, LayoutPartyTaxiInner);
                LayoutTownBuyInner();
                LayoutTownOptionsInner();
                LayoutTrainingAreaInner();
                LayoutTrainingScriptInner();
                LayoutPartyMainInner();
                LayoutPartyAcceptInner();
                LayoutPartyMatchingInner();
                LayoutPartyTaxiInner();
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

                // Lojistik kutuları buraya (Buy'dan taşınmış olabilir).
                int y = 8;
                try
                {
                    if (Town_gbxLogistics != null && Town_gbxLogistics.Parent != p && Town_gbxLogistics.Parent == opt1)
                    {
                        try { opt1.Controls.Remove(Town_gbxLogistics); } catch { }
                        try { p.Controls.Add(Town_gbxLogistics); } catch { }
                    }
                    if (Town_gbxAutoBuy != null && Town_gbxAutoBuy.Parent != p && Town_gbxAutoBuy.Parent == opt1)
                    {
                        try { opt1.Controls.Remove(Town_gbxAutoBuy); } catch { }
                        try { p.Controls.Add(Town_gbxAutoBuy); } catch { }
                    }
                    if (Town_gbxLogistics != null && Town_gbxLogistics.Parent == p)
                    {
                        Town_gbxLogistics.Location = new Point(8, y);
                        Town_gbxLogistics.Size = new Size(Math.Max(300, W - 16), Town_gbxLogistics.Height);
                        Town_gbxLogistics.Visible = true;
                        y += Town_gbxLogistics.Height + 8;
                    }
                    if (Town_gbxAutoBuy != null && Town_gbxAutoBuy.Parent == p)
                    {
                        Town_gbxAutoBuy.Location = new Point(8, y);
                        Town_gbxAutoBuy.Size = new Size(Math.Max(300, W - 16), Town_gbxAutoBuy.Height);
                        Town_gbxAutoBuy.Visible = true;
                        y += Town_gbxAutoBuy.Height + 8;
                    }
                }
                catch { }

                // phBot Town > Options 7 maddesi (görsel klon, TODO backend).
                y = TownOptCheck(p, "PhBot_TownSkipGuild", "Skip guild storage if a guild member is nearby", y, false);
                y = TownOptCheck(p, "PhBot_TownGuildRetry", "Guild storage enter retry", y, false, 3);
                y = TownOptCheck(p, "PhBot_TownStopFull", "Stop bot if the inventory is full when buying items", y, false);
                y = TownOptCheck(p, "PhBot_TownNoSpeed", "Do not cast speed or noise during the script", y, false);
                y = TownOptCheck(p, "PhBot_TownSellExcess", "Sell excess potions", y, false);
                y = TownOptCheck(p, "PhBot_TownDrop", "Drop items in town", y, false);
                y = TownOptCheck(p, "PhBot_TownNoLowScript", "Do not use low level town scripts", y, false);
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
        // TRAINING > AREA — liste solda, koordinatlar sağda.
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

                try
                {
                    if (Training_lstvAreas != null)
                    {
                        Training_lstvAreas.Location = new Point(8, 8);
                        Training_lstvAreas.Size = new Size(330, H - 16);
                        Training_lstvAreas.Visible = true;
                        Classicize(Training_lstvAreas);
                    }
                }
                catch { }
                int x = 350, y = 8;
                y = TrainCoordRow(p, Training_lblRegion, Training_tbxRegion, "Region:", x, y, W);
                y = TrainCoordRow(p, Training_lblX, Training_tbxX, "X:", x, y, W);
                y = TrainCoordRow(p, Training_lblY, Training_tbxY, "Y:", x, y, W);
                y = TrainCoordRow(p, Training_lblZ, Training_tbxZ, "Z:", x, y, W);
                y = TrainCoordRow(p, Training_lblRadius, Training_tbxRadius, "Radius:", x, y, W);
                // Pick Radius: phBot'ta Radius gibi çift tıklanır; kutu TODO backend.
                Label pr = p.Controls["PhBot_PickRadiusLbl"] as Label;
                if (pr == null)
                {
                    pr = new Label();
                    pr.Name = "PhBot_PickRadiusLbl";
                    pr.Font = PhBotFont();
                    pr.ForeColor = Color.Black;
                    pr.AutoSize = true;
                    pr.Text = "Pick Radius:";
                    try { p.Controls.Add(pr); } catch { }
                }
                try { pr.Location = new Point(x, y + 4); pr.Visible = true; } catch { }
                PhBotTodoNumber(p, "PhBot_PickRadius", x + 130, y, 80, "30");
                y += 32;
                try
                {
                    if (Training_btnGetCoordinates != null)
                    {
                        Training_btnGetCoordinates.Text = "Get Position";
                        Classicize(Training_btnGetCoordinates);
                        Training_btnGetCoordinates.Location = new Point(x, y);
                        Training_btnGetCoordinates.Size = new Size(130, 28);
                        Training_btnGetCoordinates.Visible = true;
                    }
                }
                catch { }
                y += 38;
                try
                {
                    if (Training_lblScriptPath != null)
                    {
                        Training_lblScriptPath.Location = new Point(x, y + 3);
                        Training_lblScriptPath.Visible = true;
                    }
                    if (Training_tbxScriptPath != null)
                    {
                        Training_tbxScriptPath.Location = new Point(x + 130, y);
                        Training_tbxScriptPath.Size = new Size(Math.Max(120, W - (x + 130) - 45), 24);
                        Training_tbxScriptPath.Visible = true;
                    }
                    if (Training_btnLoadScriptPath != null)
                    {
                        Training_btnLoadScriptPath.Text = "...";
                        Training_btnLoadScriptPath.Font = new Font("Tahoma", 8.25f, FontStyle.Bold);
                        Training_btnLoadScriptPath.Location = new Point(W - 40, y);
                        Training_btnLoadScriptPath.Size = new Size(36, 24);
                        Training_btnLoadScriptPath.Visible = true;
                        Classicize(Training_btnLoadScriptPath);
                    }
                }
                catch { }
                y += 34;
                try
                {
                    if (Training_cbxWalkToCenter != null)
                    {
                        Training_cbxWalkToCenter.Location = new Point(x, y);
                        Training_cbxWalkToCenter.AutoSize = true;
                        Training_cbxWalkToCenter.Visible = true;
                        if (!p.Controls.Contains(Training_cbxWalkToCenter)) p.Controls.Add(Training_cbxWalkToCenter);
                    }
                    if (gbxReturnToArea != null) gbxReturnToArea.Visible = false;
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("trainarea inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private int TrainCoordRow(Panel p, Label lbl, TextBox tbx, string text, int x, int y, int W)
        {
            try
            {
                if (lbl != null)
                {
                    lbl.Text = text;
                    lbl.Font = PhBotFont();
                    lbl.ForeColor = Color.Black;
                    lbl.AutoSize = true;
                    lbl.Location = new Point(x, y + 4);
                    lbl.Visible = true;
                }
                if (tbx != null)
                {
                    tbx.Font = PhBotFont();
                    tbx.BackColor = Color.White;
                    tbx.Location = new Point(x + 130, y);
                    tbx.Size = new Size(110, 24);
                    tbx.Visible = true;
                }
            }
            catch { }
            return y + 32;
        }

        // ---------------------------------------------------------------
        // TRAINING > SCRIPT — Creator + Record + Output.
        // ---------------------------------------------------------------
        private void LayoutTrainingScriptInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Training_Option02_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;
                try
                {
                    if (groupBox2 != null)
                    {
                        groupBox2.Text = "Script Creator";
                        groupBox2.Location = new Point(8, 8);
                        groupBox2.Size = new Size(340, H - 16);
                        groupBox2.Visible = true;
                    }
                    if (Training_gbxRecord != null)
                    {
                        Training_gbxRecord.Location = new Point(356, 8);
                        Training_gbxRecord.Size = new Size(Math.Max(200, W - 364), 120);
                        Training_gbxRecord.Visible = true;
                    }
                    if (Training_gbxOutput != null)
                    {
                        Training_gbxOutput.Location = new Point(356, 136);
                        Training_gbxOutput.Size = new Size(Math.Max(200, W - 364), Math.Max(100, H - 144));
                        Training_gbxOutput.Visible = true;
                    }
                }
                catch (Exception ex) { PhBotDebug("trainscript place: " + ex.Message); }
            }
            catch (Exception ex) { PhBotDebug("trainscript inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > PARTY — üye listesi + Forgotten World.
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
                    if (Party_cbxShowFGWInvites != null)
                    {
                        Party_cbxShowFGWInvites.Text = "Show Forgotten World summons";
                        Party_cbxShowFGWInvites.Font = PhBotFont();
                        Party_cbxShowFGWInvites.ForeColor = Color.Black;
                        Party_cbxShowFGWInvites.AutoSize = true;
                        Party_cbxShowFGWInvites.Location = new Point(8, 8);
                        Party_cbxShowFGWInvites.Visible = true;
                    }
                    if (Party_lblCurrentSetup != null)
                    {
                        Party_lblCurrentSetup.Location = new Point(8, 34);
                        Party_lblCurrentSetup.Visible = true;
                    }
                    if (Party_lstvPartyMembers != null)
                    {
                        Party_lstvPartyMembers.Location = new Point(8, 58);
                        Party_lstvPartyMembers.Size = new Size(W - 16, H - 66);
                        Party_lstvPartyMembers.Visible = true;
                        Classicize(Party_lstvPartyMembers);
                    }
                }
                catch (Exception ex) { PhBotDebug("partymain place: " + ex.Message); }
            }
            catch (Exception ex) { PhBotDebug("partymain inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > ACCEPT/INVITE — grup kutuları çözülür, phBot 15 sırası.
        // ---------------------------------------------------------------
        private void LayoutPartyAcceptInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Party_Option02_Panel;
                if (p == null) return;
                int W = p.Width;
                if (W < 200) return;
                try { p.AutoScroll = true; } catch { }

                GroupBox[] boxes = new GroupBox[] { Party_gbxSetup, Party_gbxAcceptInvite, Party_gbxLeaderList, Party_gbxPlayerList };
                var flat = new List<Control>();
                foreach (GroupBox gbx in boxes)
                {
                    if (gbx == null || gbx.Parent != p) continue;
                    var kids = new List<Control>();
                    foreach (Control k in gbx.Controls) kids.Add(k);
                    foreach (Control k in kids)
                    {
                        try { gbx.Controls.Remove(k); p.Controls.Add(k); flat.Add(k); } catch { }
                    }
                    try { gbx.Visible = false; } catch { }
                }

                int y = 8;
                const int step = 30;
                y = PartyCheck(Party_cbxAcceptAll, "Accept all party invites", 8, y);
                y = PartyCheck(Party_cbxAcceptPartyList, "Accept party invites from list", 8, y);
                y = PartyCheck(Party_cbxInviteAll, "Invite all players", 8, y);
                y = PartyCheck(Party_cbxInvitePartyList, "Invite only from the list", 8, y);
                y = PartyCheck(Party_cbxLeavePartyNoneLeader, "Leave if leader is not in the list", 8, y);
                PhBotTodoCheck(p, "PhBot_LeaveLeaderArea", "Leave if leader is in the training area", 8, y, false); y += step;
                y = PartyCheck(Party_cbxInviteOnlyPartySetup, null, 8, y);
                PhBotTodoCheck(p, "PhBot_OnlyAtArea", "Only accept invites at the training area", 8, y, false); y += step;
                y = PartyCheck(Party_cbxAcceptOnlyPartySetup, null, 8, y);
                y = PartyCheck(Party_cbxActivateLeaderCommands, null, 8, y);
                y = PartyCheck(Party_cbxRefuseInvitations, "Refuse invites from players not in the list", 8, y);
                y = PartyCheck(Party_cbxSetupMasterInvite, null, 8, y);
                y = PartyCheck(Party_cbxAcceptLeaderList, null, 8, y);

                // Party type (exp/item share panelleri) + listeler sağ sütunda.
                int rx = Math.Max(430, W / 2);
                try
                {
                    if (Party_pnlSetupItem != null)
                    {
                        Party_pnlSetupItem.Location = new Point(rx, 8);
                        Party_pnlSetupItem.Visible = true;
                    }
                    if (Party_pnlSetupExp != null)
                    {
                        Party_pnlSetupExp.Location = new Point(rx, 70);
                        Party_pnlSetupExp.Visible = true;
                    }
                }
                catch { }
                int ly = 140;
                ly = PartyListBlock(p, Party_lstvLeaderList, Party_tbxLeader, Party_btnAddLeader, "Party Leader List", rx, ly, W);
                ly = PartyListBlock(p, Party_lstvPartyList, Party_tbxPlayer, Party_btnAddPlayer, "Accept / Invite List", rx, ly, W);
                PhBotTodoCheck(p, "PhBot_AcceptDelayL", "Accept delay (seconds)", rx, ly, false);
                PhBotTodoNumber(p, "PhBot_AcceptDelay", rx + 220, ly - 2, 60, "5");
            }
            catch (Exception ex) { PhBotDebug("partyaccept inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private int PartyCheck(CheckBox cbx, string text, int x, int y)
        {
            try
            {
                if (cbx == null) return y;
                if (text != null) cbx.Text = text;
                Classicize(cbx);
                cbx.AutoSize = true;
                cbx.Location = new Point(x, y);
                cbx.Visible = true;
            }
            catch { }
            return y + 30;
        }

        private int PartyListBlock(Panel p, ListView lv, TextBox tbx, Button btn, string title, int x, int y, int W)
        {
            try
            {
                Label t = p.Controls["PhBot_PLT_" + title.Replace(" ", "")] as Label;
                if (t == null)
                {
                    t = new Label();
                    t.Name = "PhBot_PLT_" + title.Replace(" ", "");
                    t.Font = PhBotFont();
                    t.ForeColor = Color.Black;
                    t.AutoSize = true;
                    t.Text = title;
                    try { p.Controls.Add(t); } catch { }
                }
                t.Location = new Point(x, y);
                t.Visible = true;
                y += 22;
                int lw = Math.Max(200, W - x - 8);
                if (lv != null)
                {
                    lv.Location = new Point(x, y);
                    lv.Size = new Size(lw, 130);
                    lv.Visible = true;
                    Classicize(lv);
                }
                y += 136;
                if (tbx != null)
                {
                    tbx.Font = PhBotFont();
                    tbx.BackColor = Color.White;
                    tbx.Location = new Point(x, y);
                    tbx.Size = new Size(lw - 90, 24);
                    tbx.Visible = true;
                }
                if (btn != null)
                {
                    Classicize(btn);
                    btn.Location = new Point(x + lw - 84, y - 2);
                    btn.Size = new Size(84, 28);
                    btn.Visible = true;
                }
                y += 34;
            }
            catch { }
            return y;
        }

        // ---------------------------------------------------------------
        // PARTY > MATCHING — mevcut + eksik statikler.
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

                int x = 8, y = 8;
                Label lt = p.Controls["PhBot_MatchTitleLbl"] as Label;
                if (lt == null)
                {
                    lt = new Label();
                    lt.Name = "PhBot_MatchTitleLbl";
                    lt.Font = PhBotFont();
                    lt.ForeColor = Color.Black;
                    lt.AutoSize = true;
                    lt.Text = "Title:";
                    try { p.Controls.Add(lt); } catch { }
                }
                try { lt.Location = new Point(x, y + 4); lt.Visible = true; } catch { }
                PhBotTodoNumber(p, "PhBot_MatchTitle", x + 130, y, 220, "");
                try
                {
                    TextBox tt = p.Controls["PhBot_MatchTitle"] as TextBox;
                    if (tt != null) tt.Size = new Size(220, 24);
                }
                catch { }
                PhBotLabel(p, "PhBot_MatchLevelLbl", "Level:", x + 370, y + 4);
                PhBotTodoNumber(p, "PhBot_MatchLevel", x + 430, y, 80, "");
                y += 34;
                try
                {
                    if (Party_lstvPartyMatch != null)
                    {
                        Party_lstvPartyMatch.Location = new Point(x, y);
                        Party_lstvPartyMatch.Size = new Size(Math.Max(300, W - 16), H - y - 130);
                        Party_lstvPartyMatch.Visible = true;
                        Classicize(Party_lstvPartyMatch);
                    }
                }
                catch { }
                int by = H - 118;
                try
                {
                    if (Party_btnRefreshMatch != null)
                    {
                        Party_btnRefreshMatch.Text = "View Party Matching";
                        Classicize(Party_btnRefreshMatch);
                        Party_btnRefreshMatch.Location = new Point(x, by);
                        Party_btnRefreshMatch.Size = new Size(170, 28);
                        Party_btnRefreshMatch.Visible = true;
                    }
                    if (Party_lblJoinToNumber != null)
                    {
                        Party_lblJoinToNumber.Text = "Join party #";
                        Party_lblJoinToNumber.Location = new Point(x, by + 34);
                        Party_lblJoinToNumber.Visible = true;
                    }
                    if (Party_tbxJoinToNumber != null)
                    {
                        Party_tbxJoinToNumber.Location = new Point(x + 130, by + 32);
                        Party_tbxJoinToNumber.Size = new Size(100, 24);
                        Party_tbxJoinToNumber.Visible = true;
                    }
                    if (Party_btnJoinMatch != null)
                    {
                        Classicize(Party_btnJoinMatch);
                        Party_btnJoinMatch.Location = new Point(x + 240, by + 30);
                        Party_btnJoinMatch.Size = new Size(100, 28);
                        Party_btnJoinMatch.Visible = true;
                    }
                    if (Party_pnlAutoFormMatch != null)
                    {
                        Party_pnlAutoFormMatch.Location = new Point(x + 360, by);
                        Party_pnlAutoFormMatch.Size = new Size(Math.Max(200, W - (x + 360) - 8), 70);
                        Party_pnlAutoFormMatch.Visible = true;
                    }
                    if (Party_lblPageNumber != null) Party_lblPageNumber.Visible = false;
                    if (Party_btnLastPage != null) Party_btnLastPage.Visible = false;
                    if (Party_btnNextPage != null) Party_btnNextPage.Visible = false;
                }
                catch { }
                PhBotTodoCheck(p, "PhBot_JoinByName", "Join by name", x + 360, by + 72, false);
                PhBotTodoCheck(p, "PhBot_JoinByTitle", "Join by title", x + 520, by + 72, false);
            }
            catch (Exception ex) { PhBotDebug("partymatch inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // PARTY > TAXI — panel boştu; phBot taksi birebir (görsel klon).
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
                try { p.AutoScroll = true; } catch { }

                ListView lv = p.Controls["PhBot_TaxiList"] as ListView;
                if (lv == null)
                {
                    lv = NewPhBotListView(8, 8, Math.Max(300, W - 420), H - 16, "Player|200", "Time left|120");
                    lv.Name = "PhBot_TaxiList";
                    try { p.Controls.Add(lv); } catch { }
                }
                try
                {
                    lv.Location = new Point(8, 8);
                    lv.Size = new Size(Math.Max(300, W - 420), H - 16);
                    lv.Visible = true;
                }
                catch { }
                int x = W - 400;
                int y = 8;
                PhBotLabel(p, "PhBot_TaxiCostLbl", "Cost per hour:", x, y + 4);
                PhBotTodoNumber(p, "PhBot_TaxiCost", x + 220, y, 100, "0"); y += 32;
                PhBotTodoCheck(p, "PhBot_TaxiKick", "Kick players from party when time is up", x, y, false); y += 30;
                PhBotTodoCheck(p, "PhBot_TaxiMatch", "Add party matching joins to taxi", x, y, false); y += 30;
                PhBotTodoCheck(p, "PhBot_TaxiBlacklist", "Blacklist after", x, y, false);
                PhBotTodoNumber(p, "PhBot_TaxiBlacklistN", x + 220, y - 2, 60, "3");
                PhBotLabel(p, "PhBot_TaxiBlacklistL", "kicks", x + 286, y + 3); y += 32;
                PhBotLabel(p, "PhBot_TaxiExLbl", "Exchange:", x, y + 4);
                PhBotTodoNumber(p, "PhBot_TaxiExchange", x + 220, y, 160, ""); y += 32;
                PhBotLabel(p, "PhBot_TaxiKickLbl", "Kick:", x, y + 4);
                PhBotTodoNumber(p, "PhBot_TaxiKickMsg", x + 220, y, 160, ""); y += 32;
                PhBotLabel(p, "PhBot_TaxiDelayLbl", "Delay in minutes:", x, y + 4);
                PhBotTodoNumber(p, "PhBot_TaxiDelay", x + 220, y, 60, "5");
                // TODO backend: taksi motoru yok (docs/PHBOT_GAP_ANALYSIS.md §11).
            }
            catch (Exception ex) { PhBotDebug("partytaxi inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }
    }
}
