using System;
using System.Drawing;
using System.Windows.Forms;
using xGraphics;

namespace xBot.App.Controls
{
    /// <summary>
    /// Modüler güçlendirme beceri paneli (BuffsPanel).
    /// Yaratık türlerine göre buff beceri listelerini, ekleme/çıkarma ve tetikleme tercihlerini kapsüller.
    /// </summary>
    public class BuffsPanel : UserControl
    {
        public ComboBox MobTypeComboBox { get; private set; }
        public Button AddBuffButton { get; private set; }
        public Button RemoveBuffButton { get; private set; }

        public xListView ListViewGeneral { get; private set; }
        public xListView ListViewChampion { get; private set; }
        public xListView ListViewGiant { get; private set; }
        public xListView ListViewPartyGeneral { get; private set; }
        public xListView ListViewPartyChampion { get; private set; }
        public xListView ListViewPartyGiant { get; private set; }
        public xListView ListViewUnique { get; private set; }
        public xListView ListViewElite { get; private set; }

        public event EventHandler MobTypeChanged;
        public event EventHandler AddBuffClicked;
        public event EventHandler RemoveBuffClicked;

        private xListView _activeListView;

        public BuffsPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.FromArgb(241, 241, 241);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.Size = new Size(473, 345);

            // Mob Type Combo
            MobTypeComboBox = new ComboBox
            {
                Name = "Skills_cmbxBuffMobType",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(6, 38),
                Size = new Size(461, 23),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            MobTypeComboBox.Items.AddRange(new object[] {
                "General", "Champion", "Giant", "PartyGeneral",
                "PartyChampion", "PartyGiant", "Unique", "Elite"
            });
            MobTypeComboBox.SelectedIndexChanged += OnMobTypeSelectedIndexChanged;

            // Add Buff Button
            AddBuffButton = new Button
            {
                Name = "Skills_btnAddBuff",
                Text = "Buff Ekle",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Location = new Point(6, 7),
                Size = new Size(89, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            AddBuffButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            AddBuffButton.Click += (s, e) => AddBuffClicked?.Invoke(this, e);

            // Remove Buff Button
            RemoveBuffButton = new Button
            {
                Name = "Skills_btnRemBuff",
                Text = "Kaldır",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Location = new Point(101, 7),
                Size = new Size(89, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            RemoveBuffButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            RemoveBuffButton.Click += (s, e) => RemoveBuffClicked?.Invoke(this, e);

            // Create ListViews for each mob type
            ListViewGeneral = CreateListView("Skills_lstvBuffMobType_General");
            ListViewChampion = CreateListView("Skills_lstvBuffMobType_Champion");
            ListViewGiant = CreateListView("Skills_lstvBuffMobType_Giant");
            ListViewPartyGeneral = CreateListView("Skills_lstvBuffMobType_PartyGeneral");
            ListViewPartyChampion = CreateListView("Skills_lstvBuffMobType_PartyChampion");
            ListViewPartyGiant = CreateListView("Skills_lstvBuffMobType_PartyGiant");
            ListViewUnique = CreateListView("Skills_lstvBuffMobType_Unique");
            ListViewElite = CreateListView("Skills_lstvBuffMobType_Elite");

            this.Controls.Add(AddBuffButton);
            this.Controls.Add(RemoveBuffButton);
            this.Controls.Add(MobTypeComboBox);

            this.Controls.Add(ListViewGeneral);
            this.Controls.Add(ListViewChampion);
            this.Controls.Add(ListViewGiant);
            this.Controls.Add(ListViewPartyGeneral);
            this.Controls.Add(ListViewPartyChampion);
            this.Controls.Add(ListViewPartyGiant);
            this.Controls.Add(ListViewUnique);
            this.Controls.Add(ListViewElite);

            if (MobTypeComboBox.Items.Count > 0)
                MobTypeComboBox.SelectedIndex = 0;

            this.ResumeLayout(false);
        }

        private xListView CreateListView(string name)
        {
            var lv = new xListView
            {
                Name = name,
                Location = new Point(6, 68),
                Size = new Size(461, 270),
                View = View.Details,
                FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Visible = false,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            lv.Columns.Add(new ColumnHeader { Text = "Buff Beceri Adı", Width = 450 });
            return lv;
        }

        private void OnMobTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = MobTypeComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            if (_activeListView != null)
                _activeListView.Visible = false;

            switch (selected)
            {
                case "General": _activeListView = ListViewGeneral; break;
                case "Champion": _activeListView = ListViewChampion; break;
                case "Giant": _activeListView = ListViewGiant; break;
                case "PartyGeneral": _activeListView = ListViewPartyGeneral; break;
                case "PartyChampion": _activeListView = ListViewPartyChampion; break;
                case "PartyGiant": _activeListView = ListViewPartyGiant; break;
                case "Unique": _activeListView = ListViewUnique; break;
                case "Elite": _activeListView = ListViewElite; break;
            }

            if (_activeListView != null)
                _activeListView.Visible = true;

            MobTypeChanged?.Invoke(this, e);
        }

        public xListView GetCurrentListView() => _activeListView ?? ListViewGeneral;

        /// <summary>
        /// Mevcut Window formundaki kontroller ile bu paneli iki yönlü senkronize eder.
        /// </summary>
        public void BindToWindow(Window window)
        {
            if (window == null) return;
            this.AddBuffClicked += (s, e) => {
                try { window.Control_Click(window.Skills_btnAddBuff, e); } catch { }
            };
            this.RemoveBuffClicked += (s, e) => {
                try { window.Control_Click(window.Skills_btnRemBuff, e); } catch { }
            };
        }
    }
}
