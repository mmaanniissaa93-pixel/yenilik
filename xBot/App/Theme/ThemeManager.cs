using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    /// <summary>
    /// Runtime theme switching for the existing WinForms control tree.
    /// AppTheme owns the palette; this class persists the choice and maps
    /// legacy hard-coded light colors onto the active neutral palette.
    /// </summary>
    public static class ThemeManager
    {
        private static readonly string ThemePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "xBot", "ui-theme.txt");

        private static bool _loaded;

        private static readonly ConditionalWeakTable<Control, object> Watched =
            new ConditionalWeakTable<Control, object>();

        public static void Initialize()
        {
            if (_loaded) return;
            _loaded = true;
            Load();
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(ThemePath)) return;
                string value = File.ReadAllText(ThemePath).Trim();
                AppTheme.SetMode(string.Equals(value, "dark", StringComparison.OrdinalIgnoreCase)
                    ? ThemeMode.Dark : ThemeMode.Light);
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(ThemePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(ThemePath, AppTheme.IsDark ? "dark" : "light");
            }
            catch { }
        }

        public static void SetMode(ThemeMode mode)
        {
            AppTheme.SetMode(mode);
            Save();
        }

        public static void Apply(Control root)
        {
            if (root == null) return;
            try
            {
                Form form = root as Form;
                if (form != null) ApplyWindowChrome(form);
                ApplyControl(root);
            }
            catch { }
            try { Watch(root); } catch { }
            foreach (Control child in root.Controls)
            {
                try { Apply(child); } catch { }
            }
        }

        private static void Watch(Control control)
        {
            if (control == null) return;
            object existing;
            if (Watched.TryGetValue(control, out existing)) return;

            Watched.Add(control, new object());
            control.ControlAdded += Control_Added;
            foreach (Control child in control.Controls) Watch(child);
        }

        private static void Control_Added(object sender, ControlEventArgs e)
        {
            if (e == null || e.Control == null) return;
            Apply(e.Control);
        }

        public static void ApplyWindowChrome(Form form)
        {
            if (form == null) return;
            try
            {
                int dark = AppTheme.IsDark ? 1 : 0;
                int result = DwmSetWindowAttribute(form.Handle, 20, ref dark, sizeof(int));
                if (result != 0)
                    DwmSetWindowAttribute(form.Handle, 19, ref dark, sizeof(int));
            }
            catch { }
        }

        public static void ApplyListView(ListView list)
        {
            if (list == null) return;
            bool dark = AppTheme.IsDark;
            list.GridLines = false;

            list.DrawColumnHeader -= ListView_DrawColumnHeader;
            list.DrawItem -= ListView_DrawItem;
            list.DrawSubItem -= ListView_DrawSubItem;

            if (!dark)
            {
                list.OwnerDraw = false;
                list.BackColor = AppTheme.Colors.Surface;
                list.ForeColor = AppTheme.Colors.TextPrimary;
                list.Invalidate();
                return;
            }

            list.BackColor = AppTheme.Colors.Surface;
            list.ForeColor = AppTheme.Colors.TextPrimary;
            list.OwnerDraw = true;
            list.DrawColumnHeader += ListView_DrawColumnHeader;
            list.DrawItem += ListView_DrawItem;
            list.DrawSubItem += ListView_DrawSubItem;

            if (list.IsHandleCreated && list.Columns.Count > 0)
            {
                int total = 0;
                foreach (ColumnHeader column in list.Columns) total += column.Width;
                int extra = list.ClientSize.Width - total;
                if (extra > 0)
                {
                    int last = list.Columns.Count - 1;
                    SendMessage(list.Handle, LVM_SETCOLUMNWIDTH,
                        (IntPtr)last, (IntPtr)(list.Columns[last].Width + extra));
                }
            }
            list.Invalidate();
        }

        private static void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            ListView list = sender as ListView;
            if (list == null) return;

            Rectangle bounds = e.Bounds;
            if (e.ColumnIndex == list.Columns.Count - 1)
                bounds.Width = Math.Max(0, list.ClientSize.Width - bounds.Left);

            using (SolidBrush fill = new SolidBrush(AppTheme.Colors.SurfaceSecondary))
                e.Graphics.FillRectangle(fill, bounds);

            using (Pen line = new Pen(AppTheme.Colors.BorderSubtle))
            {
                e.Graphics.DrawLine(line, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
                if (e.ColumnIndex > 0)
                    e.Graphics.DrawLine(line, bounds.Left, bounds.Top + 4, bounds.Left, bounds.Bottom - 4);
            }

            Rectangle text = Rectangle.Inflate(bounds, -6, -2);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, text,
                AppTheme.Colors.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private static void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private static void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private static void ApplyControl(Control c)
        {
            if (c == null) return;
            bool dark = AppTheme.IsDark;
            Button button = c as Button;
            bool accentButton = button != null && Near(button.BackColor, AppTheme.Colors.Neutral.TextPrimary);

            if (!accentButton && c.BackColor != Color.Transparent && c.BackColor != Color.Empty)
            {
                Color mapped = MapSurface(c.BackColor, dark);
                if (mapped != c.BackColor) c.BackColor = mapped;
            }
            if (!accentButton && c.ForeColor != Color.Empty)
            {
                Color mapped = MapText(c.ForeColor, dark);
                if (mapped != c.ForeColor) c.ForeColor = mapped;
            }

            if (button != null && button.FlatStyle == FlatStyle.Flat)
            {
                button.FlatAppearance.BorderColor = MapBorder(button.FlatAppearance.BorderColor, dark);
                if (button.FlatAppearance.MouseOverBackColor != Color.Empty)
                    button.FlatAppearance.MouseOverBackColor = MapSurface(button.FlatAppearance.MouseOverBackColor, dark);
                if (button.FlatAppearance.MouseDownBackColor != Color.Empty)
                    button.FlatAppearance.MouseDownBackColor = MapSurface(button.FlatAppearance.MouseDownBackColor, dark);
            }

            LinkLabel link = c as LinkLabel;
            if (link != null)
            {
                link.LinkColor = MapText(link.LinkColor, dark);
                link.ActiveLinkColor = MapText(link.ActiveLinkColor, dark);
                link.VisitedLinkColor = MapText(link.VisitedLinkColor, dark);
            }

            TreeView tree = c as TreeView;
            if (tree != null)
            {
                tree.LineColor = MapBorder(tree.LineColor, dark);
            }

            DataGridView grid = c as DataGridView;
            if (grid != null) ApplyGrid(grid, dark);

            ListView list = c as ListView;
            if (list != null) ApplyListView(list);

            CheckBox checkBox = c as CheckBox;
            if (checkBox != null)
            {
                if (dark) DarkTheme.StyleCheckBox(checkBox);
                else DarkTheme.UnstyleCheckBox(checkBox);
            }

            RadioButton radioButton = c as RadioButton;
            if (radioButton != null)
            {
                if (dark) DarkTheme.StyleRadioButton(radioButton);
                else DarkTheme.UnstyleRadioButton(radioButton);
            }

            ToolStrip strip = c as ToolStrip;
            if (strip != null)
            {
                strip.BackColor = MapSurface(strip.BackColor, dark);
                strip.ForeColor = MapText(strip.ForeColor, dark);
                foreach (ToolStripItem item in strip.Items) ApplyToolStripItem(item, dark);
            }
        }

        private static void ApplyGrid(DataGridView grid, bool dark)
        {
            grid.BackgroundColor = MapSurface(grid.BackgroundColor, dark);
            grid.GridColor = MapBorder(grid.GridColor, dark);
            grid.EnableHeadersVisualStyles = !dark;

            DataGridViewCellStyle cells = grid.DefaultCellStyle;
            cells.BackColor = MapSurface(cells.BackColor, dark);
            cells.ForeColor = MapText(cells.ForeColor, dark);
            cells.SelectionBackColor = dark ? AppTheme.Colors.Neutral.SurfaceSelected : Color.FromArgb(236, 237, 239);
            cells.SelectionForeColor = AppTheme.Colors.Neutral.TextPrimary;

            DataGridViewCellStyle headers = grid.ColumnHeadersDefaultCellStyle;
            headers.BackColor = dark ? AppTheme.Colors.Neutral.SurfaceSecondary : Color.FromArgb(248, 248, 248);
            headers.ForeColor = AppTheme.Colors.Neutral.TextPrimary;

            DataGridViewCellStyle rows = grid.RowHeadersDefaultCellStyle;
            rows.BackColor = dark ? AppTheme.Colors.Neutral.SurfaceSecondary : Color.FromArgb(248, 248, 248);
            rows.ForeColor = AppTheme.Colors.Neutral.TextPrimary;

            DataGridViewCellStyle alternating = grid.AlternatingRowsDefaultCellStyle;
            alternating.BackColor = dark ? Color.FromArgb(26, 30, 38) : Color.FromArgb(250, 250, 250);
            alternating.ForeColor = AppTheme.Colors.Neutral.TextPrimary;

            ContextMenuStrip menu = grid.ContextMenuStrip;
            if (menu != null)
            {
                menu.BackColor = AppTheme.Colors.Neutral.Surface;
                menu.ForeColor = AppTheme.Colors.Neutral.TextPrimary;
                foreach (ToolStripItem item in menu.Items) ApplyToolStripItem(item, dark);
            }
        }

        private static void ApplyToolStripItem(ToolStripItem item, bool dark)
        {
            if (item == null) return;
            item.BackColor = MapSurface(item.BackColor, dark);
            item.ForeColor = MapText(item.ForeColor, dark);
            ToolStripDropDownItem drop = item as ToolStripDropDownItem;
            if (drop != null)
            {
                drop.DropDown.BackColor = AppTheme.Colors.Neutral.Surface;
                drop.DropDown.ForeColor = AppTheme.Colors.Neutral.TextPrimary;
                foreach (ToolStripItem child in drop.DropDownItems) ApplyToolStripItem(child, dark);
            }
        }

        private static Color MapSurface(Color c, bool dark)
        {
            if (c == Color.Transparent || c == Color.Empty) return c;
            if (dark)
            {
                if (Near(c, Color.White) || Near(c, Color.FromArgb(255, 255, 255))) return AppTheme.Colors.Neutral.Surface;
                if (Near(c, Color.FromArgb(243, 244, 245))) return AppTheme.Colors.Neutral.Canvas;
                if (Near(c, Color.FromArgb(243, 243, 243))) return AppTheme.Colors.Neutral.Canvas;
                if (Near(c, Color.FromArgb(248, 248, 248))) return AppTheme.Colors.Neutral.SurfaceSecondary;
                if (Near(c, Color.FromArgb(250, 250, 250))) return AppTheme.Colors.Neutral.SurfaceSecondary;
                if (Near(c, Color.FromArgb(246, 247, 248))) return AppTheme.Colors.Neutral.SurfaceSecondary;
                if (Near(c, Color.FromArgb(242, 242, 242))) return AppTheme.Colors.Neutral.SurfaceHover;
                if (Near(c, Color.FromArgb(245, 245, 245))) return AppTheme.Colors.Neutral.SurfaceHover;
                if (Near(c, Color.FromArgb(249, 249, 249))) return AppTheme.Colors.Neutral.SurfaceHover;
                if (Near(c, Color.FromArgb(236, 237, 239))) return AppTheme.Colors.Neutral.SurfaceSelected;
                if (Near(c, Color.FromArgb(228, 229, 231))) return AppTheme.Colors.Neutral.SurfacePressed;
                if (Near(c, Color.FromArgb(232, 232, 232))) return AppTheme.Colors.Neutral.SurfaceSecondary;
                if (Near(c, SystemColors.Control)) return AppTheme.Colors.Neutral.SurfaceSecondary;
                if (Near(c, SystemColors.Window)) return AppTheme.Colors.Neutral.Surface;
                if (IsNeutral(c) && Luma(c) >= 0.72f) return PickDarkSurface(c);
                return c;
            }

            if (Near(c, Color.FromArgb(15, 17, 23))) return Color.FromArgb(243, 244, 245);
            if (Near(c, Color.FromArgb(22, 26, 33))) return Color.White;
            if (Near(c, Color.FromArgb(28, 33, 42))) return Color.FromArgb(248, 248, 248);
            if (Near(c, Color.FromArgb(35, 42, 53))) return Color.FromArgb(242, 242, 242);
            if (Near(c, Color.FromArgb(42, 49, 61))) return Color.FromArgb(236, 237, 239);
            if (Near(c, Color.FromArgb(51, 59, 73))) return Color.FromArgb(228, 229, 231);
            if (IsNeutral(c) && Luma(c) <= 0.30f) return PickLightSurface(c);
            return c;
        }

        private static Color MapText(Color c, bool dark)
        {
            if (c == Color.Empty) return c;
            if (dark)
            {
                if (Near(c, Color.Black) || Near(c, Color.FromArgb(32, 33, 36))) return AppTheme.Colors.Neutral.TextPrimary;
                if (Near(c, Color.FromArgb(50, 50, 50))) return AppTheme.Colors.Neutral.TextPrimary;
                if (Near(c, Color.FromArgb(60, 60, 60))) return AppTheme.Colors.Neutral.TextSecondary;
                if (Near(c, Color.FromArgb(70, 70, 70))) return AppTheme.Colors.Neutral.TextSecondary;
                if (Near(c, Color.FromArgb(80, 84, 91))) return AppTheme.Colors.Neutral.TextSecondary;
                if (Near(c, Color.FromArgb(122, 128, 136))) return AppTheme.Colors.Neutral.TextMuted;
                if (Near(c, Color.FromArgb(136, 142, 150))) return AppTheme.Colors.Neutral.TextDisabled;
                if (Near(c, SystemColors.ControlText)) return AppTheme.Colors.Neutral.TextPrimary;
                if (IsNeutral(c) && Luma(c) <= 0.62f) return PickDarkText(c);
                return c;
            }

            if (Near(c, Color.FromArgb(242, 244, 247))) return Color.FromArgb(32, 33, 36);
            if (Near(c, Color.FromArgb(199, 205, 214))) return Color.FromArgb(80, 84, 91);
            if (Near(c, Color.FromArgb(146, 155, 168))) return Color.FromArgb(122, 128, 136);
            if (Near(c, Color.FromArgb(108, 118, 133))) return Color.FromArgb(136, 142, 150);
            return c;
        }

        private static Color MapBorder(Color c, bool dark)
        {
            if (c == Color.Empty) return c;
            if (dark)
            {
                if (Near(c, Color.FromArgb(214, 216, 220))) return AppTheme.Colors.Neutral.BorderDefault;
                if (Near(c, Color.FromArgb(229, 230, 232))) return AppTheme.Colors.Neutral.BorderSubtle;
                if (Near(c, Color.FromArgb(174, 180, 188))) return AppTheme.Colors.Neutral.BorderStrong;
                if (Near(c, Color.FromArgb(188, 193, 199))) return AppTheme.Colors.Neutral.BorderHover;
                if (Near(c, Color.FromArgb(139, 145, 153))) return AppTheme.Colors.Neutral.BorderFocus;
                if (Near(c, Color.FromArgb(215, 218, 222))) return AppTheme.Colors.Neutral.BorderDefault;
                if (Near(c, Color.FromArgb(220, 220, 220))) return AppTheme.Colors.Neutral.BorderDefault;
                if (Near(c, Color.FromArgb(210, 212, 216))) return AppTheme.Colors.Neutral.BorderDefault;
                if (Near(c, Color.FromArgb(225, 225, 225))) return AppTheme.Colors.Neutral.BorderSubtle;
                if (IsNeutral(c) && Luma(c) >= 0.58f) return AppTheme.Colors.Neutral.BorderDefault;
                return c;
            }

            if (Near(c, Color.FromArgb(52, 59, 71))) return Color.FromArgb(214, 216, 220);
            if (Near(c, Color.FromArgb(37, 43, 53))) return Color.FromArgb(229, 230, 232);
            if (Near(c, Color.FromArgb(74, 83, 98))) return Color.FromArgb(174, 180, 188);
            if (Near(c, Color.FromArgb(89, 99, 116))) return Color.FromArgb(188, 193, 199);
            if (Near(c, Color.FromArgb(138, 148, 163))) return Color.FromArgb(139, 145, 153);
            if (IsNeutral(c) && Luma(c) <= 0.42f) return Color.FromArgb(214, 216, 220);
            return c;
        }

        private static Color PickDarkSurface(Color c)
        {
            float l = Luma(c);
            if (l >= 0.96f) return AppTheme.Colors.Neutral.Surface;
            if (l >= 0.90f) return AppTheme.Colors.Neutral.SurfaceSecondary;
            if (l >= 0.84f) return AppTheme.Colors.Neutral.SurfaceHover;
            if (l >= 0.76f) return AppTheme.Colors.Neutral.SurfaceSelected;
            return AppTheme.Colors.Neutral.Canvas;
        }

        private static Color PickLightSurface(Color c)
        {
            float l = Luma(c);
            if (l <= 0.12f) return Color.FromArgb(243, 244, 245);
            if (l <= 0.18f) return Color.White;
            if (l <= 0.24f) return Color.FromArgb(248, 248, 248);
            if (l <= 0.30f) return Color.FromArgb(242, 242, 242);
            return Color.White;
        }

        private static Color PickDarkText(Color c)
        {
            float l = Luma(c);
            if (l <= 0.18f) return AppTheme.Colors.Neutral.TextPrimary;
            if (l <= 0.36f) return AppTheme.Colors.Neutral.TextSecondary;
            if (l <= 0.52f) return AppTheme.Colors.Neutral.TextMuted;
            return AppTheme.Colors.Neutral.TextDisabled;
        }

        private static Color PickLightText(Color c)
        {
            float l = Luma(c);
            if (l >= 0.90f) return Color.FromArgb(32, 33, 36);
            if (l >= 0.78f) return Color.FromArgb(80, 84, 91);
            if (l >= 0.66f) return Color.FromArgb(122, 128, 136);
            return Color.FromArgb(136, 142, 150);
        }

        private static bool IsNeutral(Color c)
        {
            int max = Math.Max(c.R, Math.Max(c.G, c.B));
            int min = Math.Min(c.R, Math.Min(c.G, c.B));
            return max - min <= 18;
        }

        private static bool Near(Color a, Color b)
        {
            return Math.Abs(a.R - b.R) <= 3 && Math.Abs(a.G - b.G) <= 3 && Math.Abs(a.B - b.B) <= 3;
        }

        private static float Luma(Color c)
        {
            return (0.2126f * c.R + 0.7152f * c.G + 0.0722f * c.B) / 255f;
        }

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        private const int LVM_SETCOLUMNWIDTH = 0x1000 + 30;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
