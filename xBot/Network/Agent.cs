using SecurityAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using xBot.App;
using xBot.App.Theme;
using xBot.Game;
using xBot.Game.Objects.Common;

namespace xBot.Network
{
	public class Agent
	{
		private static readonly HashSet<ushort> s_loggedUnknownOpcodes = new HashSet<ushort>();
		private static void LogUnknownOpcode(ushort opcode)
		{
			lock (s_loggedUnknownOpcodes)
			{
				if (s_loggedUnknownOpcodes.Count > 200)
					s_loggedUnknownOpcodes.Clear();
				if (!s_loggedUnknownOpcodes.Add(opcode))
					return;
			}
			BotLog.LogPacket($"[A] Unhandled opcode 0x{opcode:X4}");
		}
		public static class Opcode
		{
			// static opcodes could be edited at realtime (for different vSRO types)
			public static ushort
				CLIENT_HWID_RESPONSE = 0,
				SERVER_HWID_REQUEST = 0;
			public const ushort
				CLIENT_AUTH_REQUEST = 0x6103,
				CLIENT_GAMEMASTER_COMMAND_REQUEST = 0x7010,
				CLIENT_CHARACTER_SELECTION_JOIN_REQUEST = 0x7001,
				CLIENT_CHARACTER_SELECTION_ACTION_REQUEST = 0x7007,
				CLIENT_CHARACTER_CONFIRM_SPAWN = 0x3012,
				CLIENT_CHARACTER_MOVEMENT = 0x7021,
				CLIENT_CHARACTER_MOVEMENT_ANGLE = 0x7024,
				CLIENT_CHARACTER_PET_ACTION = 0x70C5,
				CLIENT_CHARACTER_ADD_STR_REQUEST = 0x7050,
				CLIENT_CHARACTER_ADD_INT_REQUEST = 0x7051,
				CLIENT_CHARACTER_EMOTE_USE = 0x3091,
				CLIENT_CHARACTER_AUTORESURRECTION = 0x3053,
				CLIENT_CHARACTER_ACTION_REQUEST = 0x7074,
				CLIENT_INVENTORY_ITEM_USE = 0x704C,
				CLIENT_INVENTORY_ITEM_MOVEMENT = 0x7034,
				CLIENT_STORAGE_DATA_REQUEST = 0x703C,
				CLIENT_ENTITY_SELECTION = 0x7045,
				CLIENT_ENTITY_TALK_REQUEST = 0x7046,
				CLIENT_NPC_CLOSE_REQUEST = 0x704B,
				CLIENT_CHAT_REQUEST = 0x7025,
				CLIENT_MAIL_SEND_REQUEST = 0x7309,
				CLIENT_PLAYER_INVITATION_RESPONSE = 0x3080,
				CLIENT_PARTY_CREATION_REQUEST = 0x7060,
				CLIENT_PARTY_LEAVE = 0x7061,
				CLIENT_PARTY_INVITATION_REQUEST = 0x7062,
				CLIENT_PARTY_KICK_REQUEST = 0x7063,
				CLIENT_PARTY_MATCH_CREATION_REQUEST = 0x7069,
				CLIENT_PARTY_MATCH_EDITED_REQUEST = 0x706A,
				CLIENT_PARTY_MATCH_DELETE_REQUEST = 0x706B,
				CLIENT_PARTY_MATCH_LIST_REQUEST = 0x706C,
				CLIENT_PARTY_MATCH_JOIN_REQUEST = 0x706D,
				CLIENT_PARTY_MATCH_JOIN_RESPONSE = 0x306E,
				CLIENT_GUILD_INVITATION_REQUEST = 0x70F3,
				CLIENT_GUILD_NOTICE_EDIT_REQUEST = 0x70F9,
				CLIENT_GUILD_STORAGE_REQUEST = 0x7250,
				CLIENT_GUILD_STORAGE_CLOSE_REQUEST = 0x7251,
				CLIENT_GUILD_STORAGE_DATA_REQUEST = 0x7252,
				CLIENT_ACADEMY_INVITATION_REQUEST = 0x7472,
				CLIENT_ACADEMY_MATCH_LIST_REQUEST = 0x747D,
				CLIENT_ACADEMY_NOTICE_EDIT_REQUEST = 0x7477,
				CLIENT_PET_UNSUMMON_REQUEST = 0x7116,
				CLIENT_PET_SETTINGS_CHANGE_REQUEST = 0x7420,
				CLIENT_PET_MOUNTED = 0x70CB,
				CLIENT_PET_TERMINATE = 0x70C6,
				CLIENT_STALL_CREATE_REQUEST = 0x70B1,
				CLIENT_STALL_DESTROY_REQUEST = 0x70B2,
				CLIENT_STALL_TALK_REQUEST = 0x70B3,
				CLIENT_STALL_BUY_REQUEST = 0x70B4,
				CLIENT_STALL_LEAVE_REQUEST = 0x70B5,
				CLIENT_STALL_UPDATE_REQUEST = 0x70BA,
				CLIENT_MASTERY_SKILL_LEVELUP_REQUEST = 0x70A1,
				CLIENT_MASTERY_SKILL_LEVELDOWN_REQUEST = 0x7202,
				CLIENT_MASTERY_LEVELUP_REQUEST = 0x70A2,
				CLIENT_MASTERY_LEVELDOWN_REQUEST = 0x7203,
				CLIENT_TELEPORT_USE_REQUEST = 0x705A,
				CLIENT_TELEPORT_RECALL_REQUEST = 0x7059,
				CLIENT_TELEPORT_READY_RESPONSE = 0x34B6,
				CLIENT_CONSIGNMENT_LIST_REQUEST = 0x750E,
				CLIENT_CONSIGNMENT_CLOSE_REQUEST = 0x7507,
				CLIENT_CONSIGNMENT_REGISTER_REQUEST = 0x7508,
				CLIENT_CONSIGNMENT_UNREGISTER_REQUEST = 0x7509,
				CLIENT_CONSIGNMENT_SETTLE_REQUEST = 0x750B,
				CLIENT_EXCHANGE_INVITATION_REQUEST = 0x7081,
				CLIENT_EXCHANGE_CONFIRM_REQUEST = 0x7082,
				CLIENT_EXCHANGE_APPROVE_REQUEST = 0x7083,
				CLIENT_EXCHANGE_EXIT_REQUEST = 0x7084,
				CLIENT_SECONDARY_PASSCODE = 0x7625,
				CLIENT_ALCHEMY = 0x7150,
			CLIENT_ALCHEMY_STONE = 0x7151,
			CLIENT_CHARACTER_BERSERK_ACTIVATE = 0x70A7,
			CLIENT_CHARACTER_BODYSTATE = 0x70A7,
			CLIENT_LOGOUT_REQUEST = 0x7005,
			CLIENT_LOGOUT_CANCEL_REQUEST = 0x7006,
			CLIENT_RENAME_REQUEST = 0x7450,
				CLIENT_REPAIR_ALL_EQUIPMENTS = 0x703E,
			CLIENT_QUEST_TALK_REQUEST = 0x30D4,
			CLIENT_EVENT_QUEST_ID_REQUEST = CLIENT_QUEST_TALK_REQUEST,
			CLIENT_EVENT_QUEST_REWARD_REQUEST = 0x7515,
			CLIENT_QUEST_ABANDON_REQUEST = 0x70D9,

			SERVER_AUTH_RESPONSE = 0xA103,
				SERVER_SECONDARY_PASSCODE_REQUEST = 0x3625,
				SERVER_SECONDARY_PASSCODE_RESPONSE = 0xB625,
				SERVER_ALCHEMY_RESPONSE = 0xB150,
				SERVER_CHARACTER_SELECTION_JOIN_RESPONSE = 0xB001,
				SERVER_CHARACTER_SELECTION_ACTION_RESPONSE = 0xB007,
				SERVER_CHARACTER_DATA_BEGIN = 0x34A5,
				SERVER_CHARACTER_DATA = 0x3013,
				SERVER_CHARACTER_DATA_END = 0x34A6,
				SERVER_CHARACTER_STATS_UPDATE = 0x303D,
                SERVER_CHARACTER_EXPERIENCE_UPDATE = 0x3056,
				SERVER_CHARACTER_ADD_STR_RESPONSE = 0xB050,
				SERVER_CHARACTER_ADD_INT_RESPONSE = 0xB051,
				SERVER_CHARACTER_INFO_UPDATE = 0x304E,
				SERVER_CHARACTER_DIED = 0x3011,
                SERVER_CHARACTER_ACTION_RESPONSE = 0xB074,
                SERVER_INVENTORY_ITEM_USE = 0xB04C,
				SERVER_INVENTORY_ITEM_MOVEMENT = 0xB034,
				SERVER_REPAIR_ALL_EQUIPMENTS_RESPONSE = 0xB03E,
				SERVER_INVENTORY_ITEM_DURABILITY_UPDATE = 0x3052,
                SERVER_INVENTORY_ITEM_UPDATE = 0x3040,
				SERVER_INVENTORY_CAPACITY_UPDATE = 0x3092,
				SERVER_STORAGE_DATA_BEGIN = 0x3047,
				SERVER_STORAGE_DATA = 0x3049,
				SERVER_STORAGE_DATA_END = 0x3048,
				SERVER_ENTITY_SELECTION = 0xB045,
				SERVER_ENTITY_SKILL_START = 0xB070,
				SERVER_ENTITY_SKILL_END = 0xB071,
				SERVER_ENTITY_SKILL_BUFF_ADDED = 0xB0BD,
				SERVER_ENTITY_SKILL_BUFF_REMOVED = 0xB072,
				SERVER_ENTITY_SPAWN = 0x3015,
				SERVER_ENTITY_DESPAWN = 0x3016,
				SERVER_ENTITY_GROUPSPAWN_BEGIN = 0x3017,
				SERVER_ENTITY_GROUPSPAWN_END = 0x3018,
				SERVER_ENTITY_GROUPSPAWN_DATA = 0x3019,
				SERVER_ENTITY_MOVEMENT = 0xB021,
				SERVER_ENTITY_MOVEMENT_STUCK = 0xB023,
				SERVER_ENTITY_MOVEMENT_ANGLE = 0xB024,
				SERVER_ENTITY_TALK_RESPONSE = 0xB046,
				SERVER_ENTITY_LEVEL_UP = 0x3054,
				SERVER_ENTITY_STATUS_UPDATE = 0x3057,
				SERVER_ENTITY_DISPLAY_EFFECT = 0x305C,
				SERVER_ENTITY_SPEED_UPDATE = 0x30D0,
                SERVET_ENTITY_INVENTORY_EQUIP = 0x3038,
                SERVER_ENTITY_STATE_UPDATE = 0x30BF,
				SERVER_ENTITY_STALL_CREATE = 0x30B8,
				SERVER_ENTITY_STALL_DESTROY = 0x30B9,
				SERVER_ENTITY_STALL_TITLE_UPDATE = 0x30BB,
				SERVER_ENTITY_EMOTE_USE = 0x3091,
				SERVER_CHAT_RESPONSE = 0xB025,
				SERVER_CHAT_UPDATE = 0x3026,
				SERVER_PLAYER_PETITION_REQUEST = 0x3080,
				SERVER_MAIL_SEND_RESPONSE = 0xB309,
				SERVER_NOTICE_UPDATE = 0x300C,
				SERVER_ENVIRONMENT_CELESTIAL_POSITION = 0x3020,
				SERVER_ENVIRONMENT_CELESTIAL_UPDATE = 0x3027,
				SERVER_ENVIRONMENT_WEATHER_UPDATE = 0x3809,
#pragma warning disable CS0618 // Typo uyumluluğu için eski isimler saklanıyor
				SERVER_ENVIROMENT_CELESTIAL_POSITION = SERVER_ENVIRONMENT_CELESTIAL_POSITION,
				SERVER_ENVIROMENT_CELESTIAL_UPDATE = SERVER_ENVIRONMENT_CELESTIAL_UPDATE,
				SERVER_ENVIROMENT_WHEATER_UPDATE = SERVER_ENVIRONMENT_WEATHER_UPDATE,
#pragma warning restore CS0618
				SERVER_PARTY_INVITATION_RESPONSE = 0xB060,
				SERVER_PARTY_DATA = 0x3065,
				SERVER_PARTY_UPDATE = 0x3864,
				SERVER_PARTY_MATCH_LIST_RESPONSE = 0xB06C,
				SERVER_PARTY_MATCH_CREATION_RESPONSE = 0xB069,
				SERVER_PARTY_MATCH_EDITED_RESPONSE = 0xB06A,
				SERVER_PARTY_MATCH_DELETE_RESPONSE = 0xB06B,
				SERVER_PARTY_MATCH_JOIN_REQUEST = 0x706D,
				SERVER_PET_DATA = 0x30C8,
				SERVER_PET_UPDATE = 0x30C9,
				SERVER_PET_SETTINGS_CHANGE_RESPONSE = 0xB420,
				SERVER_PET_PLAYER_MOUNTED = 0xB0CB,
				SERVER_STALL_CREATE_RESPONSE = 0xB0B1,
				SERVER_STALL_DESTROY_RESPONSE = 0xB0B2,
				SERVER_STALL_TALK_RESPONSE = 0xB0B3,
				SERVER_STALL_BUY_RESPONSE = 0xB0B4,
				SERVER_STALL_LEAVE_RESPONSE = 0xB0B5,
				SERVER_STALL_UPDATE_RESPONSE = 0xB0BA,
				SERVER_STALL_ENTITY_ACTION = 0x30B7,
				SERVER_STALL_CLOSED = 0x30B9,
				SERVER_MASTERY_SKILL_LEVELUP_RESPONSE = 0xB0A1,
				SERVER_MASTERY_SKILL_LEVELDOWN_RESPONSE = 0xB202,
				SERVER_MASTERY_LEVELUP_RESPONSE = 0xB0A2,
				SERVER_MASTERY_LEVELDOWN_RESPONSE = 0xB203,
				SERVER_TELEPORT_USE_RESPONSE = 0xB05A,
				SERVER_TELEPORT_RECALL_RESPONSE = 0xB059,
				SERVER_TELEPORT_READY_REQUEST = 0x34B5,
				SERVER_CONSIGNMENT_REGISTER_RESPONSE = 0xB508,
				SERVER_CONSIGNMENT_UNREGISTER_RESPONSE = 0xB509,
				SERVER_CONSIGNMENT_LIST_RESPONSE = 0xB50E,
				SERVER_CONSIGNMENT_SETTLE_RESPONSE = 0xB50B,
				SERVER_GUILD_CREATED_DATA = 0xB0F0,
				SERVER_GUILD_DATA_BEGIN = 0x34B3,
				SERVER_GUILD_DATA = 0x3101,
				SERVER_GUILD_DATA_END = 0x34B4,
				SERVER_GUILD_UPDATE = 0x38F5,
				SERVER_GUILD_PLAYER_LOG = 0x30FF,
				SERVER_GUILD_STORAGE_RESPONSE = 0xB250,
				SERVER_GUILD_STORAGE_DATA_BEGIN = 0x3253,
				SERVER_GUILD_STORAGE_DATA = 0x3255,
				SERVER_GUILD_STORAGE_DATA_END = 0x3254,
				SERVER_ACADEMY_DATA = 0x3C81,
				SERVER_ACADEMY_MATCH_LIST_RESPONSE = 0xB47D,
				SERVER_EXCHANGE_STARTED = 0x3085,
				SERVER_EXCHANGE_PLAYER_CONFIRMED = 0x3086,
				SERVER_EXCHANGE_COMPLETED = 0x3087,
				SERVER_EXCHANGE_CANCELED = 0x3088,
				SERVER_EXCHANGE_GOLD_UPDATE = 0x3089,
				SERVER_EXCHANGE_ITEMS_UPDATE = 0x308C,
				SERVER_EXCHANGE_INVITATION_RESPONSE = 0xB081,
				SERVER_EXCHANGE_CONFIRM_RESPONSE = 0xB082,
				SERVER_EXCHANGE_APPROVE_RESPONSE = 0xB083,
				SERVER_EXCHANGE_EXIT_RESPONSE = 0xB084,
				SERVER_DROP_UNLOCKED = 0x304D,
				SERVER_NPC_CLOSE_RESPONSE = 0xB04B,
				SERVER_LOGOUT_RESPONSE = 0xB005,
				SERVER_LOGOUT_CANCEL_RESPONSE = 0xB006,
				SERVER_LOGOUT_SUCCESS = 0x300A,
				SERVER_RENAME_RESPONSE = 0xB450,
				SERVER_CHAT_RESTRICT = 0x302D,
				SERVER_QUEST_TALK = 0x30D4,
				SERVER_EVENT_QUEST_ID_RESPONSE = 0x3514,
				SERVER_QUEST_UPDATE = 0x30D5,
				SERVER_QUEST_ABANDON_RESPONSE = 0xB0D9,
				SERVER_QUEST_SCRIPT = 0x3CA2,

				GLOBAL_HANDSHAKE = 0x5000,
				GLOBAL_HANDSHAKE_OK = 0x9000,
				GLOBAL_IDENTIFICATION = 0x2001,
				GLOBAL_PING = 0x2002,
				GLOBAL_XTRAP_IDENTIFICATION = 0x2113;
		}
		public Context Local { get; }
		public Context Remote { get; }
		public List<ushort> IgnoreOpcodeClient { get; }
		public List<ushort> IgnoreOpcodeServer { get; }
		public uint id { get; }
		public string Host { get; }
		public ushort Port { get; }
		public Agent(uint queqeID, string host, ushort port)
		{
			id = queqeID;
			Host = host;
			Port = port;

			Local = new Context();
			Local.Security.GenerateSecurity(true, true, true);
			Remote = new Context();
			// Setup cycle : (Client < > Proxy < > Server)
			Remote.RelaySecurity = Local.Security; // Client < Proxy < Server
			Local.RelaySecurity = Remote.Security; // Client > Proxy > Server
			Remote.Security.AddEncryptedOpcode(Opcode.CLIENT_INVENTORY_ITEM_USE);
			Local.Security.AddEncryptedOpcode(Opcode.CLIENT_INVENTORY_ITEM_USE);
																						 
			IgnoreOpcodeClient = new List<ushort>(); // ignore list
			IgnoreOpcodeClient.Add(Opcode.GLOBAL_HANDSHAKE);
			IgnoreOpcodeClient.Add(Opcode.GLOBAL_HANDSHAKE_OK);
			IgnoreOpcodeClient.Add(Opcode.GLOBAL_IDENTIFICATION); // proxy to remote is handled by API
			IgnoreOpcodeClient.Add(Opcode.GLOBAL_PING); // handling ping manually

			IgnoreOpcodeServer = new List<ushort>(); // ignore list
			IgnoreOpcodeServer.Add(Opcode.GLOBAL_HANDSHAKE);
			IgnoreOpcodeServer.Add(Opcode.GLOBAL_HANDSHAKE_OK);
		}
		public bool ClientlessMode { get { return Bot.Get.Proxy.ClientlessMode; } }
		public bool IgnoreOpcode(ushort opcode, Context c)
		{
			if (c == Local)
				return IgnoreOpcodeClient.Contains(opcode);
			return IgnoreOpcodeServer.Contains(opcode);
		}
		/// <summary>
		/// Analyze packets.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="packet">Packet to analyze</param>
		/// <returns>True if the packet is handled by the bot</returns>
		public bool PacketHandler(Context context, Packet packet)
		{
			try
			{
				if (context == Local)
					return Local_PacketHandler(packet);
				return Remote_PacketHandler(packet);
			}
			catch(Exception ex)
			{
				Bot.Get.LogError("Parsing Error", ex, packet);
				BotLog.Log($"[PacketParser Warning] Error parsing opcode 0x{packet.Opcode:X4}: {ex.Message}", LogLevel.Warning);
				return false;
			}
		}
		/// <summary>
		/// Analyze client packets.
		/// </summary>
		/// <param name="packet">Client packet</param>
		/// <returns>True if the packet won't be sent to the server</returns>
		private bool Local_PacketHandler(Packet packet)
		{
			// Opcode filter
			switch (packet.Opcode)
			{
				case Opcode.CLIENT_AUTH_REQUEST:
					if(Bot.Get.LoggedFromBot)
						return true;
					break;
				case Opcode.CLIENT_CHARACTER_SELECTION_JOIN_REQUEST:
					string charName = packet.ReadAscii();
					packet.SeekRead(0, System.IO.SeekOrigin.Begin);
					InfoManager.CharName = charName;
					Window wSelect = Window.Get;
					wSelect?.Login_btnStart.InvokeIfRequired(() => {
						wSelect.Login_btnStart.Enabled = false;
					});
					wSelect?.LogProcess("Selecting [" + charName + "] ...");
					wSelect?.Log("Selecting [" + charName + "] ...");
					return false;
				case Opcode.CLIENT_CHARACTER_CONFIRM_SPAWN:
					if (!ClientlessMode)
						InfoManager.OnTeleported();
					break;
				case Opcode.CLIENT_CHARACTER_EMOTE_USE:
					{
						byte emoteByte = packet.ReadByte();
						packet.SeekRead(0, System.IO.SeekOrigin.Begin);
						App.CommandCenter.CommandCenterManager.OnEmoteUsed(emoteByte);
					}
					return false;
				case Opcode.CLIENT_CHAT_REQUEST:
					{
						// Keep on track all private messages sent
						SRTypes.Chat t = (SRTypes.Chat)packet.ReadByte();
						byte chatIndex = packet.ReadByte();
						if (t == SRTypes.Chat.All)
						{
							string message = packet.ReadAscii();
							if (App.CommandCenter.CommandCenterManager.HandleChatCommand(message))
								return true;
							return Bot.Get.OnChatSending(message);
						}
						else if (t == SRTypes.Chat.Private)
						{
							Window w = Window.Get;
							w.LogChatMessage(w.Chat_rtbxPrivate, packet.ReadAscii() + "(To)", packet.ReadAscii());
						}
					}
					break;
				case Opcode.CLIENT_INVENTORY_ITEM_USE:
					{
						// Gercek clientin gonderdigi kullanim: slot+usage ogren (Sevar custom TID uyumu)
						// + bot timer/blackout senkronu (cooldown yarisini engelle). Relay bozulmaz.
						try
						{
							byte[] rawUse = packet.GetBytes();
							if (rawUse != null && rawUse.Length >= 3)
							{
								byte useSlot = rawUse[0];
								ushort useUsage = (ushort)(rawUse[1] | (rawUse[2] << 8));
								Bot.Get.NotifyExternalItemUse(useSlot, useUsage);
								System.Diagnostics.Debug.WriteLine($"[Item] Client kullanimi: slot={useSlot} ({Packet.ToStringHexadecimal(rawUse)}) ({rawUse.Length}B)");
							}
						}
						catch (Exception ex)
						{
							ModernLogger.LogSwallowed(ex, "Agent.Local.ItemUse");
						}
					}
					break;
			case Opcode.CLIENT_CHARACTER_ACTION_REQUEST:
				bool isPVPMode = false;
				Window.Get.Character_cbxPVPMode.InvokeIfRequired(() =>
				{
					isPVPMode = Window.Get.Character_cbxPVPMode.Checked;
				});
				if (!isPVPMode)
					break;
				// execute?
				if (packet.ReadByte() != 1)
					break;
				// check is casting skill
				if ((SRTypes.CharacterAction)packet.ReadByte() != SRTypes.CharacterAction.SkillCast)
					break;
				Bot.Get.CheckWeaponSwitch(new SRSkill(packet.ReadUInt()));
				break;
			case Opcode.CLIENT_ENTITY_SELECTION:
			case Opcode.CLIENT_ENTITY_TALK_REQUEST:
			case Opcode.CLIENT_TELEPORT_USE_REQUEST:
			case Opcode.CLIENT_TELEPORT_RECALL_REQUEST:
			case Opcode.CLIENT_NPC_CLOSE_REQUEST:
				// Gate dialog akışını çözmek için geçici sniff: kullanıcı kapıyı
				// elle kullanınca istemcinin gönderdiği ham baytlar loglanır.
				try
				{
					byte[] rawSniff = packet.GetBytes();
					Window.Get?.Log($"[Sniff][C->S] 0x{packet.Opcode:X4} ({Packet.ToStringHexadecimal(rawSniff)}) ({rawSniff.Length}B)");
				}
				catch (Exception ex)
				{
					ModernLogger.LogSwallowed(ex, "Agent.Local.Sniff");
				}
				break;
			case Opcode.CLIENT_QUEST_TALK_REQUEST:
				try
				{
					byte[] rawQuestTalk = packet.GetBytes();
					QuestAutomationManager.ObserveTalkChoice(rawQuestTalk.Length > 0 ? rawQuestTalk[0] : (byte)0, rawQuestTalk);
				}
				catch (Exception ex)
				{
					ModernLogger.LogSwallowed(ex, "Agent.Local.QuestTalk");
				}
				break;
			case Opcode.CLIENT_EVENT_QUEST_REWARD_REQUEST:
				try
				{
					byte[] rawQuestReward = packet.GetBytes();
					QuestAutomationManager.ObserveRewardSelection(rawQuestReward);
				}
				catch (Exception ex)
				{
					ModernLogger.LogSwallowed(ex, "Agent.Local.QuestReward");
				}
				break;
			}
			return false;
		}
		/// <summary>
		/// Analyze server packets.
		/// </summary>
		/// <param name="packet">Server packet</param>
		/// <returns>True if the packet will be ignored by the client</returns>
		public delegate bool RemotePacketHandlerDelegate(Agent agent, Packet packet);
		private static readonly Dictionary<ushort, RemotePacketHandlerDelegate> s_remotePacketHandlers = new Dictionary<ushort, RemotePacketHandlerDelegate>();

		static Agent()
		{
			RegisterRemotePacketHandlers();
		}

		private static void RegisterRemotePacketHandlers()
		{
			void Register(ushort opcode, Action<Packet> parser)
			{
				s_remotePacketHandlers[opcode] = (agent, packet) =>
				{
					parser(packet);
					return false;
				};
			}

			s_remotePacketHandlers[Opcode.GLOBAL_IDENTIFICATION] = (agent, packet) =>
			{
				if (Bot.Get.LoggedFromBot)
				{
					string service = packet.ReadAscii();
					if (service == "AgentServer")
					{
						Packet protocol = new Packet(Opcode.CLIENT_AUTH_REQUEST, true);
						protocol.WriteUInt(agent.id);

						string username = !string.IsNullOrEmpty(AccountManager.SelectedAccountUsername)
							? (AccountManager.GetAccount(AccountManager.SelectedAccountUsername)?.Username ?? "")
							: "";
						if (string.IsNullOrEmpty(username) && Window.Get != null)
							username = Window.Get.Login_tbxUsername.Text;

						string password = !string.IsNullOrEmpty(AccountManager.SelectedAccountUsername)
							? (AccountManager.GetAccount(AccountManager.SelectedAccountUsername)?.Password ?? "")
							: "";
						if (string.IsNullOrEmpty(password) && Window.Get != null)
							password = Window.Get.Login_tbxPassword.Text;

						protocol.WriteAscii(username);
						protocol.WriteAscii(password);
						protocol.WriteUShort(DataManager.Locale);
						// "MAC"
						protocol.WriteUInt(0u);
						protocol.WriteUShort(0);
						agent.InjectToServer(protocol);
					}
				}
				return false;
			};

			s_remotePacketHandlers[Opcode.SERVER_AUTH_RESPONSE] = (agent, packet) =>
			{
				byte success = packet.ReadByte();
				if (success == 1)
				{
					if (agent.ClientlessMode)
						PacketBuilder.RequestCharacterList();

					InfoManager.OnConnected();
				}
				else
				{
					byte error = packet.ReadByte();
					BotLog.Log(SroDocsPolicy.GetAuthErrorMessage(error));
				}
				return false;
			};

			Register(Opcode.SERVER_CHARACTER_SELECTION_ACTION_RESPONSE, PacketParser.CharacterSelectionActionResponse);
			Register(Opcode.SERVER_CHARACTER_SELECTION_JOIN_RESPONSE, PacketParser.CharacterSelectionJoinResponse);
			Register(Opcode.SERVER_SECONDARY_PASSCODE_REQUEST, PacketParser.SecondaryPasscodeResponse);
			Register(Opcode.SERVER_SECONDARY_PASSCODE_RESPONSE, PacketParser.SecondaryPasscodeResponse);
			Register(Opcode.SERVER_ALCHEMY_RESPONSE, PacketParser.AlchemyResponse);
			Register(Opcode.SERVER_CHARACTER_DATA_BEGIN, PacketParser.CharacterDataBegin);
			Register(Opcode.SERVER_CHARACTER_DATA, PacketParser.CharacterData);

			s_remotePacketHandlers[Opcode.SERVER_CHARACTER_DATA_END] = (agent, packet) =>
			{
				PacketParser.CharacterDataEnd();
				if (agent.ClientlessMode)
				{
					Packet protocol = new Packet(Opcode.CLIENT_CHARACTER_CONFIRM_SPAWN);
					agent.InjectToServer(protocol);

					InfoManager.OnTeleported();

					protocol = new Packet(Opcode.CLIENT_CONSIGNMENT_LIST_REQUEST);
					agent.InjectToServer(protocol);
				}
				else
				{
					InfoManager.OnTeleported();
				}
				return false;
			};

			Register(Opcode.SERVER_CHARACTER_STATS_UPDATE, PacketParser.CharacterStatsUpdate);
			Register(Opcode.SERVER_CHARACTER_EXPERIENCE_UPDATE, PacketParser.CharacterExperienceUpdate);
			Register(Opcode.SERVER_CHARACTER_INFO_UPDATE, PacketParser.CharacterInfoUpdate);
			Register(Opcode.SERVER_CHARACTER_DIED, PacketParser.CharacterDied);

			s_remotePacketHandlers[Opcode.GLOBAL_XTRAP_IDENTIFICATION] = (agent, packet) =>
			{
				if (agent.ClientlessMode)
				{
					Packet protocol = new Packet(Opcode.GLOBAL_XTRAP_IDENTIFICATION);
					protocol.WriteByte(2);
					protocol.WriteByte(2);
					protocol.WriteUInt64Array(new ulong[128]);
					agent.InjectToServer(protocol);
				}
				return false;
			};

			Register(Opcode.SERVER_ENTITY_SPAWN, PacketParser.EntitySpawn);
			Register(Opcode.SERVER_ENTITY_DESPAWN, PacketParser.EntityDespawn);
			Register(Opcode.SERVER_ENTITY_GROUPSPAWN_BEGIN, PacketParser.EntityGroupSpawnBegin);
			Register(Opcode.SERVER_ENTITY_GROUPSPAWN_DATA, PacketParser.EntityGroupSpawnData);
			Register(Opcode.SERVER_ENTITY_GROUPSPAWN_END, PacketParser.EntityGroupSpawnEnd);
			Register(Opcode.SERVER_ENTITY_SELECTION, PacketParser.EntitySelection);
			Register(Opcode.SERVER_ENTITY_LEVEL_UP, PacketParser.EntityLevelUp);
			Register(Opcode.SERVER_ENTITY_STATUS_UPDATE, PacketParser.EntityStatusUpdate);
			Register(Opcode.SERVER_ENTITY_MOVEMENT, PacketParser.EntityMovement);
			Register(Opcode.SERVER_ENTITY_MOVEMENT_STUCK, PacketParser.EntityMovementStuck);
			Register(Opcode.SERVER_ENTITY_MOVEMENT_ANGLE, PacketParser.EntityMovementAngle);
			Register(Opcode.SERVER_ENTITY_SPEED_UPDATE, PacketParser.EntitySpeedUpdate);
			Register(Opcode.SERVER_ENTITY_STATE_UPDATE, PacketParser.EntityStateUpdate);
			Register(Opcode.SERVER_ENTITY_STALL_CREATE, PacketParser.EntityStallCreate);
			Register(Opcode.SERVER_ENTITY_STALL_DESTROY, PacketParser.EntityStallDestroy);
			Register(Opcode.SERVER_ENTITY_STALL_TITLE_UPDATE, PacketParser.EntityStallTitleUpdate);
			Register(Opcode.SERVER_ENTITY_SKILL_START, PacketParser.EntitySkillStart);
			Register(Opcode.SERVER_ENTITY_SKILL_END, PacketParser.EntitySkillEnd);
			Register(Opcode.SERVER_ENTITY_SKILL_BUFF_ADDED, PacketParser.EntitySkillBuffAdded);
			Register(Opcode.SERVER_ENTITY_SKILL_BUFF_REMOVED, PacketParser.EntitySkillBuffRemoved);
			Register(Opcode.SERVER_ENVIRONMENT_CELESTIAL_POSITION, PacketParser.EnvironmentCelestialPosition);
			Register(Opcode.SERVER_ENVIRONMENT_CELESTIAL_UPDATE, PacketParser.EnvironmentCelestialUpdate);
			Register(Opcode.SERVER_ENVIRONMENT_WEATHER_UPDATE, PacketParser.EnvironmentWeatherUpdate);
			Register(Opcode.SERVER_CHAT_UPDATE, PacketParser.ChatUpdate);
			Register(Opcode.SERVER_NOTICE_UPDATE, PacketParser.NoticeUniqueUpdate);
			Register(Opcode.SERVER_PLAYER_PETITION_REQUEST, PacketParser.PlayerPetitionRequest);
			Register(Opcode.SERVER_EXCHANGE_STARTED, PacketParser.ExchangeStarted);
			Register(Opcode.SERVER_EXCHANGE_PLAYER_CONFIRMED, PacketParser.ExchangePlayerConfirmed);
			Register(Opcode.SERVER_EXCHANGE_COMPLETED, PacketParser.ExchangeCompleted);
			Register(Opcode.SERVER_EXCHANGE_CANCELED, PacketParser.ExchangeCanceled);
			Register(Opcode.SERVER_EXCHANGE_ITEMS_UPDATE, PacketParser.ExchangeItemsUpdate);
			Register(Opcode.SERVER_EXCHANGE_GOLD_UPDATE, PacketParser.ExchangeGoldUpdate);
			Register(Opcode.SERVER_EXCHANGE_INVITATION_RESPONSE, PacketParser.ExchangeInvitationResponse);
			Register(Opcode.SERVER_EXCHANGE_CONFIRM_RESPONSE, PacketParser.ExchangeConfirmResponse);
			Register(Opcode.SERVER_EXCHANGE_APPROVE_RESPONSE, PacketParser.ExchangeApproveResponse);
			Register(Opcode.SERVER_EXCHANGE_EXIT_RESPONSE, PacketParser.ExchangeExitResponse);
			Register(Opcode.SERVER_PARTY_DATA, PacketParser.PartyData);
			Register(Opcode.SERVER_PARTY_UPDATE, PacketParser.PartyUpdate);
			Register(Opcode.SERVER_PARTY_MATCH_LIST_RESPONSE, PacketParser.PartyMatchListResponse);
			Register(Opcode.SERVER_PARTY_MATCH_DELETE_RESPONSE, PacketParser.PartyMatchDeleteResponse);
			Register(Opcode.SERVER_PARTY_MATCH_JOIN_REQUEST, PacketParser.PartyMatchJoinRequest);
			Register(Opcode.SERVER_GUILD_CREATED_DATA, PacketParser.GuildCreatedData);
			s_remotePacketHandlers[Opcode.SERVER_GUILD_DATA_BEGIN] = (agent, packet) => { PacketParser.GuildDataBegin(); return false; };
			Register(Opcode.SERVER_GUILD_DATA, PacketParser.GuildData);
			s_remotePacketHandlers[Opcode.SERVER_GUILD_DATA_END] = (agent, packet) => { PacketParser.GuildDataEnd(); return false; };
			Register(Opcode.SERVER_GUILD_UPDATE, PacketParser.GuildUpdate);
			Register(Opcode.SERVER_GUILD_STORAGE_DATA_BEGIN, PacketParser.GuildStorageDataBegin);
			Register(Opcode.SERVER_GUILD_STORAGE_DATA, PacketParser.GuildStorageData);
			Register(Opcode.SERVER_GUILD_STORAGE_DATA_END, PacketParser.GuildStorageDataEnd);
			Register(Opcode.SERVER_ACADEMY_DATA, PacketParser.AcademyData);
			Register(Opcode.SERVER_ACADEMY_MATCH_LIST_RESPONSE, PacketParser.AcademyMatchList);
			Register(Opcode.SERVER_CHARACTER_ADD_INT_RESPONSE, PacketParser.CharacterAddStatPointResponse);
			Register(Opcode.SERVER_CHARACTER_ADD_STR_RESPONSE, PacketParser.CharacterAddStatPointResponse);
			Register(Opcode.SERVER_INVENTORY_ITEM_USE, PacketParser.InventoryItemUse);

			s_remotePacketHandlers[Opcode.SERVER_INVENTORY_ITEM_MOVEMENT] = (agent, packet) => PacketParser.InventoryItemMovement(packet);

			Register(Opcode.SERVER_REPAIR_ALL_EQUIPMENTS_RESPONSE, PacketParser.RepairAllEquipmentsResponse);
			Register(Opcode.SERVER_INVENTORY_ITEM_DURABILITY_UPDATE, PacketParser.InventoryItemDurabilityUpdate);
			Register(Opcode.SERVER_INVENTORY_ITEM_UPDATE, PacketParser.InventoryItemUpdate);
			Register(Opcode.SERVER_INVENTORY_CAPACITY_UPDATE, PacketParser.InventoryCapacityUpdate);
			Register(Opcode.SERVER_STORAGE_DATA_BEGIN, PacketParser.StorageDataBegin);
			Register(Opcode.SERVER_STORAGE_DATA, PacketParser.StorageData);
			Register(Opcode.SERVER_STORAGE_DATA_END, PacketParser.StorageDataEnd);
			Register(Opcode.SERVER_CONSIGNMENT_REGISTER_RESPONSE, PacketParser.ConsignmentRegisterResponse);
			Register(Opcode.SERVER_CONSIGNMENT_UNREGISTER_RESPONSE, PacketParser.ConsignmentUnregisterResponse);
			Register(Opcode.SERVER_CONSIGNMENT_LIST_RESPONSE, PacketParser.ConsignmentListResponse);
			Register(Opcode.SERVER_CONSIGNMENT_SETTLE_RESPONSE, PacketParser.ConsignmentSettleResponse);
			Register(Opcode.SERVER_STALL_CREATE_RESPONSE, PacketParser.StallCreateResponse);
			Register(Opcode.SERVER_STALL_DESTROY_RESPONSE, PacketParser.StallDestroyResponse);
			Register(Opcode.SERVER_STALL_TALK_RESPONSE, PacketParser.StallTalkResponse);
			Register(Opcode.SERVER_STALL_BUY_RESPONSE, PacketParser.StallBuyResponse);
			Register(Opcode.SERVER_STALL_LEAVE_RESPONSE, PacketParser.StallLeaveResponse);
			Register(Opcode.SERVER_STALL_UPDATE_RESPONSE, PacketParser.StallUpdateResponse);
			Register(Opcode.SERVER_STALL_ENTITY_ACTION, PacketParser.StalleEntityAction);
			Register(Opcode.SERVER_PET_DATA, PacketParser.PetData);
			Register(Opcode.SERVER_PET_UPDATE, PacketParser.PetUpdate);
			Register(Opcode.SERVER_PET_SETTINGS_CHANGE_RESPONSE, PacketParser.PetSettingsChangeResponse);
			Register(Opcode.SERVER_PET_PLAYER_MOUNTED, PacketParser.PetPlayerMounted);
			Register(Opcode.SERVER_MASTERY_SKILL_LEVELUP_RESPONSE, PacketParser.MasterySkillLevelUpResponse);
			Register(Opcode.SERVER_MASTERY_SKILL_LEVELDOWN_RESPONSE, PacketParser.MasterySkillLevelDownResponse);
			Register(Opcode.SERVER_MASTERY_LEVELUP_RESPONSE, PacketParser.MasteryLevelUpResponse);
			Register(Opcode.SERVER_MASTERY_LEVELDOWN_RESPONSE, PacketParser.MasteryLevelDownResponse);

			s_remotePacketHandlers[Opcode.SERVER_TELEPORT_READY_REQUEST] = (agent, packet) =>
			{
				if (agent.ClientlessMode)
				{
					Packet protocol = new Packet(Opcode.CLIENT_TELEPORT_READY_RESPONSE);
					agent.InjectToServer(protocol);
				}
				return false;
			};

			Register(Opcode.SERVER_ENTITY_TALK_RESPONSE, PacketParser.EntityTalkResponse);
			Register(Opcode.SERVER_NPC_CLOSE_RESPONSE, PacketParser.NpcCloseResponse);
			Register(Opcode.SERVER_LOGOUT_RESPONSE, PacketParser.LogoutResponse);
			Register(Opcode.SERVER_LOGOUT_CANCEL_RESPONSE, PacketParser.LogoutCancelResponse);
			Register(Opcode.SERVER_LOGOUT_SUCCESS, PacketParser.LogoutSuccess);
			Register(Opcode.SERVER_RENAME_RESPONSE, PacketParser.RenameResponse);
			Register(Opcode.SERVER_CHAT_RESPONSE, PacketParser.ChatAck);
			Register(Opcode.SERVER_CHAT_RESTRICT, PacketParser.ChatRestrict);
			Register(Opcode.SERVER_EVENT_QUEST_ID_RESPONSE, PacketParser.EventQuestIdResponse);
			Register(Opcode.SERVER_QUEST_TALK, PacketParser.QuestTalk);
			Register(Opcode.SERVER_QUEST_UPDATE, PacketParser.QuestUpdate);
			Register(Opcode.SERVER_QUEST_ABANDON_RESPONSE, PacketParser.QuestAbandonResponse);
			Register(Opcode.SERVER_QUEST_SCRIPT, PacketParser.QuestScript);
			Register(Opcode.SERVER_TELEPORT_USE_RESPONSE, PacketParser.TeleportUseResponse);
			Register(Opcode.SERVER_TELEPORT_RECALL_RESPONSE, PacketParser.TeleportRecallResponse);
			Register(Opcode.SERVER_PARTY_INVITATION_RESPONSE, PacketParser.PartyInvitationResponse);
			Register(Opcode.SERVER_PARTY_MATCH_CREATION_RESPONSE, PacketParser.PartyMatchCreationResponse);
			Register(Opcode.SERVER_PARTY_MATCH_EDITED_RESPONSE, PacketParser.PartyMatchEditedResponse);
			Register(Opcode.SERVER_GUILD_STORAGE_RESPONSE, PacketParser.GuildStorageResponse);
			Register(Opcode.SERVER_GUILD_PLAYER_LOG, PacketParser.GuildPlayerLog);
			Register(Opcode.SERVER_MAIL_SEND_RESPONSE, PacketParser.MailSendResponse);
			Register(Opcode.SERVER_ENTITY_DISPLAY_EFFECT, PacketParser.EntityDisplayEffect);
			Register(Opcode.SERVET_ENTITY_INVENTORY_EQUIP, PacketParser.EntityInventoryEquip);
			Register(Opcode.SERVER_ENTITY_EMOTE_USE, PacketParser.EntityEmoteUse);
			Register(Opcode.SERVER_DROP_UNLOCKED, PacketParser.DropUnlocked);

			s_remotePacketHandlers[Opcode.SERVER_CHARACTER_ACTION_RESPONSE] = (agent, packet) => false;
		}

		/// <summary>
		/// Analyze server packets using the registered opcode handler dictionary.
		/// </summary>
		/// <param name="packet">Server packet</param>
		/// <returns>True if the packet will be ignored by the client</returns>
		private bool Remote_PacketHandler(Packet packet)
		{
			if (s_remotePacketHandlers.TryGetValue(packet.Opcode, out var handler))
			{
				return handler(this, packet);
			}

			// Tanımlı ama işlenmeyen opcode'lar sessizce yutulmasın — teşhis için seyrek logla
			LogUnknownOpcode(packet.Opcode);
			return false;
		}
		/// <summary>
		/// Inject a packet to the server. The delay is used to avoid locking the current thread.
		/// </summary>
		/// <param name="p">Packet with the info</param>
		/// <param name="delay">Delay in miliseconds to be executed in other thread</param>
		public void InjectToServer(Packet p, int delay = 0)
		{
			if (delay > 0)
			{
				System.Threading.ThreadPool.QueueUserWorkItem(delegate {
					System.Threading.Thread.Sleep(delay);
					try
					{
						if (Remote != null && Remote.Socket != null && Remote.Socket.Connected)
							Remote.Security.Send(p);
					}
					catch (Exception ex)
					{
						App.Window.Get?.Log($"[Delayed packet warning] {ex.Message}");
					}
				});
				return;
			}
			{
				byte[] bytes = p.GetBytes();
				string hexPreview = "";
				if (ModernLogger.EnablePacketTrace)
				{
					hexPreview = Utility.HexDump(bytes).Replace("\r", "").Replace("\n", " ").Trim();
					if (hexPreview.Length > 80) hexPreview = hexPreview.Substring(0, 80) + "...";
				}
				ModernLogger.TracePacket("Bot->Server", p.Opcode, bytes.Length, hexPreview);
			}
			Remote.Security.Send(p);
		}
		public void InjectToClient(Packet p)
		{
			this.Remote.RelaySecurity.Send(p);
		}
	}
}
