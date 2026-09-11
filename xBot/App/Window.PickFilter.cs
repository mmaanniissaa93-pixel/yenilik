using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game;

namespace xBot.App
{
    /// <summary>
    /// phBot tarzı Toplama Filtresi sekmeleri:
    /// 1. Toplama Filtresi  2. Ez Filter  3. Ayarlar  4. Altın Aktarımı  5. Kırdırma
    /// </summary>
    public partial class Window
    {
        private TabControl tabPickFilterRoot;
        private ListView lstPickItems;
        private ComboBox cmbPickGroup;
        private ComboBox cmbPickRace;
        private ComboBox cmbPickDegree;
        private ComboBox cmbPickGender;
        private TextBox txtPickSearch;
        private Label lblPickCount;
        private int pickCtxColumn = -1;
        private bool loadingPickFilterUi;

        // Ez Filter Checkboxes
        private readonly Dictionary<EzFilterCategory, (CheckBox Pick, CheckBox PetPick, CheckBox Sell, CheckBox Store, CheckBox GuildStore)> ezChecks =
            new Dictionary<EzFilterCategory, (CheckBox Pick, CheckBox PetPick, CheckBox Sell, CheckBox Store, CheckBox GuildStore)>();

        // Ayarlar (Options)
        private CheckBox optPickItemsFirst;
        private CheckBox optUsePickPet;
        private CheckBox optPickOthers;
        private CheckBox optPickParty;
        private CheckBox optDontPickItems;
        private CheckBox optAllowSellAll;
        private CheckBox optOnlyPickRareBlue;
        private CheckBox optOnlyStoreRareBlue;
        private CheckBox optOnlyStorePlus;
        private NumericUpDown nudOnlyStorePlus;
        private CheckBox optPickEvenWhenFull;
        private CheckBox optPickCharIfPetFull;
        private CheckBox optDontMovePetItems;
        private CheckBox optNoSellPlus;
        private NumericUpDown nudNoSellPlus;
        private CheckBox optSellSelectedBlues;
        private CheckBox optDontPickGreyBar;

        private ListView lstBlues;
        private int bluesCtxColumn = -1;
        private bool bluesLoaded = false;

        // Altın Aktarımı
        private CheckBox optGoldEnabled;
        private TextBox txtGoldKeep;
        private CheckBox optGoldTakeStorage;
        private CheckBox optGoldTakeGuild;
        private CheckBox optGoldStoreStorage;
        private TextBox txtGoldStoreMax;
        private CheckBox optGoldStoreGuild;
        private TextBox txtGoldStoreGuildMax;

        // Kırdırma
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
            "--", "Armor", "Silahlar", "Kalkan", "Aksesuar", "Avatar",
            "Potlar", "Pill", "Scroll", "Ok/Mermi", "Altın", "Kervan",
            "Görev", "Elixir", "Simya", "Taşıma/Pet", "Diğer"
        };

        public void BuildPickFilterTabs()
        {
            try
            {
                if (TabPageH_Town_Option03_Panel == null)
                    return;

                try
                {
                    if (Filter_gbxPick != null) Filter_gbxPick.Visible = false;
                    var old1 = TabPageH_Town_Option03_Panel.Controls.Find("gbxItemFilterRules", false);
                    var old2 = TabPageH_Town_Option03_Panel.Controls.Find("gbxItemFilterRulesCustom", false);
                    foreach (var c in old1) c.Visible = false;
                    foreach (var c in old2) c.Visible = false;
                }
                catch { }

                try
                {
                    if (TabPageH_Town_Option01 != null) TabPageH_Town_Option01.Visible = false;
                    if (TabPageH_Town_Option01_Panel != null) TabPageH_Town_Option01_Panel.Visible = false;
                }
                catch { }

                tabPickFilterRoot = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font = PhBotFont()
                };

                bool isTR = LocalizationManager.CurrentLanguage == "TR";

                var tabItems = new TabPage(isTR ? "Toplama Filtresi" : "Pick Filter");
                var tabEz = new TabPage("Ez Filter");
                var tabOptions = new TabPage(isTR ? "Ayarlar" : "Options");
                var tabGold = new TabPage(isTR ? "Altın Aktarımı" : "Store Gold");
                var tabDismantle = new TabPage(isTR ? "Kırdırma" : "Dismantle");

                tabPickFilterRoot.TabPages.AddRange(new TabPage[] { tabItems, tabEz, tabOptions, tabGold, tabDismantle });

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
                BuildEzFilterTab(tabEz);
                BuildPickOptionsTab(tabOptions);
                BuildStoreGoldTab(tabGold);
                BuildDismantleTab(tabDismantle);

                TabPageH_Town_Option03_Panel.Controls.Add(tabPickFilterRoot);
                tabPickFilterRoot.BringToFront();

                tabPickFilterRoot.VisibleChanged += (s, e) =>
                {
                    try { if (tabPickFilterRoot.Visible) ApplyPickFilterLightTheme(); } catch { }
                };

                LoadPickFilterSettingsToUi();
            }
            catch (Exception ex)
            {
                try { Log("[PickFilter UI] " + ex.Message); } catch { }
            }
        }

        public void ApplyPickFilterLightTheme()
        {
            try
            {
                if (tabPickFilterRoot == null) return;
                foreach (TabPage page in tabPickFilterRoot.TabPages)
                {
                    page.BackColor = SystemColors.Control;
                    page.ForeColor = SystemColors.ControlText;
                    ApplyLightThemeRecursive(page);
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
                    if (c is ListView lv)
                    {
                        lv.BackColor = SystemColors.Window;
                        lv.ForeColor = SystemColors.WindowText;
                    }
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
                    else if (c is TextBox || c is ComboBox || c is NumericUpDown)
                    {
                        c.BackColor = SystemColors.Window;
                        c.ForeColor = SystemColors.WindowText;
                    }
                    else if (c is TabPage || c is TabControl || c is Panel || c is GroupBox)
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

        #region Tab 1: Toplama Filtresi (Items)

        private void BuildPickItemsTab(TabPage tab)
        {
            bool isTR = LocalizationManager.CurrentLanguage == "TR";

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
            lstPickItems.Columns.Add("No", 45);
            lstPickItems.Columns.Add("Resim", 45);
            lstPickItems.Columns.Add("Adı", 210);
            lstPickItems.Columns.Add("Seviye", 45);
            lstPickItems.Columns.Add("Topla", 45);
            lstPickItems.Columns.Add("Pet", 45);
            lstPickItems.Columns.Add("Sat", 45);
            lstPickItems.Columns.Add("Depola", 50);
            lstPickItems.Columns.Add("Guilde aktar", 75);
            lstPickItems.Columns.Add("Depoya Al", 70);

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

            lstPickItems.Dock = DockStyle.Fill;

            var footer = new Panel
            {
                Name = "XBotPickSearch",
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White
            };

            cmbPickGroup = new ComboBox { Location = new Point(6, 6), Size = new Size(95, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickGroup.Items.AddRange(PickGroups);
            cmbPickGroup.SelectedIndex = 0;
            cmbPickGroup.SelectedIndexChanged += (s, e) => RefreshPickList();

            cmbPickRace = new ComboBox { Location = new Point(105, 6), Size = new Size(70, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickRace.Items.AddRange(new string[] { "--", isTR ? "Çin" : "Chinese", isTR ? "Avrupa" : "European" });
            cmbPickRace.SelectedIndex = 0;
            cmbPickRace.SelectedIndexChanged += (s, e) => RefreshPickList();

            cmbPickDegree = new ComboBox { Location = new Point(179, 6), Size = new Size(75, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickDegree.Items.Add(isTR ? "Herhangi" : "Any");
            for (int d = 1; d <= 12; d++) cmbPickDegree.Items.Add(d.ToString());
            cmbPickDegree.SelectedIndex = 0;
            cmbPickDegree.SelectedIndexChanged += (s, e) => RefreshPickList();

            cmbPickGender = new ComboBox { Location = new Point(258, 6), Size = new Size(75, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPickGender.Items.AddRange(new string[] { isTR ? "Herhangi" : "Any", isTR ? "Erkek" : "Male", isTR ? "Kadın" : "Female" });
            cmbPickGender.SelectedIndex = 0;
            cmbPickGender.SelectedIndexChanged += (s, e) => RefreshPickList();

            txtPickSearch = new TextBox
            {
                Location = new Point(338, 6),
                Size = new Size(295, 22),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            txtPickSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { RefreshPickList(); e.Handled = true; e.SuppressKeyPress = true; } };

            lblPickCount = new Label { Location = new Point(513, 9), Size = new Size(128, 18), ForeColor = SystemColors.GrayText, Text = "", Visible = false };

            var btnClear = new Button { Text = isTR ? "Temizle" : "Clear", Location = new Point(408, 33), Size = new Size(72, 26), UseVisualStyleBackColor = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var btnUpdate = new Button { Text = isTR ? "Güncelle" : "Update", Location = new Point(484, 33), Size = new Size(72, 26), UseVisualStyleBackColor = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var btnReset = new Button { Text = isTR ? "Sıfırla" : "Reset", Location = new Point(560, 33), Size = new Size(72, 26), UseVisualStyleBackColor = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnClear.Click += (s, e) => { try { ClearPickList(); } catch { } };
            btnUpdate.Click += (s, e) => SavePickFilter();
            btnReset.Click += (s, e) => ResetPickFilter();

            var lblNote = new Label
            {
                Location = new Point(6, 36),
                Size = new Size(396, 24),
                ForeColor = Color.DimGray,
                Font = PhBotFont(),
                Text = "* SOX eşyalar satılmayacak, ve birincil/ikincil silahlar satılmayacak/depolanmayacak",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            footer.Resize += (s, e) =>
            {
                try
                {
                    if (txtPickSearch != null)
                        txtPickSearch.Width = Math.Max(100, footer.Width - 344);
                    btnClear.Location = new Point(footer.Width - 228, 33);
                    btnUpdate.Location = new Point(footer.Width - 152, 33);
                    btnReset.Location = new Point(footer.Width - 76, 33);
                    lblNote.Width = Math.Max(180, footer.Width - 235);
                }
                catch { }
            };

            footer.Controls.AddRange(new Control[] {
                cmbPickGroup, cmbPickRace, cmbPickDegree, cmbPickGender,
                txtPickSearch, lblPickCount, btnClear, btnUpdate, btnReset, lblNote });

            tab.Controls.Add(lstPickItems);
            tab.Controls.Add(footer);
            lstPickItems.BringToFront();
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
                        for (int c = 4; c < item.SubItems.Count; c++)
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
                bool take = GetFlag(item, 9);
                if (pick || pet || sell || store || guild || take)
                    ItemFilterManager.SetRuleFull(servername, pick, pet, sell, store, guild, take, false);
                else
                    ItemFilterManager.RemoveRule(servername);
            }
            catch { }
        }

        private static string MapTurkishGroupToDb(string g)
        {
            if (string.IsNullOrEmpty(g) || g == "--") return "All";
            switch (g)
            {
                case "Silahlar": return "Weapon";
                case "Kalkan": return "Shield";
                case "Aksesuar": return "Accessory";
                case "Potlar": return "Potion";
                case "Ok/Mermi": return "Ammo";
                case "Altın": return "Gold";
                case "Kervan": return "TradeGoods";
                case "Görev": return "Quest";
                case "Simya": return "AlchemyMaterial";
                case "Taşıma/Pet": return "CosTransport";
                case "Diğer": return "Other";
                default: return g;
            }
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
                    if (cmbPickGroup != null && cmbPickGroup.SelectedItem != null) group = MapTurkishGroupToDb(cmbPickGroup.SelectedItem.ToString());
                    if (cmbPickRace != null && cmbPickRace.SelectedIndex > 0) race = cmbPickRace.SelectedIndex == 1 ? "Chinese" : "European";
                    if (cmbPickGender != null && cmbPickGender.SelectedIndex > 0) gender = cmbPickGender.SelectedIndex == 1 ? "Male" : "Female";
                    if (cmbPickDegree != null && cmbPickDegree.SelectedIndex > 0) degree = cmbPickDegree.SelectedIndex;
                    if (txtPickSearch != null) search = (txtPickSearch.Text ?? "").Trim();
                }
                catch { }

                bool narrowed = !string.Equals(group, "All", StringComparison.Ordinal)
                    || !string.Equals(race, "All", StringComparison.Ordinal)
                    || !string.Equals(gender, "Any", StringComparison.Ordinal)
                    || degree > 0
                    || search.Length >= 2;

                if (!narrowed)
                {
                    lstPickItems.BeginUpdate();
                    lstPickItems.Items.Clear();
                    lstPickItems.EndUpdate();
                    return;
                }

                List<NameValueCollection> rows = DataManager.QueryItems(group, race, gender, degree, search);
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
                        string pick = "No", pet = "No", sell = "No", store = "No", guild = "No", take = "No";
                        if (rule != null)
                        {
                            pick = rule.Pickup ? "Yes" : "No";
                            pet = rule.Pet ? "Yes" : "No";
                            sell = rule.Sell ? "Yes" : "No";
                            store = rule.Store ? "Yes" : "No";
                            guild = rule.StoreGuild ? "Yes" : "No";
                            take = rule.TakeStorage ? "Yes" : "No";
                        }
                        var item = new ListViewItem(new string[] { id, "", name, level, pick, pet, sell, store, guild, take });
                        item.Tag = servername;
                        lstPickItems.Items.Add(item);
                    }
                    catch { }
                }
                lstPickItems.EndUpdate();
                try { rows.Clear(); } catch { }
            }
            catch { }
        }

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
                        bool take = GetFlag(item, 9);
                        if (pick || pet || sell || store || guild || take)
                        {
                            ItemFilterManager.SetRuleFull(servername, pick, pet, sell, store, guild, take, false);
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
                Log($"[Toplama Filtresi] {saved} kural kaydedildi.");
            }
            catch { }
        }

        private void ResetPickFilter()
        {
            try
            {
                ItemFilterManager.Reset();
                try { ClearPickList(); } catch { }
                LoadPickFilterSettingsToUi();
                try { Settings.SaveCharacterSettings(); } catch { }
                Log("[Toplama Filtresi] Yapılandırma sıfırlandı.");
            }
            catch { }
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

        #endregion

        #region Tab 2: Ez Filter

        private static readonly (EzFilterCategory Cat, string Display)[] EzCategories = new[]
        {
            (EzFilterCategory.Armor, "Armor"),
            (EzFilterCategory.Weapons, "Silahlar"),
            (EzFilterCategory.HP, "HP"),
            (EzFilterCategory.MP, "MP"),
            (EzFilterCategory.Vigor, "Vigorlar"),
            (EzFilterCategory.Universal, "Universal"),
            (EzFilterCategory.Purification, "Purification"),
            (EzFilterCategory.Elixirs, "Elixirler"),
            (EzFilterCategory.Materials, "Materyaller"),
            (EzFilterCategory.Elements, "Elementler"),
            (EzFilterCategory.Tablets, "Tabletler"),
            (EzFilterCategory.Stones, "Stoneler"),
            (EzFilterCategory.Quest, "Görev")
        };

        private void BuildEzFilterTab(TabPage tab)
        {
            bool isTR = LocalizationManager.CurrentLanguage == "TR";

            int colX0 = 15;
            int colX1 = 145; // Topla
            int colX2 = 255; // pet pick
            int colX3 = 365; // Sat
            int colX4 = 475; // Depola
            int colX5 = 585; // Guild Deposu

            // Column Header Labels
            var lblH1 = new Label { Text = "Topla", Location = new Point(colX1 - 10, 12), AutoSize = true, Font = PhBotFont(), ForeColor = Color.Black };
            var lblH2 = new Label { Text = "pet pick", Location = new Point(colX2 - 12, 12), AutoSize = true, Font = PhBotFont(), ForeColor = Color.Black };
            var lblH3 = new Label { Text = "Sat", Location = new Point(colX3 - 5, 12), AutoSize = true, Font = PhBotFont(), ForeColor = Color.Black };
            var lblH4 = new Label { Text = "Depola", Location = new Point(colX4 - 10, 12), AutoSize = true, Font = PhBotFont(), ForeColor = Color.Black };
            var lblH5 = new Label { Text = "Guild Deposu", Location = new Point(colX5 - 20, 12), AutoSize = true, Font = PhBotFont(), ForeColor = Color.Black };
            tab.Controls.AddRange(new Control[] { lblH1, lblH2, lblH3, lblH4, lblH5 });

            int y = 36;
            ezChecks.Clear();

            foreach (var (cat, display) in EzCategories)
            {
                var lblRow = new Label
                {
                    Text = display,
                    Location = new Point(colX0, y + 2),
                    Size = new Size(120, 20),
                    Font = PhBotFont(),
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var cbPick = CreateEzCheckBox(colX1, y);
                var cbPet = CreateEzCheckBox(colX2, y);
                var cbSell = CreateEzCheckBox(colX3, y);
                var cbStore = CreateEzCheckBox(colX4, y);
                var cbGuild = CreateEzCheckBox(colX5, y);

                ezChecks[cat] = (cbPick, cbPet, cbSell, cbStore, cbGuild);

                tab.Controls.AddRange(new Control[] { lblRow, cbPick, cbPet, cbSell, cbStore, cbGuild });
                y += 24;
            }

            var btnClear = new Button { Text = isTR ? "Temizle" : "Clear", Location = new Point(515, y + 8), Size = new Size(75, 26), UseVisualStyleBackColor = true };
            var btnApply = new Button { Text = isTR ? "Onayla" : "Apply", Location = new Point(598, y + 8), Size = new Size(75, 26), UseVisualStyleBackColor = true };

            btnClear.Click += (s, e) =>
            {
                try
                {
                    foreach (var tuple in ezChecks.Values)
                    {
                        tuple.Pick.Checked = false;
                        tuple.PetPick.Checked = false;
                        tuple.Sell.Checked = false;
                        tuple.Store.Checked = false;
                        tuple.GuildStore.Checked = false;
                    }
                    EzFilterManager.Reset();
                    Settings.SaveCharacterSettings();
                    Log("[Ez Filter] Tüm filtreler temizlendi.");
                }
                catch { }
            };

            btnApply.Click += (s, e) =>
            {
                try
                {
                    SaveEzFilterFromUi();
                    Settings.SaveCharacterSettings();
                    Log("[Ez Filter] Ayarlar uygulandı ve kaydedildi.");
                }
                catch { }
            };

            tab.Controls.AddRange(new Control[] { btnClear, btnApply });
        }

        private CheckBox CreateEzCheckBox(int x, int y)
        {
            var cb = new CheckBox
            {
                Text = "",
                Location = new Point(x, y),
                Size = new Size(20, 20),
                UseVisualStyleBackColor = true
            };
            cb.CheckedChanged += (s, e) => SaveAllPickFilterLive();
            return cb;
        }

        private void LoadEzFilterToUi()
        {
            try
            {
                foreach (var (cat, _) in EzCategories)
                {
                    if (ezChecks.TryGetValue(cat, out var tuple))
                    {
                        var rule = EzFilterManager.GetRule(cat);
                        tuple.Pick.Checked = rule.Pick;
                        tuple.PetPick.Checked = rule.PetPick;
                        tuple.Sell.Checked = rule.Sell;
                        tuple.Store.Checked = rule.Store;
                        tuple.GuildStore.Checked = rule.GuildStore;
                    }
                }
            }
            catch { }
        }

        private void SaveEzFilterFromUi()
        {
            try
            {
                foreach (var (cat, _) in EzCategories)
                {
                    if (ezChecks.TryGetValue(cat, out var tuple))
                    {
                        EzFilterManager.SetRule(cat, tuple.Pick.Checked, tuple.PetPick.Checked, tuple.Sell.Checked, tuple.Store.Checked, tuple.GuildStore.Checked);
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Tab 3: Ayarlar (Options)

        private void BuildPickOptionsTab(TabPage tab)
        {
            int x = 12, y = 14, w = 310, step = 22;
            optPickItemsFirst = AddOptCheck(tab, "Önce eşyaları topla", x, y, w); y += step;
            optUsePickPet = AddOptCheck(tab, "Toplama peti kullan", x, y, w); y += step;
            optPickOthers = AddOptCheck(tab, "Diğer oyuncuların eşyalarını topla", x, y, w); y += step;
            optPickParty = AddOptCheck(tab, "Parti eşyalarını topla", x, y, w); y += step;
            optDontPickItems = AddOptCheck(tab, "Eşya toplamayı durdur", x, y, w); y += step;
            optAllowSellAll = AddOptCheck(tab, "Tüm eşya türlerini satmaya izin ver", x, y, w); y += step;
            optOnlyPickRareBlue = AddOptCheck(tab, "Sadece SOX ya da Blue eşyaları topla", x, y, w); y += step;
            optOnlyStoreRareBlue = AddOptCheck(tab, "Sadece SOX ya da Blue eşyaları depola", x, y, w); y += step;

            optOnlyStorePlus = AddOptCheck(tab, "Yalnızca artı içeren eşyaları depola >=", x, y, 230);
            nudOnlyStorePlus = new NumericUpDown { Location = new Point(x + 235, y - 1), Size = new Size(48, 22), Minimum = 0, Maximum = 15, Value = 0, Font = PhBotFont() };
            nudOnlyStorePlus.ValueChanged += (s, e) => SaveAllPickFilterLive();
            tab.Controls.Add(nudOnlyStorePlus);
            y += step;

            optPickEvenWhenFull = AddOptCheck(tab, "Ã‡anta dolu olsa bile topla", x, y, w); y += step;
            optPickCharIfPetFull = AddOptCheck(tab, "Pet çağırılmamış veya dolu ise karakter ile topla", x, y, 320); y += step;
            optDontMovePetItems = AddOptCheck(tab, "Pet eşyalarını depolamak veya satmak dışında taşımayın", x, y, 320); y += step;

            optNoSellPlus = AddOptCheck(tab, "Artı >= ile ürün satmayın", x, y, 200);
            nudNoSellPlus = new NumericUpDown { Location = new Point(x + 205, y - 1), Size = new Size(48, 22), Minimum = 0, Maximum = 15, Value = 0, Font = PhBotFont() };
            nudNoSellPlus.ValueChanged += (s, e) => SaveAllPickFilterLive();
            tab.Controls.Add(nudNoSellPlus);
            y += step;

            optSellSelectedBlues = AddOptCheck(tab, "Seçili blue'lu eşyaları sat", x, y, w);

            // Gri barda eşyaları toplama (üst ortada)
            optDontPickGreyBar = AddOptCheck(tab, "Gri barda eşyaları toplama", 270, 14, 200);

            // Right Box: Bluelar
            int gbH = Math.Max(305, tab.Height - 20);
            var gbxBlues = new GroupBox
            {
                Text = "Bluelar",
                Location = new Point(410, 8),
                Size = new Size(265, gbH),
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
                Size = new Size(249, gbH - 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lstBlues.Columns.Add("Adı", 135);
            lstBlues.Columns.Add("Depola", 55);
            lstBlues.Columns.Add("Sat", 55);

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
                if (lstBlues.SelectedItems.Count == 0 || bluesCtxColumn < 1) return;
                foreach (ListViewItem item in lstBlues.SelectedItems)
                {
                    if (bluesCtxColumn < item.SubItems.Count)
                        item.SubItems[bluesCtxColumn].Text = value == "Reset" ? "No" : value;
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
                        string store = "No", sell = "No";
                        BlueAttributeRule rule;
                        if (saved != null && saved.TryGetValue(entry.Key, out rule) && rule != null)
                        {
                            if (rule.Store) store = "Yes";
                            if (rule.Sell) sell = "Yes";
                        }
                        var item = new ListViewItem(new string[] { entry.Value, store, sell });
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
                        bool sell = item.SubItems.Count > 2 && string.Equals(item.SubItems[2].Text, "Yes", StringComparison.OrdinalIgnoreCase);
                        if (store || sell)
                            ItemFilterManager.SetBlue(sn, display, store, sell);
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
                if (optSellSelectedBlues != null) optSellSelectedBlues.Checked = o.SellSelectedBlues;
                if (optDontPickGreyBar != null) optDontPickGreyBar.Checked = o.DontPickGreyBar;
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
                if (optSellSelectedBlues != null) o.SellSelectedBlues = optSellSelectedBlues.Checked;
                if (optDontPickGreyBar != null) o.DontPickGreyBar = optDontPickGreyBar.Checked;
            }
            catch { }
        }

        #endregion

        #region Tab 4: Altın Aktarımı (Store Gold)

        private void BuildStoreGoldTab(TabPage tab)
        {
            int x = 14, y = 16;
            optGoldEnabled = AddOptCheck(tab, "Envanterinde altın tut (miktar)", x, y, 320);
            txtGoldKeep = new TextBox { Location = new Point(400, y), Size = new Size(130, 22), Text = "1000000", Font = PhBotFont() };
            txtGoldKeep.TextChanged += (s, e) => SaveAllPickFilterLive();
            y += 32;

            optGoldTakeStorage = AddOptCheck(tab, "Depodan altın al", x, y, 300);
            y += 32;

            optGoldTakeGuild = AddOptCheck(tab, "Guild deposundan altın al", x, y, 300);
            y += 32;

            optGoldStoreStorage = AddOptCheck(tab, "Depoya altın aktar (0 = sınır yok)", x, y, 350);
            txtGoldStoreMax = new TextBox { Location = new Point(400, y), Size = new Size(130, 22), Text = "0", Font = PhBotFont() };
            txtGoldStoreMax.TextChanged += (s, e) => SaveAllPickFilterLive();
            y += 32;

            optGoldStoreGuild = AddOptCheck(tab, "Guild deposuna altın aktar (0 = sınır yok)", x, y, 350);
            txtGoldStoreGuildMax = new TextBox { Location = new Point(400, y), Size = new Size(130, 22), Text = "0", Font = PhBotFont() };
            txtGoldStoreGuildMax.TextChanged += (s, e) => SaveAllPickFilterLive();

            tab.Controls.AddRange(new Control[] { txtGoldKeep, txtGoldStoreMax, txtGoldStoreGuildMax });
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

        #endregion

        #region Tab 5: Kırdırma (Dismantle)

        private void BuildDismantleTab(TabPage tab)
        {
            int H = Math.Max(305, tab.Height - 16);

            // GroupBox 1: Derece
            var gbDegree = new GroupBox
            {
                Text = "Derece",
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
            lstDegrees.Columns.Add("Derece", 80);
            lstDegrees.Columns.Add("Kırdırma", 80);

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

            // GroupBox 2: Kırdırma
            var gbDis = new GroupBox
            {
                Text = "Kırdırma",
                Location = new Point(201, 8),
                Size = new Size(205, H),
                Font = PhBotFont(),
                ForeColor = Color.Black
            };
            optDisWhite = AddOptCheck(gbDis, "Clean eşyalar", 12, 24, 180);
            optDisPlussed = AddOptCheck(gbDis, "Plussed öğeler <", 12, 54, 120);
            nudDisPlussed = new NumericUpDown { Location = new Point(136, 52), Size = new Size(55, 22), Minimum = 0, Maximum = 15, Value = 3, Font = PhBotFont() };
            nudDisPlussed.ValueChanged += (s, e) => SaveAllPickFilterLive();
            gbDis.Controls.Add(nudDisPlussed);
            optDisBlue = AddOptCheck(gbDis, "Blue'lu eşyalar", 12, 84, 180);
            optDisRare = AddOptCheck(gbDis, "Sox Eşyalar", 12, 114, 180);

            // GroupBox 3: NPC'den önce otomatik kırdır...
            var gbAuto = new GroupBox
            {
                Text = "NPC'den önce otomatik kırdır...",
                Location = new Point(414, 8),
                Size = new Size(210, H),
                Font = PhBotFont(),
                ForeColor = Color.Black
            };
            optDisBeforeSmith = AddOptCheck(gbAuto, "Demirci", 12, 24, 180);
            optDisBeforeGrocery = AddOptCheck(gbAuto, "Yüzükcü", 12, 54, 180);
            optDisBeforeHerbalist = AddOptCheck(gbAuto, "Şifacı", 12, 84, 180);
            optDisBeforeStorage = AddOptCheck(gbAuto, "Depo", 12, 114, 180);
            optDisBeforeGuild = AddOptCheck(gbAuto, "Guild deposu", 12, 144, 180);

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

        #region Common Helpers & Sync

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
                Font = PhBotFont(),
                TextAlign = ContentAlignment.MiddleLeft
            };
            lbl.Click += (s, e) => { try { cbx.Checked = !cbx.Checked; } catch { } };
            cbx.CheckedChanged += (s, e) => SaveAllPickFilterLive();
            parent.Controls.Add(cbx);
            parent.Controls.Add(lbl);
            return cbx;
        }

        private void LoadPickFilterSettingsToUi()
        {
            if (loadingPickFilterUi)
                return;

            loadingPickFilterUi = true;
            try
            {
                LoadPickOptionsToUi();
                LoadEzFilterToUi();
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

        private void SaveAllPickFilterLive()
        {
            try
            {
                if (loadingPickFilterUi)
                    return;
                SavePickOptionsFromUi();
                SaveEzFilterFromUi();
                SaveGoldFromUi();
                SaveDismantleFromUi();
                SaveBluesFromUi();
                try { Settings.SaveCharacterSettings(); } catch { }
            }
            catch { }
        }

        #endregion
    }
}