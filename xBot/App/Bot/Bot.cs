using System;
using xBot.Network;
using SecurityAPI;
using System.Collections.Generic;
using System.IO;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using System.Threading;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

namespace xBot.App
{
	/// <summary>
	/// Handle everything about bot logic.
	/// </summary>
	public partial class Bot
	{
		/// <summary>
		/// Unique instance of this class.
		/// </summary>
		private static Bot _this = null;
		private Random rand = new Random();
		/// <summary>
		/// Check if the bot is using auto login mode from command line.
		/// </summary>
		public bool hasAutoLoginMode { get; set; }
		/// <summary>
		/// Get or set the proxy actually running.
		/// </summary>
		public Proxy Proxy { get; set; }
		/// <summary>
		/// Gets or set if the login has been sent through bot.
		/// </summary>
		public bool LoggedFromBot { get; set; }
		/// <summary>
		/// Check if the character is on creation process.
		/// </summary>
		public bool isCreatingCharacter { get { return !string.IsNullOrEmpty(CreatingCharacterName); } }
		private string CreatingCharacterName;
		private bool CreatingCharacterMale;
		/// <summary>
		/// Check if the character is in trace mode.
		/// </summary>
		public bool inTrace { get; private set; }
		private string TracePlayerName;
		/// <summary>
		/// Check if is sorting any inventory.
		/// </summary>
		public bool isSorting
		{
			get
			{
				lock (m_sortingLock)
				{
					return tSorting != null && tSorting.IsAlive;
				}
			}
		}
		private Thread tSorting;
		private readonly object m_sortingLock = new object();
		private int m_sortingSessionId;
		private volatile bool m_stopSortingRequested;
		/// <summary>
		/// Ping check by command
		/// </summary>
		private System.Diagnostics.Stopwatch m_Ping;
		/// <summary>
		/// Check if bot is recording a walking script.
		/// </summary>
		public bool isRecording { get; private set; }
		private Thread tRecording;
		private volatile bool m_stopRecordingRequested;
		private volatile bool m_recordingPaused;
		private readonly System.Collections.Generic.List<string> m_recordedLines = new System.Collections.Generic.List<string>();

		private Bot()
		{
			InitializeTimers();
		}
		/// <summary>
		/// GetInstance. Secures an unique class creation for being used anywhere at the project.
		/// </summary>
		public static Bot Get
		{
			get
			{
				if (_this == null)
					_this = new Bot();
				return _this;
			}
		}
		/// <summary>
		/// Log to a file the errors with the packet if is needed.
		/// </summary>
		public void LogError(string message,Exception ex,Packet packet = null)
		{
			string msg = DateTime.Now.ToString("[dd/MM/yyyy|HH:mm:ss]") + "[" + message + "][" + (ex?.Message ?? "") + "][" + (ex?.StackTrace ?? "") + "]" + Environment.NewLine;
			if (packet != null)
				msg += packet.ToString() + Environment.NewLine;
			try { File.AppendAllText("errors.log", msg); } catch { }
		}

		#region (Extended Protocol Setup)
		public void SetExtendedProtocol()
		{

		}
		#endregion

		#region (Methods)
		/// <summary>
		/// Formats a character nickname with a numeric suffix, enforcing Silkroad's 12-character limit
		/// while preserving the full numeric suffix.
		/// </summary>
		public static string FormatNicknameWithSuffix(string baseName, string suffix)
		{
			if (baseName == null) baseName = "";
			if (suffix == null) suffix = "";

			if ((baseName + suffix).Length <= 12)
				return baseName + suffix;

			if (suffix.Length >= 12)
				return suffix.Substring(0, 12);

			int allowedBaseLength = 12 - suffix.Length;
			return baseName.Substring(0, allowedBaseLength) + suffix;
		}

		/// <summary>
		/// Evaluates the current sequence text, returns the number to use for this generation,
		/// and calculates the next sequence string to save/display.
		/// </summary>
		public static void AdvanceSequenceCounter(string currentText, int maxLength, out string numberToUse, out string nextSequenceText)
		{
			if (maxLength <= 0) maxLength = 3;
			int currentVal = 0;
			if (string.IsNullOrWhiteSpace(currentText) || !int.TryParse(currentText.Trim(), out currentVal) || currentVal < 0)
			{
				currentVal = 0;
			}
			numberToUse = currentVal.ToString();

			int nextVal = currentVal + 1;
			string nextStr = nextVal.ToString();
			if (nextStr.Length > maxLength)
			{
				nextSequenceText = "0";
			}
			else
			{
				nextSequenceText = nextStr;
			}
		}
		private void CreateNickname()
		{
			Window w = Window.Get;
			string customName = "";
			if (w != null && w.Settings_tbxCustomName != null)
			{
				WinAPI.InvokeIfRequired(w.Settings_tbxCustomName, () => {
					customName = w.Settings_tbxCustomName.Text?.Trim() ?? "";
				});
			}

			if (string.IsNullOrEmpty(customName))
			{
				string genre = "Random";
				if (w != null && w.Settings_cmbxCreateCharGenre != null)
				{
					WinAPI.InvokeIfRequired(w.Settings_cmbxCreateCharGenre, () => {
						genre = w.Settings_cmbxCreateCharGenre.Text;
					});
				}
				CreatingCharacterName = GetRandomNickname(genre);
			}
			else
			{
				string numberToUse = "0";
				if (w != null && w.Settings_tbxCustomSequence != null)
				{
					WinAPI.InvokeIfRequired(w.Settings_tbxCustomSequence, () => {
						string rawSeq = w.Settings_tbxCustomSequence.Text;
						int maxLen = w.Settings_tbxCustomSequence.MaxLength > 0 ? w.Settings_tbxCustomSequence.MaxLength : 3;
						AdvanceSequenceCounter(rawSeq, maxLen, out numberToUse, out string nextSeq);
						w.Settings_tbxCustomSequence.Text = nextSeq;
					});
				}
				if (w != null)
				{
					WinAPI.InvokeIfRequired(w, () => {
						Settings.SaveBotSettings();
					});
				}
				CreatingCharacterName = FormatNicknameWithSuffix(customName, numberToUse);
			}

			w?.Log("Checking nickname [" + CreatingCharacterName + "]");
			PacketBuilder.CheckCharacterName(CreatingCharacterName);
		}
		/// <summary>
		/// Generates a random Game of Thrones nickname with Discord style.
		/// </summary>
		public string GetRandomNickname(string nameGenre)
		{
			// List with names as maximum 8 letters!
			List<string> nicknames = new List<string>();
			// Choosing name genre
			CreatingCharacterMale = (nameGenre == "Random" ? rand.Next(100) % 2 == 0 : (nameGenre == "Male"));
			if (CreatingCharacterMale)
			{
				// Male
				nicknames.AddRange(new string[] {
					"Aegon","Aerys","Aemon","Aeron","Alliser","Areo","Artos","Alyn","Alester",
					"Bran","Bronn","Benjen","Brynden","Beric","Balon","Bowen","Brandon","Barthogan","Beron",
					"Craster","Cregan","Cregard",
					"Davos","Daario","Doran","Darrik","Dyron","Duncan",
					"Eddard","Edric","Euron","Edmure",
					"Gendry","Gilly","Gregor","Garth","Gwayne",
					"Hoster","Hardwin","Harlen",
					"Illyrio",
					"Jon","Jaime","Jorah","Joffrey","Jeor","Jaqen","Jojen","Janos","Jonnel","Jory",
					"Kevan","Karlon",
					"Lancel","Loras","Luceon","Lothar","Lyonel",
					"Maekar","Mace","Mance","Meribald","Martyn",
					"Nestor",
					"Oberyn","Osric",
					"Petyr","Podrick","Perwyn",
					"Quentyn","Qyburn",
					"Robert","Robb","Ramsay","Roose","Rickon","Rickard","Rhaegar","Renly","Rodrik","Randyll",
					"Samwell","Sandor","Stannis","Stefon","Syrio","Symond",
					"Tywin","Tyrion","Theon","Tormund","Trystane","Tommen","Thoros","Tycho","Tomard",
					"Val","Varys","Viserys","Victarion","Vimar",
					"Walder","Wyman","Walys",
					"Yoren","Yohn","Yezzan",
					"Zane",
				});
			}
			else
			{
				// Female
				nicknames.AddRange(new string[] {
					"Arya","Alys","Arianne","Asha","Alaysha","Alissa","Arby","Avelley","Anya","Amerei","Alla",
					"Brienne","Bryna",
					"Catelyn","Cersei","Carlys","Chayle",
					"Daenerys","Dorea","Dyana","Daerya","Daenya","Daella",
					"Elia","Ellaria","Evelyne","Emilee","Elaenys",
					"Haenys","Hemys",
					"Graycie","Gabielle","Genna",
					"Jeyne","Jaeneth","Jocey","Jaennis",
					"Khailey","Kathryn","Khelsie","Kiara","Kristyne",
					"Lyanna","Lysa","Loreza","Laurane",
					"Margaery","Meera","Myrcella","Maella","Mordane","Megga",
					"Nymeria","Naemys","Naesys",
					"Obara","Obella","Olenna",
					"Rina","Rhaerya","Roslin",
					"Shae","Sansa","Selyse","Shireen","Sarella","Serena","Sara",
					"Tyene",
					"Unella",
					"Valeris","Vaehra","Vhaenyra","Vaella",
					"Walda",
					"Ygritte",
				});
			}
			// Adding +10000 possibilities to every nick
			return nicknames[rand.Next(nicknames.Count)] + rand.Next(10000).ToString().PadLeft(4, '0');
		}
		private void CreateCharacter()
		{
			Window w = Window.Get;
			string CreatingCharacterRace = "CH";
			WinAPI.InvokeIfRequired(w.Settings_cmbxCreateCharRace, () => {
				CreatingCharacterRace = w.Settings_cmbxCreateCharRace.Text;
			});
			bool success = PacketBuilder.CreateCharacter(CreatingCharacterName, CreatingCharacterMale, CreatingCharacterRace);
			CreatingCharacterName = "";
			if (success)
			{
				Window.Get.LogProcess("Creating character...");
			}
		}
		/// <summary>
		/// Returns the max. member count from the current party.
		/// </summary>
		public SRTypes.Weapon GetMyWeaponType()
		{
			return InfoManager.Character?.Inventory == null || InfoManager.Character.Inventory.Capacity <= 6 || InfoManager.Character.Inventory[6] == null ? SRTypes.Weapon.None : (SRTypes.Weapon)InfoManager.Character.Inventory[6].ID4;
        }
        public SREquipable.Equipable GetMySecondaryEquipableType()
        {
			if (InfoManager.Character?.Inventory == null || InfoManager.Character.Inventory.Capacity <= 7 || InfoManager.Character.Inventory[7] == null || !InfoManager.Character.Inventory[7].isEquipable())
				return SREquipable.Equipable.Unknown;
			return ((SREquipable)InfoManager.Character.Inventory[7]).ItemType;
		}
        /// <summary>
        /// Search for specific type ID's item in the inventory. Return success.
        /// </summary>
        /// <param name="ID2">type id #2</param>
        /// <param name="ID3">type id #3</param>
        /// <param name="ID4">type id #4</param>
        /// <param name="slot">Inventory slot found</param>
        /// <param name="servername">Rule the search to contains string specified</param>
        /// <param name="excludeServername">Exclude items whose servername contains this string</param>
		public bool FindItem(byte ID2, byte ID3, byte ID4, ref byte slot, string servername = "", string excludeServername = "")
		{
			if (InfoManager.Character?.Inventory == null)
				return false;
			int index = InfoManager.Character.Inventory.FindIndex(i => i != null 
				&& i.isType(ID2, ID3, ID4) 
				&& (string.IsNullOrEmpty(servername) || i.ServerName.Contains(servername))
				&& (string.IsNullOrEmpty(excludeServername) || !i.ServerName.Contains(excludeServername))
				&& !IsItemBlockedFromUse(i), 13);
			if(index == -1)
				return false;
			else
			{
				slot = (byte)index;
				return true;
			}
		}
		/// <summary>
		/// Pot/ilaç seçimi: eşleşen tüm slotları tarar, en güçlü NORMAL potu seçer.
		/// Item Mall potu (MALL / X-large) sadece normal pot yoksa kullanılır.
		/// </summary>
		public bool FindBestItem(byte ID2, byte ID3, byte ID4, ref byte slot, string servername = "", string excludeServername = "")
		{
			var inv = InfoManager.Character == null ? null : InfoManager.Character.Inventory;
			if (inv == null)
				return false;

			int bestSlot = -1;
			bool bestMall = false;
			int bestLevel = -1;
			uint bestId = 0;
			bool hasBest = false;

			for (int s = 13; s < inv.Capacity; s++)
			{
				var it = inv[s];
				if (it == null || !it.isType(ID2, ID3, ID4))
					continue;
				if (!string.IsNullOrEmpty(servername) && (it.ServerName == null || !it.ServerName.Contains(servername)))
					continue;
				if (!string.IsNullOrEmpty(excludeServername) && it.ServerName != null && it.ServerName.Contains(excludeServername))
					continue;
				if (IsItemBlockedFromUse(it))
					continue;

				bool mall = PotionPolicy.IsMallPotion(it.ServerName, it.Name);
				if (!hasBest || PotionPolicy.ComparePotions(mall, it.LevelRequired, it.ID, bestMall, bestLevel, bestId) < 0)
				{
					hasBest = true;
					bestSlot = s;
					bestMall = mall;
					bestLevel = it.LevelRequired;
					bestId = it.ID;
				}
			}

			if (!hasBest)
				return false;
			slot = (byte)bestSlot;
			return true;
		}
		/// <summary>
		/// Try to change to clientless mode.
		/// </summary>
		public void GoClientless()
		{
			if (!Proxy.ClientlessMode)
			{
				Window w = Window.Get;
				System.Timers.Timer CloseClient = new System.Timers.Timer(1000);

				byte s = 5;
				CloseClient.Elapsed += delegate	{
					try{
						if (s > 0){
							w.LogProcess("Closing client in " + s + " seconds...");
							s = (byte)(s - 1);
						}
						else
						{
							w.LogProcess("Closing client...");
							Proxy.CloseClient();
							CloseClient.Stop();
							CloseClient.Dispose();
						}
					}
					catch (Exception ex)
					{
						Window.Get?.Log("[GoClientless] " + ex.Message);
					}
				};
				CloseClient.AutoReset = true;
				CloseClient.Start();
			}
		}
		/// <summary>
		/// Try to use a return scroll from inventory.
		/// </summary>
		public bool UseReturnScroll()
		{
			var character = InfoManager.Character;
			if (character == null || character.Inventory == null) return false;
			xList<SRItem> inventory = character.Inventory;
			for (byte j = 13; j < inventory.Capacity; j++)
			{
				if (inventory[j] != null && inventory[j].isType(3, 3, 1))
				{
					switch (inventory[j].ServerName)
					{
						case "ITEM_ETC_SCROLL_RETURN_01":
						case "ITEM_ETC_SCROLL_RETURN_02":
						case "ITEM_ETC_SCROLL_RETURN_03":
						case "ITEM_ETC_SCROLL_RETURN_NEWBIE_01":
						case "ITEM_ETC_E041225_SANTA_WINGS":
						case "ITEM_MALL_RETURN_SCROLL_HIGH_SPEED":
						case "ITEM_EVENT_RETURN_SCROLL_HIGH_SPEED":
							if (!PacketBuilder.UseItem(inventory[j], j)) return false;
							ReturnToAreaPolicy.LastRecallPosition = character.GetRealtimePosition();
							return true;
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Starts tracing a player.
		/// </summary>
		public bool StartTrace(string PlayerName)
		{
			if (InfoManager.inGame)
			{
				inTrace = true;
				SetTraceName(PlayerName);
				Window w = Window.Get;
				WinAPI.InvokeIfRequired(w.Training_btnTraceStart, () => {
					w.Training_btnTraceStart.Text = LocalizationManager.Get("UI_Tr_Stop", "STOP");
				});
				return true;
			}
			return false;
		}
		public void SetTraceName(string PlayerName)
		{
			// Normalize Key
			TracePlayerName = PlayerName.Trim().ToUpper();
			if (inTrace)
			{
				// Check if player is around and move it
				SRPlayer player = InfoManager.Players[TracePlayerName] ?? InfoManager.Players?.Find(p => p != null && string.Equals(p.Name, TracePlayerName, StringComparison.OrdinalIgnoreCase));
				if (player != null && (player.PVPCapeType != SRPlayer.PVPCape.None || player.PVPStateType != SRPlayer.PVPState.Neutral))
				{
					Window.Get?.Log("NOTE: To prevent abuse, tracing is not possible for players in PVP or in certain areas.");
					return;
				}
				if (player != null){
					MoveTo(player.GetRealtimePosition());
				}
			}
		}
		/// <summary>
		/// Try to stop the trace.
		/// </summary>
		public bool StopTrace()
		{
			if (inTrace)
			{
				inTrace = false;
				Window w = Window.Get;
				WinAPI.InvokeIfRequired(w.Training_btnTraceStart, ()=>{
					w.Training_btnTraceStart.Text = LocalizationManager.Get("UI_Tr_Start", "START");
				});
				return true;
			}
			return false;
		}
		public bool isRecordingPaused { get { return m_recordingPaused; } }
		private DateTime m_lastTraceMove = DateTime.MinValue;
		/// <summary>
		/// Trace hedefini periyodik takip eder (OnLoop ~1sn tick'ten çağrılır).
		/// Eskiden sadece isim yazıldığında tek MoveTo vardı; hedef uzaklaşınca
		/// bot bekliyordu. 20m'den uzaksa ve 2sn geçtiyse yeniden yürünür.
		/// </summary>
		public void UpdateTraceTick()
		{
			if (!inTrace || !InfoManager.inGame || string.IsNullOrEmpty(TracePlayerName))
				return;
			if ((DateTime.Now - m_lastTraceMove).TotalSeconds < 2.0)
				return;
			SRPlayer player = null;
			try { player = InfoManager.Players[TracePlayerName] ?? InfoManager.Players?.Find(p => p != null && string.Equals(p.Name, TracePlayerName, StringComparison.OrdinalIgnoreCase)); } catch { return; }
			if (player != null && (player.PVPCapeType != SRPlayer.PVPCape.None || player.PVPStateType != SRPlayer.PVPState.Neutral))
				return;
			if (player == null)
				return;
			SRCoord target = null, me = null;
			try
			{
				target = player.GetRealtimePosition();
				me = InfoManager.Character.GetRealtimePosition();
			}
			catch { return; }
			if (target == null || me == null)
				return;
			double dist;
			try { dist = me.DistanceTo(target); } catch { return; }
			if (dist > 20.0)
			{
				m_lastTraceMove = DateTime.Now;
				try { MoveTo(target); } catch { }
			}
		}
		/// <summary>
		/// Move the character to the position specified.
		/// </summary>
		public void MoveTo(SRCoord position)
		{
			MoveTo(position, null);
		}
		private void MoveTo(SRCoord position, Action<string> trace)
		{
			if (position == null)
			{
				trace?.Invoke("MoveTo not sent reason=null-position");
				return;
			}
			SRCharacter character = InfoManager.Character;
			if (character == null)
			{
				trace?.Invoke("MoveTo not sent reason=no-character");
				return;
			}
			if (character.isRiding)
				PacketBuilder.MoveTo(position, character.RidingUniqueID, trace);
			else
				PacketBuilder.MoveTo(position, 0, trace);
		}
		/// <summary>
		/// Try to use and item at the slot specified. Return success.
		/// </summary>
		public bool UseItem(byte slotInventory)
		{
			xList<SRItem> inventory = InfoManager.Character?.Inventory;
			if (inventory == null)
				return false;
			if (slotInventory >= 13 && slotInventory < inventory.Capacity)
			{
				if (inventory[slotInventory] != null)
				{
					switch (inventory[slotInventory].ID2)
					{
						case 2: // Summon scroll
							return PacketBuilder.UseItem(inventory[slotInventory], slotInventory);
						case 3: // Usable
							switch(inventory[slotInventory].ID3)
							{
								case 1: // Potions
									switch (inventory[slotInventory].ID4)
									{
										case 1: // HP
										case 3: // MP
										case 2: // Vigor
											return PacketBuilder.UseItem(inventory[slotInventory], slotInventory);
									}
									break;
								case 2: // Pills
									switch (inventory[slotInventory].ID4)
									{
										case 1: // Universal
										case 6: // Purification
											return PacketBuilder.UseItem(inventory[slotInventory], slotInventory);
									}
									break;
								case 3: // Event, vehicles, etc.
									switch (inventory[slotInventory].ID4)
									{
										case 1: // All kind of scrolls, return scrolls, even customized ones (it can cause disconnect)
										case 2: // Vehicle, Transport
										case 6: // Fortress summon pet
										case 7: // Fortress summon guard
										case 9: // Fortress battle flag
										case 10: // Exp/SP scroll
										case 11: // Fortress summon unique
										case 12: // Skill Points scroll
											return PacketBuilder.UseItem(inventory[slotInventory], slotInventory);
									}
									break;
								case 13: // Buff scroll
									return PacketBuilder.UseItem(inventory[slotInventory], slotInventory);
								case 15: // Monster scroll
									return PacketBuilder.UseItem(inventory[slotInventory], slotInventory);
							}
							break;
					}
				}
			}
			return false;
		}
		/// <summary>
		/// Try to equip or unequip an item from the inventory. Return success.
		/// </summary>
		public bool EquipItem(byte slotInventory,bool useInventoryAvatar = false)
		{
			if (InfoManager.Character?.Inventory == null || slotInventory >= InfoManager.Character.Inventory.Capacity)
				return false;
			SRItem item = InfoManager.Character.Inventory[slotInventory];
			if(item != null && item.isEquipable())
			{
				if (slotInventory < 13)
				{
					// UnEquip

					// Find an empty slot
					int newSlot = InfoManager.Character.Inventory.FindIndex(i => i == null,13);
					if (newSlot != -1){
						PacketBuilder.MoveItem(slotInventory, (byte)newSlot, useInventoryAvatar ? SRTypes.InventoryItemMovement.AvatarToInventory : SRTypes.InventoryItemMovement.InventoryToInventory);
						return true;
					}
				}
				else
				{
					// Equip
					switch ((SREquipable.Equipable)item.ID3)
					{
						case SREquipable.Equipable.Garment: // GARMENT
						case SREquipable.Equipable.Protector: // PROTECTOR
						case SREquipable.Equipable.Armor: // ARMOR
						case SREquipable.Equipable.Robe: // ROBE
						case SREquipable.Equipable.LightArmor: // LIGHT ARMOR
						case SREquipable.Equipable.HeavyArmor: // HEAVY ARMOR
							switch ((SRTypes.SetPart)item.ID4)
							{
								case SRTypes.SetPart.Head: // HEAD
									PacketBuilder.MoveItem(slotInventory, 0, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
								case SRTypes.SetPart.Shoulders: // SHOULDERS
									PacketBuilder.MoveItem(slotInventory, 2, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
								case SRTypes.SetPart.Chest: // CHEST
									PacketBuilder.MoveItem(slotInventory, 1, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
								case SRTypes.SetPart.Pants: // PANTS
									PacketBuilder.MoveItem(slotInventory, 4, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
								case SRTypes.SetPart.Gloves: // GLOVES
									PacketBuilder.MoveItem(slotInventory, 3, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
								case SRTypes.SetPart.Boots: // BOOTS
									PacketBuilder.MoveItem(slotInventory, 5, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
							}
							break;
						case SREquipable.Equipable.Shield:
							PacketBuilder.MoveItem(slotInventory, 7, SRTypes.InventoryItemMovement.InventoryToInventory);
							return true;
						case SREquipable.Equipable.AccesoriesCH: // ACCESSORIES (CH)
						case SREquipable.Equipable.AccesoriesEU: // ACCESSORIES (EU)
							switch ((SRTypes.AccesoriesPart)item.ID4)
							{
								case SRTypes.AccesoriesPart.Earring: // Earring
									PacketBuilder.MoveItem(slotInventory, 9, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
								case SRTypes.AccesoriesPart.Necklace: // Necklace
									PacketBuilder.MoveItem(slotInventory, 10, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
								case SRTypes.AccesoriesPart.Ring: // Ring
									if(InfoManager.Character.Inventory[12] == null)
										PacketBuilder.MoveItem(slotInventory, 12, SRTypes.InventoryItemMovement.InventoryToInventory);
									else
										PacketBuilder.MoveItem(slotInventory, 11, SRTypes.InventoryItemMovement.InventoryToInventory);
									return true;
							}
							break;
						case SREquipable.Equipable.Weapon: // WEAPONS (CH & EU)
							PacketBuilder.MoveItem(slotInventory, 6, SRTypes.InventoryItemMovement.InventoryToInventory);
							return true;
						case SREquipable.Equipable.Job: // JOB SUIT
							PacketBuilder.MoveItem(slotInventory, 8, SRTypes.InventoryItemMovement.InventoryToInventory);
							return true;
						case SREquipable.Equipable.Avatar: // Avatar
							switch ((SRTypes.AvatarPart)item.ID4)
							{
								case SRTypes.AvatarPart.Hat: // Hat
									PacketBuilder.MoveItem(slotInventory, 0, SRTypes.InventoryItemMovement.InventoryToAvatar);
									return true;
								case SRTypes.AvatarPart.Dress: // Dress
									PacketBuilder.MoveItem(slotInventory, 1, SRTypes.InventoryItemMovement.InventoryToAvatar);
									return true;
								case SRTypes.AvatarPart.Accessory: // Accessory
									PacketBuilder.MoveItem(slotInventory, 2, SRTypes.InventoryItemMovement.InventoryToAvatar);
									return true;
								case SRTypes.AvatarPart.Flag: // Flag
									PacketBuilder.MoveItem(slotInventory, 3, SRTypes.InventoryItemMovement.InventoryToAvatar);
									return true;
							}
							break;
						case SREquipable.Equipable.DevilSpirit: // Devil Spirit
							PacketBuilder.MoveItem(slotInventory, 4, SRTypes.InventoryItemMovement.InventoryToAvatar);
							return true;
					}
				}
			}
			return false;
		}
		public void UseTeleportAsync(SRTeleport teleport, uint destinationID)
		{
			if (Proxy.ClientlessMode)
			{
				if (InfoManager.isEntityNear(teleport.UniqueID))
					PacketBuilder.UseTeleport(teleport.UniqueID, destinationID);
				else
					Window.Get.LogProcess(teleport.Name + " cannot be selected!");
			}
			else
			{
			  Thread teleportThread = new Thread( ()=> {
					if (WaitSelectEntity(teleport.UniqueID, 8, 250, "Selecting teleport " + teleport.TeleportName + "..."))
						PacketBuilder.UseTeleport(teleport.UniqueID, destinationID);
					else
						Window.Get.LogProcess(teleport.Name + " cannot be selected!");
				});
				teleportThread.IsBackground = true;
				teleportThread.Start();
			}
		}

		public bool StartInventorySort()
		{
			lock (m_sortingLock)
			{
				if (!InfoManager.inGame || InfoManager.inTeleport || isSorting)
					return false;

				m_stopSortingRequested = false;
				int sessionId = ++m_sortingSessionId;
				tSorting = new Thread(() => InventorySort(sessionId));
				tSorting.IsBackground = true;
				tSorting.Priority = ThreadPriority.BelowNormal;
				tSorting.Start();

				Window w = Window.Get;
				w?.Inventory_btnItemsSort?.BeginInvokeIfRequired(() => {
					w.Inventory_btnItemsSort.Tag = w.Inventory_btnItemsSort.ForeColor;
					w.Inventory_btnItemsSort.ForeColor = System.Drawing.Color.FromArgb(0, 180, 255);
				});
				return true;
			}
		}
		public void InventorySort()
		{
			int sessionId;
			lock (m_sortingLock)
			{
				sessionId = m_sortingSessionId;
			}
			InventorySort(sessionId);
		}
		private void InventorySort(int sessionId)
		{
			Window w = Window.Get;

			bool sort = true;
			try {
			while (InfoManager.inGame && !InfoManager.inTeleport && sort && !m_stopSortingRequested && m_sortingSessionId == sessionId)
			{
				sort = false;
				xList<SRItem> inventory = InfoManager.Character?.Inventory;
				if (inventory == null)
					break;
				for (byte j = (byte)(inventory.Capacity - 1); j >= 13; j--)
				{
					if (m_stopSortingRequested || m_sortingSessionId != sessionId || !InfoManager.inGame || InfoManager.inTeleport)
						break;

					if (inventory[j] != null && inventory[j].QuantityMax != 1)
					{
						ushort quantityInitial = inventory[j].Quantity;
						ushort quantityMax = inventory[j].QuantityMax;
						if (quantityInitial < quantityMax)
						{
							for (byte k = 13; k < j; k++)
							{
								if (m_stopSortingRequested || m_sortingSessionId != sessionId || !InfoManager.inGame || InfoManager.inTeleport)
									break;

								// Just in case
								if (inventory[j] == null)
									break;

								if (inventory[k] != null)
								{
									if (inventory[k].ID == inventory[j].ID)
									{
										ushort quantityFinal = inventory[k].Quantity;
										if (quantityFinal < quantityMax)
										{
											w?.LogProcess("Sorting (" + j + ") -> (" + k + ") ...");

											ushort quantityMaxMoved = (ushort)(quantityMax - quantityFinal);
											bool sent;
											if (quantityInitial <= quantityMaxMoved)
												sent = PacketBuilder.MoveItem(j, k, SRTypes.InventoryItemMovement.InventoryToInventory, quantityInitial);
											else
												sent = PacketBuilder.MoveItem(j++, k, SRTypes.InventoryItemMovement.InventoryToInventory, quantityMaxMoved);
											if (!sent)
											{
												sort = false;
												return;
											}
											sort = true;
											InfoManager.MonitorInventoryMovement.WaitOne(1000);
											break;
										}
									}
								}
							}
							if (m_stopSortingRequested || m_sortingSessionId != sessionId || !InfoManager.inGame || InfoManager.inTeleport)
								break;
						}
					}
				}
			}

			}
			catch (Exception ex)
			{
				w?.LogProcess("Sorting error: " + ex.Message);
			}
			finally {
			w?.Inventory_btnItemsSort?.BeginInvokeIfRequired(() => {
				if (w.Inventory_btnItemsSort.Tag != null) w.Inventory_btnItemsSort.ForeColor = (System.Drawing.Color)w.Inventory_btnItemsSort.Tag;
				w.Inventory_btnItemsSort.Tag = null;
			});

			w?.LogProcess("Sorting completed");

			lock (m_sortingLock)
			{
				if (tSorting == Thread.CurrentThread)
				{
					tSorting = null;
				}
			}
			/* sorting done */ }
		}
		public bool StopInventorySort()
		{
			Thread workerToJoin = null;
			lock (m_sortingLock)
			{
				if (tSorting == null || !tSorting.IsAlive)
					return false;

				m_stopSortingRequested = true;
				m_sortingSessionId++;
				try { InfoManager.MonitorInventoryMovement.Set(); } catch { }
				workerToJoin = tSorting;
			}

			if (workerToJoin != null && workerToJoin.IsAlive && workerToJoin != Thread.CurrentThread)
			{
				try { workerToJoin.Join(250); } catch { }
			}

			lock (m_sortingLock)
			{
				if (workerToJoin != null && !workerToJoin.IsAlive && tSorting == workerToJoin)
				{
					tSorting = null;
				}
			}

			Window w = Window.Get;
			w?.LogProcess("Sorting stopped");
			w?.Inventory_btnItemsSort?.BeginInvokeIfRequired(() => {
				if (w.Inventory_btnItemsSort.Tag != null) w.Inventory_btnItemsSort.ForeColor = (System.Drawing.Color)w.Inventory_btnItemsSort.Tag;
				w.Inventory_btnItemsSort.Tag = null;
			});
			w?.Inventory_btnStorageSort?.BeginInvokeIfRequired(() => {
				if (w.Inventory_btnStorageSort.Tag != null)
				{
					w.Inventory_btnStorageSort.ForeColor = (System.Drawing.Color)w.Inventory_btnStorageSort.Tag;
					w.Inventory_btnStorageSort.Tag = null;
				}
			});
			return true;
		}
		public bool StopStorageSort()
		{
			return StopInventorySort();
		}
		/// <summary>
		/// Depodaki bölünebilir eşyaları birleştirir (InventorySort'un depo karşılığı).
		/// UI'daki Inventory_btnStorageSort butonunun backend'idir.
		/// </summary>
		public bool StartStorageSort()
		{
			lock (m_sortingLock)
			{
				if (!InfoManager.inGame || InfoManager.inTeleport || isSorting || InfoManager.Character?.Storage == null)
					return false;

				m_stopSortingRequested = false;
				int sessionId = ++m_sortingSessionId;
				tSorting = new Thread(() => StorageSort(sessionId));
				tSorting.IsBackground = true;
				tSorting.Priority = ThreadPriority.BelowNormal;
				tSorting.Start();

				Window w = Window.Get;
				w?.Inventory_btnStorageSort?.BeginInvokeIfRequired(() => {
					w.Inventory_btnStorageSort.Tag = w.Inventory_btnStorageSort.ForeColor;
					w.Inventory_btnStorageSort.ForeColor = System.Drawing.Color.FromArgb(0, 180, 255);
				});
				return true;
			}
		}
		public void StorageSort()
		{
			int sessionId;
			lock (m_sortingLock)
			{
				sessionId = m_sortingSessionId;
			}
			StorageSort(sessionId);
		}
		private void StorageSort(int sessionId)
		{
			Window w = Window.Get;
			bool sort = true;
			try {
			while (InfoManager.inGame && !InfoManager.inTeleport && sort && !m_stopSortingRequested && m_sortingSessionId == sessionId)
			{
				sort = false;
				xList<SRItem> storage = InfoManager.Character?.Storage;
				if (storage == null)
					break;
				for (int j = storage.Capacity - 1; j >= 0; j--)
				{
					if (m_stopSortingRequested || m_sortingSessionId != sessionId || !InfoManager.inGame || InfoManager.inTeleport)
						break;

					if (storage[j] != null && storage[j].QuantityMax != 1)
					{
						ushort quantityInitial = storage[j].Quantity;
						ushort quantityMax = storage[j].QuantityMax;
						if (quantityInitial < quantityMax)
						{
							for (int k = 0; k < j; k++)
							{
								if (m_stopSortingRequested || m_sortingSessionId != sessionId || !InfoManager.inGame || InfoManager.inTeleport)
									break;

								if (storage[j] == null)
									break;
								if (storage[k] != null && storage[k].ID == storage[j].ID)
								{
									ushort quantityFinal = storage[k].Quantity;
									if (quantityFinal < quantityMax)
									{
										w?.LogProcess("Sorting storage (" + j + ") -> (" + k + ") ...");
										ushort quantityMaxMoved = (ushort)(quantityMax - quantityFinal);
										byte from = (byte)j, to = (byte)k;
										bool sent;
										if (quantityInitial <= quantityMaxMoved)
											sent = PacketBuilder.MoveItem(from, to, SRTypes.InventoryItemMovement.StorageToStorage, quantityInitial);
										else
										{
											sent = PacketBuilder.MoveItem(from, to, SRTypes.InventoryItemMovement.StorageToStorage, quantityMaxMoved);
											j++;
										}
										if (!sent)
										{
											sort = false;
											return;
										}
										sort = true;
										InfoManager.MonitorInventoryMovement.WaitOne(1000);
										break;
									}
								}
							}
							if (m_stopSortingRequested || m_sortingSessionId != sessionId || !InfoManager.inGame || InfoManager.inTeleport)
								break;
						}
					}
				}
			}
			}
			catch (Exception ex)
			{
				w?.LogProcess("Storage sorting error: " + ex.Message);
			}
			finally {
			w?.Inventory_btnStorageSort?.BeginInvokeIfRequired(() => {
				if (w.Inventory_btnStorageSort.Tag != null)
				{
					w.Inventory_btnStorageSort.ForeColor = (System.Drawing.Color)w.Inventory_btnStorageSort.Tag;
					w.Inventory_btnStorageSort.Tag = null;
				}
			});
			w?.LogProcess("Storage sorting completed");

			lock (m_sortingLock)
			{
				if (tSorting == Thread.CurrentThread)
				{
					tSorting = null;
				}
			}
			/* sorting done */ }
		}
		/// <summary>
		/// Yürüme kaydı (Training > Script sekmesindeki START/PAUSE/STOP).
		/// Konum her ~1.2sn'de örneklenir, 8m'den fazla hareket varsa
		/// "MOVE,Region,X,Y,Z" satırı Training_rtbxRecordOutput'a eklenir.
		/// STOP'ta kayıt Scripts\\record_*.txt dosyasına yazılır.
		/// </summary>
		public static event Action<string> OnRecordLineAppended;

		public bool StartRecording()
		{
			if (isRecording || !InfoManager.inGame)
				return false;
			m_stopRecordingRequested = false;
			m_recordingPaused = false;
			lock (m_recordedLines) { m_recordedLines.Clear(); }
			isRecording = true;
			tRecording = new Thread(RecordingLoop);
			tRecording.IsBackground = true;
			tRecording.Priority = ThreadPriority.BelowNormal;
			tRecording.Start();
			Window w = Window.Get;
			w.Log("Walk kaydı başladı (8m adım aralığı).");
			WinAPI.InvokeIfRequired(w.Training_btnRecordStartStop, () => { w.Training_btnRecordStartStop.Text = LocalizationManager.Get("UI_Tr_Stop", "STOP"); });
			WinAPI.InvokeIfRequired(w.Training_btnRecordPause, () => { w.Training_btnRecordPause.Enabled = true; w.Training_btnRecordPause.Text = LocalizationManager.Get("UI_Tr_Pause", "PAUSE"); });
			return true;
		}
		public void PauseResumeRecording()
		{
			if (!isRecording)
				return;
			m_recordingPaused = !m_recordingPaused;
			Window w = Window.Get;
			bool paused = m_recordingPaused;
			WinAPI.InvokeIfRequired(w.Training_btnRecordPause, () => { w.Training_btnRecordPause.Text = paused ? LocalizationManager.Get("UI_Tr_Resume", "RESUME") : LocalizationManager.Get("UI_Tr_Pause", "PAUSE"); });
			w.Log(paused ? "Walk kaydı duraklatıldı." : "Walk kaydı devam ediyor.");
		}
		public void StopRecording()
		{
			if (!isRecording)
				return;
			m_stopRecordingRequested = true;
			Thread t = tRecording;
			tRecording = null;
			try { if (t != null && t.IsAlive && t != Thread.CurrentThread) t.Join(1500); } catch { }
			isRecording = false;
			Window w = Window.Get;
			string path = "";
			try
			{
				if (!Directory.Exists("Scripts"))
					Directory.CreateDirectory("Scripts");
				path = "Scripts\\record_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
				string[] lines;
				lock (m_recordedLines) { lines = m_recordedLines.ToArray(); }
				File.WriteAllLines(path, lines);
				w.Log("Walk kaydı durdu: " + lines.Length + " nokta -> " + path);
			}
			catch (Exception ex)
			{
				w.Log("Walk kaydı yazılamadı: " + ex.Message);
			}
			WinAPI.InvokeIfRequired(w.Training_btnRecordStartStop, () => { w.Training_btnRecordStartStop.Text = LocalizationManager.Get("UI_Tr_Start", "START"); });
			WinAPI.InvokeIfRequired(w.Training_btnRecordPause, () => { w.Training_btnRecordPause.Enabled = false; w.Training_btnRecordPause.Text = LocalizationManager.Get("UI_Tr_Pause", "PAUSE"); });
		}
		private void RecordingLoop()
		{
			Window w = Window.Get;
			SRCoord last = null;
			try { last = InfoManager.Character.GetRealtimePosition(); } catch { }
			if (last != null)
				AppendRecordLine(w, last);
			while (!m_stopRecordingRequested)
			{
				try { Thread.Sleep(1200); } catch { break; }
				if (m_stopRecordingRequested)
					break;
				if (m_recordingPaused || !InfoManager.inGame)
					continue;
				SRCoord cur = null;
				try { cur = InfoManager.Character.GetRealtimePosition(); } catch { continue; }
				if (cur == null || last == null)
				{
					last = cur;
					continue;
				}
				double dist;
				try { dist = cur.DistanceTo(last); } catch { continue; }
				if (dist >= 8.0)
				{
					last = cur;
					AppendRecordLine(w, cur);
				}
			}
		}
		private void AppendRecordLine(Window w, SRCoord pos)
		{
			try
			{
				string line = "MOVE," + pos.Region + "," + pos.X + "," + pos.Y + "," + pos.Z;
				lock (m_recordedLines) { m_recordedLines.Add(line); }
				try { if (w != null) w.Training_RecordAppend(line); } catch { }
				try { OnRecordLineAppended?.Invoke(line); } catch { }
			}
			catch { }
		}

		#endregion
	}
}
