using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

namespace xBot.App
{
	public class Script
	{
		private List<string> m_lines;
		private readonly ManualResetEvent m_stopSignal = new ManualResetEvent(false);
		private bool m_isTownScript;
		private uint m_lastRepairNpcUniqueID;
		private DateTime m_lastRepairTime = DateTime.MinValue;
		public string FileName { get; private set; }
		public bool Running { get; set; }
		public bool IsTownScript { get { return m_isTownScript; } }

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
						try
						{
							Script town = new Script(full);
							town.m_isTownScript = true;
							return town;
						}
						catch { }
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
						town.m_isTownScript = true;
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
						if (TradeLoopManager.IsRunning)
							b.HandleTradeWaypoint();
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
				case "doconsignment":
					ExecuteConsignmentStep(w, b);
					break;
				case "dostall":
					ExecutePlayerStallStep(w, b);
					break;
				case "doscript":
					ExecuteTrainingScript(w, b);
					break;
				case "recall":
					ExecuteRecall(w);
					break;
				case "mount":
					ExecuteMount(invocation, w, b);
					break;
				case "dismount":
					ExecuteDismount(w, b);
					break;
				case "killhorse":
					ExecuteKillHorse(w, b);
					break;
				case "terminate":
					ExecuteTerminate(invocation, w, b);
					break;
				case "profile":
					ExecuteProfile(invocation, w);
					break;
				case "oldtrade":
					ExecuteOldTrade(invocation, w, b);
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

		private void ExecuteMount(ScriptCommandInvocation command, Window w, Bot b)
		{
			if (InfoManager.Character == null)
			{
				w.LogProcess("MOUNT: karakter verisi hazır değil.", Window.ProcessState.Warning);
				return;
			}
			string type = command.Arguments.Length == 0 ? "fellow" : command.Arguments[0].ToLowerInvariant();
			SRCoService target = type == "transport"
				? InfoManager.MyPets.Find(pet => pet != null && pet.isTransport())
				: FindFellowMountCandidate();
			if (target == null)
			{
				w.LogProcess("MOUNT: çağrılmış " + type + " pet bulunamadı.", Window.ProcessState.Warning);
				return;
			}
			if (InfoManager.Character.isRiding)
			{
				if (InfoManager.Character.RidingUniqueID == target.UniqueID)
					w.LogProcess("MOUNT: karakter zaten seçilen pet üzerinde.");
				else
					w.LogProcess("MOUNT: önce mevcut binekten inilmesi gerekiyor.", Window.ProcessState.Warning);
				return;
			}

			while (InfoManager.MonitorPetMountResponse.WaitOne(0)) { }
			w.LogProcess("Script MOUNT: " + type + " [" + target.Name + "]...");
			PacketBuilder.SetPetMounted(target.UniqueID, true);
			if (!WaitForPetMountResponse(target.UniqueID, true, b))
				w.LogProcess("MOUNT: sunucu bindirme isteğini reddetti veya zaman aşımına uğradı.", Window.ProcessState.Warning);
		}

		private static SRCoService FindFellowMountCandidate()
		{
			SRCoService candidate = InfoManager.MyPets.Find(pet => pet != null && pet.isAttackPet()
				&& IsLikelyFellow(pet));
			// Eski/private server media tablolarında fellow ayrımı bulunmayabilir.
			// Böyle bir durumda sunucu 0x70CB isteğini doğrulayacak güvenli aday kullanılır.
			return candidate ?? InfoManager.MyPets.Find(pet => pet != null && pet.isAttackPet());
		}

		private static bool IsLikelyFellow(SRCoService pet)
		{
			string code = (pet.ServerName ?? "") + " " + (pet.Name ?? "");
			return code.IndexOf("FELLOW", StringComparison.OrdinalIgnoreCase) >= 0
				|| code.IndexOf("GROWTH", StringComparison.OrdinalIgnoreCase) >= 0
				|| code.IndexOf("RIDE", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private void ExecuteDismount(Window w, Bot b)
		{
			if (InfoManager.Character == null || !InfoManager.Character.isRiding)
			{
				w.LogProcess("DISMOUNT: karakter bir binekte değil.");
				return;
			}
			uint ridingID = InfoManager.Character.RidingUniqueID;
			SRCoService ridingPet = InfoManager.MyPets.Find(pet => pet != null && pet.UniqueID == ridingID);
			if (ridingPet != null && ridingPet.isHorse())
			{
				w.LogProcess("Script DISMOUNT: normal at sonlandırılıyor...");
				TerminateTransport(ridingPet, "DISMOUNT", w, b);
				return;
			}

			while (InfoManager.MonitorPetMountResponse.WaitOne(0)) { }
			w.LogProcess("Script DISMOUNT: binekten iniliyor...");
			PacketBuilder.SetPetMounted(ridingID, false);
			if (!WaitForPetMountResponse(ridingID, false, b))
				w.LogProcess("DISMOUNT: sunucu indirme isteğini reddetti veya zaman aşımına uğradı.", Window.ProcessState.Warning);
		}

		private void ExecuteKillHorse(Window w, Bot b)
		{
			uint ridingID = InfoManager.Character != null ? InfoManager.Character.RidingUniqueID : 0;
			SRCoService target = InfoManager.MyPets.Find(pet => pet != null && pet.UniqueID == ridingID
				&& (pet.isHorse() || pet.isTransport()));
			if (target == null)
				target = InfoManager.MyPets.Find(pet => pet != null && (pet.isHorse() || pet.isTransport()));
			if (target == null)
			{
				w.LogProcess("KILLHORSE: aktif at veya transport bulunamadı.", Window.ProcessState.Warning);
				return;
			}
			w.LogProcess("Script KILLHORSE: [" + target.Name + "] tamamen sonlandırılıyor...");
			TerminateTransport(target, "KILLHORSE", w, b);
		}

		private void ExecuteTerminate(ScriptCommandInvocation command, Window w, Bot b)
		{
			string type = command.Arguments.Length == 0 ? "all" : command.Arguments[0].ToLowerInvariant();
			List<SRCoService> targets = InfoManager.MyPets.FindAll(pet => pet != null
				&& (type == "all" && (pet.isHorse() || pet.isTransport())
					|| type == "horse" && pet.isHorse()
					|| type == "transport" && pet.isTransport()));
			if (targets.Count == 0)
			{
				w.LogProcess("TERMINATE: aktif " + (type == "all" ? "at/transport" : type) + " bulunamadı.");
				return;
			}
			w.LogProcess($"Script TERMINATE: {targets.Count} {type} pet sonlandırılıyor...");
			foreach (SRCoService target in targets)
			{
				if (!Running || !IsConnectionAlive(b)) return;
				TerminateTransport(target, "TERMINATE", w, b);
			}
		}

		private void ExecuteProfile(ScriptCommandInvocation command, Window w)
		{
			string name = command.Arguments.Length == 0 ? "Default" : command.Arguments[0];
			string before = Settings.LoadedCharacterProfile;
			string error;
			if (!Settings.TryLoadCharacterProfile(name, out error))
			{
				w.LogProcess("PROFILE: " + error, Window.ProcessState.Warning);
				return;
			}
			string loaded = Settings.LoadedCharacterProfile;
			if (string.Equals(before, loaded, StringComparison.OrdinalIgnoreCase))
				w.LogProcess("PROFILE: [" + loaded + "] zaten yüklü.");
			else
				w.LogProcess("PROFILE: [" + loaded + "] yüklendi.");
		}

		private void ExecuteOldTrade(ScriptCommandInvocation command, Window w, Bot b)
		{
			string operation = command.Arguments[0].ToLowerInvariant();
			if (operation == "spawn")
			{
				SpawnOldTradeTransport(command.Arguments.Length > 1 ? command.Arguments[1] : null, w, b);
				return;
			}

			SRCoService transport = InfoManager.MyPets.Find(pet => pet != null && pet.isTransport());
			if (transport == null || transport.Inventory == null)
			{
				w.LogProcess("OLDTRADE: aktif ve envanteri yüklenmiş trade taşıtı bulunamadı.", Window.ProcessState.Warning);
				return;
			}

			int requestedAmount = 0;
			string requestedItem = null;
			if (operation == "buy")
			{
				requestedAmount = int.Parse(command.Arguments[1]);
				requestedItem = command.Arguments.Length > 2 ? command.Arguments[2] : null;
				// phBot'ta 1..5 değerleri adet değil yıldızdır. Core projesinde yıldız
				// eşiği hesabı bulunmadığı için yanlış miktar almaktansa açıkça dur.
				if (requestedAmount >= 1 && requestedAmount <= 5)
				{
					w.LogProcess("OLDTRADE BUY: 1-5 yıldız hesabı henüz doğrulanmadı. 0 (taşıtı doldur) veya 5'ten büyük kesin adet kullanın.", Window.ProcessState.Warning);
					return;
				}
			}

			bool nameMatched;
			SREntity npc = FindTownNpc("SPECIALTY_TRADER", out nameMatched);
			if (!PrepareNpcInteraction(npc, "SPECIALTY_TRADER", nameMatched, "OLDTRADE " + operation.ToUpperInvariant(), w, b))
				return;

			try
			{
				if (!OpenNpcDialog(npc, "OLDTRADE " + operation.ToUpperInvariant(), w, b, 12))
					return;
				List<NameValueCollection> shopRows = DataManager.GetShopItems(npc.ServerName);
				if (operation == "buy")
					BuyOldTradeGoods(transport, npc, shopRows, requestedAmount, requestedItem, w, b);
				else
					SellOldTradeGoods(transport, npc, shopRows, w, b);
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

		private void SpawnOldTradeTransport(string requestedName, Window w, Bot b)
		{
			SRCoService active = InfoManager.MyPets.Find(pet => pet != null && pet.isTransport());
			if (active != null)
			{
				w.LogProcess("OLDTRADE SPAWN: trade taşıtı zaten aktif [" + active.Name + "].");
				return;
			}
			if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
				return;

			SRItem selected = null;
			byte selectedSlot = 0;
			for (int slot = 13; slot < InfoManager.Character.Inventory.Capacity && slot <= byte.MaxValue; slot++)
			{
				SRItem item = InfoManager.Character.Inventory[slot];
				// Summon scroll bir COS nesnesi değil, ETC/COS_TRANSPORT itemidir
				// (core TypeIdFilter: 3,3,2,7; private serverlarda TID4 değişebilir).
				if (item == null || item.ID2 != 3 || item.ID3 != 2
					|| (item.ServerName ?? "").IndexOf("COS_T_", StringComparison.OrdinalIgnoreCase) < 0)
					continue;
				if (!string.IsNullOrWhiteSpace(requestedName)
					&& !string.Equals(item.Name, requestedName, StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(item.ServerName, requestedName, StringComparison.OrdinalIgnoreCase))
					continue;
				selected = item;
				selectedSlot = (byte)slot;
				break;
			}
			if (selected == null)
			{
				w.LogProcess("OLDTRADE SPAWN: uygun trade taşıtı scroll'u bulunamadı"
					+ (string.IsNullOrWhiteSpace(requestedName) ? "." : " [" + requestedName + "]."), Window.ProcessState.Warning);
				return;
			}

			w.LogProcess("OLDTRADE SPAWN: [" + selected.Name + "] çağrılıyor...");
			if (!PacketBuilder.UseItem(selected, selectedSlot))
			{
				w.LogProcess("OLDTRADE SPAWN: scroll kullanılamadı.", Window.ProcessState.Warning);
				return;
			}
			for (int i = 0; i < 50 && Running && IsConnectionAlive(b); i++)
			{
				active = InfoManager.MyPets.Find(pet => pet != null && pet.isTransport());
				if (active != null)
				{
					w.LogProcess("OLDTRADE SPAWN: trade taşıtı hazır [" + active.Name + "].");
					return;
				}
				Thread.Sleep(100);
			}
			w.LogProcess("OLDTRADE SPAWN: taşıt spawn bilgisi alınamadı.", Window.ProcessState.Warning);
		}

		private void BuyOldTradeGoods(SRCoService transport, SREntity npc,
			List<NameValueCollection> shopRows, int requestedAmount, string requestedName, Window w, Bot b)
		{
			NameValueCollection selectedRow = null;
			SRItem selectedItem = null;
			foreach (NameValueCollection row in shopRows)
			{
				SRItem item = SRItem.Create(row["item_servername"]);
				if (item == null || item.ID2 != 3 || item.ID3 != 8) continue;
				if (!string.IsNullOrWhiteSpace(requestedName)
					&& !string.Equals(item.Name, requestedName, StringComparison.OrdinalIgnoreCase)
					&& !string.Equals(item.ServerName, requestedName, StringComparison.OrdinalIgnoreCase)) continue;
				selectedRow = row;
				selectedItem = item;
				break;
			}
			if (selectedRow == null || selectedItem == null)
			{
				w.LogProcess("OLDTRADE BUY: NPC mağazasında uygun specialty goods bulunamadı"
					+ (string.IsNullOrWhiteSpace(requestedName) ? "." : " [" + requestedName + "]."), Window.ProcessState.Warning);
				return;
			}

			byte tab, slot;
			if (!byte.TryParse(selectedRow["tab"], out tab) || !byte.TryParse(selectedRow["slot"], out slot))
			{
				w.LogProcess("OLDTRADE BUY: mağaza slotu ayrıştırılamadı.", Window.ProcessState.Warning);
				return;
			}
			int bought = 0;
			int maxSteps = Math.Max(1, transport.Inventory.Capacity + 1);
			while (Running && IsConnectionAlive(b) && maxSteps-- > 0)
			{
				if (transport.Inventory.Count >= transport.Inventory.Capacity) break;
				int remaining = requestedAmount == 0 ? selectedItem.QuantityMax : requestedAmount - bought;
				if (remaining <= 0) break;
				ushort amount = (ushort)Math.Min(Math.Max(1, (int)selectedItem.QuantityMax), Math.Min(remaining, (int)ushort.MaxValue));
				int before = CountTransportItem(transport, selectedItem.ID);
				while (InfoManager.MonitorInventoryMovement.WaitOne(0)) { }
				PacketBuilder.BuyTradeGood(transport.UniqueID, tab, slot, amount, npc.UniqueID);
				if (!WaitForTradeInventoryChange(() => CountTransportItem(transport, selectedItem.ID) > before, b))
				{
					w.LogProcess("OLDTRADE BUY: sunucu hareketi onaylamadı; alım durduruldu.", Window.ProcessState.Warning);
					break;
				}
				int after = CountTransportItem(transport, selectedItem.ID);
				bought += Math.Max(0, after - before);
			}
			w.LogProcess("OLDTRADE BUY: [" + selectedItem.Name + "] alınan toplam " + bought + ".");
		}

		private void SellOldTradeGoods(SRCoService transport, SREntity npc,
			List<NameValueCollection> shopRows, Window w, Bot b)
		{
			var localGoods = new HashSet<uint>();
			foreach (NameValueCollection row in shopRows)
			{
				SRItem local = SRItem.Create(row["item_servername"]);
				if (local != null && local.ID2 == 3 && local.ID3 == 8) localGoods.Add(local.ID);
			}
			int sold = 0;
			for (int slot = 0; slot < transport.Inventory.Capacity && Running && IsConnectionAlive(b); slot++)
			{
				SRItem item = transport.Inventory[slot];
				if (item == null || item.ID2 != 3 || item.ID3 != 8 || localGoods.Contains(item.ID)) continue;
				ushort before = item.Quantity;
				while (InfoManager.MonitorInventoryMovement.WaitOne(0)) { }
				PacketBuilder.SellTradeGood(transport.UniqueID, (byte)slot, before, npc.UniqueID);
				if (!WaitForTradeInventoryChange(() => transport.Inventory[slot] == null
					|| transport.Inventory[slot].Quantity < before, b))
				{
					w.LogProcess("OLDTRADE SELL: [" + item.Name + "] sunucu tarafından onaylanmadı; satış durduruldu.", Window.ProcessState.Warning);
					break;
				}
				sold += before;
			}
			w.LogProcess("OLDTRADE SELL: satılan toplam specialty goods " + sold + ".");
		}

		private static int CountTransportItem(SRCoService transport, uint itemId)
		{
			int total = 0;
			if (transport == null || transport.Inventory == null) return total;
			for (int i = 0; i < transport.Inventory.Capacity; i++)
			{
				SRItem item = transport.Inventory[i];
				if (item != null && item.ID == itemId) total += item.Quantity;
			}
			return total;
		}

		private bool WaitForTradeInventoryChange(Func<bool> changed, Bot b)
		{
			for (int i = 0; i < 35 && Running && IsConnectionAlive(b); i++)
			{
				if (changed()) return true;
				InfoManager.MonitorInventoryMovement.WaitOne(100);
			}
			return changed();
		}

		private void TerminateTransport(SRCoService target, string command, Window w, Bot b)
		{
			while (InfoManager.MonitorPetRemoved.WaitOne(0)) { }
			PacketBuilder.TerminatePet(target.UniqueID);
			for (int i = 0; i < 30 && Running && IsConnectionAlive(b); i++)
			{
				if (InfoManager.MyPets.Find(pet => pet != null && pet.UniqueID == target.UniqueID) == null)
					return;
				InfoManager.MonitorPetRemoved.WaitOne(100);
			}
			w.LogProcess(command + ": pet sonlandırma onayı gelmedi; script devam ediyor.", Window.ProcessState.Warning);
		}

		private bool WaitForPetMountResponse(uint targetUniqueID, bool mounted, Bot b)
		{
			for (int i = 0; i < 25 && Running && IsConnectionAlive(b); i++)
			{
				if (!InfoManager.MonitorPetMountResponse.WaitOne(100))
					continue;
				if (!InfoManager.LastPetMountSuccess || InfoManager.Character == null)
					return false;
				return mounted
					? InfoManager.Character.RidingUniqueID == targetUniqueID
					: !InfoManager.Character.isRiding;
			}
			return false;
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
			bool repair = command == "doblacksmith" || command == "dojupiter";
			bool buyPotions = command == "doherbalist" || command == "dojupiter";
			bool buyAmmo = command == "dogrocerytrader";
			if (!PrepareNpcInteraction(npc, npcCode, nameMatched, action, w, b))
				return;
			if (repair && (w.Town_cbxRepair == null || w.Town_cbxRepair.Checked))
				RepairSelectedNpc(npc, action, w, b);
			if (!OpenNpcDialog(npc, action, w, b))
				return;

			try
			{
				string dismantleLocation = command == "doherbalist" ? "Herbalist"
					: command == "dogrocerytrader" ? "Grocery"
					: (command == "doblacksmith" || command == "doprotectortrader" || command == "dojupiter") ? "Blacksmith"
					: null;
				if (dismantleLocation != null)
					DismantleCheck(dismantleLocation, w);

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
				bool storageWasLoaded = InfoManager.isStorageLoaded;
				while (InfoManager.MonitorNpcTalk.WaitOne(0)) { }
				if (storageWasLoaded)
					PacketBuilder.TalkNPC(npc.UniqueID, 3);
				else
					PacketBuilder.OpenStorage(npc.UniqueID);
				bool opened = WaitForStorageSession(npc.UniqueID, openTime, storageWasLoaded, b);
				if (!opened || InfoManager.Character.Storage == null)
				{
					w.LogProcess("DoStorageTake: güncel storage verisi alınamadı.", Window.ProcessState.Warning);
					return;
				}
				if (!WaitInterruptible(700)) return;
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

		private void ExecuteConsignmentStep(Window w, Bot b)
		{
			const string npcCode = "NPC_OPEN_MARKET";
			bool nameMatched;
			SREntity npc = FindTownNpc(npcCode, out nameMatched);
			if (!PrepareNpcInteraction(npc, npcCode, nameMatched, "DoConsignment", w, b))
				return;

			try
			{
				// Consignment/Flea Market normal shop talk=3 yerine talk=0x21 kullanır.
				if (!OpenNpcDialog(npc, "DoConsignment", w, b, 0x21))
					return;
				while (InfoManager.MonitorConsignmentList.WaitOne(0)) { }
				DateTime requestTime = DateTime.UtcNow;
				w.LogProcess("DoConsignment: mevcut ilan listesi yenileniyor...");
				PacketBuilder.RequestConsignmentList();

				bool received = false;
				for (int i = 0; i < 40 && Running && IsConnectionAlive(b); i++)
				{
					if (InfoManager.MonitorConsignmentList.WaitOne(100)
						&& InfoManager.LastConsignmentListTime >= requestTime)
					{
						received = true;
						break;
					}
				}
				if (received)
				{
					w.LogProcess($"DoConsignment: oturum hazır ({InfoManager.LastConsignmentListPayload.Length} bayt liste verisi).");
					if (InfoManager.LastConsignmentListPayload.Length == 0
						|| InfoManager.LastConsignmentListPayload[0] != 1)
					{
						w.LogProcess("DoConsignment: liste cevabı başarılı değil; register yapılmadı.", Window.ProcessState.Warning);
						return;
					}
					List<ConsignmentListing> listings = InfoManager.LastConsignmentListings;
					if (ConsignmentManager.AutoSettleSold)
					{
						var soldIds = new List<uint>();
						foreach (ConsignmentListing listing in listings)
							if (listing.IsSold) soldIds.Add(listing.ConsignmentId);
						if (soldIds.Count > 0)
						{
							while (InfoManager.MonitorConsignmentSettle.WaitOne(0)) { }
							w.LogProcess($"DoConsignment: {soldIds.Count} satılmış ilan tahsil ediliyor...");
							PacketBuilder.SettleConsignments(soldIds);
							if (!InfoManager.MonitorConsignmentSettle.WaitOne(4000)
								|| !InfoManager.LastConsignmentSettleSuccess)
								w.LogProcess("DoConsignment: satış tahsilatı onaylanmadı.", Window.ProcessState.Warning);
						}
					}
					if (ConsignmentManager.AutoRetrieveExpired)
					{
						bool hasExpired = false;
						foreach (ConsignmentListing listing in listings)
							if (listing.IsExpired) { hasExpired = true; break; }
						if (hasExpired)
						{
							while (InfoManager.MonitorConsignmentUnregister.WaitOne(0)) { }
							w.LogProcess("DoConsignment: süresi dolan ilanlar geri alınıyor...");
							PacketBuilder.RetrieveAllExpiredConsignments();
							if (!InfoManager.MonitorConsignmentUnregister.WaitOne(4000)
								|| !InfoManager.LastConsignmentUnregisterSuccess)
								w.LogProcess("DoConsignment: süresi dolanları alma işlemi onaylanmadı.", Window.ProcessState.Warning);
						}
					}

					List<ConsignmentRegistrationCandidate> candidates = ConsignmentManager.GetInventoryCandidates();
					if (candidates.Count == 0)
					{
						w.LogProcess("DoConsignment: satış filtresine uyan envanter itemi yok.");
						return;
					}

					foreach (ConsignmentRegistrationCandidate candidate in candidates)
					{
						if (!Running || !IsConnectionAlive(b)) return;
						while (InfoManager.MonitorConsignmentRegister.WaitOne(0)) { }
						DateTime registerTime = DateTime.UtcNow;
						w.LogProcess($"DoConsignment: [{candidate.ItemName}] x{candidate.Quantity}, {candidate.Price:N0} gold kaydediliyor...");
						PacketBuilder.RegisterConsignmentItem(candidate.InventorySlot, candidate.RentableId,
							candidate.ItemId, candidate.Quantity, candidate.Price);
						bool answered = false;
						for (int i = 0; i < 40 && Running && IsConnectionAlive(b); i++)
						{
							if (InfoManager.MonitorConsignmentRegister.WaitOne(100)
								&& InfoManager.LastConsignmentRegisterTime >= registerTime)
							{
								answered = true;
								break;
							}
						}
						if (!answered)
						{
							w.LogProcess("DoConsignment: B508 onayı gelmedi; kalan kayıtlar durduruldu.", Window.ProcessState.Warning);
							break;
						}
						if (!InfoManager.LastConsignmentRegisterSuccess)
						{
							w.LogProcess($"DoConsignment: kayıt reddedildi (0x{InfoManager.LastConsignmentRegisterError:X4}); kalan kayıtlar durduruldu.", Window.ProcessState.Warning);
							break;
						}
						if (!WaitInterruptible(350)) return;
					}
				}
				else
					w.LogProcess("DoConsignment: 0xB50E liste cevabı gelmedi; güvenlik için item işlemi yapılmadı.", Window.ProcessState.Warning);
			}
			catch (Exception ex)
			{
				w.LogProcess("DoConsignment hatası: " + ex.Message, Window.ProcessState.Warning);
			}
			finally
			{
				try
				{
					if (IsConnectionAlive(b))
					{
						PacketBuilder.CloseConsignment();
						WaitInterruptible(250);
						PacketBuilder.CloseNPC(npc.UniqueID);
					}
					WaitInterruptible(300);
				}
				catch { }
			}
		}

		private void ExecutePlayerStallStep(Window w, Bot b)
		{
			try
			{
				List<ConsignmentRegistrationCandidate> candidates = ConsignmentManager.GetInventoryCandidates();
				int minimum = Math.Max(1, Math.Min(10, ConsignmentManager.MinimumPlayerStallItems));
				if (InfoManager.inStall && InfoManager.StallerPlayer != null)
				{
					w.LogProcess("DoStall: başka oyuncunun stall penceresi açık; işlem yapılmadı.", Window.ProcessState.Warning);
					return;
				}

				if (!InfoManager.inStall)
				{
					while (InfoManager.MonitorStallCreate.WaitOne(0)) { }
					w.LogProcess("DoStall: oyuncu stall'ı oluşturuluyor...");
					PacketBuilder.BeginStall(w.Stall_tbxStallTitle.Text);
					if (!InfoManager.MonitorStallCreate.WaitOne(4000) || !InfoManager.LastStallCreateSuccess)
					{
						w.LogProcess("DoStall: B0B1 oluşturma onayı gelmedi.", Window.ProcessState.Warning);
						return;
					}
					while (InfoManager.MonitorStallUpdate.WaitOne(0)) { }
					PacketBuilder.EditStallNote(w.Stall_tbxStallNote.Text);
					if (!WaitForStallUpdate(SRTypes.StallUpdate.Note, 4000))
					{
						w.LogProcess("DoStall: stall notu sunucu tarafından onaylanmadı.", Window.ProcessState.Warning);
						return;
					}
				}

				bool[] occupied = new bool[10];
				var stallInventory = InfoManager.Character.Stall.Inventory;
				int existingItemCount = 0;
				if (stallInventory != null)
				{
					for (int i = 0; i < Math.Min(10, stallInventory.Capacity); i++)
					{
						occupied[i] = stallInventory[i] != null;
						if (occupied[i]) existingItemCount++;
					}
				}
				if (existingItemCount + candidates.Count < minimum)
				{
					w.LogProcess($"DoStall: mevcut ve filtreye uyan toplam {existingItemCount + candidates.Count} item var; minimum {minimum}, stall açılmadı.", Window.ProcessState.Warning);
					return;
				}

				foreach (ConsignmentRegistrationCandidate candidate in candidates)
				{
					if (!Running || !IsConnectionAlive(b)) return;
					bool alreadyAdded = false;
					stallInventory = InfoManager.Character.Stall.Inventory;
					if (stallInventory != null)
					{
						for (int i = 0; i < stallInventory.Capacity; i++)
							if (stallInventory[i] != null && stallInventory[i].SlotInventory == candidate.InventorySlot)
							{ alreadyAdded = true; break; }
					}
					if (alreadyAdded) continue;

					int empty = Array.FindIndex(occupied, used => !used);
					if (empty < 0) break;
					while (InfoManager.MonitorStallUpdate.WaitOne(0)) { }
					w.LogProcess($"DoStall: [{candidate.ItemName}] x{candidate.Quantity}, {candidate.Price:N0} gold ekleniyor...");
					PacketBuilder.AddItemStall((byte)empty, candidate.InventorySlot, candidate.Quantity, candidate.Price);
					if (!WaitForStallUpdate(SRTypes.StallUpdate.ItemAdded, 4000))
					{
						w.LogProcess("DoStall: item ekleme onaylanmadı; kalan itemler durduruldu.", Window.ProcessState.Warning);
						break;
					}
					occupied[empty] = true;
					if (!WaitInterruptible(250)) return;
				}

				int itemCount = 0;
				stallInventory = InfoManager.Character.Stall.Inventory;
				if (stallInventory != null)
					for (int i = 0; i < Math.Min(10, stallInventory.Capacity); i++) if (stallInventory[i] != null) itemCount++;
				if (itemCount < minimum)
				{
					w.LogProcess($"DoStall: yalnız {itemCount} item eklendi; minimum {minimum}, stall açılmadı.", Window.ProcessState.Warning);
					return;
				}
				if (ConsignmentManager.AutoOpenPlayerStall && !InfoManager.IsOwnStallOpen)
				{
					while (InfoManager.MonitorStallUpdate.WaitOne(0)) { }
					PacketBuilder.EditStallState(true);
					if (!WaitForStallUpdate(SRTypes.StallUpdate.State, 4000))
						w.LogProcess("DoStall: stall açılış cevabı alınamadı.", Window.ProcessState.Warning);
					else
						w.LogProcess($"DoStall: stall {itemCount} item ile açıldı.");
				}
			}
			catch (Exception ex)
			{
				w.LogProcess("DoStall hatası: " + ex.Message, Window.ProcessState.Warning);
			}
		}

		private bool WaitForStallUpdate(SRTypes.StallUpdate expectedType, int timeoutMs)
		{
			DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
			while (Running)
			{
				int remaining = (int)Math.Ceiling((deadline - DateTime.UtcNow).TotalMilliseconds);
				if (remaining <= 0 || !InfoManager.MonitorStallUpdate.WaitOne(Math.Min(remaining, 250)))
				{
					if (DateTime.UtcNow >= deadline) return false;
					continue;
				}
				if (!InfoManager.LastStallUpdateSuccess) return false;
				if (InfoManager.LastStallUpdateType == expectedType) return true;
			}
			return false;
		}

		private void ExecuteTrainingScript(Window w, Bot b)
		{
			if (!m_isTownScript)
			{
				w.LogProcess("DoScript yalnızca town scripti içinden kullanılabilir.", Window.ProcessState.Warning);
				return;
			}
			string path = w.TrainingArea_GetScript();
			if (string.IsNullOrWhiteSpace(path))
			{
				w.LogProcess("DoScript: seçili training alanında yürüyüş scripti yok.", Window.ProcessState.Warning);
				return;
			}
			if (!Path.IsPathRooted(path))
				path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
			if (!File.Exists(path))
			{
				w.LogProcess("DoScript: yürüyüş scripti bulunamadı: " + path, Window.ProcessState.Warning);
				return;
			}
			try
			{
				w.LogProcess("DoScript: training yürüyüş scripti çalıştırılıyor [" + Path.GetFileName(path) + "]...");
				Script walkScript = new Script(path);
				walkScript.Run(0);
				if (Running && b.isBotting)
					w.LogProcess("DoScript: training yürüyüş scripti tamamlandı.");
			}
			catch (Exception ex)
			{
				w.LogProcess("DoScript hatası: " + ex.Message, Window.ProcessState.Warning);
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
		internal static bool OpenNpcDialog(SREntity npc, string action, Window w, Bot b, byte talkID = 3)
		{
			if (npc == null || !IsConnectionAlive(b))
				return false;

			while (InfoManager.MonitorNpcTalk.WaitOne(0)) { }
			w.LogProcess($"{action}: [{npc.Name}] dükkânı açılıyor...");
			PacketBuilder.TalkNPC(npc.UniqueID, talkID);
			for (int i = 0; i < 30 && IsConnectionAlive(b); i++)
			{
				if (InfoManager.MonitorNpcTalk.WaitOne(100)
					&& InfoManager.LastNpcTalkEntityUniqueID == npc.UniqueID
					&& InfoManager.LastNpcTalkID == talkID)
					return true;
			}

			w.LogProcess($"{action}: [{npc.Name}] dükkânı açılmadı; güvenlik için işlem atlandı.", Window.ProcessState.Warning);
			return false;
		}

		private bool RepairSelectedNpc(SREntity npc, string action, Window w, Bot b)
		{
			if (npc == null || !IsConnectionAlive(b) || InfoManager.SelectedEntityUniqueID != npc.UniqueID)
				return false;

			while (InfoManager.MonitorRepairResponse.WaitOne(0)) { }
			w.LogProcess($"{action}: [{npc.Name}] ekipmanlar tamir ediliyor...");
			PacketBuilder.RepairAllEquipments(npc.UniqueID);
			for (int i = 0; i < 30 && Running && IsConnectionAlive(b); i++)
			{
				if (!InfoManager.MonitorRepairResponse.WaitOne(100))
					continue;
				if (!InfoManager.LastRepairSuccess)
				{
					w.LogProcess($"{action}: tamir reddedildi (0x{InfoManager.LastRepairErrorCode:X4}).", Window.ProcessState.Warning);
					return false;
				}
				m_lastRepairNpcUniqueID = npc.UniqueID;
				m_lastRepairTime = DateTime.UtcNow;
				return true;
			}

			w.LogProcess($"{action}: 0xB03E tamir cevabı gelmedi; tamamlandı sayılmadı.", Window.ProcessState.Warning);
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
			if (code.Contains("OPEN_MARKET") || code.Contains("CONSIGNMENT"))
				return new string[] { "OPEN_MARKET", "CONSIGNMENT", "FLEA MARKET" };
			if (code.Contains("SPECIALTY") || code.Contains("SPECIALITY"))
				return new string[] { "SPECIALTY", "SPECIALITY", "SPECIAL GOODS", "TRADE MERCHANT" };
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
				bool storageWasLoaded = InfoManager.isStorageLoaded;
				while (InfoManager.MonitorNpcTalk.WaitOne(0)) { }
				// Depo daha önce hiç yüklenmediyse veri iste, açıksa diyalogu tazele
				// (programdaki manuel butonla aynı mantık).
				if (storageWasLoaded)
					PacketBuilder.TalkNPC(npc.UniqueID, 3);
				else
					PacketBuilder.OpenStorage(npc.UniqueID);
				// Depo penceresi gerçekten açıldı mı? Server 0x3047/48/49 göndermeden
				// eşya taşıma paketi atmak kick yedirir — en fazla 4sn bekle.
				bool opened = WaitForStorageSession(npc.UniqueID, openTime, storageWasLoaded, b);
				if (!opened)
				{
					w.Log("STORE: depo penceresi açılmadı (server veri göndermedi) — depozit atlanıyor, kick riski alınmadı.");
					return;
				}
				if (!WaitInterruptible(700)) return;
				w.Log("STORE: depo açık, depozit başlıyor...");
				bool depositCompleted = b.ExecuteStorageDeposit(npc.UniqueID);
				if (!depositCompleted)
				{
					if (!IsConnectionAlive(b) || !Running)
					{
						w.LogProcess("STORE: bağlantı/script durduğu için kalan storage işlemleri atlandı.", Window.ProcessState.Warning);
						return;
					}
					w.LogProcess("STORE: bazı eşya hareketleri tamamlanamadı; gold işlemi bağımsız olarak sürdürülecek.", Window.ProcessState.Warning);
				}
				if (includeGold)
					b.ExecuteStoreGold();
				w.Log(depositCompleted ? "STORE: depozit tamam." : "STORE: kısmi depozit tamamlandı.");
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

		private bool WaitForStorageSession(uint npcUniqueID, DateTime requestTime,
			bool cachedStorageAvailable, Bot b)
		{
			for (int i = 0; i < 40 && Running && IsConnectionAlive(b); i++)
			{
				if (InfoManager.LastStorageInfoTime > requestTime)
					return true;
				if (InfoManager.MonitorNpcTalk.WaitOne(100)
					&& cachedStorageAvailable
					&& InfoManager.LastNpcTalkEntityUniqueID == npcUniqueID
					&& InfoManager.LastNpcTalkID == 3)
					return true;
			}
			return false;
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
			TownNpcActions actions = TownNpcActionPolicy.ForNpcCode(code);
			if (!nameMatched)
			{
				// Yanlış NPC (genelde az önce kapanmamış bir pencerenin NPC'si veya
				// henüz spawn olmamış uzaktaki dükkân): dokunmadan geç.
				w.Log($"BUY: [{(npc != null ? npc.Name : "?")}] [{code}] ile eşleşmedi — atlanıyor.");
				return;
			}
			if (!PrepareNpcInteraction(npc, code, true, "BUY", w, b))
				return;
			if ((actions & TownNpcActions.Repair) != 0
				&& (w.Town_cbxRepair == null || w.Town_cbxRepair.Checked))
				RepairSelectedNpc(npc, "BUY", w, b);
			if (!OpenNpcDialog(npc, "BUY", w, b))
				return;
			Thread.Sleep(200);
			try
			{
				// Her doğrulanmış shop NPC'si kendi gerçek işlevlerini uygular. Tamir,
				// cephane/potion hedefinden bağımsızdır ve hedef 0 olsa da atlanmaz.
				if ((actions & TownNpcActions.Sell) != 0
					&& (w.Town_cbxSellTrash == null || w.Town_cbxSellTrash.Checked))
				{
					if (!IsConnectionAlive(b)) return;
					b.ExecuteSellTrash();
					if (!WaitInterruptible(500)) return;
				}

				if ((actions & TownNpcActions.BuyPotions) != 0)
				{
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
				if ((actions & TownNpcActions.BuyAmmo) != 0)
					b.ExecuteAutoBuyAmmoFromOpenNpc(npc);

				w.LogProcess($"BUY: [{npc.Name}] şehir işlevleri tamamlandı ({actions}).");
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
			// Eski town dosyalarında BUY SMITH satırının hemen arkasında REPAIR
			// bulunur. BUY artık tamiri yaptığı için ikinci paketi göndermeden geç.
			if (m_lastRepairNpcUniqueID != 0
				&& (DateTime.UtcNow - m_lastRepairTime).TotalSeconds < 8)
			{
				w.Log("Town Script: REPAIR, önceki demirci BUY adımında tamamlandı.");
				return;
			}
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
			try
			{
				RepairSelectedNpc(npc, "REPAIR", w, b);
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
