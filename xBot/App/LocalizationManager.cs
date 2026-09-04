using System;
using System.Collections.Generic;

namespace xBot.App
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage { get; private set; } = "TR";

        public static event Action OnLanguageChanged;

        private static readonly Dictionary<string, string> StringsEN = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> StringsTR = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static LocalizationManager()
        {
            InitializeDictionaries();
        }

        private static void InitializeDictionaries()
        {
            // English Dictionary
            StringsEN["UI_Lang_EN"] = "EN";
            StringsEN["UI_Lang_TR"] = "TR";
            StringsEN["UI_WaitingForChar"] = "Waiting for Character...";
            StringsEN["UI_Connected"] = "Connected";
            StringsEN["UI_Disconnected"] = "Disconnected";
            StringsEN["UI_BotStarted"] = "Bot Started";
            StringsEN["UI_BotStopped"] = "Bot Stopped";
            StringsEN["UI_Save"] = "Save Settings";
            StringsEN["UI_HideClient"] = "Hide Client";
            StringsEN["UI_ShowClient"] = "Show Client";

            // General & Login
            StringsEN["UI_General"] = "General";
            StringsEN["UI_Training"] = "Training";
            StringsEN["UI_Skills"] = "Skills";
            StringsEN["UI_Protection"] = "Protection";
            StringsEN["UI_Party"] = "Party";
            StringsEN["UI_Inventory"] = "Inventory";
            StringsEN["UI_Items"] = "Town & Items";
            StringsEN["UI_Map"] = "Map";
            StringsEN["UI_Statistics"] = "Statistics";
            StringsEN["UI_Chat"] = "Chat";
            StringsEN["UI_Log"] = "Log";

            StringsEN["UI_ClientPath"] = "SRO Client Path";
            StringsEN["UI_AutomatedLogin"] = "Automated Login";
            StringsEN["UI_StaticCaptcha"] = "Static Captcha";
            StringsEN["UI_LoginDelay"] = "Login Delay (sec)";
            StringsEN["UI_WaitAfterDC"] = "Wait After DC (min)";
            StringsEN["UI_AutoStartBot"] = "Auto Start Bot";
            StringsEN["UI_UseReturnScroll"] = "Use Return Scroll";
            StringsEN["UI_AutoCharSelect"] = "Auto Char Select";
            StringsEN["UI_AutoHideClient"] = "Auto Hide Client";
            StringsEN["UI_StrategyFirst"] = "First Found";
            StringsEN["UI_StrategyHighest"] = "Highest Level";
            StringsEN["UI_StayConnected"] = "Stay Connected (Crash Failover)";
            StringsEN["UI_MoveToTray"] = "Move to Tray on Minimize";

            // Berserk & Combat
            StringsEN["UI_Berserk"] = "Berserk";
            StringsEN["UI_ZerkHPFull"] = "When HP is Full";
            StringsEN["UI_ZerkMobCount"] = "Monster Count >=";
            StringsEN["UI_ZerkAvoidance"] = "Avoidance Based";
            StringsEN["UI_ZerkRarity"] = "Monster Rarity Based";
            StringsEN["UI_IgnoreDimensionPillars"] = "Ignore Dimension Pillars";
            StringsEN["UI_AttackWeakerFirst"] = "Attack Weaker First";
            StringsEN["UI_DoNotFollowMobs"] = "Do Not Follow Mobs Beyond Radius";
            StringsEN["UI_Avoid"] = "Avoid";
            StringsEN["UI_Prefer"] = "Prefer";

            // Skills
            StringsEN["UI_PlayerSkills"] = "Player Skills";
            StringsEN["UI_ActiveBuffs"] = "Active Buffs";
            StringsEN["UI_SearchSkills"] = "Search skills...";
            StringsEN["UI_Attacks"] = "Attacks";
            StringsEN["UI_Buffs"] = "Buffs";
            StringsEN["UI_InOrder"] = "In Order";
            StringsEN["UI_NoAttack"] = "No Attack (Buff/Follow Only)";
            StringsEN["UI_Imbue"] = "Imbue";
            StringsEN["UI_DevilSkill"] = "Use Malicious Devil Skill";

            // Protection
            StringsEN["UI_Recovery"] = "Health / Mana Recovery";
            StringsEN["UI_SkillHP"] = "Use Skill if HP < %";
            StringsEN["UI_SkillMP"] = "Use Skill if MP < %";
            StringsEN["UI_SkillCure"] = "Use Skill for Bad Status";
            StringsEN["UI_PetRevive"] = "Revive Growth / Fellow Pet";
            StringsEN["UI_PetAutoSummon"] = "Auto Summon Growth & Fellow Pet";
            StringsEN["UI_BackToTown"] = "Back to Town";
            StringsEN["UI_DeadDelay"] = "Dead with Delay";
            StringsEN["UI_StopInTown"] = "Stop Bot when Back in Town";
            StringsEN["UI_NoArrows"] = "No Arrows / Bolts Left";
            StringsEN["UI_FullInventory"] = "Full Inventory";
            StringsEN["UI_FullPetInventory"] = "Full Pet Inventory";
            StringsEN["UI_HPLow"] = "HP Potions Low";
            StringsEN["UI_MPLow"] = "MP Potions Low";
            StringsEN["UI_DurabilityLow"] = "Equipment Durability Low";
            StringsEN["UI_LevelUp"] = "Level Up";
            StringsEN["UI_AutoStat"] = "Auto Distribute Stat Points";

            // Items & Filter
            StringsEN["UI_ItemFilter"] = "Item Filter";
            StringsEN["UI_AddPickup"] = "Add Pickup";
            StringsEN["UI_AddSell"] = "Add Sell";
            StringsEN["UI_AddStore"] = "Add Store";
            StringsEN["UI_RemoveAll"] = "Remove All";
            StringsEN["UI_DegreeRange"] = "Degree Range";
            StringsEN["UI_SoxPrint"] = "Print (SoX)";

            // Turkish Dictionary
            StringsTR["UI_Lang_EN"] = "EN";
            StringsTR["UI_Lang_TR"] = "TR";
            StringsTR["UI_WaitingForChar"] = "Karakter Bekleniyor...";
            StringsTR["UI_Connected"] = "Bağlandı";
            StringsTR["UI_Disconnected"] = "Bağlantı Kesildi";
            StringsTR["UI_BotStarted"] = "Bot Başlatıldı";
            StringsTR["UI_BotStopped"] = "Bot Durduruldu";
            StringsTR["UI_Save"] = "Ayarları Kaydet";
            StringsTR["UI_HideClient"] = "İstemciyi Gizle";
            StringsTR["UI_ShowClient"] = "İstemciyi Göster";

            // General & Login
            StringsTR["UI_General"] = "Genel";
            StringsTR["UI_Training"] = "Kasılma";
            StringsTR["UI_Skills"] = "Beceriler";
            StringsTR["UI_Protection"] = "Koruma";
            StringsTR["UI_Party"] = "Parti";
            StringsTR["UI_Inventory"] = "Envanter";
            StringsTR["UI_Items"] = "Şehir & Eşyalar";
            StringsTR["UI_Map"] = "Harita";
            StringsTR["UI_Statistics"] = "İstatistik";
            StringsTR["UI_Chat"] = "Sohbet";
            StringsTR["UI_Log"] = "Günlük";

            StringsTR["UI_ClientPath"] = "SRO İstemci Yolu";
            StringsTR["UI_AutomatedLogin"] = "Otomatik Giriş";
            StringsTR["UI_StaticCaptcha"] = "Sabit Captcha";
            StringsTR["UI_LoginDelay"] = "Giriş Gecikmesi (sn)";
            StringsTR["UI_WaitAfterDC"] = "DC Sonrası Bekleme (dk)";
            StringsTR["UI_AutoStartBot"] = "Otomatik Bot Başlat";
            StringsTR["UI_UseReturnScroll"] = "Dönüş Parşömeni Kullan";
            StringsTR["UI_AutoCharSelect"] = "Otomatik Karakter Seçimi";
            StringsTR["UI_AutoHideClient"] = "İstemciyi Otomatik Gizle";
            StringsTR["UI_StrategyFirst"] = "İlk Bulunan Karakter";
            StringsTR["UI_StrategyHighest"] = "En Yüksek Seviye Karakter";
            StringsTR["UI_StayConnected"] = "Bağlantıyı Koru (Çökme Koruması / Failover)";
            StringsTR["UI_MoveToTray"] = "Küçültünce Tepsiye Gizle";

            // Berserk & Combat
            StringsTR["UI_Berserk"] = "Berserk";
            StringsTR["UI_ZerkHPFull"] = "Can (HP) Tam Olduğunda";
            StringsTR["UI_ZerkMobCount"] = "Canavar Sayısı >=";
            StringsTR["UI_ZerkAvoidance"] = "Kaçınma/Tercihe Göre";
            StringsTR["UI_ZerkRarity"] = "Canavar Nadirliğine Göre";
            StringsTR["UI_IgnoreDimensionPillars"] = "Dimension Pillar Sütunlarını Yok Say";
            StringsTR["UI_AttackWeakerFirst"] = "Önce Zayıf Canavarlara Saldır";
            StringsTR["UI_DoNotFollowMobs"] = "Yarıçap Dışına Çıkan Mobları Takip Etme";
            StringsTR["UI_Avoid"] = "Kaçın / Görmezden Gel";
            StringsTR["UI_Prefer"] = "Öncelikli Hedef";

            // Skills
            StringsTR["UI_PlayerSkills"] = "Karakter Becerileri";
            StringsTR["UI_ActiveBuffs"] = "Aktif Buff'lar";
            StringsTR["UI_SearchSkills"] = "Beceri ara...";
            StringsTR["UI_Attacks"] = "Saldırılar";
            StringsTR["UI_Buffs"] = "Buff'lar";
            StringsTR["UI_InOrder"] = "Sırayla Kullan (Kombo)";
            StringsTR["UI_NoAttack"] = "Saldırı Yapma (Sadece Buff / Takip)";
            StringsTR["UI_Imbue"] = "El Yakma (Imbue)";
            StringsTR["UI_DevilSkill"] = "Malicious Devil Becerisi Kullan";

            // Protection
            StringsTR["UI_Recovery"] = "Can / Mana İyileşmesi";
            StringsTR["UI_SkillHP"] = "HP < % ise Beceriyle Doldur";
            StringsTR["UI_SkillMP"] = "MP < % ise Beceriyle Doldur";
            StringsTR["UI_SkillCure"] = "Zehir / Kötü Durumu Beceriyle Tedavi Et";
            StringsTR["UI_PetRevive"] = "Ölen Peti Otomatik Dirilt (Grass of Life)";
            StringsTR["UI_PetAutoSummon"] = "Peti Otomatik Çağır";
            StringsTR["UI_BackToTown"] = "Şehre Dönüş Tetikleyicileri";
            StringsTR["UI_DeadDelay"] = "Ölünce Gecikmeli Dön";
            StringsTR["UI_StopInTown"] = "Şehre Dönünce Botu Durdur";
            StringsTR["UI_NoArrows"] = "Ok / Bolt Kalmadığında";
            StringsTR["UI_FullInventory"] = "Çanta Dolduğunda";
            StringsTR["UI_FullPetInventory"] = "Pet Çantası Dolduğunda";
            StringsTR["UI_HPLow"] = "HP Pot Azaldığında";
            StringsTR["UI_MPLow"] = "MP Pot Azaldığında";
            StringsTR["UI_DurabilityLow"] = "Ekipman Dayanıklılığı Azaldığında";
            StringsTR["UI_LevelUp"] = "Level Atlandığında";
            StringsTR["UI_AutoStat"] = "Otomatik Stat Puanı Dağıt";

            // Items & Filter
            StringsTR["UI_ItemFilter"] = "Eşya Filtresi";
            StringsTR["UI_AddPickup"] = "Toplama Ekle";
            StringsTR["UI_AddSell"] = "Satışa Ekle";
            StringsTR["UI_AddStore"] = "Depoya Ekle";
            StringsTR["UI_RemoveAll"] = "Hepsini Temizle";
            StringsTR["UI_DegreeRange"] = "Derece (Degree) Aralığı";
            StringsTR["UI_SoxPrint"] = "SoX (SOS/SOM/SUN/Nova)";
        }

        public static string Get(string key, string defaultText = "")
        {
            var dict = CurrentLanguage == "TR" ? StringsTR : StringsEN;
            if (dict.TryGetValue(key, out string val))
                return val;
            return string.IsNullOrEmpty(defaultText) ? key : defaultText;
        }

        public static void SetLanguage(string lang)
        {
            if (string.Equals(lang, "EN", StringComparison.OrdinalIgnoreCase))
                CurrentLanguage = "EN";
            else
                CurrentLanguage = "TR";

            OnLanguageChanged?.Invoke();
        }
    }
}
