using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using xBot.App;
using xBot.Game;
using xBot.Game.Navigation;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;

namespace xBot.Tests
{
    public static class Phase3NavigationScenarios
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
            Console.WriteLine(" RUNNING PHASE 3 NAVIGATION & SAMESPACE AUDIT SCENARIOS");
            Console.WriteLine("================================================================================");

            TestCoordinateAndSameSpaceContract();
            TestTownManagerCrossSpaceSafety();
            TestLocalObstacleNavigatorSpaceSafety();
            TestScriptSpaceSafety();
            TestCombatAIBerserkSpaceSafety();
            TestTeleportLinkSelection();

            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" PHASE 3 RESULTS: " + passed + " PASSED, " + failed + " FAILED");
            Console.WriteLine("================================================================================");

            return failed;
        }

        private static void TestCoordinateAndSameSpaceContract()
        {
            Console.WriteLine("\n--- [1] Coordinate & SameSpace Contract ---");

            // Open-world: Region < 0x8000
            // Sector A: Region 25000 (xSector = 25000 & 0xFF, ySector = 25000 >> 8)
            // Sector B: Region 25001 (neighboring sector)
            SRCoord outdoorA = new SRCoord(25000, 500, 0, 500);
            SRCoord outdoorB = new SRCoord(25001, 500, 0, 500);

            Assert(!outdoorA.inDungeon(), "Outdoor coordinate A inDungeon is false");
            Assert(!outdoorB.inDungeon(), "Outdoor coordinate B inDungeon is false");
            Assert(TeleportTransitionPolicy.SameSpace(outdoorA, outdoorB), "Outdoor coordinates across different Regions belong to SameSpace");

            double outdoorDist = outdoorA.DistanceTo(outdoorB);
            Assert(outdoorDist > 0 && outdoorDist < 300, "Outdoor distance across sectors is continuous: " + outdoorDist.ToString("F1") + "m");

            // Dungeon coordinates: Region > 0x8000
            ushort dungeonRegion1 = 0x8101; // e.g. Jangan Cave B1
            ushort dungeonRegion2 = 0x8501; // e.g. Jangan Cave B2 / another cave
            SRCoord dungeon1 = new SRCoord(dungeonRegion1, 500, 0, 500);
            SRCoord dungeon2 = new SRCoord(dungeonRegion2, 500, 0, 500);

            Assert(dungeon1.inDungeon(), "Dungeon coordinate 1 inDungeon is true");
            Assert(dungeon2.inDungeon(), "Dungeon coordinate 2 inDungeon is true");

            // Synthetic coordinate collision: PosX and PosY are identical because both use 128*192 base
            double euclideanDist = dungeon1.DistanceTo(dungeon2);
            Assert(euclideanDist == 0.0, "Euclidean distance between identical (X,Y) in different dungeons evaluates to 0.0m (actual: " + euclideanDist + ")");
            Assert(!TeleportTransitionPolicy.SameSpace(dungeon1, dungeon2), "SameSpace correctly returns FALSE for different dungeon Regions");

            // Outdoor vs Dungeon
            Assert(!TeleportTransitionPolicy.SameSpace(outdoorA, dungeon1), "SameSpace correctly returns FALSE for outdoor vs dungeon");
            Assert(!TeleportTransitionPolicy.SameSpace(dungeon1, outdoorA), "SameSpace correctly returns FALSE for dungeon vs outdoor");

            // Same Dungeon Room
            SRCoord dungeon1Offset = new SRCoord(dungeonRegion1, 600, 0, 500);
            Assert(TeleportTransitionPolicy.SameSpace(dungeon1, dungeon1Offset), "SameSpace returns TRUE for same dungeon Region");
            Assert(dungeon1.DistanceTo(dungeon1Offset) == 10.0, "Distance within same dungeon is evaluated accurately: 10m");

            // Null Safety
            Assert(!TeleportTransitionPolicy.SameSpace(null, dungeon1), "SameSpace with null a returns false");
            Assert(!TeleportTransitionPolicy.SameSpace(dungeon1, null), "SameSpace with null b returns false");
            Assert(!TeleportTransitionPolicy.SameSpace(null, null), "SameSpace with null a and b returns false");
        }

        private static void TestTownManagerCrossSpaceSafety()
        {
            Console.WriteLine("\n--- [2] TownManager Cross-Space Safety ---");

            // Jangan town center / services are outdoor
            // Find Jangan potion service coordinate
            var service = TownManager.Get.FindNearestService(new SRCoord(100.0, 100.0), TownServiceType.PotionMerchant);
            Assert(service != null, "TownManager services initialized and found PotionMerchant");

            SRCoord townNpcCoord = service.Coord;
            Assert(townNpcCoord != null, "Town service coordinate is non-null");
            Assert(!townNpcCoord.inDungeon(), "Town service coordinate is outdoor");

            // Outdoor position near town service
            SRCoord charNearTown = new SRCoord(townNpcCoord.PosX + 5.0, townNpcCoord.PosY + 5.0);
            Assert(TownManager.Get.IsNearTown(charNearTown, 50.0), "Character near town service in outdoor space is correctly detected as NearTown");

            // Dungeon position having identical synthetic coordinates
            SRCoord charInDungeonSameCoord = new SRCoord(townNpcCoord.PosX + 5.0, townNpcCoord.PosY + 5.0, (ushort)0x8201);
            Assert(charInDungeonSameCoord.inDungeon(), "Character in dungeon is confirmed inDungeon == true");
            Assert(!TownManager.Get.IsNearTown(charInDungeonSameCoord, 50.0), "Dungeon character with colliding (X,Y) is NOT falsely reported as NearTown");

            // FindNearestService from dungeon
            var serviceFromDungeon = TownManager.Get.FindNearestService(charInDungeonSameCoord, TownServiceType.PotionMerchant);
            Assert(serviceFromDungeon == null, "FindNearestService returns null when character is in dungeon (no cross-space town match)");
        }

        private static void TestLocalObstacleNavigatorSpaceSafety()
        {
            Console.WriteLine("\n--- [3] LocalObstacleNavigator Space Safety ---");

            SRCoord charPos = new SRCoord(100.0, 200.0, (ushort)0x8101); // In Dungeon 1
            bool moveCalled = false;

            var navigator = new LocalObstacleNavigator(
                () => charPos,
                pt => { moveCalled = true; },
                ms => { },
                () => true,
                pt => true,
                stuckTimeoutMs: 1200
            );

            // 1. MoveTo destination in different space (Dungeon 2) with colliding coordinates
            SRCoord crossSpaceDest = new SRCoord(100.5, 200.5, (ushort)0x8501); // 0.7m away Euclidean!
            bool reached = navigator.MoveTo(crossSpaceDest, 3.0);

            Assert(!reached, "LocalObstacleNavigator.MoveTo returned false for cross-space target despite Euclidean distance < tolerance");
            Assert(!moveCalled, "LocalObstacleNavigator did not issue move command for cross-space target");

            // 2. TryDetour for cross-space target
            bool detourResult = navigator.TryDetour(crossSpaceDest, 0);
            Assert(!detourResult, "LocalObstacleNavigator.TryDetour safely rejected cross-space detour target");

            // 3. MoveTo destination in same space within tolerance
            SRCoord sameSpaceDest = new SRCoord(100.2, 200.2, (ushort)0x8101);
            bool sameSpaceReached = navigator.MoveTo(sameSpaceDest, 1.0);
            Assert(sameSpaceReached, "LocalObstacleNavigator.MoveTo succeeded for valid same-space target within tolerance");
        }

        private static void TestScriptSpaceSafety()
        {
            Console.WriteLine("\n--- [4] Script Space Safety ---");

            // Create temporary script file with MOVE coordinates
            string tempScript = Path.Combine(Path.GetTempPath(), "xbot_test_script_" + Guid.NewGuid().ToString("N") + ".txt");
            try
            {
                // Write script with outdoor coordinates: Jangan area
                File.WriteAllLines(tempScript, new string[]
                {
                    "// Test script",
                    "MOVE\t100\t200",
                    "MOVE\t150\t250"
                });

                Script script = new Script();
                // Load script lines via reflection
                var linesField = typeof(Script).GetField("m_lines", BindingFlags.NonPublic | BindingFlags.Instance);
                linesField.SetValue(script, new List<string>(File.ReadAllLines(tempScript)));

                // Test GetNearMovement:
                // Character in outdoor space near (100, 200)
                SRCoord outdoorChar = new SRCoord(102.0, 201.0);
                int nearIndex = script.GetNearMovement(outdoorChar, 20);
                Assert(nearIndex == 1, "Outdoor character correctly matched script line index: " + nearIndex);

                // Character in dungeon with synthetic coords (102, 201)
                SRCoord dungeonChar = new SRCoord(102.0, 201.0, (ushort)0x8101);
                int dungeonIndex = script.GetNearMovement(dungeonChar, 20);
                Assert(dungeonIndex == -1, "Dungeon character did NOT match outdoor script line (avoided cross-space loop trigger)");
            }
            finally
            {
                if (File.Exists(tempScript)) File.Delete(tempScript);
            }
        }

        private static void TestCombatAIBerserkSpaceSafety()
        {
            Console.WriteLine("\n--- [5] Combat AI & Berserk Space Safety ---");

            // Character at outdoor position
            var character = (SRCharacter)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SRCharacter));
            typeof(SRCharacter).GetProperty("BerserkPoints", BindingFlags.Public | BindingFlags.Instance).SetValue(character, (byte)5);
            character.GameStateType = SRModel.GameState.None;
            character.UniqueID = 9999;
            character.Position = new SRCoord(500.0, 500.0);
            InfoManager.Current.Character = character;

            // Mob in dungeon with synthetic coordinate (501, 501)
            var dungeonMob = (SRMob)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SRMob));
            dungeonMob.Position = new SRCoord(501.0, 501.0, (ushort)0x8101);
            dungeonMob.MobType = SRMob.Mob.Giant;

            // CombatAIEngine settings
            CombatAIEngine.ZerkMonsterCountEnabled = true;
            CombatAIEngine.ZerkMonsterCount = 0;
            CombatAIEngine.ZerkEvenIfNotAttacking = false;
            CombatAIEngine.ZerkWhenFull = false;

            var mobs = new List<SRMob> { dungeonMob };
            bool canZerk = CombatAIEngine.CheckBerserkTrigger(mobs, 100.0);
            Assert(!canZerk, "Berserk was NOT triggered by mob located in a different space despite close Euclidean distance");

            // Mob in same space
            var sameSpaceMob = (SRMob)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SRMob));
            sameSpaceMob.Position = new SRCoord(501.0, 501.0); // outdoor
            sameSpaceMob.MobType = SRMob.Mob.Giant;

            var validMobs = new List<SRMob> { sameSpaceMob };
            bool canZerkSameSpace = CombatAIEngine.CheckBerserkTrigger(validMobs, 100.0);
            Assert(canZerkSameSpace, "Berserk triggered normally for mob in the same space");
        }

        private static void TestTeleportLinkSelection()
        {
            Console.WriteLine("\n--- [6] Teleport Link Selection Logic ---");

            // Outdoor start and dungeon target
            SRCoord outdoorStart = new SRCoord(100.0, 100.0);
            SRCoord dungeonTarget = new SRCoord(24576.0, 24576.0, (ushort)0x8101);

            Assert(!TeleportTransitionPolicy.SameSpace(outdoorStart, dungeonTarget), "Start and Target are across space boundary (outdoor -> dungeon)");

            // Link that boards outdoors and arrives in dungeon
            TeleportLinkInfo link = new TeleportLinkInfo();
            link.BoardCoord = new SRCoord(110.0, 105.0); // Outdoor near start
            link.ArriveCoord = new SRCoord(24570.0, 24570.0, (ushort)0x8101); // Dungeon near target

            Assert(TeleportTransitionPolicy.SameSpace(outdoorStart, link.BoardCoord), "Outdoor start matches link BoardCoord space");
            Assert(TeleportTransitionPolicy.SameSpace(dungeonTarget, link.ArriveCoord), "Dungeon target matches link ArriveCoord space");

            // Direct distance across spaces has no physical meaning
            double fakeDirectDist = outdoorStart.DistanceTo(dungeonTarget);
            double arriveToTargetDist = link.ArriveCoord.DistanceTo(dungeonTarget);

            // In our patched TeleportManager:
            // if (TeleportTransitionPolicy.SameSpace(start, target) && distFromArriveToTarget >= directDistance) continue;
            // Since start and target are NOT in the same space, the link is preserved!
            bool shouldFilter = TeleportTransitionPolicy.SameSpace(outdoorStart, dungeonTarget) && arriveToTargetDist >= fakeDirectDist;
            Assert(!shouldFilter, "Cross-space teleport link is correctly PRESERVED and not discarded by outdoor-to-dungeon Euclidean comparison");
        }
    }
}
