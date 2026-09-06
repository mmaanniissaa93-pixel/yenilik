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
				Database = new SQLDatabase(dbPath);
				bool connected = Database.Connect();
				if (connected)
				{
					try
					{
						var tables = Database.GetResultFromQuery("SELECT name FROM sqlite_master WHERE type='table' AND name IN ('items','skills','models')");
						if (tables == null || tables.Count < 3)
						{
							Database.Close();
							Database = null;
							return false;
						}
					}
					catch { Database = null; return false; }
					DataManager.SilkroadName = SilkroadName;
				}
				return connected;
			}
			return false;
		}
		public static void DisconnectDatabase()
		{
			if (Database != null)
				Database.Close();
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
			return Database != null;
		}
		private static List<NameValueCollection> Query(string sql)
		{
			if (Database == null) return new List<NameValueCollection>();
			return Database.GetResultFromQuery(sql);
		}
		private static List<NameValueCollection> Query(string sql, params object[] args)
		{
			if (Database == null) return new List<NameValueCollection>();
			return Database.GetResultFromQuery(sql, args);
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
