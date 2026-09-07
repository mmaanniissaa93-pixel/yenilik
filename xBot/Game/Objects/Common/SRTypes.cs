using System;

namespace xBot.Game.Objects.Common
{
	public static class SRTypes
	{
		public enum Race : byte
		{
			Chinese = 0,
			European = 1,
			Unknown = 0xFF,
		}

		public enum CharacterSelectionAction : byte
		{
			Create = 1,
			List = 2,
			Delete = 3,
			CheckName = 4,
			Restore = 5
		}
		/// <summary>
		/// All basic actions that character can do.
		/// </summary>
		public enum CharacterAction : byte
		{
			CommonAttack = 1,
			ItemPickUp = 2,
			SkillCast = 4,
			SkillRemove = 5
		}
		public enum Weapon : byte
		{
			None = byte.MaxValue,
			Sword = 2,
			Blade = 3,
			Spear = 4,
			Glaive = 5,
			Bow = 6,
			OneHandSword = 7,
			TwoHandSword = 8,
			DualAxes = 9,
			Warlock = 10,
			TwoHandStaff = 11,
			Crossbow = 12,
			Daggers = 13,
			Harp = 14,
			Cleric = 15
		}
		public enum Chat : byte
		{
			All = 1,
			Private = 2,
			GM = 3,
			Party = 4,
			Guild = 5,
			Global = 6,
			Notice = 7,
			Stall = 9,
			Union = 11,
			NPC = 13,
			Academy = 16
		}
		public enum EntityStateUpdate : byte
		{
			HP = 1,
			MP = 2,
			HPMP = 3,
			BadStatus = 4,
			EntityHPMP = 5
		}
		/// <summary>
		/// srodevs-docs: AGENT_ENTITY_STATE_UPDATE (0x30BF) UpdateType.
		/// </summary>
		public enum EntityStateUpdateKind : byte
		{
			LifeState = 0,
			MotionState = 1,
			BodyState = 4,
			CombatState = 7,
			InCombat = 8,
			Scrolling = 11
		}
		/// <summary>
		/// srodevs-docs: AGENT_CHARACTER_BODYSTATE_REQ (0x70A7).
		/// </summary>
		public enum EntityBodyState : byte
		{
			None = 0,
			Berserk = 1,
			Untouchable = 2,
			GameMasterInvincible = 3,
			GameMasterUntouchable = 4,
			GameMasterInvisible = 5,
			Stealth = 6,
			Invisible = 7
		}
		/// <summary>
		/// srodevs-docs: AGENT_GAME_LOGOUT_REQ (0x7005) LogoutMode.
		/// </summary>
		public enum LogoutMode : byte
		{
			Exit = 1,
			Restart = 2
		}
		public enum LogoutErrorCode : ushort
		{
			CantLogoutInBattle = 0x801,
			CantLogoutWhileTeleporting = 0x802
		}
		/// <summary>
		/// srodevs-docs: AGENT_CHARACTER_SELECTION_RENAME_REQ (0x7450).
		/// </summary>
		public enum CharacterRenameAction : byte
		{
			CharacterRename = 1,
			GuildRename = 2,
			GuildNameCheck = 3
		}
		/// <summary>
		/// srodevs-docs: AGENT_CHARACTER_INFO_UPDATE (0x304E) UpdateType.
		/// </summary>
		public enum CharacterInfoUpdateType : byte
		{
			Gold = 1,
			SP = 2,
			STP = 3,
			Hwan = 4,
			EgyptAP = 16
		}
		/// <summary>
		/// srodevs-docs: AGENT_CHAT_ACK (0xB025) ChatErrorCode.
		/// </summary>
		public enum ChatErrorCode : ushort
		{
			CantFindTarget = 3,
			YouAreSqueltched = 0x2006,
			InvalidCommand = 0x2008,
			NotAPartyMember = 0x200A,
			AlliancePermissionDenied = 0x200B,
			CantChatting = 0x200D,
			UnionChatLimit = 0x200E
		}
		/// <summary>
		/// srodevs-docs: GATEWAY_LOGIN_ACK (0xA102) LoginErrorCode / LoginBlockType.
		/// </summary>
		public enum GatewayLoginError : byte
		{
			InvalidCredentials = 1,
			Blocked = 2,
			AlreadyConnected = 4,
			ServerIsFull = 6,
			IPLimit = 0xB,
			BillingFailed = 0xC,
			BillingRelated = 0xD,
			AdultOnly = 0xE,
			TeenOverOnly = 0xF,
			TeenServerAdultDenied = 0x10
		}
		public enum GatewayLoginBlockType : byte
		{
			Punishment = 1,
			AccountInspection = 2,
			NoAccountInfo = 3,
			FreeServiceOver = 4
		}
		/// <summary>
		/// Players interacting petitions.
		/// </summary>
		public enum PlayerPetition : byte
		{
			ExchangeRequest = 1,
			PartyCreation = 2,
			PartyInvitation = 3,
			Resurrection = 4,
			GuildInvitation = 5,
			UnionInvitation = 6, // Not confirmed
			ResurrectionAgain = 8, // srodevs-docs agent_game_invite: ikinci Resurrection değeri
			AcademyInvitation = 9, // Not confirmed
			GuildWar = 10 // srodevs-docs agent_game_invite
		}
		/// <summary>
		/// srodevs-docs: AGENT_LOGIN_AUTH_ACK (0xA103) AuthenticationErrorCode.
		/// </summary>
		public enum AgentAuthError : byte
		{
			ConnectionErrorC9 = 1,
			ConnectionErrorC10 = 2,
			ServerIsFull = 4,
			IPLimit = 5
		}
		/// <summary>
		/// srodevs-docs: agent_environment_weather_update (0x3809) WeatherType.
		/// </summary>
		public enum WeatherType : byte
		{
			Clear = 1,
			Rain = 2,
			Snow = 3
		}
		/// <summary>
		/// srodevs-docs: GATEWAY_PATCH_ACK (0xA100) PatchErrorCode.
		/// </summary>
		public enum GatewayPatchError : byte
		{
			InvalidVersion = 1,
			Update = 2,
			NotInService = 3,
			AbnormalModule = 4,
			PatchDisabled = 5
		}
		
		public enum SetPart : byte
		{
			Head = 1,
			Shoulders = 2,
			Chest = 3,
			Pants = 4,
			Gloves = 5,
			Boots = 6
		}
		public enum AvatarPart : byte
		{
			Hat = 1,
			Dress = 2,
			Accessory = 3,
			Flag = 4
		}
		public enum AccesoriesPart : byte
		{
			Earring = 1,
			Necklace = 2,
			Ring = 3
		}
		/// <summary>
		/// Character inventory item movement.
		/// </summary>
		public enum InventoryItemMovement : byte
		{
			InventoryToInventory = 0,
			StorageToStorage = 1,
			InventoryToStorage = 2,
			StorageToInventory = 3,

			InventoryToExchange = 4,
			ExchangeToInventory = 5,

			GroundToInventory = 6,
			InventoryToGround = 7,

			ShopToInventory = 8,
			InventoryToShop = 9,

			InventoryGoldToGround = 10,
			StorageGoldToInventory = 11,
			InventoryGoldToStorage = 12,
			InventoryGoldToExchange = 13,

			QuestToInventory = 14,
			InventoryToQuest = 15,

			TransportToTransport = 16,
			GroundToPet = 17,
			ShopToTransport = 19,
			TransportToShop = 20,

			PetToPet = 25,
			PetToInventory = 26,
			InventoryToPet = 27,
			GroundToPetToInventory = 28,

			GuildToGuild = 29,
			InventoryToGuild = 30,
			GuildToInventory = 31,
			InventoryGoldToGuild = 32,
			GuildGoldToInventory = 33,

			ShopBuyBack = 34,

			AvatarToInventory = 35,
			InventoryToAvatar = 36
		}
		public enum StallUpdate:byte
		{
			ItemUpdate = 1,
			ItemAdded = 2,
			ItemRemoved = 3,
			FleaMarketMode = 4, // ???
			State = 5,
			Note = 6,
			Title = 7
		}
		public enum SkillCast : byte
		{
			Buff = 0,
			Attack = 2
		}
		[Flags]
		public enum DamageEffect : byte
		{
			None = 0,
			KnockBack = 1,
			Block = 2,
			Position = 4,
			Cancel = 8,
            Dead = 128
        }
		[Flags]
		public enum Damage : byte
		{
			None = 0,
			Normal = 1,
			Critical = 2,
			Status = 4
		}

		/// <summary>
		/// Game consola commands.
		/// </summary>
		public enum GMCommandAction : ushort
		{
			FindUser = 1,
			GoTown = 2,
			ToTown = 3,
			WorldStatus = 4,
			CreateMob = 6,
			MakeItem = 7,
			MoveToUser = 8,
			Warp = 10,
			Zoe = 12,
			Ban = 13,
			Invisible = 14,
			Invincible = 15,
			RecallUser = 17,
			RecallGuild = 18,
			LieName = 19,
			KillMob = 20,
			ResetQ = 28,
			MoveToNPC = 31,
			MakeRentItem = 38,
			SpawnUniqueLocation = 42
		}

		#region (Extensions)
		public static bool HasFlags(this ulong flags, ulong desiredFlags)
		{
			return (flags & desiredFlags) != 0;
		}
		public static bool HasFlags(this uint flags, uint desiredFlags)
		{
			return (flags & desiredFlags) != 0;
		}
		public static bool HasFlags(this ushort flags, ushort desiredFlags)
		{
			return (flags & desiredFlags) != 0;
		}
		public static bool HasFlags(this byte flags, byte desiredFlags)
		{
			return (flags & desiredFlags) != 0;
		}
		#endregion
	}
}
