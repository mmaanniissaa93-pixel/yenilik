using SecurityAPI;
using System;
using System.Collections.Generic;
using xBot.App;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;
using xBot.Game.Objects.Party;
using xBot.Network;

namespace xBot.Game
{
	public static class PacketBuilder
	{
		public static void Login(string username, string password, ushort serverID)
		{
			Packet p = new Packet(Gateway.Opcode.CLIENT_LOGIN_REQUEST, true);
			p.WriteByte(DataManager.Locale);
			p.WriteAscii(username);
			p.WriteAscii(password);
			p.WriteUShort(serverID);
			Bot.Get.Proxy.Gateway.InjectToServer(p);
		}
		public static void RequestCharacterList()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_SELECTION_ACTION_REQUEST);
			p.WriteByte(SRTypes.CharacterSelectionAction.List);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SelectCharacter(string charname, int delay = 0)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_SELECTION_JOIN_REQUEST);
			p.WriteAscii(charname);
			Bot.Get.Proxy.Agent.InjectToServer(p, delay);
		}
		public static void SendSecondaryPasscode(string passcode, int delay = 0)
		{
			if (string.IsNullOrWhiteSpace(passcode))
				return;
			Packet p = new Packet(Agent.Opcode.CLIENT_SECONDARY_PASSCODE);
			p.WriteAscii(passcode.Trim());
			Bot.Get.Proxy.Agent.InjectToServer(p, delay);
		}
		public static void FuseItem(byte targetSlot, byte elixirSlot, byte powderSlot = 0xFF)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_ALCHEMY);
			p.WriteByte(2); // Action: Fuse/Reinforce
			p.WriteByte(targetSlot);
			p.WriteByte(elixirSlot);
			if (powderSlot != 0xFF)
			{
				p.WriteByte(powderSlot);
			}
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SubmitCaptcha(string code)
		{
			if (string.IsNullOrWhiteSpace(code))
				return;
			Packet p = new Packet(Gateway.Opcode.CLIENT_CAPTCHA_SOLVED_REQUEST, true);
			p.WriteAscii(code.Trim());
			Bot.Get.Proxy.Gateway.InjectToServer(p);
			App.Window.Get?.Log("Captcha cevabı gönderildi.");
		}
		public static void DeleteCharacter(string charname)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_SELECTION_ACTION_REQUEST);
			p.WriteSByte(SRTypes.CharacterSelectionAction.Delete);
			p.WriteAscii(charname);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CheckCharacterName(string charname)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_SELECTION_ACTION_REQUEST);
			p.WriteSByte(SRTypes.CharacterSelectionAction.CheckName);
			p.WriteAscii(charname);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		/// <summary>
		/// srodevs-docs 0x7007 Restore(5): silinmekte olan karakteri geri yükler.
		/// </summary>
		public static void RestoreCharacter(string charname)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_SELECTION_ACTION_REQUEST);
			p.WriteByte(SRTypes.CharacterSelectionAction.Restore);
			p.WriteAscii(charname);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		/// <summary>
		/// srodevs-docs GATEWAY_NOTICE_REQ (0x6104).
		/// </summary>
		public static void RequestNotice()
		{
			Packet p = new Packet(Gateway.Opcode.CLIENT_NOTICE_REQUEST, true);
			p.WriteByte(DataManager.Locale);
			Bot.Get.Proxy.Gateway.InjectToServer(p);
		}
		/// <summary>
		/// srodevs-docs GATEWAY_SHARD_LIST_PING_REQ (0x6106).
		/// </summary>
		public static void RequestShardListPing()
		{
			Packet p = new Packet(Gateway.Opcode.CLIENT_SHARD_LIST_PING_REQUEST, true);
			Bot.Get.Proxy.Gateway.InjectToServer(p);
		}
		public static bool CreateCharacter(string charname, bool male, string type = "CH")
		{
			uint model, chest, legs, shoes, weapon;
			try {
				if (type == "CH")
				{
					if (male)
					{
						model = uint.Parse(DataManager.GetModelData("CHAR_CH_MAN_ADVENTURER")["id"]);
						chest = uint.Parse(DataManager.GetItemData("ITEM_CH_M_LIGHT_01_BA_A_DEF")["id"]);
						legs = uint.Parse(DataManager.GetItemData("ITEM_CH_M_LIGHT_01_LA_A_DEF")["id"]);
						shoes = uint.Parse(DataManager.GetItemData("ITEM_CH_M_LIGHT_01_FA_A_DEF")["id"]);
						weapon = uint.Parse(DataManager.GetItemData("ITEM_CH_SWORD_01_A_DEF")["id"]);
					}
					else
					{
						model = uint.Parse(DataManager.GetModelData("CHAR_CH_WOMAN_ADVENTURER")["id"]);
						chest = uint.Parse(DataManager.GetItemData("ITEM_CH_W_LIGHT_01_BA_A_DEF")["id"]);
						legs = uint.Parse(DataManager.GetItemData("ITEM_CH_W_LIGHT_01_LA_A_DEF")["id"]);
						shoes = uint.Parse(DataManager.GetItemData("ITEM_CH_W_LIGHT_01_FA_A_DEF")["id"]);
						weapon = uint.Parse(DataManager.GetItemData("ITEM_CH_SWORD_01_A_DEF")["id"]);
					}
				}
				else
				{
					if (male)
					{
						model = uint.Parse(DataManager.GetModelData("CHAR_EU_MAN_NOBLE")["id"]);
						chest = uint.Parse(DataManager.GetItemData("ITEM_EU_M_LIGHT_01_BA_A_DEF")["id"]);
						legs = uint.Parse(DataManager.GetItemData("ITEM_EU_M_LIGHT_01_LA_A_DEF")["id"]);
						shoes = uint.Parse(DataManager.GetItemData("ITEM_EU_M_LIGHT_01_FA_A_DEF")["id"]);
						weapon = uint.Parse(DataManager.GetItemData("ITEM_EU_SWORD_01_A_DEF")["id"]);
					}
					else
					{
						model = uint.Parse(DataManager.GetModelData("CHAR_EU_WOMAN_NOBLE")["id"]);
						chest = uint.Parse(DataManager.GetItemData("ITEM_EU_W_LIGHT_01_BA_A_DEF")["id"]);
						legs = uint.Parse(DataManager.GetItemData("ITEM_EU_W_LIGHT_01_LA_A_DEF")["id"]);
						shoes = uint.Parse(DataManager.GetItemData("ITEM_EU_W_LIGHT_01_FA_A_DEF")["id"]);
						weapon = uint.Parse(DataManager.GetItemData("ITEM_EU_SWORD_01_A_DEF")["id"]);
					}
				}
			}
			catch
			{
				Window.Get.Log("Error loading data...");
				return false;
			}
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_SELECTION_ACTION_REQUEST);
			p.WriteByte(SRTypes.CharacterSelectionAction.Create);
			p.WriteAscii(charname);
			p.WriteUInt(model);
			p.WriteByte(0); // Scale
			p.WriteUInt(chest);
			p.WriteUInt(legs);
			p.WriteUInt(shoes);
			p.WriteUInt(weapon);
			Bot.Get.Proxy.Agent.InjectToServer(p);
			return true;
		}
		public static void SendChatAll(string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHAT_REQUEST);
			p.WriteByte(SRTypes.Chat.All);
			p.WriteByte(Window.Get.Chat_rtbxAll.Lines.Length); // Client chat current index
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendChatPrivate(string player, string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHAT_REQUEST);
			p.WriteByte(SRTypes.Chat.Private);
			p.WriteByte(Window.Get.Chat_rtbxPrivate.Lines.Length);
			p.WriteAscii(player);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendChatParty(string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHAT_REQUEST);
			p.WriteByte(SRTypes.Chat.Party);
			p.WriteByte(Window.Get.Chat_rtbxParty.Lines.Length);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendChatGuild(string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHAT_REQUEST);
			p.WriteByte(SRTypes.Chat.Guild);
			p.WriteByte(Window.Get.Chat_rtbxGuild.Lines.Length);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendChatUnion(string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHAT_REQUEST);
			p.WriteByte(SRTypes.Chat.Union);
			p.WriteByte(Window.Get.Chat_rtbxUnion.Lines.Length);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendChatAcademy(string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHAT_REQUEST);
			p.WriteByte(SRTypes.Chat.Academy);
			p.WriteByte(Window.Get.Chat_rtbxAcademy.Lines.Length);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendChatStall(string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHAT_REQUEST);
			p.WriteByte(SRTypes.Chat.Stall);
			p.WriteByte(Window.Get.Chat_rtbxStall.Lines.Length);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendChatGlobal(byte slotGlobal,SRItem item,string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_USE, true);
			p.WriteByte(slotGlobal);
			p.WriteUShort(item.GetUsageType());
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SendMail(string title, string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_MAIL_SEND_REQUEST);
			p.WriteAscii(title);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void MoveTo(ushort region, int x, int y, int z, uint petUniqueID = 0u)
		{
			if (Bot.Get.Proxy == null || Bot.Get.Proxy.Agent == null) return;
			Packet p;
			if (petUniqueID == 0)
			{
				p = new Packet(Agent.Opcode.CLIENT_CHARACTER_MOVEMENT);
			}
			else
			{
				p = new Packet(Agent.Opcode.CLIENT_CHARACTER_PET_ACTION);
				p.WriteUInt(petUniqueID);
				p.WriteByte(SRCoService.Action.Movement); // Action
			}
			p.WriteByte(1); // Movement Type
			p.WriteUShort(region);
			if (region >= short.MaxValue)
			{
				p.WriteInt(x);
				p.WriteInt(z);
				p.WriteInt(y);
			}
			else
			{
				p.WriteUShort((ushort)x);
				p.WriteUShort((ushort)z);
				p.WriteUShort((ushort)y);
			}
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void MoveTo(SRCoord position, uint petUniqueID = 0u)
		{
			if (position == null) return;
			int z = position.Z;
			if (z == 0 && InfoManager.Character != null && InfoManager.Character.Position != null)
				z = InfoManager.Character.Position.Z;
			MoveTo(position.Region, position.X, position.Y, z, petUniqueID);
		}
		public static void AddStatPointINT()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_ADD_INT_REQUEST);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void AddStatPointSTR()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_ADD_STR_REQUEST);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void PlayerPetitionResponse(bool accept, SRTypes.PlayerPetition type)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PLAYER_INVITATION_RESPONSE);
			if (accept)
			{
				p.WriteByte(1);
				p.WriteByte(1);
			}
			else
			{
				switch (type)
				{
					case SRTypes.PlayerPetition.PartyInvitation:
					case SRTypes.PlayerPetition.PartyCreation:
						p.WriteByte(2);
						p.WriteByte(0xC);
						p.WriteByte(0x2C);
						break;
					default:
						p.WriteByte(1);
						p.WriteByte(0);
						break;
				}
			}
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void InviteToExchange(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_EXCHANGE_INVITATION_REQUEST);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CreateParty(uint uniqueID, SRParty.Setup setup)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_CREATION_REQUEST);
			p.WriteUInt(uniqueID);
			p.WriteByte(setup);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void InviteToParty(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_INVITATION_REQUEST);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void BanFromParty(uint joinID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_KICK_REQUEST);
			p.WriteUInt(joinID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void LeaveParty()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_LEAVE);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RequestPartyMatch(byte pageIndex = 0)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_MATCH_LIST_REQUEST);
			p.WriteByte(pageIndex);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CreatePartyMatch(SRPartyMatch match)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_MATCH_CREATION_REQUEST);
			p.WriteUInt(0); // Party number
			p.WriteUInt(0); // Unknown 
			p.WriteByte((byte)match.Setup);
			p.WriteByte((byte)match.Purpose);
			p.WriteByte(match.LevelMin);
			p.WriteByte(match.LevelMax);
			p.WriteAscii(match.Title);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RemovePartyMatch(uint number)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_MATCH_DELETE_REQUEST);
			p.WriteUInt(number);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void JoinToPartyMatch(uint number)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_MATCH_JOIN_REQUEST);
			p.WriteUInt(number);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void PartyMatchJoinResponse(uint requestID, uint joinID, bool accept)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PARTY_MATCH_JOIN_RESPONSE);
			p.WriteUInt(requestID);
			p.WriteUInt(joinID);
			p.WriteByte(accept ? 1 : 0);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SelectEntity(uint uniqueID)
		{
			if (uniqueID == 0 || Bot.Get.Proxy == null || Bot.Get.Proxy.Agent == null) return;
			Packet p = new Packet(Agent.Opcode.CLIENT_ENTITY_SELECTION);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void TalkNPC(uint uniqueID,byte talkID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_ENTITY_TALK_REQUEST);
			p.WriteUInt(uniqueID);
			p.WriteByte(talkID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void BuyNPC(uint uniqueID,byte tab,byte slot,ushort quantity)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_ENTITY_TALK_REQUEST);
			p.WriteByte(8); // unknown
			p.WriteByte(tab);
			p.WriteByte(slot);
			p.WriteUShort(quantity);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CloseNPC(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_NPC_CLOSE_REQUEST);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		private static readonly Dictionary<uint, ushort> s_learnedUsage = new Dictionary<uint, ushort>();
		private static readonly object s_learnedUsageLock = new object();
		private static bool s_learnedUsageLoaded = false;
		private const string LearnedUsageFile = "Config\\LearnedUsage.json";
		// itemID -> pes pese non-cooldown reject sayisi (parity ile bit0 alternatifi denenir)
		private static readonly Dictionary<uint, int> s_usageRejectCount = new Dictionary<uint, int>();
		private static uint s_lastSentItemId;
		private static ushort s_lastSentUsage;
		private static long s_lastLearnedSaveTick = 0;
		private static volatile bool s_learnedSavePending;
		/// <summary>
		/// Gercek clientin 0x704C paketinden itemID->usage ogren (Sevar gibi custom
		/// TID'li serverlarda botun DB'den hesapladigi usage tutmayabilir).
		/// Ogrenilen deger diske yazilir, oturumlar arasi korunur.
		/// </summary>
		public static void LearnItemUsage(uint itemId, ushort usage)
		{
			if (itemId == 0) return;
			bool changed = false;
			lock (s_learnedUsageLock)
			{
				EnsureLearnedUsageLoaded();
				ushort prev;
				if (!s_learnedUsage.TryGetValue(itemId, out prev) || prev != usage)
				{
					s_learnedUsage[itemId] = usage;
					changed = true;
				}
				s_usageRejectCount.Remove(itemId);
			}
			if (changed)
			{
				Window.Get?.Log($"[Item] Ogrenildi: itemId={itemId} usage=0x{usage:X4} (kalici)");
				SaveLearnedUsage();
				if (s_learnedSavePending)
				{
					System.Threading.ThreadPool.QueueUserWorkItem(delegate {
						System.Threading.Thread.Sleep(6000);
						s_learnedSavePending = false;
						SaveLearnedUsage();
					});
				}
			}
		}
		/// <summary>
		/// Reddedilen (cooldown dışı) usage yanlış demektir: kalıcı öğrenmeyi sil ki
		/// bir sonraki deneme hesaplanan/parity değere düşsün. Yanlış değerde takılı
		/// kalma (sürekli aynı baytı gönderip 0x0003 yeme) böyle çözülür.
		/// </summary>
		public static void ForgetLearnedUsage(uint itemId)
		{
			if (itemId == 0) return;
			bool removed = false;
			lock (s_learnedUsageLock)
			{
				EnsureLearnedUsageLoaded();
				removed = s_learnedUsage.Remove(itemId);
			}
			if (removed)
			{
				Window.Get?.Log($"[Item] Yanlış öğrenme silindi: itemId={itemId} (parity ile alternatif denenecek)");
				SaveLearnedUsage();
			}
		}
		/// <summary>
		/// 0xB04C success: calisan usage'i kalici ogren (gonderilen degerle).
		/// </summary>
		public static void ConfirmLastSentUsage(uint itemId)
		{
			uint sentId;
			ushort sentUsage;
			lock (s_learnedUsageLock)
			{
				sentId = s_lastSentItemId;
				sentUsage = s_lastSentUsage;
			}
			if (itemId != 0 && sentId == itemId)
				LearnItemUsage(itemId, sentUsage);
			else if (itemId != 0)
			{
				lock (s_learnedUsageLock)
				{
					EnsureLearnedUsageLoaded();
					s_usageRejectCount.Remove(itemId);
				}
			}
		}
		/// <summary>
		/// Cooldown disi reject: parity sayacini artir. Tek sayida ResolveUsage
		/// bit0 cevrilmis alternatifi dener (Sevar potion usage 0x..ED vs 0x..EC).
		/// Hangi varyant dogruysa o tutar, sonrasi kalici ogrenilir.
		/// </summary>
		public static void NoteUsageReject(uint itemId)
		{
			if (itemId == 0) return;
			lock (s_learnedUsageLock)
			{
				EnsureLearnedUsageLoaded();
				if (s_learnedUsage.ContainsKey(itemId))
					return;
				int cur;
				s_usageRejectCount.TryGetValue(itemId, out cur);
				s_usageRejectCount[itemId] = cur + 1;
			}
		}
		private static ushort ResolveUsage(uint itemId, ushort computed)
		{
			if (itemId == 0) return computed;
			lock (s_learnedUsageLock)
			{
				EnsureLearnedUsageLoaded();
				ushort learned;
				bool has = s_learnedUsage.TryGetValue(itemId, out learned);
				int rejects;
				s_usageRejectCount.TryGetValue(itemId, out rejects);
				return PotionPolicy.ResolveUsageWithFallback(computed, rejects, has, learned);
			}
		}
		private static void EnsureLearnedUsageLoaded()
		{
			lock (s_learnedUsageLock)
			{
				if (s_learnedUsageLoaded)
					return;
				s_learnedUsageLoaded = true;
				try
				{
					if (!System.IO.File.Exists(LearnedUsageFile))
						return;
					Newtonsoft.Json.Linq.JObject root = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(LearnedUsageFile));
					Newtonsoft.Json.Linq.JObject items = root["Items"] as Newtonsoft.Json.Linq.JObject;
					if (items == null)
						return;
					foreach (var kv in items)
					{
						uint id;
						int usage;
						if (uint.TryParse(kv.Key, out id) && int.TryParse(kv.Value.ToString(), out usage))
							s_learnedUsage[id] = (ushort)usage;
					}
					if (s_learnedUsage.Count > 0)
						Window.Get?.Log($"[Item] Ogrenilmis usage bilgisi yuklendi ({s_learnedUsage.Count} kayit).");
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("[PacketBuilder.EnsureLearnedUsageLoaded] " + ex.Message);
				}
			}
		}
		private static void SaveLearnedUsage()
		{
			// Debounce: potion akışında her öğrenmede disk yazma
			long now = System.Environment.TickCount;
			if (now - s_lastLearnedSaveTick < 5000)
			{
				s_learnedSavePending = true;
				return;
			}
			s_lastLearnedSaveTick = now;
			s_learnedSavePending = false;
			try
			{
				Dictionary<uint, ushort> snap;
				lock (s_learnedUsageLock)
				{
					snap = new Dictionary<uint, ushort>(s_learnedUsage);
				}
				string dir = System.IO.Path.GetDirectoryName(LearnedUsageFile);
				if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
					System.IO.Directory.CreateDirectory(dir);
				Newtonsoft.Json.Linq.JObject items = new Newtonsoft.Json.Linq.JObject();
				foreach (var kv in snap)
					items[kv.Key.ToString()] = kv.Value;
				System.IO.File.WriteAllText(LearnedUsageFile, new Newtonsoft.Json.Linq.JObject { ["Items"] = items }.ToString());
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("[PacketBuilder.SaveLearnedUsage] " + ex.Message);
			}
		}
		public static bool UseItem(SRItem item,byte slot,uint uniqueID = 0)
		{
			// Referans (WinForms1) + Sevar uyumu: usage önce kalıcı öğrenmeden,
			// yoksa hesaplanandan, reject sonrası parity alternatifinden gelir.
			// Yanlış öğrenme reject'te silinir (ForgetLearnedUsage), takılma olmaz.
			try
			{
				var chr = InfoManager.Character;
				if (chr == null || chr.Inventory == null || slot >= chr.Inventory.Capacity)
					return false;
				SRItem cur = chr.Inventory[slot];
				// Timer ile paket arasinda envanter degismis olabilir (loot/siralama);
				// yanlis slotu kullanmak serverda reject + DC'ye yol acar.
				if (cur == null || (item != null && cur.ID != item.ID))
				{
					Window.Get?.LogProcess($"[Item] Slot {slot} senkron disi, kullanim atlandi.");
					return false;
				}
				if (!cur.isEquipable() && cur.Quantity == 0)
					return false;
				if (!Bot.Get.CheckUseItemThrottle(slot))
					return false;
				ushort usage = ResolveUsage(cur.ID, item != null ? item.GetUsageType() : cur.GetUsageType());
				Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_USE, true);
				p.WriteByte(slot);
				p.WriteUShort(usage);
				if (uniqueID != 0)
					p.WriteUInt(uniqueID);
				Bot.Get.Proxy.Agent.InjectToServer(p);
				Bot.Get.MarkUseItemSent(slot, cur.ID);
				lock (s_learnedUsageLock)
				{
					s_lastSentItemId = cur.ID;
					s_lastSentUsage = usage;
				}
				Window.Get?.Log($"[Item] Kullanildi: [{cur.Name}] slot={slot} adet={cur.Quantity} ({Packet.ToStringHexadecimal(p.GetBytes())})" + (uniqueID != 0 ? " hedef=" + uniqueID : ""));
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("[PacketBuilder.UseItem] " + ex.Message);
				return false;
			}
		}
		public static void MoveItem(byte slotInitial,byte slotFinal, SRTypes.InventoryItemMovement type,ushort quantity = 0)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte(type);
			p.WriteByte(slotInitial);
			p.WriteByte(slotFinal);
			p.WriteUShort(quantity);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void MoveItem(byte slotInitial,byte slotFinal, SRTypes.InventoryItemMovement type,uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte(type);
			p.WriteByte(slotInitial);
			p.WriteByte(slotFinal);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void DropItem(byte slot)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte(SRTypes.InventoryItemMovement.InventoryToGround);
			p.WriteByte(slot);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void PickUpItem(uint itemUniqueID,uint petPickUniqueID = 0)
		{
			Packet p;
      if (petPickUniqueID == 0)
			{
				p = new Packet(Agent.Opcode.CLIENT_CHARACTER_ACTION_REQUEST);
				p.WriteByte(1);
				p.WriteByte(SRTypes.CharacterAction.ItemPickUp);
				p.WriteByte(1);
				p.WriteUInt(itemUniqueID);
			}
			else
			{
				p = new Packet(Agent.Opcode.CLIENT_CHARACTER_PET_ACTION);
				p.WriteUInt(petPickUniqueID);
				p.WriteByte(SRCoService.Action.ItemPickUp);
				p.WriteUInt(itemUniqueID);
			}
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CreateStall(string title,string note)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_CREATE_REQUEST);
			p.WriteAscii(title); // title.LengthMax = 63
			Bot.Get.Proxy.Agent.InjectToServer(p);
			p = new Packet(Agent.Opcode.CLIENT_STALL_UPDATE_REQUEST);
			p.WriteByte(SRTypes.StallUpdate.Note);
			p.WriteAscii(note);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void EditStallTitle(string title)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_UPDATE_REQUEST);
			p.WriteByte(SRTypes.StallUpdate.Title);
			p.WriteAscii(title);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void EditStallNote(string note)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_UPDATE_REQUEST);
			p.WriteByte(SRTypes.StallUpdate.Note);
			p.WriteAscii(note);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void EditStallState(bool open)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_UPDATE_REQUEST);
			p.WriteByte(SRTypes.StallUpdate.State);
			p.WriteByte(open ? 1 : 0);
			p.WriteShort(0); // unknown
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void AddItemStall(byte slotStall,byte slotInventory,ushort quantity,ulong price)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_UPDATE_REQUEST);
			p.WriteByte(SRTypes.StallUpdate.ItemAdded);
			p.WriteByte(slotStall);
			p.WriteByte(slotInventory);
			p.WriteUShort(quantity);
			p.WriteULong(price);
			p.WriteUInt(1);// FleaMarketNetworkTidGroup. Using the same stall network category
			p.WriteUShort(0); // unknown
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RemoveItemStall(byte slotStall)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_UPDATE_REQUEST);
			p.WriteByte(SRTypes.StallUpdate.ItemRemoved);
			p.WriteByte(slotStall);
			p.WriteUShort(0); // unknown
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void EditItemStall(byte slotStall,ushort quantity,ulong price)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_UPDATE_REQUEST);
			p.WriteByte(SRTypes.StallUpdate.ItemUpdate);
			p.WriteByte(slotStall);
			p.WriteUShort(quantity);
			p.WriteULong(price);
			p.WriteUShort(0); // unknown
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CloseStall()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_DESTROY_REQUEST);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void OpenStall(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_TALK_REQUEST);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void ExitStall()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_LEAVE_REQUEST);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void BuyStallItem(byte slotStall)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STALL_BUY_REQUEST);
			p.WriteByte(slotStall);
			Bot.Get.Proxy.Agent.InjectToServer(p);				
		}
		public static void RequestStorageData(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_STORAGE_DATA_REQUEST);
			p.WriteUInt(uniqueID);
			p.WriteByte(0);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void UnsummonPet(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PET_UNSUMMON_REQUEST);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void SetPetMounted(uint uniqueID, bool mounted)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PET_MOUNTED);
			p.WriteBool(mounted);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void TerminatePet(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_PET_TERMINATE);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void ResurrectAtPresentPoint()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_AUTORESURRECTION);
			p.WriteByte(2);
			Bot.Get.Proxy.Agent.InjectToServer(p,1000);
		}
		public static void CastSkill(uint skillID, uint targetUniqueID = 0)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_ACTION_REQUEST);
			p.WriteByte(1);
			p.WriteByte(SRTypes.CharacterAction.SkillCast);
			p.WriteUInt(skillID);
			if (targetUniqueID != 0)
			{
				p.WriteByte(1);
				p.WriteUInt(targetUniqueID);
			}
			else
			{
				p.WriteByte(0);
			}
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static bool IsBaseSkillId(uint id)
		{
			return id == 2 || id == 40 || id == 70 || (id >= 8419 && id <= 8421) || id == 9354 || id == 9355 || id == 9944 || id == 10625 || id == 11162 || id == 11526;
		}
		public static void AttackTarget(uint targetUniqueID, uint skillID = 1)
		{
			if (targetUniqueID == 0 || Bot.Get.Proxy == null || Bot.Get.Proxy.Agent == null) return;
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_ACTION_REQUEST);
			p.WriteByte(1);
			// Check if is common attack or animation base reference
			if (skillID <= 1 || IsBaseSkillId(skillID))
			{
				p.WriteByte((byte)SRTypes.CharacterAction.CommonAttack);
			}
			else
			{
				p.WriteByte((byte)SRTypes.CharacterAction.SkillCast);
				p.WriteUInt(skillID);
			}
			p.WriteByte(1); // has target? always.
			p.WriteUInt(targetUniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RemoveBuff(uint skillID, uint targetUniqueID = 0)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_ACTION_REQUEST);
			p.WriteByte(1);
			p.WriteByte(SRTypes.CharacterAction.SkillRemove);
			p.WriteUInt(skillID);
			if (targetUniqueID != 0)
			{
				p.WriteUInt(targetUniqueID);
			}
			p.WriteByte(0);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void UseTeleport(uint sourceUniqueID,uint destinationID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_TELEPORT_USE_REQUEST);
			p.WriteUInt(sourceUniqueID);
			p.WriteByte(2); // unknown
			p.WriteUInt(destinationID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void DesignateRecall(uint teleportUniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_TELEPORT_RECALL_REQUEST);
			p.WriteUInt(teleportUniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void InviteToGuild(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_GUILD_INVITATION_REQUEST);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void EditGuildNotice(string title, string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_GUILD_NOTICE_EDIT_REQUEST);
			p.WriteAscii(title);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void AddItemExchange(byte slot)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte(SRTypes.InventoryItemMovement.InventoryToExchange);
			p.WriteByte(slot);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RemoveItemExchange(byte slot)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte(SRTypes.InventoryItemMovement.ExchangeToInventory);
			p.WriteByte(slot);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void EditGoldExchange(ulong newGold)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte(SRTypes.InventoryItemMovement.InventoryGoldToExchange);
			p.WriteULong(newGold);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void ConfirmExchange()
		{
			Bot.Get.Proxy.InjectToServer(new Packet(Agent.Opcode.CLIENT_EXCHANGE_CONFIRM_REQUEST));
		}
		public static void ApproveExchange()
		{
			Bot.Get.Proxy.InjectToServer(new Packet(Agent.Opcode.CLIENT_EXCHANGE_APPROVE_REQUEST));
		}
		public static void InviteToAcademy(uint uniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_ACADEMY_INVITATION_REQUEST);
			p.WriteUInt(uniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		internal class Client
		{
			public static void CreatePickUpPacket(SRItem item, byte inventorySlot)
			{
				Packet p = new Packet(Agent.Opcode.SERVER_INVENTORY_ITEM_MOVEMENT);
				p.WriteByte(1); // Success
				p.WriteByte(SRTypes.InventoryItemMovement.GroundToInventory);
				p.WriteByte(inventorySlot);
				p.WriteUInt(item.Rentable.ID);
				if (item.Rentable.RentableType == SRRentable.Type.LimitedTime)
				{
					p.WriteUShort(item.Rentable.CanDelete);
					p.WriteUInt(item.Rentable.PeriodBeginTime);
					p.WriteUInt(item.Rentable.PeriodEndTime);
				}
				else if (item.Rentable.RentableType == SRRentable.Type.LimitedDistance)
				{
					p.WriteUShort(item.Rentable.CanDelete);
					p.WriteUShort(item.Rentable.CanRecharge);
					p.WriteUInt(item.Rentable.MeterRateTime);
				}
				else if (item.Rentable.RentableType == SRRentable.Type.Package)
				{
					p.WriteUShort(item.Rentable.CanDelete);
					p.WriteUShort(item.Rentable.CanRecharge);
					p.WriteUInt(item.Rentable.PeriodBeginTime);
					p.WriteUInt(item.Rentable.PeriodEndTime);
					p.WriteUInt(item.Rentable.PackingTime);
				}
				p.WriteUInt(item.ID);
				if (item.isEquipable())
				{
					SREquipable equip = (SREquipable)item;
					p.WriteByte(equip.Plus);
					p.WriteULong(equip.Variance);
					p.WriteUInt(equip.Durability);
					p.WriteByte(equip.MagicOptions.Count);
					for (byte j = 0; j < equip.MagicOptions.Count; j++)
					{
						p.WriteUInt(equip.MagicOptions[j].ID);
						p.WriteUInt(equip.MagicOptions[j].Value);
					}
					// 1 = Socket
					p.WriteByte(1);
					if (equip.Sockets != null)
					{
						p.WriteByte(equip.Sockets.Count);
						for (byte j = 0; j < equip.Sockets.Count; j++)
						{
							p.WriteByte(equip.Sockets[j].Slot);
							p.WriteUInt(equip.Sockets[j].ID);
							p.WriteUInt(equip.Sockets[j].Value);
						}
					}
					else
					{
						p.WriteByte(0);
					}
					// 2 = Advanced elixir
					p.WriteByte(2);
					if (equip.AdvancedElixirs != null)
					{
						p.WriteByte(equip.AdvancedElixirs.Count);
						for (byte j = 0; j < equip.AdvancedElixirs.Count; j++)
						{
							p.WriteByte(equip.AdvancedElixirs[j].Slot);
							p.WriteUInt(equip.AdvancedElixirs[j].ID);
							p.WriteUInt(equip.AdvancedElixirs[j].Value);
						}
					}
					else
					{
						p.WriteByte(0);
					}
				}
				else if (item.isCoS())
				{
					SRCoS cos = (SRCoS)item;
					if (cos.isPet())
					{
						// ITEM_COS_P
						p.WriteByte(cos.StateType);
						if (cos.StateType != SRCoS.State.NeverSummoned)
						{
							p.WriteUInt(cos.ModelID);
							p.WriteAscii(cos.ModelName);
							if (cos.ID4 == 2)
								p.WriteUInt(cos.Rentable.PeriodEndTime);
							p.WriteByte(cos.unkByte01);
						}
					}
					else if (cos.isTransform())
					{
						if (cos.ModelID == 0)
							cos.ModelID = new SRModel("MOB_CH_MANGNYANG").ID;
						p.WriteUInt(cos.ModelID);
					}
					else if (cos.isCube())
					{
						p.WriteUInt(cos.Quantity);
					}
				}
				else if (item.isEtc())
				{
					SREtc etc = (SREtc)item;
					p.WriteUShort(etc.Quantity);
					if (etc.isAlchemy())
					{
						if (item.ID4 == 1 || item.ID4 == 2)
						{
							// MAGIC/ATRIBUTTE STONE
							p.WriteByte(etc.AssimilationProbability);
						}
					}
					else if (item.ID3 == 14 && item.ID4 == 2)
					{
						// ITEM_MALL_GACHA_CARD_WIN
						// ITEM_MALL_GACHA_CARD_LOSE
						p.WriteByte(0); // paramCount
					}
				}
				Bot.Get.Proxy.Agent.InjectToClient(p);
			}
			public static void CreatePickUpSpecialtyGoodsPacket(SRItem item, byte inventorySlot, uint transportUniqueID,string OwnerName, uint unkUInt01)
			{
				Packet p = new Packet(Agent.Opcode.SERVER_INVENTORY_ITEM_MOVEMENT);
				p.WriteByte(1); // Success
				p.WriteByte(SRTypes.InventoryItemMovement.GroundToPet);
				p.WriteUInt(transportUniqueID);
				p.WriteByte(inventorySlot);
				p.WriteUInt(unkUInt01);
				p.WriteUInt(item.ID);
				p.WriteUShort(item.Quantity);
				p.WriteAscii(OwnerName);
				Bot.Get.Proxy.Agent.InjectToClient(p);
			}
			public static void SendNotice(string message)
			{
				Packet p = new Packet(Agent.Opcode.SERVER_CHAT_UPDATE);
				p.WriteByte(SRTypes.Chat.Notice);
				p.WriteAscii(message);
				Bot.Get.Proxy.Agent.InjectToClient(p);
			}
			public static void CreateAgentLogin(byte flag, uint loginID, string host, ushort port, ushort opcode = Gateway.Opcode.SERVER_LOGIN_RESPONSE, byte[] extraBytes = null, bool encrypted = true)
			{
				Packet p = new Packet(opcode, encrypted);
				p.WriteByte(flag);
				p.WriteUInt(loginID);
				p.WriteAscii(host);
				p.WriteUShort(port);
				if (extraBytes != null && extraBytes.Length > 0)
				{
					p.WriteByteArray(extraBytes);
				}
				Bot.Get.Proxy.Gateway.InjectToClient(p);
			}
		}
		public static void SendGMCommand(SRTypes.GMCommandAction action, string message)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_GAMEMASTER_COMMAND_REQUEST);
			p.WriteUShort(action);
			p.WriteAscii(message);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void ActivateBerserk()
		{
			SetBodyState(SRTypes.EntityBodyState.Berserk);
		}
		/// <summary>
		/// srodevs-docs AGENT_CHARACTER_BODYSTATE_REQ (0x70A7).
		/// </summary>
		public static void SetBodyState(SRTypes.EntityBodyState state)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_CHARACTER_BERSERK_ACTIVATE);
			p.WriteByte((byte)state);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		/// <summary>
		/// srodevs-docs AGENT_GAME_LOGOUT_REQ (0x7005) + CANCEL (0x7006).
		/// </summary>
		public static void RequestLogout(SRTypes.LogoutMode mode)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_LOGOUT_REQUEST);
			p.WriteByte((byte)mode);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CancelLogout()
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_LOGOUT_CANCEL_REQUEST);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		/// <summary>
		/// srodevs-docs AGENT_CHARACTER_SELECTION_RENAME_REQ (0x7450).
		/// </summary>
		public static void RenameCharacter(string currentName, string newName)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_RENAME_REQUEST);
			p.WriteByte((byte)SRTypes.CharacterRenameAction.CharacterRename);
			p.WriteAscii(currentName);
			p.WriteAscii(newName);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RenameGuild(string currentName, string newName)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_RENAME_REQUEST);
			p.WriteByte((byte)SRTypes.CharacterRenameAction.GuildRename);
			p.WriteAscii(currentName);
			p.WriteAscii(newName);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void CheckGuildName(string name)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_RENAME_REQUEST);
			p.WriteByte((byte)SRTypes.CharacterRenameAction.GuildNameCheck);
			p.WriteAscii(name);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RepairAllEquipments(uint npcUniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_REPAIR_ALL_EQUIPMENTS);
			p.WriteUInt(npcUniqueID);
			p.WriteByte(2); // 2 = Repair all equipped items
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void OpenStorage(uint npcUniqueID)
		{
			// 0x703C = UID(4) + storageType(1): 0 = personal storage.
			// Eksik trailing byte ile gönderilen paket server'ı düşürüyordu
			// (RequestStorageData ile aynı format olmalı).
			Packet p = new Packet(Agent.Opcode.CLIENT_STORAGE_DATA_REQUEST);
			p.WriteUInt(npcUniqueID);
			p.WriteByte(0);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void MoveStorageItem(byte slotInitial, byte slotFinal, SRTypes.InventoryItemMovement type,
			uint npcUniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByteArray(StorageMovementPolicy.BuildCrossContainerPayload(
				(byte)type, slotInitial, slotFinal, npcUniqueID));
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void LockGuildStorage(uint npcUniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_GUILD_STORAGE_REQUEST);
			p.WriteUInt(npcUniqueID);
			p.WriteUShort(0);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void UnlockGuildStorage(uint npcUniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_GUILD_STORAGE_CLOSE_REQUEST);
			p.WriteUInt(npcUniqueID);
			p.WriteUShort(0);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void RefreshGuildStorage(uint npcUniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_GUILD_STORAGE_DATA_REQUEST);
			p.WriteUInt(npcUniqueID);
			p.WriteUShort(0);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void MoveGold(SRTypes.InventoryItemMovement type, ulong amount)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte(type);
			p.WriteULong(amount);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
		public static void BuyItemFromShop(byte tabNumber, byte tabSlot, ushort quantity, uint npcUniqueID)
		{
			Packet p = new Packet(Agent.Opcode.CLIENT_INVENTORY_ITEM_MOVEMENT);
			p.WriteByte((byte)SRTypes.InventoryItemMovement.ShopToInventory);
			p.WriteByte(tabNumber);
			p.WriteByte(tabSlot);
			p.WriteUShort(quantity);
			p.WriteUInt(npcUniqueID);
			Bot.Get.Proxy.Agent.InjectToServer(p);
		}
	}
}
