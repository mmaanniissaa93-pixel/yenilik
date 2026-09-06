using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xGraphics
{
	public class xMapTile : PictureBox
	{
		public int SectorX { get; }
		public int SectorY { get; }
		public xMapTile(int SectorX, int SectorY)
		{
			this.SectorX = SectorX;
			this.SectorY = SectorY;
		}
		public async void LoadAsyncTile(string path,Size size)
		{
			if (File.Exists(path))
			{
				Bitmap bmp = await Task.Run(() => GetTile(path, size));
				var old = this.Image;
				this.Image = bmp;
				try { old?.Dispose(); } catch { }
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
				try { this.Image?.Dispose(); } catch { }
				this.Image = null;
			}
			base.Dispose(disposing);
		}
	}
}