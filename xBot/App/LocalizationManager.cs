using System;
using System.Collections.Generic;

namespace xBot.App
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage { get; private set; } = "EN";

        public static event Action OnLanguageChanged;

        private static readonly Dictionary<string, string> StringsEN = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> StringsTR = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static LocalizationManager()
        {
            InitializeDictionaries();
        }

        private static void InitializeDictionaries()
        {
            #region (English Dictionary)
            // Header & System
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
            StringsEN["UI_CommandCenter"] = "Command Center";

            // Modern Sidebar Categories
            StringsEN["UI_Cat_BotSettings"] = "BOT SETTINGS";
            StringsEN["UI_Cat_Community"] = "COMMUNITY";
            StringsEN["UI_Cat_System"] = "SYSTEM";

            // Modern Sidebar Navigation Items
            StringsEN["UI_Tab_Login"] = "General / Login";
            StringsEN["UI_Tab_Training"] = "Training";
            StringsEN["UI_Tab_Skills"] = "Skills";
            StringsEN["UI_Tab_Character"] = "Protection";
            StringsEN["UI_Tab_Town"] = "Town & Items";
            StringsEN["UI_Tab_Alchemy"] = "Alchemy (+ Fuse)";
            StringsEN["UI_Tab_TargetAssist"] = "Target Assist";
            StringsEN["UI_Tab_Inventory"] = "Inventory";
            StringsEN["UI_Tab_Party"] = "Party";
            StringsEN["UI_Tab_Guild"] = "Guild";
            StringsEN["UI_Tab_Academy"] = "Academy";
            StringsEN["UI_Tab_Players"] = "Players";
            StringsEN["UI_Tab_Chat"] = "Chat";
            StringsEN["UI_Tab_Stall"] = "Stall";
            StringsEN["UI_Tab_Minimap"] = "Minimap";
            StringsEN["UI_Tab_GameInfo"] = "Game Info";
            StringsEN["UI_Tab_Settings"] = "Settings";

            // General & Login Strategy
            StringsEN["UI_General"] = "General";
            StringsEN["UI_StrategyCard_Title"] = "LOGIN STRATEGY & AUTOMATION";
            StringsEN["UI_SavedAccounts"] = "Saved Accounts:";
            StringsEN["UI_SaveAccount"] = "Save Account";
            StringsEN["UI_DeleteAccount"] = "Delete";
            StringsEN["UI_AccountSetupTip"] = "Advanced Account & PIN Setup";
            StringsEN["UI_AutomatedLogin"] = "Enable Automated Login";
            StringsEN["UI_StaticCaptcha"] = "Use Static Captcha Code";
            StringsEN["UI_LoginDelay"] = "Login Delay (sec):";
            StringsEN["UI_WaitAfterDC"] = "Wait After DC (min):";
            StringsEN["UI_AutoStartBot"] = "Auto Start Bot on In-Game";
            StringsEN["UI_AutoHideClient"] = "Auto Hide Silkroad Client";
            StringsEN["UI_StayConnected"] = "Stay Connected (Crash/DC Failover)";
            StringsEN["UI_CharSelectStrategy"] = "Character Selection Strategy:";
            StringsEN["UI_StrategyFirst"] = "First Found Character";
            StringsEN["UI_StrategyHighest"] = "Highest Level Character";
            StringsEN["UI_ClientPath"] = "SRO Client Path";

            // Combat AI & Training
            StringsEN["UI_Training"] = "Training";
            StringsEN["UI_CombatAI_Title"] = "COMBAT AI";
            StringsEN["UI_CombatAI_Tab"] = "Combat";
            StringsEN["UI_Berserk"] = "Berserk";
            StringsEN["UI_BerserkDesc"] = "Control when berserk should be used during combat.";
            StringsEN["UI_ZerkHPFull"] = "When HP is full";
            StringsEN["UI_ZerkMobCount"] = "Monster count";
            StringsEN["UI_ZerkUnit"] = "mobs";
            StringsEN["UI_ZerkAvoidance"] = "Avoidance based";
            StringsEN["UI_ZerkRarity"] = "Monster rarity based";
            StringsEN["UI_IgnoreDimensionPillars"] = "Ignore dimension pillars";
            StringsEN["UI_AttackWeakerFirst"] = "Attack weaker first";
            StringsEN["UI_DoNotFollowMobs"] = "Do not follow mobs";
            StringsEN["UI_AdvTitle"] = "Advanced";
            StringsEN["UI_AdvDesc"] = "Fine tune target selection and movement behavior.";
            StringsEN["UI_AvoidTitle"] = "Avoidance";
            StringsEN["UI_AvoidDesc"] = "Set target preferences by monster rarity.";
            StringsEN["UI_AvoidH_Rarity"] = "Rarity";
            StringsEN["UI_AvoidH_Avoid"] = "Avoid";
            StringsEN["UI_AvoidH_Prefer"] = "Priority";
            StringsEN["UI_AvoidH_Berserk"] = "Berserk";
            StringsEN["UI_R_General"] = "General";
            StringsEN["UI_R_Champion"] = "Champion";
            StringsEN["UI_R_Giant"] = "Giant";
            StringsEN["UI_R_PGeneral"] = "GeneralParty";
            StringsEN["UI_R_PChampion"] = "ChampionParty";
            StringsEN["UI_R_PGiant"] = "GiantParty";
            StringsEN["UI_R_Unique"] = "Unique";
            StringsEN["UI_R_Elite"] = "Elite";
            StringsEN["UI_R_Event"] = "Event";
            StringsEN["UI_ReturnTitle"] = "Return to Area";
            StringsEN["UI_ReturnDesc"] = "Route back to the training area and startup behavior.";
            StringsEN["UI_ReturnScript"] = "Walk Script Path";
            StringsEN["UI_ReturnMount"] = "Mount";
            StringsEN["UI_ReturnBuffs"] = "Buffs";
            StringsEN["UI_ReturnSpeed"] = "Speed drug";
            StringsEN["UI_ReturnReverse"] = "Reverse route";
            StringsEN["UI_ReturnTown"] = "Town cycling";
            StringsEN["UI_AreaDetails"] = "Training Area Coordinates & Script";
            StringsEN["UI_AreaGuide"] = "To define a training area, navigate your character to the desired spot in-game and click 'Get coordinates'. Radius defines the movement perimeter. You can optionally select a script (.txt) path for walk routes.";
            StringsEN["UI_Tr_Area"] = "Area";
            StringsEN["UI_Tr_Script"] = "Script";
            StringsEN["UI_Tr_Trace"] = "Trace";
            StringsEN["UI_Tr_IdentName"] = "Identification name";
            StringsEN["UI_Tr_GetCoords"] = "Get coordinates";
            StringsEN["UI_Tr_Region"] = "Region";
            StringsEN["UI_Tr_Radius"] = "Radius";
            StringsEN["UI_Tr_ScriptPath"] = "Script Path";
            StringsEN["UI_Tr_Record"] = "Record";
            StringsEN["UI_Tr_Output"] = "Output";
            StringsEN["UI_Tr_Start"] = "START";
            StringsEN["UI_Tr_Stop"] = "STOP";
            StringsEN["UI_Tr_Pause"] = "PAUSE";
            StringsEN["UI_Tr_Resume"] = "RESUME";
            StringsEN["UI_Tr_TraceTitle"] = "Trace";
            StringsEN["UI_Tr_PlayerName"] = "Player name";
            StringsEN["UI_Tr_KeepDist"] = "Keep player distance";
            StringsEN["UI_Tr_FollowMaster"] = "Follow party master if the player name is not near";
            StringsEN["UI_Tr_AreaAdd"] = "Add";
            StringsEN["UI_Tr_AreaRemove"] = "Remove";
            StringsEN["UI_Tr_AreaActivate"] = "Activate";
            StringsEN["UI_Tr_WalkCenter"] = "Walk to center";
            StringsEN["UI_Tr_ScriptGuide"] = "Script Recording Guide:\n\n1. Stand in town and click 'START'.\n2. Walk your character toward the designated training spot.\n3. NPC interactions, potion purchases, and walk coordinates are recorded.\n4. Upon arriving at the training spot, click 'PAUSE' or 'STOP'.\n5. Select the saved script file in Training -> Area -> 'Script Path'.\n\nThe bot will automatically re-walk this route upon returning to town.";
            StringsEN["UI_Avoid"] = "Avoid";
            StringsEN["UI_Prefer"] = "Prefer";
            StringsEN["UI_RecordScript"] = "Record Script";
            StringsEN["UI_SetCurrentArea"] = "Set Current Pos as Area";

            // Skills
            StringsEN["UI_Skills"] = "Skills";
            StringsEN["UI_Skills_CardTitle"] = "SKILLS & ATTACK MANAGEMENT";
            StringsEN["UI_PlayerSkills"] = "Player Skills";
            StringsEN["UI_ActiveBuffs"] = "Active Buffs";
            StringsEN["UI_SearchSkills"] = "Search skills...";
            StringsEN["UI_Attacks"] = "Attacks";
            StringsEN["UI_Buffs"] = "Buffs";
            StringsEN["UI_InOrder"] = "Cast Skills in Order (Combo)";
            StringsEN["UI_NoAttack"] = "No Attack (Buff/Follow Only)";
            StringsEN["UI_Imbue"] = "Use Imbue Skill";
            StringsEN["UI_DevilSkill"] = "Use Malicious Devil Skill";

            // Protection
            StringsEN["UI_Protection"] = "Protection";
            StringsEN["UI_Prot_Recovery"] = "HEALTH / MANA / RECOVERY";
            StringsEN["UI_Prot_Town"] = "RETURN TO TOWN TRIGGERS";
            StringsEN["UI_Prot_AutoStat"] = "AUTO STAT DISTRIBUTION";
            StringsEN["UI_SkillHP"] = "Heal Skill if HP < %";
            StringsEN["UI_SkillMP"] = "Restore Skill if MP < %";
            StringsEN["UI_SkillCure"] = "Cure Bad Status with Skill";
            StringsEN["UI_PetRevive"] = "Auto Revive Pet (Grass of Life)";
            StringsEN["UI_PetAutoSummon"] = "Auto Summon Pet";
            StringsEN["UI_BackToTown"] = "Back to Town";
            StringsEN["UI_DeadDelay"] = "Death delay (sec):";
            StringsEN["UI_StopInTown"] = "Stop Bot when in Town";
            StringsEN["UI_NoArrows"] = "No Arrows / Bolts Left";
            StringsEN["UI_FullInventory"] = "Inventory Full";
            StringsEN["UI_FullPetInventory"] = "Pet Inventory Full";
            StringsEN["UI_HPLow"] = "HP Potions Low (<=)";
            StringsEN["UI_MPLow"] = "MP Potions Low (<=)";
            StringsEN["UI_DurabilityLow"] = "Equipment Durability Low (<=)";
            StringsEN["UI_LevelUp"] = "Return to Town on Level Up";
            StringsEN["UI_AutoStat"] = "Auto Distribute Stat Points";
            StringsEN["UI_StatRemain"] = "Remaining Stat Points:";

            // Town & Item Filter
            StringsEN["UI_Items"] = "Town & Items";
            StringsEN["UI_ItemFilter"] = "Item Filter & Rules";
            StringsEN["UI_DegreeRange"] = "Degree Range:";
            StringsEN["UI_SoxPrint"] = "Only SoX (SOS / SOM / SUN / Nova)";
            StringsEN["UI_FilterChina"] = "Chinese Equipment";
            StringsEN["UI_FilterEurope"] = "European Equipment";
            StringsEN["UI_FilterMale"] = "Male Equipment";
            StringsEN["UI_FilterFemale"] = "Female Equipment";
            StringsEN["UI_RuleName"] = "Item / Rule Name:";
            StringsEN["UI_AddPickup"] = "Pick";
            StringsEN["UI_AddSell"] = "Sell";
            StringsEN["UI_AddStore"] = "Store";
            StringsEN["UI_AddRule"] = "Add Rule";
            StringsEN["UI_RemoveAll"] = "Clear All";

            // Party
            StringsEN["UI_Party"] = "Party";
            StringsEN["UI_Party_Setup"] = "Auto Party Setup / Match";
            StringsEN["UI_Party_Heal"] = "Auto Heal Party Members";
            StringsEN["UI_Party_Ress"] = "Auto Resurrect Party Members";
            StringsEN["UI_Party_AcceptRess"] = "Accept Resurrection Requests";
            StringsEN["UI_Party_OnlyRess"] = "Party Only Resurrection";

            // Alchemy (+ Fuse)
            StringsEN["UI_Alchemy_CardSettings"] = "ALCHEMY (+ FUSE) SETTINGS";
            StringsEN["UI_Alchemy_CardActions"] = "ALCHEMY ACTIONS & LIVE STATUS";
            StringsEN["UI_Alchemy_TargetItem"] = "Target Equipment (Inventory):";
            StringsEN["UI_Alchemy_TargetSlot"] = "Inventory Slot (13-76):";
            StringsEN["UI_Alchemy_TargetPlus"] = "Target Plus (+1 to +15):";
            StringsEN["UI_Alchemy_UsePowder"] = "Use Lucky Powder (Auto-Match)";
            StringsEN["UI_Alchemy_MaxAttempts"] = "Maximum Attempts Limit:";
            StringsEN["UI_Alchemy_Delay"] = "Action Delay (ms):";
            StringsEN["UI_Alchemy_Start"] = "▶ Start Alchemy";
            StringsEN["UI_Alchemy_Stop"] = "⏹ Stop";
            StringsEN["UI_Alchemy_Reset"] = "↺ Reset Counters";
            StringsEN["UI_Alchemy_StatusPrefix"] = "Status: ";
            StringsEN["UI_Alchemy_AttemptsPrefix"] = "Attempts: ";
            StringsEN["UI_Alchemy_SuccessPrefix"] = "Success: ";
            StringsEN["UI_Alchemy_FailedPrefix"] = "Failed: ";
            StringsEN["UI_Alchemy_LogHead"] = "Live Alchemy Log:";

            // Target Assist
            StringsEN["UI_TA_Pill"] = "Target Assist";
            StringsEN["UI_TA_CardSettings"] = "CORE SETTINGS";
            StringsEN["UI_TA_CardFilters"] = "TARGET FILTERS";
            StringsEN["UI_TA_CardLists"] = "CUSTOM TARGET LISTS";
            StringsEN["UI_TA_NoCandidates"] = "No target candidates in range.";
            StringsEN["UI_TA_Enabled"] = "Enabled";
            StringsEN["UI_TA_MaxRange"] = "Max range";
            StringsEN["UI_TA_RoleMode"] = "Role mode";
            StringsEN["UI_TA_CycleKey"] = "Target cycle key";
            StringsEN["UI_TA_Capture"] = "[ Capture ]";
            StringsEN["UI_TA_PressKey"] = "[ Press key ]";
            StringsEN["UI_TA_IncludeDead"] = "Include dead targets";
            StringsEN["UI_TA_IgnoreBloody"] = "Ignore bloody storm targets";
            StringsEN["UI_TA_IgnoreSnow"] = "Ignore snow shield targets";
            StringsEN["UI_TA_OnlyCustom"] = "Only custom players";
            StringsEN["UI_TA_IgnoredGuilds"] = "Ignored Guilds";
            StringsEN["UI_TA_CustomPlayers"] = "Custom Players";
            StringsEN["UI_TA_Add"] = "+ Add";
            StringsEN["UI_TA_Remove"] = "- Remove";
            StringsEN["UI_TA_Footer"] = "Changes apply instantly.";
            StringsEN["UI_TA_Save"] = "Save";

            // Account Setup Modal
            StringsEN["UI_Acc_Title"] = "Account Setup";
            StringsEN["UI_Acc_Subtitle"] = "Manage saved accounts and character login credentials.";
            StringsEN["UI_Acc_Username"] = "Username";
            StringsEN["UI_Acc_Password"] = "Password";
            StringsEN["UI_Acc_Secondary"] = "Secondary (PIN)";
            StringsEN["UI_Acc_Server"] = "Server Name";
            StringsEN["UI_Acc_AddUpdate"] = "Add / Update";
            StringsEN["UI_Acc_Remove"] = "Remove Selected";
            StringsEN["UI_Acc_ColUser"] = "USERNAME:";
            StringsEN["UI_Acc_ColServer"] = "SERVER NAME:";
            StringsEN["UI_Acc_OK"] = "OK";
            #endregion

            #region (Turkish Dictionary)
            // Header & System
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
            StringsTR["UI_CommandCenter"] = "Komut Merkezi";

            // Modern Sidebar Categories
            StringsTR["UI_Cat_BotSettings"] = "BOT AYARLARI";
            StringsTR["UI_Cat_Community"] = "TOPLULUK";
            StringsTR["UI_Cat_System"] = "SİSTEM";

            // Modern Sidebar Navigation Items
            StringsTR["UI_Tab_Login"] = "Genel / Giriş";
            StringsTR["UI_Tab_Training"] = "Kasılma";
            StringsTR["UI_Tab_Skills"] = "Beceriler";
            StringsTR["UI_Tab_Character"] = "Koruma";
            StringsTR["UI_Tab_Town"] = "Şehir & İtem";
            StringsTR["UI_Tab_Alchemy"] = "Simya (+ Basma)";
            StringsTR["UI_Tab_TargetAssist"] = "Target Assist";
            StringsTR["UI_Tab_Inventory"] = "Envanter";
            StringsTR["UI_Tab_Party"] = "Parti";
            StringsTR["UI_Tab_Guild"] = "Guild";
            StringsTR["UI_Tab_Academy"] = "Akademi";
            StringsTR["UI_Tab_Players"] = "Oyuncular";
            StringsTR["UI_Tab_Chat"] = "Sohbet";
            StringsTR["UI_Tab_Stall"] = "Stall";
            StringsTR["UI_Tab_Minimap"] = "Harita";
            StringsTR["UI_Tab_GameInfo"] = "Oyun Bilgisi";
            StringsTR["UI_Tab_Settings"] = "Ayarlar";

            // General & Login Strategy
            StringsTR["UI_General"] = "Genel";
            StringsTR["UI_StrategyCard_Title"] = "GİRİŞ STRATEJİSİ VE OTOMASYON";
            StringsTR["UI_SavedAccounts"] = "Kayıtlı Hesaplar:";
            StringsTR["UI_SaveAccount"] = "Hesap Kaydet";
            StringsTR["UI_DeleteAccount"] = "Sil";
            StringsTR["UI_AccountSetupTip"] = "Gelişmiş Hesap ve PIN Yönetimi";
            StringsTR["UI_AutomatedLogin"] = "Otomatik Giriş Yap";
            StringsTR["UI_StaticCaptcha"] = "Sabit Captcha Kodu Kullan";
            StringsTR["UI_LoginDelay"] = "Giriş Gecikmesi (sn):";
            StringsTR["UI_WaitAfterDC"] = "DC Sonrası Bekleme (dk):";
            StringsTR["UI_AutoStartBot"] = "Oyuna Girince Botu Otomatik Başlat";
            StringsTR["UI_AutoHideClient"] = "Girişte Silkroad İstemcisini Gizle";
            StringsTR["UI_StayConnected"] = "Bağlantıyı Koru (Çökme / DC Koruması)";
            StringsTR["UI_CharSelectStrategy"] = "Karakter Seçim Stratejisi:";
            StringsTR["UI_StrategyFirst"] = "İlk Bulunan Karakter";
            StringsTR["UI_StrategyHighest"] = "En Yüksek Seviyeli Karakter";
            StringsTR["UI_ClientPath"] = "SRO İstemci Yolu";

            // Combat AI & Training
            StringsTR["UI_Training"] = "Kasılma";
            StringsTR["UI_CombatAI_Title"] = "Savaş Zekası (Combat AI)";
            StringsTR["UI_CombatAI_Tab"] = "Savaş";
            StringsTR["UI_Berserk"] = "Berserk";
            StringsTR["UI_BerserkDesc"] = "Berserk'in savaş sırasında ne zaman kullanılacağını kontrol eder.";
            StringsTR["UI_ZerkHPFull"] = "HP tam doluyken";
            StringsTR["UI_ZerkMobCount"] = "Canavar sayısı";
            StringsTR["UI_ZerkUnit"] = "canavar";
            StringsTR["UI_ZerkAvoidance"] = "Kaçınmaya göre";
            StringsTR["UI_ZerkRarity"] = "Nadirliğe göre";
            StringsTR["UI_IgnoreDimensionPillars"] = "Dimension sütunlarını yok say";
            StringsTR["UI_AttackWeakerFirst"] = "Önce zayıf mob";
            StringsTR["UI_DoNotFollowMobs"] = "Mobları takip etme";
            StringsTR["UI_AdvTitle"] = "Gelişmiş";
            StringsTR["UI_AdvDesc"] = "Hedef seçim ve hareket davranışını ince ayarlar.";
            StringsTR["UI_AvoidTitle"] = "Kaçınma";
            StringsTR["UI_AvoidDesc"] = "Canavar nadirliğine göre hedef tercihleri.";
            StringsTR["UI_AvoidH_Rarity"] = "Rarity";
            StringsTR["UI_AvoidH_Avoid"] = "Kaçın";
            StringsTR["UI_AvoidH_Prefer"] = "Öncelik";
            StringsTR["UI_AvoidH_Berserk"] = "Berserke";
            StringsTR["UI_R_General"] = "Genel";
            StringsTR["UI_R_Champion"] = "Champion";
            StringsTR["UI_R_Giant"] = "Dev (Giant)";
            StringsTR["UI_R_PGeneral"] = "GeneralParty";
            StringsTR["UI_R_PChampion"] = "ChampionParty";
            StringsTR["UI_R_PGiant"] = "GiantParty";
            StringsTR["UI_R_Unique"] = "Unique";
            StringsTR["UI_R_Elite"] = "Elite";
            StringsTR["UI_R_Event"] = "Event";
            StringsTR["UI_ReturnTitle"] = "Alana Dönüş";
            StringsTR["UI_ReturnDesc"] = "Eğitim alanına dönüş rotası ve başlangıç davranışı.";
            StringsTR["UI_ReturnScript"] = "Yürüyüş Scripti Yolu";
            StringsTR["UI_ReturnMount"] = "Mount";
            StringsTR["UI_ReturnBuffs"] = "Bufflar";
            StringsTR["UI_ReturnSpeed"] = "Speed drug";
            StringsTR["UI_ReturnReverse"] = "Reverse route";
            StringsTR["UI_ReturnTown"] = "Town cycling";
            StringsTR["UI_AreaDetails"] = "Kasılma Alanı Koordinatları & Script";
            StringsTR["UI_AreaGuide"] = "Kasılma alanı tanımlamak için karakterinizi oyunda istediğiniz noktaya götürüp 'Get coordinates' butonuna basın. Yarıçap (Radius) karakterin alandan ne kadar uzaklaşacağını belirler. Yürüme rotası için script (.txt) dosyası seçebilirsiniz.";
            StringsTR["UI_Tr_Area"] = "Alan";
            StringsTR["UI_Tr_Script"] = "Script";
            StringsTR["UI_Tr_Trace"] = "Takip";
            StringsTR["UI_Tr_IdentName"] = "Tanımlama adı";
            StringsTR["UI_Tr_GetCoords"] = "Koordinatları al";
            StringsTR["UI_Tr_Region"] = "Bölge";
            StringsTR["UI_Tr_Radius"] = "Yarıçap";
            StringsTR["UI_Tr_ScriptPath"] = "Script Yolu";
            StringsTR["UI_Tr_Record"] = "Kayıt";
            StringsTR["UI_Tr_Output"] = "Çıktı";
            StringsTR["UI_Tr_Start"] = "BAŞLAT";
            StringsTR["UI_Tr_Stop"] = "DURDUR";
            StringsTR["UI_Tr_Pause"] = "DURAKLAT";
            StringsTR["UI_Tr_Resume"] = "DEVAM";
            StringsTR["UI_Tr_TraceTitle"] = "Takip";
            StringsTR["UI_Tr_PlayerName"] = "Oyuncu adı";
            StringsTR["UI_Tr_KeepDist"] = "Oyuncu mesafesini koru";
            StringsTR["UI_Tr_FollowMaster"] = "Oyuncu yakında değilse parti liderini takip et";
            StringsTR["UI_Tr_AreaAdd"] = "Ekle";
            StringsTR["UI_Tr_AreaRemove"] = "Sil";
            StringsTR["UI_Tr_AreaActivate"] = "Aktifleştir";
            StringsTR["UI_Tr_WalkCenter"] = "Merkeze yürü";
            StringsTR["UI_Tr_ScriptGuide"] = "Script Kayıt ve Kullanım Kılavuzu:\n\n1. Karakteriniz şehirdeyken 'BAŞLAT' butonuna basın.\n2. Kasılma alanınıza doğru karakterinizi yürütün.\n3. Yolda yapılan NPC etkileşimleri, pot alımları ve koordinatlar otomatik kaydedilir.\n4. Kasılma alanına ulaştığınızda 'DURAKLAT' veya 'DURDUR' ile kaydı sonlandırın.\n5. Kaydedilen dosyayı Kasılma -> Alan sekmesindeki 'Script Yolu' kısmından seçin.\n\nBot kasabaya döndüğünde bu rota üzerinden geri yürüyecektir.";
            StringsTR["UI_Avoid"] = "Kaçın / Görmezden Gel";
            StringsTR["UI_Prefer"] = "Öncelikli Hedef";
            StringsTR["UI_RecordScript"] = "Script Kaydet";
            StringsTR["UI_SetCurrentArea"] = "Mevcut Konumu Alan Yap";

            // Skills
            StringsTR["UI_Skills"] = "Beceriler";
            StringsTR["UI_Skills_CardTitle"] = "BECERİ VE SALDIRI YÖNETİMİ";
            StringsTR["UI_PlayerSkills"] = "Karakter Becerileri";
            StringsTR["UI_ActiveBuffs"] = "Aktif Buff'lar";
            StringsTR["UI_SearchSkills"] = "Beceri ara...";
            StringsTR["UI_Attacks"] = "Saldırılar";
            StringsTR["UI_Buffs"] = "Buff'lar";
            StringsTR["UI_InOrder"] = "Becerileri Sırayla Kullan (Kombo)";
            StringsTR["UI_NoAttack"] = "Saldırı Yapma (Sadece Buff / Destek)";
            StringsTR["UI_Imbue"] = "El Yakma (Imbue) Kullan";
            StringsTR["UI_DevilSkill"] = "Malicious Devil Becerisi Kullan";

            // Protection
            StringsTR["UI_Protection"] = "Koruma";
            StringsTR["UI_Prot_Recovery"] = "CAN / MANA / İYİLEŞTİRME KORUMASI";
            StringsTR["UI_Prot_Town"] = "ŞEHRE DÖNÜŞ TETİKLEYİCİLERİ";
            StringsTR["UI_Prot_AutoStat"] = "OTOMATİK STAT DAĞITIMI";
            StringsTR["UI_SkillHP"] = "HP < % ise Beceriyle İyileş";
            StringsTR["UI_SkillMP"] = "MP < % ise Beceriyle Doldur";
            StringsTR["UI_SkillCure"] = "Zehir / Kötü Durumu Beceriyle Sil (Cure)";
            StringsTR["UI_PetRevive"] = "Ölen Peti Dirilt (Grass of Life)";
            StringsTR["UI_PetAutoSummon"] = "Peti Otomatik Çağır";
            StringsTR["UI_BackToTown"] = "Şehre Dönüş";
            StringsTR["UI_DeadDelay"] = "Ölüm gecikmesi (sn):";
            StringsTR["UI_StopInTown"] = "Kasabaya Dönünce Botu Durdur";
            StringsTR["UI_NoArrows"] = "Ok / Bolt Bittiğinde";
            StringsTR["UI_FullInventory"] = "Çanta Dolduğunda";
            StringsTR["UI_FullPetInventory"] = "Pet Çantası Dolduğunda";
            StringsTR["UI_HPLow"] = "HP Pot Sayısı Azalınca (<=)";
            StringsTR["UI_MPLow"] = "MP Pot Sayısı Azalınca (<=)";
            StringsTR["UI_DurabilityLow"] = "Ekipman Dayanıklılığı Düştüğünde (<=)";
            StringsTR["UI_LevelUp"] = "Level Atlayınca Kasabaya Dön";
            StringsTR["UI_AutoStat"] = "Stat Puanlarını Otomatik Dağıt";
            StringsTR["UI_StatRemain"] = "Kalan Stat Puanı:";

            // Town & Item Filter
            StringsTR["UI_Items"] = "Şehir & İtem";
            StringsTR["UI_ItemFilter"] = "Eşya Filtresi ve Kurallar";
            StringsTR["UI_DegreeRange"] = "Derece (Degree) Aralığı:";
            StringsTR["UI_SoxPrint"] = "Sadece SoX (SOS / SOM / SUN / Nova)";
            StringsTR["UI_FilterChina"] = "Çin Ekipmanları";
            StringsTR["UI_FilterEurope"] = "Avrupa Ekipmanları";
            StringsTR["UI_FilterMale"] = "Erkek Ekipmanları";
            StringsTR["UI_FilterFemale"] = "Kadın Ekipmanları";
            StringsTR["UI_RuleName"] = "Eşya / Kural Adı:";
            StringsTR["UI_AddPickup"] = "Topla";
            StringsTR["UI_AddSell"] = "Sat";
            StringsTR["UI_AddStore"] = "Depola";
            StringsTR["UI_AddRule"] = "Kural Ekle";
            StringsTR["UI_RemoveAll"] = "Tümünü Temizle";

            // Party
            StringsTR["UI_Party"] = "Parti";
            StringsTR["UI_Party_Setup"] = "Otomatik Parti Kur / Katıl";
            StringsTR["UI_Party_Heal"] = "Parti Üyelerini İyileştir (Heal)";
            StringsTR["UI_Party_Ress"] = "Ölen Üyeleri Dirilt (Resurrection)";
            StringsTR["UI_Party_AcceptRess"] = "Gelen Diriltme İsteklerini Kabul Et";
            StringsTR["UI_Party_OnlyRess"] = "Sadece Parti Üyesinden Gelen Diriltmeyi Kabul Et";

            // Alchemy (+ Fuse)
            StringsTR["UI_Alchemy_CardSettings"] = "SİMYA (+ BASMA) AYARLARI";
            StringsTR["UI_Alchemy_CardActions"] = "SİMYA İŞLEMLERİ VE CANLI DURUM";
            StringsTR["UI_Alchemy_TargetItem"] = "Hedef Ekipman (Envanter):";
            StringsTR["UI_Alchemy_TargetSlot"] = "Envanter Slotu (13-76):";
            StringsTR["UI_Alchemy_TargetPlus"] = "Hedef Artı Seviyesi (+1 ile +15):";
            StringsTR["UI_Alchemy_UsePowder"] = "Lucky Powder Kullan (Otomatik Eşle)";
            StringsTR["UI_Alchemy_MaxAttempts"] = "Maksimum Deneme Sınırı:";
            StringsTR["UI_Alchemy_Delay"] = "İşlem Gecikmesi (ms):";
            StringsTR["UI_Alchemy_Start"] = "▶ Simyayı Başlat";
            StringsTR["UI_Alchemy_Stop"] = "⏹ Durdur";
            StringsTR["UI_Alchemy_Reset"] = "↺ Sayaçları Sıfırla";
            StringsTR["UI_Alchemy_StatusPrefix"] = "Durum: ";
            StringsTR["UI_Alchemy_AttemptsPrefix"] = "Deneme: ";
            StringsTR["UI_Alchemy_SuccessPrefix"] = "Başarılı: ";
            StringsTR["UI_Alchemy_FailedPrefix"] = "Başarısız: ";
            StringsTR["UI_Alchemy_LogHead"] = "Canlı Simya Günlüğü:";

            // Target Assist
            StringsTR["UI_TA_Pill"] = "Target Assist";
            StringsTR["UI_TA_CardSettings"] = "TEMEL AYARLAR";
            StringsTR["UI_TA_CardFilters"] = "HEDEF FİLTRELERİ";
            StringsTR["UI_TA_CardLists"] = "ÖZEL HEDEF LİSTELERİ";
            StringsTR["UI_TA_NoCandidates"] = "Menzil içinde hedef aday yok.";
            StringsTR["UI_TA_Enabled"] = "Aktif";
            StringsTR["UI_TA_MaxRange"] = "Maksimum menzil";
            StringsTR["UI_TA_RoleMode"] = "Rol modu";
            StringsTR["UI_TA_CycleKey"] = "Hedef döngü tuşu";
            StringsTR["UI_TA_Capture"] = "[ Tuş Yakala ]";
            StringsTR["UI_TA_PressKey"] = "[ Tuşa Basın ]";
            StringsTR["UI_TA_IncludeDead"] = "Ölü hedefleri dahil et";
            StringsTR["UI_TA_IgnoreBloody"] = "Bloody Storm hedeflerini yok say";
            StringsTR["UI_TA_IgnoreSnow"] = "Snow Shield hedeflerini yok say";
            StringsTR["UI_TA_OnlyCustom"] = "Sadece listedeki oyuncular";
            StringsTR["UI_TA_IgnoredGuilds"] = "Yok Sayılan Guild'ler";
            StringsTR["UI_TA_CustomPlayers"] = "Özel Oyuncular";
            StringsTR["UI_TA_Add"] = "+ Ekle";
            StringsTR["UI_TA_Remove"] = "- Kaldır";
            StringsTR["UI_TA_Footer"] = "Değişiklikler anında uygulanır.";
            StringsTR["UI_TA_Save"] = "Kaydet";

            // Account Setup Modal
            StringsTR["UI_Acc_Title"] = "Hesap Yönetimi";
            StringsTR["UI_Acc_Subtitle"] = "Kayıtlı hesapları ve karakter giriş bilgilerini yönetin.";
            StringsTR["UI_Acc_Username"] = "Kullanıcı Adı";
            StringsTR["UI_Acc_Password"] = "Şifre";
            StringsTR["UI_Acc_Secondary"] = "İkinci Şifre (PIN)";
            StringsTR["UI_Acc_Server"] = "Sunucu Adı";
            StringsTR["UI_Acc_AddUpdate"] = "Hesap Ekle / Güncelle";
            StringsTR["UI_Acc_Remove"] = "Seçiliyi Sil";
            StringsTR["UI_Acc_ColUser"] = "KULLANICI ADI:";
            StringsTR["UI_Acc_ColServer"] = "SUNUCU ADI:";
            StringsTR["UI_Acc_OK"] = "Tamam";
            #endregion
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
