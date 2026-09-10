using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using xBot.App.Theme;

namespace xBot.App
{
    public sealed class QuestListEntry
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Npc { get; set; }
        public string State { get; set; }
        public string Objectives { get; set; }
        public int Level { get; set; }
        public bool Enabled { get; set; }
        public string Completion { get; set; }
    }

    // Presentation only. Game actions and persistence are supplied by Window.
    public sealed class QuestPanel : UserControl
    {
        public event Action RefreshRequested;
        public event Action<string> SearchRequested;
        public event Action ResetRequested;
        public event Action<uint> DetailsRequested;
        public event Action<uint, ContextMenuStrip> MenuRequested;
        public event Action OptionsChanged;
        private readonly Panel[] pages = new Panel[3];
        private readonly Button[] tabs = new Button[3];
        public readonly DataGridView ActiveGrid;
        public readonly DataGridView CatalogGrid;
        private readonly Label activeEmpty, catalogEmpty, status;
        private readonly TextBox search;
        private readonly CheckBox aboveLevel, waitAll, townEvents;
        private readonly NumericUpDown levelMargin;
        private bool loadingOptions;
        public int SelectedTab { get; private set; }
        public int LevelMargin => aboveLevel.Checked ? (int)levelMargin.Value : 0;
        public bool WaitForAll => waitAll.Checked;
        public bool TownEventsOnly => townEvents.Checked;

        public QuestPanel()
        {
            BackColor = Color.White; ForeColor = Color.Black;
            Font = new Font("Segoe UI", 9f); Padding = new Padding(8);
            var navigation = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, WrapContents = false, Margin = Padding.Empty };
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
            status = new Label { Dock = DockStyle.Bottom, Height = 26, ForeColor = Color.FromArgb(80, 80, 80), TextAlign = ContentAlignment.MiddleLeft };
            Controls.Add(body); Controls.Add(status); Controls.Add(navigation);
            string[] titles = { "Active", "All", "Options" };
            for (int i = 0; i < pages.Length; i++)
            {
                int index = i;
                pages[i] = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
                body.Controls.Add(pages[i]);
                tabs[i] = MakeTabButton(titles[i], 85); tabs[i].Height = 28;
                tabs[i].Click += (s, e) => SelectTab(index);
                navigation.Controls.Add(tabs[i]);
            }
            ActiveGrid = MakeGrid(true); CatalogGrid = MakeGrid(false);
            pages[0].Controls.Add(ActiveGrid); pages[1].Controls.Add(CatalogGrid);
            activeEmpty = EmptyMessage("No active quests", "Switch to the All tab to browse and enable quests.");
            catalogEmpty = EmptyMessage("Loading quest catalog...", "");
            ActiveGrid.Controls.Add(activeEmpty); CatalogGrid.Controls.Add(catalogEmpty);
            var activeFooter = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(0, 6, 0, 0), WrapContents = false };
            var refresh = MakeButton("Refresh", 80); refresh.Click += (s, e) => RefreshRequested?.Invoke();
            activeFooter.Controls.Add(refresh);
            activeFooter.Controls.Add(new Label { AutoSize = true, Margin = new Padding(12, 6, 0, 0), ForeColor = Color.FromArgb(100, 100, 100), Text = "Double-click: details  ·  Right-click: auto quest options" });
            pages[0].Controls.Add(activeFooter);
            var catalogFooter = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(0, 6, 0, 0), ColumnCount = 5, RowCount = 1 };
            catalogFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) catalogFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            search = new TextBox { Dock = DockStyle.Fill, BackColor = Color.White, ForeColor = Color.Black, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 3, 10, 0), AccessibleName = "Search quest name or ID" };
            catalogFooter.Controls.Add(search, 0, 0);
            string[] actions = { "Search", "Clear", "Update", "Reset" };
            var searchButton = MakeButton(actions[0], 85); searchButton.Click += (s, e) => SearchRequested?.Invoke(search.Text);
            var clear = MakeButton(actions[1], 85); clear.Click += (s, e) => { search.Clear(); SearchRequested?.Invoke(""); };
            var update = MakeButton(actions[2], 85); update.Click += (s, e) => RefreshRequested?.Invoke();
            var reset = MakeButton(actions[3], 85); reset.Click += (s, e) => ResetRequested?.Invoke();
            catalogFooter.Controls.Add(searchButton, 1, 0); catalogFooter.Controls.Add(clear, 2, 0); catalogFooter.Controls.Add(update, 3, 0); catalogFooter.Controls.Add(reset, 4, 0);
            search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { SearchRequested?.Invoke(search.Text); e.SuppressKeyPress = true; } };
            pages[1].Controls.Add(catalogFooter);

            var options = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(12, 12, 0, 0) };
            aboveLevel = MakeCheck("Retrieve quests above your level", false);
            levelMargin = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 5, Width = 55, BackColor = Color.White, ForeColor = Color.Black, Enabled = false, Margin = new Padding(8, 2, 0, 0) };
            var levelRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 8) };
            levelRow.Controls.Add(aboveLevel); levelRow.Controls.Add(levelMargin);
            var cbxExpRatio = MakeCheck("Do not turn in quests if EXP ratio is not 100%", false);
            var cbxWalkTown = MakeCheck("Walk back to town from your training area to turn in quests", false);
            waitAll = MakeCheck("Do not turn in quests until all enabled quests are completed", false);
            var cbxCompleteWay = MakeCheck("Complete quests on the way to the training area", false);
            townEvents = MakeCheck("Complete event quests while in town", true);
            var cbxDanger = MakeCheck("Danger mode", false);

            options.Controls.Add(levelRow);
            options.Controls.Add(cbxExpRatio);
            options.Controls.Add(cbxWalkTown);
            options.Controls.Add(waitAll);
            options.Controls.Add(cbxCompleteWay);
            options.Controls.Add(townEvents);
            options.Controls.Add(cbxDanger);

            foreach (var box in new[] { aboveLevel, waitAll, townEvents, cbxExpRatio, cbxWalkTown, cbxCompleteWay, cbxDanger })
                box.CheckedChanged += (s, e) => { levelMargin.Enabled = aboveLevel.Checked; if (!loadingOptions) OptionsChanged?.Invoke(); };
            levelMargin.ValueChanged += (s, e) => { if (!loadingOptions) OptionsChanged?.Invoke(); };
            pages[2].Controls.Add(options);
            SelectTab(0);
        }

        public void SetOptions(int margin, bool wait, bool events)
        {
            loadingOptions = true;
            aboveLevel.Checked = margin > 0; levelMargin.Value = Math.Max(1, Math.Min(20, margin == 0 ? 5 : margin));
            waitAll.Checked = wait; townEvents.Checked = events; loadingOptions = false;
        }

        public void SelectTab(int index)
        {
            SelectedTab = index;
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Visible = i == index;
                tabs[i].BackColor = i == index ? Color.White : SystemColors.Control;
                tabs[i].ForeColor = Color.Black;
                tabs[i].Font = new Font("Segoe UI", 9f, i == index ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        public void SetStatus(string text) { status.Text = text; }
        public void SetActive(IList<QuestListEntry> entries) { Fill(ActiveGrid, entries, true); activeEmpty.Visible = entries.Count == 0; }
        public void SetCatalog(IList<QuestListEntry> entries, bool available)
        {
            Fill(CatalogGrid, entries, false); catalogEmpty.Visible = entries.Count == 0;
            catalogEmpty.Text = available ? "Type at least 2 characters and click Search\n\nClear will empty the search results."
                : "Quest catalog is not yet loaded.\n\nPlease check Silkroad Database connection.";
        }
        public void UpdateState(uint id, string state)
        {
            foreach (var grid in new[] { ActiveGrid, CatalogGrid })
                foreach (DataGridViewRow row in grid.Rows)
                    if ((uint)row.Tag == id) row.Cells["State"].Value = state;
        }

        private void Fill(DataGridView grid, IList<QuestListEntry> entries, bool active)
        {
            uint selected = grid.SelectedRows.Count > 0 ? (uint)grid.SelectedRows[0].Tag : 0;
            int top = grid.FirstDisplayedScrollingRowIndex;
            grid.SuspendLayout(); grid.Rows.Clear();
            foreach (QuestListEntry entry in entries)
            {
                int i = active ? grid.Rows.Add(entry.Id, entry.Name + (string.IsNullOrWhiteSpace(entry.Objectives) ? "" : "\n" + entry.Objectives), entry.State)
                    : grid.Rows.Add(entry.Id, entry.Name, entry.Npc, entry.Level, entry.Enabled ? "Yes" : "No", entry.State, entry.Completion);
                var row = grid.Rows[i]; row.Tag = entry.Id;
                if (active && !string.IsNullOrWhiteSpace(entry.Objectives)) row.Height = 55;
            }
            grid.ClearSelection();
            foreach (DataGridViewRow row in grid.Rows) if ((uint)row.Tag == selected) { row.Selected = true; break; }
            if (top >= 0 && top < grid.RowCount) grid.FirstDisplayedScrollingRowIndex = top;
            grid.ResumeLayout();
        }

        private DataGridView MakeGrid(bool active)
        {
            var grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                Font = Font, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 28, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.Single, GridColor = Color.FromArgb(225, 225, 225) };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = SystemColors.Control, ForeColor = Color.Black, Font = new Font(Font, FontStyle.Regular), Padding = new Padding(6, 0, 0, 0) };
            grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.Black,
                SelectionBackColor = SystemColors.Highlight, SelectionForeColor = SystemColors.HighlightText, Padding = new Padding(6, 3, 6, 3), WrapMode = DataGridViewTriState.True };
            grid.RowTemplate.Height = 26; grid.RowTemplate.MinimumHeight = 24;
            AddColumn(grid, "Id", "ID", 60, 5); AddColumn(grid, "Name", "Name", 250, active ? 70 : 35);
            if (!active) { AddColumn(grid, "Npc", "NPC", 180, 25); AddColumn(grid, "Level", "Level", 55, 5); AddColumn(grid, "Enabled", "Enabled", 80, 8); }
            AddColumn(grid, "State", "State", 155, active ? 25 : 14);
            if (!active) AddColumn(grid, "Completion", "Return", 120, 13);
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) DetailsRequested?.Invoke((uint)grid.Rows[e.RowIndex].Tag); };
            grid.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && grid.SelectedRows.Count > 0) { DetailsRequested?.Invoke((uint)grid.SelectedRows[0].Tag); e.Handled = true; } };
            var menu = new ContextMenuStrip { BackColor = Color.White, ForeColor = Color.Black, ShowImageMargin = false };
            grid.ContextMenuStrip = menu;
            grid.CellMouseDown += (s, e) => { if (e.Button == MouseButtons.Right) { grid.ClearSelection(); if (e.RowIndex >= 0) grid.Rows[e.RowIndex].Selected = true; } };
            menu.Opening += (s, e) =>
            {
                foreach (ToolStripItem item in menu.Items.Cast<ToolStripItem>().ToArray()) item.Dispose();
                menu.Items.Clear();
                if (grid.SelectedRows.Count == 0) { e.Cancel = true; return; }
                MenuRequested?.Invoke((uint)grid.SelectedRows[0].Tag, menu);
                e.Cancel = menu.Items.Count == 0;
            };
            grid.Disposed += (s, e) => menu.Dispose();
            return grid;
        }
        public static Form CreateDetailsDialog(string title, IEnumerable<KeyValuePair<string, string>> sections)
        {
            var dialog = new Form { Text = title, Size = new Size(820, 690), MinimumSize = new Size(570, 440),
                StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9f), MinimizeBox = false, Padding = new Padding(20) };
            var content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = true, BackColor = Color.White };
            dialog.Controls.Add(content);
            var bold = new Font(dialog.Font, FontStyle.Bold);
            dialog.Disposed += (s, e) => bold.Dispose();
            foreach (var section in sections)
            {
                content.Controls.Add(new Label { AutoSize = true, Text = section.Key, ForeColor = Color.FromArgb(0, 102, 204), Font = bold, Margin = new Padding(0, 0, 0, 4) });
                content.Controls.Add(new Label { AutoSize = true, Text = section.Value, ForeColor = Color.Black,
                    Font = dialog.Font, Margin = new Padding(0, 0, 0, 18), UseMnemonic = false, MaximumSize = new Size(710, 0) });
            }
            content.Resize += (s, e) => {
                foreach (Control child in content.Controls) child.MaximumSize = new Size(Math.Max(200, content.ClientSize.Width - 24), 0);
            };
            dialog.KeyPreview = true;
            dialog.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) dialog.Close(); };
            return dialog;
        }

        private static void AddColumn(DataGridView grid, string name, string title, int minimum, int weight)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = title, MinimumWidth = minimum, FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable });
        }
        private static Label EmptyMessage(string title, string subtitle) => new Label { Dock = DockStyle.Fill, BackColor = Color.White,
            ForeColor = Color.FromArgb(120, 120, 120), TextAlign = ContentAlignment.MiddleCenter, Text = title + "\n\n" + subtitle };
        private static CheckBox MakeCheck(string text, bool value) => new CheckBox { AutoSize = true, Text = text, Checked = value, Margin = new Padding(0, 4, 0, 8), ForeColor = Color.Black };
        public static Button MakeButton(string text, int width)
        {
            var button = new Button { Text = text, Width = width, Height = 28, FlatStyle = FlatStyle.Standard, BackColor = SystemColors.Control,
                ForeColor = Color.Black, Cursor = Cursors.Default, Margin = new Padding(0, 0, 6, 0), UseVisualStyleBackColor = true };
            return button;
        }
        private static Button MakeTabButton(string text, int width)
        {
            var button = new Button { Text = text, Width = width, Height = 28, FlatStyle = FlatStyle.Standard, BackColor = SystemColors.Control,
                ForeColor = Color.Black, Margin = new Padding(0, 0, 4, 0), UseVisualStyleBackColor = true };
            return button;
        }
    }
}
