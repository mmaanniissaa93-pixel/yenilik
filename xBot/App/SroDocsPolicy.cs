using System;

namespace xBot.App
{
	/// <summary>
	/// srodevs-docs (C:/srodevs-docs) tabanlı saf karar/mesaj politikası.
	/// WinForms/Bot bağımlılığı yoktur; tests/ProtectionScenarios tarafından doğrulanır.
	/// Kaynaklar: packets/gateway/gateway_login_ack.md, packets/agent/agent_chat_ack.md,
	/// agent_game_logout_req/ack.md, agent_character_selection_rename_ack.md,
	/// agent_character_bodystate_req.md, agent_character_exp_update.md,
	/// agent_character_info_update.md, agent_entity_state_update.md.
	/// </summary>
	public static class SroDocsPolicy
	{
		[Flags]
		public enum TCBuffUpdateMask : byte
		{
			Cumulated = 0x0F,
			Accumulated = 0xF0
		}

		public static bool HasCumulated(byte flags)
		{
			return (flags & (byte)TCBuffUpdateMask.Cumulated) != 0;
		}

		public static bool HasAccumulated(byte flags)
		{
			return (flags & (byte)TCBuffUpdateMask.Accumulated) != 0;
		}

		public static bool IsKnownBodyState(byte value)
		{
			return value <= 7;
		}

		public static bool IsKnownInfoUpdateType(byte value)
		{
			return value == 1 || value == 2 || value == 3 || value == 4 || value == 16;
		}

		public static bool IsKnownStateUpdateKind(byte value)
		{
			return value == 0 || value == 1 || value == 4 || value == 7 || value == 8 || value == 11;
		}

		public static string GetLoginErrorMessage(byte errorCode, byte blockType = 0)
		{
			switch (errorCode)
			{
				case 1: return "Password entry has failed (InvalidCredentials).";
				case 2:
					switch (blockType)
					{
						case 1: return "Account blocked (Punishment).";
						case 2: return "Cannot connect: server inspection (AccountInspection).";
						case 3: return "Account info missing (NoAccountInfo).";
						case 4: return "Free service is over (FreeServiceOver).";
						default: return "Account blocked (Blocked).";
					}
				case 4: return "This user is already connected. Please try again in 5 minutes.";
				case 6: return "The server is full, please try again later.";
				case 0xB: return "IP limit exceeded.";
				case 0xC: return "Billing failed (period expired).";
				case 0xD: return "Billing server error.";
				case 0xE: return "Only adults over 18 may connect.";
				case 0xF: return "Only users over 12 may connect.";
				case 0x10: return "Adults may not connect to the Teen server.";
				default: return "Login error [" + errorCode + "]";
			}
		}

		public static string GetAuthErrorMessage(byte code)
		{
			switch (code)
			{
				case 1: return "Failed to connect to the server (C9).";
				case 2: return "Failed to connect to the server (C10).";
				case 4: return "The server is full, please try again later.";
				case 5: return "IP limit exceeded (insufficient IP).";
				default: return "Authentication error [" + code + "]";
			}
		}

		public static string GetPatchErrorMessage(byte code)
		{
			switch (code)
			{
				case 1: return "Invalid version.";
				case 2: return "Update required.";
				case 3: return "Not in service.";
				case 4: return "Abnormal module.";
				case 5: return "Patch disabled.";
				default: return "Patch error [" + code + "]";
			}
		}

		public static bool IsKnownWeatherType(byte value)
		{
			return value == 1 || value == 2 || value == 3;
		}

		public static bool IsKnownPetitionType(byte value)
		{
			return value == 1 || value == 2 || value == 3 || value == 4 || value == 5 ||
				value == 6 || value == 8 || value == 9 || value == 10;
		}

		public static string GetLogoutErrorMessage(ushort code)
		{
			switch (code)
			{
				case 0x801: return "Cannot close the game during combat.";
				case 0x802: return "Cannot exit the game while teleporting.";
				default: return "Logout error [0x" + code.ToString("X4") + "]";
			}
		}

		public static string GetChatErrorMessage(ushort code)
		{
			switch (code)
			{
				case 3: return "Cannot find chat target.";
				case 0x2006: return "Whisper is turned off.";
				case 0x2008: return "Invalid chat command.";
				case 0x200A: return "Not in the party status.";
				case 0x200B: return "No ally chat permission.";
				case 0x200D: return "Chat restricted.";
				case 0x200E: return "No union chatting authority.";
				default: return "Chat error [0x" + code.ToString("X4") + "]";
			}
		}

		public static string GetRenameErrorMessage(byte action, ushort code)
		{
			if (action == 1)
			{
				switch (code)
				{
					case 6: return "This ID already exists.";
					case 7: return "Invalid character name.";
				}
			}
			else if (action == 2)
			{
				switch (code)
				{
					case 6: return "The selected guild name already exists.";
					case 7: return "The guild name cannot be created.";
				}
			}
			return "Rename error [action=" + action + " code=" + code + "]";
		}

		public static double AngleToDegrees(short angle)
		{
			// srodevs-docs agent_entity_rotation_req: [0,32767] -> [0,180], negative -> [180,360].
			if (angle >= 0)
				return angle * 180.0 / 32767.0;
			return 360.0 + angle * 180.0 / 32767.0;
		}
	}
}
