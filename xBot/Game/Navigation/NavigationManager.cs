using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			public double DistanceTo(float x, float y)
			{
				float dx = Math.Max(0, Math.Max(MinX - x, x - MaxX));
				float dy = Math.Max(0, Math.Max(MinY - y, y - MaxY));
				return Math.Sqrt(dx * dx + dy * dy);
			}

			/// <summary>
			/// Checks if this region borders another region (overlapping edges with small gap tolerance).
			/// </summary>
			public bool Borders(RegionBoundsEntry other, float tolerance = 20f)
			{
				if (RegionId == other.RegionId)
					return false;

				// Check if rectangles are adjacent (touching edges within tolerance)
				bool xOverlap = MaxX + tolerance >= other.MinX && MinX - tolerance <= other.MaxX;
				bool yOverlap = MaxY + tolerance >= other.MinY && MinY - tolerance <= other.MaxY;
				
				// Check for edge adjacency (not just corner touching)
				bool xTouch = Math.Abs(MaxX - other.MinX) <= tolerance || Math.Abs(MinX - other.MaxX) <= tolerance;
				bool yTouch = Math.Abs(MaxY - other.MinY) <= tolerance || Math.Abs(MinY - other.MaxY) <= tolerance;

				return (xTouch && yOverlap) || (yTouch && xOverlap);
			}

			/// <summary>
			/// Gets the approximate crossing point from this region to another.
			/// Returns the midpoint of the shared edge.
			/// </summary>
			public SRCoord GetCrossingPoint(RegionBoundsEntry other)
			{
				float cx = 0, cy = 0;
				
				// Right edge of this touches left edge of other
				if (Math.Abs(MaxX - other.MinX) <= 20f)
				{
					cx = (MaxX + other.MinX) / 2f;
					float yOverlapMin = Math.Max(MinY, other.MinY);
					float yOverlapMax = Math.Min(MaxY, other.MaxY);
					cy = (yOverlapMin + yOverlapMax) / 2f;
				}
				// Left edge of this touches right edge of other
				else if (Math.Abs(MinX - other.MaxX) <= 20f)
				{
					cx = (MinX + other.MaxX) / 2f;
					float yOverlapMin = Math.Max(MinY, other.MinY);
					float yOverlapMax = Math.Min(MaxY, other.MaxY);
					cy = (yOverlapMin + yOverlapMax) / 2f;
				}
				// Top edge touches bottom
				else if (Math.Abs(MaxY - other.MinY) <= 20f)
				{
					cy = (MaxY + other.MinY) / 2f;
					float xOverlapMin = Math.Max(MinX, other.MinX);
					float xOverlapMax = Math.Min(MaxX, other.MaxX);
					cx = (xOverlapMin + xOverlapMax) / 2f;
				}
				// Bottom edge touches top
				else if (Math.Abs(MinY - other.MaxY) <= 20f)
				{
					cy = (MinY + other.MaxY) / 2f;
					float xOverlapMin = Math.Max(MinX, other.MinX);
					float xOverlapMax = Math.Min(MaxX, other.MaxX);
					cx = (xOverlapMin + xOverlapMax) / 2f;
				}
				else
				{
					// Fallback: center of this region
					cx = (MinX + MaxX) / 2f;
					cy = (MinY + MaxY) / 2f;
				}

				return new SRCoord((ushort)RegionId, (int)cx, (int)cy, 0);
			}
		}

		private string m_dataDirectory;
		private List<RegionBoundsEntry> m_boundsIndex;
		private Dictionary<int, LinkedListNode<CacheEntry>> m_cacheMap = new Dictionary<int, LinkedListNode<CacheEntry>>();
		private LinkedList<CacheEntry> m_cacheOrder = new LinkedList<CacheEntry>();
		private readonly object m_cacheSync = new object();
		private const int MaxCacheSize = 8;
		private class CacheEntry
		{
			public int RegionId;
			public NavRegion Region;
		}
		private AStarPathfinder m_pathfinder = new AStarPathfinder();
		private bool m_initialized = false;
		
		// Region adjacency graph for multi-region pathfinding
		private Dictionary<int, List<RegionConnection>> m_regionGraph = new Dictionary<int, List<RegionConnection>>();
		private class RegionConnection
		{
			public int TargetRegionId;
			public SRCoord CrossingPoint; // Where to cross from this region to target
			public double Weight; // Walking distance between region centers
		}

		public bool IsAvailable => m_initialized && m_boundsIndex != null && m_boundsIndex.Count > 0;

		public NavigationManager()
		{
			Initialize();
		}

		public void Initialize()
		{
			if (m_initialized)
				return;

			// Check potential paths for navdata (relative first, no hardcoded dev paths)
			// NOTE: bin/Debug exe'sinden repo kökündeki navdata'ya ulaşmak için 3 üst seviye gerekir:
			// bin/Debug -> bin -> xBot -> xBot-WinForms(navdata burada)
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			string[] candidateDirs = new string[]
			{
				Path.Combine(baseDir, "navdata"),
				Path.Combine(Environment.CurrentDirectory, "navdata"),
				Path.Combine(baseDir, "..", "navdata"),
				Path.Combine(baseDir, "..", "..", "navdata"),
				Path.Combine(baseDir, "..", "..", "..", "navdata"),
				Path.Combine(baseDir, "..", "..", "..", "..", "navdata")
			};

			foreach (string rawDir in candidateDirs)
			{
				string dir = rawDir;
				try { dir = Path.GetFullPath(rawDir); } catch { }
				if (Directory.Exists(dir))
				{
					string[] datFiles = new string[0];
					try { datFiles = Directory.GetFiles(dir, "*.dat"); } catch { }
					if (datFiles.Length > 0)
					{
						m_dataDirectory = dir;
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(m_dataDirectory))
			{
				Window.Get?.LogProcess("NavMesh: navdata folder not found.", Window.ProcessState.Warning);
				// LogProcess dosyaya düşmeyebilir; kullanıcı session_debug.log'da da görsün:
				try { Window.Get?.Log("NavMesh: navdata klasörü bulunamadı! exe yanına 'navdata' kopyalayın (uzun mesafe yürüyüş çalışmaz)."); } catch { }
				return;
			}

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
			Window.Get?.Log($"NavMesh: Indexed {m_boundsIndex.Count} navigation regions from [{m_dataDirectory}].");

			// Build region adjacency graph for multi-region pathfinding
			BuildRegionGraph();
		}

		/// <summary>
		/// Builds the region adjacency graph by detecting bordering regions.
		/// </summary>
		private void BuildRegionGraph()
		{
			m_regionGraph.Clear();
			
			foreach (var entry in m_boundsIndex)
			{
				var connections = new List<RegionConnection>();
				
				foreach (var other in m_boundsIndex)
				{
					if (entry.Borders(other))
					{
						var crossingPoint = entry.GetCrossingPoint(other);
						double weight = entry.DistanceTo(other.MinX, other.MinY); // Approximate weight
						
						connections.Add(new RegionConnection
						{
							TargetRegionId = other.RegionId,
							CrossingPoint = crossingPoint,
							Weight = weight
						});
					}
				}
				
				if (connections.Count > 0)
					m_regionGraph[entry.RegionId] = connections;
			}
			
			Window.Get?.Log($"NavMesh: Built region graph with {m_regionGraph.Count} connected regions ({m_regionGraph.Values.Sum(c => c.Count)} edges).");
		}

		/// <summary>
		/// Finds an obstacle-free A* path between two SRCoord positions.
		/// Returns null if outside navmesh or path not found.
		/// </summary>
		public List<SRCoord> FindPath(SRCoord start, SRCoord target)
		{
			if (!IsAvailable || start == null || target == null)
			{
				Window.Get?.Log($"NavMesh: Not available or null coords (Init={m_initialized}, Start={start != null}, Target={target != null})");
				return null;
			}

			float startX = (float)start.PosX;
			float startY = (float)start.PosY;
			float targetX = (float)target.PosX;
			float targetY = (float)target.PosY;

			// If already close, no pathfinding needed
			if (start.DistanceTo(target) <= 5.0)
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
			{
				Window.Get?.Log($"NavMesh: Failed to load region file [{Path.GetFileName(bestEntry.FilePath)}]");
				return null;
			}

			System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
			List<SRCoord> path = m_pathfinder.FindPath(region, startX, startY, targetX, targetY);
			sw.Stop();

			if (path == null || path.Count == 0)
			{
				Window.Get?.LogProcess("NavMesh: Path could not be resolved on single region.", Window.ProcessState.Warning);
			}

			return path;
		}

		/// <summary>
		/// Finds a complete compound route. If direct path is not possible (e.g. river or different region),
		/// incorporates Ferries and Teleports automatically.
		/// </summary>
		public NavigationRoute FindCompoundRoute(SRCoord start, SRCoord target)
		{
			if (!IsAvailable || start == null || target == null)
				return null;

			NavigationRoute route = new NavigationRoute();

			// 1. Try direct path first
			List<SRCoord> direct = FindPath(start, target);
			if (direct != null && direct.Count > 0)
			{
				route.Segments.Add(RouteSegment.CreateWalk(direct));
				return route;
			}

			// 2. Try multi-region NavMesh path (walking through adjacent regions)
			var multiRegionRoute = FindMultiRegionRoute(start, target);
			if (multiRegionRoute != null && !multiRegionRoute.IsEmpty)
			{
				return multiRegionRoute;
			}

		// 3. Multi-region path not found. Search for Ferry / Teleport bridge!
		Window.Get?.LogProcess("NavMesh: Multi-region path unavailable. Searching for Ferry/Teleport bridges...");
		TeleportLinkInfo bestLink = TeleportManager.Get.FindBestLink(start, target);
		if (bestLink != null)
		{
			Window.Get?.Log($"NavMesh: Bridge selected: [{bestLink.SourceName} -> {bestLink.DestinationName}]");

			// Segment 1: Walk to ferry/gate board position — kapının tam üstüne
			// değil 17m yakınına (Dimensional Gate/NPC collision'ı var, dibine
			// girmek takılma yapar). Son yaklaşmayı teleport adımı radyal yapar.
			SRCoord boardStand = StandOff(start, bestLink.BoardCoord, 17.0);
			Window.Get?.Log($"NavMesh: walk-to-board stops {start.DistanceTo(boardStand):F0}m from gate (collision margin 17m).");
			List<SRCoord> pathToBoard = FindPath(start, boardStand);
			if (pathToBoard == null || pathToBoard.Count == 0)
			{
				pathToBoard = new List<SRCoord> { boardStand };
			}
			else
			{
				// Kapı çevresindeki sık waypoint'leri ele: NavMesh araziyi bilir,
				// kapı/havuz/duvar yapısını bilmez — bu mikro noktalar karakteri
				// yapının içine sokup duvara toslatır. 20m dışı korunur, final
				// yaklaşmayı teleport adımı radyal yapar.
				var trimmed = pathToBoard.Where(wp => wp.DistanceTo(bestLink.BoardCoord) > 20.0).ToList();
				trimmed.Add(boardStand);
				pathToBoard = trimmed;
			}
			route.Segments.Add(RouteSegment.CreateWalk(pathToBoard));

				// Segment 2: Ferry Teleport
				route.Segments.Add(RouteSegment.CreateTeleport(bestLink));

				// Segment 3: Walk from arrival position to target
				List<SRCoord> pathFromArrive = FindPath(bestLink.ArriveCoord, target);
				if (pathFromArrive == null || pathFromArrive.Count == 0)
				{
					pathFromArrive = new List<SRCoord> { target };
				}
				route.Segments.Add(RouteSegment.CreateWalk(pathFromArrive));

				Window.Get?.Log($"NavMesh: Multi-hop route created: {route.Segments.Count} segments, {route.TotalWaypointsCount} total waypoints.");
				return route;
			}

			return null;
		}

		/// <summary>
		/// Finds the region containing the given position.
		/// </summary>
		public RegionBoundsEntry FindRegionAt(float x, float y, float padding = 40f)
		{
			foreach (var entry in m_boundsIndex)
			{
				if (entry.Contains(x, y, padding))
					return entry;
			}
			return null;
		}

		/// <summary>
		/// Finds a multi-region path by computing region sequence via graph A*
		/// and stitching together intra-region paths through crossing points.
		/// </summary>
		public NavigationRoute FindMultiRegionRoute(SRCoord start, SRCoord target)
		{
			if (!IsAvailable || start == null || target == null)
				return null;

			var startRegion = FindRegionAt((float)start.PosX, (float)start.PosY);
			var targetRegion = FindRegionAt((float)target.PosX, (float)target.PosY);

			if (startRegion == null || targetRegion == null)
			{
				Window.Get?.LogProcess("NavMesh: Start or target position not in any known region.", Window.ProcessState.Warning);
				return null;
			}

			// Same region - use direct path
			if (startRegion.RegionId == targetRegion.RegionId)
			{
				var direct = FindPath(start, target);
				if (direct != null && direct.Count > 0)
				{
					var singleRegionRoute = new NavigationRoute();
					singleRegionRoute.Segments.Add(RouteSegment.CreateWalk(direct));
					return singleRegionRoute;
				}
				return null;
			}

			// Find region sequence using graph A*
			var regionPath = FindRegionPath(startRegion.RegionId, targetRegion.RegionId);
			if (regionPath == null || regionPath.Count < 2)
			{
				Window.Get?.LogProcess("NavMesh: No region path found.", Window.ProcessState.Warning);
				return null; // Let FindCompoundRoute handle teleport/ferry fallback
			}

			Window.Get?.Log($"NavMesh: Multi-region route through {regionPath.Count} regions: {string.Join(" -> ", regionPath)}");

			// Build the complete route by stitching intra-region paths
			var route = new NavigationRoute();
			SRCoord currentPos = start;

			for (int i = 0; i < regionPath.Count - 1; i++)
			{
				int currentRegionId = regionPath[i];
				int nextRegionId = regionPath[i + 1];

				var currentEntry = m_boundsIndex.Find(e => e.RegionId == currentRegionId);
				var nextEntry = m_boundsIndex.Find(e => e.RegionId == nextRegionId);

				if (currentEntry == null || nextEntry == null)
					continue;

				// Get crossing point between regions
				SRCoord crossingPoint = currentEntry.GetCrossingPoint(nextEntry);

				// Path from current position to crossing point
				var segmentPath = FindPath(currentPos, crossingPoint);
				if (segmentPath != null && segmentPath.Count > 0)
				{
					route.Segments.Add(RouteSegment.CreateWalk(segmentPath));
					currentPos = segmentPath[segmentPath.Count - 1];
				}
				else
				{
					// Fallback: direct to crossing point
					route.Segments.Add(RouteSegment.CreateWalk(new List<SRCoord> { crossingPoint }));
					currentPos = crossingPoint;
				}
			}

			// Final segment from last crossing to target
			var finalSegment = FindPath(currentPos, target);
			if (finalSegment != null && finalSegment.Count > 0)
			{
				route.Segments.Add(RouteSegment.CreateWalk(finalSegment));
			}

			Window.Get?.Log($"NavMesh: Multi-region route created: {route.Segments.Count} segments, {route.TotalWaypointsCount} total waypoints.");
			return route;
		}

		/// <summary>
		/// A* pathfinding on the region adjacency graph.
		/// Returns the sequence of region IDs from start to target.
		/// </summary>
		private List<int> FindRegionPath(int startRegionId, int targetRegionId)
		{
			if (!m_regionGraph.ContainsKey(startRegionId) || !m_regionGraph.ContainsKey(targetRegionId))
				return null;

			var openSet = new SortedSet<RegionNode>(Comparer<RegionNode>.Create((a, b) =>
			{
				int cmp = a.F.CompareTo(b.F);
				if (cmp == 0) cmp = a.RegionId.CompareTo(b.RegionId);
				return cmp;
			}));
			
			var closedSet = new HashSet<int>();
			var cameFrom = new Dictionary<int, int>();
			var gScore = new Dictionary<int, double>();

			openSet.Add(new RegionNode { RegionId = startRegionId, G = 0, H = HeuristicRegionDistance(startRegionId, targetRegionId) });
			gScore[startRegionId] = 0;

			while (openSet.Count > 0)
			{
				var current = openSet.Min;
				openSet.Remove(current);

				if (current.RegionId == targetRegionId)
				{
					// Reconstruct path
					var path = new List<int>();
					int nodeId = targetRegionId;
					while (cameFrom.ContainsKey(nodeId))
					{
						path.Add(nodeId);
						nodeId = cameFrom[nodeId];
					}
					path.Add(startRegionId);
					path.Reverse();
					return path;
				}

				closedSet.Add(current.RegionId);

				if (!m_regionGraph.TryGetValue(current.RegionId, out var neighbors))
					continue;

				foreach (var conn in neighbors)
				{
					if (closedSet.Contains(conn.TargetRegionId))
						continue;

					double tentativeG = current.G + conn.Weight;
					
					if (!gScore.ContainsKey(conn.TargetRegionId) || tentativeG < gScore[conn.TargetRegionId])
					{
						cameFrom[conn.TargetRegionId] = current.RegionId;
						gScore[conn.TargetRegionId] = tentativeG;
						double h = HeuristicRegionDistance(conn.TargetRegionId, targetRegionId);
						openSet.Add(new RegionNode { RegionId = conn.TargetRegionId, G = tentativeG, H = h });
					}
				}
			}

			return null; // No path found
		}

		private double HeuristicRegionDistance(int regionId1, int regionId2)
		{
			var e1 = m_boundsIndex.Find(e => e.RegionId == regionId1);
			var e2 = m_boundsIndex.Find(e => e.RegionId == regionId2);
			if (e1 == null || e2 == null) return 0;

			double cx1 = (e1.MinX + e1.MaxX) / 2.0;
			double cy1 = (e1.MinY + e1.MaxY) / 2.0;
			double cx2 = (e2.MinX + e2.MaxX) / 2.0;
			double cy2 = (e2.MinY + e2.MaxY) / 2.0;

			return Math.Sqrt((cx1 - cx2) * (cx1 - cx2) + (cy1 - cy2) * (cy1 - cy2));
		}

		private class RegionNode
		{
			public int RegionId;
			public double G; // Cost from start
			public double H; // Heuristic to target
			public double F => G + H;
		}

		private RegionBoundsEntry FindContainingRegion(float startX, float startY, float targetX, float targetY)
		{
			// Direct single-region path is ONLY possible if one region contains BOTH start and target!
			// If start and target are in different regions (e.g. across Huang He river or in different cities),
			// return null so FindCompoundRoute will search for Ferry/Teleport bridges.
			foreach (var entry in m_boundsIndex)
			{
				if (entry.Contains(startX, startY, 40f) && entry.Contains(targetX, targetY, 40f))
					return entry;
			}

			return null;
		}

		private NavRegion GetOrLoadRegion(RegionBoundsEntry entry)
		{
			lock (m_cacheSync)
			{
				if (m_cacheMap.TryGetValue(entry.RegionId, out LinkedListNode<CacheEntry> node))
				{
					m_cacheOrder.Remove(node);
					m_cacheOrder.AddFirst(node);
					return node.Value.Region;
				}
			}

			// File IO is intentionally outside the cache lock.
			NavRegion loaded = NavDataReader.Read(entry.FilePath, entry.RegionId);
			if (loaded != null)
			{
				lock (m_cacheSync)
				{
					// Another preload may have completed while this file was read.
					if (m_cacheMap.TryGetValue(entry.RegionId, out LinkedListNode<CacheEntry> existing))
					{
						m_cacheOrder.Remove(existing);
						m_cacheOrder.AddFirst(existing);
						return existing.Value.Region;
					}
					if (m_cacheMap.Count >= MaxCacheSize && m_cacheOrder.Last != null)
					{
						var lru = m_cacheOrder.Last;
						m_cacheOrder.RemoveLast();
						m_cacheMap.Remove(lru.Value.RegionId);
					}
					var newNode = new LinkedListNode<CacheEntry>(new CacheEntry { RegionId = entry.RegionId, Region = loaded });
					m_cacheOrder.AddFirst(newNode);
					m_cacheMap[entry.RegionId] = newNode;
				}
			}
			return loaded;
		}

		/// <summary>
		/// Preloads a region asynchronously into the cache.
		/// Returns a Task that completes when the region is loaded.
		/// </summary>
		public System.Threading.Tasks.Task PreloadRegionAsync(int regionId)
		{
			return System.Threading.Tasks.Task.Run(() =>
			{
				var entry = m_boundsIndex.Find(e => e.RegionId == regionId);
				if (entry != null)
				{
					GetOrLoadRegion(entry);
				}
			});
		}

		/// <summary>
		/// Warms up the navigation cache by preloading regions near the given position.
		/// Loads the containing region and adjacent regions within the specified radius.
		/// </summary>
		public void WarmupCacheNear(float x, float y, int adjacentRegionCount = 2)
		{
			if (!IsAvailable)
				return;

			var containing = m_boundsIndex.Find(e => e.Contains(x, y, 40f));
			if (containing == null)
				return;

			// Load containing region first (synchronously for immediate use)
			GetOrLoadRegion(containing);

			// Then preload adjacent regions asynchronously
			var adjacent = m_boundsIndex
				.Where(e => e.RegionId != containing.RegionId)
				.OrderBy(e => e.DistanceTo(x, y))
				.Take(adjacentRegionCount);

			foreach (var entry in adjacent)
			{
				PreloadRegionAsync(entry.RegionId);
			}
		}

		/// <summary>
	/// Kapı/NPC'nin tam üstüne değil standDist metre yakınına durma noktası
	/// (oyundan hedefe bakınca hedefin gerisi). Collision takılmasını önler.
	/// </summary>
	private static SRCoord StandOff(SRCoord from, SRCoord target, double standDist)
	{
		try
		{
			if (from == null || target == null)
				return target;
			double dx = from.PosX - target.PosX;
			double dy = from.PosY - target.PosY;
			double d = System.Math.Sqrt(dx * dx + dy * dy);
			if (d < 0.001 || d <= standDist)
				return from;
			double k = standDist / d;
			return new SRCoord(target.PosX + dx * k, target.PosY + dy * k);
		}
		catch { return target; }
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
