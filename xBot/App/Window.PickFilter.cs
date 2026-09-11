using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game;

namespace xBot.App
{
    /// <summary>
    /// phBot tarzı Pick Filter sekmeleri: Pick Filter / Options / Store Gold / Dismantle.
    /// Item Filter sekmesinin içeriğini kaplar; motor ItemFilterManager üzerinden çalışır.
    /// </summary>
    public partial class Window
    {
        private TabControl tabPickFilterRoot;
        private ListView lstPickItems;
        private ComboBox cmbPickGroup;
        private ComboBox cmbPickRace;
        private ComboBox cmbPickGender;
        private ComboBox cmbPickDegree;
        private TextBox txtPickSearch;
        private Label lblPickCount;
        private int pickCtxColumn = -1;
        private bool loadingPickFilterUi;

        private CheckBox optDontPickItems;
        private CheckBox optPickItemsFirst;
        private CheckBox optUsePickPet;
        private CheckBox optPickOthers;
        private CheckBox optPickParty;
        private CheckBox optAllowSellAll;
        private CheckBox optOnlyPickRareBlue;
        private CheckBox optOnlyStoreRareBlue;
        private CheckBox optPickCharIfPetFull;
        private CheckBox optDontMovePetItems;
        private CheckBox optOnlyStorePlus;
        private NumericUpDown nudOnlyStorePlus;
        private CheckBox optNoSellPlus;
        private NumericUpDown nudNoSellPlus;
        private CheckBox optPickEvenWhenFull;
        private ListView lstBlues;
        private int bluesCtxColumn = -1;

        private CheckBox optGoldEnabled;
        private TextBox txtGoldKeep;
        private CheckBox optGoldTakeStorage;
        private CheckBox optGoldTakeGuild;
        private CheckBox optGoldStoreStorage;
        private TextBox txtGoldStoreMax;
        private CheckBox optGoldStoreGuild;
        private TextBox txtGoldStoreGuildMax;

        private ListView lstDegrees;
        private int degreeCtxColumn = -1;
        private CheckBox optDisWhite;
        private CheckBox optDisPlussed;
        private NumericUpDown nudDisPlussed;
        private CheckBox optDisBlue;
        private CheckBox optDisRare;
        private CheckBox optDisBeforeSmith;
        private CheckBox optDisBeforeGrocery;
        private CheckBox optDisBeforeHerbalist;
        private CheckBox optDisBeforeStorage;
        private CheckBox optDisBeforeGuild;

        private static readonly string[] PickGroups = new string[]
        {
            "All", "Weapon", "Armor", "Shield", "Accessory", "Avatar",
            "Potion", "Pill", "Scroll", "Ammo", "Gold", "TradeGoods",
            "Quest", "Elixir", "AlchemyMaterial", "CosTransport", "Other"
        };

        public void BuildPickFilterTabs()
        {
            try
            {
                if (TabPageH_Town_Option03_Panel == null)
                    return;

                // Eski basit filtre grubu yeni sistemle çakışmasın diye gizlenir
                // (referanslar Bot tarafında null-kontrollü kullanıldığı için durur).
                try
                {
                    if (Filter_gbxPick != null) Filter_gbxPick.Visible = false;
                    var old1 = TabPageH_Town_Option03_Panel.Controls.Find("gbxItemFilterRules", false);
                    var old2 = TabPageH_Town_Option03_Panel.Controls.Find("gbxItemFilterRulesCustom", false);
                    foreach (var c in old1) c.Visible = false;
                    foreach (var c in old2) c.Visible = false;
                }
                catch { }

                // "Lojistik & Pot Alma" sekmesi kaldırıldı: motor varsayılan
                // değerlerle çalışır (hepsi açık), ayar menüsü yok.
                try
                {
                    if (TabPageH_Town_Option01 != null) TabPageH_Town_Option01.Visible = false;
                    if (TabPageH_Town_Option01_Panel != null) TabPageH_Town_Option01_Panel.Visible = false;
                }
                catch { }

                tabPickFilterRoot = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 8.5F)
                };

                var tabItems = new TabPage("Pick Filter");
                var tabOptions = new TabPage("Options");
                var tabGold = new TabPage("Store Gold");
                var tabDismantle = new TabPage("Dismantle");
                tabPickFilterRoot.TabPages.AddRange(new TabPage[] { tabItems, tabOptions, tabGold, tabDismantle });
                // Blues DB'ye bağlıdır (açılışta DB henüz yoksa boş kalır) — sekmeye
                // girince eksikse doldur. Eşya listesi bilinçli tembeldir.
                tabPickFilterRoot.SelectedIndexChanged += (s, e) =>
                {
                    try
                    {
                        if (tabPickFilterRoot.SelectedTab == tabOptions && !bluesLoaded)
                            RefreshBluesList();
                    }
                    catch { }
                };

                BuildPickItemsTab(tabItems);
                BuildPickOptionsTab(tabOptions);
                BuildStoreGoldTab(tabGold);
                BuildDismantleTab(tabDismantle);

                TabPageH_Town_Option03_Panel.Controls.Add(tabPickFilterRoot);
                tabPickFilterRoot.BringToFront();

                // Tema (Window_Load dahil) sonradan tekrar koyu basarsa diye:
                // sekme görünür olunca + açılıştan 1.5sn sonra açık temayı tazelet.
                tabPickFilterRoot.VisibleChanged += (s, e) =>
                {
                    try { if (tabPickFilterRoot.Visible) ApplyPickFilterLightTheme(); } catch { }
                };
                try
                {
                    var lateTheme = new Timer { Interval = 1500 };
                    lateTheme.Tick += (s, e) =>
                    {
                        try
                        {
                            lateTheme.Stop();
                            lateTheme.Dispose();
                            ApplyPickFilterLightTheme();
                        }
                        catch { }
                    };
                    lateTheme.Start();
                }
                catch { }

                // Eşya listesi açılışta YÜKLENMEZ (RAM + isimsiz yığın): kullanıcı
                // kategori seçince ya da isim arayınca dolar. Blues/Degree küçüktür.
                LoadPickFilterSettingsToUi();
            }
            catch (Exception ex)
            {
                try { Log("[PickFilter UI] " + ex.Message); } catch { }
            }
        }

        /// <summary>
        /// ApplyModernTheme tüm kontrollere koyu tema bastığı için Pick Filter
        /// sekmeleri (phBot görünümü) sonradan açık temaya geri alınır.
        /// InitializeCustomUBOTFeatures içinde ApplyModernTheme() SONRASINDA çağrılmalı.
        /// </summary>
        public void ApplyPickFilterLightTheme()
        {
            try
            {
                if (tabPickFilterRoot == null) return;
                foreach (TabPage page in tabPickFilterRoot.TabPages)
                {
                    try { page.BackColor = SystemColors.Control; page.ForeColor = SystemColors.ControlText; } catch { }
                    ApplyLightThemeRecursive(page);
                }
                // OwnerDraw kapatılınca sistem başlıkları (açık) geri gelir.
                foreach (var lv in new ListView[] { lstPickItems, lstBlues, lstDegrees })
                {
                    try
                    {
                        if (lv == null) continue;
                        lv.OwnerDraw = false;
                        lv.BackColor = SystemColors.Window;
                        lv.ForeColor = SystemColors.WindowText;
                        foreach (ColumnHeader col in lv.Columns)
                        {
                            try { col.Width = Math.Max(40, col.Width); } catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void ApplyLightThemeRecursive(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                try
                {
                    if (c is ListView) { /* ayrıca ele alınır */ }
                    else if (c is Button b)
                    {
                        b.BackColor = SystemColors.Control;
                        b.ForeColor = SystemColors.ControlText;
                        b.FlatStyle = FlatStyle.Standard;
                        b.UseVisualStyleBackColor = true;
                    }
                    else if (c is CheckBox)
                    {
                        c.ForeColor = Color.Black;
                        c.BackColor = Color.Transparent;
                    }
                    else if (c is Label)
                    {
                        c.ForeColor = Color.Black;
                        c.BackColor = Color.Transparent;
                    }
                    else if (c is TextBox || c is ComboBox)
                    {
                        c.BackColor = SystemColors.Window;
                        c.ForeColor = SystemColors.WindowText;
                    }
                    else if (c is NumericUpDown)
                    {
                        c.BackColor = SystemColors.Window;
                        c.ForeColor = SystemColors.WindowText;
                    }
                    else if (c is TabPage || c is TabControl || c is Panel)
                    {
                        c.BackColor = SystemColors.Control;
                        c.ForeColor = SystemColors.ControlText;
                    }
                    if (c.Controls.Count > 0)
                        ApplyLightThemeRecursive(c);
                }
                catch { }
            }
        }

        #region Tab: Pick Filter (liste)

        private void BuildPickItemsTab(TabPage tab)
        {
            lstPickItems = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = true,
                HideSelection = false,
                BackColor = SystemColors.Window,
                ForeColor = SystemColors.WindowText,
                Location = new Point(8, 8),
                Size = new Size(633, 190),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            try { lstPickItems.SmallImageList = Window.Get?.lstimgIcons; } catch { }
            lstPickItems.Columns.Add("ID", 50);
            lstPickItems.Columns.Add("Icon", 45);
            lstPickItems.Columns.Add("Name", 210);
            lstPickItems.Columns.Add("Level", 45);
            lstPickItems.Columns.Add("Pick", 42);
            lstPickItems.Columns.Add("Pet", 42);
            lstPickItems.Columns.Add("Sell", 42);
            lstPickItems.Columns.Add("Store", 45);
            lstPickItems.Columns.Add("Store Guild", 75);
            lstPickItems.Columns.Add("Take", 45);
            lstPickItems.Columns.Add("Take Guild", 75);

            var ctx = new ContextMenuStrip();
            var miYes = new ToolStripMenuItem("Yes");
            var miNo = new ToolStripMenuItem("No");
            var miReset = new ToolStripMenuItem("Reset");
            miYes.Click += (s, e) => ApplyPickFlag("Yes");
            miNo.Click += (s, e) => ApplyPickFlag("No");
            miReset.Click += (s, e) => ApplyPickFlag("Reset");
            ctx.Items.AddRange(new ToolStripItem[] { miYes, miNo, miReset });
            lstPickItems.ContextMenuStrip = ctx;
            lstPickItems.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = lstPickItems.HitTest(e.Location);
                    pickCtxColumn = hit.Item != null ? hit.SubItem != null ? hit.Item.SubItems.IndexOf(hit.SubItem) : -1 : -1;
                }
            };

            int fy = 204;
            cmbPickGroup = new ComboBox { Location = new Point(8, fy), Size = new Size(115, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickGroup.Items.AddRange(PickGroups);
            cmbPickGroup.SelectedIndex = 0;
            cmbPickGroup.SelectedIndexChanged += (s, e) => RefreshPickList();

            cmbPickRace = new ComboBox { Location = new Point(127, fy), Size = new Size(75, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickRace.Items.AddRange(new string[] { "All", "Chinese", "European" });
            cmbPickRace.SelectedIndex = 0;
            cmbPickRace.SelectedIndexChanged += (s, e) => RefreshPickList();

            cmbPickGender = new ComboBox { Location = new Point(206, fy), Size = new Size(65, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickGender.Items.AddRange(new string[] { "Any", "Male", "Female" });
            cmbPickGender.SelectedIndex = 0;
            cmbPickGender.SelectedIndexChanged += (s, e) => RefreshPickList();

            cmbPickDegree = new ComboBox { Location = new Point(275, fy), Size = new Size(55, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickDegree.Items.Add("Any");
            for (int d = 1; d <= 12; d++) cmbPickDegree.Items.Add(d.ToString());
            cmbPickDegree.SelectedIndex = 0;
            cmbPickDegree.SelectedIndexChanged += (s, e) => RefreshPickList();

            txtPickSearch = new TextBox { Location = new Point(334, fy), Size = new Size(307, 22), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            txtPickSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { RefreshPickList(); e.Handled = true; e.SuppressKeyPress = true; } };

            lblPickCount = new Label { Location = new Point(513, fy + 3), Size = new Size(128, 18), ForeColor = SystemColors.GrayText, Text = "", Visible = false };

            var btnClear = new Button { Text = "Clear", Location = new Point(415, fy + 28), Size = new Size(70, 26), UseVisualStyleBackColor = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var btnUpdate = new Button { Text = "Update", Location = new Point(491, fy + 28), Size = new Size(70, 26), UseVisualStyleBackColor = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var btnReset = new Button { Text = "Reset", Location = new Point(567, fy + 28), Size = new Size(70, 26), UseVisualStyleBackColor = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnClear.Click += (s, e) => { try { ClearPickList(); } catch { } };
            btnUpdate.Click += (s, e) => SavePickFilter();
            btnReset.Click += (s, e) => ResetPickFilter();

            var lblNote = new Label
            {
                Location = new Point(8, fy + 32),
                Size = new Size(400, 24),
                ForeColor = SystemColors.GrayText,
                Text = "* SOX items will not be sold, and your primary/secondary weapon will not be sold/stored",
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            tab.Controls.AddRange(new Control[] {
                lstPickItems, cmbPickGroup, cmbPickRace, cmbPickGender, cmbPickDegree,
                txtPickSearch, lblPickCount, btnClear, btnUpdate, btnReset, lblNote });
        }

        /// <summary>
        /// Bayrak değişikliği ANINDA motora işlenir (Update beklenmez).
        /// </summary>
        private void SaveAllPickFilterLive()
        {
            try
            {
                if (loadingPickFilterUi)
                    return;
                SavePickOptionsFromUi();
                SaveGoldFromUi();
                SaveDismantleFromUi();
                SaveBluesFromUi();
                try { Settings.SaveCharacterSettings(); } catch { }
            }
            catch { }
        }

        private void ApplyPickFlag(string value)
        {
            try
            {
                if (lstPickItems.SelectedItems.Count == 0 || pickCtxColumn < 4)
                    return;
                foreach (ListViewItem item in lstPickItems.SelectedItems)
                {
                    if (pickCtxColumn >= item.SubItems.Count)
                        continue;
                    if (value == "Reset")
                    {
                        for (int c = 4; c <= 10 && c < item.SubItems.Count; c++)
                            item.SubItems[c].Text = "No";
                    }
                    else
                    {
                        item.SubItems[pickCtxColumn].Text = value;
                    }
                    ApplyPickRowLive(item);
                }
                try { Settings.SaveCharacterSettings(); } catch { }
            }
            catch { }
        }

        private void ApplyPickRowLive(ListViewItem item)
        {
            try
            {
                string servername = item.Tag as string;
                if (string.IsNullOrEmpty(servername)) return;
                bool pick = GetFlag(item, 4), pet = GetFlag(item, 5), sell = GetFlag(item, 6);
                bool store = GetFlag(item, 7), guild = GetFlag(item, 8);
                bool take = GetFlag(item, 9), takeGuild = GetFlag(item, 10);
                if (pick || pet || sell || store || guild || take || takeGuild)
                    ItemFilterManager.SetRuleFull(servername, pick, pet, sell, store, guild, take, takeGuild);
                else
                    ItemFilterManager.RemoveRule(servername);
            }
            catch { }
        }

        private void RefreshPickList()
        {
            try
            {
                if (lstPickItems == null) return;
                string group = "All", race = "All", gender = "Any";
                int degree = 0;
                string search = "";
                try
                {
                    if (cmbPickGroup != null && cmbPickGroup.SelectedItem != null) group = cmbPickGroup.SelectedItem.ToString();
                    if (cmbPickRace != null && cmbPickRace.SelectedItem != null) race = cmbPickRace.SelectedItem.ToString();
                    if (cmbPickGender != null && cmbPickGender.SelectedItem != null) gender = cmbPickGender.SelectedItem.ToString();
                    if (cmbPickDegree != null && cmbPickDegree.SelectedIndex > 0) degree = cmbPickDegree.SelectedIndex;
                    if (txtPickSearch != null) search = (txtPickSearch.Text ?? "").Trim();
                }
                catch { }

                bool narrowed = !string.Equals(group, "All", StringComparison.Ordinal)
                    || !string.Equals(race, "All", StringComparison.Ordinal)
                    || !string.Equals(gender, "Any", StringComparison.Ordinal)
                    || degree > 0
                    || search.Length >= 2;
                // Daraltma yoksa DB'ye gitme: 2000 satırlık liste RAM'de kalırdı.
                // Temizle sonrası da liste boş kalır.
                if (!narrowed)
                {
                    lstPickItems.BeginUpdate();
                    lstPickItems.Items.Clear();
                    lstPickItems.EndUpdate();
                    try
                    {
                        if (search.Length == 1)
                            lblPickCount.Text = "En az 2 karakter yazın...";
                        else
                            lblPickCount.Text = "Kategori seçin veya arayın...";
                    }
                    catch { }
                    return;
                }

                List<NameValueCollection> rows = DataManager.QueryItems(group, race, gender, degree, search);
                bool truncated = rows.Count >= 500;
                lstPickItems.BeginUpdate();
                lstPickItems.Items.Clear();
                foreach (var row in rows)
                {
                    try
                    {
                        string id = row["id"] ?? "";
                        string name = (row["name"] ?? "").Trim();
                        string servername = row["servername"] ?? "";
                        string level = row["level"] ?? "0";
                        var rule = ItemFilterManager.GetRule(name) ?? ItemFilterManager.GetRule(servername);
                        string pick = "No", pet = "No", sell = "No", store = "No", guild = "No", take = "No", takeGuild = "No";
                        if (rule != null)
                        {
                            pick = rule.Pickup ? "Yes" : "No";
                            pet = rule.Pet ? "Yes" : "No";
                            sell = rule.Sell ? "Yes" : "No";
                            store = rule.Store ? "Yes" : "No";
                            guild = rule.StoreGuild ? "Yes" : "No";
                            take = rule.TakeStorage ? "Yes" : "No";
                            takeGuild = rule.TakeGuildStorage ? "Yes" : "No";
                        }
                        var item = new ListViewItem(new string[] { id, "", name, level, pick, pet, sell, store, guild, take, takeGuild });
                        item.Tag = servername;
                        lstPickItems.Items.Add(item);
                    }
                    catch { }
                }
                lstPickItems.EndUpdate();
                try
                {
                    lblPickCount.Text = lstPickItems.Items.Count + " items"
                        + (truncated ? " (ilk 500 — daraltın)" : "");
                }
                catch { }
                // Geçici DB satırlarını bırak (ListView kendi kopyasını tutar).
                try { rows.Clear(); } catch { }
            }
            catch (Exception ex)
            {
                try { lblPickCount.Text = "DB yok: " + ex.Message; } catch { }
            }
        }

        /// <summary>
        /// Listeyi boşaltır, arama kutusunu temizler ve referansları bırakır.
        /// Temizle'ye basınca liste kaybolur, RAM şişmez.
        /// </summary>
        public void ClearPickList()
        {
            try
            {
                if (txtPickSearch != null) txtPickSearch.Clear();
                if (lstPickItems != null)
                {
                    lstPickItems.BeginUpdate();
                    lstPickItems.Items.Clear();
                    lstPickItems.EndUpdate();
                }
                if (lblPickCount != null) lblPickCount.Text = "Kategori seçin veya isim arayın...";
            }
            catch { }
        }

        private void SavePickFilter()
        {
            try
            {
                int saved = 0;
                foreach (ListViewItem item in lstPickItems.Items)
                {
                    try
                    {
                        string servername = item.Tag as string;
                        if (string.IsNullOrEmpty(servername)) continue;
                        bool pick = GetFlag(item, 4), pet = GetFlag(item, 5), sell = GetFlag(item, 6);
                        bool store = GetFlag(item, 7), guild = GetFlag(item, 8);
                        bool take = GetFlag(item, 9), takeGuild = GetFlag(item, 10);
                        if (pick || pet || sell || store || guild || take || takeGuild)
                        {
                            ItemFilterManager.SetRuleFull(servername, pick, pet, sell, store, guild, take, takeGuild);
                            saved++;
                        }
                        else
                        {
                            ItemFilterManager.RemoveRule(servername);
                        }
                    }
                    catch { }
                }
                SavePickOptionsFromUi();
                SaveGoldFromUi();
                SaveDismantleFromUi();
                SaveBluesFromUi();
                try { Settings.SaveCharacterSettings(); } catch { }
                Log($"[Pick Filter] {saved} kural kaydedildi.");
            }
            catch (Exception ex)
            {
                try { Log("[Pick Filter] Kayıt hatası: " + ex.Message); } catch { }
            }
        }

        private static bool GetFlag(ListViewItem item, int col)
        {
            try
            {
                if (col < item.SubItems.Count)
                    return string.Equals(item.SubItems[col].Text, "Yes", StringComparison.OrdinalIgnoreCase);
            }
            catch { }
            return false;
        }

        private void ResetPickFilter()
        {
            try
            {
                ItemFilterManager.Reset();
                try { ClearPickList(); } catch { }
                LoadPickFilterSettingsToUi();
                try { Settings.SaveCharacterSettings(); } catch { }
                Log("[Pick Filter] Yapılandırma sıfırlandı.");
            }
            catch { }
        }

        #endregion

        #region Tab: Options

        private void LoadPickFilterSettingsToUi()
        {
            if (loadingPickFilterUi)
                return;

            loadingPickFilterUi = true;
            try
            {
                LoadPickOptionsToUi();
                LoadGoldToUi();
                LoadDismantleToUi();
                RefreshBluesList();
                RefreshDegreeList();
                if (lstPickItems != null && lstPickItems.Items.Count > 0)
                    RefreshPickList();
            }
            finally
            {
                loadingPickFilterUi = false;
            }
        }

        /// <summary>
        /// Tema checkbox yazısını owner-draw ile beyaza boyadığı için başlık
        /// ayrı Label'dadır (tema onu ezemez) — kutu sadece kutudur.
        /// </summary>
        private CheckBox AddOptCheck(Control parent, string text, int x, int y, int width)
        {
            var cbx = new CheckBox
            {
                Text = "",
                Location = new Point(x, y),
                Size = new Size(20, 19),
                AutoSize = false,
                UseVisualStyleBackColor = true
            };
            var lbl = new Label
            {
                Text = text,
                Location = new Point(x + 22, y),
                Size = new Size(Math.Max(40, width - 22), 19),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            lbl.Click += (s, e) => { try { cbx.Checked = !cbx.Checked; } catch { } };
            cbx.CheckedChanged += (s, e) => SaveAllPickFilterLive();
            parent.Controls.Add(cbx);
            parent.Controls.Add(lbl);
            return cbx;
        }

        private void BuildPickOptionsTab(TabPage tab)
        {
            int x = 12, y = 12, w = 310, step = 22;
            optPickItemsFirst = AddOptCheck(tab, "Pick items first", x, y, w); y += step;
            optUsePickPet = AddOptCheck(tab, "Use pick pet", x, y, w); y += step;
            optPickOthers = AddOptCheck(tab, "Pick other player's items", x, y, w); y += step;
            optPickParty = AddOptCheck(tab, "Pick party items", x, y, w); y += step;
            optDontPickItems = AddOptCheck(tab, "Don't pick items", x, y, w); y += step;
            optAllowSellAll = AddOptCheck(tab, "Allow selling of all item types", x, y, w); y += step;
            optOnlyPickRareBlue = AddOptCheck(tab, "Only pick rare or blue items", x, y, w); y += step;
            optOnlyStoreRareBlue = AddOptCheck(tab, "Only store rare or blue items", x, y, w); y += step;

            optOnlyStorePlus = AddOptCheck(tab, "Only store items with plus >=", x, y, 220);
            nudOnlyStorePlus = new NumericUpDown { Location = new Point(x + 225, y - 1), Size = new Size(50, 22), Minimum = 0, Maximum = 15, Value = 0 };
            nudOnlyStorePlus.ValueChanged += (s, e) => SaveAllPickFilterLive();
            tab.Controls.Add(nudOnlyStorePlus);
            y += step;

            optPickEvenWhenFull = AddOptCheck(tab, "Pick even when inventory is full", x, y, w); y += step;
            optPickCharIfPetFull = AddOptCheck(tab, "Pick with character if pet is unsummoned or full", x, y, 320); y += step;
            optDontMovePetItems = AddOptCheck(tab, "Do not move pet items except for storing/selling", x, y, 320); y += step;

            optNoSellPlus = AddOptCheck(tab, "Do not sell items with plus >=", x, y, 220);
            nudNoSellPlus = new NumericUpDown { Location = new Point(x + 225, y - 1), Size = new Size(50, 22), Minimum = 0, Maximum = 15, Value = 0 };
            nudNoSellPlus.ValueChanged += (s, e) => SaveAllPickFilterLive();
            tab.Controls.Add(nudNoSellPlus);

            int gbH = Math.Max(305, tab.Height - 16);
            var gbxBlues = new GroupBox
            {
                Text = "Blues",
                Location = new Point(345, 8),
                Size = new Size(280, gbH),
                Font = PhBotFont(),
                ForeColor = Color.Black
            };
            lstBlues = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                BackColor = SystemColors.Window,
                ForeColor = SystemColors.WindowText,
                Location = new Point(8, 20),
                Size = new Size(264, gbH - 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lstBlues.Columns.Add("Name", 175);
            lstBlues.Columns.Add("Store", 80);
            var ctx = new ContextMenuStrip();
            var miYes = new ToolStripMenuItem("Yes");
            var miNo = new ToolStripMenuItem("No");
            var miReset = new ToolStripMenuItem("Reset");
            miYes.Click += (s, e) => ApplyBluesFlag("Yes");
            miNo.Click += (s, e) => ApplyBluesFlag("No");
            miReset.Click += (s, e) => ApplyBluesFlag("Reset");
            ctx.Items.AddRange(new ToolStripItem[] { miYes, miNo, miReset });
            lstBlues.ContextMenuStrip = ctx;
            lstBlues.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = lstBlues.HitTest(e.Location);
                    bluesCtxColumn = hit.Item != null ? hit.SubItem != null ? hit.Item.SubItems.IndexOf(hit.SubItem) : -1 : -1;
                }
            };
            gbxBlues.Controls.Add(lstBlues);
            tab.Controls.Add(gbxBlues);
        }

        private void ApplyBluesFlag(string value)
        {
            try
            {
                if (lstBlues.SelectedItems.Count == 0) return;
                foreach (ListViewItem item in lstBlues.SelectedItems)
                {
                    if (item.SubItems.Count > 1)
                        item.SubItems[1].Text = value == "Reset" ? "No" : value;
                }
                SaveAllPickFilterLive();
            }
            catch { }
        }

        private void RefreshBluesList()
        {
            try
            {
                if (lstBlues == null) return;
                var rows = DataManager.QueryBlueOptions();
                var saved = ItemFilterManager.GetBlues();
                var entries = new List<KeyValuePair<string, string>>();
                foreach (var row in rows)
                {
                    try
                    {
                        string sn = row["servername"] ?? "";
                        if (string.IsNullOrEmpty(sn)) continue;
                        string display = ItemFilterManager.GetBlueDisplayName(sn, row["name"]);
                        entries.Add(new KeyValuePair<string, string>(sn, display));
                    }
                    catch { }
                }
                entries.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.OrdinalIgnoreCase));
                lstBlues.BeginUpdate();
                lstBlues.Items.Clear();
                foreach (var entry in entries)
                {
                    try
                    {
                        string store = "No";
                        BlueAttributeRule rule;
                        if (saved != null && saved.TryGetValue(entry.Key, out rule) && rule != null && rule.Store)
                            store = "Yes";
                        var item = new ListViewItem(new string[] { entry.Value, store });
                        item.Tag = entry.Key;
                        lstBlues.Items.Add(item);
                    }
                    catch { }
                }
                lstBlues.EndUpdate();
                bluesLoaded = lstBlues.Items.Count > 0;
            }
            catch { }
        }

        private bool bluesLoaded = false;

        private void SaveBluesFromUi()
        {
            try
            {
                if (lstBlues == null || !bluesLoaded) return;
                ItemFilterManager.ClearBlues();
                foreach (ListViewItem item in lstBlues.Items)
                {
                    try
                    {
                        string sn = item.Tag as string;
                        if (string.IsNullOrEmpty(sn)) continue;
                        string display = item.SubItems.Count > 0 ? item.SubItems[0].Text : sn;
                        bool store = item.SubItems.Count > 1 && string.Equals(item.SubItems[1].Text, "Yes", StringComparison.OrdinalIgnoreCase);
                        if (store)
                            ItemFilterManager.SetBlue(sn, display, true);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void LoadPickOptionsToUi()
        {
            try
            {
                var o = ItemFilterManager.Pick;
                if (o == null || optPickItemsFirst == null) return;
                if (optDontPickItems != null) optDontPickItems.Checked = o.DontPickItems;
                optPickItemsFirst.Checked = o.PickItemsFirst;
                optUsePickPet.Checked = o.UsePickPet;
                optPickOthers.Checked = o.PickOthersItems;
                optPickParty.Checked = o.PickPartyItems;
                optAllowSellAll.Checked = o.AllowSellAll;
                optOnlyPickRareBlue.Checked = o.OnlyPickRareBlue;
                optOnlyStoreRareBlue.Checked = o.OnlyStoreRareBlue;
                optPickCharIfPetFull.Checked = o.PickWithCharIfPetGoneFull;
                optDontMovePetItems.Checked = o.DontMovePetItemsExceptStoreSell;
                optOnlyStorePlus.Checked = o.OnlyStorePlusEnabled;
                try { nudOnlyStorePlus.Value = Math.Max(0, Math.Min(15, o.OnlyStorePlus)); } catch { }
                optNoSellPlus.Checked = o.NoSellPlusEnabled;
                try { nudNoSellPlus.Value = Math.Max(0, Math.Min(15, o.NoSellPlus)); } catch { }
                optPickEvenWhenFull.Checked = o.PickEvenWhenFull;
            }
            catch { }
        }

        private void SavePickOptionsFromUi()
        {
            try
            {
                var o = ItemFilterManager.Pick;
                if (o == null || optPickItemsFirst == null) return;
                if (optDontPickItems != null)
                {
                    o.DontPickItems = optDontPickItems.Checked;
                    o.Enabled = !optDontPickItems.Checked;
                }
                o.PickItemsFirst = optPickItemsFirst.Checked;
                o.UsePickPet = optUsePickPet.Checked;
                o.PickOthersItems = optPickOthers.Checked;
                o.PickPartyItems = optPickParty.Checked;
                o.AllowSellAll = optAllowSellAll.Checked;
                o.OnlyPickRareBlue = optOnlyPickRareBlue.Checked;
                o.OnlyStoreRareBlue = optOnlyStoreRareBlue.Checked;
                o.PickWithCharIfPetGoneFull = optPickCharIfPetFull.Checked;
                o.DontMovePetItemsExceptStoreSell = optDontMovePetItems.Checked;
                o.OnlyStorePlusEnabled = optOnlyStorePlus.Checked;
                try { o.OnlyStorePlus = (int)nudOnlyStorePlus.Value; } catch { }
                o.NoSellPlusEnabled = optNoSellPlus.Checked;
                try { o.NoSellPlus = (int)nudNoSellPlus.Value; } catch { }
                o.PickEvenWhenFull = optPickEvenWhenFull.Checked;
            }
            catch { }
        }

        #endregion

        #region Tab: Store Gold

        private void BuildStoreGoldTab(TabPage tab)
        {
            int x = 12, y = 16;
            optGoldEnabled = AddOptCheck(tab, "Gold keep amount", x, y, 390);
            txtGoldKeep = new TextBox { Location = new Point(420, y), Size = new Size(150, 22), Text = "1000000" };
            txtGoldKeep.TextChanged += (s, e) => SaveAllPickFilterLive();
            y += 32;
            optGoldTakeStorage = AddOptCheck(tab, "Take gold from storage", x, y, 300);
            y += 32;
            optGoldTakeGuild = AddOptCheck(tab, "Take gold from guild storage", x, y, 300);
            y += 32;
            optGoldStoreStorage = AddOptCheck(tab, "Store gold in storage (maximum amount, 0 = no limit)", x, y, 400);
            txtGoldStoreMax = new TextBox { Location = new Point(420, y), Size = new Size(150, 22), Text = "0" };
            txtGoldStoreMax.TextChanged += (s, e) => SaveAllPickFilterLive();
            y += 32;
            optGoldStoreGuild = AddOptCheck(tab, "Store gold in guild storage (maximum amount, 0 = no limit)", x, y, 400);
            txtGoldStoreGuildMax = new TextBox { Location = new Point(420, y), Size = new Size(150, 22), Text = "0" };
            txtGoldStoreGuildMax.TextChanged += (s, e) => SaveAllPickFilterLive();
            tab.Controls.AddRange(new Control[] { txtGoldKeep, txtGoldStoreMax, txtGoldStoreGuildMax });
        }

        private static ulong ParseGold(string text, ulong def)
        {
            try
            {
                ulong v;
                if (ulong.TryParse((text ?? "").Trim(), out v))
                    return v;
            }
            catch { }
            return def;
        }

        private void LoadGoldToUi()
        {
            try
            {
                var g = ItemFilterManager.StoreGold;
                if (g == null || optGoldEnabled == null) return;
                optGoldEnabled.Checked = g.Enabled;
                txtGoldKeep.Text = g.GoldKeepAmount.ToString();
                optGoldTakeStorage.Checked = g.TakeGoldFromStorage;
                optGoldTakeGuild.Checked = g.TakeGoldFromGuildStorage;
                optGoldStoreStorage.Checked = g.StoreGoldInStorage;
                txtGoldStoreMax.Text = g.StoreGoldMax.ToString();
                optGoldStoreGuild.Checked = g.StoreGoldInGuildStorage;
                txtGoldStoreGuildMax.Text = g.StoreGoldGuildMax.ToString();
            }
            catch { }
        }

        private void SaveGoldFromUi()
        {
            try
            {
                var g = ItemFilterManager.StoreGold;
                if (g == null || optGoldEnabled == null) return;
                g.TakeGoldFromStorage = optGoldTakeStorage.Checked;
                g.TakeGoldFromGuildStorage = optGoldTakeGuild.Checked;
                g.StoreGoldInStorage = optGoldStoreStorage.Checked;
                g.StoreGoldInGuildStorage = optGoldStoreGuild.Checked;
                g.Enabled = optGoldEnabled.Checked;
                g.GoldKeepAmount = ParseGold(txtGoldKeep.Text, 1000000);
                g.StoreGoldMax = ParseGold(txtGoldStoreMax.Text, 0);
                g.StoreGoldGuildMax = ParseGold(txtGoldStoreGuildMax.Text, 0);
            }
            catch { }
        }

        #endregion

        #region Tab: Dismantle

        private void BuildDismantleTab(TabPage tab)
        {
            int H = Math.Max(305, tab.Height - 16);

            // GroupBox 1: Degree
            var gbDegree = new GroupBox
            {
                Text = "Degree",
                Location = new Point(8, 8),
                Size = new Size(185, H),
                Font = PhBotFont(),
                ForeColor = Color.Black
            };
            lstDegrees = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                BackColor = SystemColors.Window,
                ForeColor = SystemColors.WindowText,
                Location = new Point(8, 20),
                Size = new Size(169, H - 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lstDegrees.Columns.Add("Degree", 80);
            lstDegrees.Columns.Add("Dismantle", 80);
            var ctx = new ContextMenuStrip();
            var miYes = new ToolStripMenuItem("Yes");
            var miNo = new ToolStripMenuItem("No");
            var miReset = new ToolStripMenuItem("Reset");
            miYes.Click += (s, e) => ApplyDegreeFlag("Yes");
            miNo.Click += (s, e) => ApplyDegreeFlag("No");
            miReset.Click += (s, e) => ApplyDegreeFlag("Reset");
            ctx.Items.AddRange(new ToolStripItem[] { miYes, miNo, miReset });
            lstDegrees.ContextMenuStrip = ctx;
            lstDegrees.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = lstDegrees.HitTest(e.Location);
                    degreeCtxColumn = hit.Item != null ? hit.SubItem != null ? hit.Item.SubItems.IndexOf(hit.SubItem) : -1 : -1;
                }
            };
            gbDegree.Controls.Add(lstDegrees);

            // GroupBox 2: Dismantle
            var gbDis = new GroupBox
            {
                Text = "Dismantle",
                Location = new Point(201, 8),
                Size = new Size(205, H),
                Font = PhBotFont(),
                ForeColor = Color.Black
            };
            optDisWhite = AddOptCheck(gbDis, "White items", 12, 24, 180);
            optDisPlussed = AddOptCheck(gbDis, "Plussed items <", 12, 54, 120);
            nudDisPlussed = new NumericUpDown { Location = new Point(136, 52), Size = new Size(55, 22), Minimum = 0, Maximum = 15, Value = 3 };
            nudDisPlussed.ValueChanged += (s, e) => SaveAllPickFilterLive();
            gbDis.Controls.Add(nudDisPlussed);
            optDisBlue = AddOptCheck(gbDis, "Blue items", 12, 84, 180);
            optDisRare = AddOptCheck(gbDis, "Rare items", 12, 114, 180);

            // GroupBox 3: Auto dismantle before...
            var gbAuto = new GroupBox
            {
                Text = "Auto dismantle before...",
                Location = new Point(414, 8),
                Size = new Size(210, H),
                Font = PhBotFont(),
                ForeColor = Color.Black
            };
            optDisBeforeSmith = AddOptCheck(gbAuto, "Blacksmith", 12, 24, 180);
            optDisBeforeGrocery = AddOptCheck(gbAuto, "Grocery trader", 12, 54, 180);
            optDisBeforeHerbalist = AddOptCheck(gbAuto, "Herbalist", 12, 84, 180);
            optDisBeforeStorage = AddOptCheck(gbAuto, "Storage", 12, 114, 180);
            optDisBeforeGuild = AddOptCheck(gbAuto, "Guild storage", 12, 144, 180);

            tab.Controls.AddRange(new Control[] { gbDegree, gbDis, gbAuto });
        }

        private void ApplyDegreeFlag(string value)
        {
            try
            {
                if (lstDegrees.SelectedItems.Count == 0) return;
                foreach (ListViewItem item in lstDegrees.SelectedItems)
                {
                    if (item.SubItems.Count > 1)
                        item.SubItems[1].Text = value == "Reset" ? "No" : value;
                }
                SaveAllPickFilterLive();
            }
            catch { }
        }

        private void RefreshDegreeList()
        {
            try
            {
                if (lstDegrees == null) return;
                lstDegrees.BeginUpdate();
                lstDegrees.Items.Clear();
                for (int d = 1; d <= 12; d++)
                {
                    string flag = ItemFilterManager.Dismantle.Degrees.Contains(d) ? "Yes" : "No";
                    lstDegrees.Items.Add(new ListViewItem(new string[] { d.ToString(), flag }));
                }
                lstDegrees.EndUpdate();
            }
            catch { }
        }

        private void LoadDismantleToUi()
        {
            try
            {
                var d = ItemFilterManager.Dismantle;
                if (d == null || optDisWhite == null) return;
                optDisWhite.Checked = d.WhiteItems;
                optDisPlussed.Checked = d.PlussedBelowEnabled;
                try { nudDisPlussed.Value = Math.Max(0, Math.Min(15, d.PlussedBelow)); } catch { }
                optDisBlue.Checked = d.BlueItems;
                optDisRare.Checked = d.RareItems;
                optDisBeforeSmith.Checked = d.BeforeBlacksmith;
                optDisBeforeGrocery.Checked = d.BeforeGrocery;
                optDisBeforeHerbalist.Checked = d.BeforeHerbalist;
                optDisBeforeStorage.Checked = d.BeforeStorage;
                optDisBeforeGuild.Checked = d.BeforeGuildStorage;
            }
            catch { }
        }

        private void SaveDismantleFromUi()
        {
            try
            {
                var d = ItemFilterManager.Dismantle;
                if (d == null || optDisWhite == null) return;
                d.Degrees.Clear();
                if (lstDegrees != null)
                {
                    foreach (ListViewItem item in lstDegrees.Items)
                    {
                        try
                        {
                            int deg = int.Parse(item.SubItems[0].Text);
                            if (item.SubItems.Count > 1 && string.Equals(item.SubItems[1].Text, "Yes", StringComparison.OrdinalIgnoreCase))
                                d.Degrees.Add(deg);
                        }
                        catch { }
                    }
                }
                d.WhiteItems = optDisWhite.Checked;
                d.PlussedBelowEnabled = optDisPlussed.Checked;
                try { d.PlussedBelow = (int)nudDisPlussed.Value; } catch { }
                d.BlueItems = optDisBlue.Checked;
                d.RareItems = optDisRare.Checked;
                d.BeforeBlacksmith = optDisBeforeSmith.Checked;
                d.BeforeGrocery = optDisBeforeGrocery.Checked;
                d.BeforeHerbalist = optDisBeforeHerbalist.Checked;
                d.BeforeStorage = optDisBeforeStorage.Checked;
                d.BeforeGuildStorage = optDisBeforeGuild.Checked;
            }
            catch { }
        }

        #endregion
    }
}
