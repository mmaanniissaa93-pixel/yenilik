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
        // PLAYERS — docs/phbot_ref/guide/phbot_players_01.png
        // Sol: Players (0) + liste + alt butonlar (Refresh, Clear, 4 Invite)
        // Sağ: Trace grubu (Player to trace + butonlar + seçenekler)
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

                int listW = Math.Max(300, W - 320);
                try
                {
                    if (Players_tvwPlayers != null)
                    {
                        Players_tvwPlayers.Location = new Point(8, 28);
                        Players_tvwPlayers.Size = new Size(listW, Math.Max(150, H - 70));
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

                // Alt butonlar (phBot players 2..7 butonları)
                int by = H - 36;
                Button bRefresh = EnsurePlayerButton(p, "PhBot_PlayersRefresh", "Refresh", 8, by, 75);
                if (bRefresh != null && Players_btnRefreshPlayers != null)
                {
                    try { bRefresh.Click -= PlayersRefreshClick; } catch { }
                    bRefresh.Click += PlayersRefreshClick;
                }
                Button bClear = EnsurePlayerButton(p, "PhBot_PlayersClear", "Clear", 88, by, 75);
                if (bClear != null)
                {
                    try { bClear.Click -= PlayersClearClick; } catch { }
                    bClear.Click += PlayersClearClick;
                }
                Button bParty = EnsurePlayerButton(p, "PhBot_PlayersPartyInvite", "Party Invite", 168, by, 95);
                if (bParty != null)
                {
                    try { bParty.Click -= PlayersPartyInviteClick; } catch { }
                    bParty.Click += PlayersPartyInviteClick;
                }
                Button bAcad = EnsurePlayerButton(p, "PhBot_PlayersAcadInvite", "Academy Invite", 268, by, 105);
                if (bAcad != null)
                {
                    try { bAcad.Click -= PlayersAcadInviteClick; } catch { }
                    bAcad.Click += PlayersAcadInviteClick;
                }
                Button bGuild = EnsurePlayerButton(p, "PhBot_PlayersGuildInvite", "Guild Invite", 378, by, 95);
                if (bGuild != null)
                {
                    try { bGuild.Click -= PlayersGuildInviteClick; } catch { }
                    bGuild.Click += PlayersGuildInviteClick;
                }
                EnsurePlayerButton(p, "PhBot_PlayersUnionInvite", "Union Party Invite", 478, by, 120);

                // Sağ sütun: Trace bölümü (phBot players 8..17)
                int x = W - 300;
                PhBotLabel(p, "PhBot_TraceGroupTitle", "Trace", x, 8);
                PhBotLabel(p, "PhBot_TraceLbl", "Player to trace", x, 32);
                TextBox tbx = p.Controls["PhBot_TraceName"] as TextBox;
                if (tbx == null)
                {
                    tbx = new TextBox();
                    tbx.Name = "PhBot_TraceName";
                    tbx.Font = PhBotFont();
                    tbx.BackColor = Color.White;
                    try { p.Controls.Add(tbx); } catch { }
                }
                try { tbx.Location = new Point(x + 105, 28); tbx.Size = new Size(175, 24); tbx.Visible = true; } catch { }

                Button bTRefresh = EnsurePlayerButton(p, "PhBot_TraceRefresh", "Refresh", x, 60, 80);
                if (bTRefresh != null && Players_btnRefreshPlayers != null)
                {
                    try { bTRefresh.Click -= PlayersRefreshClick; } catch { }
                    bTRefresh.Click += PlayersRefreshClick;
                }
                PhBotLabel(p, "PhBot_TraceXLbl", "Trace X: 0", x + 95, 64);

                Button bTStart = EnsurePlayerButton(p, "PhBot_TraceStart", "Start", x, 92, 80);
                if (bTStart != null)
                {
                    try { bTStart.Click -= TraceStartClick; } catch { }
                    bTStart.Click += TraceStartClick;
                }
                PhBotLabel(p, "PhBot_TraceYLbl", "Trace Y: 0", x + 95, 96);

                Button bTStop = EnsurePlayerButton(p, "PhBot_TraceStop", "Stop", x, 124, 80);
                if (bTStop != null)
                {
                    try { bTStop.Click -= TraceStopClick; } catch { }
                    bTStop.Click += TraceStopClick;
                }

                int ty = 160;
                PhBotTodoCheck(p, "PhBot_TraceAttack", "Attack monsters (uses training radius)", x, ty, false); ty += 26;
                PhBotTodoCheck(p, "PhBot_TraceClear", "Kill all monsters before tracing", x, ty, false); ty += 26;
                PhBotTodoCheck(p, "PhBot_TraceBuffs", "Buffs", x, ty, false); ty += 26;
                PhBotTodoCheck(p, "PhBot_TracePartyBuffs", "Party buffs", x, ty, false); ty += 26;
                PhBotTodoCheck(p, "PhBot_TracePick", "Pick items", x, ty, false); ty += 26;
                PhBotTodoCheck(p, "PhBot_TraceGame", "Game trace", x, ty, false);
            }
            catch (Exception ex) { PhBotDebug("players inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private Button EnsurePlayerButton(Panel p, string name, string text, int x, int y, int w = 170, int h = 28)
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
                        if (Guild_lstvInfo.Columns.Count >= 4)
                        {
                            Guild_lstvInfo.Columns[0].Width = 220;
                            Guild_lstvInfo.Columns[1].Width = 70;
                            Guild_lstvInfo.Columns[2].Width = 140;
                            Guild_lstvInfo.Columns[3].Width = 120;
                        }
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
                gbxInvite.SetBounds(8, by, 480, 115);
                gbxInvite.Visible = true;

                PhBotTodoCheck(gbxInvite, "PhBot_GuildShowInv", "Show guild invites", 14, 22, true);
                PhBotTodoCheck(gbxInvite, "PhBot_GuildAutoInv", "Auto invite other players to the guild with level >", 14, 50, false);
                PhBotTodoNumber(gbxInvite, "PhBot_GuildAutoInvN", 350, 48, 45, "0");
                PhBotTodoCheck(gbxInvite, "PhBot_GuildAcceptAll", "Accept all guild invites", 14, 78, false);

                // Alt sağ: View Notice butonu
                Button bNotice = EnsurePlayerButton(p, "PhBot_GuildNoticeView", "View Notice", Math.Max(500, W - 140), by + 30, 120, 28);
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
                        try { pg0.BackColor = Color.White; pg0.AutoScroll = true; } catch { }
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
                    // Boyutlandırma shell'e aittir (LayoutXBotReference); burada
                    // yalnızca içerik kurulur.
                    tabs.Visible = true;
                }
                catch { }
                TabPage main = tabs.TabPages[0];
                if (main.Controls["PhBot_AcMembers"] == null)
                {
                    ListView lv = NewPhBotListView(10, 10, 560, 320, "Member|300", "Level|100");
                    lv.Name = "PhBot_AcMembers";
                    main.Controls.Add(lv);
                    Button bNotice = PhBotButton(main, "PhBot_AcNoticeView", "Notice View", 10, 340, 130);
                    if (bNotice != null)
                    {
                        try { bNotice.Click -= AcademyNoticeClick; } catch { }
                        bNotice.Click += AcademyNoticeClick;
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
                tp.BackColor = PhBotBg;
                int y = 10;
                PhBotTodoCheck(tp, "PhBot_AcAcceptAll", "Accept all academy invites", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcAcceptList", "Accept academy invites from the list", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcInviteAll", "Invite all players", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcInviteList", "Invite only from the list", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcShowInv", "Show academy invites", 10, y, true); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcAcceptJoin", "Accept join invitations", 10, y, true); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcInvitePlayers", "Invite players to the academy", 10, y, true); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcAutoGrad", "Auto graduate from the academy", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcRefuse", "Refuse invites from players not in the list", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcGradLevelL", "Graduate level:", 10, y, false);
                PhBotTodoNumber(tp, "PhBot_AcGradLevel", 230, y - 2, 60, "40"); y += 30;
                PhBotLabel(tp, "PhBot_AcPassLbl", "Password:", 10, y + 4);
                PhBotTodoNumber(tp, "PhBot_AcPass", 230, y, 160, ""); y += 32;
                ListView lv = NewPhBotListView(10, y, 460, 180, "Accept / Invite List|440");
                lv.Name = "PhBot_AcList";
                tp.Controls.Add(lv);
                // TODO backend: academy motoru davet paketinden öteye gitmiyor.
            }
            catch (Exception ex) { PhBotDebug("acadopt: " + ex.Message); }
        }

        private void BuildAcademyMatching(TabPage tp)
        {
            if (tp == null || tp.Controls.Count > 0) return;
            try
            {
                tp.BackColor = PhBotBg;
                int y = 10;
                PhBotLabel(tp, "PhBot_AcTitleLbl", "Title:", 10, y + 4);
                PhBotTodoNumber(tp, "PhBot_AcTitle", 130, y, 260, ""); y += 32;
                PhBotTodoCheck(tp, "PhBot_AcAutoForm", "Auto form academy matching", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcMatchType", "Matching type:", 10, y, false); y += 30;
                PhBotButton(tp, "PhBot_AcViewMatch", "View Matching", 10, y, 150); y += 38;
                ListView lv = NewPhBotListView(10, y, 500, 180, "Academy|300", "Members|180");
                lv.Name = "PhBot_AcMatchList";
                tp.Controls.Add(lv);
                y += 190;
                PhBotLabel(tp, "PhBot_AcJoinLbl", "Join academy #", 10, y + 4);
                PhBotTodoNumber(tp, "PhBot_AcJoinN", 130, y, 80, ""); y += 32;
                PhBotTodoCheck(tp, "PhBot_AcJoinByName", "Auto join matching by player name", 10, y, false); y += 30;
                PhBotTodoCheck(tp, "PhBot_AcJoinByTitle", "Auto join matching by title", 10, y, false);
            }
            catch (Exception ex) { PhBotDebug("acmatch: " + ex.Message); }
        }

        // ---------------------------------------------------------------
        // PET — Attack Pet 12 maddesi birebir (diğerleri bilgi).
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
                if (atk.Controls["PhBot_PetUse"] == null)
                {
                    try { atk.Controls.Clear(); } catch { }
                    atk.BackColor = Color.White;
                    int y = 10;
                    PhBotTodoCheck(atk, "PhBot_PetUse", "Use attack pet", 10, y, true); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetSummon", "Auto summon attack pet", 10, y, true); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetRevive", "Auto revive attack pet", 10, y, true);
                    PhBotTodoNumber(atk, "PhBot_PetReviveN", 300, y - 2, 60, "0"); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetReturn", "Return to town when the attack pet dies", 10, y, false); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetSP", "Use fellow pet SP recall in town", 10, y, false); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetProtect", "Protect the attack pet", 10, y, false); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetPassive", "Don't attack monsters", 10, y, false); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetTownOnly", "Only summon the attack pet in town", 10, y, false); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetNoRevive", "Only return when there aren't any revive items", 10, y, false); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetAreaOnly", "Only summon the attack pet at the training area", 10, y, false); y += 30;
                    PhBotTodoCheck(atk, "PhBot_PetHPPotions", "Only revive if HP potions are present", 10, y, false); y += 30;
                    Button bUn = PhBotButton(atk, "PhBot_PetUnsummon", "Unsummon", 10, y, 120);
                    if (bUn != null)
                    {
                        try { bUn.Click -= PetUnsummonClick; } catch { }
                        bUn.Click += PetUnsummonClick;
                    }
                    // TODO backend: pet politikaları (docs/PHBOT_GAP_ANALYSIS.md §10).
                }
                foreach (TabPage pg in new TabPage[] { tabs.TabPages[1], tabs.TabPages[2] })
                {
                    if (pg.Controls["PhBot_PetInfoDone"] == null)
                    {
                        try { pg.Controls.Clear(); } catch { }
                        pg.BackColor = Color.White;
                        ListView lv = NewPhBotListView(10, 10, 480, 260, "Item|300", "Value|170");
                        pg.Controls.Add(lv);
                        PhBotButton(pg, "PhBot_PetUn_" + pg.Text, "Unsummon", 10, 280, 120);
                        Label dd = new Label();
                        dd.Name = "PhBot_PetInfoDone";
                        dd.Location = new Point(10, 312);
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
                // TODO backend: unsummon paketi.
                Log("Pet unsummon istendi (TODO backend).");
            }
            catch { }
        }

        // ---------------------------------------------------------------
        // UNION PARTY — 3 liste + seçenekler birebir.
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
                int W = host.Width, H = host.Height;
                if (W < 200 || H < 100) return;
                try { host.AutoScroll = true; } catch { }

                // Kurulum (bir kez).
                int lw = Math.Max(150, (W - 300) / 3);
                int rx = 24 + lw * 3;
                if (host.Controls["PhBot_Union1"] == null)
                {
                    ListView l1 = NewPhBotListView(8, 8, lw, H - 16, "Union Party #1|150");
                    l1.Name = "PhBot_Union1";
                    ListView l2 = NewPhBotListView(16 + lw, 8, lw, H - 16, "Union Party #2|150");
                    l2.Name = "PhBot_Union2";
                    ListView l3 = NewPhBotListView(24 + lw * 2, 8, lw, H - 16, "Union Party #3|150");
                    l3.Name = "PhBot_Union3";
                    host.Controls.Add(l1);
                    host.Controls.Add(l2);
                    host.Controls.Add(l3);
                    ComboBox cb = new ComboBox();
                    cb.Name = "PhBot_UnionType";
                    cb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cb.Items.Add("Exp Share");
                    cb.Items.Add("Item Share");
                    cb.SelectedIndex = 0;
                    try { host.Controls.Add(cb); } catch { }
                    ListView li = NewPhBotListView(0, 0, 200, 150, "Invite list|190");
                    li.Name = "PhBot_UnionInviteList";
                    host.Controls.Add(li);
                    ListView la = NewPhBotListView(0, 0, 200, 150, "Accept list|190");
                    la.Name = "PhBot_UnionAcceptList";
                    host.Controls.Add(la);
                    // TODO backend: union motoru yok (docs/PHBOT_GAP_ANALYSIS.md §11).
                }
                // Yerleşim (her zaman).
                try
                {
                    ListView l1 = host.Controls["PhBot_Union1"] as ListView;
                    ListView l2 = host.Controls["PhBot_Union2"] as ListView;
                    ListView l3 = host.Controls["PhBot_Union3"] as ListView;
                    if (l1 != null) { l1.Location = new Point(8, 8); l1.Size = new Size(lw, H - 16); l1.Visible = true; }
                    if (l2 != null) { l2.Location = new Point(16 + lw, 8); l2.Size = new Size(lw, H - 16); l2.Visible = true; }
                    if (l3 != null) { l3.Location = new Point(24 + lw * 2, 8); l3.Size = new Size(lw, H - 16); l3.Visible = true; }
                }
                catch { }
                int x = rx, y = 8;
                PhBotTodoCheck(host, "PhBot_UnionAccept", "Accept union party invites", x, y, false); y += 30;
                PhBotTodoCheck(host, "PhBot_UnionInvite", "Invite players to the union party", x, y, false); y += 30;
                PhBotLabel(host, "PhBot_UnionTypeLbl", "Union Party Type:", x, y + 4);
                ComboBox cbx = host.Controls["PhBot_UnionType"] as ComboBox;
                try { GrayCombo(cbx); cbx.Location = new Point(x, y + 26); cbx.Size = new Size(180, 24); cbx.Visible = true; } catch { }
                y += 60;
                try
                {
                    ListView li2 = host.Controls["PhBot_UnionInviteList"] as ListView;
                    if (li2 != null) { li2.Location = new Point(x, y); li2.Size = new Size(200, 150); li2.Visible = true; }
                    y += 160;
                    ListView la2 = host.Controls["PhBot_UnionAcceptList"] as ListView;
                    if (la2 != null) { la2.Location = new Point(x, y); la2.Size = new Size(200, 150); la2.Visible = true; }
                }
                catch { }
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
                int W = p.Width;
                if (W < 200) return;
                int y = 8;
                try
                {
                    if (Stall_lblStallTitle != null) { Stall_lblStallTitle.Location = new Point(8, y + 4); Stall_lblStallTitle.Visible = true; }
                    if (Stall_tbxStallTitle != null) { Stall_tbxStallTitle.Location = new Point(130, y); Stall_tbxStallTitle.Size = new Size(300, 24); Stall_tbxStallTitle.Visible = true; }
                }
                catch { }
                y += 32;
                try
                {
                    if (Stall_lblStallNote != null) { Stall_lblStallNote.Location = new Point(8, y + 4); Stall_lblStallNote.Visible = true; }
                    if (Stall_tbxStallNote != null) { Stall_tbxStallNote.Location = new Point(130, y); Stall_tbxStallNote.Size = new Size(300, 24); Stall_tbxStallNote.Visible = true; }
                }
                catch { }
                y += 32;
                try
                {
                    if (Stall_cbxAutoOpenPlayerStall != null) { Stall_cbxAutoOpenPlayerStall.Location = new Point(8, y); Stall_cbxAutoOpenPlayerStall.Visible = true; }
                }
                catch { }
                y += 30;
                try
                {
                    if (Stall_lblMinimumPlayerStallItems != null) { Stall_lblMinimumPlayerStallItems.Location = new Point(8, y + 4); Stall_lblMinimumPlayerStallItems.Visible = true; }
                    if (Stall_nudMinimumPlayerStallItems != null) { Stall_nudMinimumPlayerStallItems.Location = new Point(230, y); Stall_nudMinimumPlayerStallItems.Visible = true; }
                }
                catch { }
                y += 32;
                PhBotTodoCheck(p, "PhBot_StallTownLoop", "Stall before/after the town loop", 8, y, false);
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
                int sW = pg.Width, sH = pg.Height;
                if (sW < 300 || sH < 200) return;
                if (pg.Controls["PhBot_StallInvList"] == null)
                {
                    ListView lv = NewPhBotListView(8, 8, Math.Max(300, sW - 16), Math.Max(200, sH - 60), "Item|350", "Price|150");
                    lv.Name = "PhBot_StallInvList";
                    pg.Controls.Add(lv);
                    Label n = new Label();
                    n.Name = "PhBot_StallInvNote";
                    n.Font = PhBotFont();
                    n.ForeColor = Color.FromArgb(60, 60, 60);
                    n.AutoSize = true;
                    n.Text = "Items are de-duplicated and prices are saved; stall a specific item like armor or weapons here.";
                    pg.Controls.Add(n);
                    // TODO backend: envanter fiyat modeli.
                }
                try
                {
                    ListView lv2 = pg.Controls["PhBot_StallInvList"] as ListView;
                    if (lv2 != null) { lv2.Location = new Point(8, 8); lv2.Size = new Size(sW - 16, sH - 60); lv2.Visible = true; }
                    Label n2 = pg.Controls["PhBot_StallInvNote"] as Label;
                    if (n2 != null) { n2.Location = new Point(8, sH - 40); n2.Visible = true; }
                }
                catch { }
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
                int fW = pg.Width, fH = pg.Height;
                if (fW < 300 || fH < 200) return;
                if (pg.Controls["PhBot_StallFilterList"] == null)
                {
                    ListView lv = NewPhBotListView(8, 8, Math.Max(300, fW - 16), Math.Max(200, fH - 100), "Item|350", "Quantity|100");
                    lv.Name = "PhBot_StallFilterList";
                    pg.Controls.Add(lv);
                    Label n = new Label();
                    n.Name = "PhBot_StallFilterNote";
                    n.Font = PhBotFont();
                    n.ForeColor = Color.FromArgb(60, 60, 60);
                    n.AutoSize = true;
                    n.Text = "Sell generic items like elixirs here; the bot combines then splits stacks automatically.";
                    pg.Controls.Add(n);
                    // TODO backend: generic stall filtresi.
                }
                try
                {
                    ListView lv2 = pg.Controls["PhBot_StallFilterList"] as ListView;
                    if (lv2 != null) { lv2.Location = new Point(8, 8); lv2.Size = new Size(fW - 16, fH - 100); lv2.Visible = true; }
                    PhBotLabel(pg, "PhBot_StallFilterQLbl", "Quantity:", 8, fH - 80);
                    PhBotTodoNumber(pg, "PhBot_StallFilterQ", 130, fH - 82, 80, "1");
                    Label n2 = pg.Controls["PhBot_StallFilterNote"] as Label;
                    if (n2 != null) { n2.Location = new Point(8, fH - 46); n2.Visible = true; }
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("stallfilter inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }
    }
}
