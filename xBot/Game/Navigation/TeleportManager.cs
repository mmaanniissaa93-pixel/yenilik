using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using xBot.App;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
	public class TeleportLinkInfo
	{
		public uint SourceId { get; set; }
		public uint DestinationId { get; set; }
		public uint NpcId { get; set; }
		public string SourceName { get; set; }
		public string DestinationName { get; set; }
		public SRCoord BoardCoord { get; set; }
		public SRCoord ArriveCoord { get; set; }
		public bool IsFerry { get; set; }

		public override string ToString()
		{
			return $"[{SourceName} -> {DestinationName}] Board:({(int)BoardCoord.PosX},{(int)BoardCoord.PosY}) Arrive:({(int)ArriveCoord.PosX},{(int)ArriveCoord.PosY})";
		}
	}

	public class TeleportManager
	{
		private static TeleportManager m_instance;
		public static TeleportManager Get => m_instance ?? (m_instance = new TeleportManager());

		private List<TeleportLinkInfo> m_links = new List<TeleportLinkInfo>();
		private bool m_initialized = false;

		public bool IsAvailable => m_initialized && m_links.Count > 0;
		public List<TeleportLinkInfo> Links => m_links;

		// Sürekli başarısız olan köprü geçici kara listeye alınır (saf politika);
		// bot aynı yanlış NPC/dialog döngüsüne girip takılmaz, alternatife düşer.
		public bool IsLinkBlacklisted(TeleportLinkInfo link)
		{
			if (link == null)
				return true;
			return TeleportLinkPolicy.IsBlacklisted(link.SourceId, link.DestinationId, DateTime.Now);
		}

		public void NoteLinkFailure(TeleportLinkInfo link)
		{
			if (link == null)
				return;
			if (TeleportLinkPolicy.NoteFailure(link.SourceId, link.DestinationId, DateTime.Now))
				Window.Get?.Log($"TeleportManager: [{link.SourceName} -> {link.DestinationName}] {TeleportLinkPolicy.MaxLinkFailures} kez başarısız — 10dk kara listede, alternatif rota/script denenecek.");
		}

		public void NoteLinkSuccess(TeleportLinkInfo link)
		{
			if (link == null)
				return;
			TeleportLinkPolicy.NoteSuccess(link.SourceId, link.DestinationId);
		}

		public TeleportManager()
		{
			Initialize();
		}

		public void Initialize()
		{
			if (m_initialized)
				return;

			m_links.Clear();

			// 1. Try to load from SQLite database
			TryLoadFromDatabase();

			// 2. Add Built-in / Fallback Ferries to guarantee 100% availability
			AddBuiltinFerries();

			m_initialized = true;
			Window.Get?.Log($"TeleportManager: Loaded {m_links.Count} teleport and ferry connections.");
		}

		private void TryLoadFromDatabase()
		{
			try
			{
				// Aktif Data dizinine göre relative çözümle, hardcoded dev yolu yok
				var candidates = new System.Collections.Generic.List<string>();
				string baseDir = AppDomain.CurrentDomain.BaseDirectory;
				string silk = Game.DataManager.SilkroadName;
				if (!string.IsNullOrEmpty(silk))
				{
					candidates.Add(Path.Combine(baseDir, "Data", silk, "Database.sqlite3"));
					candidates.Add(Path.Combine(Environment.CurrentDirectory, "Data", silk, "Database.sqlite3"));
				}
				candidates.Add(Path.Combine(baseDir, "Data", "Silkroad #2", "Database.sqlite3"));
				candidates.Add(Path.Combine(baseDir, "Data", "Silkroad #1", "Database.sqlite3"));
				candidates.Add(Path.Combine(baseDir, "Data", "Database.sqlite3"));
				string[] candidatePaths = candidates.ToArray();

				string dbPath = null;
				foreach (string p in candidatePaths)
				{
					if (File.Exists(p))
					{
						dbPath = p;
						break;
					}
				}

				if (string.IsNullOrEmpty(dbPath))
					return;

				SQLDatabase db = new SQLDatabase(dbPath);
				if (!db.Connect())
					return;

				string query = "SELECT sourceid, destinationid, id, name, destination, spawn_region, spawn_x, spawn_y, spawn_z, pos_region, pos_x, pos_y, pos_z FROM teleportlinks;";
				var results = db.GetResultFromQuery(query);
				db.Close();

				if (results != null && results.Count > 0)
				{
					foreach (var row in results)
					{
						try
						{
							uint srcId = uint.Parse(row["sourceid"]);
							uint dstId = uint.Parse(row["destinationid"]);
							uint npcId = uint.Parse(row["id"]);
							string srcName = row["name"]?.Trim();
							string dstName = row["destination"]?.Trim();

							ushort sReg = ushort.Parse(row["spawn_region"]);
							int sX = int.Parse(row["spawn_x"]);
							int sY = int.Parse(row["spawn_y"]);
							int sZ = int.Parse(row["spawn_z"]);

							ushort tReg = ushort.Parse(row["pos_region"]);
							int tX = int.Parse(row["pos_x"]);
							int tY = int.Parse(row["pos_y"]);
							int tZ = int.Parse(row["pos_z"]);

							// Board coord is spawn (where NPC is)
							SRCoord boardCoord = new SRCoord(sReg, sX, sZ, sY);
							// Arrive coord is pos (where player lands)
							SRCoord arriveCoord = new SRCoord(tReg, tX, tZ, tY);

							bool isFerry = srcName.ToLower().Contains("ferry") || dstName.ToLower().Contains("ferry") ||
							               srcName.ToLower().Contains("ticket") || dstName.ToLower().Contains("ticket");

							m_links.Add(new TeleportLinkInfo
							{
								SourceId = srcId,
								DestinationId = dstId,
								NpcId = npcId,
								SourceName = srcName,
								DestinationName = dstName,
								BoardCoord = boardCoord,
								ArriveCoord = arriveCoord,
								IsFerry = isFerry
							});
						}
						catch { }
					}
				}
			}
			catch (Exception ex)
			{
				Window.Get?.LogProcess("TeleportManager DB load: " + ex.Message, Window.ProcessState.Warning);
			}
		}

		private void AddBuiltinFerries()
		{
			// Huang He River Ferries (Always guarantee these crucial river crossings)
			AddIfMissing(6, 9, 2120, "Ferry Ticket Seller Hageuk", "Ferry Ticket Seller Chau", 25244, 1025, 601, 160, 24734, 523, 1267, 140, true);
			AddIfMissing(9, 6, 2119, "Ferry Ticket Seller Chau", "Ferry Ticket Seller Hageuk", 24734, 523, 1267, 140, 25244, 1025, 601, 160, true);
			AddIfMissing(4, 3, 2056, "Ferry Ticket Seller Tayun", "Ferry Ticket Seller Doji", 25761, 636, 1482, 160, 24993, 560, 1460, 140, true);
			AddIfMissing(3, 4, 2011, "Ferry Ticket Seller Doji", "Ferry Ticket Seller Tayun", 24993, 560, 1460, 140, 25761, 636, 1482, 160, true);

			// Jangan <-> Donwhang Teleport Gate
			AddIfMissing(1, 2, 2094, "Jangan", "Donwhang", 25000, 969, 1369, 0, 26265, 957, 1508, -80, false);
			AddIfMissing(2, 1, 2095, "Donwhang", "Jangan", 26265, 957, 1508, -80, 25000, 969, 1369, 0, false);
			// Donwhang <-> Hotan
			AddIfMissing(2, 5, 2095, "Donwhang", "Hotan", 26265, 957, 1508, -80, 23687, 1138, 153, 250, false);
			AddIfMissing(5, 2, 2096, "Hotan", "Donwhang", 23687, 1138, 153, 250, 26265, 957, 1508, -80, false);
			// Hotan <-> Samarkand
			AddIfMissing(5, 25, 2096, "Hotan", "Samarkand", 23687, 1138, 153, 250, 27244, 270, 1421, 180, false);
			AddIfMissing(25, 5, 2095, "Samarkand", "Hotan", 27244, 270, 1421, 180, 23687, 1138, 153, 250, false);
			// Samarkand <-> Constantinople
			AddIfMissing(25, 20, 2095, "Samarkand", "Constantinople", 27244, 270, 1421, 180, 26959, 950, 1070, 84, false);
			AddIfMissing(20, 25, 19554, "Constantinople", "Samarkand", 26959, 950, 1070, 84, 27244, 270, 1421, 180, false);
		}

		private void AddIfMissing(uint srcId, uint dstId, uint npcId, string srcName, string dstName,
			ushort sReg, int sX, int sY, int sZ, ushort tReg, int tX, int tY, int tZ, bool isFerry)
		{
			bool exists = m_links.Exists(l => l.SourceId == srcId && l.DestinationId == dstId);
			if (!exists)
			{
				m_links.Add(new TeleportLinkInfo
				{
					SourceId = srcId,
					DestinationId = dstId,
					NpcId = npcId,
					SourceName = srcName,
					DestinationName = dstName,
					BoardCoord = new SRCoord(sReg, sX, sZ, sY),
					ArriveCoord = new SRCoord(tReg, tX, tZ, tY),
					IsFerry = isFerry
				});
			}
		}

		/// <summary>
		/// Finds the best Teleport/Ferry bridge to cross from start position towards target position.
		/// </summary>
		public TeleportLinkInfo FindBestLink(SRCoord start, SRCoord target)
		{
			if (start == null || target == null || m_links.Count == 0)
				return null;

			TeleportLinkInfo bestLink = null;
			double bestScore = double.MaxValue;
			double directDistance = start.DistanceTo(target);

			foreach (var link in m_links)
			{
				// Kara listedeki köprü atlanır (yanlış NPC döngüsüne girilmez).
				if (IsLinkBlacklisted(link))
					continue;

				double distToBoard = start.DistanceTo(link.BoardCoord);
				double distFromArriveToTarget = link.ArriveCoord.DistanceTo(target);

				// Only consider bridges that bring us closer to the destination
				if (distFromArriveToTarget >= directDistance)
					continue;

				// Weighted walking distance
				double score = distToBoard + distFromArriveToTarget;
				if (link.IsFerry)
				{
					score *= 0.6; // Strong priority for river ferries
				}

				if (score < bestScore)
				{
					bestScore = score;
					bestLink = link;
				}
			}

			if (bestLink != null)
			{
				Window.Get?.Log($"TeleportManager: Selected [{bestLink.SourceName} -> {bestLink.DestinationName}] (Board: {start.DistanceTo(bestLink.BoardCoord):F0}m, Target: {bestLink.ArriveCoord.DistanceTo(target):F0}m)");
			}

			return bestLink;
		}
	}
}
