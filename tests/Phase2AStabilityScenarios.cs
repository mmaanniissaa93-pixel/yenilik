using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using xBot.App;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;

namespace xBot.Tests
{
    public static class Phase2AStabilityScenarios
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

        private static void SetCharacter(SRCharacter character)
        {
            InfoManager.Current.Character = character;
        }

        public static int RunAll()
        {
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" RUNNING PHASE 2A STABILITY SCENARIOS (C# IN-PROCESS AUDIT)");
            Console.WriteLine("================================================================================");

            Test_H02_CharacterNicknameGeneration();
            Test_H05_OnLoopTimerAndProtection();
            Test_H11_TrainingAreaAndConfigSnapshot();
            Test_H13_xListBoundarySafety();

            Console.WriteLine();
            Console.WriteLine("================================================================================");
            if (s_failCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" ALL PHASE 2A STABILITY SCENARIOS PASSED (" + s_passCount + " assertions)!");
                Console.ResetColor();
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" PHASE 2A STABILITY SCENARIOS COMPLETED WITH " + s_failCount + " FAILURES (" + s_passCount + " passed)!");
                Console.ResetColor();
                return 1;
            }
        }

        #region [H-02] Character Nickname Generation
        private static void Test_H02_CharacterNicknameGeneration()
        {
            Console.WriteLine();
            Console.WriteLine("--- [H-02] Karakter Adı Üretimi ve Sayaç ---");

            // 1. Initial empty counter produces "0" and advances to "1"
            string numToUse;
            string nextSeq;
            Bot.AdvanceSequenceCounter("", 3, out numToUse, out nextSeq);
            Assert(numToUse == "0" && nextSeq == "1", "Empty counter string defaults to '0' and advances to '1'");

            // 2. Sequential generation of 3 distinct names
            string baseName = "SilkHero";
            string currentSeq = "";
            
            // Attempt 1
            Bot.AdvanceSequenceCounter(currentSeq, 3, out numToUse, out nextSeq);
            string name1 = Bot.FormatNicknameWithSuffix(baseName, numToUse);
            currentSeq = nextSeq;

            // Attempt 2
            Bot.AdvanceSequenceCounter(currentSeq, 3, out numToUse, out nextSeq);
            string name2 = Bot.FormatNicknameWithSuffix(baseName, numToUse);
            currentSeq = nextSeq;

            // Attempt 3
            Bot.AdvanceSequenceCounter(currentSeq, 3, out numToUse, out nextSeq);
            string name3 = Bot.FormatNicknameWithSuffix(baseName, numToUse);

            Assert(name1 == "SilkHero0", "First generated name is 'SilkHero0' (actual: " + name1 + ")");
            Assert(name2 == "SilkHero1", "Second generated name is 'SilkHero1' (actual: " + name2 + ")");
            Assert(name3 == "SilkHero2", "Third generated name is 'SilkHero2' (actual: " + name3 + ")");
            Assert(name1 != name2 && name2 != name3, "Sequential names are strictly distinct");

            // 3. 12-character limit: preserve suffix and truncate base
            // "WarriorKnight" is 13 chars. With suffix "1", length is 14. Max is 12.
            // Base should be truncated to 11 chars ("WarriorKnig") + "1" = 12 chars.
            string longBase = "WarriorKnight";
            string limitedName = Bot.FormatNicknameWithSuffix(longBase, "1");
            Assert(limitedName.Length == 12, "Truncated name length is exactly 12 (actual: " + limitedName.Length + ")");
            Assert(limitedName == "WarriorKnig1", "Truncated name preserves suffix '1' (actual: " + limitedName + ")");

            // Base with 3-digit suffix: "LegendaryHero" (13 chars) + "999" (3 chars)
            // Allowed base is 12 - 3 = 9 ("Legendary") + "999" = "Legendary999"
            string limitedName3Digits = Bot.FormatNicknameWithSuffix("LegendaryHero", "999");
            Assert(limitedName3Digits.Length == 12, "Truncated name length with 3 digits is 12 (actual: " + limitedName3Digits.Length + ")");
            Assert(limitedName3Digits == "Legendary999", "Truncated name preserves suffix '999' (actual: " + limitedName3Digits + ")");

            // Short name does not truncate
            string shortName = Bot.FormatNicknameWithSuffix("Short", "5");
            Assert(shortName == "Short5", "Short name is not truncated (actual: " + shortName + ")");

            // 4. Invalid and negative inputs handled safely
            Bot.AdvanceSequenceCounter("-10", 3, out numToUse, out nextSeq);
            Assert(numToUse == "0" && nextSeq == "1", "Negative sequence resets to '0' and advances to '1'");

            Bot.AdvanceSequenceCounter("abc", 3, out numToUse, out nextSeq);
            Assert(numToUse == "0" && nextSeq == "1", "Non-numeric sequence resets to '0' and advances to '1'");

            // 5. Overflow rollover
            Bot.AdvanceSequenceCounter("999", 3, out numToUse, out nextSeq);
            Assert(numToUse == "999" && nextSeq == "0", "Sequence '999' resets next counter to '0' on MaxLength=3");
        }
        #endregion

        #region [H-05] OnLoop Timer and Protection
        private static void Test_H05_OnLoopTimerAndProtection()
        {
            Console.WriteLine();
            Console.WriteLine("--- [H-05] Timer ve Koruma Döngüsü (OnLoop) ---");

            var bot = Bot.Get;
            Assert(bot != null, "Bot singleton instance is available");

            // 1. Character == null: RunLoopTick executes safely without NRE
            SetCharacter(null);
            bool threw = false;
            try
            {
                bot.RunLoopTick();
            }
            catch (Exception ex)
            {
                threw = true;
                Console.WriteLine("Caught exception: " + ex);
            }
            Assert(!threw, "RunLoopTick with Character == null completed safely without NRE");

            // 2. Character != null but RealtimePosition == null: executes safely
            var character = (SRCharacter)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SRCharacter));
            character.Position = null;
            SetCharacter(character);
            threw = false;
            try
            {
                bot.RunLoopTick();
            }
            catch (Exception ex)
            {
                threw = true;
                Console.WriteLine("Caught exception: " + ex);
            }
            Assert(!threw, "RunLoopTick with null RealtimePosition completed safely without NRE");

            // 3. Character becomes valid: RunLoopTick executes normally
            character.Position = new SRCoord(0, 100, 200, 0);
            threw = false;
            try
            {
                bot.RunLoopTick();
            }
            catch (Exception ex)
            {
                threw = true;
                Console.WriteLine("Caught exception: " + ex);
            }
            Assert(!threw, "RunLoopTick with valid Character & Position completed successfully");

            // 4. Concurrency guard: Interlocked guard prevents duplicate execution
            int concurrentAttempts = 0;
            int ranCount = 0;
            var threads = new List<Thread>();
            for (int i = 0; i < 10; i++)
            {
                var t = new Thread(() =>
                {
                    Interlocked.Increment(ref concurrentAttempts);
                    bot.RunLoopTick();
                    Interlocked.Increment(ref ranCount);
                });
                threads.Add(t);
            }
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert(ranCount == 10, "10 concurrent RunLoopTick calls completed without deadlock or exception");
        }
        #endregion

        #region [H-11] Training Area Selection and Config Snapshot
        private static void Test_H11_TrainingAreaAndConfigSnapshot()
        {
            Console.WriteLine();
            Console.WriteLine("--- [H-11] Eğitim Alanı ve UI Thread Güvenliği ---");

            var window = Window.Get;
            Assert(window != null, "Window singleton instance is available");

            // 1. Unselected training area returns null (does not silently use Items[0])
            window.InvokeIfRequired(() =>
            {
                window.Training_lstvAreas.Items.Clear();
                window.Training_lstvAreas.Tag = null;
                window.Training_lstvAreas.SelectedItems.Clear();

                // Add an item to the list, but leave it unselected and unactivated
                var unselectedItem = new ListViewItem("Area #1");
                unselectedItem.SubItems.Add(new ListViewItem.ListViewSubItem { Tag = (ushort)25000 });
                unselectedItem.SubItems.Add(new ListViewItem.ListViewSubItem { Tag = 100 });
                unselectedItem.SubItems.Add(new ListViewItem.ListViewSubItem { Tag = 200 });
                unselectedItem.SubItems.Add(new ListViewItem.ListViewSubItem { Tag = 0 });
                window.Training_lstvAreas.Items.Add(unselectedItem);
            });

            SRCoord posWithoutSelection = window.TrainingArea_GetPosition();
            Assert(posWithoutSelection == null, "TrainingArea_GetPosition returned null when area is not activated/selected (avoided silent Items[0] fallback)");

            // 2. Activated area (Tag is set) returns the correct coordinates
            window.InvokeIfRequired(() =>
            {
                var activeItem = new ListViewItem("ActiveArea");
                activeItem.Tag = new TrainingAreaInfo { Region = 25000, X = 500, Y = 600, Z = 0, Radius = 65 };
                window.Training_lstvAreas.Tag = activeItem;
            });

            SRCoord activePos = window.TrainingArea_GetPosition();
            Assert(activePos != null && activePos.Region == 25000 && activePos.X == 500 && activePos.Y == 600, "TrainingArea_GetPosition returned correct coordinates when area is activated");

            int activeRadius = window.TrainingArea_GetRadius();
            Assert(activeRadius == 65, "TrainingArea_GetRadius returned correct radius: " + activeRadius);

            // 3. BotConfigSnapshot: Thread-safe capture and lifecycle
            window.InvokeIfRequired(() =>
            {
                if (window.Training_cbxWalkToCenter != null) window.Training_cbxWalkToCenter.Checked = true;
                if (window.Town_cbxEnableTownLoop != null) window.Town_cbxEnableTownLoop.Checked = true;
            });

            var snapshot = Bot.BotConfigSnapshot.Capture(window);
            Assert(snapshot != null, "BotConfigSnapshot.Capture succeeded");
            Assert(snapshot.WalkToCenter == true, "Snapshot accurately captured WalkToCenter == true");
            Assert(snapshot.EnableTownLoop == true, "Snapshot accurately captured EnableTownLoop == true");

            // Verify background thread can read snapshot properties without WinForms Invoke
            bool bgReadSuccess = false;
            var bgThread = new Thread(() =>
            {
                bool wtc = snapshot.WalkToCenter;
                bool etl = snapshot.EnableTownLoop;
                bool emp = snapshot.EnableMobPriority;
                bool tg = snapshot.TargetGeneral;
                if (wtc && etl && emp && tg)
                    bgReadSuccess = true;
            });
            bgThread.Start();
            bgThread.Join();
            Assert(bgReadSuccess, "Background worker safely read snapshot properties without cross-thread WinForms exceptions");
        }
        #endregion

        #region [H-13] xList Boundary Safety
        private static void Test_H13_xListBoundarySafety()
        {
            Console.WriteLine();
            Console.WriteLine("--- [H-13] xList<T> Sınır Güvenliği ---");

            var list = new xList<string>(10);
            Assert(list.Capacity == 10, "Initial xList capacity is 10");
            Assert(list.Count == 0, "Initial xList element count is 0");

            // 1. Negative index setter throws ArgumentOutOfRangeException
            bool caughtNegativeSetter = false;
            try
            {
                list[-1] = "Negative";
            }
            catch (ArgumentOutOfRangeException)
            {
                caughtNegativeSetter = true;
            }
            Assert(caughtNegativeSetter, "Setting list[-1] threw ArgumentOutOfRangeException as expected");

            // 2. Valid index 0 set and get
            list[0] = "FirstItem";
            Assert(list[0] == "FirstItem", "list[0] retrieved stored item");
            Assert(list.Count == 1, "list.Count incremented to 1");

            // 3. Out-of-bounds getter returns default(T) (preserves backward compatibility)
            string outOfBounds = list[99];
            Assert(outOfBounds == null, "list[99] returned null (default) without throwing");

            string negativeGetter = list[-5];
            Assert(negativeGetter == null, "list[-5] returned null (default) without throwing");

            // 4. Out-of-bounds setter expands the list
            list[25] = "ExpandedItem";
            Assert(list.Capacity == 26, "list capacity expanded to 26 (actual: " + list.Capacity + ")");
            Assert(list[25] == "ExpandedItem", "list[25] retrieved stored item");
            Assert(list.Count == 2, "list.Count is 2 (only real items counted)");

            // 5. TryGet API: distinguishes empty slot from out-of-bounds
            string val;
            bool okNegative = list.TryGet(-1, out val);
            Assert(!okNegative && val == null, "TryGet(-1) returned false (invalid index)");

            bool okOutOfBounds = list.TryGet(100, out val);
            Assert(!okOutOfBounds && val == null, "TryGet(100) returned false (invalid index)");

            bool okEmptySlot = list.TryGet(5, out val);
            Assert(okEmptySlot && val == null, "TryGet(5) returned true with null (distinguished valid empty slot from invalid index)");

            bool okExisting = list.TryGet(0, out val);
            Assert(okExisting && val == "FirstItem", "TryGet(0) returned true with 'FirstItem'");

            // 6. IsValidIndex and IsSlotEmpty APIs
            Assert(!list.IsValidIndex(-1), "IsValidIndex(-1) is false");
            Assert(!list.IsValidIndex(100), "IsValidIndex(100) is false");
            Assert(list.IsValidIndex(0) && list.IsValidIndex(25), "IsValidIndex(0) and IsValidIndex(25) are true");

            Assert(!list.IsSlotEmpty(-1), "IsSlotEmpty(-1) is false (out of bounds is not an empty slot)");
            Assert(list.IsSlotEmpty(5), "IsSlotEmpty(5) is true (allocated slot with null item)");
            Assert(!list.IsSlotEmpty(0), "IsSlotEmpty(0) is false (allocated slot with real item)");
        }
        #endregion
    }
}
