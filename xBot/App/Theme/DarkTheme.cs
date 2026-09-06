using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    /// <summary>
    /// Centralized Dark Mode Design Tokens: Colors, Typography, and GDI+ Drawing Helpers.
    /// </summary>
    public static class DarkTheme
    {
        // -------------------------------------------------------------
        // Background & Surface Colors (Dark Void Palette)
        // -------------------------------------------------------------
        public static readonly Color BgDark          = Color.FromArgb(15, 17, 23);     // #0F1117 (Main form canvas)
        public static readonly Color BgSidebar       = Color.FromArgb(22, 25, 34);     // #161922 (Vertical sidebar)
        public static readonly Color BgCard          = Color.FromArgb(30, 34, 45);     // #1E222D (Surface card panel)
        public static readonly Color BgCardHeader    = Color.FromArgb(24, 28, 38);     // #181C26 (Card header / table header)
        public static readonly Color BgInput         = Color.FromArgb(38, 43, 58);     // #262B3A (Input fields / hover card)
        public static readonly Color BgInputFocus    = Color.FromArgb(46, 54, 76);     // #2E364C (Focused input background)
        public static readonly Color BgHeader        = Color.FromArgb(22, 25, 34);     // #161922 (Top window header)

        // -------------------------------------------------------------
        // Border & Divider Colors
        // -------------------------------------------------------------
        public static readonly Color BorderSubtle    = Color.FromArgb(45, 51, 69);     // #2D3345 (Default 1px borders)
        public static readonly Color BorderBright    = Color.FromArgb(61, 70, 96);     // #3D4660 (Hovered borders)
        public static readonly Color BorderFocus     = Color.FromArgb(59, 130, 246);   // #3B82F6 (Focused inputs)

        // -------------------------------------------------------------
        // Accent Colors (Electric Azure)
        // -------------------------------------------------------------
        public static readonly Color Accent          = Color.FromArgb(37, 99, 235);    // #2563EB (Primary action/tab bar)
        public static readonly Color Primary         = Color.FromArgb(37, 99, 235);    // Alias for Accent
        public static readonly Color AccentHover     = Color.FromArgb(59, 130, 246);   // #3B82F6 (Hover highlight)
        public static readonly Color AccentPressed   = Color.FromArgb(29, 78, 216);    // #1D4ED8 (Mouse down)
        public static readonly Color AccentSubtle    = Color.FromArgb(30, 41, 59);     // #1E293B (Active tab background)

        // -------------------------------------------------------------
        // Typography Colors
        // -------------------------------------------------------------
        public static readonly Color TextPrimary     = Color.FromArgb(248, 250, 252);  // #F8FAFC (High contrast titles/values)
        public static readonly Color TextSecondary   = Color.FromArgb(226, 232, 240);  // #E2E8F0 (Standard body text)
        public static readonly Color TextMuted       = Color.FromArgb(148, 163, 184);  // #94A3B8 (Labels, captions)
        public static readonly Color TextFaint       = Color.FromArgb(100, 116, 139);  // #64748B (Category headings, disabled)

        // -------------------------------------------------------------
        // Status & Semantic Colors
        // -------------------------------------------------------------
        public static readonly Color Success         = Color.FromArgb(16, 185, 129);   // #10B981 (HP / Success logs)
        public static readonly Color SuccessHover    = Color.FromArgb(52, 211, 153);   // #34D399
        public static readonly Color Warning         = Color.FromArgb(245, 158, 11);   // #F59E0B (EXP / Warning logs)
        public static readonly Color Danger          = Color.FromArgb(239, 68, 68);    // #EF4444 (Stop / Error logs / Low HP)
        public static readonly Color DangerHover     = Color.FromArgb(248, 113, 113);  // #F87171
        public static readonly Color Mana            = Color.FromArgb(6, 182, 212);    // #06B6D4 (MP bar / Mana skills)
        public static readonly Color InfoBlue        = Color.FromArgb(56, 189, 248);   // #38BDF8 (Info log)
        public static readonly Color SystemPurple    = Color.FromArgb(168, 85, 247);   // #A855F7 (System log)

        // -------------------------------------------------------------
        // Window & Layout Metrics (8px Grid System)
        // -------------------------------------------------------------
        public const int DefaultWindowWidth  = 1440;
        public const int DefaultWindowHeight = 920;
        public const int HeaderHeight        = 46;
        public const int SidebarWidth        = 210;
        public const int LogPanelHeight      = 150;
        public const int SpacingMicro        = 4;
        public const int SpacingCompact      = 8;
        public const int SpacingDefault      = 16;
        public const int SpacingLarge        = 24;
        public const int DefaultRadius       = 8;

        // -------------------------------------------------------------
        // Typography Factories
        // -------------------------------------------------------------
        private static readonly string[] PreferredFontFamilies = new string[]
        {
            "Segoe UI Variable Display",
            "Segoe UI Variable Text",
            "Segoe UI",
            "Microsoft Sans Serif"
        };

        private static string _resolvedFontFamily = null;

        public static string ResolveFontFamily()
        {
            if (_resolvedFontFamily != null)
                return _resolvedFontFamily;

            foreach (var name in PreferredFontFamilies)
            {
                using (var testFont = new Font(name, 9f))
                {
                    if (string.Equals(testFont.FontFamily.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        _resolvedFontFamily = name;
                        return _resolvedFontFamily;
                    }
                }
            }
            _resolvedFontFamily = "Segoe UI";
            return _resolvedFontFamily;
        }

        public static Font GetFont(float size, FontStyle style = FontStyle.Regular)
        {
            return new Font(ResolveFontFamily(), size, style);
        }

        public static Font FontTitle        => GetFont(11f, FontStyle.Bold);
        public static Font FontHeader       => GetFont(10f, FontStyle.Bold);
        public static Font FontCategory     => GetFont(8f, FontStyle.Bold);
        public static Font FontBody         => GetFont(9f, FontStyle.Regular);
        public static Font FontBodyBold     => GetFont(9f, FontStyle.Bold);
        public static Font FontCaption      => GetFont(8f, FontStyle.Regular);
        public static Font FontCaptionBold  => GetFont(8f, FontStyle.Bold);
        public static Font FontConsole      => new Font("Consolas", 8.5f, FontStyle.Regular);

        // -------------------------------------------------------------
        // GDI+ Geometry Helpers
        // -------------------------------------------------------------
        public static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-left
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private static readonly System.Collections.Generic.HashSet<Control> s_styledControls = new System.Collections.Generic.HashSet<Control>();

        public static void StyleCheckBox(CheckBox cbx)
        {
            if (cbx == null) return;
            cbx.ForeColor = TextPrimary;
            cbx.Font = FontBody;
            cbx.Padding = new Padding(0, 0, 16, 0);

            lock (s_styledControls)
            {
                if (!s_styledControls.Add(cbx))
                    return;
            }

            cbx.Paint += (sender, e) =>
            {
                var cb = (CheckBox)sender;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Color bg = (cb.Parent is GroupBox) ? BgCard : (cb.Parent != null && cb.Parent.BackColor != Color.Transparent ? cb.Parent.BackColor : BgDark);
                using (SolidBrush bgBrush = new SolidBrush(bg))
                {
                    g.FillRectangle(bgBrush, cb.ClientRectangle);
                }

                int boxSize = 14;
                int boxX = 2;
                int boxY = (cb.Height - boxSize) / 2;
                Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);

                if (cb.Checked)
                {
                    using (SolidBrush fillBrush = new SolidBrush(cb.Enabled ? Accent : BorderSubtle))
                    {
                        g.FillRectangle(fillBrush, boxRect);
                    }
                    using (Pen borderPen = new Pen(cb.Enabled ? BorderFocus : BorderSubtle, 1.0f))
                    {
                        g.DrawRectangle(borderPen, boxRect);
                    }
                    using (Pen checkPen = new Pen(cb.Enabled ? Color.White : TextMuted, 2.0f))
                    {
                        PointF p1 = new PointF(boxX + 2.5f, boxY + 7.0f);
                        PointF p2 = new PointF(boxX + 5.5f, boxY + 10.5f);
                        PointF p3 = new PointF(boxX + 11.5f, boxY + 3.5f);
                        g.DrawLine(checkPen, p1, p2);
                        g.DrawLine(checkPen, p2, p3);
                    }
                }
                else
                {
                    using (SolidBrush fillBrush = new SolidBrush(BgInput))
                    {
                        g.FillRectangle(fillBrush, boxRect);
                    }
                    using (Pen borderPen = new Pen(BorderBright, 1.0f))
                    {
                        g.DrawRectangle(borderPen, boxRect);
                    }
                }

                if (!string.IsNullOrEmpty(cb.Text))
                {
                    int textX = boxX + boxSize + 8;
                    Rectangle textRect = new Rectangle(textX, 0, Math.Max(0, cb.Width - textX), cb.Height);
                    TextRenderer.DrawText(g, cb.Text, cb.Font, textRect, cb.Enabled ? TextPrimary : TextMuted,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                }
            };
        }

        public static void StyleRadioButton(RadioButton rbn)
        {
            if (rbn == null) return;
            rbn.ForeColor = TextPrimary;
            rbn.Font = FontBody;
            rbn.Padding = new Padding(0, 0, 16, 0);

            lock (s_styledControls)
            {
                if (!s_styledControls.Add(rbn))
                    return;
            }

            rbn.Paint += (sender, e) =>
            {
                var rb = (RadioButton)sender;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                Color bg = (rb.Parent is GroupBox) ? BgCard : (rb.Parent != null && rb.Parent.BackColor != Color.Transparent ? rb.Parent.BackColor : BgDark);
                using (SolidBrush bgBrush = new SolidBrush(bg))
                {
                    g.FillRectangle(bgBrush, rb.ClientRectangle);
                }

                int circleSize = 14;
                int circleX = 2;
                int circleY = (rb.Height - circleSize) / 2;
                Rectangle circleRect = new Rectangle(circleX, circleY, circleSize, circleSize);

                using (SolidBrush fillBrush = new SolidBrush(BgInput))
                {
                    g.FillEllipse(fillBrush, circleRect);
                }

                if (rb.Checked)
                {
                    using (Pen borderPen = new Pen(rb.Enabled ? BorderFocus : BorderSubtle, 1.5f))
                    {
                        g.DrawEllipse(borderPen, circleRect);
                    }
                    Rectangle innerRect = new Rectangle(circleX + 3, circleY + 3, circleSize - 6, circleSize - 6);
                    using (SolidBrush innerBrush = new SolidBrush(rb.Enabled ? Accent : TextMuted))
                    {
                        g.FillEllipse(innerBrush, innerRect);
                    }
                }
                else
                {
                    using (Pen borderPen = new Pen(BorderBright, 1.0f))
                    {
                        g.DrawEllipse(borderPen, circleRect);
                    }
                }

                if (!string.IsNullOrEmpty(rb.Text))
                {
                    int textX = circleX + circleSize + 8;
                    Rectangle textRect = new Rectangle(textX, 0, Math.Max(0, rb.Width - textX), rb.Height);
                    TextRenderer.DrawText(g, rb.Text, rb.Font, textRect, rb.Enabled ? TextPrimary : TextMuted,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                }
            };
        }
    }
}
