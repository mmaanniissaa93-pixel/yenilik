using System;
using System.Collections.Generic;
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
			Window w = Window.Get;
			Bot b = Bot.Get;
			Running = true;
			// Parsing script
			for (int j = startIndex; j < m_lines.Count && Running && b.isBotting; j++)
			{
				if (m_lines[j].StartsWith("//") || string.IsNullOrWhiteSpace(m_lines[j]))
					continue;
				string[] command = Tokenize(m_lines[j]);
				if (command.Length == 0)
					continue;

				// Execute the command
				string cmd = command[0].Trim().ToLower();
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
						int ms = 1000;
						if (command.Length > 1 && int.TryParse(command[1], out int parsedMs))
							ms = parsedMs;
						w.LogProcess($"Script waiting ({ms}ms)...");
						Thread.Sleep(ms);
						break;
				}
			}
			Running = false;
		}

		public void Stop()
		{
			Running = false;
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
			return true;
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
			return new string[0];
		}

		private void ExecuteStoreStep(string[] command, Window w, Bot b)
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
			Thread.Sleep(200);
			try
			{
				// Potion satıcısında tam lojistik: çöp sat + potion al + mermi al.
				// Diğer dükkanlar (armor/accessory/horse) için seçim yeterli;
				// alım listesi kullanıcı ayarlarından yapılır.
				if (isPotionRun)
				{
					b.ExecuteSellTrash();
					Thread.Sleep(500);
					SREntity live = npc;
					try
					{
						bool againMatched;
						var again = FindTownNpc(code, out againMatched);
						if (again != null && againMatched) live = again;
					}
					catch { }
					b.ExecuteAutoBuyPotions(live);
					b.ExecuteAutoBuyAmmo();
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
					if (npc != null)
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
