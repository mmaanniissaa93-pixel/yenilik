using System;
using System.Collections.Generic;
using System.IO;
using xBot.App;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
	public class NavigationManager
	{
		private static NavigationManager m_instance;
		public static NavigationManager Get => m_instance ?? (m_instance = new NavigationManager());

		public class RegionBoundsEntry
		{
			public string FilePath { get; set; }
			public int RegionId { get; set; }
			public float MinX { get; set; }
			public float MaxX { get; set; }
			public float MinY { get; set; }
			public float MaxY { get; set; }

			public bool Contains(float x, float y, float padding = 15f)
			{
				return x >= (MinX - padding) && x <= (MaxX + padding) &&
				       y >= (MinY - padding) && y <= (MaxY + padding);
			}
		}

		private string m_dataDirectory;
		private List<RegionBoundsEntry> m_boundsIndex;
		private Dictionary<int, NavRegion> m_cache = new Dictionary<int, NavRegion>();
		private AStarPathfinder m_pathfinder = new AStarPathfinder();
		private bool m_initialized = false;

		public bool IsAvailable => m_initialized && m_boundsIndex != null && m_boundsIndex.Count > 0;

		public NavigationManager()
		{
			Initialize();
		}

		public void Initialize()
		{
			if (m_initialized)
				return;

			// Check potential paths for navdata
			string[] candidateDirs = new string[]
			{
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "navdata"),
				Path.Combine(Environment.CurrentDirectory, "navdata"),
				@"C:\Users\auguu\Desktop\xBot-WinForms\navdata",
				@"C:\Users\auguu\Desktop\navdata"
			};

			foreach (string dir in candidateDirs)
			{
				if (Directory.Exists(dir) && Directory.GetFiles(dir, "*.dat").Length > 0)
				{
					m_dataDirectory = dir;
					break;
				}
			}

			if (string.IsNullOrEmpty(m_dataDirectory))
			{
				Window.Get?.LogProcess("NavMesh: navdata folder not found.", Window.ProcessState.Warning);
				return;
			}

			Window.Get?.LogProcess($"NavMesh: Loading navdata index from [{m_dataDirectory}]...");
			m_boundsIndex = new List<RegionBoundsEntry>();

			string[] files = Directory.GetFiles(m_dataDirectory, "*.dat");
			foreach (string file in files)
			{
				string fileName = Path.GetFileName(file);
				int regionId = ParseRegionId(fileName);

				if (NavDataReader.TryReadBounds(file, out float minX, out float maxX, out float minY, out float maxY))
				{
					m_boundsIndex.Add(new RegionBoundsEntry
					{
						FilePath = file,
						RegionId = regionId,
						MinX = minX,
						MaxX = maxX,
						MinY = minY,
						MaxY = maxY
					});
				}
			}

			m_initialized = true;
			Window.Get?.Log($"NavMesh: Indexed {m_boundsIndex.Count} navigation regions.");
		}

		/// <summary>
		/// Finds an obstacle-free A* path between two SRCoord positions.
		/// Returns null if outside navmesh or path not found.
		/// </summary>
		public List<SRCoord> FindPath(SRCoord start, SRCoord target)
		{
			if (!IsAvailable || start == null || target == null)
				return null;

			float startX = (float)start.PosX;
			float startY = (float)start.PosY;
			float targetX = (float)target.PosX;
			float targetY = (float)target.PosY;

			// If very close, no pathfinding needed
			if (start.DistanceTo(target) <= 4.0)
			{
				return new List<SRCoord> { target };
			}

			RegionBoundsEntry bestEntry = FindContainingRegion(startX, startY, targetX, targetY);
			if (bestEntry == null)
			{
				return null;
			}

			NavRegion region = GetOrLoadRegion(bestEntry);
			if (region == null)
				return null;

			Window.Get?.LogProcess($"NavMesh: Calculating path on [{Path.GetFileName(bestEntry.FilePath)}]...");
			DateTime startTime = DateTime.Now;

			List<SRCoord> path = m_pathfinder.FindPath(region, startX, startY, targetX, targetY);

			double ms = (DateTime.Now - startTime).TotalMilliseconds;
			if (path != null && path.Count > 0)
			{
				Window.Get?.Log($"NavMesh: Path found with {path.Count} waypoints ({ms:F1}ms).");
			}
			else
			{
				Window.Get?.LogProcess("NavMesh: Path could not be resolved.", Window.ProcessState.Warning);
			}

			return path;
		}

		private RegionBoundsEntry FindContainingRegion(float startX, float startY, float targetX, float targetY)
		{
			// 1. First priority: contains both start and target
			foreach (var entry in m_boundsIndex)
			{
				if (entry.Contains(startX, startY) && entry.Contains(targetX, targetY))
					return entry;
			}

			// 2. Second priority: contains start with padding
			foreach (var entry in m_boundsIndex)
			{
				if (entry.Contains(startX, startY, 30f))
					return entry;
			}

			return null;
		}

		private NavRegion GetOrLoadRegion(RegionBoundsEntry entry)
		{
			if (m_cache.TryGetValue(entry.RegionId, out NavRegion cached))
			{
				return cached;
			}

			NavRegion loaded = NavDataReader.Read(entry.FilePath, entry.RegionId);
			if (loaded != null)
			{
				// Keep cache small (max 5 regions in memory to save RAM)
				if (m_cache.Count > 5)
				{
					m_cache.Clear();
				}
				m_cache[entry.RegionId] = loaded;
			}
			return loaded;
		}

		private static int ParseRegionId(string fileName)
		{
			try
			{
				string name = Path.GetFileNameWithoutExtension(fileName).ToLower();
				bool isCave = name.StartsWith("cnav");
				string numStr = name.Replace("cnav", "").Replace("nav", "");
				if (int.TryParse(numStr, out int num))
				{
					return isCave ? (num | 0x8000) : num;
				}
			}
			catch { }
			return 0;
		}
	}
}
