using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using xBot.App;
using xBot.PK2Extractor;
using xBot.Game.Objects;
using xBot.Game.Objects.Guild;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;
using xBot.Game.Objects.Common;

namespace xBot.Game
{
	public static class DataManager
	{
		#region (Properties)
		/// <summary>
		/// The current path to the SR_Client.
		/// </summary>
		public static string ClientPath { get; set; }
		/// <summary>
		/// Unique name from the Silkroad.
		/// </summary>
		public static string SilkroadName { get; private set; }
		/// <summary>
		/// Silkroad Locale
		/// </summary>
		public static byte Locale { get; set; }
		/// <summary>
		/// SR_Client name
		/// </summary>
		public static string SR_Client { get; set; }
		/// <summary>
		/// Silkroad Version
		/// </summary>
		public static uint Version { get; set; }
		/// <summary>
		/// Gets the database previouly selected.
		/// </summary>
		private static SQLDatabase Database { get; set; }
		private static readonly object DatabaseSync = new object();
		private static bool QuestCatalogAvailable { get; set; }
		#endregion

		/// <summary>
		/// Select the database if exists. Return success.
		/// </summary>
		/// <param name="name">Database unique name</param>
		public static bool ConnectToDatabase(string SilkroadName)
		{
			if (Pk2Extractor.DirectoryExists(SilkroadName))
			{
				string dbPath = Pk2Extractor.GetDatabasePath(SilkroadName);
				// SQLite dosya yoksa sessizce boş DB açar; boş/eksik DB ile 0x3013 akışı
				// kayar (item tipleri DB'den okunur) ve skill/mob listeleri boş gelir.
				if (!System.IO.File.Exists(dbPath))
					return false;
				SQLDatabase candidate = new SQLDatabase(dbPath);
				if (!candidate.Connect())
				{
					candidate.Dispose();
					return false;
				}
				List<NameValueCollection> tables;
				try
				{
					tables = candidate.GetResultFromQuery("SELECT name FROM sqlite_master WHERE type='table' AND name IN ('items','skills','models','quests')");
					if (tables == null || tables.Count < 3)
					{
						candidate.Dispose();
						return false;
					}
				}
				catch
				{
					candidate.Dispose();
					return false;
				}

				lock (DatabaseSync)
				{
					Database?.Dispose();
					Database = candidate;
					DataManager.SilkroadName = SilkroadName;
					QuestCatalogAvailable = false;
					foreach (NameValueCollection table in tables)
						if (string.Equals(table["name"], "quests", StringComparison.OrdinalIgnoreCase)) QuestCatalogAvailable = true;
					s_cacheById.Clear();
				}
				return true;
			}
			return false;
		}
		public static void DisconnectDatabase()
		{
			lock (DatabaseSync)
			{
				Database?.Dispose();
				Database = null;
				SilkroadName = null;
				QuestCatalogAvailable = false;
				s_cacheById.Clear();
			}
		}

		private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, NameValueCollection> s_cacheById = new System.Collections.Concurrent.ConcurrentDictionary<string, NameValueCollection>();
		private static NameValueCollection GetCached(string key, System.Func<NameValueCollection> loader)
		{
			if (s_cacheById.TryGetValue(key, out NameValueCollection hit))
				return hit;
			NameValueCollection val = loader();
			if (val != null)
			{
				if (s_cacheById.Count > 4000)
					s_cacheById.Clear();
				s_cacheById[key] = val;
			}
			return val;
		}
		private static bool IsDbReady()
		{
			lock (DatabaseSync) { return Database != null; }
		}
		private static List<NameValueCollection> Query(string sql)
		{
			lock (DatabaseSync)
			{
				if (Database == null) return new List<NameValueCollection>();
				return Database.GetResultFromQuery(sql);
			}
		}
		private static List<NameValueCollection> Query(string sql, params object[] args)
		{
			lock (DatabaseSync)
			{
				if (Database == null) return new List<NameValueCollection>();
				return Database.GetResultFromQuery(sql, args);
			}
		}

		#region Gets from Database
			/// <summary>
			/// Gets the maximum exp required for the level specified.
			/// </summary>
		public static ulong GetExpMax(byte level)
		{
			if (!IsDbReady()) return 0;
			List<NameValueCollection> result = Query("SELECT player FROM leveldata WHERE level=@p0", level);
      if (result.Count > 0)
				return ulong.Parse(result[0]["player"]);
			return 0;
		}
		/// <summary>
		/// Gets the maximum exp required for the level specified.
		/// </summary>
		public static ulong GetPetExpMax(byte level)
		{
			List<NameValueCollection> result = Query("SELECT pet FROM leveldata WHERE level=@p0", level);
      if (result.Count > 0)
				return ulong.Parse(result[0]["pet"]);
			return 0;
		}
		/// <summary>
		/// Gets the maximum exp required for the job level specified.
		/// </summary>
		/// <param name="level">Job level</param>
		/// <param name="type">Trader, Thief or Hunter</param>
		public static uint GetJobExpMax(byte level, SRPlayer.Job type)
		{
			if (type == SRPlayer.Job.None)
				return 0;
			List<NameValueCollection> result = Query("SELECT * FROM leveldata WHERE level=@p0", level);
      if (result.Count > 0)
				return uint.Parse(result[0][type.ToString().ToLower()]);
			return 0;
		}
		/// <summary>
		/// Get model by id, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetModelData(uint id)
		{
			return GetCached("model:" + id, () => {
				List<NameValueCollection> result = Query("SELECT * FROM models WHERE id=@p0", id);
				if (result.Count > 0)
					return result[0];
				return null;
			});
		}
		/// <summary>
		/// Get model by servername, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetModelData(string servername)
		{
			List<NameValueCollection> result = Query("SELECT * FROM models WHERE servername=@p0", servername);
      if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get teleportlink by id, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetTeleport(uint id)
		{
			List<NameValueCollection> result = Query("SELECT * FROM teleportbuildings WHERE id=@p0", id);
      if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get teleportlink by servername, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetTeleport(string servername)
		{
			List<NameValueCollection> result = Query("SELECT * FROM teleportbuildings WHERE servername=@p0", servername);
      if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get teleportlink by id, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetTeleportLinkByID(uint id)
		{
			List<NameValueCollection> result = Query("SELECT * FROM teleportlinks WHERE id=@p0", id);
      if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get teleportlink by servername, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetTeleportLinkByServerName(string servername)
		{
			List<NameValueCollection> result = Query("SELECT * FROM teleportlinks WHERE servername=@p0", servername);
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get item by id, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetItemData(uint id)
		{
			return GetCached("item:" + id, () => {
				List<NameValueCollection> result = Query("SELECT * FROM items WHERE id=@p0", id);
				if (result.Count > 0)
					return result[0];
				return null;
			});
		}
		/// <summary>
		/// Get item by servername, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetItemData(string servername)
		{
			List<NameValueCollection> result = Query("SELECT * FROM items WHERE servername=@p0", servername);
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get an magic option from items, by id.
		/// </summary>
		public static NameValueCollection GetMagicOption(uint id)
		{
			List<NameValueCollection> result = Query("SELECT * FROM magicoptions WHERE id=@p0", id);
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get an magic option from items, by servername.
		/// </summary>
		public static NameValueCollection GetMagicOption(string servername)
		{
			List<NameValueCollection> result = Query("SELECT * FROM magicoptions WHERE servername=@p0", servername);
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Pick Filter kataloğu: kategori + arama ile eşya listesi (en fazla 500 satır).
		/// group: All/Weapon/Armor/Shield/Accessory/Avatar/Potion/Pill/Scroll/Ammo/Gold/
		/// TradeGoods/Quest/Elixir/AlchemyMaterial/CosTransport/Other.
		/// race: All/Chinese/European. gender: Any/Male/Female. degree: 0=Any, 1-16.
		/// Liste tembeldir: çağrıcı filtre daraltmadan çağırmamalıdır (RAM şişmesini önler).
		/// </summary>
		public static List<NameValueCollection> QueryItems(string group, string race, string gender, int degree, string search)
		{
			if (!IsDbReady()) return new List<NameValueCollection>();
			try
			{
				var conds = new List<string>();
				var args = new List<object>();
				string g = (group ?? "All").Trim();
				switch (g)
				{
					case "Weapon": conds.Add("(tid2=1 AND tid3=6)"); break;
					case "Armor": conds.Add("(tid2=1 AND tid3 IN (1,2,3,9,10,11))"); break;
					case "Shield": conds.Add("(tid2=1 AND tid3=4)"); break;
					case "Accessory": conds.Add("(tid2=1 AND tid3 IN (5,12))"); break;
					case "Avatar": conds.Add("(tid2=1 AND tid3 IN (13,14))"); break;
					case "Potion": conds.Add("(tid2=3 AND tid3=1)"); break;
					case "Pill": conds.Add("(tid2=3 AND tid3=2)"); break;
					case "Scroll": conds.Add("(tid2=3 AND tid3=3)"); break;
					case "Ammo": conds.Add("(tid2=3 AND tid3=4)"); break;
					case "Gold": conds.Add("(tid2=3 AND tid3=5)"); break;
					case "TradeGoods": conds.Add("(tid2=3 AND tid3=8)"); break;
					case "Quest": conds.Add("(tid2=3 AND tid3=9)"); break;
					case "Elixir": conds.Add("(tid2=3 AND tid3=10)"); break;
					case "AlchemyMaterial": conds.Add("(tid2=3 AND tid3=11)"); break;
					case "CosTransport": conds.Add("(tid2=2)"); break;
					case "Other":
						conds.Add("((tid2=1 AND tid3 IN (7)) OR (tid2=3 AND tid3 IN (6,7,12,13,14,15,16)))");
						break;
				}
				string r = (race ?? "All").Trim();
				if (r == "Chinese") conds.Add("servername LIKE '%_CH_%'");
				else if (r == "European") conds.Add("servername LIKE '%_EU_%'");
				string gd = (gender ?? "Any").Trim();
				if (gd == "Male") conds.Add("servername LIKE '%_M_%'");
				else if (gd == "Female") conds.Add("servername LIKE '%_W_%'");
				if (degree > 0)
				{
					// degree = (reqLevel+7)/8  ->  reqLevel aralığı
					int lo = (degree - 1) * 8 + 1;
					int hi = degree * 8;
					conds.Add("(tid2<>1 OR (level>=@p" + args.Count + " AND level<=@p" + (args.Count + 1) + "))");
					args.Add(lo); args.Add(hi);
				}
				if (!string.IsNullOrWhiteSpace(search))
				{
					conds.Add("(name LIKE @p" + args.Count + " OR servername LIKE @p" + args.Count + ")");
					args.Add("%" + search.Trim() + "%");
				}
				// İsimsiz kayıtlar listeyi ID yığınına çevirir, her zaman dışlanır.
				conds.Add("(name IS NOT NULL AND TRIM(name) <> '')");
			string sql = "SELECT id, servername, name, tid2, tid3, tid4, level FROM items";
			if (conds.Count > 0)
				sql += " WHERE " + string.Join(" AND ", conds);
			sql += " ORDER BY name LIMIT 500";
				return Query(sql, args.ToArray());
			}
			catch { return new List<NameValueCollection>(); }
		}

		/// <summary>
		/// Blues sekmesi için benzersiz mavi özellik listesi (servername + görünen ad).
		/// </summary>
		public static List<NameValueCollection> QueryBlueOptions()
		{
			if (!IsDbReady()) return new List<NameValueCollection>();
			try
			{
				return Query("SELECT MIN(id) AS id, servername, MAX(name) AS name FROM magicoptions GROUP BY servername ORDER BY name, servername");
			}
			catch { return new List<NameValueCollection>(); }
		}
		/// <summary>
		/// Get skill by id, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetSkillData(uint id)
		{
			return GetCached("skill:" + id, () => {
				List<NameValueCollection> result = Query("SELECT * FROM skills WHERE id=@p0", id);
				if (result.Count > 0)
					return result[0];
				return null;
			});
		}
		/// <summary>
		/// Get skill by servername, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetSkillData(string servername)
		{
			List<NameValueCollection> result = Query("SELECT * FROM skills WHERE servername=@p0", servername);
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get mastery by id, using the current database loaded.
		/// </summary>
		public static NameValueCollection GetMastery(uint id)
		{
			List<NameValueCollection> result = Query("SELECT * FROM masteries WHERE id=@p0", id);
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Get region name by id, using the current database loaded.
		/// </summary>
		public static string GetRegion(ushort id)
		{
			List<NameValueCollection> result = Query("SELECT name FROM regions WHERE id=@p0 LIMIT 1", id);
			if (result.Count > 0)
				return result[0]["name"];
			return "";
		}
		/// <summary>
		/// Gets the UIIT text already formated with params provided. returns an empty string if the servername is not found.
		/// </summary>
		public static string GetUIFormat(string servername, params object[] args)
		{
			List<NameValueCollection> result = Query("SELECT text FROM textuisystem WHERE servername=@p0", servername);
			if (result.Count > 0)
				return string.Format(result[0]["text"], args);
			return "";
		}
		/// <summary>
		/// Get's an item object from the shop at slot specified.
		/// </summary>
		public static SRItem GetItemFromShop(string npc_servername, byte tabNumber, byte tabSlot)
		{
			List<NameValueCollection> result = Query("SELECT * FROM shops WHERE model_servername=@p0 AND tab=@p1 AND slot=@p2", npc_servername, tabNumber, tabSlot);
			if (result.Count > 0)
			{
				SRItem item = SRItem.Create(result[0]["item_servername"],new SRRentable(0));
				if (item.isEquipable())
				{
					SREquipable equip = (SREquipable)item;
					byte plus; if (!byte.TryParse(result[0]["plus"], out plus)) plus = 0;
					equip.Plus = plus;
					uint dura; if (!uint.TryParse(result[0]["durability"], out dura)) dura = 0;
					equip.Durability = dura;
					// Add magic options
					if (result[0]["magic_params"] != "")
					{
						string[] mParams = result[0]["magic_params"].Split('|');
						xList<SRMagicOption> magicOptions = new xList<SRMagicOption>(mParams.Length);
						for (byte j = 0; j < mParams.Length; j++)
						{
							ulong param = ulong.Parse(mParams[j]);
							magicOptions[j] = new SRMagicOption((uint)(param & uint.MaxValue));
							magicOptions[j].Value = (uint)(param >> 32);
						}
						equip.MagicOptions = magicOptions;
					}
					else
					{
						equip.MagicOptions = new xList<SRMagicOption>();
          }
				}
				return item;
			}
			return null;
		}
		public static NameValueCollection GetQuestData(uint id)
		{
			if (!IsQuestCatalogAvailable()) return null;
			return GetCached("quest:" + id, () => {
				try
				{
					List<NameValueCollection> result = Query("SELECT * FROM quests WHERE id=@p0", id);
					return result.Count > 0 ? result[0] : null;
				}
				catch { return null; }
			});
		}

		public static List<NameValueCollection> QueryQuests(string search, int maximumLevel = 0)
		{
			if (!IsQuestCatalogAvailable()) return new List<NameValueCollection>();
			try
			{
				List<string> conditions = new List<string>();
				List<object> args = new List<object>();
				if (!string.IsNullOrWhiteSpace(search))
				{
					conditions.Add("(name LIKE @p0 OR servername LIKE @p0 OR CAST(id AS TEXT) LIKE @p0)");
					args.Add("%" + search.Trim() + "%");
				}
				if (maximumLevel > 0)
				{
					conditions.Add("level<=@p" + args.Count);
					args.Add(maximumLevel);
				}
				// Liste görünümü yalnız id/servername/name/level/notice_npc_text kullanır.
				// SELECT * büyük mission/reward metinleriyle 3000 satırda onlarca MB tutardı;
				// dar sütun + 500 satır sınırı katalog RAM'ini ~10x düşürür. Detaylar
				// GetQuestData(id) ile tek satır olarak okunur. Eski kataloglarla uyum
				// için notice sütunları yoksa dar sorgu başarısız olur, o zaman
				// geriye dönük daraltılmamış sorguya düş.
				string sql = "SELECT id, servername, name, level, notice_npc_text, notice_npc FROM quests";
				if (conditions.Count > 0) sql += " WHERE " + string.Join(" AND ", conditions);
				sql += " ORDER BY level,name LIMIT 500";
				try { return Query(sql, args.ToArray()); }
				catch
				{
					string fallback = "SELECT * FROM quests";
					if (conditions.Count > 0) fallback += " WHERE " + string.Join(" AND ", conditions);
					fallback += " ORDER BY level,name LIMIT 500";
					return Query(fallback, args.ToArray());
				}
			}
			catch { return new List<NameValueCollection>(); }
		}

		public static bool IsQuestCatalogAvailable()
		{
			lock (DatabaseSync) return Database != null && QuestCatalogAvailable;
		}
		public static List<NameValueCollection> GetQuestNpcPositions(uint questId, uint modelId, string serverName)
        {
            if (!IsDbReady()) return new List<NameValueCollection>();
            try
            {
                string sql = "SELECT p.*,m.servername,m.name FROM npc_positions p JOIN models m ON m.id=p.model_id ";
                if (modelId != 0) return Query(sql + "WHERE m.id=@p0", modelId);
                if (!string.IsNullOrWhiteSpace(serverName)) return Query(sql + "WHERE m.servername=@p0", serverName);
                NameValueCollection quest = GetQuestData(questId);
                string notice = quest?["notice_npc_text"];
                string reference = quest?["notice_npc"];
                List<NameValueCollection> matches = new List<NameValueCollection>();
                foreach (NameValueCollection row in Query(sql + "WHERE m.tid2=2 AND m.tid3=2"))
                    if (QuestNpcCatalogPolicy.MatchesQuestNpc(quest?["servername"], notice, row["servername"], row["name"])
                        || QuestNpcCatalogPolicy.MatchesNotice(reference, row["servername"], row["name"])) matches.Add(row);
                // Multiple placements of one model are valid; different identities are ambiguous.
                HashSet<string> ids = new HashSet<string>();
                foreach (NameValueCollection match in matches) ids.Add(match["model_id"]);
                return ids.Count == 1 ? matches : new List<NameValueCollection>();
            }
            catch { return new List<NameValueCollection>(); } // Older catalogs need rebuilding.
        }

		public static List<NameValueCollection> GetQuestRewardItems(uint questId)
        {
            List<NameValueCollection> rewards;
            TryGetQuestRewardItems(questId, out rewards);
            return rewards;
        }

        public static bool TryGetQuestRewardItems(uint questId, out List<NameValueCollection> rewards)
        {
            rewards = new List<NameValueCollection>();
            if (!IsDbReady() || questId == 0) return false;
            try
            {
                rewards = Query("SELECT q.quest_id,q.reward_type,q.item_servername,q.amount,i.id AS reward_id,i.name,i.tid2,i.tid3,i.tid4 "
                    + "FROM quest_reward_items q LEFT JOIN items i ON i.servername=q.item_servername WHERE q.quest_id=@p0 ORDER BY q.item_servername", questId);
                return true;
            }
            catch { return false; }
        }
		/// <summary>
		/// NPC mağazasındaki bütün paketleri slot sırasıyla döndürür. Eski vSRO
		/// trade komutu buradan yalnız TID2=3/TID3=8 specialty goods seçer.
		/// </summary>
		public static List<NameValueCollection> GetShopItems(string npcServerName)
		{
			if (string.IsNullOrWhiteSpace(npcServerName))
				return new List<NameValueCollection>();
			try
			{
				return Query("SELECT tab, slot, item_servername FROM shops WHERE model_servername=@p0 ORDER BY tab, slot", npcServerName);
			}
			catch
			{
				return new List<NameValueCollection>();
			}
		}
		/// <summary>
		/// Gets the last skill ID from the skill updated, returns 0 if none is found.
		/// </summary>
		public static uint GetLastSkillID(SRSkill skill)
		{
			List<NameValueCollection> result = Query("SELECT id FROM skills WHERE group_id=@p0 AND level<@p1 ORDER BY level DESC LIMIT 1", skill.GroupID, skill.Level);
			if (result.Count > 0)
				return uint.Parse(result[0]["id"]);
			return 0;
		}
		/// <summary>
		/// Gets the next skill ID from the skill to update, returns 0 if none is found.
		/// </summary>
		public static uint GetNextSkillID(SRSkill skill)
		{
			List<NameValueCollection> result = Query("SELECT id FROM skills WHERE group_id=@p0 AND level>@p1 ORDER BY level LIMIT 1", skill.GroupID, skill.Level);
			if (result.Count > 0)
				return uint.Parse(result[0]["id"]);
			return 0;
		}
		/// <summary>
		/// Gets the teleport link data. Return null if link is not found.
		/// </summary>
		public static NameValueCollection GetTeleportLink(string teleportName)
		{
			string safe = (teleportName ?? "").Replace("%", "").Replace("_", "");
			List<NameValueCollection> result = Query("SELECT * FROM teleportlinks WHERE name LIKE @p0 LIMIT 1", "%" + safe + "%");
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Gets the teleport link data. Return null if link is not found.
		/// </summary>
		public static NameValueCollection GetTeleportLink(string sourceTeleportName, string destinationTeleportName)
		{
			string s = (sourceTeleportName ?? "").Replace("%", "").Replace("_", "");
			string d = (destinationTeleportName ?? "").Replace("%", "").Replace("_", "");
			List<NameValueCollection> result = Query("SELECT * FROM teleportlinks WHERE name LIKE @p0 AND destination LIKE @p1 LIMIT 1", "%" + s + "%", "%" + d + "%");
			if (result.Count > 0)
				return result[0];
			return null;
		}
		/// <summary>
		/// Gets all links from the teleport specified.
		/// </summary>
		public static List<NameValueCollection> GetTeleportLinks(uint sourceTeleportID)
		{
			return Query("SELECT * FROM teleportlinks WHERE sourceid=@p0", sourceTeleportID);
    }
		/// <summary>
		/// Gets all destinations offered by the gate with the given MODEL id.
		/// (teleportlinks.id holds the gate model ID; sourceid holds the
		/// TeleportData row index, so model-ID lookups must use this method.)
		/// </summary>
		public static List<NameValueCollection> GetTeleportLinksByModel(uint gateModelID)
		{
			return Query("SELECT * FROM teleportlinks WHERE id=@p0", gateModelID);
    }
		/// <summary>
		/// Gets the teleport destination ID. Return 0 if none is found.
		/// </summary>
		public static uint GetTeleportLinkDestinationID(uint sourceTeleportID, uint destinationTeleportID)
		{
			List<NameValueCollection> result = Query("SELECT t1.destinationid FROM teleportlinks AS t1 JOIN teleportlinks AS t2 WHERE t1.destination=t2.name AND t2.destination=t1.name AND t1.id=@p0 AND t2.id=@p1 LIMIT 1", sourceTeleportID, destinationTeleportID);
			if (result.Count > 0)
			{
				return uint.Parse(result[0]["destinationid"]);
			}
			return 0;
		}
		/// <summary>
		/// Gets the next skill ID from the skill to update, returns 0 if none is found.
		/// </summary>
		public static uint GetCommonAttack(SRTypes.Weapon type)
		{
			List<NameValueCollection> result = Query("SELECT id FROM skills WHERE (weapon_first = @p0 or weapon_second = @p0) and group_name LIKE '%_BASE'", ((byte)type));
			if (result.Count > 0)
			{
				return uint.Parse(result[0]["id"]);
			}
			return 0;
		}
		#endregion
	}
}
