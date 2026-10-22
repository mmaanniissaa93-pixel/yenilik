using System;

namespace xBot.Game.Profiles
{
	public class ServerProfile
	{
		public ServerType Type { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public byte DefaultLocale { get; set; }
		public ushort DefaultPort { get; set; }

		#region (0x3013 Character Data Rules)
		/// <summary>
		/// PVP Cape sonrası 21 baytlık Silkroad-R başlığı var mı?
		/// </summary>
		public bool HasSilkroadRHeader21 { get; set; }

		/// <summary>
		/// Eşyalarda soketler 8 bayt (4 x ushort) sabit blok mu? (Silkroad-R'da Adv Elixir bloğu yoktur)
		/// false ise klasik vSRO: soket sayısı + soket listesi + adv sayısı + adv listesi.
		/// </summary>
		public bool Fixed8ByteSockets { get; set; }

		/// <summary>
		/// Avatar envanteri sonrası ekstra meslek kesesi (job pouch) baytı var mı?
		/// </summary>
		public bool HasJobPouchByte { get; set; }

		/// <summary>
		/// Görev bölümü Silkroad-R formatında mı? (8 bayt başlık + Silkroad-R görev dizisi)
		/// false ise klasik vSRO: ushort tamamlanan sayısı + uint ID dizisi + aktif görevler.
		/// </summary>
		public bool SilkroadRQuestFormat { get; set; }

		/// <summary>
		/// Aktif görevlerde 6 baytlık başlık (State + 2B Time) ve her görev sonunda NPC listesi var mı? (Elysian Silkroad-R vb.)
		/// </summary>
		public bool HasModernQuestLayout { get; set; }

		/// <summary>
		/// Hareket hızlarından önce ekstra 1 durum baytı var mı?
		/// </summary>
		public bool HasExtraStateByte { get; set; }

		/// <summary>
		/// 0x3015 Canavar spawn paketinde ekstra 4 baytlık mob DWORD var mı?
		/// </summary>
		public bool HasExtraSpawnMobDword { get; set; }

		/// <summary>
		/// 0xB070 skill-start packet has an extra DWORD before the target UID.
		/// </summary>
		public bool HasExtendedSkillCastTargetId { get; set; }

		/// <summary>
		/// 0x704C Eşya kullanım paketi 32-bit (4 bayt) TID/Usage mı kullanıyor? (Silkroad-R, TRSRO).
		/// false ise klasik vSRO: 16-bit ushort (2 bayt) kullanım tipi.
		/// </summary>
		public bool Uses32BitItemUsage { get; set; }
		#endregion

		#region (Gateway & Auth Rules)
		/// <summary>
		/// Sunucu listesi (0xA101) 2 baytlık modern trafik formatında mı?
		/// false ise klasik vSRO (6+ bayt: players, maxPlayers, isAvailable, farmID).
		/// </summary>
		public bool ModernTrafficFormat { get; set; }

		/// <summary>
		/// İkinci şifre (0x7625/0xB625) zorunlu mu? (TRSRO, iSRO)
		/// </summary>
		public bool RequiresSecondaryPasscode { get; set; }

		/// <summary>
		/// Xigncode3 bellek bypass gerektirir mi?
		/// </summary>
		public bool RequiresXigncodePatch { get; set; }

		/// <summary>
		/// DLL enjeksiyonunda alan adlarını (ör. login.silkroadonline.org) askıdaki süreç Winsock
		/// hatası vermeden önce C# tarafında otomatik IPv4'e çöz.
		/// </summary>
		public bool AutoResolveDomainToIPv4 { get; set; } = true;
		#endregion

		public override string ToString() => DisplayName;
	}
}
