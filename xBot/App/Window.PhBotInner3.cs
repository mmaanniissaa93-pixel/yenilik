using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    /// <summary>
    /// phBot birebir iç düzenler 3. dalga: Players, Guild, Academy, Pet,
    /// Union Party, Stall (+Inventory/Filter sekmeleri).
    /// Referans: docs/phbot_ref/md/phbot_{players,guild,academy,pet,union-party,stall}.md
    /// </summary>
    public partial class Window
    {
        private void LayoutPhBotInners3()
        {
            try
            {
                HookInner(TabPageH_Party_Option01_Panel, LayoutPartyMainInner);
                HookInner(TabPageH_Party_Option02_Panel, LayoutPartyAcceptInner);
                HookInner(TabPageH_Party_Option03_Panel, LayoutPartyMatchingInner);
                HookInner(TabPageH_Party_Option04_Panel, LayoutPartyTaxiInner);
                HookInner(TabPageH_Players_Option01_Panel, LayoutPlayersInner);
                HookInner(TabPageH_Guild_Option01_Panel, LayoutGuildInner);
                HookInner(TabPageH_Stall_Option01_Panel, LayoutStallMainInner);
                HookInner(TabPageH_Stall_Option02_Panel, LayoutStallOptionsInner);
                LayoutPlayersInner();
                LayoutGuildInner();
                LayoutAcademyFull();
                LayoutPetInner();
                LayoutUnionInner();
                LayoutStallMainInner();
                LayoutStallOptionsInner();
                LayoutStallInventoryInner();
                LayoutStallFilterInner();
            }
            catch (Exception ex) { PhBotDebug("inners3: " + ex.Message); }
        }

        // Stall Inventory/Filter sekmeleri WrapAllPhBotStrips ÖNCESİ açılmalıdır.
        private void EnsurePhBotStallExtraTabs()
        {
            try
            {
                if (TabPageV_Control01_Stall_Panel == null) return;
                Panel host = TabPageV_Control01_Stall_Panel;
                if (TabPageH_Stall_Option02 == null) return;
                const string pre = "TabPageH_Stall_Option";
                EnsureExtraHTab(host, TabPageH_Stall_Option02, pre, "Inventory");
                EnsureExtraHTab(host, TabPageH_Stall_Option02, pre, "Filter");
                LayoutHStrip(TabPageH_Stall_Option02.Parent, new string[]
                {
                    "TabPageH_Stall_Option01", pre + "Inventory", pre + "Filter",
                    "TabPageH_Stall_Option03", "TabPageH_Stall_Option02"
                });
            }
            catch (Exception ex) { PhBotDebug("stall tabs: " + ex.Message); }
        }

        private SRPlayer SelectedPlayer()
        {
            try
            {
                if (Players_tvwPlayers != null && Players_tvwPlayers.SelectedNode != null)
                    return Players_tvwPlayers.SelectedNode.Tag as SRPlayer;
            }
            catch { }
            return null;
        }

        // ---------------------------------------------------------------
        // PLAYERS — phbot_players_01.png birebir yerleşim
        // Sol: Players (0) + liste + alt 6 buton
        // Sağ: Trace GroupBox (Player to trace dropdown + butonlar + koordinatlar + 6 checkbox)
        // ---------------------------------------------------------------
        private void LayoutPlayersInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Players_Option01_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                int traceW = 255;
                int listW = Math.Max(280, W - traceW - 16);

                try
                {
                    if (Players_tvwPlayers != null)
                    {
                        Players_tvwPlayers.Location = new Point(8, 28);
                        Players_tvwPlayers.Size = new Size(listW, Math.Max(150, H - 66));
                        Players_tvwPlayers.Visible = true;
                        Classicize(Players_tvwPlayers);
                    }
                    if (Players_lblPlayerCount != null)
                    {
                        Players_lblPlayerCount.Text = "Players (" + (Players_tvwPlayers != null ? Players_tvwPlayers.Nodes.Count : 0) + ")";
                        Players_lblPlayerCount.Font = PhBotFont();
                        Players_lblPlayerCount.ForeColor = Color.Black;
                        Players_lblPlayerCount.Location = new Point(8, 8);
                        Players_lblPlayerCount.AutoSize = true;
                        Players_lblPlayerCount.Visible = true;
                    }
                }
                catch { }

                // Alt 6 buton (phBot players 2..7 butonları)
                int by = H - 32;
                int gap = 4;
                Button bRefresh, bClear, bParty, bAcad, bGuild, bUnion;
                int reqW = 60 + 55 + 76 + 90 + 74 + 108 + (5 * gap); // 483 px
                if (listW < reqW)
                {
                    int avail = listW - (5 * gap);
                    int w1 = 56;
                    int w2 = 50;
                    int w3 = 74;
                    int w4 = 96;
                    int w5 = 78;
                    int w6 = Math.Max(105, avail - (w1 + w2 + w3 + w4 + w5));
                    int curX = 8;
                    bRefresh = EnsurePlayerButton(p, "PhBot_PlayersRefresh", "Refresh", curX, by, w1, 24); curX += w1 + gap;
                    bClear = EnsurePlayerButton(p, "PhBot_PlayersClear", "Clear", curX, by, w2, 24); curX += w2 + gap;
                    bParty = EnsurePlayerButton(p, "PhBot_PlayersPartyInvite", "Party Invite", curX, by, w3, 24); curX += w3 + gap;
                    bAcad = EnsurePlayerButton(p, "PhBot_PlayersAcadInvite", "Academy Invite", curX, by, w4, 24); curX += w4 + gap;
                    bGuild = EnsurePlayerButton(p, "PhBot_PlayersGuildInvite", "Guild Invite", curX, by, w5, 24); curX += w5 + gap;
                    bUnion = EnsurePlayerButton(p, "PhBot_PlayersUnionInvite", "Union Party Invite", curX, by, w6, 24);
                }
                else
                {
                    int w1 = 65, w2 = 65;
                    int w3 = 80, w4 = 98, w5 = 76, w6 = 112;
                    bRefresh = EnsurePlayerButton(p, "PhBot_PlayersRefresh", "Refresh", 8, by, w1, 24);
                    bClear = EnsurePlayerButton(p, "PhBot_PlayersClear", "Clear", 8 + w1 + gap, by, w2, 24);

                    int rx = 8 + listW;
                    int x6 = rx - w6;
                    int x5 = x6 - gap - w5;
                    int x4 = x5 - gap - w4;
                    int x3 = x4 - gap - w3;

                    bParty = EnsurePlayerButton(p, "PhBot_PlayersPartyInvite", "Party Invite", x3, by, w3, 24);
                    bAcad = EnsurePlayerButton(p, "PhBot_PlayersAcadInvite", "Academy Invite", x4, by, w4, 24);
                    bGuild = EnsurePlayerButton(p, "PhBot_PlayersGuildInvite", "Guild Invite", x5, by, w5, 24);
                    bUnion = EnsurePlayerButton(p, "PhBot_PlayersUnionInvite", "Union Party Invite", x6, by, w6, 24);
                }

                if (bRefresh != null && Players_btnRefreshPlayers != null)
                {
                    try { bRefresh.Click -= PlayersRefreshClick; } catch { }
                    bRefresh.Click += PlayersRefreshClick;
                }
                if (bClear != null)
                {
                    try { bClear.Click -= PlayersClearClick; } catch { }
                    bClear.Click += PlayersClearClick;
                }
                if (bParty != null)
                {
                    try { bParty.Click -= PlayersPartyInviteClick; } catch { }
                    bParty.Click += PlayersPartyInviteClick;
                }
                if (bAcad != null)
                {
                    try { bAcad.Click -= PlayersAcadInviteClick; } catch { }
                    bAcad.Click += PlayersAcadInviteClick;
                }
                if (bGuild != null)
                {
                    try { bGuild.Click -= PlayersGuildInviteClick; } catch { }
                    bGuild.Click += PlayersGuildInviteClick;
                }

                // Sağ sütun: Trace GroupBox (phBot players 8..17)
                int gx = W - traceW - 8;
                GroupBox gbTrace = p.Controls["PhBot_TraceGroupBox"] as GroupBox;
                if (gbTrace == null)
                {
                    gbTrace = new GroupBox
                    {
                        Name = "PhBot_TraceGroupBox",
                        Text = "Trace",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        BackColor = Color.White
                    };
                    p.Controls.Add(gbTrace);
                }
                gbTrace.Location = new Point(gx, 8);
                gbTrace.Size = new Size(traceW, H - 16);
                gbTrace.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
                gbTrace.Visible = true;

                PhBotLabel(gbTrace, "PhBot_TraceLbl", "Player to trace", 10, 24);
                ComboBox cbTrace = gbTrace.Controls["PhBot_TraceName"] as ComboBox;
                if (cbTrace == null)
                {
                    cbTrace = new ComboBox
                    {
                        Name = "PhBot_TraceName",
                        Font = PhBotFont(),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        BackColor = Color.White
                    };
                    cbTrace.Items.Add("None");
                    cbTrace.SelectedIndex = 0;
                    GrayCombo(cbTrace);
                    gbTrace.Controls.Add(cbTrace);
                }
                cbTrace.Location = new Point(110, 20);
                cbTrace.Size = new Size(138, 24);
                cbTrace.Visible = true;

                Button bTRefresh = EnsurePlayerButton(gbTrace, "PhBot_TraceRefresh", "Refresh", 15, 52, 75, 24);
                if (bTRefresh != null && Players_btnRefreshPlayers != null)
                {
                    try { bTRefresh.Click -= PlayersRefreshClick; } catch { }
                    bTRefresh.Click += PlayersRefreshClick;
                }
                PhBotLabel(gbTrace, "PhBot_TraceXLbl", "Trace X: 0", 105, 56);

                Button bTStart = EnsurePlayerButton(gbTrace, "PhBot_TraceStart", "Start", 15, 82, 75, 24);
                if (bTStart != null)
                {
                    try { bTStart.Click -= TraceStartClick; } catch { }
                    bTStart.Click += TraceStartClick;
                }
                PhBotLabel(gbTrace, "PhBot_TraceYLbl", "Trace Y: 0", 105, 86);

                Button bTStop = EnsurePlayerButton(gbTrace, "PhBot_TraceStop", "Stop", 15, 112, 75, 24);
                if (bTStop != null)
                {
                    try { bTStop.Click -= TraceStopClick; } catch { }
                    bTStop.Click += TraceStopClick;
                }

                int ty = 146;
                PhBotTodoCheck(gbTrace, "PhBot_TraceAttack", "Attack monsters (uses training radius)", 12, ty, false); ty += 26;
                PhBotTodoCheck(gbTrace, "PhBot_TraceClear", "Kill all monsters before tracing", 12, ty, false); ty += 26;
                PhBotTodoCheck(gbTrace, "PhBot_TraceBuffs", "Buffs", 12, ty, false); ty += 26;
                PhBotTodoCheck(gbTrace, "PhBot_TracePartyBuffs", "Party buffs", 12, ty, false); ty += 26;
                PhBotTodoCheck(gbTrace, "PhBot_TracePick", "Pick items", 12, ty, false); ty += 26;
                PhBotTodoCheck(gbTrace, "PhBot_TraceGame", "Game trace", 12, ty, false);
            }
            catch (Exception ex) { PhBotDebug("players inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private Button EnsurePlayerButton(Control p, string name, string text, int x, int y, int w = 170, int h = 28)
        {
            Button b = p.Controls[name] as Button;
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
                b.Size = new Size(w, h);
                b.Visible = true;
            }
            catch { }
            return b;
        }

        private void PlayersRefreshClick(object sender, EventArgs e)
        {
            try { if (Players_btnRefreshPlayers != null) Players_btnRefreshPlayers.PerformClick(); }
            catch { }
        }

        private void PlayersClearClick(object sender, EventArgs e)
        {
            try
            {
                if (Players_tvwPlayers != null) Players_tvwPlayers.Nodes.Clear();
            }
            catch { }
        }

        private void PlayersPartyInviteClick(object sender, EventArgs e)
        {
            try
            {
                SRPlayer player = SelectedPlayer();
                if (player == null || !InfoManager.inGame) return;
                if (!InfoManager.isEntityNear(player.UniqueID)) return;
                if (InfoManager.inParty)
                    PacketBuilder.InviteToParty(player.UniqueID);
                else
                    PacketBuilder.CreateParty(player.UniqueID, GetPartySetup());
            }
            catch { }
        }

        private void PlayersAcadInviteClick(object sender, EventArgs e)
        {
            try
            {
                SRPlayer player = SelectedPlayer();
                if (player == null) return;
                if (InfoManager.isEntityNear(player.UniqueID))
                {
                    PacketBuilder.InviteToAcademy(player.UniqueID);
                    Log("Academy invitation sent to [" + player.Name + "]");
                }
            }
            catch { }
        }

        private void PlayersGuildInviteClick(object sender, EventArgs e)
        {
            try
            {
                SRPlayer player = SelectedPlayer();
                if (player == null || !InfoManager.inGuild) return;
                if (InfoManager.isEntityNear(player.UniqueID))
                    PacketBuilder.InviteToGuild(player.UniqueID);
            }
            catch { }
        }

        private void TraceStartClick(object sender, EventArgs e)
        {
            try
            {
                string name = "";
                try
                {
                    Panel p = TabPageH_Players_Option01_Panel;
                    TextBox tbx = p != null ? p.Controls["PhBot_TraceName"] as TextBox : null;
                    if (tbx != null) name = (tbx.Text ?? "").Trim();
                }
                catch { }
                if (name.Length == 0)
                {
                    SRPlayer player = SelectedPlayer();
                    if (player != null) name = player.Name;
                }
                if (name.Length > 0 && Bot.Get != null)
                {
                    if (!Bot.Get.StartTrace(name))
                        Log("Trace başlatılamadı: " + name);
                }
            }
            catch { }
        }

        private void TraceStopClick(object sender, EventArgs e)
        {
            try { if (Bot.Get != null) Bot.Get.StopTrace(); }
            catch { }
        }

        // ---------------------------------------------------------------
        // GUILD — docs/phbot_ref/guide/phbot_guild_01.png
        // Üst: Üye listesi (Name, Level, Type, Online, Location)
        // Alt-sol: Invite GroupBox
        // Alt-sağ: View Notice butonu
        // ---------------------------------------------------------------
        private void LayoutGuildInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Guild_Option01_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                int listH = Math.Max(120, H - 140);
                try
                {
                    if (Guild_lstvInfo != null)
                    {
                        Guild_lstvInfo.Location = new Point(8, 8);
                        Guild_lstvInfo.Size = new Size(W - 16, listH);
                        Guild_lstvInfo.Visible = true;
                        Classicize(Guild_lstvInfo);
                        Guild_lstvInfo.Columns.Clear();
                        int colLocW = Math.Max(120, Guild_lstvInfo.ClientSize.Width - 180 - 65 - 85 - 80 - 4);
                        Guild_lstvInfo.Columns.Add("Name", 180);
                        Guild_lstvInfo.Columns.Add("Level", 65, HorizontalAlignment.Center);
                        Guild_lstvInfo.Columns.Add("Type", 85, HorizontalAlignment.Center);
                        Guild_lstvInfo.Columns.Add("Online", 80, HorizontalAlignment.Center);
                        Guild_lstvInfo.Columns.Add("Location", colLocW);
                    }
                    foreach (Control c in new Control[] { Guild_lblLevel, Guild_lblMasterIcon, Guild_lblName, Guild_lblNotice, Guild_btnInfoRefresh })
                    {
                        try { if (c != null) c.Visible = false; } catch { }
                    }
                }
                catch { }

                // Alt sol: Invite grubu
                int by = listH + 16;
                GroupBox gbxInvite = p.Controls["PhBot_GuildInviteGbx"] as GroupBox;
                if (gbxInvite == null)
                {
                    gbxInvite = new GroupBox();
                    gbxInvite.Name = "PhBot_GuildInviteGbx";
                    gbxInvite.Text = "Invite";
                    gbxInvite.Font = PhBotFont();
                    gbxInvite.ForeColor = Color.Black;
                    try { p.Controls.Add(gbxInvite); } catch { }
                }
                gbxInvite.SetBounds(8, by, 420, 115);
                gbxInvite.Visible = true;

                PhBotTodoCheck(gbxInvite, "PhBot_GuildShowInv", "Show guild invites", 14, 22, true);
                PhBotTodoCheck(gbxInvite, "PhBot_GuildAutoInv", "Auto invite other players to the guild with level >", 14, 50, false);
                PhBotTodoNumber(gbxInvite, "PhBot_GuildAutoInvN", 350, 48, 45, "0");
                PhBotTodoCheck(gbxInvite, "PhBot_GuildAcceptAll", "Accept all guild invites", 14, 78, false);

                // Alt sağ: View Notice butonu
                Button bNotice = EnsurePlayerButton(p, "PhBot_GuildNoticeView", "View Notice", Math.Max(450, W - 140), by + 30, 120, 28);
                if (bNotice != null)
                {
                    try { bNotice.Click -= GuildNoticeClick; } catch { }
                    bNotice.Click += GuildNoticeClick;
                }
            }
            catch (Exception ex) { PhBotDebug("guild inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void GuildNoticeClick(object sender, EventArgs e)
        {
            try
            {
                string notice = InfoManager.Guild != null && !string.IsNullOrEmpty(InfoManager.Guild.Notice) ? InfoManager.Guild.Notice : "No notice.";
                MessageBox.Show(notice, "Guild Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // ACADEMY — TabControl (Academy/Options/Matching), phBot birebir.
        // ---------------------------------------------------------------
        private void LayoutAcademyFull()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Academy_Panel;
                if (host == null) return;
                int W = host.Width, H = host.Height;
                if (W < 200 || H < 100) return;

                TabControl tabs = null;
                foreach (Control c in host.Controls)
                {
                    if (c is TabControl && c.Name == "PhBot_AcademyTabs") { tabs = (TabControl)c; break; }
                }
                if (tabs == null)
                {
                    tabs = new TabControl();
                    tabs.Name = "PhBot_AcademyTabs";
                    tabs.Font = PhBotFont();
                    try { host.Controls.Add(tabs); } catch { }
                    tabs.TabPages.Add(new TabPage("Academy"));
                    tabs.TabPages.Add(new TabPage("Options"));
                    tabs.TabPages.Add(new TabPage("Matching"));
                    foreach (TabPage pg0 in tabs.TabPages)
                    {
                        try { pg0.BackColor = Color.White; pg0.AutoScroll = false; } catch { }
                    }
                    // Mevcut içeriği ilk sayfaya taşı.
                    var kids = new List<Control>();
                    foreach (Control k in host.Controls)
                    {
                        if (k != tabs) kids.Add(k);
                    }
                    foreach (Control k in kids)
                    {
                        try { host.Controls.Remove(k); tabs.TabPages[0].Controls.Add(k); } catch { }
                    }
                }
                try
                {
                    tabs.Visible = true;
                }
                catch { }
                TabPage main = tabs.TabPages[0];
                ListView lv = main.Controls["PhBot_AcMembers"] as ListView;
                if (lv == null)
                {
                    main.Controls.Clear();
                    main.BackColor = Color.White;
                    lv = NewPhBotListView(8, 8, W - 16, H - 48, "Name|180", "Level|65", "Position|120", "Type|100", "Online|80");
                    lv.Name = "PhBot_AcMembers";
                    lv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    main.Controls.Add(lv);

                    Button bNotice = PhBotButton(main, "PhBot_AcNoticeView", "View Notice", 8, H - 36, W - 16);
                    if (bNotice != null)
                    {
                        bNotice.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        try { bNotice.Click -= AcademyNoticeClick; } catch { }
                        bNotice.Click += AcademyNoticeClick;
                    }
                }
                if (lv != null)
                {
                    lv.Location = new Point(8, 8);
                    lv.Size = new Size(W - 16, H - 48);
                    if (lv.Columns.Count >= 5)
                    {
                        lv.Columns[0].Width = 180;
                        lv.Columns[1].Width = 65;
                        lv.Columns[2].Width = 125;
                        lv.Columns[3].Width = 100;
                        int rem = lv.ClientSize.Width - 180 - 65 - 125 - 100 - 4;
                        lv.Columns[4].Width = Math.Max(80, rem);
                    }
                }
                BuildAcademyOptions(tabs.TabPages[1]);
                BuildAcademyMatching(tabs.TabPages[2]);
            }
            catch (Exception ex) { PhBotDebug("academy full: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void AcademyNoticeClick(object sender, EventArgs e)
        {
            try { MessageBox.Show(this, "Academy notice: (TODO backend)", ProductName); }
            catch { }
        }

        private void BuildAcademyOptions(TabPage tp)
        {
            if (tp == null || tp.Controls.Count > 0) return;
            try
            {
                tp.BackColor = Color.White;
                tp.AutoScroll = false;
                int W = tp.Width, H = tp.Height;
                int gbH = Math.Max(290, Math.Min(305, H - 20));

                // Sol: Accept / Invite GroupBox
                GroupBox gbOpt = new GroupBox
                {
                    Name = "PhBot_AcOptGroup",
                    Text = "Accept / Invite",
                    Location = new Point(8, 8),
                    Size = new Size(310, gbH),
                    Font = PhBotFont(),
                    ForeColor = Color.Black,
                    BackColor = Color.White
                };
                tp.Controls.Add(gbOpt);

                int y = 20;
                RadioButton rbAccAll = new RadioButton { Name = "PhBot_AcAccAll", Text = "Accept all academy invites", Location = new Point(12, y), AutoSize = true, Checked = true, Font = PhBotFont() };
                gbOpt.Controls.Add(rbAccAll); y += 22;
                RadioButton rbAccList = new RadioButton { Name = "PhBot_AcAccList", Text = "Accept academy invites from list", Location = new Point(12, y), AutoSize = true, Font = PhBotFont() };
                gbOpt.Controls.Add(rbAccList); y += 26;

                RadioButton rbInvAll = new RadioButton { Name = "PhBot_AcInvAll", Text = "Invite all players", Location = new Point(12, y), AutoSize = true, Checked = true, Font = PhBotFont() };
                gbOpt.Controls.Add(rbInvAll); y += 22;
                RadioButton rbInvList = new RadioButton { Name = "PhBot_AcInvList", Text = "Invite only from the list", Location = new Point(12, y), AutoSize = true, Font = PhBotFont() };
                gbOpt.Controls.Add(rbInvList); y += 26;

                PhBotTodoCheck(gbOpt, "PhBot_AcShowInv", "Show academy invites", 12, y, true); y += 22;
                PhBotTodoCheck(gbOpt, "PhBot_AcAcceptJoin", "Accept join invitations", 12, y, false); y += 22;
                PhBotTodoCheck(gbOpt, "PhBot_AcInvitePlayers", "Invite players to the academy", 12, y, false); y += 22;
                PhBotTodoCheck(gbOpt, "PhBot_AcAutoGrad", "Auto graduate from academy", 12, y, false); y += 22;
                PhBotTodoCheck(gbOpt, "PhBot_AcRefuse", "Refuse invites from players not in the list", 12, y, false); y += 26;

                PhBotLabel(gbOpt, "PhBot_AcGradLevelLbl", "Graduate level", 12, y + 4);
                TextBox tbGrad = new TextBox { Name = "PhBot_AcGradLevel", Text = "40", Location = new Point(120, y), Size = new Size(50, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                gbOpt.Controls.Add(tbGrad); y += 26;

                PhBotTodoCheck(gbOpt, "PhBot_AcPassCheck", "Password", 12, y + 2, false);
                TextBox tbPass = new TextBox { Name = "PhBot_AcPass", Location = new Point(120, y), Size = new Size(130, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                gbOpt.Controls.Add(tbPass);

                // Sağ: Accept / Invite List (Regex) GroupBox
                GroupBox gbList = new GroupBox
                {
                    Name = "PhBot_AcListGroup",
                    Text = "Accept / Invite List (Regex)",
                    Location = new Point(326, 8),
                    Size = new Size(240, gbH),
                    Font = PhBotFont(),
                    ForeColor = Color.Black,
                    BackColor = Color.White
                };
                tp.Controls.Add(gbList);

                ListBox lbx = new ListBox
                {
                    Name = "PhBot_AcRegexList",
                    Location = new Point(10, 22),
                    Size = new Size(175, gbH - 35),
                    Font = PhBotFont(),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };
                gbList.Controls.Add(lbx);

                Button btnA = new Button { Name = "PhBot_AcBtnA", Text = "A", Location = new Point(192, 28), Size = new Size(34, 26), Font = PhBotFont(), FlatStyle = FlatStyle.Standard };
                Button btnR = new Button { Name = "PhBot_AcBtnR", Text = "R", Location = new Point(192, 60), Size = new Size(34, 26), Font = PhBotFont(), FlatStyle = FlatStyle.Standard };
                gbList.Controls.Add(btnA);
                gbList.Controls.Add(btnR);
            }
            catch (Exception ex) { PhBotDebug("acadopt: " + ex.Message); }
        }

        private void BuildAcademyMatching(TabPage tp)
        {
            if (tp == null || tp.Controls.Count > 0) return;
            try
            {
                tp.BackColor = Color.White;
                int y = 12;

                PhBotLabel(tp, "PhBot_AcTitleLbl", "Title", 12, y + 4);
                TextBox tbTitle = new TextBox { Name = "PhBot_AcMatchTitle", Text = "Academy", Location = new Point(65, y), Size = new Size(295, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                tp.Controls.Add(tbTitle); y += 32;

                PhBotTodoCheck(tp, "PhBot_AcAutoForm", "Auto form academy matching", 12, y, false); y += 26;

                RadioButton rbAppr = new RadioButton { Name = "PhBot_AcApprentice", Text = "Apprentice", Location = new Point(24, y), AutoSize = true, Checked = true, Font = PhBotFont() };
                tp.Controls.Add(rbAppr); y += 22;
                RadioButton rbAsst = new RadioButton { Name = "PhBot_AcAssistant", Text = "Assistant", Location = new Point(24, y), AutoSize = true, Font = PhBotFont() };
                tp.Controls.Add(rbAsst); y += 32;

                PhBotButton(tp, "PhBot_AcViewMatch", "View Matching", 12, y, 145); y += 36;

                PhBotLabel(tp, "PhBot_AcJoinLbl", "Join academy #", 12, y + 4);
                TextBox tbJoin = new TextBox { Name = "PhBot_AcJoinNum", Location = new Point(125, y), Size = new Size(80, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                tp.Controls.Add(tbJoin);
                PhBotButton(tp, "PhBot_AcJoinBtn", "Join", 215, y - 1, 75); y += 32;

                PhBotTodoCheck(tp, "PhBot_AcJoinNameCheck", "Join by name", 12, y + 2, false);
                TextBox tbName = new TextBox { Name = "PhBot_AcJoinName", Location = new Point(125, y), Size = new Size(165, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                tp.Controls.Add(tbName); y += 32;

                PhBotTodoCheck(tp, "PhBot_AcJoinTitleCheck", "Join by title", 12, y + 2, false);
                TextBox tbTitleMatch = new TextBox { Name = "PhBot_AcJoinTitle", Location = new Point(125, y), Size = new Size(235, 22), Font = PhBotFont(), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
                tp.Controls.Add(tbTitleMatch);
            }
            catch (Exception ex) { PhBotDebug("acmatch: " + ex.Message); }
        }

        // ---------------------------------------------------------------
        // PET — phbot_pet_01.png birebir Attack Pet 3 sütun + göstergeler
        // ---------------------------------------------------------------
        private void LayoutPetInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = null;
                foreach (Control c in pnlWindow.Controls)
                {
                    if (c is Panel && c.Name == "TabPageV_Control01_Pet_Panel") { host = (Panel)c; break; }
                }
                if (host == null) return;
                TabControl tabs = null;
                foreach (Control c in host.Controls)
                {
                    if (c is TabControl) { tabs = (TabControl)c; break; }
                }
                if (tabs == null || tabs.TabPages.Count < 3) return;
                try { tabs.TabPages[0].Text = "Attack Pet"; } catch { }
                try { tabs.TabPages[1].Text = "Transport"; } catch { }
                try { tabs.TabPages[2].Text = "Pick"; } catch { }
                TabPage atk = tabs.TabPages[0];
                int W = Math.Max(500, atk.Width);

                if (atk.Controls["PhBot_PetUse"] == null)
                {
                    try { atk.Controls.Clear(); } catch { }
                    atk.BackColor = Color.White;

                    // 1. Sütun Checkbox'lar
                    PhBotTodoCheck(atk, "PhBot_PetUse", "Use attack pet", 12, 12, false);
                    PhBotTodoCheck(atk, "PhBot_PetSummon", "Auto summon attack pet", 12, 38, false);
                    PhBotTodoCheck(atk, "PhBot_PetRevive", "Auto revive attack pet", 12, 64, false);
                    PhBotTodoCheck(atk, "PhBot_PetReturn", "Return to town when the attack pet dies", 12, 90, false);
                    PhBotTodoCheck(atk, "PhBot_PetSP", "Use fellow pet SP recall in town", 12, 116, false);
                    PhBotTodoCheck(atk, "PhBot_PetProtect", "Protect the attack pet", 12, 142, false);

                    // 2. Sütun Checkbox'lar
                    PhBotTodoCheck(atk, "PhBot_PetPassive", "Don't attack monsters", 215, 12, false);
                    PhBotTodoCheck(atk, "PhBot_PetTownOnly", "only in town", 215, 38, false);
                    NumericUpDown nudTimes = new NumericUpDown
                    {
                        Name = "PhBot_PetMaxTimes",
                        Location = new Point(200, 62),
                        Size = new Size(42, 22),
                        Minimum = 0,
                        Maximum = 999,
                        Value = 0,
                        Font = PhBotFont(),
                        BackColor = Color.White
                    };
                    atk.Controls.Add(nudTimes);
                    PhBotLabel(atk, "PhBot_PetMaxTimesLbl", "max times (0 for unlimited)", 248, 64);
                    PhBotTodoCheck(atk, "PhBot_PetNoRevive", "only if there aren't any revive items", 248, 90, false);

                    // 3. Sütun Checkbox'lar
                    PhBotTodoCheck(atk, "PhBot_PetAreaOnly", "only at training area", 415, 38, false);
                    PhBotTodoCheck(atk, "PhBot_PetHPPotions", "only if HP potions are present", 415, 64, false);

                    // Unsummon Butonu
                    Button bUn = PhBotButton(atk, "PhBot_PetUnsummon", "Unsummon", atk.ClientSize.Width > 150 ? atk.ClientSize.Width - 85 : 520, 12, 75);
                    if (bUn != null)
                    {
                        bUn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                        try { bUn.Click -= PetUnsummonClick; } catch { }
                        bUn.Click += PetUnsummonClick;
                    }

                    // Alt Göstergeler (phbot_pet_01.png birebir)
                    PhBotLabel(atk, "PhBot_PetNameLbl", "Name", 12, 175);
                    PhBotLabel(atk, "PhBot_PetNameVal", "No name", 75, 175);

                    PhBotLabel(atk, "PhBot_PetLevelLbl", "Level", 12, 202);
                    PhBotLabel(atk, "PhBot_PetLevelVal", "0", 75, 202);

                    PhBotLabel(atk, "PhBot_PetHpLbl", "HP", 12, 228);
                    ProgressBar pbHp = new ProgressBar { Name = "PhBot_PetHpBar", Location = new Point(75, 226), Size = new Size(240, 18), Minimum = 0, Maximum = 100, Value = 0 };
                    atk.Controls.Add(pbHp);
                    PhBotLabel(atk, "PhBot_PetHpPct", "0%", 325, 228);

                    PhBotLabel(atk, "PhBot_PetHgpLbl", "HGP", 12, 254);
                    ProgressBar pbHgp = new ProgressBar { Name = "PhBot_PetHgpBar", Location = new Point(75, 252), Size = new Size(240, 18), Minimum = 0, Maximum = 100, Value = 0 };
                    atk.Controls.Add(pbHgp);
                    PhBotLabel(atk, "PhBot_PetHgpPct", "0%", 325, 254);

                    PhBotLabel(atk, "PhBot_PetExpLbl", "EXP", 12, 280);
                    ProgressBar pbExp = new ProgressBar { Name = "PhBot_PetExpBar", Location = new Point(75, 278), Size = new Size(240, 18), Minimum = 0, Maximum = 100, Value = 0 };
                    atk.Controls.Add(pbExp);
                    PhBotLabel(atk, "PhBot_PetExpPct", "0%", 325, 280);

                    Label lblNote = new Label
                    {
                        Name = "PhBot_PetNote",
                        Text = "* Make sure you only have one attack pet in your inventory",
                        Font = PhBotFont(),
                        ForeColor = Color.FromArgb(60, 60, 60),
                        Location = new Point(12, 308),
                        AutoSize = true
                    };
                    atk.Controls.Add(lblNote);
                }

                foreach (TabPage pg in new TabPage[] { tabs.TabPages[1], tabs.TabPages[2] })
                {
                    if (pg.Controls["PhBot_PetInfoDone"] == null)
                    {
                        try { pg.Controls.Clear(); } catch { }
                        pg.BackColor = Color.White;
                        ListView lv = NewPhBotListView(10, 10, 480, 260, "Item|300", "Value|170");
                        pg.Controls.Add(lv);
                        Button bUnOther = PhBotButton(pg, "PhBot_PetUn_" + pg.Text, "Unsummon", 10, 280, 120);
                        if (bUnOther != null)
                        {
                            try { bUnOther.Click -= PetUnsummonClick; } catch { }
                            bUnOther.Click += PetUnsummonClick;
                        }
                        Label dd = new Label { Name = "PhBot_PetInfoDone", Visible = false };
                        pg.Controls.Add(dd);
                    }
                }
            }
            catch (Exception ex) { PhBotDebug("pet inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void PetUnsummonClick(object sender, EventArgs e)
        {
            try
            {
                if (InfoManager.MyPets == null || InfoManager.MyPets.Count == 0)
                {
                    Log("Pet: Aktif çağrılmış pet bulunamadı.");
                    return;
                }

                string btnName = (sender as Control)?.Name ?? "";
                SRCoService targetPet = null;
                if (btnName.Contains("Attack") || btnName == "PhBot_PetUnsummon")
                {
                    targetPet = InfoManager.MyPets.Find(p => p != null && p.isAttackPet());
                }
                else if (btnName.Contains("Pick"))
                {
                    targetPet = InfoManager.MyPets.Find(p => p != null && p.isPickPet());
                }
                else if (btnName.Contains("Transport"))
                {
                    targetPet = InfoManager.MyPets.Find(p => p != null && (p.isTransport() || p.isHorse()));
                }

                if (targetPet == null)
                    targetPet = InfoManager.MyPets.GetAt(0);

                if (targetPet != null)
                {
                    PacketBuilder.UnsummonPet(targetPet.UniqueID);
                    Log(string.Format("Pet unsummon gönderildi: {0} (ID: {1})", targetPet.Name ?? "Pet", targetPet.UniqueID));
                }
            }
            catch (Exception ex)
            {
                Log("Pet unsummon hatası: " + ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // UNION PARTY — phbot_union-party_01.png birebir yerleşim
        // ---------------------------------------------------------------
        private void LayoutUnionInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = null;
                foreach (Control c in pnlWindow.Controls)
                {
                    if (c is Panel && c.Name == "TabPageV_Control01_UnionParty_Panel") { host = (Panel)c; break; }
                }
                if (host == null) return;
                int W = host.ClientSize.Width, H = host.ClientSize.Height;
                if (W < 300 || H < 150) return;
                try { host.AutoScroll = false; } catch { }

                int rightW = 210;
                int leftW = W - rightW - 24;
                int topH = Math.Max(150, H - 105);
                int rx = W - rightW - 12;

                GroupBox gbUnion = host.Controls["PhBot_UnionPartyGroup"] as GroupBox;
                if (gbUnion == null)
                {
                    gbUnion = new GroupBox
                    {
                        Name = "PhBot_UnionPartyGroup",
                        Text = "Union Party",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        BackColor = Color.White
                    };
                    host.Controls.Add(gbUnion);

                    Panel p1 = new Panel { Name = "PhBot_UnionP1", BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
                    Panel p2 = new Panel { Name = "PhBot_UnionP2", BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
                    Panel p3 = new Panel { Name = "PhBot_UnionP3", BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
                    gbUnion.Controls.Add(p1);
                    gbUnion.Controls.Add(p2);
                    gbUnion.Controls.Add(p3);
                }
                gbUnion.Location = new Point(8, 8);
                gbUnion.Size = new Size(leftW, topH);
                gbUnion.Visible = true;

                int colW = (leftW - 24) / 3;
                Control p1C = gbUnion.Controls["PhBot_UnionP1"];
                Control p2C = gbUnion.Controls["PhBot_UnionP2"];
                Control p3C = gbUnion.Controls["PhBot_UnionP3"];
                if (p1C != null) p1C.SetBounds(8, 20, colW, topH - 28);
                if (p2C != null) p2C.SetBounds(12 + colW, 20, colW, topH - 28);
                if (p3C != null) p3C.SetBounds(16 + colW * 2, 20, colW, topH - 28);

                GroupBox gbOpt = host.Controls["PhBot_UnionOptGroup"] as GroupBox;
                if (gbOpt == null)
                {
                    gbOpt = new GroupBox
                    {
                        Name = "PhBot_UnionOptGroup",
                        Text = "Options",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        BackColor = Color.White
                    };
                    host.Controls.Add(gbOpt);

                    PhBotTodoCheck(gbOpt, "PhBot_UnionAccept", "Accept union party invites", 12, 22, false);
                    PhBotTodoCheck(gbOpt, "PhBot_UnionInvite", "Invite players to the union party", 12, 50, false);

                    ComboBox cbDist = new ComboBox
                    {
                        Name = "PhBot_UnionDist",
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = PhBotFont()
                    };
                    cbDist.Items.Add("EXP Free for All / Distribute Items Randomly");
                    cbDist.Items.Add("EXP Distribution / Item Distribution");
                    cbDist.SelectedIndex = 0;
                    GrayCombo(cbDist);
                    gbOpt.Controls.Add(cbDist);
                }
                gbOpt.Location = new Point(8, topH + 12);
                gbOpt.Size = new Size(leftW, H - topH - 20);
                gbOpt.Visible = true;

                ComboBox cbD = gbOpt.Controls["PhBot_UnionDist"] as ComboBox;
                if (cbD != null)
                {
                    cbD.Location = new Point(220, 48);
                    cbD.Size = new Size(Math.Max(150, leftW - 230), 24);
                }

                // Sağ Üst: Invite GroupBox
                GroupBox gbInv = host.Controls["PhBot_UnionInvGroup"] as GroupBox;
                if (gbInv == null)
                {
                    gbInv = new GroupBox
                    {
                        Name = "PhBot_UnionInvGroup",
                        Text = "Invite",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        BackColor = Color.White
                    };
                    host.Controls.Add(gbInv);

                    ListBox lbxInv = new ListBox { Name = "PhBot_UnionLbxInv", BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Font = PhBotFont() };
                    Button btnInvA = new Button { Name = "PhBot_UnionBtnInvA", Text = "A", Size = new Size(26, 24), Font = PhBotFont() };
                    Button btnInvR = new Button { Name = "PhBot_UnionBtnInvR", Text = "R", Size = new Size(26, 24), Font = PhBotFont() };
                    gbInv.Controls.Add(lbxInv);
                    gbInv.Controls.Add(btnInvA);
                    gbInv.Controls.Add(btnInvR);
                }
                gbInv.Location = new Point(rx, 8);
                gbInv.Size = new Size(rightW, (H - 60) / 2);
                gbInv.Visible = true;

                Control lbxInvC = gbInv.Controls["PhBot_UnionLbxInv"];
                Control btnInvAC = gbInv.Controls["PhBot_UnionBtnInvA"];
                Control btnInvRC = gbInv.Controls["PhBot_UnionBtnInvR"];
                if (lbxInvC != null) lbxInvC.SetBounds(8, 20, rightW - 42, gbInv.Height - 28);
                if (btnInvAC != null) btnInvAC.Location = new Point(rightW - 32, 20);
                if (btnInvRC != null) btnInvRC.Location = new Point(rightW - 32, 48);

                // Sağ Orta: Accept GroupBox
                int accY = 8 + gbInv.Height + 8;
                GroupBox gbAcc = host.Controls["PhBot_UnionAccGroup"] as GroupBox;
                if (gbAcc == null)
                {
                    gbAcc = new GroupBox
                    {
                        Name = "PhBot_UnionAccGroup",
                        Text = "Accept",
                        Font = PhBotFont(),
                        ForeColor = Color.Black,
                        BackColor = Color.White
                    };
                    host.Controls.Add(gbAcc);

                    ListBox lbxAcc = new ListBox { Name = "PhBot_UnionLbxAcc", BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Font = PhBotFont() };
                    Button btnAccA = new Button { Name = "PhBot_UnionBtnAccA", Text = "A", Size = new Size(26, 24), Font = PhBotFont() };
                    Button btnAccR = new Button { Name = "PhBot_UnionBtnAccR", Text = "R", Size = new Size(26, 24), Font = PhBotFont() };
                    gbAcc.Controls.Add(lbxAcc);
                    gbAcc.Controls.Add(btnAccA);
                    gbAcc.Controls.Add(btnAccR);
                }
                gbAcc.Location = new Point(rx, accY);
                gbAcc.Size = new Size(rightW, (H - 60) / 2);
                gbAcc.Visible = true;

                Control lbxAccC = gbAcc.Controls["PhBot_UnionLbxAcc"];
                Control btnAccAC = gbAcc.Controls["PhBot_UnionBtnAccA"];
                Control btnAccRC = gbAcc.Controls["PhBot_UnionBtnAccR"];
                if (lbxAccC != null) lbxAccC.SetBounds(8, 20, rightW - 42, gbAcc.Height - 28);
                if (btnAccAC != null) btnAccAC.Location = new Point(rightW - 32, 20);
                if (btnAccRC != null) btnAccRC.Location = new Point(rightW - 32, 48);

                // Sağ Alt: Dismiss / Leave Butonu
                Button btnLeave = host.Controls["PhBot_UnionLeave"] as Button;
                if (btnLeave == null)
                {
                    btnLeave = PhBotButton(host, "PhBot_UnionLeave", "Dismiss / Leave", rx, H - 32, rightW);
                }
                btnLeave.SetBounds(rx, H - 32, rightW, 26);
                btnLeave.Visible = true;
                Classicize(btnLeave);
            }
            catch (Exception ex) { PhBotDebug("union inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // STALL > STALL — mevcut slot/envanter düzenlenir.
        // ---------------------------------------------------------------
        private void LayoutStallMainInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Stall_Option01_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;
                int half = (W - 24) / 2;
                try
                {
                    Panel invPg = FindHTabPanel(TabPageV_Control01_Stall_Panel, "TabPageH_Stall_Option", "Inventory");
                    if (invPg != null && Stall_lstvInventoryStall != null)
                    {
                        if (Stall_lstvInventoryStall.Parent != invPg)
                        {
                            try { Stall_lstvInventoryStall.Parent?.Controls.Remove(Stall_lstvInventoryStall); } catch { }
                            try { invPg.Controls.Add(Stall_lstvInventoryStall); } catch { }
                        }
                        Stall_lstvInventoryStall.Location = new Point(8, 8);
                        Stall_lstvInventoryStall.Size = new Size(Math.Max(300, invPg.Width - 16), Math.Max(200, invPg.Height - 16));
                        Stall_lstvInventoryStall.Visible = true;
                        Classicize(Stall_lstvInventoryStall);
                    }

                    if (Stall_lstvStall != null)
                    {
                        Stall_lstvStall.Location = new Point(8, 8);
                        Stall_lstvStall.Size = new Size(Math.Max(200, W - 130), Math.Max(100, H - 16));
                        Stall_lstvStall.Visible = true;
                        Classicize(Stall_lstvStall);
                    }

                    int bx = W - 116;
                    if (Stall_btnIGCreateModify != null)
                    {
                        Stall_btnIGCreateModify.Location = new Point(bx, 10);
                        Stall_btnIGCreateModify.Size = new Size(104, 26);
                        Stall_btnIGCreateModify.Text = "Create Stall";
                        Stall_btnIGCreateModify.Visible = true;
                    }
                    Button btnOpenStall = p.Controls["PhBot_StallOpen"] as Button;
                    if (btnOpenStall == null)
                    {
                        btnOpenStall = PhBotButton(p, "PhBot_StallOpen", "Open Stall", bx, 42, 104);
                        if (btnOpenStall != null) btnOpenStall.Enabled = false;
                    }
                    else
                    {
                        btnOpenStall.Location = new Point(bx, 42);
                        btnOpenStall.Size = new Size(104, 26);
                        btnOpenStall.Visible = true;
                    }
                    if (Stall_btnClose != null)
                    {
                        Stall_btnClose.Location = new Point(bx, 74);
                        Stall_btnClose.Size = new Size(104, 26);
                        Stall_btnClose.Text = "Close Stall";
                        Stall_btnClose.Visible = true;
                    }
                    Button btnExitStall = p.Controls["PhBot_StallExit"] as Button;
                    if (btnExitStall == null)
                    {
                        btnExitStall = PhBotButton(p, "PhBot_StallExit", "Exit", bx, 106, 104);
                    }
                    else
                    {
                        btnExitStall.Location = new Point(bx, 106);
                        btnExitStall.Size = new Size(104, 26);
                        btnExitStall.Visible = true;
                    }

                    try { if (Stall_lblInventoryStall != null) Stall_lblInventoryStall.Visible = false; } catch { }
                    try { if (Stall_tbxTitle != null) Stall_tbxTitle.Visible = false; } catch { }
                    try { if (Stall_btnTitleEdit != null) Stall_btnTitleEdit.Visible = false; } catch { }
                    try { if (Stall_tbxNote != null) Stall_tbxNote.Visible = false; } catch { }
                    try { if (Stall_btnNoteEdit != null) Stall_btnNoteEdit.Visible = false; } catch { }
                    try { if (Stall_tbxPrice != null) Stall_tbxPrice.Visible = false; } catch { }
                    try { if (Stall_tbxQuantity != null) Stall_tbxQuantity.Visible = false; } catch { }
                    try { if (Stall_btnAddItem != null) Stall_btnAddItem.Visible = false; } catch { }
                    try { if (Stall_lblState != null) Stall_lblState.Visible = false; } catch { }
                }
                catch (Exception ex) { PhBotDebug("stallmain place: " + ex.Message); }
            }
            catch (Exception ex) { PhBotDebug("stallmain inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void LayoutStallOptionsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Stall_Option02_Panel;
                if (p == null) return;
                p.AutoScroll = false;
                p.BackColor = Color.White;
                int W = p.Width;
                if (W < 200) return;

                // Hide obsolete designer controls
                try { if (Stall_lblStallTitle != null) Stall_lblStallTitle.Visible = false; } catch { }
                try { if (Stall_lblStallNote != null) Stall_lblStallNote.Visible = false; } catch { }
                try { if (Stall_cbxAutoOpenPlayerStall != null) Stall_cbxAutoOpenPlayerStall.Visible = false; } catch { }
                try { if (Stall_lblMinimumPlayerStallItems != null) Stall_lblMinimumPlayerStallItems.Visible = false; } catch { }
                try { if (Stall_nudMinimumPlayerStallItems != null) Stall_nudMinimumPlayerStallItems.Visible = false; } catch { }

                // Authentic phBot stall options (phbot_stall_02.png)
                CheckBox cbAutoStall = p.Controls["PhBot_StallOptAuto"] as CheckBox;
                if (cbAutoStall == null)
                {
                    cbAutoStall = AddPhBotCheck(p, "Automatically stall items when at least", 15, 15, Stall_cbxAutoOpenPlayerStall != null && Stall_cbxAutoOpenPlayerStall.Checked);
                    cbAutoStall.Name = "PhBot_StallOptAuto";
                    cbAutoStall.CheckedChanged += (s, e) => {
                        if (Stall_cbxAutoOpenPlayerStall != null) Stall_cbxAutoOpenPlayerStall.Checked = cbAutoStall.Checked;
                    };
                }
                cbAutoStall.Location = new Point(15, 15);
                cbAutoStall.Visible = true;

                NumericUpDown nudCount = p.Controls["PhBot_StallOptCount"] as NumericUpDown;
                if (nudCount == null)
                {
                    nudCount = new NumericUpDown();
                    nudCount.Name = "PhBot_StallOptCount";
                    nudCount.Font = PhBotFont();
                    nudCount.Location = new Point(275, 13);
                    nudCount.Size = new Size(50, 22);
                    nudCount.Maximum = 100;
                    nudCount.Value = Stall_nudMinimumPlayerStallItems != null ? Stall_nudMinimumPlayerStallItems.Value : 0;
                    nudCount.ValueChanged += (s, e) => {
                        if (Stall_nudMinimumPlayerStallItems != null) Stall_nudMinimumPlayerStallItems.Value = nudCount.Value;
                    };
                    p.Controls.Add(nudCount);
                }
                nudCount.Location = new Point(275, 13);
                nudCount.Visible = true;

                Label lblCanStall = p.Controls["PhBot_StallOptCanStall"] as Label;
                if (lblCanStall == null)
                {
                    lblCanStall = new Label { Name = "PhBot_StallOptCanStall", Text = "items can be stalled", Font = PhBotFont(), AutoSize = true, Location = new Point(330, 15) };
                    p.Controls.Add(lblCanStall);
                }
                lblCanStall.Location = new Point(330, 15);
                lblCanStall.Visible = true;

                CheckBox cbTown = p.Controls["PhBot_StallOptTown"] as CheckBox;
                if (cbTown == null)
                {
                    cbTown = AddPhBotCheck(p, "Execute town script before opening the stall", 15, 42, false);
                    cbTown.Name = "PhBot_StallOptTown";
                }
                cbTown.Location = new Point(15, 42);
                cbTown.Visible = true;

                CheckBox cbRefill = p.Controls["PhBot_StallOptRefill"] as CheckBox;
                if (cbRefill == null)
                {
                    cbRefill = AddPhBotCheck(p, "Do not refill stall until it is empty", 15, 69, false);
                    cbRefill.Name = "PhBot_StallOptRefill";
                }
                cbRefill.Location = new Point(15, 69);
                cbRefill.Visible = true;

                Label lblT = p.Controls["PhBot_StallOptTitleLbl"] as Label;
                if (lblT == null)
                {
                    lblT = new Label { Name = "PhBot_StallOptTitleLbl", Text = "Title", Font = PhBotFont(), AutoSize = true, Location = new Point(15, 105) };
                    p.Controls.Add(lblT);
                }
                lblT.Location = new Point(15, 105);
                lblT.Visible = true;

                if (Stall_tbxStallTitle != null)
                {
                    Stall_tbxStallTitle.Location = new Point(100, 102);
                    Stall_tbxStallTitle.Size = new Size(250, 22);
                    Stall_tbxStallTitle.Font = PhBotFont();
                    Stall_tbxStallTitle.Visible = true;
                }

                Label lblG = p.Controls["PhBot_StallOptGreetLbl"] as Label;
                if (lblG == null)
                {
                    lblG = new Label { Name = "PhBot_StallOptGreetLbl", Text = "Greeting", Font = PhBotFont(), AutoSize = true, Location = new Point(15, 135) };
                    p.Controls.Add(lblG);
                }
                lblG.Location = new Point(15, 135);
                lblG.Visible = true;

                if (Stall_tbxStallNote != null)
                {
                    Stall_tbxStallNote.Location = new Point(100, 132);
                    Stall_tbxStallNote.Size = new Size(380, 22);
                    Stall_tbxStallNote.Font = PhBotFont();
                    if (string.IsNullOrEmpty(Stall_tbxStallNote.Text) || Stall_tbxStallNote.Text.Contains("xBot"))
                        Stall_tbxStallNote.Text = "Welcome!";
                    Stall_tbxStallNote.Visible = true;
                }

                Label lblChat = p.Controls["PhBot_StallOptChatLbl"] as Label;
                if (lblChat == null)
                {
                    lblChat = new Label { Name = "PhBot_StallOptChatLbl", Text = "Chat message", Font = PhBotFont(), AutoSize = true, Location = new Point(15, 165) };
                    p.Controls.Add(lblChat);
                }
                lblChat.Location = new Point(15, 165);
                lblChat.Visible = true;

                TextBox txtChat = p.Controls["PhBot_StallOptChatTxt"] as TextBox;
                if (txtChat == null)
                {
                    txtChat = new TextBox { Name = "PhBot_StallOptChatTxt", Font = PhBotFont(), Location = new Point(100, 162), Size = new Size(380, 22) };
                    p.Controls.Add(txtChat);
                }
                txtChat.Location = new Point(100, 162);
                txtChat.Visible = true;

                Label lblNote = p.Controls["PhBot_StallOptTownNote"] as Label;
                if (lblNote == null)
                {
                    lblNote = new Label { Name = "PhBot_StallOptTownNote", Text = "* Automatic stalling occurs when the bot is started in a town", Font = PhBotFont(), AutoSize = true, Location = new Point(15, 275), ForeColor = Color.FromArgb(60, 60, 60) };
                    p.Controls.Add(lblNote);
                }
                lblNote.Location = new Point(15, 275);
                lblNote.Visible = true;
            }
            catch (Exception ex) { PhBotDebug("stallopt inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void LayoutStallInventoryInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Stall_Panel;
                Panel pg = FindHTabPanel(host, "TabPageH_Stall_Option", "Inventory");
                if (pg == null) return;
                pg.AutoScroll = false;
                pg.BackColor = Color.White;
                int sW = pg.Width, sH = pg.Height;
                if (sW < 300 || sH < 200) return;

                ListView lv = pg.Controls["PhBot_StallInvList"] as ListView;
                if (lv == null)
                {
                    lv = NewPhBotListView(15, 10, Math.Max(300, sW - 65), Math.Max(180, sH - 20), "Slot|50", "Icon|50", "Name|220", "Quantity|65", "Stall|55", "Consignment|85", "Price|80");
                    lv.Name = "PhBot_StallInvList";
                    lv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                    pg.Controls.Add(lv);

                    var btnRefresh = new Button();
                    btnRefresh.Name = "PhBot_StallInvRefresh";
                    btnRefresh.Text = "R\nE\nF\nR\nE\nS\nH";
                    btnRefresh.Font = new Font("Segoe UI", 8f, FontStyle.Regular);
                    btnRefresh.Size = new Size(28, Math.Max(150, sH - 20));
                    btnRefresh.Location = new Point(sW - 40, 10);
                    btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                    Classicize(btnRefresh);
                    pg.Controls.Add(btnRefresh);
                }
                else
                {
                    lv.Location = new Point(15, 10);
                    lv.Size = new Size(Math.Max(300, sW - 65), Math.Max(180, sH - 20));
                    lv.Visible = true;
                }
            }
            catch (Exception ex) { PhBotDebug("stallinv inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void LayoutStallFilterInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel host = TabPageV_Control01_Stall_Panel;
                Panel pg = FindHTabPanel(host, "TabPageH_Stall_Option", "Filter");
                if (pg == null) return;
                pg.AutoScroll = false;
                pg.BackColor = Color.White;
                int fW = pg.Width, fH = pg.Height;
                if (fW < 300 || fH < 200) return;

                ListView lv = pg.Controls["PhBot_StallFilterList"] as ListView;
                if (lv == null)
                {
                    lv = NewPhBotListView(15, 10, Math.Max(300, fW - 30), Math.Max(150, fH - 55), "ID|50", "Icon|50", "Name|200", "Quantity|65", "Stall|55", "Stall Price|75", "Consignment|85", "Consignment Price|110");
                    lv.Name = "PhBot_StallFilterList";
                    lv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                    pg.Controls.Add(lv);

                    var cbWear = new ComboBox { Name = "PhBot_StallFilterWear", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont(), Location = new Point(15, fH - 35), Size = new Size(95, 22), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                    cbWear.Items.AddRange(new object[] { "Wearable", "Consumable", "General" });
                    cbWear.SelectedIndex = 0;
                    pg.Controls.Add(cbWear);

                    var cbAll = new ComboBox { Name = "PhBot_StallFilterAll", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont(), Location = new Point(115, fH - 35), Size = new Size(75, 22), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                    cbAll.Items.AddRange(new object[] { "All", "CH", "EU" });
                    cbAll.SelectedIndex = 0;
                    pg.Controls.Add(cbAll);

                    var cbAny = new ComboBox { Name = "PhBot_StallFilterAny", DropDownStyle = ComboBoxStyle.DropDownList, Font = PhBotFont(), Location = new Point(195, fH - 35), Size = new Size(75, 22), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                    cbAny.Items.AddRange(new object[] { "Any", "Weapon", "Armor" });
                    cbAny.SelectedIndex = 0;
                    pg.Controls.Add(cbAny);

                    var txtSearch = new TextBox { Name = "PhBot_StallFilterTxt", Font = PhBotFont(), Location = new Point(275, fH - 35), Size = new Size(140, 22), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                    pg.Controls.Add(txtSearch);

                    var btnSearch = new Button { Name = "PhBot_StallFilterSearch", Text = "Search", Font = PhBotFont(), Location = new Point(420, fH - 37), Size = new Size(65, 25), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                    Classicize(btnSearch);
                    pg.Controls.Add(btnSearch);

                    var btnClear = new Button { Name = "PhBot_StallFilterClear", Text = "Clear", Font = PhBotFont(), Location = new Point(490, fH - 37), Size = new Size(60, 25), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                    Classicize(btnClear);
                    pg.Controls.Add(btnClear);

                    var btnReset = new Button { Name = "PhBot_StallFilterReset", Text = "Reset", Font = PhBotFont(), Location = new Point(555, fH - 37), Size = new Size(60, 25), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                    Classicize(btnReset);
                    pg.Controls.Add(btnReset);
                }
            }
            catch (Exception ex) { PhBotDebug("stallfilter inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }
    }
}
