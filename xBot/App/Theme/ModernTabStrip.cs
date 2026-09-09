using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    public class TabItem
    {
        public string Key { get; set; }
        public string Title { get; set; }

        public TabItem(string key, string title)
        {
            Key = key;
            Title = title;
        }
    }

    /// <summary>
    /// Modern horizontal tab strip with even layout, active accent indicator, and smooth hover states.
    /// </summary>
    public class ModernTabStrip : UserControl
    {
        private readonly List<TabItem> _tabs = new List<TabItem>();
        private string _selectedKey = string.Empty;
        private string _hoveredKey = string.Empty;

        public event Action<string> TabSelected;

        public string SelectedKey
        {
            get => _selectedKey;
            set
            {
                if (_selectedKey != value)
                {
                    _selectedKey = value;
                    Invalidate();
                }
            }
        }

        public ModernTabStrip()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = DarkTheme.BgSidebar;
            Height = 32;
        }

        public void AddTab(string key, string title)
        {
            _tabs.Add(new TabItem(key, title));
            Invalidate();
        }

        public void ClearTabs()
        {
            _tabs.Clear();
            _selectedKey = string.Empty;
            _hoveredKey = string.Empty;
            Invalidate();
        }

        public void SelectTab(string key)
        {
            TabItem found = _tabs.Find(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (found == null) return;

            _selectedKey = found.Key;
            Invalidate();
            TabSelected?.Invoke(found.Key);
        }

        private struct TabLayout
        {
            public TabItem Tab;
            public Rectangle Bounds;
        }

        private List<TabLayout> ComputeLayout()
        {
            List<TabLayout> layouts = new List<TabLayout>();

            List<TabItem> visibleTabs = new List<TabItem>();
            foreach (var tab in _tabs)
            {
                if (tab.Key.StartsWith("...") || tab.Key.StartsWith(". . ."))
                    continue;
                visibleTabs.Add(tab);
            }

            if (visibleTabs.Count == 0) return layouts;

            int tabWidth = Width / visibleTabs.Count;
            int currentX = 0;

            for (int i = 0; i < visibleTabs.Count; i++)
            {
                var tab = visibleTabs[i];
                int w = (i == visibleTabs.Count - 1) ? (Width - currentX) : tabWidth;
                layouts.Add(new TabLayout
                {
                    Tab = tab,
                    Bounds = new Rectangle(currentX, 0, w, Height)
                });
                currentX += w;
            }

            return layouts;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var layouts = ComputeLayout();
            string newHover = string.Empty;

            foreach (var layout in layouts)
            {
                if (layout.Bounds.Contains(e.Location))
                {
                    newHover = layout.Tab.Key;
                    Cursor = Cursors.Hand;
                    break;
                }
            }

            if (string.IsNullOrEmpty(newHover))
                Cursor = Cursors.Default;

            if (_hoveredKey != newHover)
            {
                _hoveredKey = newHover;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!string.IsNullOrEmpty(_hoveredKey))
            {
                _hoveredKey = string.Empty;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;

            var layouts = ComputeLayout();
            foreach (var layout in layouts)
            {
                if (layout.Bounds.Contains(e.Location))
                {
                    SelectTab(layout.Tab.Key);
                    break;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Bottom 1px border line
            using (Pen borderPen = new Pen(DarkTheme.BorderSubtle, 1f))
            {
                g.DrawLine(borderPen, 0, Height - 1, Width, Height - 1);
            }

            var layouts = ComputeLayout();

            foreach (var layout in layouts)
            {
                var tab = layout.Tab;
                bool isSelected = string.Equals(_selectedKey, tab.Key, StringComparison.OrdinalIgnoreCase);
                bool isHovered = string.Equals(_hoveredKey, tab.Key, StringComparison.OrdinalIgnoreCase);

                Rectangle bounds = layout.Bounds;

                // 1. Background fill for hovered (inactive) state
                if (isHovered && !isSelected)
                {
                    // Subtle background
                    using (SolidBrush hovBrush = new SolidBrush(DarkTheme.BgInput))
                    {
                        g.FillRectangle(hovBrush, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - 1));
                    }
                }

                // 2. Title Text
                Color textColor = isSelected ? DarkTheme.TextPrimary : (isHovered ? DarkTheme.TextSecondary : DarkTheme.TextMuted);
                Font textFont = isSelected ? DarkTheme.FontBodyBold : DarkTheme.FontBody;

                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(tab.Title, textFont, textBrush, bounds, sf);
                }

                // 3. Bottom Accent Line
                if (isSelected)
                {
                    using (SolidBrush accentBrush = new SolidBrush(DarkTheme.Accent))
                    {
                        g.FillRectangle(accentBrush, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2));
                    }
                }
            }
        }
    }
}
