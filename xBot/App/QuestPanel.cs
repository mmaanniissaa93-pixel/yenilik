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
            BackColor = DarkTheme.BgDark; ForeColor = DarkTheme.TextPrimary;
            Font = new Font("Segoe UI", 10f); Padding = new Padding(16);
            var navigation = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 43, WrapContents = false, Margin = Padding.Empty };
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 0) };
            status = new Label { Dock = DockStyle.Bottom, Height = 30, ForeColor = DarkTheme.TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            Controls.Add(body); Controls.Add(status); Controls.Add(navigation);
            string[] titles = { "Aktif", "Tümü", "Seçenekler" };
            for (int i = 0; i < pages.Length; i++)
            {
                int index = i;
                pages[i] = new Panel { Dock = DockStyle.Fill, BackColor = DarkTheme.BgDark };
                body.Controls.Add(pages[i]);
                tabs[i] = MakeButton(titles[i], 112); tabs[i].Height = 39;
                tabs[i].Click += (s, e) => SelectTab(index);
                navigation.Controls.Add(tabs[i]);
            }
            ActiveGrid = MakeGrid(true); CatalogGrid = MakeGrid(false);
            pages[0].Controls.Add(ActiveGrid); pages[1].Controls.Add(CatalogGrid);
            activeEmpty = EmptyMessage("Henüz aktif görev yok", "Görev seçmek için Tümü sekmesini açın.");
            catalogEmpty = EmptyMessage("Görev kataloğu yükleniyor", "");
            ActiveGrid.Controls.Add(activeEmpty); CatalogGrid.Controls.Add(catalogEmpty);
            var activeFooter = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 49, Padding = new Padding(0, 12, 0, 0), WrapContents = false };
            var refresh = MakeButton("Yenile", 95); refresh.Click += (s, e) => RefreshRequested?.Invoke();
            activeFooter.Controls.Add(refresh);
            activeFooter.Controls.Add(new Label { AutoSize = true, Margin = new Padding(14, 8, 0, 0), ForeColor = DarkTheme.TextMuted, Text = "Çift tık: görev detayları  ·  Sağ tık: otomasyon" });
            pages[0].Controls.Add(activeFooter);
            var catalogFooter = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 49, Padding = new Padding(0, 12, 0, 0), ColumnCount = 5, RowCount = 1 };
            catalogFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) catalogFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98));
            search = new TextBox { Dock = DockStyle.Fill, BackColor = DarkTheme.BgInput, ForeColor = DarkTheme.TextPrimary, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 4, 12, 0), AccessibleName = "Görev adı veya numarası ara" };
            catalogFooter.Controls.Add(search, 0, 0);
            string[] actions = { "Ara", "Temizle", "Güncelle", "Sıfırla" };
            var searchButton = MakeButton(actions[0], 90); searchButton.Click += (s, e) => SearchRequested?.Invoke(search.Text);
            var clear = MakeButton(actions[1], 90); clear.Click += (s, e) => { search.Clear(); SearchRequested?.Invoke(""); };
            var update = MakeButton(actions[2], 90); update.Click += (s, e) => RefreshRequested?.Invoke();
            var reset = MakeButton(actions[3], 90); reset.Click += (s, e) => ResetRequested?.Invoke();
            catalogFooter.Controls.Add(searchButton, 1, 0); catalogFooter.Controls.Add(clear, 2, 0); catalogFooter.Controls.Add(update, 3, 0); catalogFooter.Controls.Add(reset, 4, 0);
            search.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { SearchRequested?.Invoke(search.Text); e.SuppressKeyPress = true; } };
            pages[1].Controls.Add(catalogFooter);

            var options = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(8, 12, 0, 0) };
            aboveLevel = MakeCheck("Seviyemin üzerindeki görevleri de al", false);
            levelMargin = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 5, Width = 65, BackColor = DarkTheme.BgInput, ForeColor = DarkTheme.TextPrimary, Enabled = false, Margin = new Padding(12, 4, 0, 0) };
            var levelRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 14) };
            levelRow.Controls.Add(aboveLevel); levelRow.Controls.Add(levelMargin);
            levelRow.Controls.Add(new Label { AutoSize = true, Text = "seviye", ForeColor = DarkTheme.TextMuted, Margin = new Padding(8, 7, 0, 0) });
            waitAll = MakeCheck("Etkin görevlerin tümü tamamlanana kadar teslim etme", false);
            townEvents = MakeCheck("Etkinlik görevlerini yalnızca şehirde işle", true);
            options.Controls.Add(levelRow); options.Controls.Add(waitAll); options.Controls.Add(townEvents);
            options.Controls.Add(new Label { AutoSize = true, MaximumSize = new Size(760, 0), Margin = new Padding(0, 22, 0, 0), ForeColor = DarkTheme.TextMuted,
                Text = "Ayarlar otomatik kaydedilir. Görev bazında teslim, tekrar ve ödül tercihleri için listedeki göreve sağ tıklayın." });
            foreach (var box in new[] { aboveLevel, waitAll, townEvents }) box.CheckedChanged += (s, e) => { levelMargin.Enabled = aboveLevel.Checked; if (!loadingOptions) OptionsChanged?.Invoke(); };
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
                tabs[i].BackColor = i == index ? DarkTheme.AccentSubtle : DarkTheme.BgDark;
                tabs[i].ForeColor = i == index ? DarkTheme.InfoBlue : DarkTheme.TextMuted;
            }
        }

        public void SetStatus(string text) { status.Text = text; }
        public void SetActive(IList<QuestListEntry> entries) { Fill(ActiveGrid, entries, true); activeEmpty.Visible = entries.Count == 0; }
        public void SetCatalog(IList<QuestListEntry> entries, bool available)
        {
            Fill(CatalogGrid, entries, false); catalogEmpty.Visible = entries.Count == 0;
            catalogEmpty.Text = available ? "Aramak için en az 2 karakter yazıp Ara'ya basın\n\nTemizle listeyi boşaltır ve belleği bırakır."
                : "Görev kataloğu henüz yüklenmedi\n\nOyun veritabanını seçin veya PK2 veritabanını güncelleyin.";
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
                int i = active ? grid.Rows.Add(entry.Id, entry.Name + (string.IsNullOrWhiteSpace(entry.Objectives) ? "" : "\n" + entry.Objectives), entry.State, entry.Enabled ? "Etkin" : "Kapalı")
                    : grid.Rows.Add(entry.Id, entry.Name, entry.Npc, entry.Level, entry.Enabled ? "Etkin" : "Kapalı", entry.State, entry.Completion);
                var row = grid.Rows[i]; row.Tag = entry.Id;
                if (active && !string.IsNullOrWhiteSpace(entry.Objectives)) row.Height = 65;
            }
            grid.ClearSelection();
            foreach (DataGridViewRow row in grid.Rows) if ((uint)row.Tag == selected) { row.Selected = true; break; }
            if (top >= 0 && top < grid.RowCount) grid.FirstDisplayedScrollingRowIndex = top;
            grid.ResumeLayout();
        }

        private DataGridView MakeGrid(bool active)
        {
            var grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = DarkTheme.BgCard, BorderStyle = BorderStyle.None,
                Font = Font, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 37, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, GridColor = DarkTheme.BorderSubtle };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = DarkTheme.BgCardHeader, ForeColor = DarkTheme.TextSecondary, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(10, 0, 0, 0) };
            grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = DarkTheme.BgCard, ForeColor = DarkTheme.TextPrimary,
                SelectionBackColor = DarkTheme.AccentSubtle, SelectionForeColor = DarkTheme.TextPrimary, Padding = new Padding(10, 4, 8, 4), WrapMode = DataGridViewTriState.True };
            grid.RowTemplate.Height = 41; grid.RowTemplate.MinimumHeight = 41;
            AddColumn(grid, "Id", "ID", 70, 5); AddColumn(grid, "Name", "Görev", 250, active ? 68 : 35);
            if (!active) { AddColumn(grid, "Npc", "NPC", 180, 25); AddColumn(grid, "Level", "Seviye", 60, 5); AddColumn(grid, "Enabled", "Otomasyon", 95, 9); }
            AddColumn(grid, "State", "Durum", 155, 16);
            if (active) AddColumn(grid, "Enabled", "Otomasyon", 100, 11);
            else AddColumn(grid, "Completion", "Tamamlanınca", 145, 13);
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) DetailsRequested?.Invoke((uint)grid.Rows[e.RowIndex].Tag); };
            grid.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && grid.SelectedRows.Count > 0) { DetailsRequested?.Invoke((uint)grid.SelectedRows[0].Tag); e.Handled = true; } };
            var menu = new ContextMenuStrip { BackColor = DarkTheme.BgCard, ForeColor = DarkTheme.TextPrimary, ShowImageMargin = false };
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
                StartPosition = FormStartPosition.CenterParent, BackColor = DarkTheme.BgDark, ForeColor = DarkTheme.TextPrimary,
                Font = new Font("Segoe UI", 10f), MinimizeBox = false, Padding = new Padding(24) };
            var content = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = true, BackColor = DarkTheme.BgDark };
            dialog.Controls.Add(content);
            var bold = new Font(dialog.Font, FontStyle.Bold);
            dialog.Disposed += (s, e) => bold.Dispose();
            foreach (var section in sections)
            {
                content.Controls.Add(new Label { AutoSize = true, Text = section.Key, ForeColor = DarkTheme.InfoBlue, Font = bold, Margin = new Padding(0, 0, 0, 6) });
                content.Controls.Add(new Label { AutoSize = true, Text = section.Value, ForeColor = DarkTheme.TextPrimary,
                    Font = dialog.Font, Margin = new Padding(0, 0, 0, 24), UseMnemonic = false, MaximumSize = new Size(710, 0) });
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
        private static Label EmptyMessage(string title, string subtitle) => new Label { Dock = DockStyle.Fill, BackColor = DarkTheme.BgCard,
            ForeColor = DarkTheme.TextMuted, TextAlign = ContentAlignment.MiddleCenter, Text = title + "\n\n" + subtitle };
        private static CheckBox MakeCheck(string text, bool value) => new CheckBox { AutoSize = true, Text = text, Checked = value, Margin = new Padding(0, 5, 0, 19), ForeColor = DarkTheme.TextPrimary };
        public static Button MakeButton(string text, int width)
        {
            var button = new Button { Text = text, Width = width, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = DarkTheme.BgInput,
                ForeColor = DarkTheme.TextPrimary, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) };
            button.FlatAppearance.BorderSize = 0; return button;
        }
    }
}
