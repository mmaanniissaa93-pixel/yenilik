using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    public enum ThemeMode
    {
        Light,
        Dark
    }

    /// <summary>
    /// Centralized UI design system for xBot WinForms desktop application.
    /// Calm, professional, low-visual-noise, nearly monochrome desktop palette.
    /// Structured source of truth: 95% Neutral Grayscale, 5% Semantic State colors.
    /// Zero decorative brand colors. Hierarchy driven by spacing, weight, and subtle contrast.
    /// </summary>
    public static class AppTheme
    {
        private static ThemeMode _mode = ThemeMode.Light;

        public static ThemeMode Mode
        {
            get { return _mode; }
        }

        public static bool IsDark
        {
            get { return _mode == ThemeMode.Dark; }
        }

        public static event Action ThemeChanged;

        public static void SetMode(ThemeMode mode)
        {
            if (_mode == mode) return;
            _mode = mode;
            Action handler = ThemeChanged;
            if (handler != null) handler();
        }

        // =============================================================
        // 1. Colors (95% Neutral, 5% Semantic State)
        // =============================================================
        public static class Colors
        {
            public static class Neutral
            {
                // Canvas & Surfaces
                public static Color Canvas           { get { return IsDark ? DarkPalette.Canvas : LightPalette.Canvas; } }
                public static Color Surface          { get { return IsDark ? DarkPalette.Surface : LightPalette.Surface; } }
                public static Color SurfaceSecondary { get { return IsDark ? DarkPalette.SurfaceSecondary : LightPalette.SurfaceSecondary; } }
                public static Color SurfaceHover     { get { return IsDark ? DarkPalette.SurfaceHover : LightPalette.SurfaceHover; } }
                public static Color SurfaceSelected  { get { return IsDark ? DarkPalette.SurfaceSelected : LightPalette.SurfaceSelected; } }
                public static Color SurfacePressed   { get { return IsDark ? DarkPalette.SurfacePressed : LightPalette.SurfacePressed; } }

                // Borders & Dividers
                public static Color BorderDefault    { get { return IsDark ? DarkPalette.BorderDefault : LightPalette.BorderDefault; } }
                public static Color BorderStrong     { get { return IsDark ? DarkPalette.BorderStrong : LightPalette.BorderStrong; } }
                public static Color BorderSubtle     { get { return IsDark ? DarkPalette.BorderSubtle : LightPalette.BorderSubtle; } }
                public static Color BorderHover      { get { return IsDark ? DarkPalette.BorderHover : LightPalette.BorderHover; } }
                public static Color BorderFocus      { get { return IsDark ? DarkPalette.BorderFocus : LightPalette.BorderFocus; } }
                public static Color SelectionIndicator { get { return IsDark ? DarkPalette.SelectionIndicator : LightPalette.SelectionIndicator; } }

                // Typography Colors (WCAG AAA/AA compliant on the active surface)
                public static Color TextPrimary      { get { return IsDark ? DarkPalette.TextPrimary : LightPalette.TextPrimary; } }
                public static Color TextSecondary    { get { return IsDark ? DarkPalette.TextSecondary : LightPalette.TextSecondary; } }
                public static Color TextMuted        { get { return IsDark ? DarkPalette.TextMuted : LightPalette.TextMuted; } }
                public static Color TextDisabled     { get { return IsDark ? DarkPalette.TextDisabled : LightPalette.TextDisabled; } }
                public static Color TextInverse      { get { return IsDark ? DarkPalette.TextInverse : LightPalette.TextInverse; } }
            }

            public static class State
            {
                // Semantic Status Colors (Only for badges, dots, and functional states — no decorative use)
                public static Color Success          { get { return IsDark ? DarkPalette.Success : LightPalette.Success; } }
                public static Color Warning          { get { return IsDark ? DarkPalette.Warning : LightPalette.Warning; } }
                public static Color Danger           { get { return IsDark ? DarkPalette.Danger : LightPalette.Danger; } }
                public static Color DangerBorder     { get { return IsDark ? DarkPalette.DangerBorder : LightPalette.DangerBorder; } }
                public static Color DangerHoverBg    { get { return IsDark ? DarkPalette.DangerHoverBg : LightPalette.DangerHoverBg; } }
            }

            // Direct properties on Colors for convenient access and backward compatibility
            public static Color Canvas           => Neutral.Canvas;
            public static Color Surface          => Neutral.Surface;
            public static Color SurfaceSecondary => Neutral.SurfaceSecondary;
            public static Color SurfaceHover     => Neutral.SurfaceHover;
            public static Color SurfaceSelected  => Neutral.SurfaceSelected;
            public static Color SurfaceActive    => Neutral.SurfacePressed;

            public static Color BorderDefault    => Neutral.BorderDefault;
            public static Color BorderStrong     => Neutral.BorderStrong;
            public static Color BorderSubtle     => Neutral.BorderSubtle;
            public static Color BorderHover      => Neutral.BorderHover;
            public static Color BorderFocus      => Neutral.BorderFocus;
            public static Color BorderDanger     => State.DangerBorder;
            public static Color SelectionIndicator => Neutral.SelectionIndicator;

            public static Color TextPrimary      => Neutral.TextPrimary;
            public static Color TextSecondary    => Neutral.TextSecondary;
            public static Color TextMuted        => Neutral.TextMuted;
            public static Color TextDisabled     => Neutral.TextDisabled;
            public static Color TextInverse      => Neutral.TextInverse;

            public static Color Success          => State.Success;
            public static Color Warning          => State.Warning;
            public static Color Danger           => State.Danger;
            public static Color DangerText       => State.Danger;
            public static Color DangerHoverBg    => State.DangerHoverBg;

            // Log stream colors (mostly monochrome, functional semantics only)
            public static Color LogTimestamp     => Neutral.TextMuted;     // #7A8088
            public static Color LogInfo          => Neutral.TextPrimary;   // #202124 (neutral text)
            public static Color LogSystem        => Neutral.TextSecondary; // #50545B (secondary charcoal)
            public static Color LogSuccess       => State.Success;         // #166534
            public static Color LogWarning       => State.Warning;         // #B45309
            public static Color LogError         => State.Danger;          // #B91C1C
        }

        private static class LightPalette
        {
            public static readonly Color Canvas           = Color.FromArgb(243, 244, 245);
            public static readonly Color Surface          = Color.FromArgb(255, 255, 255);
            public static readonly Color SurfaceSecondary = Color.FromArgb(248, 248, 248);
            public static readonly Color SurfaceHover     = Color.FromArgb(242, 242, 242);
            public static readonly Color SurfaceSelected  = Color.FromArgb(236, 237, 239);
            public static readonly Color SurfacePressed   = Color.FromArgb(228, 229, 231);

            public static readonly Color BorderDefault    = Color.FromArgb(214, 216, 220);
            public static readonly Color BorderStrong     = Color.FromArgb(174, 180, 188);
            public static readonly Color BorderSubtle     = Color.FromArgb(229, 230, 232);
            public static readonly Color BorderHover      = Color.FromArgb(188, 193, 199);
            public static readonly Color BorderFocus      = Color.FromArgb(139, 145, 153);
            public static readonly Color SelectionIndicator = Color.FromArgb(107, 112, 120);

            public static readonly Color TextPrimary      = Color.FromArgb(32, 33, 36);
            public static readonly Color TextSecondary    = Color.FromArgb(80, 84, 91);
            public static readonly Color TextMuted        = Color.FromArgb(122, 128, 136);
            public static readonly Color TextDisabled     = Color.FromArgb(136, 142, 150);
            public static readonly Color TextInverse      = Color.FromArgb(255, 255, 255);

            public static readonly Color Success          = Color.FromArgb(22, 101, 52);
            public static readonly Color Warning          = Color.FromArgb(180, 83, 9);
            public static readonly Color Danger           = Color.FromArgb(185, 28, 28);
            public static readonly Color DangerBorder     = Color.FromArgb(252, 165, 165);
            public static readonly Color DangerHoverBg    = Color.FromArgb(254, 242, 242);
        }

        private static class DarkPalette
        {
            public static readonly Color Canvas           = Color.FromArgb(15, 17, 23);
            public static readonly Color Surface          = Color.FromArgb(22, 26, 33);
            public static readonly Color SurfaceSecondary = Color.FromArgb(28, 33, 42);
            public static readonly Color SurfaceHover     = Color.FromArgb(35, 42, 53);
            public static readonly Color SurfaceSelected  = Color.FromArgb(42, 49, 61);
            public static readonly Color SurfacePressed   = Color.FromArgb(51, 59, 73);

            public static readonly Color BorderDefault    = Color.FromArgb(52, 59, 71);
            public static readonly Color BorderStrong     = Color.FromArgb(74, 83, 98);
            public static readonly Color BorderSubtle     = Color.FromArgb(37, 43, 53);
            public static readonly Color BorderHover      = Color.FromArgb(89, 99, 116);
            public static readonly Color BorderFocus      = Color.FromArgb(138, 148, 163);
            public static readonly Color SelectionIndicator = Color.FromArgb(182, 191, 204);

            public static readonly Color TextPrimary      = Color.FromArgb(242, 244, 247);
            public static readonly Color TextSecondary    = Color.FromArgb(199, 205, 214);
            public static readonly Color TextMuted        = Color.FromArgb(146, 155, 168);
            public static readonly Color TextDisabled     = Color.FromArgb(108, 118, 133);
            public static readonly Color TextInverse      = Color.FromArgb(17, 19, 23);

            public static readonly Color Success          = Color.FromArgb(74, 222, 128);
            public static readonly Color Warning          = Color.FromArgb(251, 191, 36);
            public static readonly Color Danger           = Color.FromArgb(248, 113, 113);
            public static readonly Color DangerBorder     = Color.FromArgb(127, 29, 29);
            public static readonly Color DangerHoverBg    = Color.FromArgb(42, 22, 24);
        }

        // =============================================================
        // 2. Typography Scale (Standard Windows Segoe UI + Consolas)
        // =============================================================
        public static class Typography
        {
            public static readonly Font FontBody          = SafeCreateFont("Segoe UI", 9.0f, FontStyle.Regular);
            public static readonly Font FontBodyStrong    = SafeCreateFont("Segoe UI", 9.0f, FontStyle.Bold);
            public static readonly Font FontSection       = SafeCreateFont("Segoe UI", 9.5f, FontStyle.Bold);
            public static readonly Font FontSidebar       = SafeCreateFont("Segoe UI", 8.75f, FontStyle.Regular);
            public static readonly Font FontSidebarActive = SafeCreateFont("Segoe UI", 8.75f, FontStyle.Bold);
            public static readonly Font FontButton        = SafeCreateFont("Segoe UI", 8.5f, FontStyle.Regular);
            public static readonly Font FontButtonStrong  = SafeCreateFont("Segoe UI", 8.5f, FontStyle.Bold);
            public static readonly Font FontCaption       = SafeCreateFont("Segoe UI", 8.0f, FontStyle.Regular);
            public static readonly Font FontMonospace     = SafeCreateFont("Consolas", 8.5f, FontStyle.Regular);

            public static Font SafeCreateFont(string familyName, float emSize, FontStyle style)
            {
                try
                {
                    return new Font(familyName, emSize, style, GraphicsUnit.Point);
                }
                catch
                {
                    try
                    {
                        return new Font(SystemFonts.DefaultFont.FontFamily, emSize, style, GraphicsUnit.Point);
                    }
                    catch
                    {
                        return SystemFonts.DefaultFont;
                    }
                }
            }
        }

        // =============================================================
        // 3. Spacing Scale (4, 8, 12, 16, 24px)
        // =============================================================
        public static class Spacing
        {
            public const int XS = 4;  // Gap between icon and text, badge padding
            public const int S  = 8;  // Gap between controls in a row, label-to-input gap
            public const int M  = 12; // GroupBox inner padding, section spacing
            public const int L  = 16; // Outer panel margin, main view margins
            public const int XL = 24; // Major section separation
        }

        // =============================================================
        // 4. Desktop Layout Metrics
        // =============================================================
        public static class Metrics
        {
            public const int SidebarWidth         = 182;
            public const int SidebarItemHeight    = 26;
            public const int SidebarItemStep      = 27;
            public const int InputHeight          = 24;
            public const int ButtonHeight         = 26;
            public const int ActionButtonWidth    = 103;
            public const int ActionButtonHeight   = 27;
            public const int ActionsPanelWidth    = 212;
            public const int ActionsPanelHeight   = 108;
            public const int LogPanelHeight       = 108;
            public const int BorderThickness      = 1;
            public const int IndicatorBarWidth    = 3;
            public const int IndicatorBarHeight   = 16;
        }

        // =============================================================
        // 5. Component Style Helpers
        // =============================================================
        public static class ComponentStyles
        {
            /// <summary>
            /// Styles a bottom action button in neutral desktop utility style.
            /// All 6 actions share identical typography (Segoe UI 8.5pt Regular), identical background (#F8F8F8),
            /// identical 1px border (#D6D8DC), and identical text color (#202124).
            /// Enabled/Disabled state controls interaction. No artificial primary CTA dominance.
            /// </summary>
            public static void StyleActionButton(Button b, bool enabled, bool subtleDangerHover = false)
            {
                if (b == null) return;
                b.UseVisualStyleBackColor = false;
                b.Font = Typography.FontButton;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = Metrics.BorderThickness;
                b.Padding = Padding.Empty;
                b.Margin = Padding.Empty;

                if (enabled)
                {
                    b.Enabled = true;
                    b.BackColor = Colors.Neutral.SurfaceSecondary;
                    b.ForeColor = Colors.Neutral.TextPrimary;
                    b.FlatAppearance.BorderColor = Colors.Neutral.BorderDefault;
                    b.FlatAppearance.MouseOverBackColor = subtleDangerHover ? Colors.State.DangerHoverBg : Colors.Neutral.SurfaceHover;
                    b.FlatAppearance.MouseDownBackColor = Colors.Neutral.SurfacePressed;
                }
                else
                {
                    b.Enabled = false;
                    b.BackColor = Colors.Neutral.Canvas;
                    b.ForeColor = IsDark ? Colors.Neutral.TextMuted : Colors.Neutral.TextDisabled;
                    b.FlatAppearance.BorderColor = Colors.Neutral.BorderSubtle;
                    b.FlatAppearance.MouseOverBackColor = Colors.Neutral.Canvas;
                    b.FlatAppearance.MouseDownBackColor = Colors.Neutral.Canvas;
                }
                b.FlatStyle = FlatStyle.Flat;
            }

            public static void StylePrimaryButton(Button b, bool isRunning)
            {
                // Start Bot: enabled neutral when !isRunning, disabled neutral when isRunning
                StyleActionButton(b, !isRunning, false);
            }

            public static void StyleDestructiveButton(Button b, bool isRunning)
            {
                // Stop Bot: enabled neutral when isRunning, disabled neutral when !isRunning
                StyleActionButton(b, isRunning, true);
            }

            public static void StyleSecondaryButton(Button b)
            {
                StyleActionButton(b, true, false);
            }

            public static void StyleDestructiveSecondaryButton(Button b)
            {
                StyleActionButton(b, true, true);
            }

            public static void StyleTextBox(TextBox tb)
            {
                if (tb == null) return;
                tb.Font = Typography.FontBody;
                tb.BorderStyle = BorderStyle.FixedSingle;
                tb.BackColor = Colors.Neutral.Surface;
                tb.ForeColor = Colors.Neutral.TextPrimary;
            }

            public static void StyleComboBox(ComboBox cb)
            {
                if (cb == null) return;
                cb.Font = Typography.FontBody;
                cb.FlatStyle = FlatStyle.Standard;
                cb.BackColor = Colors.Neutral.Surface;
                cb.ForeColor = Colors.Neutral.TextPrimary;
            }

            public static void StyleListView(ListView lv)
            {
                if (lv == null) return;
                lv.Font = Typography.FontBody;
                lv.OwnerDraw = false;
                lv.BorderStyle = BorderStyle.FixedSingle;
                lv.FullRowSelect = true;
                lv.HideSelection = false;
                lv.GridLines = false;
                lv.BackColor = Colors.Neutral.Surface;
                lv.ForeColor = Colors.Neutral.TextPrimary;
                ThemeManager.ApplyListView(lv);
            }

            public static void StyleSectionHeader(Label l)
            {
                if (l == null) return;
                l.Font = Typography.FontSection;
                l.ForeColor = Colors.Neutral.TextPrimary;
                l.BackColor = Color.Transparent;
            }

            public static void StyleSubTabControl(TabControl tc)
            {
                if (tc == null) return;
                tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                tc.Appearance = TabAppearance.FlatButtons;
                tc.Padding = new Point(12, 4);
                tc.ItemSize = new Size(Math.Max(72, tc.ItemSize.Width), Math.Max(26, tc.Font.Height + 10));
                tc.SizeMode = TabSizeMode.Normal;
                tc.DrawItem -= SubTabControl_DrawItem;
                tc.DrawItem += SubTabControl_DrawItem;
                tc.MouseMove -= SubTabControl_MouseMove;
                tc.MouseMove += SubTabControl_MouseMove;
                tc.MouseLeave -= SubTabControl_MouseLeave;
                tc.MouseLeave += SubTabControl_MouseLeave;
                tc.HandleCreated -= SubTabControl_HandleCreated;
                tc.HandleCreated += SubTabControl_HandleCreated;
                HideFocusCues(tc);
            }

            private const int WM_CHANGEUISTATE = 0x0127;
            private const int UIS_SET = 1;
            private const int UISF_HIDEFOCUS = 1;

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            private sealed class SubTabHover
            {
                public int Index = -1;
            }

            private static readonly ConditionalWeakTable<TabControl, SubTabHover> SubTabHoverStates =
                new ConditionalWeakTable<TabControl, SubTabHover>();

            private static void SubTabControl_HandleCreated(object sender, EventArgs e)
            {
                HideFocusCues(sender as Control);
            }

            private static void HideFocusCues(Control control)
            {
                if (control == null || !control.IsHandleCreated) return;
                SendMessage(control.Handle, WM_CHANGEUISTATE,
                    (IntPtr)(UIS_SET | (UISF_HIDEFOCUS << 16)), IntPtr.Zero);
            }

            private static void SubTabControl_MouseMove(object sender, MouseEventArgs e)
            {
                TabControl tc = sender as TabControl;
                if (tc == null) return;
                int hover = -1;
                for (int i = 0; i < tc.TabCount; i++)
                {
                    if (tc.GetTabRect(i).Contains(e.Location)) { hover = i; break; }
                }
                SubTabHover state = SubTabHoverStates.GetOrCreateValue(tc);
                if (state.Index == hover) return;
                state.Index = hover;
                InvalidateTabStrip(tc);
            }

            private static void SubTabControl_MouseLeave(object sender, EventArgs e)
            {
                TabControl tc = sender as TabControl;
                if (tc == null) return;
                SubTabHover state = SubTabHoverStates.GetOrCreateValue(tc);
                if (state.Index < 0) return;
                state.Index = -1;
                InvalidateTabStrip(tc);
            }

            private static void InvalidateTabStrip(TabControl tc)
            {
                int height = Math.Max(tc.ItemSize.Height + (tc.Padding.Y * 2) + 4, 32);
                tc.Invalidate(new Rectangle(0, 0, tc.Width, height));
            }

            private static void SubTabControl_DrawItem(object sender, DrawItemEventArgs e)
            {
                TabControl tc = sender as TabControl;
                if (tc == null || e.Index < 0 || e.Index >= tc.TabPages.Count) return;
                TabPage page = tc.TabPages[e.Index];
                bool isSelected = (tc.SelectedIndex == e.Index);
                SubTabHover state = SubTabHoverStates.GetOrCreateValue(tc);
                bool isHovered = !isSelected && state.Index == e.Index;
                Rectangle bounds = e.Bounds;

                Color bg = isSelected ? Colors.Neutral.Surface
                    : isHovered ? Colors.Neutral.SurfaceHover : Colors.Neutral.Canvas;
                Color textCol = isSelected ? Colors.Neutral.TextPrimary
                    : isHovered ? Colors.Neutral.TextPrimary : Colors.Neutral.TextSecondary;
                using (SolidBrush bb = new SolidBrush(bg))
                {
                    e.Graphics.FillRectangle(bb, bounds);
                }

                TextRenderer.DrawText(e.Graphics, page.Text, tc.Font,
                    bounds, textCol, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

                if (isSelected)
                {
                    using (Pen p = new Pen(Colors.Neutral.BorderStrong, 2f))
                    {
                        e.Graphics.DrawLine(p, bounds.Left + 10, bounds.Bottom - 1, bounds.Right - 10, bounds.Bottom - 1);
                    }
                }
            }
        }

        // =============================================================
        // 6. Direct Aliases for Clean Backward-Compatibility
        // =============================================================
        public static Color Background        => Colors.Neutral.Canvas;
        public static Color Surface           => Colors.Neutral.Surface;
        public static Color SurfaceAlt        => Colors.Neutral.SurfaceSecondary;
        public static Color SurfaceHover      => Colors.Neutral.SurfaceHover;
        public static Color SurfaceSelected   => Colors.Neutral.SurfaceSelected;
        public static Color SurfaceActive     => Colors.Neutral.SurfacePressed;

        public static Color BorderDefault     => Colors.Neutral.BorderDefault;
        public static Color BorderStrong      => Colors.Neutral.BorderStrong;
        public static Color BorderSubtle      => Colors.Neutral.BorderSubtle;
        public static Color BorderHover       => Colors.Neutral.BorderHover;
        public static Color BorderFocus       => Colors.Neutral.BorderFocus;
        public static Color SelectionIndicator => Colors.Neutral.SelectionIndicator;

        public static Color TextPrimary       => Colors.Neutral.TextPrimary;
        public static Color TextSecondary     => Colors.Neutral.TextSecondary;
        public static Color TextMuted         => Colors.Neutral.TextMuted;
        public static Color TextDisabled      => Colors.Neutral.TextDisabled;
        public static Color TextActive        => Colors.Neutral.TextPrimary;
        public static Color TextInverse       => Colors.Neutral.TextInverse;

        public static Color LogTimestamp      => Colors.LogTimestamp;
        public static Color LogInfo           => Colors.LogInfo;
        public static Color LogSuccess        => Colors.LogSuccess;
        public static Color LogWarning        => Colors.LogWarning;
        public static Color LogError          => Colors.LogError;
        public static Color LogSystem         => Colors.LogSystem;

        public static Font FontBody           => Typography.FontBody;
        public static Font FontBodyBold       => Typography.FontBodyStrong;
        public static Font FontSidebar        => Typography.FontSidebar;
        public static Font FontSidebarActive  => Typography.FontSidebarActive;
        public static Font FontButton         => Typography.FontButton;
        public static Font FontButtonBold     => Typography.FontButtonStrong;
        public static Font FontSubTab         => Typography.FontBody;
        public static Font FontGroupTitle     => Typography.FontSection;
        public static Font FontCaption        => Typography.FontCaption;
        public static Font Monospace          => Typography.FontMonospace;
        public static Font FontMonospace      => Typography.FontMonospace;

        public const int SpacingMicro         = Spacing.XS;
        public const int SpacingCompact       = Spacing.S;
        public const int SidebarWidth         = Metrics.SidebarWidth;
        public const int ActionButtonWidth    = Metrics.ActionButtonWidth;
        public const int ActionButtonHeight   = Metrics.ActionButtonHeight;

        public static Color StartBotBg        => Colors.Neutral.SurfaceSecondary;
        public static Color StartBotText      => Colors.Neutral.TextPrimary;
        public static Color StartBotBorder    => Colors.Neutral.BorderDefault;
        public static Color StartBotHoverBg   => Colors.Neutral.SurfaceHover;
        public static Color StartBotPressedBg => Colors.Neutral.SurfacePressed;

        public static Color StopBotBg         => Colors.Neutral.SurfaceSecondary;
        public static Color StopBotText       => Colors.Neutral.TextPrimary;
        public static Color StopBotBorder     => Colors.Neutral.BorderDefault;
        public static Color StopBotHoverBg    => Colors.Neutral.SurfaceHover;
        public static Color StopBotPressedBg  => Colors.Neutral.SurfacePressed;

        public static Color ActionSecondaryBg        => Colors.Neutral.SurfaceSecondary;
        public static Color ActionSecondaryHoverBg   => Colors.Neutral.SurfaceHover;
        public static Color ActionSecondaryPressedBg => Colors.Neutral.SurfacePressed;
    }
}

