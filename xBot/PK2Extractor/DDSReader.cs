using System;
using System.Drawing;
using System.IO;

namespace xBot.PK2Extractor
{
	public static class DDSReader
	{
		public static Bitmap FromFile(string FileName)
		{
			return DevIL.DevIL.LoadBitmap(FileName);
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
			string tempFile = Path.GetTempFileName();
			try
			{
				byte[] DDSBuffer = ToDDS(DDJBuffer);
				File.WriteAllBytes(tempFile, DDSBuffer);
				return DevIL.DevIL.LoadBitmap(tempFile);
			}
			finally
			{
				try { File.Delete(tempFile); } catch { }
			}
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
