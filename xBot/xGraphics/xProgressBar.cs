using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace xGraphics
{
	public enum xProgressBarDisplay : byte
	{
		Percentage,
		Values,
		Text
	}
	public class xProgressBar : Control
	{
		[Category("Appearance")]
		public xProgressBarDisplay Display { get; set; }
		[Category("Appearance")]
		public Color DisplayShadow { get; set; }
		[Category("Appearance")]
		public int BackColorDegradationLevel { get; set; }

		[Category("Behavior")]
		public ulong Value
		{
			get	{	return _Value; }
			set	{	_Value = value;	Invalidate();	}
		}
		private ulong _Value;
		[Category("Behavior")]
		public ulong ValueMaximum
		{
			get {	return _ValueMaximum; }
			set	{	_ValueMaximum = value; Invalidate(); }
		}
		private ulong _ValueMaximum;
		private SolidBrush TextColorBrush { get; }
		private Pen FillColor { get; }
		private SolidBrush TextShadowBrush { get; }
		public double ValuePercentage
		{
			get
			{
				if (ValueMaximum == 0)
					return 0.0;
				double pct = (double)Value * 100.0 / (double)ValueMaximum;
				if (double.IsNaN(pct) || double.IsInfinity(pct))
					return 0.0;
				return Math.Max(0.0, Math.Min(100.0, pct));
			}
		} 
		public xProgressBar() : base()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			TextColorBrush = new SolidBrush(Color.White);
			TextShadowBrush = new SolidBrush(Color.White);
			FillColor = new Pen(Color.MidnightBlue);
			DisplayShadow = Color.Black;
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (TextColorBrush.Color != ForeColor)
			{
				TextColorBrush.Color = ForeColor;
			}
			if (TextShadowBrush.Color != DisplayShadow)
			{
				TextShadowBrush.Color = DisplayShadow;
			}
			if (FillColor.Color != BackColor)
			{
				FillColor.Color = BackColor;
			}

			Rectangle rect = base.ClientRectangle;
			Graphics g = e.Graphics;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			int radius = Math.Min(4, Math.Min(rect.Width, rect.Height) / 2);
			Rectangle outerRect = new Rectangle(rect.X, rect.Y, Math.Max(0, rect.Width - 1), Math.Max(0, rect.Height - 1));

			// Rounded track background
			using (System.Drawing.Drawing2D.GraphicsPath trackPath = CreateRoundedRect(outerRect, radius))
			{
				using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(24, 28, 38)))
				{
					g.FillPath(trackBrush, trackPath);
				}
				using (Pen borderPen = new Pen(Color.FromArgb(45, 51, 69), 1f))
				{
					g.DrawPath(borderPen, trackPath);
				}
			}

			// Fill bar with rounded clip
			Rectangle innerRect = new Rectangle(rect.X + 2, rect.Y + 2, Math.Max(0, rect.Width - 4), Math.Max(0, rect.Height - 4));
			if (Value != 0 && ValueMaximum > 0 && innerRect.Width > 0 && innerRect.Height > 0)
			{
				int fillWidth = (int)Math.Round((double)innerRect.Width * (ValuePercentage / 100.0));
				fillWidth = Math.Max(0, Math.Min(innerRect.Width, fillWidth));
				if (fillWidth > 0)
				{
					int fillRadius = Math.Min(radius - 1, Math.Min(fillWidth, innerRect.Height) / 2);
					fillRadius = Math.Max(0, fillRadius);
					Rectangle fillRect = new Rectangle(innerRect.X, innerRect.Y, fillWidth, innerRect.Height);
					using (System.Drawing.Drawing2D.GraphicsPath fillPath = CreateRoundedRect(fillRect, fillRadius))
					{
						using (SolidBrush barBrush = new SolidBrush(BackColor))
						{
							g.FillPath(barBrush, fillPath);
						}
					}
				}
			}

			// Centered text with shadow
			string text = GetDisplayText();
			SizeF len = g.MeasureString(text, Font);
			int px = Convert.ToInt32((base.Width / 2) - len.Width / 2f);
			int py = Convert.ToInt32((base.Height / 2) - len.Height / 2f);
			using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
			{
				g.DrawString(text, Font, shadowBrush, px + 1, py + 1);
			}
			using (SolidBrush textBrush = new SolidBrush(Color.White))
			{
				g.DrawString(text, Font, textBrush, px, py);
			}
		}
		private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
		{
			System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
			if (radius <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
			{
				path.AddRectangle(bounds);
				return path;
			}
			int d = radius * 2;
			Size size = new Size(d, d);
			Rectangle arc = new Rectangle(bounds.Location, size);
			path.AddArc(arc, 180, 90);
			arc.X = bounds.Right - d;
			path.AddArc(arc, 270, 90);
			arc.Y = bounds.Bottom - d;
			path.AddArc(arc, 0, 90);
			arc.X = bounds.Left;
			path.AddArc(arc, 90, 90);
			path.CloseFigure();
			return path;
		}
		private string GetDisplayText()
		{
			switch (Display)
			{
				case xProgressBarDisplay.Percentage:
					{
						StringBuilder text = new StringBuilder();
						if (ValueMaximum == 0)
							text.Append("0");
						else
							text.Append(Math.Round(ValuePercentage, 2));
						text.Append("%");
						return text.ToString();
					}
				case xProgressBarDisplay.Values:
					{
						StringBuilder text = new StringBuilder();
						text.Append(Value);
						text.Append(" / ");
						text.Append(ValueMaximum);
						return text.ToString();
					}
				default:
					return Text;
			}
		}
	}

}
