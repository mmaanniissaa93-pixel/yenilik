using System;
using xBot.App;

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

        if (failures != 0)
        {
            Console.WriteLine("Protection senaryoları başarısız: " + failures);
            return 1;
        }

        Console.WriteLine("Protection senaryoları başarılı: 14");
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
}
