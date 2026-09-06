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

			// Sleek modern dark track
			using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(24, 28, 38)))
			{
				g.FillRectangle(trackBrush, rect);
			}
			using (Pen borderPen = new Pen(Color.FromArgb(45, 51, 69), 1f))
			{
				g.DrawRectangle(borderPen, rect.X, rect.Y, Math.Max(0, rect.Width - 1), Math.Max(0, rect.Height - 1));
			}

			rect.Inflate(-2, -2);
			if (Value != 0 && ValueMaximum > 0 && rect.Width > 0 && rect.Height > 0)
			{
				int x0 = rect.X;
				int fillWidth = (int)Math.Round((double)rect.Width * (ValuePercentage / 100.0));
				int xf = Math.Max(x0, Math.Min(rect.Right, x0 + fillWidth));
				if (xf > x0)
				{
					Rectangle fillRect = new Rectangle(x0, rect.Y, xf - x0, rect.Height);
					using (SolidBrush barBrush = new SolidBrush(BackColor))
					{
						g.FillRectangle(barBrush, fillRect);
					}
				}
			}
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
