using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    /// <summary>
    /// Modern styled TextBox with rounded corners, focus state border highlight, and placeholder support.
    /// </summary>
    [DefaultEvent("TextChanged")]
    public class ModernTextBox : UserControl
    {
        private readonly TextBox _innerTextBox;
        private int _borderRadius = 6;
        private Color _borderColor = DarkTheme.BorderSubtle;
        private Color _borderFocusColor = DarkTheme.BorderFocus;
        private Color _backFillColor = DarkTheme.BgInput;
        private string _placeholderText = string.Empty;
        private bool _isFocused = false;

        public new event EventHandler TextChanged
        {
            add => _innerTextBox.TextChanged += value;
            remove => _innerTextBox.TextChanged -= value;
        }

        public new event KeyEventHandler KeyDown
        {
            add => _innerTextBox.KeyDown += value;
            remove => _innerTextBox.KeyDown -= value;
        }

        [Category("Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        public Color BorderFocusColor
        {
            get => _borderFocusColor;
            set { _borderFocusColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color BackFillColor
        {
            get => _backFillColor;
            set
            {
                _backFillColor = value;
                _innerTextBox.BackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        public string PlaceholderText
        {
            get => _placeholderText;
            set { _placeholderText = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => _innerTextBox.Text;
            set => _innerTextBox.Text = value;
        }

        [Category("Behavior")]
        public bool UseSystemPasswordChar
        {
            get => _innerTextBox.UseSystemPasswordChar;
            set => _innerTextBox.UseSystemPasswordChar = value;
        }

        [Category("Behavior")]
        public char PasswordChar
        {
            get => _innerTextBox.PasswordChar;
            set => _innerTextBox.PasswordChar = value;
        }

        [Category("Behavior")]
        public bool ReadOnly
        {
            get => _innerTextBox.ReadOnly;
            set => _innerTextBox.ReadOnly = value;
        }

        [Category("Behavior")]
        public bool Multiline
        {
            get => _innerTextBox.Multiline;
            set
            {
                _innerTextBox.Multiline = value;
                UpdateInnerPosition();
            }
        }

        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(200, 32);
            Padding = new Padding(8, 6, 8, 6);

            _innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = _backFillColor,
                ForeColor = DarkTheme.TextPrimary,
                Font = DarkTheme.FontBody,
                Dock = DockStyle.None
            };

            _innerTextBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _innerTextBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            _innerTextBox.TextChanged += (s, e) => Invalidate();

            Controls.Add(_innerTextBox);
            UpdateInnerPosition();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateInnerPosition();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            _innerTextBox.Font = Font;
            UpdateInnerPosition();
        }

        private void UpdateInnerPosition()
        {
            if (_innerTextBox == null) return;

            if (_innerTextBox.Multiline)
            {
                _innerTextBox.Location = new Point(10, 6);
                _innerTextBox.Size = new Size(Width - 20, Height - 12);
            }
            else
            {
                int innerH = _innerTextBox.PreferredHeight;
                int innerY = (Height - innerH) / 2;
                _innerTextBox.Location = new Point(10, Math.Max(2, innerY));
                _innerTextBox.Size = new Size(Width - 20, innerH);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (GraphicsPath path = DarkTheme.CreateRoundedRectangle(rect, _borderRadius))
            {
                // Background fill
                using (SolidBrush bgBrush = new SolidBrush(_backFillColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Border (Bright Accent when focused, Subtle when unfocused)
                Color curBorder = _isFocused ? _borderFocusColor : _borderColor;
                using (Pen pen = new Pen(curBorder, _isFocused ? 1.5f : 1f))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Draw Placeholder text when empty and not focused
            if (string.IsNullOrEmpty(_innerTextBox.Text) && !string.IsNullOrEmpty(_placeholderText) && !_isFocused)
            {
                Rectangle phRect = new Rectangle(12, 0, Width - 24, Height);
                using (SolidBrush phBrush = new SolidBrush(DarkTheme.TextFaint))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(_placeholderText, Font, phBrush, phRect, sf);
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _innerTextBox.Focus();
        }
    }
}
