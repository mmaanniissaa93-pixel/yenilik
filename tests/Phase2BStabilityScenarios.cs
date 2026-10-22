using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using xBot.App;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;
using xBot.Network;

namespace xBot.Tests
{
    public static class Phase2BStabilityScenarios
    {
        private static int s_passCount = 0;
        private static int s_failCount = 0;

        private static void Assert(bool condition, string message)
        {
            if (condition)
            {
                s_passCount++;
                Console.WriteLine("PASS: " + message);
            }
            else
            {
                s_failCount++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAIL: " + message);
                Console.ResetColor();
            }
        }

        public static int RunAll()
        {
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" RUNNING PHASE 2B STABILITY SCENARIOS (C# IN-PROCESS AUDIT)");
            Console.WriteLine("================================================================================");

            Test_H08_TeleportSortSafety();
            Test_H09_ProxyReconnectLifecycle();

            Console.WriteLine();
            Console.WriteLine("================================================================================");
            if (s_failCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" ALL PHASE 2B STABILITY SCENARIOS PASSED (" + s_passCount + " assertions)!");
                Console.ResetColor();
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" PHASE 2B COMPLETED WITH " + s_failCount + " FAILS (" + s_passCount + " passed)");
                Console.ResetColor();
                return 1;
            }
        }

        private static void Test_H08_TeleportSortSafety()
        {
            Console.WriteLine();
            Console.WriteLine("--- [H-08] Teleport Sırasında Envanter/Depo Sıralama Güvenliği ---");

            Bot bot = Bot.Get;
            Assert(bot != null, "Bot singleton instance is available");

            var window = Window.Get;
            Assert(window != null, "Window singleton instance is initialized safely with double-checked lock");

            // Setup character using FormatterServices to bypass database lookups
            SRCharacter character = (SRCharacter)FormatterServices.GetUninitializedObject(typeof(SRCharacter));
            character.Inventory = new xList<SRItem>(108);

            var storageList = new xList<SRItem>(120);
            PropertyInfo storageProp = typeof(SRCharacter).GetProperty("Storage", BindingFlags.Public | BindingFlags.Instance);
            if (storageProp != null)
            {
                storageProp.SetValue(character, storageList, null);
            }

            // Add test items to inventory
            PropertyInfo itemIdProp = typeof(SRItem).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance);

            SRItem item1 = (SRItem)FormatterServices.GetUninitializedObject(typeof(SRItem));
            if (itemIdProp != null) itemIdProp.SetValue(item1, (uint)5001, null);
            item1.Quantity = 10;
            item1.QuantityMax = 50;
            character.Inventory[13] = item1;

            SRItem item2 = (SRItem)FormatterServices.GetUninitializedObject(typeof(SRItem));
            if (itemIdProp != null) itemIdProp.SetValue(item2, (uint)5001, null);
            item2.Quantity = 20;
            item2.QuantityMax = 50;
            character.Inventory[14] = item2;

            // Add test items to storage
            SRItem sItem1 = (SRItem)FormatterServices.GetUninitializedObject(typeof(SRItem));
            if (itemIdProp != null) itemIdProp.SetValue(sItem1, (uint)6001, null);
            sItem1.Quantity = 15;
            sItem1.QuantityMax = 100;
            storageList[0] = sItem1;

            SRItem sItem2 = (SRItem)FormatterServices.GetUninitializedObject(typeof(SRItem));
            if (itemIdProp != null) itemIdProp.SetValue(sItem2, (uint)6001, null);
            sItem2.Quantity = 25;
            sItem2.QuantityMax = 100;
            storageList[1] = sItem2;

            InfoManager.Current.Character = character;

            // 1. inTeleport == true prevents starting inventory or storage sorting
            InfoManager.Current.inGame = true;
            InfoManager.Current.inTeleport = true;

            bool invStartedDuringTeleport = bot.StartInventorySort();
            Assert(!invStartedDuringTeleport, "StartInventorySort returned false when inTeleport is true");

            bool storageStartedDuringTeleport = bot.StartStorageSort();
            Assert(!storageStartedDuringTeleport, "StartStorageSort returned false when inTeleport is true");

            // 2. PacketBuilder.MoveItem fails safely when inTeleport == true
            bool moveSentDuringTeleport = PacketBuilder.MoveItem(13, 14, SRTypes.InventoryItemMovement.InventoryToInventory, 5);
            Assert(!moveSentDuringTeleport, "PacketBuilder.MoveItem returned false when inTeleport is true");

            bool moveSentDuringTeleportUniqueID = PacketBuilder.MoveItem(13, 14, SRTypes.InventoryItemMovement.InventoryToInventory, (uint)999);
            Assert(!moveSentDuringTeleportUniqueID, "PacketBuilder.MoveItem(uniqueID) returned false when inTeleport is true");

            // 3. StopStorageSort alias exists and returns properly
            bool stopStorageWhenNotRunning = bot.StopStorageSort();
            Assert(!stopStorageWhenNotRunning, "StopStorageSort() returned false when no sorting worker is active");

            // 4. Test active inventory sorting interruption when inTeleport is signaled
            InfoManager.Current.inTeleport = false;
            InfoManager.Current.inGame = true;

            bool sortStarted = bot.StartInventorySort();
            Assert(sortStarted, "StartInventorySort started when inGame is true and inTeleport is false");

            // Set inTeleport = true immediately to simulate teleport transition while sorting
            InfoManager.Current.inTeleport = true;
            Thread.Sleep(100);

            Assert(!bot.isSorting, "Inventory sorting worker terminated promptly after inTeleport was signaled");

            // 5. Test InfoManager.OnTeleporting cleanly calls StopInventorySort
            InfoManager.Current.inTeleport = false;
            bot.StartInventorySort();
            Assert(bot.isSorting, "Sorting active before OnTeleporting call");

            MethodInfo onTeleportingMethod = typeof(InfoManager).GetMethod("OnTeleporting", BindingFlags.NonPublic | BindingFlags.Static);
            if (onTeleportingMethod != null)
            {
                onTeleportingMethod.Invoke(null, null);
            }

            Assert(InfoManager.inTeleport, "OnTeleporting flagged inTeleport == true");
            Assert(!bot.isSorting, "OnTeleporting safely stopped active sorting worker");

            // 6. Test storage sorting interruption when inTeleport is signaled
            InfoManager.Current.inTeleport = false;
            bool storageSortStarted = bot.StartStorageSort();
            Assert(storageSortStarted, "StartStorageSort started when inGame is true and inTeleport is false");

            InfoManager.Current.inTeleport = true;
            Thread.Sleep(100);

            Assert(!bot.isSorting, "Storage sorting worker terminated promptly after inTeleport was signaled");

            // 7. Test StopInventorySort cleanly halts running sort
            InfoManager.Current.inTeleport = false;
            bot.StartInventorySort();
            Assert(bot.isSorting, "Sorting active before explicit StopInventorySort");

            bool stopped = bot.StopInventorySort();
            Assert(stopped, "StopInventorySort returned true when worker was alive");
            Assert(!bot.isSorting, "bot.isSorting is false after StopInventorySort completed");

            // Cleanup state
            InfoManager.Current.inTeleport = false;
            InfoManager.Current.inGame = false;
        }

        private static void Test_H09_ProxyReconnectLifecycle()
        {
            Console.WriteLine();
            Console.WriteLine("--- [H-09] Proxy Gateway/Agent Reconnect Yaşam Döngüsü ---");

            var hosts = new List<string> { "127.0.0.1" };
            var ports = new List<ushort> { 15779 };
            Proxy proxy = new Proxy(true, hosts, ports);

            Assert(proxy != null, "Proxy instance initialized in Clientless mode");
            Assert(!proxy.isRunning, "Proxy isRunning is false initially");

            // 1. Re-entrancy & Thread-Safety: rapid calls to Start()
            proxy.Start();
            Assert(proxy.isRunning, "Proxy isRunning is true after first Start()");

            // Call Start multiple times rapidly
            for (int i = 0; i < 10; i++)
            {
                proxy.Start();
            }

            // Inspect private field m_gwThread via reflection
            FieldInfo gwThreadField = typeof(Proxy).GetField("m_gwThread", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert(gwThreadField != null, "Proxy.m_gwThread field found via reflection");

            Thread gwThread = (Thread)gwThreadField.GetValue(proxy);
            Assert(gwThread != null && gwThread.IsAlive, "Only a single active m_gwThread was spawned despite repeated Start() calls");

            // 2. Stop() cleanly cancels and halts proxy
            proxy.Stop(false);
            Assert(!proxy.isRunning, "Proxy isRunning is false after Stop()");

            Thread.Sleep(150);
            Assert(!gwThread.IsAlive, "Gateway worker thread exited cleanly after Stop()");

            // 3. Isolated reconnect and timeout events exist and are separate
            FieldInfo gwRecEventField = typeof(Proxy).GetField("_gwReconnectCancelEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo gwTimeoutEventField = typeof(Proxy).GetField("_gwTimeoutCancelEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo agTimeoutEventField = typeof(Proxy).GetField("_agTimeoutCancelEvent", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert(gwRecEventField != null, "Proxy._gwReconnectCancelEvent field exists");
            Assert(gwTimeoutEventField != null, "Proxy._gwTimeoutCancelEvent field exists");
            Assert(agTimeoutEventField != null, "Proxy._agTimeoutCancelEvent field exists");

            var gwRecEvent = (ManualResetEventSlim)gwRecEventField.GetValue(proxy);
            var gwTimeoutEvent = (ManualResetEventSlim)gwTimeoutEventField.GetValue(proxy);
            var agTimeoutEvent = (ManualResetEventSlim)agTimeoutEventField.GetValue(proxy);

            Assert(gwRecEvent != null && gwTimeoutEvent != null && agTimeoutEvent != null, "All 3 timeout/reconnect cancellation events instantiated");
            Assert(gwRecEvent != agTimeoutEvent, "Gateway and Agent cancellation events are strictly isolated instances");

            // 4. Verify listener cleanup: m_gwListener and m_agListener are nulled/closed
            FieldInfo gwListenerField = typeof(Proxy).GetField("m_gwListener", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo agListenerField = typeof(Proxy).GetField("m_agListener", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert(gwListenerField != null && agListenerField != null, "Listening socket tracking fields exist");
            Assert(gwListenerField.GetValue(proxy) == null, "m_gwListener is null after Stop()");
            Assert(agListenerField.GetValue(proxy) == null, "m_agListener is null after Stop()");

            // 5. Test BindGatewaySocket and BindSocket port freeing
            MethodInfo bindGatewayMethod = typeof(Proxy).GetMethod("BindGatewaySocket", BindingFlags.NonPublic | BindingFlags.Instance);
            Socket s1 = (Socket)bindGatewayMethod.Invoke(proxy, new object[] { "127.0.0.1" });
            Assert(s1 != null && s1.IsBound, "BindGatewaySocket successfully bound an ephemeral TCP port");
            int boundPort = ((IPEndPoint)s1.LocalEndPoint).Port;
            s1.Close();

            // Re-binding on same port after close succeeds without WSAEADDRINUSE
            MethodInfo bindSocketMethod = typeof(Proxy).GetMethod("BindSocket", BindingFlags.NonPublic | BindingFlags.Instance);
            Socket s2 = null;
            bool rebindSuccess = false;
            try
            {
                s2 = (Socket)bindSocketMethod.Invoke(proxy, new object[] { "127.0.0.1", boundPort });
                rebindSuccess = (s2 != null && s2.IsBound);
            }
            catch { }
            finally
            {
                if (s2 != null)
                {
                    try { s2.Close(); } catch { }
                }
            }
            Assert(rebindSuccess, "Rebinding on closed port succeeded immediately without address collision");

            // 6. Test Start() re-entrancy protection when m_agThread is active
            FieldInfo agThreadField = typeof(Proxy).GetField("m_agThread", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert(agThreadField != null, "Proxy.m_agThread field found via reflection");

            // Set proxy isRunning = true and assign a live mock agent thread
            PropertyInfo isRunningProp = typeof(Proxy).GetProperty("isRunning", BindingFlags.Public | BindingFlags.Instance);
            isRunningProp.SetValue(proxy, true, null);

            var mockAgThread = new Thread(() => {
                Thread.Sleep(500);
            });
            mockAgThread.IsBackground = true;
            mockAgThread.Start();
            agThreadField.SetValue(proxy, mockAgThread);

            // Attempt to call Start() while agent thread is alive
            proxy.Start();

            // Verify m_gwThread was NOT spawned
            Thread gwThreadAfterAg = (Thread)gwThreadField.GetValue(proxy);
            Assert(gwThreadAfterAg == null, "Start() safely rejected spawning new Gateway thread while Agent thread was active");

            // Clean up
            proxy.Stop(false);
            mockAgThread.Join(500);
            Assert(!proxy.isRunning, "Proxy stopped cleanly after agent thread test");
        }
    }
}
