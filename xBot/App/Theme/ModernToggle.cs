using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    /// <summary>
    /// Smooth animated toggle switch (on/off) replacement for standard WinForms CheckBox.
    /// </summary>
    [DefaultEvent("CheckedChanged")]
    public class ModernToggle : Control
    {
        private bool _checked = false;
        private float _animProgress = 0f; // 0.0f (Off) to 1.0f (On)
        private readonly Timer _animTimer;

        private Color _activeColor = DarkTheme.Accent;
        private Color _inactiveColor = Color.FromArgb(51, 65, 85); // #334155
        private Color _thumbColor = Color.White;
        private int _switchWidth = 38;
        private int _switchHeight = 20;

        public event EventHandler CheckedChanged;

        [Category("Appearance")]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    _animTimer.Start();
                    OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        public Color ActiveColor
        {
            get => _activeColor;
            set { _activeColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color InactiveColor
        {
            get => _inactiveColor;
            set { _inactiveColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        public ModernToggle()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Selectable, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = DarkTheme.FontBody;
            ForeColor = DarkTheme.TextSecondary;
            Size = new Size(160, 24);

            _animTimer = new Timer { Interval = 15 };
            _animTimer.Tick += AnimTimer_Tick;
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            float target = _checked ? 1.0f : 0.0f;
            float step = 0.15f;

            if (_checked)
            {
                _animProgress += step;
                if (_animProgress >= 1.0f)
                {
                    _animProgress = 1.0f;
                    _animTimer.Stop();
                }
            }
            else
            {
                _animProgress -= step;
                if (_animProgress <= 0.0f)
                {
                    _animProgress = 0.0f;
                    _animTimer.Stop();
                }
            }

            Invalidate();
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
            Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                Checked = !Checked;
                e.Handled = true;
            }
        }

        private static Color InterpolateColor(Color c1, Color c2, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            int r = (int)(c1.R + (c2.R - c1.R) * t);
            int g = (int)(c1.G + (c2.G - c1.G) * t);
            int b = (int)(c1.B + (c2.B - c1.B) * t);
            return Color.FromArgb(r, g, b);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (Window.UsePhBotClassic)
            {
                pevent.Graphics.Clear(BackColor.A == 0 ? Parent.BackColor : BackColor);
                var state = !Enabled
                    ? (_checked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedDisabled : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled)
                    : (_checked ? System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal : System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                CheckBoxRenderer.DrawCheckBox(pevent.Graphics, new Point(0, Math.Max(0, (Height - 13) / 2)), state);
                TextRenderer.DrawText(pevent.Graphics, Text, Font, new Rectangle(20, 0, Math.Max(0, Width - 20), Height), ForeColor, TextFormatFlags.VerticalCenter);
                return;
            }
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int trackY = (Height - _switchHeight) / 2;
            Rectangle trackRect = new Rectangle(0, trackY, _switchWidth, _switchHeight);

            // Interpolate track color
            Color currentTrackColor = InterpolateColor(_inactiveColor, _activeColor, _animProgress);

            // Draw rounded pill track
            int trackRadius = _switchHeight / 2;
            using (GraphicsPath trackPath = DarkTheme.CreateRoundedRectangle(trackRect, trackRadius))
            {
                using (SolidBrush trackBrush = new SolidBrush(currentTrackColor))
                {
                    g.FillPath(trackBrush, trackPath);
                }

                // Subtle border
                using (Pen borderPen = new Pen(DarkTheme.BorderSubtle, 1f))
                {
                    g.DrawPath(borderPen, trackPath);
                }
            }

            // Draw thumb circle
            int thumbDiameter = _switchHeight - 4;
            float minX = 2;
            float maxX = _switchWidth - thumbDiameter - 2;
            float thumbX = minX + (maxX - minX) * _animProgress;
            float thumbY = trackY + 2;

            RectangleF thumbRect = new RectangleF(thumbX, thumbY, thumbDiameter, thumbDiameter);
            using (SolidBrush thumbBrush = new SolidBrush(_thumbColor))
            {
                g.FillEllipse(thumbBrush, thumbRect);
            }

            // Draw Text Label if present
            if (!string.IsNullOrEmpty(Text))
            {
                int textX = _switchWidth + DarkTheme.SpacingCompact;
                Rectangle textRect = new Rectangle(textX, 0, Width - textX, Height);
                Color textColor = _checked ? DarkTheme.TextPrimary : DarkTheme.TextMuted;

                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(Text, Font, textBrush, textRect, sf);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
