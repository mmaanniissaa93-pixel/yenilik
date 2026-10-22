using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace xBot.App
{
    /// <summary>
    /// Kasılma alanı parametrelerini (Adı, Menzil, Toplama Menzili, Dosya, Tipi) düzenleme penceresi.
    /// </summary>
    public sealed class TrainingAreaEditForm : Form
    {
        private TextBox txtName;
        private NumericUpDown numRadius;
        private NumericUpDown numPickRadius;
        private TextBox txtScriptPath;
        private Button btnBrowseScript;
        private ComboBox cbxType;
        private Button btnSave;
        private Button btnCancel;

        public TrainingAreaInfo AreaInfo { get; private set; }

        public TrainingAreaEditForm(TrainingAreaInfo info)
        {
            AreaInfo = info ?? new TrainingAreaInfo();
            InitializeComponent();
            Theme.ThemeManager.Apply(this);
        }

        private void InitializeComponent()
        {
            this.Text = "Kasılma Alanı Düzenle";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(340, 250);
            this.Font = Theme.AppTheme.FontBody;
            this.BackColor = Theme.AppTheme.Background;
            this.ForeColor = Theme.AppTheme.TextPrimary;
            this.AutoScaleDimensions = new SizeF(96f, 96f);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            int y = 16;
            var lblName = new Label { Text = "Adı:", Location = new Point(16, y + 2), AutoSize = true };
            txtName = new TextBox { Text = AreaInfo.Name, Location = new Point(130, y), Size = new Size(185, 23) };
            this.Controls.AddRange(new Control[] { lblName, txtName });
            y += 32;

            var lblRadius = new Label { Text = "Menzil (Radius):", Location = new Point(16, y + 2), AutoSize = true };
            numRadius = new NumericUpDown { Minimum = 1, Maximum = Math.Max(1000, AreaInfo.Radius), Value = Math.Max(1, AreaInfo.Radius), Location = new Point(130, y), Size = new Size(185, 23) };
            this.Controls.AddRange(new Control[] { lblRadius, numRadius });
            y += 32;

            var lblPickRadius = new Label { Text = "Toplama Menzili:", Location = new Point(16, y + 2), AutoSize = true };
            numPickRadius = new NumericUpDown { Minimum = 1, Maximum = Math.Max(1000, AreaInfo.PickRadius), Value = Math.Max(1, AreaInfo.PickRadius), Location = new Point(130, y), Size = new Size(185, 23) };
            this.Controls.AddRange(new Control[] { lblPickRadius, numPickRadius });
            y += 32;

            var lblScript = new Label { Text = "Dosya (Script):", Location = new Point(16, y + 2), AutoSize = true };
            txtScriptPath = new TextBox { Text = AreaInfo.ScriptPath ?? "", Location = new Point(130, y), Size = new Size(140, 23) };
            btnBrowseScript = new Button { Text = "...", Location = new Point(275, y - 1), Size = new Size(40, 25), UseVisualStyleBackColor = true };
            btnBrowseScript.Click += (s, e) => {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Script Dosyası Seç";
                    ofd.Filter = "Komut Dosyası (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*";
                    if (ofd.ShowDialog(this) == DialogResult.OK)
                    {
                        txtScriptPath.Text = ofd.FileName;
                    }
                }
            };
            this.Controls.AddRange(new Control[] { lblScript, txtScriptPath, btnBrowseScript });
            y += 32;

            var lblType = new Label { Text = "Tipi:", Location = new Point(16, y + 2), AutoSize = true };
            cbxType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(130, y), Size = new Size(185, 23) };
            cbxType.Items.AddRange(new object[] { "Menzil", "Komut", "Eğitim" });
            cbxType.SelectedItem = !string.IsNullOrEmpty(AreaInfo.Type) && cbxType.Items.Contains(AreaInfo.Type) ? AreaInfo.Type : "Menzil";
            this.Controls.AddRange(new Control[] { lblType, cbxType });
            y += 40;

            btnSave = new Button { Text = "Kaydet", DialogResult = DialogResult.OK, Location = new Point(155, y), Size = new Size(75, 26), UseVisualStyleBackColor = true };
            btnCancel = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Location = new Point(240, y), Size = new Size(75, 26), UseVisualStyleBackColor = true };
            btnSave.Click += (s, e) => {
                AreaInfo.Name = string.IsNullOrWhiteSpace(txtName.Text) ? "Yeni Kasılma Alanı" : txtName.Text.Trim();
                AreaInfo.Radius = (int)numRadius.Value;
                AreaInfo.PickRadius = (int)numPickRadius.Value;
                AreaInfo.ScriptPath = txtScriptPath.Text.Trim();
                AreaInfo.Type = cbxType.SelectedItem?.ToString() ?? "Menzil";
            };

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            foreach (Control control in this.Controls)
            {
                control.Font = Theme.AppTheme.FontBody;
                var label = control as Label;
                if (label != null)
                {
                    label.ForeColor = Theme.AppTheme.Colors.TextSecondary;
                }
                var text = control as TextBox;
                if (text != null) Theme.AppTheme.ComponentStyles.StyleTextBox(text);
                var combo = control as ComboBox;
                if (combo != null) Theme.AppTheme.ComponentStyles.StyleComboBox(combo);
            }
            Theme.AppTheme.ComponentStyles.StyleSecondaryButton(btnBrowseScript);
            Theme.AppTheme.ComponentStyles.StyleSecondaryButton(btnCancel);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.BackColor = Theme.AppTheme.Colors.Neutral.TextPrimary;
            btnSave.ForeColor = Color.White;
            btnSave.Font = Theme.AppTheme.FontBodyBold;
            btnSave.Cursor = Cursors.Hand;

            try
            {
                if (Window.Get != null && Window.Get.Icon != null)
                    this.Icon = Window.Get.Icon;
            }
            catch { }
        }
    }
}
