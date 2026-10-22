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
        // Background & Surface Colors (Runtime Light/Dark Palette)
        // -------------------------------------------------------------
        public static Color BgDark          { get { return AppTheme.Colors.Neutral.Canvas; } }
        public static Color BgSidebar       { get { return AppTheme.Colors.Neutral.Surface; } }
        public static Color BgCard          { get { return AppTheme.Colors.Neutral.Surface; } }
        public static Color BgCardHeader    { get { return AppTheme.Colors.Neutral.SurfaceSecondary; } }
        public static Color BgInput         { get { return AppTheme.Colors.Neutral.Surface; } }
        public static Color BgInputFocus    { get { return AppTheme.Colors.Neutral.SurfaceSecondary; } }
        public static Color BgHeader        { get { return AppTheme.Colors.Neutral.Surface; } }

        // -------------------------------------------------------------
        // Border & Divider Colors
        // -------------------------------------------------------------
        public static Color BorderSubtle    { get { return AppTheme.Colors.Neutral.BorderSubtle; } }
        public static Color BorderBright    { get { return AppTheme.Colors.Neutral.BorderDefault; } }
        public static Color BorderFocus     { get { return AppTheme.Colors.Neutral.BorderFocus; } }

        // -------------------------------------------------------------
        // Accent Colors (Neutral Charcoal Primary)
        // -------------------------------------------------------------
        public static Color Accent          { get { return AppTheme.Colors.Neutral.TextPrimary; } }
        public static Color Primary         { get { return AppTheme.Colors.Neutral.TextPrimary; } }
        public static Color AccentHover     { get { return AppTheme.IsDark ? Color.FromArgb(243, 244, 246) : Color.FromArgb(58, 61, 66); } }
        public static Color AccentPressed   { get { return AppTheme.IsDark ? Color.FromArgb(209, 213, 219) : Color.FromArgb(17, 18, 20); } }
        public static Color AccentSubtle    { get { return AppTheme.Colors.Neutral.SurfaceSelected; } }
        public static Color BgTabActive     { get { return AppTheme.Colors.Neutral.Surface; } }
        public static Color BgTabInactive   { get { return AppTheme.Colors.Neutral.Canvas; } }
        public static Color BgTabHover      { get { return AppTheme.Colors.Neutral.SurfaceHover; } }
        public static Color TextTabActive   { get { return AppTheme.Colors.Neutral.TextPrimary; } }
        public static Color TextTabInactive { get { return AppTheme.Colors.Neutral.TextMuted; } }

        // -------------------------------------------------------------
        // Typography Colors
        // -------------------------------------------------------------
        public static Color TextPrimary     { get { return AppTheme.Colors.Neutral.TextPrimary; } }
        public static Color TextSecondary   { get { return AppTheme.Colors.Neutral.TextSecondary; } }
        public static Color TextMuted       { get { return AppTheme.Colors.Neutral.TextMuted; } }
        public static Color TextFaint       { get { return AppTheme.Colors.Neutral.TextDisabled; } }
        public static Color TextInverse     { get { return AppTheme.Colors.Neutral.TextInverse; } }

        // -------------------------------------------------------------
        // Status & Semantic Colors
        // -------------------------------------------------------------
        public static Color Success         { get { return AppTheme.Colors.State.Success; } }
        public static Color SuccessHover    { get { return AppTheme.IsDark ? Color.FromArgb(134, 239, 172) : Color.FromArgb(21, 128, 61); } }
        public static Color Warning         { get { return AppTheme.Colors.State.Warning; } }
        public static Color Danger          { get { return AppTheme.Colors.State.Danger; } }
        public static Color DangerHover     { get { return AppTheme.IsDark ? Color.FromArgb(252, 165, 165) : Color.FromArgb(220, 38, 38); } }
        public static Color Mana            { get { return AppTheme.IsDark ? Color.FromArgb(34, 211, 238) : Color.FromArgb(14, 116, 144); } }
        public static Color InfoBlue        { get { return AppTheme.IsDark ? Color.FromArgb(125, 211, 252) : Color.FromArgb(3, 105, 161); } }
        public static Color SystemPurple    { get { return AppTheme.IsDark ? Color.FromArgb(196, 181, 253) : Color.FromArgb(109, 40, 217); } }
        public static Color RowAlternate    { get { return AppTheme.IsDark ? Color.FromArgb(26, 30, 38) : Color.FromArgb(250, 250, 250); } }
        public static Color RowSelected     { get { return AppTheme.Colors.Neutral.SurfaceSelected; } }
        public static Color RowHover        { get { return AppTheme.Colors.Neutral.SurfaceHover; } }

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
        public const int CardPadding     = 12;
        public const int StatusBarHeight = 28;
        public const int TabStripHeight  = 32;
        public const int SubTabHeight    = 30;

        // -------------------------------------------------------------
        // Typography Factories
        // -------------------------------------------------------------
        private static readonly string[] PreferredFontFamilies = new string[]
        {
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

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, PaintEventHandler> s_paintHandlers =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Control, PaintEventHandler>();

        public static void StyleCheckBox(CheckBox cbx)
        {
            if (cbx == null) return;
            cbx.ForeColor = TextPrimary;
            cbx.Font = FontBody;
            cbx.Padding = new Padding(0, 0, 16, 0);

            PaintEventHandler existing;
            if (s_paintHandlers.TryGetValue(cbx, out existing)) return;

            PaintEventHandler handler = (sender, e) =>
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
                    using (Pen checkPen = new Pen(cb.Enabled ? TextInverse : TextMuted, 2.0f))
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
            s_paintHandlers.Add(cbx, handler);
            cbx.Paint += handler;
        }

        public static void UnstyleCheckBox(CheckBox cbx)
        {
            if (cbx == null) return;
            PaintEventHandler handler;
            if (s_paintHandlers.TryGetValue(cbx, out handler))
            {
                cbx.Paint -= handler;
                s_paintHandlers.Remove(cbx);
                cbx.Padding = new Padding(0);
            }
        }

        public static void StyleRadioButton(RadioButton rbn)
        {
            if (rbn == null) return;
            rbn.ForeColor = TextPrimary;
            rbn.Font = FontBody;
            rbn.Padding = new Padding(0, 0, 16, 0);

            PaintEventHandler existing;
            if (s_paintHandlers.TryGetValue(rbn, out existing)) return;

            PaintEventHandler handler = (sender, e) =>
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
            s_paintHandlers.Add(rbn, handler);
            rbn.Paint += handler;
        }

        public static void UnstyleRadioButton(RadioButton rbn)
        {
            if (rbn == null) return;
            PaintEventHandler handler;
            if (s_paintHandlers.TryGetValue(rbn, out handler))
            {
                rbn.Paint -= handler;
                s_paintHandlers.Remove(rbn);
                rbn.Padding = new Padding(0);
            }
        }

        public static void StyleTreeView(TreeView tv)
        {
            if (tv == null) return;
            tv.BackColor = BgCard;
            tv.ForeColor = TextPrimary;
            tv.BorderStyle = BorderStyle.FixedSingle;
            tv.Font = FontBody;
            tv.LineColor = BorderSubtle;
            tv.ItemHeight = 22;
        }

        public static void StyleListBox(ListBox lb)
        {
            if (lb == null) return;
            lb.BackColor = BgCard;
            lb.ForeColor = TextPrimary;
            lb.BorderStyle = BorderStyle.FixedSingle;
            lb.Font = FontBody;
            lb.ItemHeight = 20;
        }
    }
}
