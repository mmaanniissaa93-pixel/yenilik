using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    public enum StatBarType
    {
        HP,
        MP,
        EXP,
        Level,
        Custom
    }

    public enum StatBarDisplayMode
    {
        Percentage,
        Values,
        Full,
        TextOnly
    }

    /// <summary>
    /// Custom-drawn modern status / progress bar for HP, MP, EXP, and LVL.
    /// Features rounded corners, dynamic color grading, and crisp high-contrast centered text.
    /// </summary>
    public class ModernStatusBar : Control
    {
        private StatBarType _barType = StatBarType.HP;
        private StatBarDisplayMode _displayMode = StatBarDisplayMode.Percentage;
        private ulong _currentValue = 100;
        private ulong _maximumValue = 100;
        private string _prefix = "HP";
        private string _customText = string.Empty;
        private int _borderRadius = 5;
        private Color _customColor = DarkTheme.Success;
        private Color _troughColor = Color.FromArgb(22, 25, 34); // #161922

        [Category("Appearance")]
        public StatBarType BarType
        {
            get => _barType;
            set
            {
                _barType = value;
                if (_barType == StatBarType.HP) _prefix = "HP";
                else if (_barType == StatBarType.MP) _prefix = "MP";
                else if (_barType == StatBarType.EXP) _prefix = "EXP";
                else if (_barType == StatBarType.Level) _prefix = "LVL";
                Invalidate();
            }
        }

        [Category("Appearance")]
        public StatBarDisplayMode DisplayMode
        {
            get => _displayMode;
            set { _displayMode = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string Prefix
        {
            get => _prefix;
            set { _prefix = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string CustomText
        {
            get => _customText;
            set { _customText = value; Invalidate(); }
        }

        [Category("Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        public Color CustomColor
        {
            get => _customColor;
            set { _customColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color TroughColor
        {
            get => _troughColor;
            set { _troughColor = value; Invalidate(); }
        }

        [Category("Behavior")]
        public ulong CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        public ulong MaximumValue
        {
            get => _maximumValue;
            set
            {
                _maximumValue = Math.Max(1, value);
                Invalidate();
            }
        }

        public double Percentage
        {
            get
            {
                if (_maximumValue == 0) return 0.0;
                double pct = (double)_currentValue * 100.0 / (double)_maximumValue;
                return Math.Max(0.0, Math.Min(100.0, pct));
            }
        }

        public ModernStatusBar()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Font = DarkTheme.GetFont(8.5f, FontStyle.Bold);
            ForeColor = Color.White;
            Size = new Size(130, 22);
        }

        public void SetValues(ulong current, ulong maximum)
        {
            _currentValue = current;
            _maximumValue = Math.Max(1, maximum);
            Invalidate();
        }

        private Color ResolveBarColor()
        {
            switch (_barType)
            {
                case StatBarType.HP:
                    // Dynamic color shift if critical
                    if (Percentage <= 25.0) return DarkTheme.Danger;
                    if (Percentage <= 50.0) return DarkTheme.Warning;
                    return DarkTheme.Success;

                case StatBarType.MP:
                    return DarkTheme.Mana;

                case StatBarType.EXP:
                case StatBarType.Level:
                    return DarkTheme.Warning;

                case StatBarType.Custom:
                default:
                    return _customColor;
            }
        }

        private string GetFormattedText()
        {
            if (!string.IsNullOrEmpty(_customText))
                return _customText;

            if (_barType == StatBarType.Level)
                return $"{_prefix}: {_currentValue}";

            switch (_displayMode)
            {
                case StatBarDisplayMode.Percentage:
                    return $"{_prefix}: {Percentage:0.#}%";

                case StatBarDisplayMode.Values:
                    return $"{_prefix}: {_currentValue:N0} / {_maximumValue:N0}";

                case StatBarDisplayMode.Full:
                    return $"{_prefix}: {_currentValue:N0} ({Percentage:0.#}%)";

                case StatBarDisplayMode.TextOnly:
                default:
                    return $"{_prefix}: {_currentValue}";
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

            // 1. Draw Trough Background
            using (GraphicsPath troughPath = DarkTheme.CreateRoundedRectangle(rect, _borderRadius))
            {
                using (SolidBrush troughBrush = new SolidBrush(_troughColor))
                {
                    g.FillPath(troughBrush, troughPath);
                }

                // 2. Draw Progress Fill
                if (_barType != StatBarType.Level && _currentValue > 0)
                {
                    int fillWidth = (int)Math.Round((double)rect.Width * (Percentage / 100.0));
                    fillWidth = Math.Max(0, Math.Min(rect.Width, fillWidth));

                    if (fillWidth > 0)
                    {
                        Rectangle fillRect = new Rectangle(0, 0, fillWidth, rect.Height);
                        using (GraphicsPath fillPath = DarkTheme.CreateRoundedRectangle(fillRect, _borderRadius))
                        using (SolidBrush fillBrush = new SolidBrush(ResolveBarColor()))
                        {
                            g.SetClip(troughPath);
                            g.FillPath(fillBrush, fillPath);
                            g.ResetClip();
                        }
                    }
                }

                // 3. Draw Outer Border
                using (Pen borderPen = new Pen(DarkTheme.BorderSubtle, 1f))
                {
                    g.DrawPath(borderPen, troughPath);
                }
            }

            // 4. Draw Centered Text with Subtle Drop Shadow
            string text = GetFormattedText();
            Rectangle textRect = new Rectangle(0, 0, Width, Height);

            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };

            // Shadow
            Rectangle shadowRect = new Rectangle(textRect.X + 1, textRect.Y + 1, textRect.Width, textRect.Height);
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            {
                g.DrawString(text, Font, shadowBrush, shadowRect, sf);
            }

            // Foreground Text
            using (SolidBrush textBrush = new SolidBrush(ForeColor))
            {
                g.DrawString(text, Font, textBrush, textRect, sf);
            }
        }
    }
}
