using System;
using System.Drawing;
using System.Windows.Forms;

namespace xBot.App
{
    /// <summary>
    /// Yürü butonu için koordinat giriş diyaloğu (X, Y).
    /// </summary>
    public sealed class WalkCoordinateForm : Form
    {
        private NumericUpDown numX;
        private NumericUpDown numY;
        private Button btnOk;
        private Button btnCancel;

        public int CoordX => (int)numX.Value;
        public int CoordY => (int)numY.Value;

        public WalkCoordinateForm(int defaultX = 0, int defaultY = 0)
        {
            InitializeComponent(defaultX, defaultY);
        }

        private void InitializeComponent(int defaultX, int defaultY)
        {
            this.Text = "Yürü - Koordinat Gir";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(260, 130);
            this.Font = Theme.AppTheme.FontBody;
            this.BackColor = Theme.AppTheme.Background;
            this.ForeColor = Theme.AppTheme.TextPrimary;
            this.AutoScaleDimensions = new SizeF(96f, 96f);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            var lblX = new Label();
            lblX.Text = "Hedef X:";
            lblX.Location = new Point(20, 22);
            lblX.AutoSize = true;

            numX = new NumericUpDown();
            numX.Location = new Point(90, 20);
            numX.Size = new Size(140, 23);
            numX.Minimum = -200000;
            numX.Maximum = 200000;
            numX.Value = Math.Max(numX.Minimum, Math.Min(numX.Maximum, defaultX));

            var lblY = new Label();
            lblY.Text = "Hedef Y:";
            lblY.Location = new Point(20, 56);
            lblY.AutoSize = true;

            numY = new NumericUpDown();
            numY.Location = new Point(90, 54);
            numY.Size = new Size(140, 23);
            numY.Minimum = -200000;
            numY.Maximum = 200000;
            numY.Value = Math.Max(numY.Minimum, Math.Min(numY.Maximum, defaultY));

            btnOk = new Button();
            btnOk.Text = "Yürü";
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(74, 92);
            btnOk.Size = new Size(75, 26);
            btnOk.UseVisualStyleBackColor = true;

            btnCancel = new Button();
            btnCancel.Text = "İptal";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(155, 92);
            btnCancel.Size = new Size(75, 26);
            btnCancel.UseVisualStyleBackColor = true;

            this.Controls.AddRange(new Control[] { lblX, numX, lblY, numY, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            lblX.ForeColor = Theme.AppTheme.Colors.TextSecondary;
            lblY.ForeColor = Theme.AppTheme.Colors.TextSecondary;
            Theme.AppTheme.ComponentStyles.StyleSecondaryButton(btnCancel);
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.BackColor = Theme.AppTheme.Colors.Neutral.TextPrimary;
            btnOk.ForeColor = Theme.AppTheme.Colors.Neutral.TextInverse;
            btnOk.Font = Theme.AppTheme.FontBodyBold;
            btnOk.Cursor = Cursors.Hand;

            try
            {
                if (Window.Get != null && Window.Get.Icon != null)
                    this.Icon = Window.Get.Icon;
            }
            catch { }
            Theme.ThemeManager.Apply(this);
        }
    }
}
