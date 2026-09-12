using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace xBot.App.Splash
{
	/// <summary>
	/// Win32 layered window helper providing per-pixel alpha rendering (WS_EX_LAYERED + UpdateLayeredWindow).
	/// Preserves full PNG alpha transparency, anti-aliasing, and subtle glow effects without WinForms color-key masking.
	/// </summary>
	internal static class LayeredWindowRenderer
	{
		public const int WS_EX_LAYERED = 0x00080000;
		public const int WS_EX_NOACTIVATE = 0x08000000;
		public const int WS_EX_TOOLWINDOW = 0x00000080;

		private const byte AC_SRC_OVER = 0x00;
		private const byte AC_SRC_ALPHA = 0x01;
		private const uint ULW_ALPHA = 0x00000002;

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				X = x;
				Y = y;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE
		{
			public int CX;
			public int CY;

			public SIZE(int cx, int cy)
			{
				CX = cx;
				CY = cy;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BLENDFUNCTION
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern bool UpdateLayeredWindow(
			IntPtr hwnd,
			IntPtr hdcDst,
			ref POINT pptDst,
			ref SIZE psize,
			IntPtr hdcSrc,
			ref POINT pptSrc,
			uint crKey,
			[In] ref BLENDFUNCTION pblend,
			uint dwFlags);

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll", ExactSpelling = true)]
		private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll", ExactSpelling = true)]
		private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern bool DeleteObject(IntPtr hObject);

		/// <summary>
		/// Loads a PNG image into a 32bpp Premultiplied ARGB bitmap without holding any file lock.
		/// Windows UpdateLayeredWindow expects AC_SRC_ALPHA with premultiplied alpha channels.
		/// </summary>
		public static Bitmap LoadPremultipliedFrame(string filePath)
		{
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
				return null;

			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (Bitmap source = (Bitmap)Image.FromStream(fs, false, false))
			{
				Bitmap pArgbBitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);
				using (Graphics g = Graphics.FromImage(pArgbBitmap))
				{
					g.CompositingMode = CompositingMode.SourceCopy;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.PixelOffsetMode = PixelOffsetMode.HighQuality;
					g.DrawImage(source, 0, 0, source.Width, source.Height);
				}
				return pArgbBitmap;
			}
		}

		/// <summary>
		/// Updates the layered window frame using per-pixel alpha blending.
		/// GDI handles are released immediately in a finally block to eliminate leaks.
		/// </summary>
		public static bool RenderFrame(Form form, Bitmap bitmap, byte opacity = 255)
		{
			if (form == null || form.IsDisposed || !form.IsHandleCreated || bitmap == null)
				return false;

			IntPtr screenDc = IntPtr.Zero;
			IntPtr memDc = IntPtr.Zero;
			IntPtr hBitmap = IntPtr.Zero;
			IntPtr oldBitmap = IntPtr.Zero;

			try
			{
				screenDc = GetDC(IntPtr.Zero);
				if (screenDc == IntPtr.Zero)
					return false;

				memDc = CreateCompatibleDC(screenDc);
				if (memDc == IntPtr.Zero)
					return false;

				// Color.FromArgb(0) ensures alpha channel is respected during HBitmap generation
				hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
				if (hBitmap == IntPtr.Zero)
					return false;

				oldBitmap = SelectObject(memDc, hBitmap);

				POINT newLocation = new POINT(form.Left, form.Top);
				SIZE newSize = new SIZE(bitmap.Width, bitmap.Height);
				POINT sourceLocation = new POINT(0, 0);

				BLENDFUNCTION blend = new BLENDFUNCTION
				{
					BlendOp = AC_SRC_OVER,
					BlendFlags = 0,
					SourceConstantAlpha = opacity,
					AlphaFormat = AC_SRC_ALPHA
				};

				return UpdateLayeredWindow(
					form.Handle,
					screenDc,
					ref newLocation,
					ref newSize,
					memDc,
					ref sourceLocation,
					0,
					ref blend,
					ULW_ALPHA);
			}
			catch
			{
				return false;
			}
			finally
			{
				if (memDc != IntPtr.Zero)
				{
					if (oldBitmap != IntPtr.Zero)
						SelectObject(memDc, oldBitmap);
					DeleteDC(memDc);
				}
				if (hBitmap != IntPtr.Zero)
				{
					DeleteObject(hBitmap);
				}
				if (screenDc != IntPtr.Zero)
				{
					ReleaseDC(IntPtr.Zero, screenDc);
				}
			}
		}
	}
}
