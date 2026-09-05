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

        if (failures != 0)
        {
            Console.WriteLine("Protection senaryoları başarısız: " + failures);
            return 1;
        }

        Console.WriteLine("Koruma, item filtre, combat, skill, imbue ve command center senaryoları başarılı: 62");
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
}
