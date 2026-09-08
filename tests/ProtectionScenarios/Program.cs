using System;
using System.Collections.Generic;
using xBot.App;
using xBot.App.CommandCenter;
using xBot.Game.Objects;

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
        RunPickFilter("Ana toplama ayarı kapalıysa açık kural da eşya toplamaz", new ItemFilterInput { IsGold = true }, new ItemFilterRule { Pickup = true }, new PickFilterOptions { Enabled = false }, false);
        RunPickFilter("Ana toplama ayarı açıksa pickup kuralı çalışır", new ItemFilterInput { IsGold = true }, new ItemFilterRule { Pickup = true }, new PickFilterOptions { Enabled = true }, true);
        RunPickPet("Ana toplama ayarı kapalıysa pet kuralı da çalışmaz", new ItemFilterRule { Pet = true }, new PickFilterOptions { Enabled = false, UsePickPet = true }, false);
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

        RunSkillOrder("Sıralı seçim sıradakini alır ve imleci ilerletir", new[] { true, true, true }, 0, 0, 1);
        RunSkillOrder("Beklemedeki skill atlanıp altındaki kullanılır", new[] { false, true, true }, 0, 1, 2);
        RunSkillOrder("Ortadaki beklemedeyse altındaki kullanılır", new[] { true, false, true }, 1, 2, 0);
        RunSkillOrder("Liste biterse başa sarılır", new[] { true, false, false }, 2, 0, 1);
        RunSkillOrder("Hiçbiri hazır değilse -1 döner, imleç değişmez", new[] { false, false, false }, 1, -1, 1);
        RunSkillOrder("Bozuk imleç sıfırlanır", new[] { true, true }, 9, 0, 1);
        RunSkillOrder("Boş liste -1 döner", new bool[0], 0, -1, 0);

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

        // Target Assist (PvP Hedef Döngüsü & Filtreleme) Tests
        var taBaseSettings = new TargetAssistSettings
        {
            Enabled = true,
            MaxRange = 40.0,
            RoleMode = TargetAssistRoleMode.Civil,
            IncludeDeadTargets = false,
            IgnoreSnowShieldTargets = true,
            IgnoreBloodyStormTargets = false,
            OnlyCustomPlayers = false
        };

        var validPlayer = new TargetCandidateData
        {
            UniqueId = 101,
            Name = "EnemyPlayer",
            Distance = 25.0,
            GuildName = "RedDragon",
            IsDead = false,
            HasSnowShield = false,
            HasBloodyStorm = false,
            HasJobMode = false,
            JobType = 0
        };

        RunTargetAssistCheck("Menzil içindeki canlı oyuncu geçerli hedeftir", TargetAssistPolicy.IsValidTarget(validPlayer, taBaseSettings, 999), true);

        var farPlayer = new TargetCandidateData { UniqueId = 102, Name = "FarPlayer", Distance = 55.0 };
        RunTargetAssistCheck("Menzil dışındaki (55m > 40m) oyuncu hedeflenmez", TargetAssistPolicy.IsValidTarget(farPlayer, taBaseSettings, 999), false);

        var selfPlayer = new TargetCandidateData { UniqueId = 999, Name = "MyChar", Distance = 0.0 };
        RunTargetAssistCheck("Karakterin kendisi hedeflenmez", TargetAssistPolicy.IsValidTarget(selfPlayer, taBaseSettings, 999), false);

        var deadPlayer = new TargetCandidateData { UniqueId = 103, Name = "DeadPlayer", Distance = 15.0, IsDead = true };
        RunTargetAssistCheck("IncludeDeadTargets kapalıyken ölü hedef elenir", TargetAssistPolicy.IsValidTarget(deadPlayer, taBaseSettings, 999), false);

        var taAllowDead = new TargetAssistSettings { Enabled = true, MaxRange = 40.0, IncludeDeadTargets = true };
        RunTargetAssistCheck("IncludeDeadTargets açıkken ölü hedef kabul edilir", TargetAssistPolicy.IsValidTarget(deadPlayer, taAllowDead, 999), true);

        var snowPlayer = new TargetCandidateData { UniqueId = 104, Name = "SnowPlayer", Distance = 20.0, HasSnowShield = true };
        RunTargetAssistCheck("IgnoreSnowShield açıkken Snow Shield'lı hedef atlanır", TargetAssistPolicy.IsValidTarget(snowPlayer, taBaseSettings, 999), false);
        var taAllowSnow = new TargetAssistSettings { Enabled = true, MaxRange = 40.0, IgnoreSnowShieldTargets = false };
        RunTargetAssistCheck("IgnoreSnowShield kapalıyken Snow Shield'lı hedef kabul edilir", TargetAssistPolicy.IsValidTarget(snowPlayer, taAllowSnow, 999), true);

        var bloodyPlayer = new TargetCandidateData { UniqueId = 105, Name = "BloodyPlayer", Distance = 20.0, HasBloodyStorm = true };
        RunTargetAssistCheck("IgnoreBloodyStorm kapalıyken Bloody Storm'lu hedef kabul edilir", TargetAssistPolicy.IsValidTarget(bloodyPlayer, taBaseSettings, 999), true);
        var taIgnoreBloody = new TargetAssistSettings { Enabled = true, MaxRange = 40.0, IgnoreBloodyStormTargets = true };
        RunTargetAssistCheck("IgnoreBloodyStorm açıkken Bloody Storm'lu hedef atlanır", TargetAssistPolicy.IsValidTarget(bloodyPlayer, taIgnoreBloody, 999), false);

        var guildSettings = new TargetAssistSettings { Enabled = true, MaxRange = 40.0 };
        guildSettings.IgnoredGuilds.Add("BlackListGuild");
        var ignoredGuildMember = new TargetCandidateData { UniqueId = 106, Name = "IgnoredMember", Distance = 10.0, GuildName = "BlackListGuild" };
        var friendGuildMember = new TargetCandidateData { UniqueId = 107, Name = "FriendMember", Distance = 10.0, GuildName = "WhiteListGuild" };
        RunTargetAssistCheck("Ignored Guild üyesi listeden elenir", TargetAssistPolicy.IsValidTarget(ignoredGuildMember, guildSettings, 999), false);
        RunTargetAssistCheck("Normal guild üyesi kabul edilir", TargetAssistPolicy.IsValidTarget(friendGuildMember, guildSettings, 999), true);

        var customSettings = new TargetAssistSettings { Enabled = true, MaxRange = 40.0, OnlyCustomPlayers = true };
        customSettings.CustomPlayers.Add("TargetAlpha");
        var customTarget = new TargetCandidateData { UniqueId = 108, Name = "TargetAlpha", Distance = 15.0 };
        var otherPlayer = new TargetCandidateData { UniqueId = 109, Name = "RandomPlayer", Distance = 15.0 };
        RunTargetAssistCheck("OnlyCustomPlayers açıkken listedeki oyuncu hedeflenir", TargetAssistPolicy.IsValidTarget(customTarget, customSettings, 999), true);
        RunTargetAssistCheck("OnlyCustomPlayers açıkken listede olmayan oyuncu hedeflenmez", TargetAssistPolicy.IsValidTarget(otherPlayer, customSettings, 999), false);

        // Role mode checks
        var thiefSuitTarget = new TargetCandidateData { UniqueId = 110, Name = "ThiefEnemy", Distance = 12.0, HasJobMode = true, JobType = 2 }; // Thief
        var hunterSuitTarget = new TargetCandidateData { UniqueId = 111, Name = "HunterEnemy", Distance = 12.0, HasJobMode = true, JobType = 3 }; // Hunter
        var traderSuitTarget = new TargetCandidateData { UniqueId = 112, Name = "TraderEnemy", Distance = 12.0, HasJobMode = true, JobType = 1 }; // Trader
        var civilPlayer = new TargetCandidateData { UniqueId = 113, Name = "Civilian", Distance = 12.0, HasJobMode = false, JobType = 0 };

        var thiefSettings = new TargetAssistSettings { Enabled = true, MaxRange = 40.0, RoleMode = TargetAssistRoleMode.Thief };
        RunTargetAssistCheck("Thief modu: Hunter hedef alınır", TargetAssistPolicy.IsValidTarget(hunterSuitTarget, thiefSettings, 999), true);
        RunTargetAssistCheck("Thief modu: Trader hedef alınır", TargetAssistPolicy.IsValidTarget(traderSuitTarget, thiefSettings, 999), true);
        RunTargetAssistCheck("Thief modu: Başka Thief hedef alınmaz", TargetAssistPolicy.IsValidTarget(thiefSuitTarget, thiefSettings, 999), false);
        RunTargetAssistCheck("Thief modu: Mesleksiz oyuncu hedef alınmaz", TargetAssistPolicy.IsValidTarget(civilPlayer, thiefSettings, 999), false);

        var hunterTraderSettings = new TargetAssistSettings { Enabled = true, MaxRange = 40.0, RoleMode = TargetAssistRoleMode.HunterTrader };
        RunTargetAssistCheck("HunterTrader modu: Thief hedef alınır", TargetAssistPolicy.IsValidTarget(thiefSuitTarget, hunterTraderSettings, 999), true);
        RunTargetAssistCheck("HunterTrader modu: Hunter hedef alınmaz", TargetAssistPolicy.IsValidTarget(hunterSuitTarget, hunterTraderSettings, 999), false);

        // Cycle resolution tests
        var candidatesList = new List<TargetCandidateData>
        {
            new TargetCandidateData { UniqueId = 201, Name = "Target1", Distance = 10.0 },
            new TargetCandidateData { UniqueId = 202, Name = "Target2", Distance = 20.0 },
            new TargetCandidateData { UniqueId = 203, Name = "Target3", Distance = 30.0 }
        };

        RunTargetAssistCheck("Hedef seçili değilken (0) en yakın ilk aday (201) seçilir",
            TargetAssistPolicy.ResolveNextTarget(candidatesList, 0)?.UniqueId == 201, true);

        RunTargetAssistCheck("201 seçiliyken sıradaki hedef 202 seçilir",
            TargetAssistPolicy.ResolveNextTarget(candidatesList, 201)?.UniqueId == 202, true);

        RunTargetAssistCheck("Son hedef 203 seçiliyken döngü başa (201) döner",
            TargetAssistPolicy.ResolveNextTarget(candidatesList, 203)?.UniqueId == 201, true);

        // Buff helpers
        RunTargetAssistCheck("COLD_SHIELD yeteneği Snow Shield olarak tanınır", TargetAssistPolicy.HasSnowShield("SKILL_CH_COLD_SHIELD_01"), true);
        RunTargetAssistCheck("AUTO_TRANSFER parametresi Snow Shield olarak tanınır", TargetAssistPolicy.HasSnowShield("UNKNOWN", 1701213281), true);
        RunTargetAssistCheck("FANSTORM yeteneği Bloody Storm olarak tanınır", TargetAssistPolicy.HasBloodyStorm("SKILL_EU_WARRIOR_FANSTORM_01"), true);
        RunTargetAssistCheck("FAN_STORM yeteneği Bloody Storm olarak tanınır", TargetAssistPolicy.HasBloodyStorm("SKILL_EU_WARRIOR_FAN_STORM_01"), true);

        // Status formatter
        string emptyStatus = TargetAssistPolicy.FormatCandidateStatus(0, "", -1);
        RunTargetAssistCheck("Aday yokken doğru durum metni üretilir", emptyStatus == "No target candidates in range.", true);
        string activeStatus = TargetAssistPolicy.FormatCandidateStatus(3, "Target1", 12.4);
        RunTargetAssistCheck("Aday varken formatlı durum metni üretilir", activeStatus.Contains("Candidates: 3") && activeStatus.Contains("Target1") && activeStatus.Contains("12.4m"), true);

        // Localization (TR / EN Dynamic Optimization) Tests
        bool langEventFired = false;
        LocalizationManager.OnLanguageChanged += () => { langEventFired = true; };

        // Test Turkish Localization
        LocalizationManager.SetLanguage("TR");
        RunLocalizationCheck("Dil TR olarak seçildi", LocalizationManager.CurrentLanguage == "TR", true);
        RunLocalizationCheck("Dil değiştirme event'i tetiklendi", langEventFired, true);

        RunLocalizationString("TR Kategori: BOT AYARLARI", LocalizationManager.Get("UI_Cat_BotSettings"), "BOT AYARLARI");
        RunLocalizationString("TR Tab: Genel / Giriş", LocalizationManager.Get("UI_Tab_Login"), "Genel / Giriş");
        RunLocalizationString("TR Tab: Kasılma", LocalizationManager.Get("UI_Tab_Training"), "Kasılma");
        RunLocalizationString("TR Tab: Beceriler", LocalizationManager.Get("UI_Tab_Skills"), "Beceriler");
        RunLocalizationString("TR Tab: Koruma", LocalizationManager.Get("UI_Tab_Character"), "Koruma");
        RunLocalizationString("TR Tab: Simya (+ Basma)", LocalizationManager.Get("UI_Tab_Alchemy"), "Simya (+ Basma)");
        RunLocalizationString("TR Tab: Target Assist", LocalizationManager.Get("UI_Tab_TargetAssist"), "Target Assist");
        RunLocalizationString("TR Buton: Ayarları Kaydet", LocalizationManager.Get("UI_Save"), "Ayarları Kaydet");
        RunLocalizationString("TR Buton: Komut Merkezi", LocalizationManager.Get("UI_CommandCenter"), "Komut Merkezi");
        RunLocalizationString("TR Giriş: Otomatik Giriş Yap", LocalizationManager.Get("UI_AutomatedLogin"), "Otomatik Giriş Yap");
        RunLocalizationString("TR Combat: HP tam doluyken", LocalizationManager.Get("UI_ZerkHPFull"), "HP tam doluyken");
        RunLocalizationString("TR Savaş: Alana Dönüş", LocalizationManager.Get("UI_ReturnTitle"), "Alana Dönüş");
        RunLocalizationString("TR Kasılma: Alan sekmesi", LocalizationManager.Get("UI_Tr_Area"), "Alan");
        RunLocalizationString("TR Kasılma: Takip sekmesi", LocalizationManager.Get("UI_Tr_Trace"), "Takip");
        RunLocalizationString("TR Kaçınma: Genel satırı", LocalizationManager.Get("UI_R_General"), "Genel");
        RunLocalizationString("TR Kayıt: BAŞLAT", LocalizationManager.Get("UI_Tr_Start"), "BAŞLAT");
        RunLocalizationString("TR Beceri: Becerileri Sırayla Kullan (Kombo)", LocalizationManager.Get("UI_InOrder"), "Becerileri Sırayla Kullan (Kombo)");
        RunLocalizationString("TR Koruma: HP < % ise Beceriyle İyileş", LocalizationManager.Get("UI_SkillHP"), "HP < % ise Beceriyle İyileş");
        RunLocalizationString("TR Koruma: Ölen Peti Dirilt (Grass of Life)", LocalizationManager.Get("UI_PetRevive"), "Ölen Peti Dirilt (Grass of Life)");
        RunLocalizationString("TR Filtre: Sadece SoX", LocalizationManager.Get("UI_SoxPrint"), "Sadece SoX (SOS / SOM / SUN / Nova)");
        RunLocalizationString("TR Simya: ▶ Simyayı Başlat", LocalizationManager.Get("UI_Alchemy_Start"), "▶ Simyayı Başlat");
        RunLocalizationString("TR Simya: Lucky Powder Kullan (Otomatik Eşle)", LocalizationManager.Get("UI_Alchemy_UsePowder"), "Lucky Powder Kullan (Otomatik Eşle)");
        RunLocalizationString("TR TargetAssist: Ölü hedefleri dahil et", LocalizationManager.Get("UI_TA_IncludeDead"), "Ölü hedefleri dahil et");
        RunLocalizationString("TR Hesap Yönetimi: Başlık", LocalizationManager.Get("UI_Acc_Title"), "Hesap Yönetimi");
        RunLocalizationString("TR Hesap Yönetimi: İkinci Şifre (PIN)", LocalizationManager.Get("UI_Acc_Secondary"), "İkinci Şifre (PIN)");

        // Test English Localization
        langEventFired = false;
        LocalizationManager.SetLanguage("EN");
        RunLocalizationCheck("Dil EN olarak seçildi", LocalizationManager.CurrentLanguage == "EN", true);
        RunLocalizationCheck("Dil değiştirme event'i EN için tetiklendi", langEventFired, true);

        RunLocalizationString("EN Kategori: BOT SETTINGS", LocalizationManager.Get("UI_Cat_BotSettings"), "BOT SETTINGS");
        RunLocalizationString("EN Tab: General / Login", LocalizationManager.Get("UI_Tab_Login"), "General / Login");
        RunLocalizationString("EN Tab: Training", LocalizationManager.Get("UI_Tab_Training"), "Training");
        RunLocalizationString("EN Tab: Skills", LocalizationManager.Get("UI_Tab_Skills"), "Skills");
        RunLocalizationString("EN Tab: Protection", LocalizationManager.Get("UI_Tab_Character"), "Protection");
        RunLocalizationString("EN Tab: Alchemy (+ Fuse)", LocalizationManager.Get("UI_Tab_Alchemy"), "Alchemy (+ Fuse)");
        RunLocalizationString("EN Tab: Target Assist", LocalizationManager.Get("UI_Tab_TargetAssist"), "Target Assist");
        RunLocalizationString("EN Buton: Save Settings", LocalizationManager.Get("UI_Save"), "Save Settings");
        RunLocalizationString("EN Buton: Command Center", LocalizationManager.Get("UI_CommandCenter"), "Command Center");
        RunLocalizationString("EN Giriş: Enable Automated Login", LocalizationManager.Get("UI_AutomatedLogin"), "Enable Automated Login");
        RunLocalizationString("EN Combat: When HP is full", LocalizationManager.Get("UI_ZerkHPFull"), "When HP is full");
        RunLocalizationString("EN Savaş: Return to Area", LocalizationManager.Get("UI_ReturnTitle"), "Return to Area");
        RunLocalizationString("EN Kasılma: Trace sekmesi", LocalizationManager.Get("UI_Tr_Trace"), "Trace");
        RunLocalizationString("EN Kaçınma: General satırı", LocalizationManager.Get("UI_R_General"), "General");
        RunLocalizationString("EN Kayıt: START", LocalizationManager.Get("UI_Tr_Start"), "START");
        RunLocalizationString("EN Beceri: Cast Skills in Order (Combo)", LocalizationManager.Get("UI_InOrder"), "Cast Skills in Order (Combo)");
        RunLocalizationString("EN Koruma: Heal Skill if HP < %", LocalizationManager.Get("UI_SkillHP"), "Heal Skill if HP < %");
        RunLocalizationString("EN Koruma: Auto Revive Pet (Grass of Life)", LocalizationManager.Get("UI_PetRevive"), "Auto Revive Pet (Grass of Life)");
        RunLocalizationString("EN Filtre: Only SoX", LocalizationManager.Get("UI_SoxPrint"), "Only SoX (SOS / SOM / SUN / Nova)");
        RunLocalizationString("EN Simya: ▶ Start Alchemy", LocalizationManager.Get("UI_Alchemy_Start"), "▶ Start Alchemy");
        RunLocalizationString("EN Simya: Use Lucky Powder (Auto-Match)", LocalizationManager.Get("UI_Alchemy_UsePowder"), "Use Lucky Powder (Auto-Match)");
        RunLocalizationString("EN TargetAssist: Include dead targets", LocalizationManager.Get("UI_TA_IncludeDead"), "Include dead targets");
        RunLocalizationString("EN Hesap Setup: Title", LocalizationManager.Get("UI_Acc_Title"), "Account Setup");
        RunLocalizationString("EN Hesap Setup: Secondary (PIN)", LocalizationManager.Get("UI_Acc_Secondary"), "Secondary (PIN)");

        // Silkroad Oyun Terimleri Koruma Testi (Hem TR hem EN'de Silkroad terimleri bozulmamalı)
        RunLocalizationCheck("TR dilinde SoX terimi korunur", LocalizationManager.Get("UI_SoxPrint").Contains("SoX"), true);
        RunLocalizationCheck("TR dilinde Grass of Life terimi korunur", LocalizationManager.Get("UI_PetRevive").Contains("Grass of Life"), true);
        RunLocalizationCheck("TR dilinde Lucky Powder terimi korunur", LocalizationManager.Get("UI_Alchemy_UsePowder").Contains("Lucky Powder"), true);
        RunLocalizationCheck("TR dilinde PIN terimi korunur", LocalizationManager.Get("UI_Acc_Secondary").Contains("PIN"), true);
        RunLocalizationCheck("TR dilinde Berserk terimi korunur", LocalizationManager.Get("UI_Berserk").Contains("Berserk"), true);
        RunLocalizationCheck("TR dilinde HP terimi korunur", LocalizationManager.Get("UI_ZerkHPFull").Contains("HP"), true);

        // Reset to TR default
        // Potion Cooldown & 0x185B Handling Tests
        RunPotionCheck("EU karakter ırkı tanınır (CHAR_EU_MAN_MERCHANT)", PotionPolicy.IsEuropean("CHAR_EU_MAN_MERCHANT"), true);
        RunPotionCheck("EU karakter Çin sayılmaz", PotionPolicy.IsChinese("CHAR_EU_MAN_MERCHANT"), false);
        RunPotionCheck("Çin karakter ırkı tanınır (CHAR_CH_MAN_WARRIOR)", PotionPolicy.IsChinese("CHAR_CH_MAN_WARRIOR"), true);
        RunPotionCheck("Çin karakter EU sayılmaz", PotionPolicy.IsEuropean("CHAR_CH_MAN_WARRIOR"), false);
        RunPotionInt("EU pot bekleme süresi 15000ms olmalı", PotionPolicy.GetPotionCooldownMs(true), 15000);
        RunPotionInt("Çin pot bekleme süresi 1000ms olmalı", PotionPolicy.GetPotionCooldownMs(false), 1000);
        RunPotionInt("Vigor pot bekleme süresi 15000ms olmalı", PotionPolicy.VigorPotionCooldownMs, 15000);
        RunPotionCheck("0x185B (Cooldown) slot bloklamaz", PotionPolicy.ShouldBlockSlot(0x185B), false);
        RunPotionInt("0x185B için slot bloklama süresi 0sn olmalı", PotionPolicy.GetSlotBlockSeconds(0x185B), 0);
        RunPotionInt("0x185B için blackout süresi 500ms olmalı", PotionPolicy.GetBlackoutMs(0x185B), 500);
        RunPotionCheck("0x1889 (Genel reject) slot bloklar", PotionPolicy.ShouldBlockSlot(0x1889), true);
        RunPotionInt("0x1889 için slot bloklama süresi 5sn olmalı (30sn değil)", PotionPolicy.GetSlotBlockSeconds(0x1889), 5);
        RunPotionInt("0x1889 için blackout süresi 2000ms olmalı (15sn değil)", PotionPolicy.GetBlackoutMs(0x1889), 2000);
        RunPotionCheck("ServerName MALL içeren pot mall sayılır", PotionPolicy.IsMallPotion("ITEM_MALL_HP_POTION_01", "HP Recovery potion (X-large)"), true);
        RunPotionCheck("X-large adlı pot mall sayılır", PotionPolicy.IsMallPotion("ITEM_ETC_MP_POTION_99", "MP Recovery potion (X-large)"), true);
        RunPotionCheck("Normal NPC potu mall sayılmaz", PotionPolicy.IsMallPotion("ITEM_ETC_HP_POTION_03", "HP Recovery Potion (Large)"), false);
        RunPotionInt("Normal pot mall pottan önce gelir", PotionPolicy.ComparePotions(false, 1, 100, true, 99, 200), -1);
        RunPotionInt("Güçlü pot (yüksek level) önce gelir", PotionPolicy.ComparePotions(false, 10, 100, false, 5, 200), -1);
        RunPotionInt("Eşit güçte büyük ID önce gelir", PotionPolicy.ComparePotions(false, 5, 200, false, 5, 100), -1);
        RunPotionInt("Aynı pot eşit sayılır", PotionPolicy.ComparePotions(true, 5, 100, true, 5, 100), 0);
        RunPotionInt("Öğrenilmiş usage her zaman kazanır", PotionPolicy.ResolveUsageWithFallback(0x08EC, 5, true, 0x08ED), 0x08ED);
        RunPotionInt("Tek sayıda reject bit0 alternatifini dener", PotionPolicy.ResolveUsageWithFallback(0x08EC, 1, false, 0), 0x08ED);
        RunPotionInt("Çift rejectte hesaplanan usage döner", PotionPolicy.ResolveUsageWithFallback(0x08EC, 2, false, 0), 0x08EC);
        RunPotionInt("Reject yoksa hesaplanan usage döner", PotionPolicy.ResolveUsageWithFallback(0x08EC, 0, false, 0), 0x08EC);

        // Return To Area (Alana Dönüş) Tests
        RunReturnCheck("Varsayılan: dönüşte binek kullanılır", ReturnToAreaPolicy.UseMount, true);
        RunReturnCheck("Varsayılan: dönüşte buff tazelenir", ReturnToAreaPolicy.CastBuffs, true);
        RunReturnCheck("Varsayılan: dönüşte hız eşyası kullanılır", ReturnToAreaPolicy.UseSpeedDrug, true);
        RunReturnCheck("Varsayılan: ters rota kapalı", ReturnToAreaPolicy.ReverseRoute, false);
        RunReturnCheck("Varsayılan: şehir döngüsü açık", ReturnToAreaPolicy.TownCycling, true);
        RunReturnJsonRoundtrip();

        // Teleport Link Blacklist Tests
        TeleportLinkPolicy.Reset();
        System.DateTime t0 = new System.DateTime(2026, 1, 1, 12, 0, 0);
        RunReturnCheck("1 hata kara listeye sokmaz", TeleportLinkPolicy.NoteFailure(2, 5, t0), false);
        RunReturnCheck("1 hatada link seçilebilir", TeleportLinkPolicy.IsBlacklisted(2, 5, t0), false);
        RunReturnCheck("2 hata kara listeye sokmaz", TeleportLinkPolicy.NoteFailure(2, 5, t0), false);
        RunReturnCheck("3. hata kara listeye sokar", TeleportLinkPolicy.NoteFailure(2, 5, t0), true);
        RunReturnCheck("Kara listedeki link seçilemez", TeleportLinkPolicy.IsBlacklisted(2, 5, t0), true);
        RunReturnCheck("10dk sonra kara liste dolar", TeleportLinkPolicy.IsBlacklisted(2, 5, t0.AddMinutes(11)), false);
        TeleportLinkPolicy.NoteFailure(2, 5, t0);
        TeleportLinkPolicy.NoteFailure(2, 5, t0);
        TeleportLinkPolicy.NoteFailure(2, 5, t0);
        TeleportLinkPolicy.NoteSuccess(2, 5);
        RunReturnCheck("Başarılı geçiş kara listeyi temizler", TeleportLinkPolicy.IsBlacklisted(2, 5, t0), false);
        TeleportLinkPolicy.Reset();

        // srodevs-docs (C:/srodevs-docs) paket uyum senaryoları
        RunSroCheck("TC flags 0x0F cumulated taşır", SroDocsPolicy.HasCumulated(0x0F), true);
        RunSroCheck("TC flags 0xF0 accumulated taşır", SroDocsPolicy.HasAccumulated(0xF0), true);
        RunSroCheck("TC flags 0x00 boş", SroDocsPolicy.HasCumulated(0x00) || SroDocsPolicy.HasAccumulated(0x00), false);
        RunSroCheck("BodyState 1 (Berserk) tanınır", SroDocsPolicy.IsKnownBodyState(1), true);
        RunSroCheck("BodyState 7 tanınır", SroDocsPolicy.IsKnownBodyState(7), true);
        RunSroCheck("BodyState 8 tanınmaz", SroDocsPolicy.IsKnownBodyState(8), false);
        RunSroCheck("InfoUpdate STP(3) tanınır", SroDocsPolicy.IsKnownInfoUpdateType(3), true);
        RunSroCheck("InfoUpdate EgyptAP(16) tanınır", SroDocsPolicy.IsKnownInfoUpdateType(16), true);
        RunSroCheck("InfoUpdate 5 tanınmaz", SroDocsPolicy.IsKnownInfoUpdateType(5), false);
        RunSroCheck("StateUpdate 0/1/4/7/8/11 tanınır", SroDocsPolicy.IsKnownStateUpdateKind(0) && SroDocsPolicy.IsKnownStateUpdateKind(1) && SroDocsPolicy.IsKnownStateUpdateKind(4) && SroDocsPolicy.IsKnownStateUpdateKind(7) && SroDocsPolicy.IsKnownStateUpdateKind(8) && SroDocsPolicy.IsKnownStateUpdateKind(11), true);
        RunSroCheck("StateUpdate 2 tanınmaz", SroDocsPolicy.IsKnownStateUpdateKind(2), false);
        RunSroString("Login 0x04 already-connected mesajı", SroDocsPolicy.GetLoginErrorMessage(4), "This user is already connected. Please try again in 5 minutes.");
        RunSroString("Login 0x06 server-full mesajı", SroDocsPolicy.GetLoginErrorMessage(6), "The server is full, please try again later.");
        RunSroString("Login 0x0B IP limit mesajı", SroDocsPolicy.GetLoginErrorMessage(0x0B), "IP limit exceeded.");
        RunSroString("Login block inspection mesajı", SroDocsPolicy.GetLoginErrorMessage(2, 2), "Cannot connect: server inspection (AccountInspection).");
        RunSroString("Logout 0x801 combat mesajı", SroDocsPolicy.GetLogoutErrorMessage(0x801), "Cannot close the game during combat.");
        RunSroString("Logout 0x802 teleport mesajı", SroDocsPolicy.GetLogoutErrorMessage(0x802), "Cannot exit the game while teleporting.");
        RunSroString("Chat 0x2008 invalid-command mesajı", SroDocsPolicy.GetChatErrorMessage(0x2008), "Invalid chat command.");
        RunSroString("Rename char 6 already-exists mesajı", SroDocsPolicy.GetRenameErrorMessage(1, 6), "This ID already exists.");
        RunSroString("Rename guild 7 cannot-create mesajı", SroDocsPolicy.GetRenameErrorMessage(2, 7), "The guild name cannot be created.");
        RunSroCheck("Açı 0 -> 0 derece", SroDocsPolicy.AngleToDegrees(0) == 0.0, true);
        RunSroCheck("Açı 32767 -> ~180 derece", System.Math.Abs(SroDocsPolicy.AngleToDegrees(32767) - 180.0) < 0.01, true);
        RunSroString("Auth 0x04 server-full mesajı", SroDocsPolicy.GetAuthErrorMessage(4), "The server is full, please try again later.");
        RunSroString("Auth 0x05 IP limit mesajı", SroDocsPolicy.GetAuthErrorMessage(5), "IP limit exceeded (insufficient IP).");
        RunSroString("Auth 0x01 C9 mesajı", SroDocsPolicy.GetAuthErrorMessage(1), "Failed to connect to the server (C9).");
        RunSroString("Patch 0x02 update mesajı", SroDocsPolicy.GetPatchErrorMessage(2), "Update required.");
        RunSroString("Patch 0x03 not-in-service mesajı", SroDocsPolicy.GetPatchErrorMessage(3), "Not in service.");
        RunSroCheck("Weather Rain(2) tanınır", SroDocsPolicy.IsKnownWeatherType(2), true);
        RunSroCheck("Weather 4 tanınmaz", SroDocsPolicy.IsKnownWeatherType(4), false);
        RunSroCheck("Petition GuildWar(10) tanınır", SroDocsPolicy.IsKnownPetitionType(10), true);
        RunSroCheck("Petition ResurrectionAgain(8) tanınır", SroDocsPolicy.IsKnownPetitionType(8), true);
        RunSroCheck("Petition 7 tanınmaz", SroDocsPolicy.IsKnownPetitionType(7), false);

        RunDictionaryRenameScenarios();
        RunSecretStoreScenarios();
        RunScriptCommandScenarios();

        if (failures != 0)
        {
            Console.WriteLine("Protection senaryoları başarısız: " + failures);
            return 1;
        }

        Console.WriteLine("Koruma, item filtre, combat, skill, script, imbue, command center, lojistik, parti, PIN, SOCKS5, Auto Alchemy, Target Assist, Localization, Potion, Alana Dönüş, Teleport, SroDocs, koleksiyon ve secret senaryoları başarılı.");
        return 0;
    }

    private static void RunScriptCommandScenarios()
    {
        ScriptCommandInvocation invocation;
        string error;
        RunScriptCheck("WALK phBot alias'ı MOVE olarak çözülür",
            ScriptCommandCatalog.TryParse("walk, 100, 200", out invocation, out error)
                && invocation.Command == "move" && invocation.Arguments.Length == 2, true);
        RunScriptCheck("Virgüllü CAST boşluklu skill adını korur",
            ScriptCommandCatalog.TryParse("cast, Moving March", out invocation, out error)
                && invocation.Arguments[0] == "Moving March", true);
        RunScriptCheck("TELEPORT iki isim parametresini korur",
            ScriptCommandCatalog.TryParse("teleport, Jangan South, Donwhang West", out invocation, out error)
                && invocation.Arguments.Length == 2 && invocation.Arguments[1] == "Donwhang West", true);
        RunScriptCheck("STOP parametresiz kabul edilir",
            ScriptCommandCatalog.TryParse("stop", out invocation, out error), true);
        RunScriptCheck("Eksik CAST parametresi reddedilir",
            ScriptCommandCatalog.TryParse("cast", out invocation, out error), false);
        RunScriptCheck("Bilinmeyen script komutu reddedilir",
            ScriptCommandCatalog.TryParse("dance, 1", out invocation, out error), false);
        RunScriptCheck("Script Creator standart virgüllü komut üretir",
            ScriptCommandCatalog.Format(ScriptCommandCatalog.Find("use"), "Return Scroll") == "use, Return Scroll", true);
        RunScriptCheck("phBot DoBlacksmith komutu parametresiz kabul edilir",
            ScriptCommandCatalog.TryParse("DoBlacksmith", out invocation, out error)
                && invocation.Command == "DoBlacksmith", true);
        string[] townCommands = { "DoHerbalist", "DoStable", "DoStorage", "DoStorageStore", "DoGuildStorage", "DoGuildStorageStore", "DoGroceryTrader", "DoProtectorTrader", "DoJupiter" };
        bool allTownCommandsKnown = true;
        for (int i = 0; i < townCommands.Length; i++)
            allTownCommandsKnown &= ScriptCommandCatalog.TryParse(townCommands[i], out invocation, out error);
        RunScriptCheck("phBot town komut ailesi kataloğa kayıtlıdır", allTownCommandsKnown, true);
    }

    private static void RunScriptCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }
        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunDictionaryRenameScenarios()
    {
        var values = new xDictionary<string, string>();
        values["a"] = "one";
        values["b"] = "two";

        RunDictionaryCheck("xDictionary mevcut hedefe rename'i reddeder", values.TrySetKey("a", "b"), false);
        RunDictionaryCheck("xDictionary reddedilen rename sonrası değerleri korur",
            values.Count == 2 && values["a"] == "one" && values["b"] == "two", true);
        RunDictionaryCheck("xDictionary boş hedefe rename yapar",
            values.TrySetKey("a", "c") && values.Count == 2 && !values.ContainsKey("a") && values["c"] == "one", true);
    }

    private static void RunDictionaryCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunSecretStoreScenarios()
    {
        const string secret = "xBot-test-secret";
        string protectedValue = SecretStore.Protect(secret);
        RunDictionaryCheck("SecretStore düz metni DPAPI çıktısında bırakmaz",
            protectedValue.StartsWith("enc:", StringComparison.Ordinal) && !protectedValue.Contains(secret), true);
        RunDictionaryCheck("SecretStore DPAPI roundtrip",
            SecretStore.Unprotect(protectedValue) == secret, true);
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

    private static void RunPickFilter(string name, ItemFilterInput input, ItemFilterRule rule, PickFilterOptions pick, bool expected)
    {
        bool actual = ItemFilterPolicy.ShouldPickup(input, FilterOptions(1, 15), rule, pick);
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunPickPet(string name, ItemFilterRule rule, PickFilterOptions pick, bool expected)
    {
        bool actual = ItemFilterPolicy.ShouldUsePet(new ItemFilterInput(), rule, pick, true, false);
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

    private static void RunSkillOrder(string name, bool[] ready, int startCursor, int expectedPicked, int expectedCursor)
    {
        int cursor = startCursor;
        int actual = SkillPolicy.SelectNextReadyIndex(ready.Length, idx => ready[idx], ref cursor);
        if (actual == expectedPicked && cursor == expectedCursor)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=(" + expectedPicked + "," + expectedCursor + "), gerçek=(" + actual + "," + cursor + ")");
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

    private static void RunReturnCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunSroCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunSroString(string name, string actual, string expected)
    {
        if (string.Equals(actual, expected, StringComparison.Ordinal))
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=\"" + expected + "\", gerçek=\"" + actual + "\"");
    }

    private static void RunReturnJsonRoundtrip()
    {
        string name = "Alana Dönüş ayarları JSON roundtrip korunur";
        try
        {
            bool m = ReturnToAreaPolicy.UseMount;
            bool b = ReturnToAreaPolicy.CastBuffs;
            bool s = ReturnToAreaPolicy.UseSpeedDrug;
            bool r = ReturnToAreaPolicy.ReverseRoute;
            bool t = ReturnToAreaPolicy.TownCycling;

            ReturnToAreaPolicy.UseMount = false;
            ReturnToAreaPolicy.CastBuffs = false;
            ReturnToAreaPolicy.UseSpeedDrug = false;
            ReturnToAreaPolicy.ReverseRoute = true;
            ReturnToAreaPolicy.TownCycling = false;
            string json = ReturnToAreaPolicy.ToJson().ToString();

            ReturnToAreaPolicy.UseMount = true;
            ReturnToAreaPolicy.CastBuffs = true;
            ReturnToAreaPolicy.UseSpeedDrug = true;
            ReturnToAreaPolicy.ReverseRoute = false;
            ReturnToAreaPolicy.TownCycling = true;
            ReturnToAreaPolicy.FromJson(Newtonsoft.Json.Linq.JObject.Parse(json));

            bool ok = !ReturnToAreaPolicy.UseMount
                && !ReturnToAreaPolicy.CastBuffs
                && !ReturnToAreaPolicy.UseSpeedDrug
                && ReturnToAreaPolicy.ReverseRoute
                && !ReturnToAreaPolicy.TownCycling;

            ReturnToAreaPolicy.UseMount = m;
            ReturnToAreaPolicy.CastBuffs = b;
            ReturnToAreaPolicy.UseSpeedDrug = s;
            ReturnToAreaPolicy.ReverseRoute = r;
            ReturnToAreaPolicy.TownCycling = t;

            if (ok)
            {
                Console.WriteLine("PASS: " + name);
                return;
            }

            failures++;
            Console.WriteLine("FAIL: " + name + " | JSON değerleri geri yüklenemedi");
        }
        catch (System.Exception ex)
        {
            failures++;
            Console.WriteLine("FAIL: " + name + " | hata: " + ex.Message);
        }
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

    private static void RunTargetAssistCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunLocalizationString(string name, string actual, string expected)
    {
        if (string.Equals(actual, expected, StringComparison.Ordinal))
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=\"" + expected + "\", gerçek=\"" + actual + "\"");
    }

    private static void RunLocalizationCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunPotionCheck(string name, bool actual, bool expected)
    {
        if (actual == expected)
        {
            Console.WriteLine("PASS: " + name);
            return;
        }

        failures++;
        Console.WriteLine("FAIL: " + name + " | beklenen=" + expected + ", gerçek=" + actual);
    }

    private static void RunPotionInt(string name, int actual, int expected)
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
