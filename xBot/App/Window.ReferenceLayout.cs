using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace xBot.App
{
    public partial class Window
    {
        // The reference supplies the visual structure. The controls, settings and
        // event handlers belong to xBot and retain their original object identity.
        private readonly Dictionary<Control, TabPage> _referencePages = new Dictionary<Control, TabPage>();
        private readonly List<Panel> _referenceViews = new List<Panel>();
        private FlowLayoutPanel _referenceTools;
        private bool _referenceLayingOut;
        private readonly Font _referenceFont = new Font("Segoe UI", 9F);

        private void ApplyXBotReferenceLayout()
        {
            if (_phBotClassicApplied) return;
            SuspendLayout();
            try
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                MaximizeBox = MinimizeBox = ControlBox = true;
                MinimumSize = new Size(1100, 700);
                var work = Screen.FromControl(this).WorkingArea;
                ClientSize = new Size(Math.Min(1440, work.Width - 40), Math.Min(880, work.Height - 80));
                BackColor = PhBotBg;
                ForeColor = Color.Black;
                pnlWindow.Dock = DockStyle.Fill;
                pnlWindow.SizeChanged += (s, e) => LayoutXBotReference();
                pnlWindow.BorderStyle = BorderStyle.None;
                pnlHeader.Visible = false;
                btnBotStart.Visible = btnClientOptions.Visible = btnAnalyzer.Visible = false;
                BuildAlchemyTab();
                BuildTargetAssistTab();
                pnlWindow.Controls.Add(TabPageV_Control01_Alchemy_Panel);
                pnlWindow.Controls.Add(TabPageV_Control01_TargetAssist_Panel);
                BuildXBotReferenceSidebar();
                foreach (Panel view in _referenceViews)
                {
                    foreach (Panel strip in view.Controls.OfType<Panel>().ToArray())
                    {
                        if (strip.Controls.OfType<Button>().Any(b => b.Name.StartsWith("TabPageH_")))
                            WrapXBotReferenceTabs(view, strip);
                    }
                }
                EnsurePhBotBottomArea();
                BuildXBotReferenceTools();
                StyleXBotReference(pnlWindow);
                PrepareXBotReferenceContent();
                _phBotClassicApplied = true;
                LayoutXBotReference();
                SizeChanged += (s, e) => LayoutXBotReference();
                LocalizationManager.OnLanguageChanged += RefreshReferenceLanguage;
                Disposed += (s, e) => {
                    LocalizationManager.OnLanguageChanged -= RefreshReferenceLanguage;
                    _referenceFont.Dispose();
                };
                Text = ProductName + " v" + ProductVersion;
                TabPageV_Option_Click(TabPageV_Control01.Controls["TabPageV_Control01_Login"], EventArgs.Empty);
            }
            finally { ResumeLayout(true); LayoutXBotReference(); }
        }

        private void BuildXBotReferenceSidebar()
        {
            var titles = new Dictionary<string, string> {
                { "GameInfo", "Statistics" }, { "Login", "Silkroad Login" },
                { "Character", "Protection" }, { "Skills", "Attack" },
                { "Training", "Training Area" }, { "Minimap", "Map" }, { "Town", "Town" }
            };
            var oldButtons = TabPageV_Control01.Controls.OfType<Button>().ToDictionary(b => b.Name);
            var oldIcons = TabPageV_Control01.Controls.OfType<Label>().ToDictionary(b => b.Name);
            if (tabPickFilterRoot != null)
            {
                var pick = new Panel { Name = "TabPageV_Control01_PickFilter_Panel", Visible = false };
                pick.Controls.Add(tabPickFilterRoot);
                tabPickFilterRoot.Dock = DockStyle.Fill;
                pnlWindow.Controls.Add(pick);
            }
            string[] order = { "GameInfo", "Login", "Character", "Town", "Training", "Skills", "Party",
                "PickFilter", "Quest", "Players", "Guild", "Academy", "Inventory", "Stall", "Trade",
                "Alchemy", "TargetAssist", "Chat", "Minimap", "Settings" };
            var views = pnlWindow.Controls.OfType<Panel>()
                .Where(p => p.Name.StartsWith("TabPageV_Control01_") && p.Name.EndsWith("_Panel")).ToList();
            views.Sort((a, b) => {
                int ai = Array.IndexOf(order, a.Name.Replace("TabPageV_Control01_", "").Replace("_Panel", ""));
                int bi = Array.IndexOf(order, b.Name.Replace("TabPageV_Control01_", "").Replace("_Panel", ""));
                return (ai < 0 ? 100 : ai).CompareTo(bi < 0 ? 100 : bi);
            });
            TabPageV_Control01.Controls.Clear();
            TabPageV_Control01.Tag = null;
            TabPageV_Control01.AutoScroll = true;
            TabPageV_Control01.BorderStyle = BorderStyle.FixedSingle;
            TabPageV_Control01.BackColor = Color.White;
            TabPageV_ColorSelected = PhBotSelect;
            TabPageV_ColorHover = PhBotHover;
            var brand = new Label { Name = "XBotBrand", Text = ProductName, AutoSize = false,
                Location = new Point(10, 6), Size = new Size(180, 30), TextAlign = ContentAlignment.MiddleLeft };
            TabPageV_Control01.Controls.Add(brand);
            int y = 42;
            foreach (Panel view in views)
            {
                string key = view.Name.Replace("TabPageV_Control01_", "").Replace("_Panel", "");
                string name = view.Name.Substring(0, view.Name.Length - 6);
                string title;
                if (!titles.TryGetValue(key, out title))
                    title = key == "PickFilter" ? "Pick Filter" : key == "TargetAssist" ? "Target Assist" : key;
                Button button;
                bool existingButton = oldButtons.TryGetValue(name, out button);
                if (!existingButton) button = new Button();
                button.Name = name; button.Text = title;
                button.SetBounds(30, y, 165, 27);
                button.TextAlign = ContentAlignment.MiddleLeft;
                button.FlatStyle = FlatStyle.Flat; button.BackColor = Color.White;
                button.TabIndex = _referenceViews.Count;
                button.Visible = true;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = PhBotHover;
                button.FlatAppearance.MouseDownBackColor = PhBotSelect;
                Label icon;
                bool existingIcon = oldIcons.TryGetValue(name + "_Icon", out icon);
                if (!existingIcon) icon = new Label();
                icon.Name = name + "_Icon"; icon.Text = "";
                icon.Image = PhBotIcons.Get(title);
                icon.SetBounds(4, y, 26, 27);
                icon.BackColor = Color.White; icon.Visible = true;
                foreach (Control item in new Control[] { button, icon })
                {
                    if ((item == button && !existingButton) || (item == icon && !existingIcon))
                    {
                        item.Click += TabPageV_Option_Click;
                        item.MouseEnter += TabPageV_Option_MouseEnter;
                        item.MouseLeave += TabPageV_Option_MouseLeave;
                    }
                    TabPageV_Control01.Controls.Add(item);
                }
                view.Visible = false;
                view.BorderStyle = BorderStyle.None;
                _referenceViews.Add(view);
                y += 27;
            }
        }

        private void WrapXBotReferenceTabs(Panel view, Control strip)
        {
            if (strip == null || strip.Tag is TabControl) return;
            var buttons = strip.Controls.OfType<Button>().Where(b => b.Name.StartsWith("TabPageH_"))
                .OrderBy(b => b.Left).ToList();
            var tabs = new TabControl { Name = strip.Name + "_Tabs", Font = _referenceFont,
                Padding = new Point(12, 5), Multiline = false };
            var selected = strip.Tag as Control;
            foreach (Button button in buttons)
            {
                var panel = view.Controls[button.Name + "_Panel"] as Panel;
                if (panel == null) continue;
                // Pick Filter now has its own navigation entry.
                if (button == TabPageH_Town_Option03) { panel.Visible = false; continue; }
                var page = new TabPage(button.Text) { Tag = button, BackColor = Color.White, Padding = new Padding(8) };
                tabs.TabPages.Add(page);
                _referencePages.Add(button, page);
                panel.Dock = DockStyle.Fill;
                panel.BorderStyle = BorderStyle.None;
                panel.AutoScroll = true;
                page.Controls.Add(panel);
                panel.Visible = true;
                if (button == selected) tabs.SelectedTab = page;
            }
            if (tabs.TabCount == 0) { tabs.Dispose(); return; }
            strip.Tag = tabs;
            strip.Visible = false;
            view.Controls.Add(tabs);
            tabs.BringToFront();
            // Real TabPages own their contents, so both mouse and keyboard tab
            // changes show exactly one page, even while the old strip is hidden.
            tabs.SelectedIndexChanged += (s, e) => {
                if (view == TabPageV_Control01_Skills_Panel) LayoutXBotSkillPages();
            };
        }

        private void BuildXBotReferenceTools()
        {
            _referenceTools = new FlowLayoutPanel { Name = "XBotTools", WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, AutoScroll = false };
            pnlWindow.Controls.Add(_referenceTools);
            var tools = new[] { btnLangTR, btnLangEN, btnQuickSave, btnCommandCenter, btnAnalyzer };
            string[] labels = { "TR", "EN", "Save", "Command Center", "Analyzer" };
            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] == null) continue;
                _referenceTools.Controls.Add(tools[i]);
                tools[i].Text = labels[i];
                tools[i].AutoSize = true;
                tools[i].Size = new Size(i < 2 ? 34 : 75, 26);
                tools[i].Margin = new Padding(0, 0, 6, 0);
                tools[i].Visible = true;
            }
        }

        private void RefreshReferenceLanguage()
        {
            foreach (var entry in _referencePages) entry.Value.Text = entry.Key.Text;
            StyleXBotReference(pnlWindow);
            PreparePotionCaptions();
            // Reapply the active row after recoloring the navigation list.
            var selected = TabPageV_Control01.Tag as List<Control>;
            if (selected != null) foreach (Control c in selected) c.BackColor = PhBotSelect;
            LayoutXBotReference();
        }

        private void StyleXBotReference(Control control)
        {
            if (control is QuestPanel) return; // Quest already supplies its light table theme.
            control.Font = _referenceFont;
            control.ForeColor = Color.Black;
            control.BackColor = control == pnlWindow || control == _phBotActionsPanel || control == _referenceTools
                ? PhBotBg : Color.White;
            var button = control as Button;
            if (button != null)
            {
                bool sidebar = button.Parent == TabPageV_Control01;
                button.FlatStyle = sidebar ? FlatStyle.Flat : FlatStyle.Standard;
                button.UseVisualStyleBackColor = !sidebar;
                button.BackColor = sidebar ? Color.White : PhBotBg;
                button.Padding = Padding.Empty;
            }
            var check = control as CheckBox;
            if (check != null) { check.FlatStyle = FlatStyle.Standard; check.Padding = Padding.Empty; }
            var radio = control as RadioButton;
            if (radio != null) { radio.FlatStyle = FlatStyle.Standard; radio.Padding = Padding.Empty; }
            var list = control as ListView;
            if (list != null) {
                list.OwnerDraw = false;
                list.BorderStyle = BorderStyle.FixedSingle;
                list.FullRowSelect = true;
                list.HideSelection = false;
                list.GridLines = list.Columns.Count > 1;
            }
            var input = control as TextBox;
            if (input != null) input.BorderStyle = BorderStyle.FixedSingle;
            var combo = control as ComboBox;
            if (combo != null) combo.FlatStyle = FlatStyle.Standard;
            foreach (Control child in control.Controls) StyleXBotReference(child);
        }

        private void LayoutXBotReference()
        {
            if (!_phBotClassicApplied || _referenceLayingOut) return;
            _referenceLayingOut = true;
            try
            {
                int w = pnlWindow.ClientSize.Width, h = pnlWindow.ClientSize.Height;
                int sidebar = Math.Max(196, Math.Min(256, w / 5));
                const int margin = 12, gap = 12, bottom = 158;
                int x = margin + sidebar + gap;
                int contentW = Math.Max(600, w - x - margin);
                int contentH = Math.Max(380, h - bottom - margin * 3);
                TabPageV_Control01.SetBounds(margin, margin, sidebar, h - 2 * margin);
                foreach (Control c in TabPageV_Control01.Controls)
                    if (c is Button) c.Width = sidebar - 54; // reserve the vertical scrollbar; never scroll horizontally
                foreach (Panel view in _referenceViews)
                {
                    view.SetBounds(x, margin, contentW, contentH);
                    foreach (TabControl tabs in view.Controls.OfType<TabControl>())
                    {
                        int left = view == TabPageV_Control01_Skills_Panel ? Math.Min(250, contentW / 4) + gap : 0;
                        tabs.SetBounds(left, 0, contentW - left, contentH);
                    }
                }
                int logY = margin + contentH + gap;
                const int actionsW = 250;
                rtbxLogs.SetBounds(x, logY, contentW - actionsW - gap, 120);
                rtbxLogs.BorderStyle = BorderStyle.FixedSingle;
                _referenceTools.SetBounds(x, logY + 128, contentW - actionsW - gap, 28);
                _phBotActionsPanel.SetBounds(w - margin - actionsW, logY, actionsW, bottom);
                Button[] actions = { _phBotBtnLaunch, _phBotBtnStart, _phBotBtnClientless, _phBotBtnStop, _phBotBtnHide, _phBotBtnReturn };
                for (int i = 0; i < actions.Length; i++) actions[i].SetBounds((i % 2) * 130, 4 + (i / 2) * 39, 120, 28);
                _phBotCopyright.SetBounds(0, 132, actionsW, 22);
                LayoutXBotSkillPages();
            }
            finally { _referenceLayingOut = false; }
        }

        private void PrepareXBotReferenceContent()
        {
            // Existing settings groups keep their data bindings. A wrapping flow
            // provides natural reading order and scrolling on smaller windows.
            PrepareXBotLogin();
            FlowReferenceGroups(TabPageV_Control01_Login_Panel);
            FlowReferenceGroups(TabPageH_Town_Option01_Panel);
            FlowReferenceGroups(TabPageV_Control01_Alchemy_Panel);
            FlowReferenceGroups(TabPageH_Character_Option01_Panel);
            FlowReferenceGroups(TabPageH_Character_Option04_Panel);
            PrepareXBotPotions();
            PrepareXBotTraining();
            PrepareXBotPickFilter();
            TabPageV_Control01_Town_Panel.Controls[TabPageH_Town_Option03.Name + "_Panel"].Visible = false;
            foreach (var entry in _referencePages)
            {
                var pagePanel = entry.Value.Controls.OfType<Panel>().FirstOrDefault();
                if (pagePanel == null || pagePanel == TabPageH_Skills_Option01_Panel || pagePanel == TabPageH_Skills_Option02_Panel) continue;
                var lists = pagePanel.Controls.OfType<ListView>().ToArray();
                if (lists.Length == 1 && lists[0].Width > 450)
                    lists[0].Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            }
            TabPageH_Skills_Option01_Panel.SizeChanged += (s, e) => LayoutXBotSkillPages();
            TabPageH_Skills_Option02_Panel.SizeChanged += (s, e) => LayoutXBotSkillPages();
        }

        private void PrepareXBotLogin()
        {
            var selector = new GroupBox { Name = "XBotServerSelection", Text = "Silkroad", Size = new Size(415, 80), Font = _referenceFont, BackColor = Color.White };
            foreach (Control c in new Control[] { Login_lblSilkroad, Login_cmbxSilkroad, Login_btnAddSilkroad })
                selector.Controls.Add(c);
            Login_lblSilkroad.SetBounds(12, 24, 108, 26);
            Login_cmbxSilkroad.SetBounds(124, 24, 232, 26);
            Login_btnAddSilkroad.SetBounds(364, 24, 32, 26);
            Login_btnAddSilkroad.Text = "...";
            TabPageV_Control01_Login_Panel.Controls.Add(selector);
            // Sort before the server list when the wrapping layout is built.
            selector.Location = new Point(0, -1);
            Login_gbxLogin.Size = new Size(350, 330);
            Login_lblAccount.SetBounds(12, 24, 320, 22);
            Login_cmbxSavedAccounts.SetBounds(12, 50, 320, 26);
            Button[] accountButtons = { Login_btnSaveAccount, Login_btnDeleteAccount, Login_btnAccountSetup };
            string[] captions = { "Save", "Delete", "Account Setup" };
            for (int i = 0; i < accountButtons.Length; i++)
            {
                accountButtons[i].Text = captions[i];
                accountButtons[i].SetBounds(12 + i * 108, 84, 102, 28);
            }
            Control[] labels = { Login_lblUsername, Login_lblPassword, Login_lblCaptcha, Login_lblServer, Login_lblCharacter };
            Control[] inputs = { Login_tbxUsername, Login_tbxPassword, Login_tbxCaptcha, Login_cmbxServer, Login_cmbxCharacter };
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].SetBounds(12, 128 + i * 36, 95, 26);
                inputs[i].SetBounds(114, 128 + i * 36, 218, 26);
            }
            // At most two columns at laptop sizes; each group remains readable.
            Login_gbxConnection.Size = new Size(350, 220);
        }

        private void PrepareXBotPickFilter()
        {
            if (lstPickItems == null) return;
            var page = tabPickFilterRoot.TabPages[0];
            var footer = new Panel { Name = "XBotPickSearch", Dock = DockStyle.Bottom, Height = 105, BackColor = Color.White };
            foreach (Control c in page.Controls.Cast<Control>().Where(c => c != lstPickItems).ToArray())
            {
                int y = c.Top - 205;
                c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                footer.Controls.Add(c);
                c.Top = Math.Max(0, y);
            }
            page.Controls.Add(footer);
            page.Padding = new Padding(8);
            lstPickItems.Dock = DockStyle.Fill;
            lstPickItems.BringToFront();
        }

        private void FlowReferenceGroups(Panel host)
        {
            var groups = host.Controls.Cast<Control>().Where(c => c is GroupBox || c is Theme.ModernCard)
                .OrderBy(c => c.Top).ThenBy(c => c.Left).ToArray();
            if (groups.Length == 0) return;
            var flow = new FlowLayoutPanel { Name = host.Name + "_Groups", Dock = DockStyle.Fill,
                AutoScroll = true, BackColor = Color.White, Padding = new Padding(8), WrapContents = true };
            foreach (Control group in groups)
            {
                if (group == Login_gbxAdvertising || group == Login_gbxCharacters) continue;
                group.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                group.Margin = new Padding(0, 0, 14, 14);
                flow.Controls.Add(group);
            }
            host.Controls.Add(flow);
            flow.BringToFront();
        }

        private void PrepareXBotPotions()
        {
            var panel = TabPageH_Character_Option02_Panel;
            var rows = new[] { Character_cbxUseHP, Character_cbxUseHPGrain, Character_cbxUseMP, Character_cbxUseMPGrain,
                Character_cbxUseHPVigor, Character_cbxUseMPVigor, Character_cbxUseTransportHP, Character_cbxUsePetHP,
                Character_cbxUsePillUniversal, Character_cbxUsePillPurification, Character_cbxUsePetsPill, Character_cbxUsePetHGP };
            var values = new[] { Character_tbxUseHP, null, Character_tbxUseMP, null, Character_tbxUseHPVigor,
                Character_tbxUseMPVigor, Character_tbxUseTransportHP, Character_tbxUsePetHP, null, null, null, Character_tbxUsePetHGP };
            for (int i = 0; i < rows.Length; i++)
            {
                panel.Controls.Add(rows[i]);
                rows[i].AutoSize = false;
                rows[i].SetBounds(12, 10 + i * 32, 310, 26);
                if (values[i] == null) continue;
                panel.Controls.Add(values[i]);
                values[i].SetBounds(334, 12 + i * 32, 58, 24);
                panel.Controls.Add(new Label { Text = "%", Location = new Point(400, 15 + i * 32), AutoSize = true, Font = _referenceFont });
            }
            Character_gbxPotionsPlayer.Visible = Character_gbxPotionPet.Visible = false;
            panel.AutoScrollMinSize = new Size(440, 410);
            PreparePotionCaptions();
        }

        private void PreparePotionCaptions()
        {
            foreach (CheckBox check in TabPageH_Character_Option02_Panel.Controls.OfType<CheckBox>())
                check.Text = check.Text.Replace("%", "").TrimEnd();
        }

        private void PrepareXBotTraining()
        {
            var panel = TabPageH_Training_Option01_Panel;
            var details = new GroupBox { Name = "XBotAreaDetails", Text = "Training Area", Size = new Size(470, 190), BackColor = Color.White, Font = _referenceFont };
            // Move the original editor as a whole, translating its coordinates;
            // named controls continue to be read by the route and settings code.
            var editor = panel.Controls.Cast<Control>().Where(c => c != Training_lstvAreas && c != gbxReturnToArea).ToArray();
            foreach (Control c in editor) details.Controls.Add(c);
            var right = new FlowLayoutPanel { Name = "XBotAreaEditor", Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.White };
            details.Size = new Size(490, 250);
            Control[] labels = { Training_lblRegion, Training_lblX, Training_lblY, Training_lblZ };
            Control[] inputs = { Training_tbxRegion, Training_tbxX, Training_tbxY, Training_tbxZ };
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].SetBounds(12 + i * 115, 30, 100, 22);
                inputs[i].SetBounds(12 + i * 115, 58, 96, 26);
            }
            Training_btnGetCoordinates.SetBounds(12, 99, 146, 28);
            Training_lblRadius.SetBounds(185, 100, 65, 26);
            Training_tbxRadius.SetBounds(260, 100, 70, 26);
            Training_lblScriptPath.SetBounds(12, 145, 125, 24);
            Training_tbxScriptPath.SetBounds(12, 175, 420, 26);
            Training_btnLoadScriptPath.SetBounds(440, 174, 32, 28);
            Training_btnLoadScriptPath.Text = "...";
            details.Controls.Add(Training_cbxWalkToCenter);
            Training_cbxWalkToCenter.SetBounds(12, 213, 360, 26);
            right.Controls.Add(details);
            if (gbxReturnToArea != null)
            {
                gbxReturnToArea.Size = new Size(490, 285);
                gbxReturnToArea.Margin = new Padding(3, 10, 3, 3);
                gbxReturnToArea.Controls["lblReturnDesc"].SetBounds(12, 24, 460, 40);
                gbxReturnToArea.Controls["lblReturnScript"].SetBounds(12, 69, 460, 24);
                tbxReturnScriptPath.SetBounds(12, 98, 420, 26);
                btnReturnBrowseScript.SetBounds(440, 97, 32, 28);
                int y = 136;
                foreach (CheckBox c in gbxReturnToArea.Controls.OfType<CheckBox>())
                { c.SetBounds(12, y, 460, 24); y += 27; }
                right.Controls.Add(gbxReturnToArea);
            }
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(Training_lstvAreas, 0, 0);
            Training_lstvAreas.Dock = DockStyle.Fill;
            layout.Controls.Add(right, 1, 0);
            panel.Controls.Add(layout);
            layout.BringToFront();
        }

        private bool _referenceSkillsLayout;
        private void LayoutXBotSkillPages()
        {
            if (_referenceSkillsLayout || !_phBotClassicApplied) return;
            _referenceSkillsLayout = true;
            try
            {
                var host = TabPageV_Control01_Skills_Panel;
                int sourceW = Math.Min(250, host.Width / 4);
                Skills_lstvSkills.SetBounds(0, 0, sourceW, host.Height);
                Skills_lstvSkills.HeaderStyle = ColumnHeaderStyle.None;
                if (Skills_lstvSkills.Columns.Count > 0) Skills_lstvSkills.Columns[0].Width = sourceW - 24;
                var pages = new[] { TabPageH_Skills_Option01_Panel, TabPageH_Skills_Option02_Panel };
                for (int i = 0; i < pages.Length; i++)
                {
                    var p = pages[i];
                    const int listX = 48;
                    int listW = Math.Max(190, Math.Min(290, p.ClientSize.Width / 2 - 75));
                    int listH = Math.Max(240, p.ClientSize.Height - 115);
                    var combo = i == 0 ? Skills_cmbxAttackMobType : Skills_cmbxBuffMobType;
                    combo.SetBounds(listX, 12, listW, 26);
                    foreach (ListView list in p.Controls.OfType<ListView>())
                    {
                        list.SetBounds(listX, 50, listW, listH);
                        list.HeaderStyle = ColumnHeaderStyle.None;
                        if (list.Columns.Count > 0) list.Columns[0].Width = listW - 24;
                    }
                    var add = i == 0 ? Skills_btnAddAttack : Skills_btnAddBuff;
                    var remove = i == 0 ? Skills_btnRemAttack : Skills_btnRemBuff;
                    add.Text = "▶"; remove.Text = "◀";
                    add.SetBounds(0, 138, 38, 32); remove.SetBounds(0, 180, 38, 32);
                    ToolTips.SetToolTip(add, "Add selected skill"); ToolTips.SetToolTip(remove, "Remove selected skill");
                    int optionsX = listX + listW + 58;
                    if (i == 0)
                    {
                        int row = 18;
                        foreach (Control c in new Control[] { Skills_cbxCastInOrder, cbxSkillNoAttack, cbxSkillDevil })
                        {
                            if (c == null) continue;
                            c.SetBounds(optionsX, row, Math.Max(200, p.Width - optionsX - 12), 40);
                            row += 48;
                        }
                        int n = 0;
                        foreach (Button b in p.Controls.OfType<Button>().Where(b => b.Text == "▲" || b.Text == "▼"))
                        { b.SetBounds(listX + listW + 8, 138 + n++ * 42, 38, 32); }
                        lblSkillImbue.Text = "Imbue";
                        lblSkillImbue.SetBounds(listX, listH + 54, listW, 20);
                        cmbxImbue.SetBounds(listX, listH + 76, listW, 26);
                        lblSkillRuntimeStatus.SetBounds(optionsX, 176, Math.Max(200, p.Width - optionsX - 16), 50);
                    }
                }
            }
            finally { _referenceSkillsLayout = false; }
        }
    }
}
