using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    public class ModernStatusStrip : UserControl
    {
        private string _botStatus = "Bekleniyor";
        private Color _botStatusColor = Color.Gray;
        private string _connectionInfo = "";
        private string _locationText = "";

        public string BotStatus
        {
            get => _botStatus;
            set { _botStatus = value; Invalidate(); }
        }

        public Color BotStatusColor
        {
            get => _botStatusColor;
            set { _botStatusColor = value; Invalidate(); }
        }

        public string ConnectionInfo
        {
            get => _connectionInfo;
            set { _connectionInfo = value; Invalidate(); }
        }

        public string LocationText
        {
            get => _locationText;
            set { _locationText = value; Invalidate(); }
        }

        public ModernStatusStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 28;
            Dock = DockStyle.Bottom;
            BackColor = DarkTheme.BgHeader;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush bgBrush = new SolidBrush(DarkTheme.BgHeader))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            using (Pen borderPen = new Pen(DarkTheme.BorderSubtle, 1f))
            {
                g.DrawLine(borderPen, 0, 0, Width, 0);
            }

            int padding = 12;
            int dotSize = 6;
            int dotY = (Height - dotSize) / 2;
            
            using (SolidBrush dotBrush = new SolidBrush(BotStatusColor))
            {
                g.FillEllipse(dotBrush, padding, dotY, dotSize, dotSize);
            }

            int textX = padding + dotSize + 6;
            if (!string.IsNullOrEmpty(BotStatus))
            {
                TextRenderer.DrawText(g, BotStatus, DarkTheme.FontBody, 
                    new Rectangle(textX, 0, Width / 3, Height), 
                    DarkTheme.TextSecondary, 
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            if (!string.IsNullOrEmpty(ConnectionInfo))
            {
                TextRenderer.DrawText(g, ConnectionInfo, DarkTheme.FontCaption, 
                    new Rectangle(0, 0, Width, Height), 
                    DarkTheme.TextMuted, 
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            if (!string.IsNullOrEmpty(LocationText))
            {
                TextRenderer.DrawText(g, LocationText, DarkTheme.FontCaption, 
                    new Rectangle(0, 0, Width - padding, Height), 
                    DarkTheme.TextMuted, 
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }
    }
}
