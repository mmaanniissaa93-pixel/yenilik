using System;
using System.Drawing;
using System.Windows.Forms;
using xGraphics;

namespace xBot.App.Controls
{
    /// <summary>
    /// Modüler saldırı yönetim paneli (AttackPanel).
    /// Farklı yaratık türlerine (General, Champion, Giant, Party, Unique, Elite, Event) göre
    /// beceri rotasyonları, sıra takibi ve merkez yürüyüş ayarlarını kapsüller.
    /// </summary>
    public class AttackPanel : UserControl
    {
        public ComboBox MobTypeComboBox { get; private set; }
        public Button AddSkillButton { get; private set; }
        public Button RemoveSkillButton { get; private set; }
        public CheckBox CastInOrderCheckBox { get; private set; }
        public CheckBox WalkToCenterCheckBox { get; private set; }

        public xListView ListViewGeneral { get; private set; }
        public xListView ListViewChampion { get; private set; }
        public xListView ListViewGiant { get; private set; }
        public xListView ListViewPartyGeneral { get; private set; }
        public xListView ListViewPartyChampion { get; private set; }
        public xListView ListViewPartyGiant { get; private set; }
        public xListView ListViewUnique { get; private set; }
        public xListView ListViewElite { get; private set; }
        public xListView ListViewEvent { get; private set; }

        public event EventHandler MobTypeChanged;
        public event EventHandler AddSkillClicked;
        public event EventHandler RemoveSkillClicked;
        public event EventHandler CastInOrderChanged;
        public event EventHandler WalkToCenterChanged;

        private xListView _activeListView;

        public AttackPanel()
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
                Name = "Skills_cmbxAttackMobType",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(6, 38),
                Size = new Size(461, 23),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            MobTypeComboBox.Items.AddRange(new object[] {
                "General", "Champion", "Giant", "PartyGeneral",
                "PartyChampion", "PartyGiant", "Unique", "Elite", "Event"
            });
            MobTypeComboBox.SelectedIndexChanged += OnMobTypeSelectedIndexChanged;

            // Add Skill Button
            AddSkillButton = new Button
            {
                Name = "Skills_btnAddAttack",
                Text = "Saldırıya Ekle",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Location = new Point(6, 7),
                Size = new Size(89, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            AddSkillButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            AddSkillButton.Click += (s, e) => AddSkillClicked?.Invoke(this, e);

            // Remove Skill Button
            RemoveSkillButton = new Button
            {
                Name = "Skills_btnRemAttack",
                Text = "Kaldır",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Location = new Point(101, 7),
                Size = new Size(89, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(241, 241, 241)
            };
            RemoveSkillButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            RemoveSkillButton.Click += (s, e) => RemoveSkillClicked?.Invoke(this, e);

            // Cast in order
            CastInOrderCheckBox = new CheckBox
            {
                Name = "Skills_cbxCastInOrder",
                Text = "Becerileri sırayla kullan",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(197, 7),
                Size = new Size(139, 25),
                ForeColor = Color.FromArgb(241, 241, 241),
                FlatStyle = FlatStyle.Flat
            };
            CastInOrderCheckBox.CheckedChanged += (s, e) => CastInOrderChanged?.Invoke(this, e);

            // Walk to center
            WalkToCenterCheckBox = new CheckBox
            {
                Name = "Training_cbxWalkToCenter",
                Text = "Merkeze yürü",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(340, 7),
                Size = new Size(125, 25),
                ForeColor = Color.FromArgb(241, 241, 241),
                FlatStyle = FlatStyle.Flat
            };
            WalkToCenterCheckBox.CheckedChanged += (s, e) => WalkToCenterChanged?.Invoke(this, e);

            // Create ListViews for each mob type
            ListViewGeneral = CreateListView("Skills_lstvAttackMobType_General");
            ListViewChampion = CreateListView("Skills_lstvAttackMobType_Champion");
            ListViewGiant = CreateListView("Skills_lstvAttackMobType_Giant");
            ListViewPartyGeneral = CreateListView("Skills_lstvAttackMobType_PartyGeneral");
            ListViewPartyChampion = CreateListView("Skills_lstvAttackMobType_PartyChampion");
            ListViewPartyGiant = CreateListView("Skills_lstvAttackMobType_PartyGiant");
            ListViewUnique = CreateListView("Skills_lstvAttackMobType_Unique");
            ListViewElite = CreateListView("Skills_lstvAttackMobType_Elite");
            ListViewEvent = CreateListView("Skills_lstvAttackMobType_Event");

            this.Controls.Add(AddSkillButton);
            this.Controls.Add(RemoveSkillButton);
            this.Controls.Add(CastInOrderCheckBox);
            this.Controls.Add(WalkToCenterCheckBox);
            this.Controls.Add(MobTypeComboBox);

            this.Controls.Add(ListViewGeneral);
            this.Controls.Add(ListViewChampion);
            this.Controls.Add(ListViewGiant);
            this.Controls.Add(ListViewPartyGeneral);
            this.Controls.Add(ListViewPartyChampion);
            this.Controls.Add(ListViewPartyGiant);
            this.Controls.Add(ListViewUnique);
            this.Controls.Add(ListViewElite);
            this.Controls.Add(ListViewEvent);

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
            lv.Columns.Add(new ColumnHeader { Text = "Beceri Adı", Width = 450 });
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
                case "Event": _activeListView = ListViewEvent; break;
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
            // Event köprüleri
            this.AddSkillClicked += (s, e) => {
                try { window.Control_Click(window.Skills_btnAddAttack, e); } catch { }
            };
            this.RemoveSkillClicked += (s, e) => {
                try { window.Control_Click(window.Skills_btnRemAttack, e); } catch { }
            };
        }
    }
}
