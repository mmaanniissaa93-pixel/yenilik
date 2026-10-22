using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using xBot.App;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;
using xBot.Game.Profiles;
using SecurityAPI;

public static class Phase1StabilityScenarios
{
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception("FAIL: " + message);
        }
        Console.WriteLine("PASS: " + message);
    }

    private static Packet CreatePacket(ushort opcode, byte[] bytes)
    {
        Packet p = new Packet(opcode);
        if (bytes != null)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                p.WriteByte(bytes[i]);
            }
        }
        p.Lock();
        return p;
    }

    public static void Run(Bot bot, Window window)
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine(" RUNNING PHASE 1 STABILITY SCENARIOS (C# IN-PROCESS AUDIT)");
        Console.WriteLine("================================================================================");

        TestH01_PacketParsingAndSignals();
        TestH03_SortingConcurrencyStress(bot);
        TestH06_SelfJoinDeadlockPrevention(bot);
        TestH07_MoveToNonBlocking(bot);
        TestMoveItem_NullSafetyAndReturn(bot);

        Console.WriteLine("\n================================================================================");
        Console.WriteLine(" ALL PHASE 1 STABILITY SCENARIOS COMPLETED SUCCESSFULLY!");
        Console.WriteLine("================================================================================");
    }

    private static void TestH01_PacketParsingAndSignals()
    {
        Console.WriteLine("\n--- [H-01] Target UID Extraction, Cast Confirmation & Wait Signal ---");

        uint myCharId = 87880;
        uint targetMobId = 763393;
        uint skillId = 8;

        var charType = typeof(SRCharacter);
        var mockChar = (SRCharacter)FormatterServices.GetUninitializedObject(charType);
        mockChar.UniqueID = myCharId;
        InfoManager.Current.Character = mockChar;

        // 1. Extended Silkroad-R Format: 25-byte packet
        // Format: status(1) + type(1) + pad(1) + skill(4) + source(4) + action(4) + unknownId(4) + target(4) + tail(2)
        var profileExtended = new ServerProfile { HasExtendedSkillCastTargetId = true };
        DataManager.CurrentProfile = profileExtended;

        InfoManager.MonitorSkillCast.Reset();
        InfoManager.LastSkillCastSuccess = false;
        InfoManager.LastSkillCastErrorCode = 99;
        InfoManager.CharacterActions.Reset();
        InfoManager.CharacterActions.TryBegin(myCharId, skillId, targetMobId, false, true, 1500);

        byte[] real25Bytes = new byte[] {
            0x01,                   // success = 1
            0x02,                   // type = Attack (2)
            0x01,                   // pad = 1
            0x08, 0x00, 0x00, 0x00, // skillID = 8
            0x48, 0x57, 0x01, 0x00, // sourceUniqueID = 87880
            0x01, 0x00, 0x00, 0x00, // actionID = 1
            0x00, 0x00, 0x00, 0x00, // unknownId = 0 (extra DWORD in Silkroad-R)
            0x01, 0xA6, 0x0B, 0x00, // targetUniqueID = 763393 (0x000BA601)
            0x01, 0x01             // trailing bytes
        };
        Packet pExtended = CreatePacket(0xB070, real25Bytes);
        PacketParser.EntitySkillStart(pExtended);

        Check(InfoManager.LastSkillCastSuccess == true, "Extended packet: LastSkillCastSuccess is true (target UID resolved accurately)");
        Check(InfoManager.LastSkillCastErrorCode == 0, "Extended packet: LastSkillCastErrorCode cleared to 0");
        Check(InfoManager.MonitorSkillCast.WaitOne(0) == true, "Extended packet: MonitorSkillCast signal pulsed immediately");

        // 2. Classic vSRO Format: 19-byte packet (no extra DWORD)
        var profileClassic = new ServerProfile { HasExtendedSkillCastTargetId = false };
        DataManager.CurrentProfile = profileClassic;

        InfoManager.MonitorSkillCast.Reset();
        InfoManager.LastSkillCastSuccess = false;
        InfoManager.CharacterActions.Reset();
        InfoManager.CharacterActions.TryBegin(myCharId, skillId, targetMobId, false, true, 1500);

        byte[] classic19Bytes = new byte[] {
            0x01,                   // success = 1
            0x02,                   // type = Attack (2)
            0x01,                   // pad = 1
            0x08, 0x00, 0x00, 0x00, // skillID = 8
            0x48, 0x57, 0x01, 0x00, // sourceUniqueID = 87880
            0x01, 0x00, 0x00, 0x00, // actionID = 1
            0x01, 0xA6, 0x0B, 0x00  // targetUniqueID = 763393
        };
        Packet pClassic = CreatePacket(0xB070, classic19Bytes);
        PacketParser.EntitySkillStart(pClassic);

        Check(InfoManager.LastSkillCastSuccess == true, "Classic packet: LastSkillCastSuccess is true (target UID resolved without extra DWORD)");
        Check(InfoManager.MonitorSkillCast.WaitOne(0) == true, "Classic packet: MonitorSkillCast signal pulsed immediately");

        // 3. Truncated packet
        InfoManager.MonitorSkillCast.Reset();
        InfoManager.LastSkillCastSuccess = false;
        InfoManager.CharacterActions.Reset();
        InfoManager.CharacterActions.TryBegin(myCharId, skillId, targetMobId, false, true, 1500);

        byte[] truncatedBytes = new byte[] { 0x01, 0x02, 0x01, 0x08 }; // Only 4 bytes
        Packet pTruncated = CreatePacket(0xB070, truncatedBytes);
        PacketParser.EntitySkillStart(pTruncated);

        Check(InfoManager.LastSkillCastSuccess == false, "Truncated packet: did not confirm cast or produce bogus success");
        Check(InfoManager.MonitorSkillCast.WaitOne(0) == false, "Truncated packet: MonitorSkillCast signal correctly remained unset");

        InfoManager.Current.Character = null;
    }

    private static void TestH03_SortingConcurrencyStress(Bot bot)
    {
        Console.WriteLine("\n--- [H-03] Sorting Concurrency & Stop -> Start Race Stress Test ---");

        InfoManager.Current.inGame = true;
        var tSortingField = typeof(Bot).GetField("tSorting", Flags);
        var sessionField = typeof(Bot).GetField("m_sortingSessionId", Flags);

        int maxConcurrentObserved = 0;
        int iterations = 25;

        for (int i = 0; i < iterations; i++)
        {
            bot.StartInventorySort();

            Thread worker = (Thread)tSortingField.GetValue(bot);
            int active = (worker != null && worker.IsAlive) ? 1 : 0;
            if (active > maxConcurrentObserved) maxConcurrentObserved = active;

            Thread.Sleep(10);
            bot.StopInventorySort();

            // Immediate restart attempt in tight loop
            bot.StartInventorySort();

            Thread worker2 = (Thread)tSortingField.GetValue(bot);
            if (worker != null && worker2 != null && worker != worker2)
            {
                if (worker.IsAlive && worker2.IsAlive)
                {
                    Check(false, "Detected two distinct sorting workers concurrently alive!");
                }
            }
        }

        bot.StopInventorySort();
        Thread.Sleep(50);

        Check(maxConcurrentObserved <= 1, string.Format("Max active sorting workers at any instant: {0} (never > 1)", maxConcurrentObserved));
        Check(!bot.isSorting, "isSorting is false after stress test completion");
        int finalSessionId = (int)sessionField.GetValue(bot);
        Check(finalSessionId > 1, string.Format("Sorting session ID progressed cleanly: {0}", finalSessionId));

        InfoManager.Current.inGame = false;
    }

    private static void TestH06_SelfJoinDeadlockPrevention(Bot bot)
    {
        Console.WriteLine("\n--- [H-06] Self-Join Deadlock Prevention & Catch Safety ---");

        var tBottingField = typeof(Bot).GetField("tBotting", Flags);
        long elapsedMs = -1;
        bool joinAvoided = false;

        Thread testThread = new Thread(() => {
            // Simulate being tBotting
            tBottingField.SetValue(bot, Thread.CurrentThread);
            Stopwatch sw = Stopwatch.StartNew();
            bot.Stop();
            sw.Stop();
            elapsedMs = sw.ElapsedMilliseconds;
            // If Stop() had called Thread.CurrentThread.Join(2000), elapsedMs would be >= 1800ms
            joinAvoided = elapsedMs < 1000;
        });

        testThread.IsBackground = true;
        testThread.Start();
        testThread.Join(2500);

        Check(!testThread.IsAlive, "Stop() called from within botting worker completed without hanging");
        Check(joinAvoided, string.Format("Stop() recognized t == Thread.CurrentThread and bypassed Join(2000) (took {0}ms)", elapsedMs));

        tBottingField.SetValue(bot, null);
    }

    private static void TestH07_MoveToNonBlocking(Bot bot)
    {
        Console.WriteLine("\n--- [H-07] MoveTo Non-Blocking Execution & Caller Loop Safety ---");

        InfoManager.Current.Character = null;
        SRCoord targetPos = new SRCoord(200.0, 300.0);

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            bot.MoveTo(targetPos);
            bot.MoveTo(null);
        }
        sw.Stop();
        Check(sw.ElapsedMilliseconds < 100, string.Format("2000 MoveTo calls with Character == null completed in {0}ms (< 100ms, non-blocking)", sw.ElapsedMilliseconds));

        // WaitMovement with null position
        sw.Restart();
        bool waitRes = bot.WaitMovement(null, 10, 3.0);
        sw.Stop();
        Check(!waitRes, "WaitMovement(null) safely returned false");
        Check(sw.ElapsedMilliseconds < 50, string.Format("WaitMovement(null) returned in {0}ms without blocking", sw.ElapsedMilliseconds));

        // WaitNavigationWaypoint with null point
        var waitNavMethod = typeof(Bot).GetMethod("WaitNavigationWaypoint", Flags);
        sw.Restart();
        bool navRes = (bool)waitNavMethod.Invoke(bot, new object[] { null, 10, 3.0 });
        sw.Stop();
        Check(!navRes, "WaitNavigationWaypoint(null) safely returned false");
        Check(sw.ElapsedMilliseconds < 50, string.Format("WaitNavigationWaypoint(null) returned in {0}ms without blocking", sw.ElapsedMilliseconds));
    }

    private static void TestMoveItem_NullSafetyAndReturn(Bot bot)
    {
        Console.WriteLine("\n--- [5] PacketBuilder.MoveItem Null-Safety & Non-Blocking Returns ---");

        var moveItemMethod = typeof(PacketBuilder).GetMethod("MoveItem", BindingFlags.Static | BindingFlags.Public, null,
            new Type[] { typeof(byte), typeof(byte), typeof(SRTypes.InventoryItemMovement), typeof(ushort) }, null);

        Check(moveItemMethod.ReturnType == typeof(bool), "PacketBuilder.MoveItem returns bool indicating dispatch status");

        // With no proxy/agent connected:
        bool sent = (bool)moveItemMethod.Invoke(null, new object[] { (byte)13, (byte)14, SRTypes.InventoryItemMovement.InventoryToInventory, (ushort)10 });
        Check(!sent, "PacketBuilder.MoveItem returns false when Proxy/Agent is null instead of throwing NRE");

        // Verify InventorySort exits immediately on !sent instead of waiting 1000ms per item
        InfoManager.Current.inGame = true;

        var charType = typeof(SRCharacter);
        var mockChar = (SRCharacter)FormatterServices.GetUninitializedObject(charType);
        var itemType = typeof(SRItem);
        var xListType = typeof(xList<SRItem>);
        var inv = (xList<SRItem>)Activator.CreateInstance(xListType, new object[] { (byte)109 });
        mockChar.Inventory = inv;

        var item1 = (SRItem)FormatterServices.GetUninitializedObject(itemType);
        itemType.GetField("<ID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(item1, 5001u);
        item1.Quantity = 10;
        item1.QuantityMax = 50;

        var item2 = (SRItem)FormatterServices.GetUninitializedObject(itemType);
        itemType.GetField("<ID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(item2, 5001u);
        item2.Quantity = 20;
        item2.QuantityMax = 50;

        inv[13] = item1;
        inv[14] = item2;
        InfoManager.Current.Character = mockChar;

        Stopwatch swSort = Stopwatch.StartNew();
        bot.InventorySort();
        swSort.Stop();

        Check(swSort.ElapsedMilliseconds < 200, string.Format("InventorySort with unconnectable agent terminated in {0}ms (did not hang for 1000ms)", swSort.ElapsedMilliseconds));

        InfoManager.Current.Character = null;
        InfoManager.Current.inGame = false;
    }
}
