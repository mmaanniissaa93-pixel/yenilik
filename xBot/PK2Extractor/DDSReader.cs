using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace xBot.PK2Extractor
{
	public static class DDSReader
	{
		public static Bitmap FromFile(string FileName)
		{
			return FromDdsBuffer(File.ReadAllBytes(FileName));
		}
		public static Bitmap FromDDJ(string FileName)
		{
			byte[] DDJStream = File.ReadAllBytes(FileName);
			return LoadDdjBuffer(DDJStream);
		}
		public static Bitmap FromDDJ(byte[] DDJBuffer)
		{
			return LoadDdjBuffer(DDJBuffer);
		}
		private static Bitmap LoadDdjBuffer(byte[] DDJBuffer)
		{
			return FromDdsBuffer(ToDDS(DDJBuffer));
		}
		/// <summary>
		/// Yönetilen DDS çözücü. Eskiden DevIL.NET2 native sarmalayıcısı kullanılıyordu,
		/// ancak DevIL DLL'leri repoda yok ve NuGet'te birebir paket bulunmuyor.
		/// Silkroad DDJ/DDS içerikleri (DXT1/2/3/4/5 + sıkıştırmasız RGB/Luminance) burada
		/// çözülür. Desteklenmeyen biçimler InvalidDataException atar; çağrıcılar
		/// zaten try/catch ile bozuk girdiyi atlar. Çıktı 32bpp ARGB'dir.
		/// </summary>
		private static Bitmap FromDdsBuffer(byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (buffer.Length < 128)
				throw new InvalidDataException("DDS data is shorter than its 128-byte header.");
			if (buffer[0] != (byte)'D' || buffer[1] != (byte)'D' || buffer[2] != (byte)'S' || buffer[3] != (byte)' ')
				throw new InvalidDataException("Missing DDS magic.");
			if (ReadI32(buffer, 4) != 124)
				throw new InvalidDataException("Unsupported DDS header size.");
			int height = ReadI32(buffer, 12);
			int width = ReadI32(buffer, 16);
			if (width <= 0 || height <= 0 || width > 4096 || height > 4096)
				throw new InvalidDataException("Invalid DDS dimensions: " + width + "x" + height);
			if (ReadI32(buffer, 76) != 32)
				throw new InvalidDataException("Unsupported DDS pixel-format size.");
			int pfFlags = ReadI32(buffer, 80);
			int fourCC = ReadI32(buffer, 84);
			int rgbBitCount = ReadI32(buffer, 88);
			uint rMask = (uint)ReadI32(buffer, 92);
			uint gMask = (uint)ReadI32(buffer, 96);
			uint bMask = (uint)ReadI32(buffer, 100);
			uint aMask = (uint)ReadI32(buffer, 104);

			const int DDPF_FOURCC = 0x4;
			const int DDPF_RGB = 0x40;
			const int DDPF_LUMINANCE = 0x20000;
			const int DDPF_ALPHAPIXELS = 0x1;

			byte[] bgra = new byte[width * height * 4];
			const int dataOffset = 128;
			if ((pfFlags & DDPF_FOURCC) != 0)
			{
				if (fourCC == FourCC('D', 'X', 'T', '1'))
					DecodeDxt(buffer, dataOffset, width, height, bgra, false, false);
				else if (fourCC == FourCC('D', 'X', 'T', '2') || fourCC == FourCC('D', 'X', 'T', '3'))
					DecodeDxt(buffer, dataOffset, width, height, bgra, true, false);
				else if (fourCC == FourCC('D', 'X', 'T', '4') || fourCC == FourCC('D', 'X', 'T', '5'))
					DecodeDxt(buffer, dataOffset, width, height, bgra, true, true);
				else if (fourCC == FourCC('D', 'X', '1', '0'))
					throw new InvalidDataException("DX10-extended DDS is not supported.");
				else
					throw new InvalidDataException("Unsupported DDS FourCC: " + FourCCName(fourCC));
			}
			else if ((pfFlags & DDPF_RGB) != 0)
			{
				DecodeRgb(buffer, dataOffset, width, height, bgra, rgbBitCount, rMask, gMask, bMask, aMask, (pfFlags & DDPF_ALPHAPIXELS) != 0);
			}
			else if ((pfFlags & DDPF_LUMINANCE) != 0)
			{
				DecodeLuminance(buffer, dataOffset, width, height, bgra, rgbBitCount);
			}
			else
			{
				throw new InvalidDataException("Unsupported DDS pixel format flags: 0x" + pfFlags.ToString("X8"));
			}

			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			try
			{
				Marshal.Copy(bgra, 0, data.Scan0, bgra.Length);
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}
			finally
			{
				bitmap.UnlockBits(data);
			}
			return bitmap;
		}
		private static void DecodeDxt(byte[] src, int offset, int width, int height, byte[] bgra, bool hasAlphaBlock, bool interpolatedAlpha)
		{
			int blockBytes = hasAlphaBlock ? 16 : 8;
			int blocksX = (width + 3) / 4;
			int blocksY = (height + 3) / 4;
			if ((long)src.Length - offset < (long)blocksX * blocksY * blockBytes)
				throw new InvalidDataException("Truncated DDS block data.");
			byte[] blockAlpha = new byte[16];
			int p = offset;
			for (int by = 0; by < blocksY; by++)
			{
				for (int bx = 0; bx < blocksX; bx++)
				{
					if (hasAlphaBlock)
					{
						if (interpolatedAlpha)
							DecodeDxt5Alpha(src, p, blockAlpha);
						else
							DecodeDxt3Alpha(src, p, blockAlpha);
						p += 8;
					}
					else
					{
						for (int i = 0; i < 16; i++)
							blockAlpha[i] = 255;
					}
					DecodeDxtColors(src, p, blockAlpha, !hasAlphaBlock, bgra, width, height, bx * 4, by * 4);
					p += 8;
				}
			}
		}
		private static void DecodeDxtColors(byte[] src, int p, byte[] blockAlpha, bool isDxt1, byte[] bgra, int width, int height, int x0, int y0)
		{
			ushort c0 = ReadU16(src, p);
			ushort c1 = ReadU16(src, p + 2);
			uint code = (uint)ReadI32(src, p + 4);
			byte r0, g0, b0, r1, g1, b1;
			Expand565(c0, out r0, out g0, out b0);
			Expand565(c1, out r1, out g1, out b1);
			byte r2 = (byte)((2 * r0 + r1) / 3);
			byte g2 = (byte)((2 * g0 + g1) / 3);
			byte b2 = (byte)((2 * b0 + b1) / 3);
			byte r3 = (byte)((r0 + 2 * r1) / 3);
			byte g3 = (byte)((g0 + 2 * g1) / 3);
			byte b3 = (byte)((b0 + 2 * b1) / 3);
			bool fourColor = !isDxt1 || c0 > c1;
			for (int ty = 0; ty < 4; ty++)
			{
				for (int tx = 0; tx < 4; tx++)
				{
					int x = x0 + tx;
					int y = y0 + ty;
					if (x >= width || y >= height)
						continue;
					int sel = (int)((code >> (2 * (4 * ty + tx))) & 3);
					byte r, g, b, a;
					if (sel == 0)
					{
						r = r0; g = g0; b = b0; a = blockAlpha[4 * ty + tx];
					}
					else if (sel == 1)
					{
						r = r1; g = g1; b = b1; a = blockAlpha[4 * ty + tx];
					}
					else if (sel == 2)
					{
						r = r2; g = g2; b = b2; a = blockAlpha[4 * ty + tx];
					}
					else if (fourColor)
					{
						r = r3; g = g3; b = b3; a = blockAlpha[4 * ty + tx];
					}
					else
					{
						r = 0; g = 0; b = 0; a = 0;
					}
					int dst = (y * width + x) * 4;
					bgra[dst] = b;
					bgra[dst + 1] = g;
					bgra[dst + 2] = r;
					bgra[dst + 3] = a;
				}
			}
		}
		private static void DecodeDxt3Alpha(byte[] src, int p, byte[] blockAlpha)
		{
			for (int i = 0; i < 16; i += 2)
			{
				byte v = src[p + i / 2];
				byte lo = (byte)(v & 0x0F);
				byte hi = (byte)((v >> 4) & 0x0F);
				blockAlpha[i] = (byte)((lo << 4) | lo);
				blockAlpha[i + 1] = (byte)((hi << 4) | hi);
			}
		}
		private static void DecodeDxt5Alpha(byte[] src, int p, byte[] blockAlpha)
		{
			byte a0 = src[p];
			byte a1 = src[p + 1];
			ulong bits = 0;
			for (int i = 0; i < 6; i++)
				bits |= ((ulong)src[p + 2 + i]) << (8 * i);
			for (int i = 0; i < 16; i++)
			{
				int idx = (int)((bits >> (3 * i)) & 7);
				byte a;
				if (idx == 0)
					a = a0;
				else if (idx == 1)
					a = a1;
				else if (a0 > a1)
					a = (byte)(((8 - idx) * a0 + (idx - 1) * a1) / 7);
				else if (idx == 6)
					a = 0;
				else if (idx == 7)
					a = 255;
				else
					a = (byte)(((6 - idx) * a0 + (idx - 1) * a1) / 5);
				blockAlpha[i] = a;
			}
		}
		private static void DecodeRgb(byte[] src, int offset, int width, int height, byte[] bgra, int bitCount, uint rMask, uint gMask, uint bMask, uint aMask, bool hasAlpha)
		{
			int bpp;
			if (bitCount == 32)
				bpp = 4;
			else if (bitCount == 24)
				bpp = 3;
			else if (bitCount == 16)
				bpp = 2;
			else
				throw new InvalidDataException("Unsupported DDS RGB bit count: " + bitCount);
			if ((long)src.Length - offset < (long)width * height * bpp)
				throw new InvalidDataException("Truncated DDS pixel data.");
			int rShift, rBits, gShift, gBits, bShift, bBits, aShift, aBits;
			MaskShiftBits(rMask, out rShift, out rBits);
			MaskShiftBits(gMask, out gShift, out gBits);
			MaskShiftBits(bMask, out bShift, out bBits);
			MaskShiftBits(aMask, out aShift, out aBits);
			int p = offset;
			for (int i = 0; i < width * height; i++)
			{
				uint v = 0;
				for (int k = 0; k < bpp; k++)
					v |= ((uint)src[p++]) << (8 * k);
				bgra[4 * i] = ScaleTo8((v & bMask) >> bShift, bBits);
				bgra[4 * i + 1] = ScaleTo8((v & gMask) >> gShift, gBits);
				bgra[4 * i + 2] = ScaleTo8((v & rMask) >> rShift, rBits);
				bgra[4 * i + 3] = (hasAlpha && aBits > 0) ? ScaleTo8((v & aMask) >> aShift, aBits) : (byte)255;
			}
		}
		private static void DecodeLuminance(byte[] src, int offset, int width, int height, byte[] bgra, int bitCount)
		{
			if (bitCount != 8 && bitCount != 16)
				throw new InvalidDataException("Unsupported DDS luminance bit count: " + bitCount);
			int bpp = bitCount / 8;
			if ((long)src.Length - offset < (long)width * height * bpp)
				throw new InvalidDataException("Truncated DDS pixel data.");
			int p = offset;
			for (int i = 0; i < width * height; i++)
			{
				byte l = src[p++];
				byte a = 255;
				if (bpp == 2)
					a = src[p++];
				bgra[4 * i] = l;
				bgra[4 * i + 1] = l;
				bgra[4 * i + 2] = l;
				bgra[4 * i + 3] = a;
			}
		}
		private static void Expand565(ushort c, out byte r, out byte g, out byte b)
		{
			byte r5 = (byte)((c >> 11) & 0x1F);
			byte g6 = (byte)((c >> 5) & 0x3F);
			byte b5 = (byte)(c & 0x1F);
			r = (byte)((r5 << 3) | (r5 >> 2));
			g = (byte)((g6 << 2) | (g6 >> 4));
			b = (byte)((b5 << 3) | (b5 >> 2));
		}
		private static void MaskShiftBits(uint mask, out int shift, out int bits)
		{
			shift = 0;
			bits = 0;
			if (mask == 0)
				return;
			while (((mask >> shift) & 1) == 0)
				shift++;
			uint m = mask >> shift;
			while ((m & 1) == 1)
			{
				bits++;
				m >>= 1;
			}
		}
		private static byte ScaleTo8(uint v, int bits)
		{
			if (bits <= 0)
				return 0;
			if (bits >= 8)
				return (byte)(v >> (bits - 8));
			return (byte)((v * 255 + (((1 << bits) - 1) / 2)) / ((1 << bits) - 1));
		}
		private static int ReadI32(byte[] b, int o)
		{
			return b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24);
		}
		private static ushort ReadU16(byte[] b, int o)
		{
			return (ushort)(b[o] | (b[o + 1] << 8));
		}
		private static int FourCC(char a, char b, char c, char d)
		{
			return a | (b << 8) | (c << 16) | (d << 24);
		}
		private static string FourCCName(int f)
		{
			return new string(new char[] { (char)(f & 0xFF), (char)((f >> 8) & 0xFF), (char)((f >> 16) & 0xFF), (char)((f >> 24) & 0xFF) });
		}
		private static byte[] ToDDS(byte[] DDJBuffer)
		{
			if (DDJBuffer == null)
				throw new ArgumentNullException(nameof(DDJBuffer));
			if (DDJBuffer.Length < 20)
				throw new InvalidDataException("DDJ data is shorter than its 20-byte header.");
			byte[] DDSBuffer = new byte[DDJBuffer.Length - 20];
			Array.Copy(DDJBuffer, 20, DDSBuffer, 0, DDSBuffer.Length);
			return DDSBuffer;
		}
	}
}
