using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
	public class Script
	{
		private List<string> m_lines;
		private readonly ManualResetEvent m_stopSignal = new ManualResetEvent(false);
		public string FileName { get; private set; }
		public bool Running { get; set; }

		public Script()
		{
			this.FileName = "";
			this.m_lines = new List<string>();
		}
		public Script(string path)
		{
			this.FileName = Path.GetFileName(path);
			string[] lines = File.ReadAllLines(path);
			this.m_lines = new List<string>(lines.Length);
			foreach (string line in lines)
				this.Add(line);
		}
		/// <summary>
		/// Town script klasörleri: çalışma dizini + exe dizini, "Town"/"Towns", *.txt + *.rbs.
		/// </summary>
		private static IEnumerable<string> GetTownScriptDirs()
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var candidates = new List<string>
			{
				"Town",
				"Towns",
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Town"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Towns"),
				Path.Combine(Environment.CurrentDirectory, "Town"),
				Path.Combine(Environment.CurrentDirectory, "Towns"),
			};
			foreach (string d in candidates)
			{
				string full = d;
				try { full = Path.GetFullPath(d); } catch { }
				if (seen.Add(full))
					yield return d;
			}
		}
		/// <summary>
		/// Bölge ID'sine göre town scripti bulur (örn. 25000.rbs). Karakter şehirde
		/// doğduğunda script noktalarına 50m'den uzak olsa bile town döngüsüne girer.
		/// </summary>
		public static Script GetTownScriptForRegion(ushort region)
		{
			string[] names = new string[] { region + ".rbs", region + ".txt" };
			foreach (string dir in GetTownScriptDirs())
			{
				if (!Directory.Exists(dir))
					continue;
				foreach (string name in names)
				{
					string full = null;
					try { full = Path.Combine(dir, name); } catch { continue; }
					if (File.Exists(full))
					{
						try { return new Script(full); } catch { }
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Tanı için: tüm town scriptlerindeki en yakın MOVE noktasına uzaklık.
		/// -1 = hiç script/MOVE yok.
		/// </summary>
		public static double GetNearestTownMoveDistance(SRCoord position)
		{
			double best = double.MaxValue;
			bool any = false;
			foreach (string dir in GetTownScriptDirs())
			{
				if (!Directory.Exists(dir))
					continue;
				string[] files = new string[0];
				try
				{
					var all = new List<string>();
					try { all.AddRange(Directory.GetFiles(dir, "*.rbs")); } catch { }
					try { all.AddRange(Directory.GetFiles(dir, "*.txt")); } catch { }
					files = all.ToArray();
				}
				catch { continue; }
				foreach (string file in files)
				{
					Script town;
					try { town = new Script(file); } catch { continue; }
					for (int i = 0; i < town.m_lines.Count; i++)
					{
						string line = town.m_lines[i];
						if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
							continue;
						string[] args = Tokenize(line);
						if (args.Length == 0 || !args[0].Equals("MOVE", StringComparison.OrdinalIgnoreCase))
							continue;
						SRCoord coord = ParseMove(args);
						if (coord == null)
							continue;
						any = true;
						double d = coord.DistanceTo(position);
						if (d < best)
							best = d;
					}
				}
			}
			return any ? best : -1;
		}
		/// <summary>
		/// Gets the near script to be used as town loop. Returns null if cannot be found.
		/// </summary>
		public static Script GetNearestTownScript(SRCoord position, int maxRange)
		{
			foreach (string dir in GetTownScriptDirs())
			{
				if (!Directory.Exists(dir))
					continue;
				string[] files = new string[0];
				try
				{
					var all = new List<string>();
					try { all.AddRange(Directory.GetFiles(dir, "*.rbs")); } catch { }
					try { all.AddRange(Directory.GetFiles(dir, "*.txt")); } catch { }
					files = all.ToArray();
				}
				catch { continue; }
				foreach (string file in files)
				{
					try
					{
						Script town = new Script(file);
						if (town.GetNearMovement(position, maxRange) != -1)
							return town;
					}
					catch { }
				}
			}
			return null;
		}
		public void Add(string line)
		{
			// Keep intact if starts as comment
			if (line.StartsWith("//"))
				m_lines.Add(line);
			else
				m_lines.Add(line.ToUpper());
		}
		public void RemoveAt(int index)
		{
			m_lines.RemoveAt(index);
		}
		public void Save(string path)
		{
			File.WriteAllLines(path,this.m_lines.ToArray());
		}
		/// <summary>
		/// Satırı virgül VE boşlukla tokenize eder (hem "move,1,2" hem "move 1 2" çalışır).
		/// </summary>
		private static string[] Tokenize(string line)
		{
			return line.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		}
		/// <summary>
		/// MOVE satırını çözer. Desteklenen formatlar:
		/// "move,PosX,PosY" / "move PosX PosY",
		/// "move,Region,X,Y,Z" (virgüllü eski format),
		/// "move X Y Z secX secY" (.rbs formatı: Region=(secY&lt;&lt;8)|secX).
		/// </summary>
		private static SRCoord ParseMove(string[] args)
		{
			if (args == null || args.Length < 3 || !args[0].Equals("MOVE", StringComparison.OrdinalIgnoreCase))
				return null;
			try
			{
				if (args.Length == 3)
				{
					int x, y;
					if (int.TryParse(args[1], out x) && int.TryParse(args[2], out y))
						return new SRCoord(x, y);
				}
				else if (args.Length == 5)
				{
					// "move,Region,X,Y,Z" eski format
					ushort r;
					int x, y, z;
					if (ushort.TryParse(args[1], out r)
						&& int.TryParse(args[2], out x)
						&& int.TryParse(args[3], out y)
						&& int.TryParse(args[4], out z))
						return new SRCoord(r, x, z, y);
				}
				else if (args.Length == 6)
				{
					// .rbs formatı: "move X Y Z secX secY"
					int x, y, z, secX, secY;
					if (int.TryParse(args[1], out x)
						&& int.TryParse(args[2], out y)
						&& int.TryParse(args[3], out z)
						&& int.TryParse(args[4], out secX)
						&& int.TryParse(args[5], out secY))
					{
						ushort region = (ushort)(((secY & 0xFF) << 8) | (secX & 0xFF));
						return new SRCoord(region, x, z, y);
					}
				}
			}
			catch { }
			return null;
		}
		/// <summary>
		/// Check if is near to the script. Returns the near index or (-1).
		/// </summary>
		public int GetNearMovement(SRCoord position, int maxRange)
		{
			for (int i = 0; i < m_lines.Count; i++)
			{
				string line = m_lines[i];
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
					continue;
				string[] args = Tokenize(line);
				if (args.Length == 0 || !args[0].Equals("MOVE", StringComparison.OrdinalIgnoreCase))
					continue;
				SRCoord coord = ParseMove(args);
				if (coord != null && coord.DistanceTo(position) <= maxRange)
					return i;
			}
			return -1;
		}
		/// <summary>
		/// Start running script.
		/// </summary>
		public void Run(int startIndex = 0)
		{
			Bot b = Bot.Get;
			m_stopSignal.Reset();
			Running = true;
			// Parsing script
			for (int j = startIndex; j < m_lines.Count && Running && b.isBotting; j++)
				ExecuteLine(j);
			Running = false;
		}
		/// <summary>
		/// Scripti sondan başa doğru koşar (Alana Dönüş &gt; Reverse route).
		/// Yürüyüş scripti alan-&gt;şehir yönünde kaydedildiyse dönüş yolculuğu
		/// bu yönde şehir-&gt;alan olur. startIndex -1 ise sondan başlar.
		/// </summary>
		public void RunReversed(int startIndex = -1)
		{
			Bot b = Bot.Get;
			if (startIndex < 0 || startIndex >= m_lines.Count)
				startIndex = m_lines.Count - 1;
			m_stopSignal.Reset();
			Running = true;
			for (int j = startIndex; j >= 0 && Running && b.isBotting; j--)
				ExecuteLine(j);
			Running = false;
		}
		/// <summary>
		/// Tek script satırını çalıştırır (düz ve ters koşu ortak).
		/// </summary>
		private void ExecuteLine(int j)
		{
			Window w = Window.Get;
			Bot b = Bot.Get;
			if (m_lines[j].StartsWith("//") || string.IsNullOrWhiteSpace(m_lines[j]))
				return;
			ScriptCommandInvocation invocation;
			string parseError;
			if (!ScriptCommandCatalog.TryParse(m_lines[j], out invocation, out parseError))
			{
				w.LogProcess($"Script [{j + 1}]: {parseError}", Window.ProcessState.Warning);
				return;
			}
			string[] command = invocation.ToLegacyTokens();

			// Execute the command
			string cmd = invocation.Command.ToLowerInvariant();
			switch (cmd)
			{
				case "move":
					SRCoord position = ParseMove(command);
					if (position != null) {
						w.LogProcess($"Script step [{j + 1}/{m_lines.Count}]");
						if (!b.WaitMovement(position, 10))
						{
							// Tek noktaya ulaşılamadı diye TÜM botu durdurma:
							// tezgâh/duvar arkası gibi noktalar atlanır, NPC
							// adımları (store/buy/repair) yine de çalışır.
							w.Log($"Script step [{j + 1}] atlandı (ulaşılamadı), devam ediliyor.");
						}
					}
					break;
				case "store":
					ExecuteStoreStep(command, w, b);
					break;
				case "buy":
					ExecuteBuyStep(command, w, b);
					break;
				case "repair":
					ExecuteRepairStep(command, w, b);
					break;
				case "wait":
					ExecuteWait(invocation, w);
					break;
				case "cast":
					ExecuteCast(invocation, w, b);
					break;
				case "use":
					ExecuteUse(invocation, w, b);
					break;
				case "teleport":
					ExecuteTeleport(invocation, w, b);
					break;
				case "doblacksmith":
				case "doherbalist":
				case "dostable":
				case "dostorage":
				case "dostoragestore":
				case "dostoragetake":
				case "doguildstorage":
				case "doguildstoragestore":
				case "doguildstoragetake":
				case "dogrocerytrader":
				case "doprotectortrader":
				case "dojupiter":
					ExecuteTownService(cmd, w, b);
					break;
				case "recall":
					ExecuteRecall(w);
					break;
				case "stop":
					w.LogProcess("Script: bot durduruluyor...");
					Stop();
					b.Stop();
					break;
				case "disconnect":
					w.LogProcess("Script: bağlantı kapatılıyor...");
					Stop();
					b.Stop();
					if (b.Proxy != null)
						b.Proxy.Stop();
					break;
			}
		}

		public void Stop()
		{
			Running = false;
			m_stopSignal.Set();
		}

		private bool WaitInterruptible(int milliseconds)
		{
			if (milliseconds <= 0)
				return Running;
			int waited = 0;
			while (waited < milliseconds && Running)
			{
				int slice = Math.Min(100, milliseconds - waited);
				if (m_stopSignal.WaitOne(slice))
					return false;
				waited += slice;
			}
			return Running;
		}

		private void ExecuteWait(ScriptCommandInvocation command, Window w)
		{
			int milliseconds;
			if (!int.TryParse(command.Arguments[0], out milliseconds) || milliseconds < 0 || milliseconds > 3600000)
			{
				w.LogProcess("WAIT: süre 0-3600000 ms arasında olmalı.", Window.ProcessState.Warning);
				return;
			}
			w.LogProcess($"Script waiting ({milliseconds}ms)...");
			WaitInterruptible(milliseconds);
		}

		private void ExecuteCast(ScriptCommandInvocation command, Window w, Bot b)
		{
			if (InfoManager.Character == null || InfoManager.Character.Skills == null)
			{
				w.LogProcess("CAST: karakter skill listesi hazır değil.", Window.ProcessState.Warning);
				return;
			}
			string query = command.Arguments[0];
			uint skillID;
			uint.TryParse(query, out skillID);
			SRSkill skill = InfoManager.Character.Skills.Find(candidate => candidate != null
				&& (candidate.ID == skillID
					|| string.Equals(candidate.Name, query, StringComparison.OrdinalIgnoreCase)
					|| string.Equals(candidate.ServerName, query, StringComparison.OrdinalIgnoreCase)));
			if (skill == null)
				skill = InfoManager.Character.Skills.Find(candidate => candidate != null
					&& ((candidate.Name != null && candidate.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
						|| (candidate.ServerName != null && candidate.ServerName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)));
			if (skill == null)
			{
				w.LogProcess("CAST: skill bulunamadı [" + query + "].", Window.ProcessState.Warning);
				return;
			}
			if (!skill.isCastingEnabled)
			{
				w.LogProcess("CAST: skill bekleme süresinde [" + skill.Name + "].", Window.ProcessState.Warning);
				return;
			}
			b.CheckWeaponSwitch(skill);
			uint target = skill.isTargetRequired ? InfoManager.SelectedEntityUniqueID : 0;
			if (skill.isTargetRequired && target == 0)
			{
				w.LogProcess("CAST: hedef isteyen skill için seçili hedef yok [" + skill.Name + "].", Window.ProcessState.Warning);
				return;
			}
			w.LogProcess("Script CAST: " + skill.Name);
			PacketBuilder.CastSkill(skill.ID, target);
			WaitInterruptible(Math.Max(250, skill.CastingTime));
		}

		private void ExecuteUse(ScriptCommandInvocation command, Window w, Bot b)
		{
			if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
			{
				w.LogProcess("USE: envanter hazır değil.", Window.ProcessState.Warning);
				return;
			}
			string query = command.Arguments[0];
			int slot;
			if (!int.TryParse(query, out slot))
			{
				slot = InfoManager.Character.Inventory.FindIndex(item => item != null
					&& (string.Equals(item.Name, query, StringComparison.OrdinalIgnoreCase)
						|| string.Equals(item.ServerName, query, StringComparison.OrdinalIgnoreCase)), 13);
				if (slot == -1)
					slot = InfoManager.Character.Inventory.FindIndex(item => item != null
						&& ((item.Name != null && item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
							|| (item.ServerName != null && item.ServerName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)), 13);
			}
			if (slot < 13 || slot >= InfoManager.Character.Inventory.Capacity || !b.UseItem((byte)slot))
			{
				w.LogProcess("USE: kullanılabilir item bulunamadı [" + query + "].", Window.ProcessState.Warning);
				return;
			}
			w.LogProcess("Script USE: slot " + slot);
			WaitInterruptible(500);
		}

		private void ExecuteTeleport(ScriptCommandInvocation command, Window w, Bot b)
		{
			string source = command.Arguments[0];
			string destination = command.Arguments[1];
			uint sourceID;
			uint destinationModelID;
			uint destinationID = 0;
			SRTeleport teleport = null;
			if (uint.TryParse(source, out sourceID) && uint.TryParse(destination, out destinationModelID))
			{
				destinationID = DataManager.GetTeleportLinkDestinationID(sourceID, destinationModelID);
				teleport = InfoManager.TeleportAndBuildings.Find(item => item != null && item.ID == sourceID);
			}
			else
			{
				NameValueCollection link = DataManager.GetTeleportLink(source, destination);
				if (link != null && uint.TryParse(link["id"], out sourceID))
				{
					uint.TryParse(link["destinationid"], out destinationID);
					teleport = InfoManager.TeleportAndBuildings.Find(item => item != null && item.ID == sourceID);
				}
			}
			if (destinationID == 0 || teleport == null)
			{
				w.LogProcess("TELEPORT: yakın bağlantı bulunamadı [" + source + " -> " + destination + "].", Window.ProcessState.Warning);
				return;
			}
			w.LogProcess("Script TELEPORT: " + source + " -> " + destination);
			if (b.Proxy.ClientlessMode)
			{
				if (!InfoManager.isEntityNear(teleport.UniqueID))
				{
					w.LogProcess("TELEPORT: kaynak kapı yakında değil.", Window.ProcessState.Warning);
					return;
				}
			}
			else if (!b.WaitSelectEntity(teleport.UniqueID, 8, 250, "Selecting teleport " + teleport.TeleportName + "..."))
			{
				w.LogProcess("TELEPORT: kaynak kapı seçilemedi.", Window.ProcessState.Warning);
				return;
			}
			PacketBuilder.UseTeleport(teleport.UniqueID, destinationID);
			for (int i = 0; i < 100 && Running && !InfoManager.inTeleport; i++)
				WaitInterruptible(100);
			for (int i = 0; i < 300 && Running && InfoManager.inTeleport; i++)
				WaitInterruptible(100);
		}

		private void ExecuteRecall(Window w)
		{
			SRCoService pickPet = InfoManager.MyPets.Find(pet => pet != null && pet.isPickPet());
			if (pickPet == null)
			{
				w.LogProcess("RECALL: aktif toplama peti yok.", Window.ProcessState.Warning);
				return;
			}
			w.LogProcess("Script RECALL: " + pickPet.Name);
			PacketBuilder.UnsummonPet(pickPet.UniqueID);
			WaitInterruptible(500);
		}

		private void ExecuteTownService(string command, Window w, Bot b)
		{
			if (command == "doguildstorage" || command == "doguildstoragestore" || command == "doguildstoragetake")
			{
				ExecuteGuildStorageStep(command != "doguildstoragetake", command == "doguildstorage",
					command == "doguildstoragetake", w, b);
				return;
			}
			if (command == "dostoragetake")
			{
				ExecuteStorageTakeStep(w, b);
				return;
			}
			if (command == "dostorage" || command == "dostoragestore")
			{
				ExecuteStoreStep(new[] { "store", "WAREHOUSE" }, w, b, command == "dostorage");
				return;
			}

			string npcCode;
			switch (command)
			{
				case "doblacksmith": npcCode = "BLACKSMITH"; break;
				case "doherbalist": npcCode = "HERBALIST"; break;
				case "dostable": npcCode = "STABLE"; break;
				case "dogrocerytrader": npcCode = "GROCERY"; break;
				case "doprotectortrader": npcCode = "PROTECTOR"; break;
				case "dojupiter": npcCode = "JUPITER"; break;
				default: return;
			}

			bool nameMatched;
			SREntity npc = FindTownNpc(npcCode, out nameMatched);
			ScriptCommandDefinition actionDefinition = ScriptCommandCatalog.Find(command);
			string action = actionDefinition == null ? command : actionDefinition.Name;
			if (!PrepareNpcInteraction(npc, npcCode, nameMatched, action, w, b))
				return;
			if (!OpenNpcDialog(npc, action, w, b))
				return;

			try
			{
				bool repair = command == "doblacksmith" || command == "dojupiter";
				bool buyPotions = command == "doherbalist" || command == "dojupiter";
				bool buyAmmo = command == "dogrocerytrader";

				string dismantleLocation = command == "doherbalist" ? "Herbalist"
					: command == "dogrocerytrader" ? "Grocery"
					: (command == "doblacksmith" || command == "doprotectortrader" || command == "dojupiter") ? "Blacksmith"
					: null;
				if (dismantleLocation != null)
					DismantleCheck(dismantleLocation, w);

				if (repair && (w.Town_cbxRepair == null || w.Town_cbxRepair.Checked))
				{
					w.LogProcess(action + ": repairing equipment...");
					PacketBuilder.RepairAllEquipments(npc.UniqueID);
					if (!WaitInterruptible(800)) return;
				}

				if (w.Town_cbxSellTrash == null || w.Town_cbxSellTrash.Checked)
				{
					b.ExecuteSellTrash();
					if (!WaitInterruptible(400)) return;
				}

				if (buyPotions)
					b.ExecuteAutoBuyPotions(npc);
				if (buyAmmo)
					b.ExecuteAutoBuyAmmoFromOpenNpc(npc);
				if (command == "dostable")
					w.LogProcess("DoStable: stable alış profili tanımlı değil; güvenli satış adımı tamamlandı.");
			}
			catch (Exception ex)
			{
				w.LogProcess(action + " hatası: " + ex.Message, Window.ProcessState.Warning);
			}
			finally
			{
				try
				{
					if (npc != null && IsConnectionAlive(b))
						PacketBuilder.CloseNPC(npc.UniqueID);
					WaitInterruptible(300);
				}
				catch { }
			}
		}

		private void ExecuteStorageTakeStep(Window w, Bot b)
		{
			bool nameMatched;
			SREntity npc = FindTownNpc("WAREHOUSE", out nameMatched);
			if (!PrepareNpcInteraction(npc, "WAREHOUSE", nameMatched, "DoStorageTake", w, b))
				return;
			try
			{
				DateTime openTime = DateTime.Now;
				if (InfoManager.isStorageLoaded)
					PacketBuilder.TalkNPC(npc.UniqueID, 3);
				else
					PacketBuilder.OpenStorage(npc.UniqueID);
				bool opened = false;
				for (int i = 0; i < 40 && IsConnectionAlive(b) && Running; i++)
				{
					if (InfoManager.LastStorageInfoTime > openTime || InfoManager.inStorage)
					{
						opened = true;
						break;
					}
					if (!WaitInterruptible(100)) return;
				}
				if (!opened || InfoManager.Character.Storage == null)
				{
					w.LogProcess("DoStorageTake: güncel storage verisi alınamadı.", Window.ProcessState.Warning);
					return;
				}
				b.ExecuteStorageTake(npc.UniqueID);
			}
			catch (Exception ex)
			{
				w.LogProcess("DoStorageTake hatası: " + ex.Message, Window.ProcessState.Warning);
			}
			finally
			{
				try
				{
					if (IsConnectionAlive(b)) PacketBuilder.CloseNPC(npc.UniqueID);
					WaitInterruptible(300);
				}
				catch { }
			}
		}

		private void ExecuteGuildStorageStep(bool storeItems, bool includeGold, bool takeItems, Window w, Bot b)
		{
			if (!InfoManager.inGuild || InfoManager.Guild == null)
			{
				w.LogProcess("DoGuildStorage: karakter bir guild üyesi değil.", Window.ProcessState.Warning);
				return;
			}
			try
			{
				var me = InfoManager.Guild.Members.Find(member => member != null
					&& string.Equals(member.Name, InfoManager.Character.Name, StringComparison.OrdinalIgnoreCase));
				if (me != null && (uint)me.PermissionsFlags != uint.MaxValue
					&& (((uint)me.PermissionsFlags & 8u) == 0))
				{
					w.LogProcess("DoGuildStorage: karakterin guild storage yetkisi yok.", Window.ProcessState.Warning);
					return;
				}
			}
			catch { }

			bool nameMatched;
			SREntity npc = FindTownNpc("GUILD_STORAGE", out nameMatched);
			if (!PrepareNpcInteraction(npc, "GUILD_STORAGE", nameMatched, "DoGuildStorage", w, b))
				return;

			bool locked = false;
			try
			{
				while (InfoManager.MonitorNpcTalk.WaitOne(0)) { }
				PacketBuilder.TalkNPC(npc.UniqueID, 15);
				bool dialogOpened = false;
				for (int i = 0; i < 30 && IsConnectionAlive(b) && Running; i++)
				{
					if (InfoManager.MonitorNpcTalk.WaitOne(100)
						&& InfoManager.LastNpcTalkEntityUniqueID == npc.UniqueID
						&& InfoManager.LastNpcTalkID == 15)
					{
						dialogOpened = true;
						break;
					}
				}
				if (!dialogOpened)
				{
					w.LogProcess("DoGuildStorage: NPC guild-storage oturumu açılmadı.", Window.ProcessState.Warning);
					return;
				}

				while (InfoManager.MonitorGuildStorageResponse.WaitOne(0)) { }
				PacketBuilder.LockGuildStorage(npc.UniqueID);
				if (!InfoManager.MonitorGuildStorageResponse.WaitOne(2500)
					|| InfoManager.LastGuildStorageResult != 1)
				{
					w.LogProcess("DoGuildStorage: guild storage kilidi alınamadı; başka üye kullanıyor olabilir.", Window.ProcessState.Warning);
					return;
				}
				locked = true;

				DateTime previousData = InfoManager.LastGuildStorageInfoTime;
				while (InfoManager.MonitorGuildStorageData.WaitOne(0)) { }
				PacketBuilder.RefreshGuildStorage(npc.UniqueID);
				bool loaded = false;
				for (int i = 0; i < 40 && IsConnectionAlive(b) && Running; i++)
				{
					if (InfoManager.MonitorGuildStorageData.WaitOne(100)
						&& InfoManager.LastGuildStorageInfoTime > previousData
						&& InfoManager.Guild.Storage != null)
					{
						loaded = true;
						break;
					}
				}
				if (!loaded)
				{
					w.LogProcess("DoGuildStorage: güncel depo verisi gelmedi; eşya hareketi yapılmadı.", Window.ProcessState.Warning);
					return;
				}

				DismantleCheck("Guild", w);
				if (storeItems)
					b.ExecuteGuildStorageDeposit(npc.UniqueID);
				if (takeItems && Running)
					b.ExecuteGuildStorageTake(npc.UniqueID);
				if (includeGold)
					b.ExecuteGuildStorageGold();
			}
			catch (Exception ex)
			{
				w.LogProcess("DoGuildStorage hatası: " + ex.Message, Window.ProcessState.Warning);
			}
			finally
			{
				try
				{
					if (locked && IsConnectionAlive(b))
					{
						PacketBuilder.UnlockGuildStorage(npc.UniqueID);
						WaitInterruptible(250);
					}
					if (IsConnectionAlive(b))
						PacketBuilder.CloseNPC(npc.UniqueID);
					WaitInterruptible(300);
				}
				catch { }
			}
		}

		#region Town NPC steps (.rbs: store / buy / repair)

		private static string NpcCode(string[] command)
		{
			if (command != null && command.Length > 1)
				return command[1].Trim().ToUpper();
			return "";
		}

		/// <summary>
		/// NPC kodundan isim eşleşmesi + yakındaki NPC fallback ile canlı NPC'yi bulur.
		/// DÖNEN nameMatched=false ise sonuç sadece "yakındaki NPC tahminidir":
		/// OpenStorage/Repair gibi kick riski taşıyan paketler GÖNDERİLMEMELİ.
		/// (Yanlış NPC'ye storage paketi private server'da bağlantıyı kestiriyor.)
		/// </summary>
		private static SREntity FindTownNpc(string code, out bool nameMatched)
		{
			nameMatched = false;
			string[] keywords = KeywordsForCode(code);
			SRCoord myPos = null;
			try { myPos = InfoManager.Character.GetRealtimePosition(); } catch { }

			// 0. ServerName eşleşmesi (en güvenilir: .rbs kodunun kendisi server adıdır,
			//    örn. NPC_KT_WAREHOUSE. Display name server'dan server'a değişir, bu değişmez.)
			if (!string.IsNullOrEmpty(code))
			{
				SREntity byServer = InfoManager.Npcs.Find(n => n != null && n.ServerName != null
					&& n.ServerName.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0);
				if (byServer != null)
				{
					nameMatched = true;
					return byServer;
				}
				try
				{
					SREntity byServer2 = InfoManager.Entities.Find(e => e != null && e.ServerName != null
						&& e.ServerName.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0);
					if (byServer2 != null)
					{
						nameMatched = true;
						return byServer2;
					}
				}
				catch { }
			}

			// 1. İsim eşleşmesi (Npcs + Entities)
			if (keywords != null && keywords.Length > 0)
			{
				SREntity byName = InfoManager.Npcs.Find(n => n != null && n.Name != null && ContainsAny(n.Name, keywords));
				if (byName != null)
				{
					nameMatched = true;
					return byName;
				}
				try
				{
					SREntity byName2 = InfoManager.Entities.Find(e => e != null && e.Name != null && ContainsAny(e.Name, keywords));
					if (byName2 != null)
					{
						nameMatched = true;
						return byName2;
					}
				}
				catch { }
			}

			// 2. Fallback: oyuncuya en yakın NPC (15m — 35m çok genişti, komşu NPC'yi yakalıyordu)
			if (myPos != null)
			{
				SREntity best = null;
				double bestDist = 15.0;
				try
				{
					foreach (var n in InfoManager.Npcs.Snapshot())
					{
						if (n == null || n.Position == null)
							continue;
						double d = myPos.DistanceTo(n.Position);
						if (d < bestDist)
						{
							bestDist = d;
							best = n;
						}
					}
				}
				catch { }
				if (best != null)
					return best;
			}
			return null;
		}

		private static SREntity FindTownNpc(string code)
		{
			bool ignored;
			return FindTownNpc(code, out ignored);
		}

		/// <summary>
		/// NPC ile etkileşim öncesi güvenlik: ismi doğrulanmamış NPC'ye paket gönderme.
		/// Ayrıca NPC 10m'den uzaksa önce yanına yürü (server yakınlık ister).
		/// </summary>
		private static bool PrepareNpcInteraction(SREntity npc, string code, bool nameMatched, string action, Window w, Bot b)
		{
			if (!IsConnectionAlive(b))
				return false;
			if (npc == null)
			{
				w.LogProcess($"{action}: NPC bulunamadı [{code}], atlanıyor.", Window.ProcessState.Warning);
				return false;
			}
			if (!nameMatched)
			{
				w.Log($"{action}: [{npc.Name}] ismi doğrulanamadı ([{code}] bekleniyor) — kick riski yüzünden paket göndermeden atlanıyor.");
				return false;
			}
			try
			{
				SRCoord myPos = InfoManager.Character.GetRealtimePosition();
				if (myPos != null && npc.Position != null && myPos.DistanceTo(npc.Position) > 10.0)
				{
					w.LogProcess($"{action}: NPC'ye yaklaşılıyor ({myPos.DistanceTo(npc.Position):F0}m)...");
					if (!b.WaitMovement(npc.Position, 10))
					{
						w.LogProcess($"{action}: NPC'ye yaklaşılamadı.", Window.ProcessState.Warning);
						return false;
					}
				}
			}
			catch { }
			if (!b.WaitSelectEntity(npc.UniqueID, 8, 250, $"Selecting [{npc.Name}]..."))
			{
				w.LogProcess($"{action}: NPC seçilemedi [{npc.Name}].", Window.ProcessState.Warning);
				return false;
			}
			// Seçim server'da gerçekten oturdu mu?
			if (InfoManager.SelectedEntityUniqueID != npc.UniqueID)
			{
				w.LogProcess($"{action}: seçim doğrulanamadı [{npc.Name}], atlanıyor.", Window.ProcessState.Warning);
				return false;
			}
			Thread.Sleep(500);
			return IsConnectionAlive(b);
		}

		private static bool IsConnectionAlive(Bot b)
		{
			return b != null && b.isBotting && InfoManager.inGame
				&& b.Proxy != null && b.Proxy.isRunning;
		}

		/// <summary>
		/// Seçim (0x7045) tek başına dükkânı açmaz. Sunucudan başarılı
		/// 0xB046 cevabı gelmeden satış/alım paketi göndermek bazı sunucularda DC'ye yol açar.
		/// </summary>
		internal static bool OpenNpcDialog(SREntity npc, string action, Window w, Bot b)
		{
			if (npc == null || !IsConnectionAlive(b))
				return false;

			while (InfoManager.MonitorNpcTalk.WaitOne(0)) { }
			w.LogProcess($"{action}: [{npc.Name}] dükkânı açılıyor...");
			PacketBuilder.TalkNPC(npc.UniqueID, 3);
			for (int i = 0; i < 30 && IsConnectionAlive(b); i++)
			{
				if (InfoManager.MonitorNpcTalk.WaitOne(100)
					&& InfoManager.LastNpcTalkEntityUniqueID == npc.UniqueID
					&& InfoManager.LastNpcTalkID == 3)
					return true;
			}

			w.LogProcess($"{action}: [{npc.Name}] dükkânı açılmadı; güvenlik için işlem atlandı.", Window.ProcessState.Warning);
			return false;
		}

		private static bool ContainsAny(string name, string[] keywords)
		{
			foreach (string k in keywords)
			{
				if (name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			}
			return false;
		}

		private static string[] KeywordsForCode(string code)
		{
			if (string.IsNullOrEmpty(code))
				return new string[0];
			if (code.Contains("GUILD_STORAGE"))
				return new string[] { "GUILD_STORAGE", "GUILD WAREHOUSE", "GENARAL_SP", "_GUILD" };
			if (code.Contains("WAREHOUSE") || code.Contains("STORAGE"))
				return new string[] { "WAREHOUSE", "STORAGE" };
			if (code.Contains("SMITH") || code.Contains("BLACKSMITH") || code.Contains("REPAIR"))
				return new string[] { "SMITH", "BLACKSMITH" };
			if (code.Contains("POTION") || code.Contains("HERBALIST") || code.Contains("PHARMACY"))
				return new string[] { "POTION", "HERBALIST", "DRUG" };
			if (code.Contains("HORSE") || code.Contains("STABLE"))
				return new string[] { "HORSE", "STABLE" };
			if (code.Contains("ACCESSORY") || code.Contains("JEWEL"))
				return new string[] { "ACCESSORY", "JEWEL" };
			if (code.Contains("ARMOR") || code.Contains("CLOTH") || code.Contains("PROTECTOR"))
				return new string[] { "ARMOR", "CLOTH", "PROTECTOR", "WEAPON" };
			if (code.Contains("GROCERY"))
				return new string[] { "GROCERY" };
			if (code.Contains("JUPITER"))
				return new string[] { "JUPITER" };
			return new string[0];
		}

		private void ExecuteStoreStep(string[] command, Window w, Bot b, bool includeGold = true)
		{
			string code = NpcCode(command);
			// Kullanıcı Town sekmesinde Storage'ı kapattıysa script adımı da atlanır.
			try
			{
				if (w.Town_cbxStorage != null && !w.Town_cbxStorage.Checked)
				{
					w.Log("Town Script: STORE atlandı (Town ayarlarında Storage kapalı).");
					return;
				}
			}
			catch { }
			w.Log($"Town Script: STORE [{code}]...");
			bool nameMatched;
			SREntity npc = FindTownNpc(code, out nameMatched);
			if (!PrepareNpcInteraction(npc, code, nameMatched, "STORE", w, b))
				return;
			try
			{
				w.Log($"STORE: [{npc.Name}] açılıyor...");
				DateTime openTime = DateTime.Now;
				// Depo daha önce hiç yüklenmediyse veri iste, açıksa diyalogu tazele
				// (programdaki manuel butonla aynı mantık).
				if (InfoManager.isStorageLoaded)
					PacketBuilder.TalkNPC(npc.UniqueID, 3);
				else
					PacketBuilder.OpenStorage(npc.UniqueID);
				// Depo penceresi gerçekten açıldı mı? Server 0x3047/48/49 göndermeden
				// eşya taşıma paketi atmak kick yedirir — en fazla 4sn bekle.
				bool opened = false;
				for (int i = 0; i < 40 && b.isBotting; i++)
				{
					if (InfoManager.LastStorageInfoTime > openTime || InfoManager.inStorage)
					{
						opened = true;
						break;
					}
					Thread.Sleep(100);
				}
				if (!opened)
				{
					w.Log("STORE: depo penceresi açılmadı (server veri göndermedi) — depozit atlanıyor, kick riski alınmadı.");
					return;
				}
				w.Log("STORE: depo açık, depozit başlıyor...");
				b.ExecuteStorageDeposit();
				if (includeGold)
					b.ExecuteStoreGold();
				w.Log("STORE: depozit tamam.");
			}
			catch (Exception ex)
			{
				w.LogProcess("STORE hatası: " + ex.Message, Window.ProcessState.Warning);
			}
			finally
			{
				// Depo/NPC penceresi açık kalırsa server sonrasındaki yürüme
				// paketlerini reddeder ve bot depoda takılı kalır — her durumda kapat.
				try
				{
					PacketBuilder.CloseNPC(npc.UniqueID);
					Thread.Sleep(400);
				}
				catch { }
			}
		}

		/// <summary>
		/// Dismantle konum kancası: ilgili NPC öncesi söküm adaylarını raporlar.
		/// (Söküm paketi bu server sürümünde doğrulanmadığı için uygulama yok.)
		/// </summary>
		private void DismantleCheck(string location, Window w)
		{
			try
			{
				var d = ItemFilterManager.Dismantle;
				if (d == null)
					return;
				bool want = (location == "Blacksmith" && d.BeforeBlacksmith)
					|| (location == "Grocery" && d.BeforeGrocery)
					|| (location == "Herbalist" && d.BeforeHerbalist)
					|| (location == "Storage" && d.BeforeStorage)
					|| (location == "Guild" && d.BeforeGuildStorage);
				if (!want)
					return;
				if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
					return;
				int count = 0;
				var inv = InfoManager.Character.Inventory;
				for (int i = 13; i < inv.Capacity; i++)
				{
					var item = inv[i];
					if (item == null)
						continue;
					try
					{
						if (ItemFilterManager.ShouldDismantle(item))
							count++;
					}
					catch { }
				}
				if (count > 0)
					w.Log($"Dismantle: {count} aday ({location} öncesi) — söküm paketi bu sürümde yok, atlandı.");
			}
			catch { }
		}

		private static string DismantleLocationForShop(string code)
		{
			if (string.IsNullOrEmpty(code))
				return "Grocery";
			if (code.Contains("SMITH") || code.Contains("ARMOR") || code.Contains("ACCESSORY"))
				return "Blacksmith";
			if (code.Contains("GROCERY"))
				return "Grocery";
			return "Herbalist";
		}

		private void ExecuteBuyStep(string[] command, Window w, Bot b)
		{
			string code = NpcCode(command);
			w.Log($"Town Script: BUY [{code}]...");
			DismantleCheck(DismantleLocationForShop(code), w);
			bool nameMatched;
			SREntity npc = FindTownNpc(code, out nameMatched);
			// Potion lojistiği (sell/buy paketleri) sadece ismi doğrulanmış NPC'de.
			bool isPotionRun = (code.Contains("POTION") || code.Contains("HERBALIST")) && nameMatched;
			if (!nameMatched)
			{
				// Yanlış NPC (genelde az önce kapanmamış bir pencerenin NPC'si veya
				// henüz spawn olmamış uzaktaki dükkân): dokunmadan geç.
				w.Log($"BUY: [{(npc != null ? npc.Name : "?")}] [{code}] ile eşleşmedi — atlanıyor.");
				return;
			}
			if (!PrepareNpcInteraction(npc, code, true, "BUY", w, b))
				return;
			if (!OpenNpcDialog(npc, "BUY", w, b))
				return;
			Thread.Sleep(200);
			try
			{
				// Potion satıcısında tam lojistik: çöp sat + potion al + mermi al.
				// Diğer dükkanlar (armor/accessory/horse) için seçim yeterli;
				// alım listesi kullanıcı ayarlarından yapılır.
				if (isPotionRun)
				{
					if (!IsConnectionAlive(b)) return;
					b.ExecuteSellTrash();
					Thread.Sleep(500);
					if (!IsConnectionAlive(b)) return;
					SREntity live = npc;
					try
					{
						bool againMatched;
						var again = FindTownNpc(code, out againMatched);
						if (again != null && againMatched) live = again;
					}
					catch { }
					b.ExecuteAutoBuyPotions(live);
				}
				else
				{
					w.LogProcess($"Shop [{npc.Name}] seçildi (alım listesi Town ayarlarından).");
					Thread.Sleep(800);
				}
			}
			catch (Exception ex)
			{
				w.LogProcess("BUY hatası: " + ex.Message, Window.ProcessState.Warning);
			}
			finally
			{
				// Dükkân penceresi açık kalırsa sonraki yürümeler reddedilir.
				try
				{
					if (npc != null && IsConnectionAlive(b))
						PacketBuilder.CloseNPC(npc.UniqueID);
					Thread.Sleep(300);
				}
				catch { }
			}
		}

		private void ExecuteRepairStep(string[] command, Window w, Bot b)
		{
			string code = NpcCode(command);
			try
			{
				if (w.Town_cbxRepair != null && !w.Town_cbxRepair.Checked)
				{
					w.Log("Town Script: REPAIR atlandı (Town ayarlarında Repair kapalı).");
					return;
				}
			}
			catch { }
			w.Log($"Town Script: REPAIR [{code}]...");
			DismantleCheck("Blacksmith", w);
			bool nameMatched;
			SREntity npc = FindTownNpc(code, out nameMatched);
			if (!PrepareNpcInteraction(npc, code, nameMatched, "REPAIR", w, b))
				return;
			if (!OpenNpcDialog(npc, "REPAIR", w, b))
				return;
			try
			{
				w.Log("Town Script: Repairing all equipments...");
				PacketBuilder.RepairAllEquipments(npc.UniqueID);
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				w.LogProcess("REPAIR hatası: " + ex.Message, Window.ProcessState.Warning);
			}
			finally
			{
				try
				{
					PacketBuilder.CloseNPC(npc.UniqueID);
					Thread.Sleep(300);
				}
				catch { }
			}
		}

		#endregion

		#region Script command parsing
		private SRCoord Move(string[] args)
		{
			// Eski virgüllü formatla uyumluluk için korunur.
			if (args.Length >= 3)
			{
				// Formats accepted :  "move,PosX,PosY" or "move,Region,X,Y,Z"
				ushort r;
				int x, y, z;
				if (args.Length == 5
					&& ushort.TryParse(args[1], out r)
					&& int.TryParse(args[4], out z)
					&& int.TryParse(args[2], out x)
					&& int.TryParse(args[3], out y))
				{
					// take it as Region,X,Z,Y
					return new SRCoord(r, x, z, y);
				}
				else if (args.Length == 3
					&& int.TryParse(args[1], out x)
					&& int.TryParse(args[2], out y))
				{
					// take it as ingame coordinates
					return new SRCoord(x, y);
				}
			}
			return ParseMove(args);
		}
		#endregion
	}
}
