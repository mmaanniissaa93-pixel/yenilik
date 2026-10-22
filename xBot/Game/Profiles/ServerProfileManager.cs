using System;
using System.Collections.Generic;

namespace xBot.Game.Profiles
{
	public static class ServerProfileManager
	{
		private static readonly Dictionary<ServerType, ServerProfile> s_profiles = new Dictionary<ServerType, ServerProfile>();

		static ServerProfileManager()
		{
			// 1. vSRO (Standart Private / PvP Sunucuları)
			s_profiles[ServerType.vSRO] = new ServerProfile
			{
				Type = ServerType.vSRO,
				DisplayName = "vSRO (Private / PvP Server)",
				Description = "Standart v1.188 / 80-140 Cap Private sunucuları. Klasik Joymax paket yapısı.",
				DefaultLocale = 22,
				DefaultPort = 15779,
				HasSilkroadRHeader21 = false,
				Fixed8ByteSockets = false,
				HasJobPouchByte = false,
				SilkroadRQuestFormat = false,
				HasExtraStateByte = false,
				HasExtraSpawnMobDword = false,
				HasExtendedSkillCastTargetId = false,
				ModernTrafficFormat = false,
				RequiresSecondaryPasscode = false,
				RequiresXigncodePatch = false,
				AutoResolveDomainToIPv4 = true
			};

			// 2. Silkroad-R (Elysian Silkroad-R, Joymax SRO-R vb.)
			s_profiles[ServerType.SilkroadR] = new ServerProfile
			{
				Type = ServerType.SilkroadR,
				DisplayName = "Silkroad-R (Elysian & SRO-R)",
				Description = "Silkroad-R motoru kullanan özel veya resmi sunucular (21B başlık, 8B soket, Fellow Pet).",
				DefaultLocale = 65,
				DefaultPort = 15779,
				HasSilkroadRHeader21 = true,
				Fixed8ByteSockets = true,
				HasJobPouchByte = true,
				SilkroadRQuestFormat = false,
				HasModernQuestLayout = true,
				HasExtraStateByte = true,
				HasExtraSpawnMobDword = true,
				HasExtendedSkillCastTargetId = true,
				Uses32BitItemUsage = true,
				ModernTrafficFormat = true,
				RequiresSecondaryPasscode = false,
				RequiresXigncodePatch = false,
				AutoResolveDomainToIPv4 = true
			};

			// 3. TRSRO (Silkroad Online Türkiye - GameGami)
			s_profiles[ServerType.TRSRO] = new ServerProfile
			{
				Type = ServerType.TRSRO,
				DisplayName = "TRSRO (Silkroad Online Türkiye)",
				Description = "GameGami resmi Türkiye sunucusu (Xigncode3, 0x7625 PIN, güncel motor).",
				DefaultLocale = 56,
				DefaultPort = 15779,
				HasSilkroadRHeader21 = true,
				Fixed8ByteSockets = true,
				HasJobPouchByte = true,
				SilkroadRQuestFormat = true,
				HasModernQuestLayout = false,
				HasExtraStateByte = true,
				HasExtraSpawnMobDword = true,
				HasExtendedSkillCastTargetId = true,
				Uses32BitItemUsage = true,
				ModernTrafficFormat = true,
				RequiresSecondaryPasscode = true,
				RequiresXigncodePatch = true,
				AutoResolveDomainToIPv4 = true
			};

			// 4. iSRO (International Silkroad - Joymax / Webzen)
			s_profiles[ServerType.iSRO] = new ServerProfile
			{
				Type = ServerType.iSRO,
				DisplayName = "iSRO (International Silkroad)",
				Description = "Joymax / Webzen resmi global sunucusu.",
				DefaultLocale = 18,
				DefaultPort = 15779,
				HasSilkroadRHeader21 = true,
				Fixed8ByteSockets = true,
				HasJobPouchByte = true,
				SilkroadRQuestFormat = false,
				HasModernQuestLayout = true,
				HasExtraStateByte = true,
				HasExtraSpawnMobDword = true,
				HasExtendedSkillCastTargetId = true,
				Uses32BitItemUsage = true,
				ModernTrafficFormat = true,
				RequiresSecondaryPasscode = true,
				RequiresXigncodePatch = true,
				AutoResolveDomainToIPv4 = true
			};

			// 5. kSRO (Korea Silkroad - Joymax)
			s_profiles[ServerType.kSRO] = new ServerProfile
			{
				Type = ServerType.kSRO,
				DisplayName = "kSRO (Kore Silkroad)",
				Description = "Joymax Kore orijinal sunucusu.",
				DefaultLocale = 1,
				DefaultPort = 15779,
				HasSilkroadRHeader21 = false,
				Fixed8ByteSockets = false,
				HasJobPouchByte = false,
				SilkroadRQuestFormat = false,
				HasExtraStateByte = false,
				HasExtraSpawnMobDword = false,
				HasExtendedSkillCastTargetId = true,
				ModernTrafficFormat = true,
				RequiresSecondaryPasscode = false,
				RequiresXigncodePatch = true,
				AutoResolveDomainToIPv4 = true
			};

			// 6. cSRO (China Silkroad)
			s_profiles[ServerType.cSRO] = new ServerProfile
			{
				Type = ServerType.cSRO,
				DisplayName = "cSRO (Çin Silkroad)",
				Description = "Çin sunucuları (cSRO / cSRO-R).",
				DefaultLocale = 34,
				DefaultPort = 15779,
				HasSilkroadRHeader21 = false,
				Fixed8ByteSockets = false,
				HasJobPouchByte = false,
				SilkroadRQuestFormat = false,
				HasExtraStateByte = false,
				HasExtraSpawnMobDword = false,
				HasExtendedSkillCastTargetId = false,
				ModernTrafficFormat = true,
				RequiresSecondaryPasscode = false,
				RequiresXigncodePatch = false,
				AutoResolveDomainToIPv4 = true
			};

			// 7. jSRO (Japan Silkroad)
			s_profiles[ServerType.jSRO] = new ServerProfile
			{
				Type = ServerType.jSRO,
				DisplayName = "jSRO (Japon Silkroad)",
				Description = "Japon sunucuları.",
				DefaultLocale = 3,
				DefaultPort = 15779,
				HasSilkroadRHeader21 = false,
				Fixed8ByteSockets = false,
				HasJobPouchByte = false,
				SilkroadRQuestFormat = false,
				HasExtraStateByte = false,
				HasExtraSpawnMobDword = false,
				HasExtendedSkillCastTargetId = false,
				ModernTrafficFormat = true,
				RequiresSecondaryPasscode = false,
				RequiresXigncodePatch = false,
				AutoResolveDomainToIPv4 = true
			};
		}

		private static ServerProfile s_currentProfile;
		public static ServerProfile CurrentProfile
		{
			get => s_currentProfile ?? GetProfile(ServerType.vSRO);
			set => s_currentProfile = value;
		}

		public static ServerProfile GetProfile(ServerType type)
		{
			if (s_profiles.TryGetValue(type, out ServerProfile profile))
				return profile;
			return s_profiles[ServerType.vSRO];
		}

		public static IEnumerable<ServerProfile> GetAllProfiles() => s_profiles.Values;

		/// <summary>
		/// Locale, sürüm veya dosya yollarına bakarak sunucu tipini otomatik tahmin eder.
		/// </summary>
		public static ServerType AutoDetect(byte locale, uint version = 0, string serverName = "", string clientPath = "")
		{
			string combined = ((serverName ?? "") + " " + (clientPath ?? "")).ToLowerInvariant();

			if (locale == 65 || combined.Contains("elysian") || combined.Contains("silkroad-r") || combined.Contains("sro-r"))
				return ServerType.SilkroadR;

			if (locale == 56 || combined.Contains("trsro") || combined.Contains("gamegami") || combined.Contains("turkey") || combined.Contains("türkiye"))
				return ServerType.TRSRO;

			if (locale == 18)
				return ServerType.iSRO;

			if (locale == 1 || combined.Contains("ksro"))
				return ServerType.kSRO;

			if (locale == 34 || locale == 38 || combined.Contains("csro"))
				return ServerType.cSRO;

			if (locale == 3 || combined.Contains("jsro"))
				return ServerType.jSRO;

			return ServerType.vSRO;
		}
	}
}
