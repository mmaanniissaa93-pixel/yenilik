using System;
using System.Collections.Generic;
using xBot.App;
using xBot.App.CommandCenter;

internal static class Program
{
    private static int failures;

    private static int Main()
    {
        Run("Bot çalışmıyorsa koruma aksiyonu yok", BaseInput(), BaseOptions(), ProtectionDecision.None);
        Run("Tetik kapalıysa koşul aksiyon üretmez", With(input => input.FullInventory = true), BaseOptions(), ProtectionDecision.None);

        ProtectionPolicyOptions allTriggers = BaseOptions();
        allTriggers.ReturnNoArrows = true;
        allTriggers.ReturnFullInventory = true;
        allTriggers.ReturnFullPetInventory = true;
        allTriggers.ReturnHPLow = true;
        allTriggers.ReturnMPLow = true;
        allTriggers.ReturnDurabilityLow = true;
        allTriggers.ReturnLevelUp = true;

        Run("Dolu envanter + scroll kasabaya döner", With(input => input.FullInventory = true, input => input.HasReturnScroll = true), allTriggers, ProtectionDecision.ReturnToTown);
        Run("Dolu envanter + scroll yoksa aksiyon yok", With(input => input.FullInventory = true), allTriggers, ProtectionDecision.None);
        Run("Dolu pet envanteri + scroll kasabaya döner", With(input => input.FullPetInventory = true, input => input.HasReturnScroll = true), allTriggers, ProtectionDecision.ReturnToTown);
        Run("Ok/bolt bittiğinde kasabaya döner", With(input => input.NoArrows = true, input => input.HasReturnScroll = true), allTriggers, ProtectionDecision.ReturnToTown);
        Run("HP stoğu eşikteyken kasabaya döner", With(input => input.HPLow = true, input => input.HasReturnScroll = true), allTriggers, ProtectionDecision.ReturnToTown);
        Run("MP stoğu eşikteyken kasabaya döner", With(input => input.MPLow = true, input => input.HasReturnScroll = true), allTriggers, ProtectionDecision.ReturnToTown);
        Run("Dayanıklılık eşikteyken kasabaya döner", With(input => input.DurabilityLow = true, input => input.HasReturnScroll = true), allTriggers, ProtectionDecision.ReturnToTown);
        Run("Level-up bekleyen bot kasabaya döner", With(input => input.LevelUpPending = true, input => input.HasReturnScroll = true), allTriggers, ProtectionDecision.ReturnToTown);

        ProtectionPolicyOptions deadOptions = BaseOptions();
        deadOptions.ReturnDeadWithDelay = true;
        Run("Ölüm gecikmesi dolmadan dönmez", With(input => input.IsAlive = false, input => input.DeadDelayElapsed = false, input => input.HasReturnScroll = true), deadOptions, ProtectionDecision.None);
        Run("Ölüm gecikmesi dolunca kasabaya döner", With(input => input.IsAlive = false, input => input.DeadDelayElapsed = true, input => input.HasReturnScroll = true), deadOptions, ProtectionDecision.ReturnToTown);

        ProtectionPolicyOptions stopOptions = BaseOptions();
        stopOptions.StopBotInTown = true;
        Run("Dönüş tamamlanıp şehre gelince bot durur", With(input => input.IsInTown = true, input => input.StopAfterReturn = true), stopOptions, ProtectionDecision.StopBotInTown);
        Run("Dönüş beklenmiyorsa şehirde bot durmaz", With(input => input.IsInTown = true), stopOptions, ProtectionDecision.None);

        RunFilter("Degree aralığı dışındaki ekipman alınmaz", new ItemFilterInput { IsEquipable = true, Degree = 8 }, FilterOptions(1, 7), null, false);
        RunFilter("SoX filtresi normal ekipmanı almaz", new ItemFilterInput { IsEquipable = true, Degree = 5, IsSox = false }, FilterOptions(1, 15, true), null, false);
        RunFilter("China kapalıyken China ekipmanı alınmaz", new ItemFilterInput { IsEquipable = true, Degree = 5, IsChina = true }, FilterOptions(1, 15, false, false, true), null, false);
        RunFilter("Female kapalıyken kadın ekipmanı alınmaz", new ItemFilterInput { IsEquipable = true, Degree = 5, IsFemale = true }, FilterOptions(1, 15, false, true, true, true, false), null, false);
        RunFilter("Gold ve alchemy filtreden bağımsız alınır", new ItemFilterInput { IsGold = true, Degree = 0 }, FilterOptions(8, 8, true, false, false, false, false), null, true);
        RunFilter("Açık pickup kuralı global filtreyi geçersiz kılar", new ItemFilterInput { IsEquipable = true, Degree = 8 }, FilterOptions(1, 7), new ItemFilterRule { Pickup = true }, true);
        RunFilter("Kapalı pickup kuralı itemi engeller", new ItemFilterInput { IsEquipable = false }, FilterOptions(1, 15), new ItemFilterRule { Pickup = false }, false);
        RunAction("Elixir/stone varsayılan olarak depolanır", new ItemFilterInput { IsElixirOrStone = true }, null, true, false);
        RunAction("SoX varsayılan olarak depolanır", new ItemFilterInput { IsSox = true }, null, true, false);
        RunAction("Açık sell kuralı satışa izin verir", new ItemFilterInput(), new ItemFilterRule { Sell = true }, false, true);
        RunAction("Store=false açık kuralı varsayılan SoX depolamasını kapatır", new ItemFilterInput { IsSox = true }, new ItemFilterRule { Store = false }, false, false);

        RunCombat("Avoid kuralındaki mob hedeflenmez", new CombatTargetInput { Avoided = true }, false);
        RunCombat("Dimension pillar ayarı açıkken hedeflenmez", new CombatTargetInput { IsDimensionPillar = true, IgnoreDimensionPillars = true }, false);
        RunCombat("Yarıçap dışındaki mob takip edilmez", new CombatTargetInput { DoNotFollowMobs = true, WithinTrainingArea = false }, false);
        RunCombat("Yarıçap içindeki izinli mob hedeflenir", new CombatTargetInput { DoNotFollowMobs = true, WithinTrainingArea = true }, true);
        RunBerserk("Berserk bar dolu değilse tetiklenmez", false, false, true, 5, true, 3, true, false);
        RunBerserk("Mob sayısı eşiği zerk tetikler", true, false, false, 3, true, 3, false, true);
        RunBerserk("HP tetikleyicisi mob yokken zerk tetiklemez", true, false, true, 0, true, 3, false, false);

        RunSkill("Başarılı skill sonrası fallback kullanılmaz", false, true, false);
        RunSkill("Skill başarısızsa canlı hedefte fallback kullanılır", true, false, true);
        RunSkill("Ölü hedefte fallback kullanılmaz", false, false, false);
        RunSkillCombo("Sıralı combo başarılı cast sonrası devam eder", true, true, true);
        RunSkillCombo("Sırasız combo başarılı cast sonrası durur", true, false, false);

        RunImbue("Çin Fire GIGONGTA skill'i tanınır", "SKILL_CH_FIRE_01_GIGONGTA_01", "Fire", true);
        RunImbue("Çin Lightning skill'i tanınır", "SKILL_CH_LIGHTNING_01_GIGONGTA_01", "Lightning", true);
        RunImbue("Kısa LIGHT varyantı tanınır", "SKILL_CH_LIGHT_01_ENCHANT_01", "Lightning", true);
        RunImbue("Normal Fire saldırısı imbue sayılmaz", "SKILL_CH_FIRE_01_ATTACK_01", "Fire", false);
        RunActiveImbue("Çin Fire imbue buff'ı aktif kabul edilir", "SKILL_CH_FIRE_01_GIGONGTA_01", "Fire", true);
        RunActiveImbue("Yanlış element aktif buff kabul edilmez", "BUFF_CH_COLD_GIGONGTA", "Fire", false);

        // Command Center (Emote & Chat Commands) Tests
        RunCommandCenter("Emote No -> stop", CommandCenterPolicy.ResolveDefaultCommandForEmote(EmoteType.No), "stop");
        RunCommandCenter("Emote Joy -> none", CommandCenterPolicy.ResolveDefaultCommandForEmote(EmoteType.Joy), "none");
        RunCommandCenter("Emote Rush -> area", CommandCenterPolicy.ResolveDefaultCommandForEmote(EmoteType.Rush), "area");
        RunCommandCenter("Emote Yes -> start", CommandCenterPolicy.ResolveDefaultCommandForEmote(EmoteType.Yes), "start");
        RunCommandCenter("Emote Greeting -> area", CommandCenterPolicy.ResolveDefaultCommandForEmote(EmoteType.Greeting), "area");
        RunCommandCenter("Emote Smile -> show", CommandCenterPolicy.ResolveDefaultCommandForEmote(EmoteType.Smile), "show");
        RunCommandCenter("Emote Hi -> none", CommandCenterPolicy.ResolveDefaultCommandForEmote(EmoteType.Hi), "none");

        var customMap = new Dictionary<string, string> { { "Greeting", "buff" }, { "Smile", "here" }, { "No", "invalid_cmd" } };
        RunCommandCenter("Custom Greeting -> buff", CommandCenterPolicy.ResolveAssignedEmoteCommand(EmoteType.Greeting, customMap), "buff");
        RunCommandCenter("Custom Smile -> here", CommandCenterPolicy.ResolveAssignedEmoteCommand(EmoteType.Smile, customMap), "here");
        RunCommandCenter("Invalid mapping fallback -> default stop", CommandCenterPolicy.ResolveAssignedEmoteCommand(EmoteType.No, customMap), "stop");

        RunChatCommand("Chat \\start -> start", "\\start", true, "start");
        RunChatCommand("Chat \\stop -> stop", "\\stop", true, "stop");
        RunChatCommand("Chat \\area -> area", "\\area", true, "area");
        RunChatCommand("Chat \\buff -> buff", "\\buff", true, "buff");
        RunChatCommand("Chat \\show -> show", "\\show", true, "show");
        RunChatCommand("Chat \\here -> here", "\\here", true, "here");
        RunChatCommand("Chat normal text -> false", "hello bot", false, null);
        RunChatCommand("Chat unknown cmd -> false", "\\randomcmd", false, null);

        // Town Logistics & Ammo / Return Scroll / Pet Unload Tests
        RunAmmoCheck("Bow (6) 200 ok varken ek ok alınmalı", 6, 200, true);
        RunAmmoCheck("Bow (6) 1500 ok varken ek ok gerekmez", 6, 1500, false);
        RunAmmoCheck("Crossbow (12) 400 bolt varken ek bolt alınmalı", 12, 400, true);
        RunAmmoCheck("Kılıç (1) kullanan ok/bolt almaz", 1, 0, false);

        RunAmmoSlot("Bow ok alış slotu: 0", TownLogisticsPolicy.GetAmmoShopSlot(AmmoType.Arrow), 0);
        RunAmmoSlot("Crossbow bolt alış slotu: 1", TownLogisticsPolicy.GetAmmoShopSlot(AmmoType.Bolt), 1);

        RunScrollCheck("1 scroll varken 4 adet satın alınmalı", 1, 5, 4);
        RunScrollCheck("5 scroll varken satın alma yapılmaz", 5, 5, 0);
        RunScrollCheck("8 scroll varken satın alma yapılmaz", 8, 5, 0);

        RunAction("Pet çantasındaki SoX eşyası depolanır", new ItemFilterInput { IsSox = true }, null, true, false);
        RunAction("Pet çantasındaki çöp eşya depolanmaz", new ItemFilterInput { IsEquipable = true, IsSox = false }, null, false, false);

        // Party Support, Heal, Ress and Weapon Swap Revert Tests
        RunPartySupport("Canı %50 olan parti üyesi (%70 eşik) iyileştirilir", PartyPolicy.ShouldHealPartyMember(50, 70), true);
        RunPartySupport("Canı %80 olan parti üyesi (%70 eşik) iyileştirilmez", PartyPolicy.ShouldHealPartyMember(80, 70), false);
        RunPartySupport("Ölü parti üyesi (%0 HP) iyileştirilmez (ress gerekir)", PartyPolicy.ShouldHealPartyMember(0, 70), false);

        RunPartySupport("Yakındaki ölü parti üyesine ress atılır", PartyPolicy.ShouldResurrectMember(0, true), true);
        RunPartySupport("Uzaktaki ölü parti üyesine ress atılmaz", PartyPolicy.ShouldResurrectMember(0, false), false);
        RunPartySupport("Canlı parti üyesine ress atılmaz", PartyPolicy.ShouldResurrectMember(50, true), false);

        RunPartySupport("Ress kabul kapalıyken istek reddedilir", PartyPolicy.ShouldAcceptRess(false, false, false), false);
        RunPartySupport("Ress kabul açık ve partyOnly kapalıyken kabul edilir", PartyPolicy.ShouldAcceptRess(true, false, false), true);
        RunPartySupport("PartyOnly açıkken parti üyesinden gelen ress kabul edilir", PartyPolicy.ShouldAcceptRess(true, true, true), true);
        RunPartySupport("PartyOnly açıkken yabancıdan gelen ress reddedilir", PartyPolicy.ShouldAcceptRess(true, true, false), false);

        RunPartySupport("Buff sonrası elde Cleric (14) varken ana silah Staff'a (9) dönülmeli", PartyPolicy.ShouldRevertWeapon(14, 9), true);
        RunPartySupport("Elde zaten ana silah Staff (9) varken geçiş yapılmaz", PartyPolicy.ShouldRevertWeapon(9, 9), false);
        RunPartySupport("Ana silah tanımlı değilse (0) geçiş yapılmaz", PartyPolicy.ShouldRevertWeapon(14, 0), false);

        RunPartySupport("Cleric Healing Division yeteneği tanınır", PartyPolicy.IsClericHealingSkill("SKILL_EU_CLERIC_HEAL_DIVISION_01"), true);
        RunPartySupport("Cleric Recovery Division yeteneği tanınır", PartyPolicy.IsClericHealingSkill("SKILL_EU_CLERIC_RECOVERY_DIVISION_01"), true);
        RunPartySupport("Cleric Resurrection yeteneği tanınır", PartyPolicy.IsClericRessSkill("SKILL_EU_CLERIC_RESURRECTION_01"), true);
        RunPartySupport("Cleric Rebirth Art yeteneği tanınır", PartyPolicy.IsClericRessSkill("SKILL_EU_CLERIC_REBIRTH_01"), true);
        RunPartySupport("Cleric Integrity kötü durum silme yeteneği tanınır", PartyPolicy.IsClericCureSkill("SKILL_EU_CLERIC_CURE_INTEGRITY_01"), true);
        RunPartySupport("Bard Moving March parti buffı tanınır", PartyPolicy.IsPartyBuffSkill("SKILL_EU_BARD_SPEED_01"), true);
        RunPartySupport("Warrior Pain Quota parti koruması tanınır", PartyPolicy.IsPartyBuffSkill("SKILL_EU_WARRIOR_GUARD_PAIN_01"), true);

        // Secondary Passcode (PIN) Tests
        RunPINCheck("4 haneli PIN geçerli", SecondaryPasscodePolicy.IsValidPasscode("1234"), true);
        RunPINCheck("6 haneli PIN geçerli", SecondaryPasscodePolicy.IsValidPasscode("123456"), true);
        RunPINCheck("8 haneli PIN geçerli", SecondaryPasscodePolicy.IsValidPasscode("12345678"), true);
        RunPINCheck("3 haneli PIN geçersiz", SecondaryPasscodePolicy.IsValidPasscode("123"), false);
        RunPINCheck("9 haneli PIN geçersiz", SecondaryPasscodePolicy.IsValidPasscode("123456789"), false);
        RunPINCheck("Boş PIN geçersiz", SecondaryPasscodePolicy.IsValidPasscode(""), false);
        RunPINCheck("Null PIN geçersiz", SecondaryPasscodePolicy.IsValidPasscode(null), false);

        RunPINString("Hesap PIN'i global PIN'e önceliklidir", SecondaryPasscodePolicy.ResolvePasscode("654321", "112233"), "654321");
        RunPINString("Hesap PIN'i yoksa global PIN seçilir", SecondaryPasscodePolicy.ResolvePasscode("", "112233"), "112233");
        RunPINString("İki PIN de yoksa boş döner", SecondaryPasscodePolicy.ResolvePasscode("", ""), "");
        RunPINString("Hesap PIN'i geçersizse (2 hane) global PIN fallback yapılır", SecondaryPasscodePolicy.ResolvePasscode("12", "112233"), "112233");

        RunPINCheck("0x7625 CLIENT_SECONDARY_PASSCODE tanınır", SecondaryPasscodePolicy.IsPasscodeOpcode(0x7625), true);
        RunPINCheck("0x3625 SERVER_SECONDARY_PASSCODE_REQUEST tanınır", SecondaryPasscodePolicy.IsPasscodeOpcode(0x3625), true);
        RunPINCheck("0xB625 SERVER_SECONDARY_PASSCODE_RESPONSE tanınır", SecondaryPasscodePolicy.IsPasscodeOpcode(0xB625), true);
        RunPINCheck("0x7001 normal opcode passcode sayılmaz", SecondaryPasscodePolicy.IsPasscodeOpcode(0x7001), false);

        RunPINCheck("AutoEnter açık ve geçerli PIN -> gönderilmeli", SecondaryPasscodePolicy.ShouldSendPasscode(true, "123456"), true);
        RunPINCheck("AutoEnter kapalı ve geçerli PIN -> gönderilmemeli", SecondaryPasscodePolicy.ShouldSendPasscode(false, "123456"), false);
        RunPINCheck("AutoEnter açık ama boş PIN -> gönderilmemeli", SecondaryPasscodePolicy.ShouldSendPasscode(true, ""), false);
        RunPINCheck("Hesapta PIN kayıtlıysa (354500) AutoEnter kapalı olsa bile gönderilmeli", SecondaryPasscodePolicy.ShouldSendPasscodeForAccount("354500", false, ""), true);
        RunPINCheck("Hesapta PIN kayıtlı değilse ve AutoEnter kapalıysa gönderilmemeli", SecondaryPasscodePolicy.ShouldSendPasscodeForAccount("", false, ""), false);
        RunPINCheck("Hesapta PIN yok ama global AutoEnter açıksa global PIN ile gönderilmeli", SecondaryPasscodePolicy.ShouldSendPasscodeForAccount("", true, "123456"), true);
        RunPINCheck("Hesapta geçersiz PIN (2 hane) varsa ve global AutoEnter kapalıysa gönderilmemeli", SecondaryPasscodePolicy.ShouldSendPasscodeForAccount("12", false, ""), false);

        // SOCKS5 Proxy Tests
        RunSocks5Check("SOCKS5 geçerli konfigürasyon doğrulanır", Socks5Policy.IsValidConfig("127.0.0.1", 1080), true);
        RunSocks5Check("SOCKS5 boş host geçersizdir", Socks5Policy.IsValidConfig("", 1080), false);
        RunSocks5Check("SOCKS5 port 0 geçersizdir", Socks5Policy.IsValidConfig("127.0.0.1", 0), false);

        byte[] greetingNoAuth = Socks5Policy.BuildGreeting(false);
        RunSocks5Check("Auth yokken SOCKS5 greeting [0x05, 0x01, 0x00] olmalı",
            greetingNoAuth.Length == 3 && greetingNoAuth[0] == 0x05 && greetingNoAuth[1] == 0x01 && greetingNoAuth[2] == 0x00, true);

        byte[] greetingAuth = Socks5Policy.BuildGreeting(true);
        RunSocks5Check("Auth varken SOCKS5 greeting [0x05, 0x02, 0x00, 0x02] olmalı",
            greetingAuth.Length == 4 && greetingAuth[0] == 0x05 && greetingAuth[1] == 0x02 && greetingAuth[2] == 0x00 && greetingAuth[3] == 0x02, true);

        RunSocks5Byte("SOCKS5 NoAuth yanıtı (0x00) tanınır", Socks5Policy.ParseGreetingResponse(new byte[] { 0x05, 0x00 }, 2), 0x00);
        RunSocks5Byte("SOCKS5 UserPass yanıtı (0x02) tanınır", Socks5Policy.ParseGreetingResponse(new byte[] { 0x05, 0x02 }, 2), 0x02);
        RunSocks5Byte("SOCKS5 Red yanıtı (0xFF) tanınır", Socks5Policy.ParseGreetingResponse(new byte[] { 0x05, 0xFF }, 2), 0xFF);
        RunSocks5Byte("SOCKS5 geçersiz protokol versiyonu red edilir", Socks5Policy.ParseGreetingResponse(new byte[] { 0x04, 0x00 }, 2), 0xFF);

        byte[] authReq = Socks5Policy.BuildAuthRequest("testuser", "secretpass");
        RunSocks5Check("RFC 1929 auth isteği doğru başlık ve uzunluklara sahip",
            authReq.Length == 3 + 8 + 10 && authReq[0] == 0x01 && authReq[1] == 8 && authReq[10] == 10, true);

        RunSocks5Check("RFC 1929 auth başarı yanıtı (0x00) kabul edilir", Socks5Policy.ParseAuthResponse(new byte[] { 0x01, 0x00 }, 2), true);
        RunSocks5Check("RFC 1929 auth hata yanıtı (0x01) reddedilir", Socks5Policy.ParseAuthResponse(new byte[] { 0x01, 0x01 }, 2), false);

        byte[] connectIpReq = Socks5Policy.BuildConnectRequest("192.168.1.1", 15779);
        RunSocks5Check("SOCKS5 CONNECT IPv4 isteği doğru paket yapısına sahip",
            connectIpReq[0] == 0x05 && connectIpReq[1] == 0x01 && connectIpReq[3] == 0x01 &&
            connectIpReq[4] == 192 && connectIpReq[5] == 168 && connectIpReq[6] == 1 && connectIpReq[7] == 1 &&
            connectIpReq[8] == ((15779 >> 8) & 0xFF) && connectIpReq[9] == (15779 & 0xFF), true);

        byte[] connectDomainReq = Socks5Policy.BuildConnectRequest("gw.silkroad.com", 15779);
        RunSocks5Check("SOCKS5 CONNECT Domain isteği ATYP 0x03 ve doğru uzunluğa sahip",
            connectDomainReq[0] == 0x05 && connectDomainReq[1] == 0x01 && connectDomainReq[3] == 0x03 &&
            connectDomainReq[4] == 15, true);

        bool parseSuccess = Socks5Policy.ParseConnectResponse(new byte[] { 0x05, 0x00, 0x00, 0x01, 0, 0, 0, 0, 0, 0 }, 10, out string err1);
        RunSocks5Check("SOCKS5 CONNECT REP_SUCCESS (0x00) başarılı kabul edilir", parseSuccess && string.IsNullOrEmpty(err1), true);

        bool parseRefused = Socks5Policy.ParseConnectResponse(new byte[] { 0x05, 0x05, 0x00, 0x01, 0, 0, 0, 0, 0, 0 }, 10, out string err2);
        RunSocks5Check("SOCKS5 CONNECT REP_CONNECTION_REFUSED (0x05) hata mesajı üretir", !parseRefused && err2.Contains("refused"), true);

        // Auto Alchemy (+ Basma) Tests
        RunAlchemyByte("Level 1 -> D1 hesaplanır", AlchemyPolicy.CalculateDegree(1), 1);
        RunAlchemyByte("Level 16 -> D3 hesaplanır", AlchemyPolicy.CalculateDegree(16), 3);
        RunAlchemyByte("Level 64 -> D8 hesaplanır", AlchemyPolicy.CalculateDegree(64), 8);
        RunAlchemyByte("Level 90 -> D10 hesaplanır", AlchemyPolicy.CalculateDegree(90), 10);
        RunAlchemyByte("Level 101 -> D11 hesaplanır", AlchemyPolicy.CalculateDegree(101), 11);

        RunAlchemyCheck("Silah (id3: 6) için Elixir Weapon gerekir", AlchemyPolicy.GetRequiredElixirType(6) == ElixirType.Weapon, true);
        RunAlchemyCheck("Kalkan (id3: 4) için Elixir Shield gerekir", AlchemyPolicy.GetRequiredElixirType(4) == ElixirType.Shield, true);
        RunAlchemyCheck("Zırh (id3: 1) için Elixir Protector gerekir", AlchemyPolicy.GetRequiredElixirType(1) == ElixirType.Protector, true);
        RunAlchemyCheck("Takı (id3: 12) için Elixir Accessory gerekir", AlchemyPolicy.GetRequiredElixirType(12) == ElixirType.Accessory, true);
        RunAlchemyCheck("Bilinmeyen eşya (id3: 99) için Elixir None döner", AlchemyPolicy.GetRequiredElixirType(99) == ElixirType.None, true);

        RunAlchemyCheck("Elixir Weapon silahla eşleşir", AlchemyPolicy.IsElixirMatching("ITEM_ETC_ARCHEMY_POTION_WEAPON", ElixirType.Weapon), true);
        RunAlchemyCheck("Elixir Weapon kalkanla eşleşmez", AlchemyPolicy.IsElixirMatching("ITEM_ETC_ARCHEMY_POTION_WEAPON", ElixirType.Shield), false);
        RunAlchemyCheck("Elixir Shield kalkanla eşleşir", AlchemyPolicy.IsElixirMatching("ITEM_ETC_ARCHEMY_POTION_SHIELD", ElixirType.Shield), true);
        RunAlchemyCheck("Elixir Armor zırhla eşleşir", AlchemyPolicy.IsElixirMatching("ITEM_ETC_ARCHEMY_POTION_ARMOR", ElixirType.Protector), true);
        RunAlchemyCheck("Elixir Accessory takıyla eşleşir", AlchemyPolicy.IsElixirMatching("ITEM_ETC_ARCHEMY_POTION_ACCESSORY", ElixirType.Accessory), true);

        RunAlchemyCheck("Lucky Powder D01 1st Degree ile eşleşir", AlchemyPolicy.IsLuckyPowderMatching("ITEM_ETC_ARCHEMY_REINFORCE_RECIPE_01", 1), true);
        RunAlchemyCheck("Lucky Powder D08 8th Degree ile eşleşir", AlchemyPolicy.IsLuckyPowderMatching("ITEM_ETC_ARCHEMY_REINFORCE_RECIPE_08", 8), true);
        RunAlchemyCheck("Lucky Powder D08 7th Degree ile eşleşmez", AlchemyPolicy.IsLuckyPowderMatching("ITEM_ETC_ARCHEMY_REINFORCE_RECIPE_08", 7), false);

        RunAlchemyCheck("Hedefe ulaşılmadıysa ve malzeme varsa devam edilir", AlchemyPolicy.ShouldContinueFusing(2, 3, 5, 2, 50), true);
        RunAlchemyCheck("Hedef artıya ulaşıldığında (+3 -> +3) simya durur", AlchemyPolicy.ShouldContinueFusing(3, 3, 5, 3, 50), false);
        RunAlchemyCheck("Elixir bittiğinde simya durur", AlchemyPolicy.ShouldContinueFusing(1, 3, 0, 1, 50), false);
        RunAlchemyCheck("Maksimum deneme aşıldığında simya durur", AlchemyPolicy.ShouldContinueFusing(1, 3, 5, 50, 50), false);

        bool alcOk = AlchemyPolicy.ParseAlchemyResponse(1, out bool alcSuccess, out string alcMsg);
        RunAlchemyCheck("Simya başarı yanıtı (code: 1) tanınır", alcOk && alcSuccess, true);

        bool alcFail = AlchemyPolicy.ParseAlchemyResponse(2, out bool alcFailSuccess, out string alcFailMsg);
        RunAlchemyCheck("Simya başarısızlık yanıtı (code: 2) tanınır", alcFail && !alcFailSuccess, true);

        bool alcDest = AlchemyPolicy.ParseAlchemyResponse(3, out bool alcDestSuccess, out string alcDestMsg);
        RunAlchemyCheck("Simya kırılma yanıtı (code: 3) tanınır", alcDest && !alcDestSuccess && alcDestMsg.Contains("destroyed"), true);

        if (failures != 0)
        {
            Console.WriteLine("Protection senaryoları başarısız: " + failures);
            return 1;
        }

        Console.WriteLine("Koruma, item filtre, combat, skill, imbue, command center, lojistik, parti, PIN, SOCKS5 ve Auto Alchemy senaryoları başarılı: 154");
        return 0;
    }

    private static ProtectionPolicyInput BaseInput()
    {
        return new ProtectionPolicyInput
        {
            IsInGame = true,
            IsBotting = true,
            IsAlive = true,
            IsInTown = false,
            HasReturnScroll = false
        };
    }

    private static ProtectionPolicyOptions BaseOptions()
    {
        return new ProtectionPolicyOptions();
    }

    private static ProtectionPolicyInput With(params Action<ProtectionPolicyInput>[] changes)
    {
        ProtectionPolicyInput input = BaseInput();
        foreach (Action<ProtectionPolicyInput> change in changes)
            change(input);
        return input;
    }

    private static void Run(string name, ProtectionPolicyInput input, ProtectionPolicyOptions options, ProtectionDecision expected)
    {
        ProtectionDecision actual = ProtectionPolicy.Evaluate(input, options);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static ItemFilterOptions FilterOptions(int min, int max, bool onlySox = false,
        bool filterChina = true, bool filterEurope = true, bool filterMale = true, bool filterFemale = true)
    {
        return new ItemFilterOptions
        {
            MinDegree = min,
            MaxDegree = max,
            OnlySox = onlySox,
            FilterChina = filterChina,
            FilterEurope = filterEurope,
            FilterMale = filterMale,
            FilterFemale = filterFemale
        };
    }

    private static void RunFilter(string name, ItemFilterInput input, ItemFilterOptions options, ItemFilterRule rule, bool expected)
    {
        bool actual = ItemFilterPolicy.ShouldPickup(input, options, rule);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunAction(string name, ItemFilterInput input, ItemFilterRule rule, bool expectedStore, bool expectedSell)
    {
        bool actualStore = ItemFilterPolicy.ShouldStore(input, rule);
        bool actualSell = ItemFilterPolicy.ShouldSell(input, rule);
        if (actualStore == expectedStore && actualSell == expectedSell)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | store beklenen=" + expectedStore + ", gerçek=" + actualStore
            + "; sell beklenen=" + expectedSell + ", gerçek=" + actualSell);
    }

    private static void RunCombat(string name, CombatTargetInput input, bool expected)
    {
        bool actual = CombatPolicy.CanTarget(input);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunBerserk(string name, bool barFull, bool alreadyBerserk,
        bool hpFullTrigger, int nearbyMobs, bool countEnabled, int countThreshold,
        bool mobRuleTrigger, bool expected)
    {
        bool actual = CombatPolicy.ShouldBerserk(barFull, alreadyBerserk, hpFullTrigger,
            nearbyMobs, countEnabled, countThreshold, mobRuleTrigger);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunSkill(string name, bool targetAlive, bool castConfirmed, bool expected)
    {
        bool actual = SkillPolicy.ShouldUseFallback(targetAlive, castConfirmed);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunSkillCombo(string name, bool castConfirmed, bool castInOrder, bool expected)
    {
        bool actual = SkillPolicy.ShouldContinueCombo(castConfirmed, castInOrder);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunImbue(string name, string serverName, string element, bool expected)
    {
        bool actual = ImbuePolicy.IsImbueSkill(serverName, element);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunActiveImbue(string name, string serverName, string element, bool expected)
    {
        bool actual = ImbuePolicy.IsActiveImbue(serverName, element);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunCommandCenter(string name, string actual, string expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunChatCommand(string name, string message, bool expectedResult, string expectedCmd)
    {
        bool actualResult = CommandCenterPolicy.ParseChatCommand(message, out string actualCmd);
        if (actualResult == expectedResult && actualCmd == expectedCmd)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenenResult=" + expectedResult + ", gerçekResult=" + actualResult + " | beklenenCmd=" + expectedCmd + ", gerçekCmd=" + actualCmd);
    }

    private static void RunAmmoCheck(string name, int weaponType, int currentAmmo, bool expectedNeedAmmo)
    {
        bool actualNeedAmmo = TownLogisticsPolicy.ShouldBuyAmmo(weaponType, currentAmmo);
        if (actualNeedAmmo == expectedNeedAmmo)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expectedNeedAmmo + ", gerçek=" + actualNeedAmmo);
    }

    private static void RunAmmoSlot(string name, byte actualSlot, byte expectedSlot)
    {
        if (actualSlot == expectedSlot)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expectedSlot + ", gerçek=" + actualSlot);
    }

    private static void RunScrollCheck(string name, int currentScrolls, int targetScrolls, int expectedMissing)
    {
        int actualMissing = TownLogisticsPolicy.CalculateMissingScrolls(currentScrolls, targetScrolls);
        if (actualMissing == expectedMissing)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expectedMissing + ", gerçek=" + actualMissing);
    }

    private static void RunPartySupport(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunPINCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunPINString(string name, string actual, string expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunSocks5Check(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunSocks5Byte(string name, byte actual, byte expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunAlchemyCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunAlchemyByte(string name, byte actual, byte expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }
}
