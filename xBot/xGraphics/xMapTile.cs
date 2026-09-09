using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xGraphics
{
	public class xMapTile : PictureBox
	{
		// Bound simultaneous image decoding during zooming and rapid movement.
		private static readonly SemaphoreSlim LoadSlots = new SemaphoreSlim(2);
		private int loadVersion;
		public int SectorX { get; }
		public int SectorY { get; }
		public xMapTile(int SectorX, int SectorY)
		{
			this.SectorX = SectorX;
			this.SectorY = SectorY;
		}
		public async void LoadAsyncTile(string path,Size size)
		{
			int version = ++loadVersion;
			if (IsDisposed || size.Width <= 0 || size.Height <= 0 || !File.Exists(path))
				return;
			Bitmap bitmap = null;
			await LoadSlots.WaitAsync();
			try
			{
				if (IsDisposed || version != loadVersion)
					return;
				bitmap = await Task.Run(() => GetTile(path, size));
				if (IsDisposed || version != loadVersion)
					return;
				var old = Image;
				Image = bitmap;
				bitmap = null; // Ownership passes to the control.
				old?.Dispose();
			}
			catch (IOException) { }
			catch (ArgumentException) { } // Invalid image data.
			catch (OutOfMemoryException) { } // GDI+ also uses this for invalid images.
			finally
			{
				bitmap?.Dispose();
				LoadSlots.Release();
			}
		}
		private Bitmap GetTile(string path, Size size)
		{
			using (Image src = Image.FromFile(path))
			{
				return new Bitmap(src, size);
			}
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				++loadVersion;
				try { this.Image?.Dispose(); } catch { }
				this.Image = null;
			}
			base.Dispose(disposing);
		}
	}
}
