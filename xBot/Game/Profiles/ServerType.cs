namespace xBot.Game.Profiles
{
	public enum ServerType
	{
		/// <summary>
		/// Standart Private / PvP sunucuları (v1.188 / 80-140 Cap).
		/// Klasik 0x3013 paket yapısı, klasik soket ve elixir alanları.
		/// </summary>
		vSRO = 0,

		/// <summary>
		/// Silkroad-R tabanlı sunucular (Elysian Silkroad-R, Joymax SRO-R).
		/// 21 baytlık meslek/onur başlığı, 8 baytlık sabit soket bloğu, meslek kesesi, Silkroad-R görev yapısı.
		/// </summary>
		SilkroadR = 1,

		/// <summary>
		/// Silkroad Online Türkiye (GameGami).
		/// Modern motor, Xigncode3, 0x7625 ikinci şifre (PIN) zorunluluğu, modern sunucu durum yapısı.
		/// </summary>
		TRSRO = 2,

		/// <summary>
		/// International Silkroad (Joymax / Webzen).
		/// Resmi global sunucu, modern trafik durumu, ikincil şifre, VIP desteği.
		/// </summary>
		iSRO = 3,

		/// <summary>
		/// Kore Silkroad (kSRO - Joymax).
		/// Orijinal Kore paket yapısı, KSSN/OTP kimlik doğrulama.
		/// </summary>
		kSRO = 4,

		/// <summary>
		/// Çin Silkroad (cSRO / cSRO-R).
		/// Çin sunucularına özel anti-addiction ve paket yapıları.
		/// </summary>
		cSRO = 5,

		/// <summary>
		/// Japon Silkroad (jSRO).
		/// </summary>
		jSRO = 6
	}
}
