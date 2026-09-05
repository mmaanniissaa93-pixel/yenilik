using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    /// <summary>
    /// Modern Card panel with rounded corners, subtle border, and optional header badge/title.
    /// Replaces standard WinForms GroupBox controls.
    /// </summary>
    public class ModernCard : Panel
    {
        private int _borderRadius = DarkTheme.DefaultRadius;
        private Color _borderColor = DarkTheme.BorderSubtle;
        private Color _cardColor = DarkTheme.BgCard;
        private string _titleText = string.Empty;
        private int _headerHeight = 34;
        private Color _accentColor = DarkTheme.Accent;

        [Category("Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color CardColor
        {
            get => _cardColor;
            set { _cardColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public string TitleText
        {
            get => _titleText;
            set { _titleText = value; Invalidate(); }
        }

        [Category("Appearance")]
        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        public ModernCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Padding = new Padding(DarkTheme.SpacingDefault);
            Font = DarkTheme.FontBody;
            ForeColor = DarkTheme.TextSecondary;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle clientRect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (clientRect.Width <= 0 || clientRect.Height <= 0) return;

            // 1. Draw Card Surface
            using (GraphicsPath path = DarkTheme.CreateRoundedRectangle(clientRect, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(_cardColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // 2. Draw Header (if TitleText exists)
                if (!string.IsNullOrEmpty(_titleText))
                {
                    int headerH = Math.Min(_headerHeight, Height);

                    // Accent pill indicator on the left of the header
                    int pillX = 12;
                    int pillY = 8;
                    int pillW = 3;
                    int pillH = 14;
                    using (GraphicsPath pillPath = DarkTheme.CreateRoundedRectangle(new Rectangle(pillX, pillY, pillW, pillH), 1))
                    using (SolidBrush pillBrush = new SolidBrush(_accentColor))
                    {
                        g.FillPath(pillBrush, pillPath);
                    }

                    // Header Title Text
                    Rectangle textRect = new Rectangle(21, 6, Width - 26, 18);
                    using (Font titleFont = DarkTheme.FontHeader)
                    using (SolidBrush textBrush = new SolidBrush(DarkTheme.TextPrimary))
                    {
                        StringFormat sf = new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter
                        };
                        g.DrawString(_titleText, titleFont, textBrush, textRect, sf);
                    }
                }

                // 3. Draw Outer Border
                using (Pen borderPen = new Pen(_borderColor, 1f))
                {
                    g.DrawPath(borderPen, path);
                }
            }
        }
    }
}
