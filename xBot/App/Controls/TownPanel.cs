using System;
using System.Drawing;
using System.Windows.Forms;

namespace xBot.App.Controls
{
    /// <summary>
    /// Modüler şehir döngüsü, tedarik ve filtre paneli (TownPanel).
    /// Şehir lojistiği (tamir, depolama, çöp satışı, navmesh dönüşü), otomatik iksir/ok alımı
    /// ve yerdeki eşyaları filtreleme ayarlarını kapsüller.
    /// </summary>
    public class TownPanel : UserControl
    {
        public GroupBox LogisticsGroup { get; private set; }
        public GroupBox AutoBuyGroup { get; private set; }
        public GroupBox PickFilterGroup { get; private set; }

        // Logistics controls
        public CheckBox EnableTownLoopCheckBox { get; private set; }
        public CheckBox RepairCheckBox { get; private set; }
        public CheckBox StorageCheckBox { get; private set; }
        public CheckBox SellTrashCheckBox { get; private set; }
        public CheckBox ReturnNavMeshCheckBox { get; private set; }

        // AutoBuy controls
        public CheckBox AutoBuyCheckBox { get; private set; }
        public ComboBox HpTypeComboBox { get; private set; }
        public NumericUpDown HpAmountNumeric { get; private set; }
        public ComboBox MpTypeComboBox { get; private set; }
        public NumericUpDown MpAmountNumeric { get; private set; }
        public CheckBox BuyPillsCheckBox { get; private set; }

        // PickFilter controls
        public CheckBox PickGoldCheckBox { get; private set; }
        public CheckBox PickEquipCheckBox { get; private set; }
        public CheckBox PickElixirStoneCheckBox { get; private set; }
        public CheckBox PickMaterialsCheckBox { get; private set; }
        public CheckBox UsePetCheckBox { get; private set; }

        public event EventHandler TownSettingChanged;

        public TownPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.FromArgb(241, 241, 241);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.Size = new Size(657, 372);
            this.AutoScroll = true;

            // 1. Logistics Group
            LogisticsGroup = new GroupBox
            {
                Name = "Town_gbxLogistics",
                Text = "Şehir Döngüsü & Lojistik",
                Location = new Point(10, 10),
                Size = new Size(305, 170),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            int y = 22;
            const int step = 26;

            EnableTownLoopCheckBox = CreateCheck("Town_cbxEnableTownLoop", "Şehir döngüsünü etkinleştir", 15, y, 250);
            y += step;
            RepairCheckBox = CreateCheck("Town_cbxRepair", "Eşyaları tamir et (Repair all)", 15, y, 250);
            y += step;
            StorageCheckBox = CreateCheck("Town_cbxStorage", "Depoya eşya aktar (Store items)", 15, y, 250);
            y += step;
            SellTrashCheckBox = CreateCheck("Town_cbxSellTrash", "Çöpleri sat (Sell trash)", 15, y, 250);
            y += step;
            ReturnNavMeshCheckBox = CreateCheck("Town_cbxReturnNavMesh", "Navmesh ile kasılma alanına dön", 15, y, 250);

            LogisticsGroup.Controls.AddRange(new Control[] {
                EnableTownLoopCheckBox,
                RepairCheckBox,
                StorageCheckBox,
                SellTrashCheckBox,
                ReturnNavMeshCheckBox
            });

            // 2. AutoBuy Group
            AutoBuyGroup = new GroupBox
            {
                Name = "Town_gbxAutoBuy",
                Text = "Otomatik Satın Alma (Auto-Buy)",
                Location = new Point(325, 10),
                Size = new Size(320, 170),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            y = 22;
            AutoBuyCheckBox = CreateCheck("Town_cbxAutoBuy", "Otomatik alımı etkinleştir", 15, y, 250);
            y += step;

            var lblHp = new Label { Text = "HP Tipi & Adet:", Location = new Point(15, y + 2), Size = new Size(100, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F) };
            HpTypeComboBox = new ComboBox { Name = "Town_cmbxHpType", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, y), Size = new Size(110, 22), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            HpTypeComboBox.Items.AddRange(new object[] { "X-Small", "Small", "Medium", "Large", "X-Large" });
            if (HpTypeComboBox.Items.Count > 3) HpTypeComboBox.SelectedIndex = 3;
            HpAmountNumeric = new NumericUpDown { Name = "Town_nudHpAmount", Location = new Point(235, y), Size = new Size(70, 22), Maximum = 5000, Value = 500, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            y += step;

            var lblMp = new Label { Text = "MP Tipi & Adet:", Location = new Point(15, y + 2), Size = new Size(100, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F) };
            MpTypeComboBox = new ComboBox { Name = "Town_cmbxMpType", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, y), Size = new Size(110, 22), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            MpTypeComboBox.Items.AddRange(new object[] { "X-Small", "Small", "Medium", "Large", "X-Large" });
            if (MpTypeComboBox.Items.Count > 3) MpTypeComboBox.SelectedIndex = 3;
            MpAmountNumeric = new NumericUpDown { Name = "Town_nudMpAmount", Location = new Point(235, y), Size = new Size(70, 22), Maximum = 5000, Value = 500, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            y += step;

            BuyPillsCheckBox = CreateCheck("Town_cbxBuyPills", "Universal Pill satın al", 15, y, 250);

            AutoBuyGroup.Controls.AddRange(new Control[] {
                AutoBuyCheckBox,
                lblHp, HpTypeComboBox, HpAmountNumeric,
                lblMp, MpTypeComboBox, MpAmountNumeric,
                BuyPillsCheckBox
            });

            // 3. PickFilter Group
            PickFilterGroup = new GroupBox
            {
                Name = "Filter_gbxPick",
                Text = "Yerden Eşya Toplama (Pick Filter)",
                Location = new Point(10, 190),
                Size = new Size(635, 170),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            y = 22;
            PickGoldCheckBox = CreateCheck("Filter_cbxPickGold", "Altınları topla (Pick Gold)", 15, y, 250);
            PickGoldCheckBox.Checked = true;
            y += step;

            PickEquipCheckBox = CreateCheck("Filter_cbxPickEquip", "Ekipmanları topla (Pick Equipments)", 15, y, 250);
            PickEquipCheckBox.Checked = true;
            y += step;

            PickElixirStoneCheckBox = CreateCheck("Filter_cbxPickElixirStone", "Elixir ve Magic Stone'ları topla", 15, y, 250);
            PickElixirStoneCheckBox.Checked = true;
            y += step;

            PickMaterialsCheckBox = CreateCheck("Filter_cbxPickMaterials", "Element ve Materyalleri topla", 15, y, 250);
            PickMaterialsCheckBox.Checked = true;
            y += step;

            UsePetCheckBox = CreateCheck("Filter_cbxUsePet", "Toplama peti kullan (Varsa pet ile topla)", 15, y, 300);
            UsePetCheckBox.Checked = true;

            PickFilterGroup.Controls.AddRange(new Control[] {
                PickGoldCheckBox,
                PickEquipCheckBox,
                PickElixirStoneCheckBox,
                PickMaterialsCheckBox,
                UsePetCheckBox
            });

            this.Controls.Add(LogisticsGroup);
            this.Controls.Add(AutoBuyGroup);
            this.Controls.Add(PickFilterGroup);

            this.ResumeLayout(false);
        }

        private CheckBox CreateCheck(string name, string text, int x, int y, int width)
        {
            var cb = new CheckBox
            {
                Name = name,
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 24),
                ForeColor = Color.FromArgb(241, 241, 241),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            cb.CheckedChanged += (s, e) => TownSettingChanged?.Invoke(this, e);
            return cb;
        }

        public void BindToWindow(Window window)
        {
            if (window == null) return;
            // Eşitleme veya event bağlama köprüleri
        }
    }
}
