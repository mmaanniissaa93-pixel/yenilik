using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    public enum ModernButtonStyle
    {
        Primary,    // Electric Azure Accent
        Secondary,  // Dark Surface Raised
        Success,    // Emerald Green (Start, Save)
        Danger,     // Crimson Red (Stop, Exit)
        Ghost       // Transparent with border
    }

    /// <summary>
    /// Modern flat button with rounded corners, mouse state transitions, and semantic styles.
    /// </summary>
    public class ModernButton : Button
    {
        private ModernButtonStyle _buttonStyle = ModernButtonStyle.Primary;
        private int _borderRadius = 6;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private string _iconText = string.Empty;

        [Category("Appearance")]
        public ModernButtonStyle ButtonStyle
        {
            get => _buttonStyle;
            set { _buttonStyle = value; Invalidate(); }
        }

        [Category("Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        public string IconText
        {
            get => _iconText;
            set { _iconText = value; Invalidate(); }
        }

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = DarkTheme.FontBodyBold;
            Size = new Size(110, 34);
            Margin = new Padding(DarkTheme.SpacingCompact);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (mevent.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        private void ResolveColors(out Color bgColor, out Color borderColor, out Color textColor)
        {
            if (!Enabled)
            {
                bgColor = Color.FromArgb(30, 34, 45);
                borderColor = DarkTheme.BorderSubtle;
                textColor = DarkTheme.TextFaint;
                return;
            }

            switch (_buttonStyle)
            {
                case ModernButtonStyle.Primary:
                    bgColor = _isPressed ? DarkTheme.AccentPressed : (_isHovered ? DarkTheme.AccentHover : DarkTheme.Accent);
                    borderColor = bgColor;
                    textColor = DarkTheme.TextPrimary;
                    break;

                case ModernButtonStyle.Success:
                    bgColor = _isPressed ? Color.FromArgb(5, 150, 105) : (_isHovered ? DarkTheme.SuccessHover : DarkTheme.Success);
                    borderColor = bgColor;
                    textColor = Color.White;
                    break;

                case ModernButtonStyle.Danger:
                    bgColor = _isPressed ? Color.FromArgb(220, 38, 38) : (_isHovered ? DarkTheme.DangerHover : DarkTheme.Danger);
                    borderColor = bgColor;
                    textColor = Color.White;
                    break;

                case ModernButtonStyle.Secondary:
                    bgColor = _isPressed ? Color.FromArgb(50, 56, 75) : (_isHovered ? DarkTheme.BgInput : Color.FromArgb(30, 34, 45));
                    borderColor = _isHovered ? DarkTheme.BorderBright : DarkTheme.BorderSubtle;
                    textColor = DarkTheme.TextSecondary;
                    break;

                case ModernButtonStyle.Ghost:
                default:
                    bgColor = _isPressed ? Color.FromArgb(30, 41, 59) : (_isHovered ? Color.FromArgb(22, 27, 38) : Color.Transparent);
                    borderColor = _isHovered ? DarkTheme.BorderBright : DarkTheme.BorderSubtle;
                    textColor = _isHovered ? DarkTheme.TextPrimary : DarkTheme.TextMuted;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            ResolveColors(out Color bgColor, out Color borderColor, out Color textColor);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (GraphicsPath path = DarkTheme.CreateRoundedRectangle(rect, _borderRadius))
            {
                // Background
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Border
                if (borderColor != Color.Transparent)
                {
                    using (Pen borderPen = new Pen(borderColor, 1f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }

                // Text & Icon
                string displayText = string.IsNullOrEmpty(_iconText) ? Text : $"{_iconText}  {Text}";
                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(displayText, Font, textBrush, rect, sf);
                }
            }
        }
    }
}
