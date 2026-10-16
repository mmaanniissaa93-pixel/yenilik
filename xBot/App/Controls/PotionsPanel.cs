using System;
using System.Drawing;
using System.Windows.Forms;

namespace xBot.App.Controls
{
    /// <summary>
    /// Modüler iksir ve yenilenme yönetim paneli (PotionsPanel).
    /// Karakter ve pet için HP, MP, Vigor, Grain ve Pill kullanım eşiklerini kapsüller.
    /// </summary>
    public class PotionsPanel : UserControl
    {
        public GroupBox PlayerPotionsGroup { get; private set; }
        public GroupBox PetPotionsGroup { get; private set; }

        // Player controls
        public CheckBox UseHPCheckBox { get; private set; }
        public TextBox UseHPTextBox { get; private set; }
        public CheckBox UseHPGrainCheckBox { get; private set; }
        public CheckBox UseHPVigorCheckBox { get; private set; }
        public TextBox UseHPVigorTextBox { get; private set; }
        public CheckBox UseMPCheckBox { get; private set; }
        public TextBox UseMPTextBox { get; private set; }
        public CheckBox UseMPGrainCheckBox { get; private set; }
        public CheckBox UseMPVigorCheckBox { get; private set; }
        public TextBox UseMPVigorTextBox { get; private set; }
        public CheckBox UsePillUniversalCheckBox { get; private set; }
        public CheckBox UsePillPurificationCheckBox { get; private set; }

        // Pet controls
        public CheckBox UsePetHPCheckBox { get; private set; }
        public TextBox UsePetHPTextBox { get; private set; }
        public CheckBox UsePetHGPCheckBox { get; private set; }
        public TextBox UsePetHGPTextBox { get; private set; }
        public CheckBox UsePetPillUniversalCheckBox { get; private set; }

        public event EventHandler PotionSettingChanged;

        public PotionsPanel()
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
            this.AutoScroll = true;

            // Player Potions Group
            PlayerPotionsGroup = new GroupBox
            {
                Name = "Character_gbxPotionsPlayer",
                Text = "Karakter İksirleri",
                Location = new Point(6, 6),
                Size = new Size(225, 330),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            int y = 20;
            const int step = 28;

            UseHPCheckBox = CreateCheck("Character_cbxUseHP", "Oto HP (%)", 10, y, 100);
            UseHPTextBox = CreateText("Character_tbxUseHP", "70", 115, y, 40);
            y += step;

            UseHPGrainCheckBox = CreateCheck("Character_cbxUseHPGrain", "HP Grain Kullan", 10, y, 180);
            y += step;

            UseHPVigorCheckBox = CreateCheck("Character_cbxUseHPVigor", "HP Vigor (%)", 10, y, 100);
            UseHPVigorTextBox = CreateText("Character_tbxUseHPVigor", "40", 115, y, 40);
            y += step;

            UseMPCheckBox = CreateCheck("Character_cbxUseMP", "Oto MP (%)", 10, y, 100);
            UseMPTextBox = CreateText("Character_tbxUseMP", "50", 115, y, 40);
            y += step;

            UseMPGrainCheckBox = CreateCheck("Character_cbxUseMPGrain", "MP Grain Kullan", 10, y, 180);
            y += step;

            UseMPVigorCheckBox = CreateCheck("Character_cbxUseMPVigor", "MP Vigor (%)", 10, y, 100);
            UseMPVigorTextBox = CreateText("Character_tbxUseMPVigor", "30", 115, y, 40);
            y += step;

            UsePillUniversalCheckBox = CreateCheck("Character_cbxUsePillUniversal", "Universal Pill Kullan", 10, y, 180);
            y += step;

            UsePillPurificationCheckBox = CreateCheck("Character_cbxUsePillPurification", "Purification Pill Kullan", 10, y, 180);

            PlayerPotionsGroup.Controls.AddRange(new Control[] {
                UseHPCheckBox, UseHPTextBox,
                UseHPGrainCheckBox,
                UseHPVigorCheckBox, UseHPVigorTextBox,
                UseMPCheckBox, UseMPTextBox,
                UseMPGrainCheckBox,
                UseMPVigorCheckBox, UseMPVigorTextBox,
                UsePillUniversalCheckBox,
                UsePillPurificationCheckBox
            });

            // Pet Potions Group
            PetPotionsGroup = new GroupBox
            {
                Name = "Character_gbxPotionPet",
                Text = "Pet İksirleri",
                Location = new Point(237, 6),
                Size = new Size(225, 330),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            y = 20;
            UsePetHPCheckBox = CreateCheck("Character_cbxUsePetHP", "Pet HP (%)", 10, y, 100);
            UsePetHPTextBox = CreateText("Character_tbxUsePetHP", "50", 115, y, 40);
            y += step;

            UsePetHGPCheckBox = CreateCheck("Character_cbxUsePetHGP", "Pet HGP (%)", 10, y, 100);
            UsePetHGPTextBox = CreateText("Character_tbxUsePetHGP", "50", 115, y, 40);
            y += step;

            UsePetPillUniversalCheckBox = CreateCheck("Character_cbxUsePetPillUniversal", "Pet Pill Kullan", 10, y, 180);

            PetPotionsGroup.Controls.AddRange(new Control[] {
                UsePetHPCheckBox, UsePetHPTextBox,
                UsePetHGPCheckBox, UsePetHGPTextBox,
                UsePetPillUniversalCheckBox
            });

            this.Controls.Add(PlayerPotionsGroup);
            this.Controls.Add(PetPotionsGroup);
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
            cb.CheckedChanged += (s, e) => PotionSettingChanged?.Invoke(this, e);
            return cb;
        }

        private TextBox CreateText(string name, string text, int x, int y, int width)
        {
            var tb = new TextBox
            {
                Name = name,
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 22),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            tb.TextChanged += (s, e) => PotionSettingChanged?.Invoke(this, e);
            return tb;
        }

        public void BindToWindow(Window window)
        {
            if (window == null) return;
            // Eşitleme veya event bağlama köprüleri
        }
    }
}
