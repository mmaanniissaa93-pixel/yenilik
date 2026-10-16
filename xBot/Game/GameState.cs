using System;
using System.Collections.Generic;
using System.Threading;
using xBot.App;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Guild;
using xBot.Game.Objects.Item;
using xBot.Game.Objects.Party;

namespace xBot.Game
{
	/// <summary>
	/// Encapsulates the entire runtime game and session state for a Silkroad character.
	/// Enables multiple simultaneous sessions (multi-client) and headless test mocking.
	/// </summary>
	public class GameState
	{
		#region (Server and Identification)
		public string ServerName { get; set; } = string.Empty;
		public string ServerID { get; set; } = string.Empty;
		public string CharName { get; set; } = string.Empty;
		public SRCharacter Character { get; set; }
		public bool inTeleport { get; set; }
		public bool inGame { get; set; }
		public DateTime JoinedGameUtc { get; set; } = DateTime.MinValue;
		public uint SelectedEntityUniqueID { get; set; }
		public uint LastNpcTalkEntityUniqueID { get; set; }
		public byte LastNpcTalkID { get; set; }
		public bool LastRepairSuccess { get; set; }
		public ushort LastRepairErrorCode { get; set; }
		public byte[] LastConsignmentListPayload { get; set; } = new byte[0];
		public DateTime LastConsignmentListTime { get; set; } = DateTime.MinValue;
		public bool LastConsignmentRegisterSuccess { get; set; }
		public ushort LastConsignmentRegisterError { get; set; }
		public DateTime LastConsignmentRegisterTime { get; set; } = DateTime.MinValue;
		public List<ConsignmentListing> LastConsignmentListings { get; set; } = new List<ConsignmentListing>();
		public bool LastConsignmentUnregisterSuccess { get; set; }
		public bool LastConsignmentSettleSuccess { get; set; }
		public bool LastStallCreateSuccess { get; set; }
		public bool LastStallUpdateSuccess { get; set; }
		public SRTypes.StallUpdate LastStallUpdateType { get; set; }
		#endregion

		#region (Entities and Dictionaries)
		public xDictionary<uint, SREntity> Entities { get; } = new xDictionary<uint, SREntity>();
		public xDictionary<string, SRPlayer> Players { get; } = new xDictionary<string, SRPlayer>();
		public xDictionary<uint, SRCoService> MyPets { get; } = new xDictionary<uint, SRCoService>();
		public xDictionary<uint, SRNpc> Npcs { get; } = new xDictionary<uint, SRNpc>();
		public xDictionary<uint, SRTeleport> TeleportAndBuildings { get; } = new xDictionary<uint, SRTeleport>();
		public xDictionary<uint, SRMob> Mobs { get; } = new xDictionary<uint, SRMob>();
		public xDictionary<uint, SRBuff> Buffs { get; } = new xDictionary<uint, SRBuff>();
		#endregion

		#region (Social and Group)
		public SRParty Party { get; set; }
		public bool inParty => Party != null;
		public SRGuild Guild { get; set; }
		public bool inGuild => Guild != null;
		public bool inAcademy { get; set; }
		public bool inStall { get; set; }
		public bool IsOwnStallOpen { get; set; }
		public SRPlayer StallerPlayer { get; set; }
		public bool inExchange => ExchangerPlayer != null;
		public SRPlayer ExchangerPlayer { get; set; }
		public bool isExchangerConfirmed { get; set; }
		#endregion

		#region (Environment)
		public byte GameHour { get; set; }
		public byte GameMinute { get; set; }
		public ushort Moonphase { get; set; }
		public byte WeatherType { get; set; }
		public byte WeatherIntensity { get; set; }
		#endregion

		#region (Storage, Pet, Quest, Combat)
		public bool isStorageLoaded { get; set; }
		public bool inStorage { get; set; }
		public DateTime LastStorageInfoTime { get; set; } = DateTime.MinValue;
		public DateTime LastGuildStorageInfoTime { get; set; } = DateTime.MinValue;
		public byte LastGuildStorageResult { get; set; } = byte.MaxValue;
		public bool LastPetMountSuccess { get; set; }
		public bool LastPetMountState { get; set; }
		public uint LastPetMountUniqueID { get; set; }
		public uint LastPetRemovedUniqueID { get; set; }
		public uint LastEventQuestId { get; set; }
		public bool LastQuestAbandonSuccess { get; set; }
		public uint LastQuestAbandonId { get; set; }
		public DateTime LastQuestUpdateTime { get; set; } = DateTime.MinValue;
		public bool LastSkillCastSuccess { get; set; } = true;
		public ushort LastSkillCastErrorCode { get; set; } = 0;
		public CharacterActionTracker CharacterActions { get; } = new CharacterActionTracker();
		public SRTimeStamp TimeStamp { get; set; }
		public DateTime TimeStampDate { get; set; } = DateTime.MinValue;
		public int StallViewsCount { get; set; }
		public ulong StallEarnings { get; set; }
		#endregion

		#region (Monitors)
		public AutoResetEvent MonitorEntitySelected { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorWeaponChanged { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorInventoryMovement { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorRepairResponse { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorConsignmentList { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorConsignmentRegister { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorConsignmentUnregister { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorConsignmentSettle { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorStallCreate { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorStallUpdate { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorBuffRemoved { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorSkillCast { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorMobSpawnChanged { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorNpcTalk { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorGuildStorageResponse { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorGuildStorageData { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorPetMountResponse { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorPetRemoved { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorEventQuestId { get; } = new AutoResetEvent(false);
		public AutoResetEvent MonitorQuestAbandon { get; } = new AutoResetEvent(false);
		#endregion

		/// <summary>
		/// Resets runtime state for disconnects or fresh session starts.
		/// </summary>
		public virtual void Reset()
		{
			Character = null;
			inGame = false;
			inTeleport = false;
			JoinedGameUtc = DateTime.MinValue;
			SelectedEntityUniqueID = 0;
			LastNpcTalkEntityUniqueID = 0;
			LastNpcTalkID = 0;
			LastRepairSuccess = false;
			LastRepairErrorCode = 0;
			LastConsignmentListPayload = new byte[0];
			LastConsignmentListTime = DateTime.MinValue;
			LastConsignmentRegisterSuccess = false;
			LastConsignmentRegisterError = 0;
			LastConsignmentRegisterTime = DateTime.MinValue;
			LastConsignmentListings.Clear();
			LastConsignmentUnregisterSuccess = false;
			LastConsignmentSettleSuccess = false;
			LastStallCreateSuccess = false;
			LastStallUpdateSuccess = false;
			Party = null;
			Guild = null;
			inAcademy = false;
			inStall = false;
			IsOwnStallOpen = false;
			StallerPlayer = null;
			ExchangerPlayer = null;
			isExchangerConfirmed = false;
			GameHour = 0;
			GameMinute = 0;
			Moonphase = 0;
			WeatherType = 0;
			WeatherIntensity = 0;
			isStorageLoaded = false;
			inStorage = false;
			LastStorageInfoTime = DateTime.MinValue;
			LastGuildStorageInfoTime = DateTime.MinValue;
			LastGuildStorageResult = byte.MaxValue;
			LastPetMountSuccess = false;
			LastPetMountState = false;
			LastPetMountUniqueID = 0;
			LastPetRemovedUniqueID = 0;
			LastEventQuestId = 0;
			LastQuestAbandonSuccess = false;
			LastQuestAbandonId = 0;
			LastQuestUpdateTime = DateTime.MinValue;
			LastSkillCastSuccess = true;
			LastSkillCastErrorCode = 0;
			CharacterActions.Reset();
			TimeStamp = null;
			TimeStampDate = DateTime.MinValue;
			StallViewsCount = 0;
			StallEarnings = 0;

			Entities.Clear();
			Players.Clear();
			MyPets.Clear();
			Npcs.Clear();
			TeleportAndBuildings.Clear();
			Mobs.Clear();
			Buffs.Clear();
		}
	}
}
