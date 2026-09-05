using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    public class NavItem
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string IconText { get; set; }
        public string Category { get; set; }
        public Control TargetPanel { get; set; }

        public NavItem(string key, string title, string iconText, string category, Control targetPanel = null)
        {
            Key = key;
            Title = title;
            IconText = iconText;
            Category = category;
            TargetPanel = targetPanel;
        }
    }

    /// <summary>
    /// Modern categorized vertical sidebar navigation with active accent indicators,
    /// smooth hover states, and automated panel switching.
    /// </summary>
    public class ModernSidebar : UserControl
    {
        private readonly List<NavItem> _items = new List<NavItem>();
        private string _selectedKey = string.Empty;
        private string _hoveredKey = string.Empty;

        public event Action<string, Control> TabSelected;

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

        public ModernSidebar()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = DarkTheme.BgSidebar;
            AutoScroll = false;
            Width = DarkTheme.SidebarWidth;
        }

        public void AddItem(string key, string title, string iconText, string category, Control targetPanel = null)
        {
            _items.Add(new NavItem(key, title, iconText, category, targetPanel));
            Invalidate();
        }

        public void ClearItems()
        {
            _items.Clear();
            _selectedKey = string.Empty;
            _hoveredKey = string.Empty;
            Invalidate();
        }

        public void UpdateItemText(string key, string newTitle, string newCategory = null)
        {
            NavItem found = _items.Find(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                if (!string.IsNullOrEmpty(newTitle))
                    found.Title = newTitle;
                if (!string.IsNullOrEmpty(newCategory))
                    found.Category = newCategory;
                Invalidate();
            }
        }

        public void SelectTab(string key, bool triggerEvent = true)
        {
            NavItem found = _items.Find(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (found == null) return;

            _selectedKey = found.Key;

            // Manage panel visibility automatically if TargetPanel is bound
            foreach (var item in _items)
            {
                if (item.TargetPanel != null)
                {
                    item.TargetPanel.Visible = (item == found);
                }
            }

            Invalidate();

            if (triggerEvent)
            {
                TabSelected?.Invoke(found.Key, found.TargetPanel);
            }
        }

        private struct LayoutRow
        {
            public bool IsHeader;
            public string Text;
            public NavItem Item;
            public Rectangle Bounds;
        }

        private List<LayoutRow> ComputeLayout()
        {
            List<LayoutRow> rows = new List<LayoutRow>();
            int currentY = 6;
            int itemW = Width;
            string lastCategory = null;

            foreach (var item in _items)
            {
                if (item.Category != lastCategory)
                {
                    lastCategory = item.Category;
                    // Category Header Row
                    Rectangle headerRect = new Rectangle(0, currentY, itemW, 20);
                    rows.Add(new LayoutRow { IsHeader = true, Text = lastCategory, Bounds = headerRect });
                    currentY += 21;
                }

                // Nav Item Row (compact 28px)
                Rectangle itemRect = new Rectangle(6, currentY, itemW - 12, 28);
                rows.Add(new LayoutRow { IsHeader = false, Item = item, Bounds = itemRect });
                currentY += 30;
            }

            return rows;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var rows = ComputeLayout();
            string newHover = string.Empty;

            foreach (var row in rows)
            {
                if (!row.IsHeader && row.Bounds.Contains(e.Location))
                {
                    newHover = row.Item.Key;
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

            var rows = ComputeLayout();
            foreach (var row in rows)
            {
                if (!row.IsHeader && row.Bounds.Contains(e.Location))
                {
                    SelectTab(row.Item.Key);
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

            // Draw sidebar right border line
            using (Pen borderPen = new Pen(DarkTheme.BorderSubtle, 1f))
            {
                g.DrawLine(borderPen, Width - 1, 0, Width - 1, Height);
            }

            var rows = ComputeLayout();

            foreach (var row in rows)
            {
                if (row.IsHeader)
                {
                    // Render Category Header (Uppercase Faint text)
                    Rectangle textRect = new Rectangle(14, row.Bounds.Y, row.Bounds.Width - 28, row.Bounds.Height);
                    using (SolidBrush headerBrush = new SolidBrush(DarkTheme.TextFaint))
                    {
                        StringFormat sf = new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(row.Text.ToUpperInvariant(), DarkTheme.FontCategory, headerBrush, textRect, sf);
                    }
                }
                else
                {
                    NavItem item = row.Item;
                    bool isSelected = string.Equals(_selectedKey, item.Key, StringComparison.OrdinalIgnoreCase);
                    bool isHovered = string.Equals(_hoveredKey, item.Key, StringComparison.OrdinalIgnoreCase);

                    Rectangle itemBounds = row.Bounds;

                    // 1. Background fill
                    if (isSelected)
                    {
                        using (GraphicsPath path = DarkTheme.CreateRoundedRectangle(itemBounds, 6))
                        using (SolidBrush selBrush = new SolidBrush(DarkTheme.AccentSubtle))
                        {
                            g.FillPath(selBrush, path);
                        }

                        // Left vertical accent bar
                        Rectangle barRect = new Rectangle(itemBounds.X, itemBounds.Y + 4, 3, itemBounds.Height - 8);
                        using (GraphicsPath barPath = DarkTheme.CreateRoundedRectangle(barRect, 1))
                        using (SolidBrush barBrush = new SolidBrush(DarkTheme.Accent))
                        {
                            g.FillPath(barBrush, barPath);
                        }
                    }
                    else if (isHovered)
                    {
                        using (GraphicsPath path = DarkTheme.CreateRoundedRectangle(itemBounds, 6))
                        using (SolidBrush hovBrush = new SolidBrush(DarkTheme.BgCard))
                        {
                            g.FillPath(hovBrush, path);
                        }
                    }

                    // 2. Icon glyph
                    Color iconColor = isSelected ? DarkTheme.AccentHover : (isHovered ? DarkTheme.TextPrimary : DarkTheme.TextMuted);
                    Rectangle iconRect = new Rectangle(itemBounds.X + 8, itemBounds.Y, 20, itemBounds.Height);
                    using (Font iconFont = new Font("Segoe UI Symbol", 9.5f, FontStyle.Regular))
                    using (SolidBrush iconBrush = new SolidBrush(iconColor))
                    {
                        StringFormat sfIcon = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(item.IconText, iconFont, iconBrush, iconRect, sfIcon);
                    }

                    // 3. Title Text
                    Color textColor = isSelected ? DarkTheme.TextPrimary : (isHovered ? DarkTheme.TextPrimary : DarkTheme.TextMuted);
                    Font textFont = isSelected ? DarkTheme.FontBodyBold : DarkTheme.FontBody;
                    Rectangle titleRect = new Rectangle(itemBounds.X + 32, itemBounds.Y, itemBounds.Width - 36, itemBounds.Height);

                    using (SolidBrush textBrush = new SolidBrush(textColor))
                    {
                        StringFormat sfText = new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                            Trimming = StringTrimming.EllipsisCharacter
                        };
                        g.DrawString(item.Title, textFont, textBrush, titleRect, sfText);
                    }
                }
            }
        }
    }
}
