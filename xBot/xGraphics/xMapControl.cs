using System;
using System.Windows.Forms;
using System.Drawing;

namespace xGraphics
{
	public class xMapControl : Control
	{
		private Image _image;
		private Image ownedImage;
		public Image Image
		{
			get { return _image; }
			set {
				if (ReferenceEquals(_image, value)) return;
				Image previousOwned = ownedImage;
				ownedImage = null;
				_image = value;
				previousOwned?.Dispose();
				try
				{
					RecreateHandle();
				}
				catch { }
			}
		}
		// Resource icons are shared; only dynamically generated images are owned.
		public void SetOwnedImage(Image image)
		{
			Image = image;
			ownedImage = image;
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ownedImage?.Dispose();
				ownedImage = null;
				_image = null;
				Tag = null;
			}
			base.Dispose(disposing);
		}
		public xMapControl()
		{
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = Color.Transparent;
			this.SetStyle(ControlStyles.ResizeRedraw, true);
		}
		protected override CreateParams CreateParams
		{
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x20;
				return cp;
			}
		}
		protected override void OnMove(EventArgs e)
		{
			try
			{
				RecreateHandle();
			}
			catch { }
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			if (_image != null) e.Graphics.DrawImage(_image,0,0, _image.Size.Width, _image.Size.Height);
		}
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// Do not paint background
		}
		public void RePaint()
		{
			try
			{
				RecreateHandle();
			}
			catch { }
		}
	}
}
