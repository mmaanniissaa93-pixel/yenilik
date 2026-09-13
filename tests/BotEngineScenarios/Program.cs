using System;
using xBot.App;

static class Program
{
    static void Check(bool ok, string label)
    {
        if (!ok) throw new Exception(label);
        Console.WriteLine("PASS: " + label);
    }

    static void Main()
    {
        long now = 0;
        var action = new CharacterActionTracker(() => now);
        bool attack;
        Check(action.TryBegin(1, 31141, 0, false, false, 500), "Startup buff owns character channel");
        Check(!action.TryBegin(1, 100, 22, false, true, 900), "Attack cannot overlap an unresolved buff");
        Check(action.Reject(0x10, out attack) && !attack, "Buff rejection cannot become an attack obstacle");
        now = 200;
        Check(action.TryBegin(1, 100, 22, false, true, 900), "Attack follows settled buff");
        Check(!action.Confirm(1, 31141, 0, false, out attack), "Late buff cannot acknowledge mob attack");
        Check(!action.Confirm(2, 100, 22, false, out attack), "Another player's damage cannot acknowledge attack");
        Check(!action.Confirm(1, 100, 23, false, out attack), "Previous target's reply cannot acknowledge current target");
        Check(action.Confirm(1, 100, 22, false, out attack) && attack, "Matching skill and target acknowledges attack");
        Check(!action.TryBegin(1, 101, 22, false, true, 900) && !action.TryReserve(), "Animation blocks next cast and character loot");
        now = 1099;
        Check(!action.CanStart, "Ready skill does not cut short current animation");
        now = 1100;
        Check(action.CanStart && action.TryReserve(), "Character loot can start after animation");
        Check(!action.TryBegin(1, 31141, 0, false, false, 500), "Background buff cannot interrupt character pickup");
        action.Release();
        Check(action.TryBegin(1, 1, 22, true, true, 700), "Basic attack shares the same action channel");
        Check(action.Confirm(1, 40, 22, true, out attack) && attack, "Server weapon animation acknowledges basic attack");
        now += 700;
        action.TryBegin(1, 100, 22, false, true, 900);
        now += 1500;
        Check(!action.CanStart && !action.Reject(0x10, out attack), "Timeout expires request and ignores late unidentified error");
        now += 200;
        Check(action.CanStart, "Lost response does not block the motor forever");

        action.Reset();
        Check(action.TryBegin(1, 1377, 0, false, false, 0), "Instant imbue can be requested immediately");
        Check(!action.ConfirmAppliedBuff(2, 1377) && !action.ConfirmAppliedBuff(1, 1378), "Unrelated buff does not unlock pending imbue");
        Check(action.ConfirmAppliedBuff(1, 1377) && action.CanStart, "Buff addition releases instant imbue without waiting for cast timeout");
        action.TryBegin(1, 100, 22, false, true, 900);
        Check(!action.ConfirmAppliedBuff(1, 100) && !action.CanStart, "Buff event cannot end an attack animation");
        var buffs = new BuffRetryTracker(() => now);
        Check(!buffs.TrySend(1377, () => false) && buffs.TrySend(1377, () => true), "Busy channel does not discard due imbue");
        Check(!buffs.TrySend(1377, () => true) && buffs.TrySend(31141, () => true), "Unconfirmed imbue is throttled independently from Devil");
        buffs.Applied(1377);
        Check(buffs.TrySend(1377, () => true), "Removed confirmed imbue can be renewed without artificial retry delay");
        buffs.Rejected(31141, 5000);
        now += 4999;
        Check(!buffs.CanTry(31141), "Rejected Devil respects configured retry interval");
        now++;
        Check(buffs.TrySend(31141, () => true), "Devil becomes eligible after rejection interval");
        buffs.Reset();
        Check(buffs.CanTry(31141), "Restart clears unconfirmed retry state");

        var pick = new PickFilterOptions { UsePickPet = true };
        var opts = new ItemFilterOptions();
        foreach (var item in new[] { new ItemFilterInput { IsGold = true }, new ItemFilterInput { IsElixirOrStone = true }, new ItemFilterInput { IsEquipable = true } })
            Check(ItemFilterPolicy.ResolveLootActor(item, opts, null, pick, true, false) == LootActor.None,
                "Empty filter plus pet checkbox never enables implicit character pickup");
        var input = new ItemFilterInput();
        var petRule = new ItemFilterRule { Pet = true };
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, true, false) == LootActor.Pet, "Pet-only rule belongs to pet");
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, false, false) == LootActor.None, "Absent pet does not silently activate character");
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, true, true) == LootActor.None, "Full pet does not silently activate character");
        pick.PickWithCharIfPetGoneFull = true;
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, false, false) == LootActor.Character, "Explicit fallback allows character");
        input.OwnerKind = 3;
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, false, false) == LootActor.None, "Fallback respects ownership restrictions");
        input.OwnerKind = 0;
        pick.Enabled = false;
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, true, false) == LootActor.None, "Disabled filter overrides saved pet rule");
        pick.Enabled = true;
        pick.DontPickItems = true;
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, false, false) == LootActor.None, "Do not pick overrides fallback");
        pick.DontPickItems = false;
        petRule.Pickup = true;
        Check(ItemFilterPolicy.ResolveLootActor(input, opts, petRule, pick, true, false) == LootActor.Pet, "Shared rule gives one actor ownership, preferring pet");
        EzFilterManager.SetRule(EzFilterCategory.Elixirs, false, true, false, false, false);
        Check(ItemFilterPolicy.ResolveLootActor(new ItemFilterInput { EzCategory = EzFilterCategory.Elixirs }, opts, null, pick, true, false) == LootActor.Pet,
            "Category pet rule uses same actor decision as exact item rule");

        var queue = new PickupQueue();
        Check(queue.TryBegin(8, 10, 0, 3000), "Pet sends first pickup");
        for (int t = 100; t < 3000; t += 100)
        {
            queue.Observe(true, true, t);
            if (queue.TryBegin(8, 11, t, 3000)) throw new Exception("Pet changed target before completion");
        }
        Check(queue.Target == 10, "Pet keeps target throughout travel instead of switching every 100 ms");
        queue.Observe(true, true, 3000);
        Check(queue.Target == 0 && !queue.TryBegin(8, 10, 3000, 3000), "Unreachable drop is deferred after bounded wait");
        Check(queue.TryBegin(8, 11, 3000, 3000), "Other loot remains available after failed drop");
        queue.Observe(false, true, 3100);
        Check(queue.Target == 0, "Despawn completes pickup immediately");
        queue.TryBegin(8, 12, 3100, 3000);
        queue.Observe(true, false, 3200);
        Check(queue.Target == 0, "Pet disappearance releases pending pickup");
        Check(queue.TryBegin(8, 10, 13000, 3000), "Deferred drop becomes eligible after cooldown");
        queue.Reset();
        Check(queue.Target == 0, "Stop/restart clears pickup ownership");
    }
}
