using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace xBot.Game.Navigation
{
	public static class NavDataReader
	{
		private const int RecordSize = 12;

		/// <summary>
		/// Decompress zlib stream using .NET built-in DeflateStream.
		/// Standard zlib format starts with 2-byte header (CMF/FLG) and ends with 4-byte Adler-32.
		/// </summary>
		public static byte[] DecompressZlib(byte[] input)
		{
			if (input == null || input.Length < 6)
				throw new InvalidDataException("Invalid zlib data length.");

			// Skip 2-byte zlib header (usually 0x78 0x9C, etc.)
			using (MemoryStream ms = new MemoryStream(input, 2, input.Length - 2))
			using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
			using (MemoryStream output = new MemoryStream())
			{
				byte[] buffer = new byte[8192];
				int read;
				while ((read = ds.Read(buffer, 0, buffer.Length)) > 0)
				{
					output.Write(buffer, 0, read);
				}
				return output.ToArray();
			}
		}

		public static NavRegion Read(string filePath, int regionId)
		{
			if (!File.Exists(filePath))
				return null;

			byte[] compressed = File.ReadAllBytes(filePath);
			byte[] decompressed = DecompressZlib(compressed);

			if (decompressed.Length < 4)
				return null;

			uint count = BitConverter.ToUInt32(decompressed, 0);
			if (count == 0 || count > 300000)
				return null;

			int pointCount = (int)count;
			long requiredBytes = 4 + (long)pointCount * RecordSize;
			if (requiredBytes > decompressed.Length)
				return null;

			NavPoint[] points = new NavPoint[pointCount];
			float minX = float.MaxValue;
			float maxX = float.MinValue;
			float minY = float.MaxValue;
			float maxY = float.MinValue;

			int offset = 4;
			for (int i = 0; i < pointCount; i++)
			{
				float x = BitConverter.ToSingle(decompressed, offset);
				float y = BitConverter.ToSingle(decompressed, offset + 4);
				ushort z = BitConverter.ToUInt16(decompressed, offset + 8);
				ushort flag = BitConverter.ToUInt16(decompressed, offset + 10);

				points[i] = new NavPoint(i, x, y, z, flag);

				if (x < minX) minX = x;
				if (x > maxX) maxX = x;
				if (y < minY) minY = y;
				if (y > maxY) maxY = y;

				offset += RecordSize;
			}

			// Count both directions first, then allocate exact-sized adjacency buffers.
			// This avoids a List and backing array for every point in a large region.
			int[] neighborCounts = new int[pointCount];
			int cursor = (int)requiredBytes;
			for (int i = 0; i < pointCount && cursor < decompressed.Length; i++)
			{
				byte neighborCount = decompressed[cursor++];
				for (int n = 0; n < neighborCount && cursor + 4 <= decompressed.Length; n++)
				{
					int target = BitConverter.ToInt32(decompressed, cursor);
					cursor += 4;
					if (target >= 0 && target < pointCount && target != i)
					{
						neighborCounts[i]++;
						neighborCounts[target]++;
					}
				}
			}

			int[][] neighbors = new int[pointCount][];
			for (int i = 0; i < pointCount; i++)
				neighbors[i] = neighborCounts[i] == 0 ? Array.Empty<int>() : new int[neighborCounts[i]];
			Array.Clear(neighborCounts, 0, neighborCounts.Length);

			cursor = (int)requiredBytes;
			for (int i = 0; i < pointCount && cursor < decompressed.Length; i++)
			{
				byte neighborCount = decompressed[cursor++];
				for (int n = 0; n < neighborCount && cursor + 4 <= decompressed.Length; n++)
				{
					int target = BitConverter.ToInt32(decompressed, cursor);
					cursor += 4;
					if (target >= 0 && target < pointCount && target != i)
					{
						neighbors[i][neighborCounts[i]++] = target;
						neighbors[target][neighborCounts[target]++] = i;
					}
				}
			}

			// Reuse one set instead of allocating a HashSet per point.
			HashSet<int> unique = new HashSet<int>();
			for (int i = 0; i < pointCount; i++)
			{
				unique.Clear();
				int[] adjacent = neighbors[i];
				int length = 0;
				for (int j = 0; j < adjacent.Length; j++)
					if (unique.Add(adjacent[j]))
						adjacent[length++] = adjacent[j];
				if (length != adjacent.Length)
					Array.Resize(ref neighbors[i], length);
			}

			return new NavRegion
			{
				RegionId = regionId,
				SourceFile = filePath,
				Points = points,
				Neighbors = neighbors,
				MinX = minX,
				MaxX = maxX,
				MinY = minY,
				MaxY = maxY
			};
		}

		/// <summary>
		/// Quick bounds extraction without building full topology in memory.
		/// </summary>
		public static bool TryReadBounds(string filePath, out float minX, out float maxX, out float minY, out float maxY)
		{
			minX = float.MaxValue;
			maxX = float.MinValue;
			minY = float.MaxValue;
			maxY = float.MinValue;

			try
			{
				if (!File.Exists(filePath))
					return false;

				// Bounds need only point records; do not inflate the entire topology.
				using (FileStream file = File.OpenRead(filePath))
				using (DeflateStream stream = OpenZlibPayload(file))
				using (BinaryReader reader = new BinaryReader(stream))
				{
					uint count = reader.ReadUInt32();
					if (count == 0 || count > 300000)
						return false;
					for (int i = 0; i < count; i++)
					{
						float x = reader.ReadSingle();
						float y = reader.ReadSingle();
						reader.ReadUInt32(); // Z and edge flag.
						if (x < minX) minX = x;
						if (x > maxX) maxX = x;
						if (y < minY) minY = y;
						if (y > maxY) maxY = y;
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static DeflateStream OpenZlibPayload(FileStream file)
		{
			if (file.Length < 6)
				throw new InvalidDataException("Invalid zlib data length.");
			file.Position = 2;
			return new DeflateStream(file, CompressionMode.Decompress);
		}
	}
}
