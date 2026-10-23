using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using xBot.App;
using xBot.Game;
using xBot.Game.Navigation;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

namespace xBot.Tests
{
    public static class NavigationAndItemAuditScenarios
    {
        private static int passed = 0;
        private static int failed = 0;

        private static void Assert(bool condition, string message)
        {
            if (condition)
            {
                Console.WriteLine("PASS: " + message);
                passed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FAIL: " + message);
                Console.ResetColor();
                failed++;
            }
        }

        public static int RunAll()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine(" RUNNING NAVIGATION, TRAINING AREA & ITEM USE REJECTION AUDIT SCENARIOS");
            Console.WriteLine("================================================================================");

            TestItemUseRejectionBehavior();
            TestTrainingAreaAutoActivation();
            TestMapNavigationRoutes();
            TestConfiguredTrainingRoute();

            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" AUDIT RESULTS: " + passed + " PASSED, " + failed + " FAILED");
            Console.WriteLine("================================================================================");

            return failed;
        }

        private static void TestItemUseRejectionBehavior()
        {
            Console.WriteLine("\n--- [1] Item Use Rejection & 0xB04C / 0x0002 Cooldown Handling ---");

            var bot = Bot.Get;
            var useItemLockField = typeof(Bot).GetField("m_useItemLock", BindingFlags.NonPublic | BindingFlags.Static);
            var rejectedSlotsField = typeof(Bot).GetField("m_rejectedUseSlots", BindingFlags.NonPublic | BindingFlags.Static);
            var lastUseSlotField = typeof(Bot).GetField("m_lastUseItemSlot", BindingFlags.NonPublic | BindingFlags.Static);
            var lastUseIdField = typeof(Bot).GetField("m_lastUseItemId", BindingFlags.NonPublic | BindingFlags.Static);
            var lastUseUtcField = typeof(Bot).GetField("m_lastUseItemUtc", BindingFlags.NonPublic | BindingFlags.Static);
            var rejectBlackoutField = typeof(Bot).GetField("m_rejectBlackoutUntil", BindingFlags.NonPublic | BindingFlags.Static);

            var useLock = useItemLockField.GetValue(null);
            var rejectedSlots = (Dictionary<byte, DateTime>)rejectedSlotsField.GetValue(null);

            var charType = typeof(SRCharacter);
            var mockChar = (SRCharacter)FormatterServices.GetUninitializedObject(charType);
            mockChar.Inventory = new xList<SRItem>(109);
            InfoManager.Current.Character = mockChar;

            var itemType = typeof(SRItem);

            // Test Case 1: Potion usage rejected with 0x0002 (Server Cooldown / Action Busy)
            lock (useLock)
            {
                rejectedSlots.Clear();
                lastUseSlotField.SetValue(null, (byte)15);
                lastUseIdField.SetValue(null, (uint)1001);
                lastUseUtcField.SetValue(null, DateTime.UtcNow);
            }

            var potionItem = (SRItem)FormatterServices.GetUninitializedObject(itemType);
            itemType.GetField("<ID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(potionItem, 1001u);
            itemType.GetField("<ID2>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(potionItem, (byte)3);
            itemType.GetField("<ID3>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(potionItem, (byte)1);
            mockChar.Inventory[15] = potionItem;

            bot.MarkLastUseRejected(0x0002);

            bool slotBlocked;
            DateTime blackout;
            lock (useLock)
            {
                slotBlocked = rejectedSlots.ContainsKey(15) && rejectedSlots[15] > DateTime.UtcNow;
                blackout = (DateTime)rejectBlackoutField.GetValue(null);
            }

            Assert(!slotBlocked, "Error 0x0002 on potion MUST NOT block slot in m_rejectedUseSlots (no 5s lockout)");
            double blackoutMs = (blackout - DateTime.UtcNow).TotalMilliseconds;
            Assert(blackoutMs <= 500, "Blackout duration for 0x0002 is brief debounce (<= 500ms, actual: " + blackoutMs.ToString("F0") + "ms)");

            // Test Case 2: Potion rejected with 0x185B (Cooldown Active)
            lock (useLock)
            {
                rejectedSlots.Clear();
                lastUseSlotField.SetValue(null, (byte)16);
                lastUseIdField.SetValue(null, (uint)1002);
                lastUseUtcField.SetValue(null, DateTime.UtcNow);
            }
            var mpPotion = (SRItem)FormatterServices.GetUninitializedObject(itemType);
            itemType.GetField("<ID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mpPotion, 1002u);
            itemType.GetField("<ID2>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mpPotion, (byte)3);
            itemType.GetField("<ID3>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mpPotion, (byte)2);
            mockChar.Inventory[16] = mpPotion;

            bot.MarkLastUseRejected(0x185B);

            lock (useLock)
            {
                slotBlocked = rejectedSlots.ContainsKey(16) && rejectedSlots[16] > DateTime.UtcNow;
            }
            Assert(!slotBlocked, "Error 0x185B on potion MUST NOT block slot in m_rejectedUseSlots");

            // Test Case 3: Non-potion item rejection with generic error (0x1234)
            lock (useLock)
            {
                rejectedSlots.Clear();
                lastUseSlotField.SetValue(null, (byte)20);
                lastUseIdField.SetValue(null, (uint)9999);
                lastUseUtcField.SetValue(null, DateTime.UtcNow);
            }
            var nonPotionItem = (SRItem)FormatterServices.GetUninitializedObject(itemType);
            itemType.GetField("<ID>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(nonPotionItem, 9999u);
            itemType.GetField("<ID2>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(nonPotionItem, (byte)1);
            itemType.GetField("<ID3>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(nonPotionItem, (byte)1);
            mockChar.Inventory[20] = nonPotionItem;

            bot.MarkLastUseRejected(0x1234);

            lock (useLock)
            {
                slotBlocked = rejectedSlots.ContainsKey(20) && rejectedSlots[20] > DateTime.UtcNow;
            }
            Assert(slotBlocked, "Generic non-potion item rejection correctly blocks slot temporarily to prevent spam");
        }

        private static void TestTrainingAreaAutoActivation()
        {
            Console.WriteLine("\n--- [2] Training Area Auto-Activation & Fallback ---");

            var w = Window.Get;
            if (w == null)
            {
                Console.WriteLine("Window instance not initialized, skipping UI test.");
                return;
            }

            w.InvokeIfRequired(() =>
            {
                w.Training_lstvAreas.Items.Clear();
                w.Training_lstvAreas.Tag = null;
                w.Training_lstvAreas.SelectedItems.Clear();

                // Case 1: Empty list -> returns null
                var posEmpty = w.TrainingArea_GetPosition();
                Assert(posEmpty == null, "Empty training area list returns null");

                // Case 2: Area configured, but neither Tag nor SelectedItems set (e.g. on bot start or headless load)
                var info = new TrainingAreaInfo("Jangan Mangyang", 25000, 1100, 1200, 0, 75, 45, "", "Menzil");
                var it = new ListViewItem(info.Name);
                it.Name = info.Name;
                it.Tag = info;
                it.SubItems.Add("");
                it.SubItems.Add(info.Radius.ToString());
                it.SubItems.Add(info.PickRadius.ToString());
                it.SubItems.Add(info.Type);
                w.Training_lstvAreas.Items.Add(it);

                // Neither Tag nor SelectedItems is set
                w.Training_lstvAreas.Tag = null;
                w.Training_lstvAreas.SelectedItems.Clear();

                var autoPos = w.TrainingArea_GetPosition();
                Assert(autoPos != null, "TrainingArea_GetPosition automatically activates first area when none explicitly selected");
                Assert(autoPos != null && autoPos.Region == 25000 && autoPos.X == 1100, "Auto-activated area coordinate matches configured area (Region 25000, X 1100)");
                Assert(w.TrainingArea_GetRadius() == 75, "Auto-activated area radius matches configured (75m)");
                Assert(w.TrainingArea_GetPickRadius() == 45, "Auto-activated area pick radius matches configured (45m)");
                Assert(w.Training_lstvAreas.Tag == it, "Training_lstvAreas.Tag is set to the activated item");
            });
        }

        private static void TestMapNavigationRoutes()
        {
            Console.WriteLine("\n--- [3] Comprehensive Map Navigation & Cross-City Compound Routes ---");

            var nav = NavigationManager.Get;
            Assert(nav.IsAvailable, "NavigationManager is initialized and available");

            // Key City & Field Coordinates
            var janganTown = new SRCoord(25000, 969, 0, 1369);
            var janganField = new SRCoord(24999, 1000, 0, 1000);
            var donwhangTown = new SRCoord(26265, 957, -80, 1508);
            var donwhangField = new SRCoord(26521, 1000, -50, 1200);
            var hotanTown = new SRCoord(23687, 1138, 250, 153);
            var samarkandTown = new SRCoord(27244, 270, 180, 1421);
            var constantinopleTown = new SRCoord(26959, 950, 84, 1070);
            var alexandriaSouth = new SRCoord(23088, 612, 910, 1089);

            // Test 1: Intra-Region & Multi-Region Walk (Jangan Town -> Jangan Field)
            var routeJanganField = nav.FindCompoundRoute(janganTown, janganField);
            Assert(routeJanganField != null && !routeJanganField.IsEmpty, "Jangan Town -> Jangan Field: Compound route found");
            Assert(routeJanganField != null && routeJanganField.Segments.Count >= 1 && routeJanganField.Segments[0].Type == RouteSegmentType.Walk, "Jangan Town -> Jangan Field: Walk segment with waypoints");

            // Test 2: Jangan -> Donwhang (Cross-Town via Dimensional Gate)
            var routeJanganDonwhang = nav.FindCompoundRoute(janganTown, donwhangTown);
            Assert(routeJanganDonwhang != null && !routeJanganDonwhang.IsEmpty, "Jangan -> Donwhang: Cross-town gate route found");
            bool hasJanganDonwhangGate = false;
            if (routeJanganDonwhang != null)
            {
                foreach (var s in routeJanganDonwhang.Segments)
                {
                    if (s.Type == RouteSegmentType.Teleport && s.TeleportLink != null && s.TeleportLink.SourceName.Contains("Jangan") && s.TeleportLink.DestinationName.Contains("Donwhang"))
                        hasJanganDonwhangGate = true;
                }
            }
            Assert(hasJanganDonwhangGate, "Jangan -> Donwhang route includes Jangan -> Donwhang Dimensional Gate transition");

            // Test 3: Donwhang -> Hotan (Cross-Town via Dimensional Gate)
            var routeDonwhangHotan = nav.FindCompoundRoute(donwhangTown, hotanTown);
            Assert(routeDonwhangHotan != null && !routeDonwhangHotan.IsEmpty, "Donwhang -> Hotan: Cross-town gate route found");
            bool hasDonwhangHotanGate = false;
            if (routeDonwhangHotan != null)
            {
                foreach (var s in routeDonwhangHotan.Segments)
                {
                    if (s.Type == RouteSegmentType.Teleport && s.TeleportLink != null && s.TeleportLink.SourceName.Contains("Donwhang") && s.TeleportLink.DestinationName.Contains("Hotan"))
                        hasDonwhangHotanGate = true;
                }
            }
            Assert(hasDonwhangHotanGate, "Donwhang -> Hotan route includes Donwhang -> Hotan Dimensional Gate transition");

            // Test 4: Hotan -> Samarkand (Cross-Town via Dimensional Gate)
            var routeHotanSamarkand = nav.FindCompoundRoute(hotanTown, samarkandTown);
            Assert(routeHotanSamarkand != null && !routeHotanSamarkand.IsEmpty, "Hotan -> Samarkand: Cross-town gate route found");
            bool hasHotanSamarkandGate = false;
            if (routeHotanSamarkand != null)
            {
                foreach (var s in routeHotanSamarkand.Segments)
                {
                    if (s.Type == RouteSegmentType.Teleport && s.TeleportLink != null && s.TeleportLink.SourceName.Contains("Hotan") && s.TeleportLink.DestinationName.Contains("Samarkand"))
                        hasHotanSamarkandGate = true;
                }
            }
            Assert(hasHotanSamarkandGate, "Hotan -> Samarkand route includes Hotan -> Samarkand Dimensional Gate transition");

            // Test 5: Samarkand -> Constantinople (Cross-Town via Dimensional Gate)
            var routeSamarkandConst = nav.FindCompoundRoute(samarkandTown, constantinopleTown);
            Assert(routeSamarkandConst != null && !routeSamarkandConst.IsEmpty, "Samarkand -> Constantinople: Cross-town gate route found");
            bool hasSamarkandConstGate = false;
            if (routeSamarkandConst != null)
            {
                foreach (var s in routeSamarkandConst.Segments)
                {
                    if (s.Type == RouteSegmentType.Teleport && s.TeleportLink != null && s.TeleportLink.SourceName.Contains("Samarkand") && s.TeleportLink.DestinationName.Contains("Constantinople"))
                        hasSamarkandConstGate = true;
                }
            }
            Assert(hasSamarkandConstGate, "Samarkand -> Constantinople route includes Samarkand -> Constantinople Dimensional Gate transition");

            // Test 6: Hotan Town -> Hotan Hunting Field (Oasis / Karakoram approach in Region 3)
            var hotanField = new SRCoord(23687, 500, 250, 500);
            var routeHotanField = nav.FindCompoundRoute(hotanTown, hotanField);
            Assert(routeHotanField != null && !routeHotanField.IsEmpty, "Hotan Town -> Hotan Field: Compound route found");

            // Test 7: Multi-Hop Continental Route (Jangan Town -> Constantinople Town)
            var routeJanganConst = nav.FindCompoundRoute(janganTown, constantinopleTown);
            Assert(routeJanganConst != null && !routeJanganConst.IsEmpty, "Jangan -> Constantinople: Multi-hop continental route found");
            int teleportGateHops = 0;
            if (routeJanganConst != null)
            {
                foreach (var s in routeJanganConst.Segments)
                {
                    if (s.Type == RouteSegmentType.Teleport)
                        teleportGateHops++;
                }
            }
            Assert(teleportGateHops >= 4, "Jangan -> Constantinople multi-hop route chains through all intermediate gates (Hops: " + teleportGateHops + ")");

            // Test 8: Town -> Field in another city (Jangan Town -> Donwhang Field)
            var routeJanganToDhField = nav.FindCompoundRoute(janganTown, donwhangField);
            Assert(routeJanganToDhField != null && !routeJanganToDhField.IsEmpty, "Jangan Town -> Donwhang Field: Gate + Multi-Region Walk route found");
            if (routeJanganToDhField != null && routeJanganToDhField.Segments.Count > 0)
            {
                var finalSegment = routeJanganToDhField.Segments[routeJanganToDhField.Segments.Count - 1];
                int wpCount = finalSegment.Waypoints != null ? finalSegment.Waypoints.Count : 0;
                Assert(finalSegment.Type == RouteSegmentType.Walk && wpCount > 0,
                    "Final segment to training field is a valid NavMesh walk (waypoint count: " + wpCount + ")");
            }

            // Test 9: Huang He River Crossing (River Ferry Bridge)
            var southRiverPoint = new SRCoord(25244, 915, 69, 360); // Near Ferry Hageuk
            var northRiverPoint = new SRCoord(24734, 325, 62, 1615); // Near Ferry Chau
            var routeRiver = nav.FindCompoundRoute(southRiverPoint, northRiverPoint);
            Assert(routeRiver != null && !routeRiver.IsEmpty, "Huang He River crossing route found");
            bool hasFerryLink = false;
            if (routeRiver != null)
            {
                foreach (var s in routeRiver.Segments)
                {
                    if (s.Type == RouteSegmentType.Teleport && s.TeleportLink != null && s.TeleportLink.IsFerry)
                        hasFerryLink = true;
                }
            }
            Assert(hasFerryLink, "Huang He River crossing correctly selects Ferry bridge");
        }

        private static void TestConfiguredTrainingRoute()
        {
            Console.WriteLine("\n--- [4] Configured Training Route (Jangan -> Region 24211) ---");

            var nav = NavigationManager.Get;
            var janganTown = new SRCoord(25000, 969, 0, 1369);
            var training = new SRCoord((ushort)24211, 213, 1053, 535);

            var sw = Stopwatch.StartNew();
            var route = nav.FindCompoundRoute(janganTown, training);
            sw.Stop();

            Assert(route != null && !route.IsEmpty, "Jangan -> configured training field: route found");
            Assert(route != null && route.Segments.Exists(s => s.Type == RouteSegmentType.Teleport),
                "Jangan -> configured training field: route uses a teleport/gate");
            Assert(route != null && route.Segments.Exists(s => s.Type == RouteSegmentType.Walk),
                "Jangan -> configured training field: route ends with a walk segment");
            Assert(sw.ElapsedMilliseconds < 10000,
                "Jangan -> configured training field: route resolves quickly (actual: " + sw.ElapsedMilliseconds + "ms)");

            sw.Restart();
            var directWalk = nav.FindMultiRegionRoute(janganTown, training);
            sw.Stop();

            Assert(directWalk == null, "Jangan -> configured training field: no fabricated cross-river walk route");
            Assert(sw.ElapsedMilliseconds < 5000,
                "Jangan -> configured training field: cross-river walk rejection is fast (actual: " + sw.ElapsedMilliseconds + "ms)");

            Console.WriteLine("\n--- [5] Off-Mesh Training Target Last-Mile ---");
            var offMesh = new SRCoord((ushort)23445, 349, 1019, 193);
            sw.Restart();
            var offMeshRoute = nav.FindCompoundRoute(janganTown, offMesh);
            sw.Stop();

            Assert(offMeshRoute != null && !offMeshRoute.IsEmpty, "Off-mesh training target: route via nearest walkable point found");
            bool endsAtOffMeshTarget = false;
            if (offMeshRoute != null && offMeshRoute.Segments.Count > 0)
            {
                var finalWalk = offMeshRoute.Segments[offMeshRoute.Segments.Count - 1];
                endsAtOffMeshTarget = finalWalk.Type == RouteSegmentType.Walk
                    && finalWalk.Waypoints != null && finalWalk.Waypoints.Count > 0
                    && finalWalk.Waypoints[finalWalk.Waypoints.Count - 1].DistanceTo(offMesh) < 1.0;
            }
            Assert(endsAtOffMeshTarget, "Off-mesh training target: original target preserved as final waypoint");
            Assert(sw.ElapsedMilliseconds < 5000,
                "Off-mesh training target: last-mile route resolves quickly (actual: " + sw.ElapsedMilliseconds + "ms)");
        }
    }
}
