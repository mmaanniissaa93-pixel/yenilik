using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using xBot.App;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;

internal static class Program
{
    private static int checks;
    private static void Check(string name, bool result)
    {
        if (!result) throw new Exception(name);
        checks++; Console.WriteLine("PASS: " + name);
    }

    private static byte[] Menu(uint id)
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write((byte)4); Ascii(writer, "QUEST"); writer.Write((byte)1); Ascii(writer, "SN_Q_" + id);
            return stream.ToArray();
        }
    }
    private static void Ascii(BinaryWriter writer, string value)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value); writer.Write((ushort)bytes.Length); writer.Write(bytes);
    }
    private static QuestNpcTransaction Transaction => (QuestNpcTransaction)typeof(QuestAutomationManager)
        .GetField("Current", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static int Main()
    {
        try
        {
            var rule = new QuestAutomationRule { QuestId = 100, Enabled = true, RepeatIfAvailable = true,
                RewardPreference = QuestRewardPreference.ItemName, PreferredItemName = "Example", PreferredRewardId = 5,
                PreferredWeaponType = 3, NpcRegion = 25001, NpcX = 184, NpcZ = -36, NpcY = 913, NpcModelId = 2015 };
            QuestAutomationManager.SaveRule(rule);
            var json = QuestAutomationManager.ToJson();
            QuestAutomationManager.FromJson(json);
            var loaded = QuestAutomationManager.GetRule(100);
            Check("Profile preserves reward choice and X/Z/Y", loaded.PreferredItemName == "Example" && loaded.PreferredRewardId == 5
                && loaded.RewardPreference == QuestRewardPreference.ItemName && loaded.NpcY == 913 && loaded.NpcZ == -36);
            InfoManager.Npcs[77] = new SRNpc { UniqueID = 77, ID = 2015, ServerName = "NPC_TEST", Position = InfoManager.Character.Position };
            Check("Catalog/saved NPC starts automatic accept", QuestAutomationManager.TryExecutePending(new Bot()) && PacketBuilder.Sent.Contains("talk:77"));
            string error;
            Check("Second conversation cannot replace pending operation", !QuestAutomationManager.ArmQuestTalkSelection(101, 77, false, out error));
            QuestAutomationManager.ObserveTalkPacket(Menu(100));
            int sent = PacketBuilder.Sent.Count;
            QuestAutomationManager.ObserveTalkPacket(Menu(100));
            Check("Duplicate menu sends only one choice", PacketBuilder.Sent.Count == sent);
            QuestAutomationManager.ObserveQuestIdResponse(100);
            Check("Accept ID response retains 0x30D5 wait", QuestAutomationManager.IsBusy && !PacketBuilder.Sent.Contains("reward:100"));
            QuestAutomationManager.ObserveServerUpdate(999, 1);
            Check("Unrelated add cannot finish accept", QuestAutomationManager.IsBusy);
            InfoManager.Character.Quests[100] = new SRQuest { ID = 100, State = 1 };
            QuestAutomationManager.ObserveServerUpdate(100, 1);
            Check("Server add releases conversation and shows Active", !QuestAutomationManager.IsBusy && QuestAutomationManager.GetStateText(100) == "Active");
            InfoManager.Character.Quests[100].State = 2;
            Check("Completed comes from server state", QuestAutomationManager.GetStateText(100) == "Completed");
            Check("Completed quest starts turn-in", QuestAutomationManager.TryExecutePending(new Bot()));
            QuestAutomationManager.ObserveTalkPacket(Menu(100));
            QuestAutomationManager.ObserveQuestIdResponse(100);
            Check("Reward send retains Turning in", QuestAutomationManager.IsBusy && QuestAutomationManager.GetStateText(100) == "Turning in");
            sent = PacketBuilder.Sent.Count;
            QuestAutomationManager.ObserveQuestIdResponse(100);
            Check("Duplicate reward response cannot double send", PacketBuilder.Sent.Count == sent);
            Check("No new conversation while awaiting removal", QuestAutomationManager.TryExecutePending(new Bot()) && PacketBuilder.Sent.Count == sent);
            InfoManager.Character.Quests.RemoveKey(100);
            QuestAutomationManager.ObserveServerUpdate(100, 3);
            Check("Removal starts repeat wait", !QuestAutomationManager.IsBusy && QuestAutomationManager.GetStateText(100) == "Waiting to repeat");
            Check("Repeat cooldown prevents immediate accept", !QuestAutomationManager.TryExecutePending(new Bot()));
            var repeats = (Dictionary<uint, DateTime>)typeof(QuestAutomationManager).GetField("RepeatAfterUtc", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            repeats[100] = DateTime.UtcNow.AddSeconds(-1);
            Check("Repeat resumes accept after confirmed removal and cooldown", QuestAutomationManager.TryExecutePending(new Bot()) && Transaction.Operation == QuestNpcOperation.Accept);
            QuestAutomationManager.ObserveTalkPacket(Menu(100));
            InfoManager.Character.Quests[100] = new SRQuest { ID = 100, State = 1 };
            QuestAutomationManager.ObserveServerUpdate(100, 1);
            Check("Repeated acceptance verified by fresh add", QuestAutomationManager.GetStateText(100) == "Active");

            InfoManager.Character.Quests.RemoveKey(100);
            QuestAutomationManager.ArmQuestTalkSelection(100, 77, false, out error);
            var tx = Transaction;
            tx.AwaitConfirmation(DateTime.UtcNow.AddMinutes(-1));
            QuestAutomationManager.PollTimeout();
            Check("Timeout releases NPC and marks failed", !QuestAutomationManager.IsBusy && QuestAutomationManager.GetStateText(100) == "Failed");
            Check("Backoff rejects immediate retry", !QuestAutomationManager.ArmQuestTalkSelection(100, 77, false, out error));
            QuestAutomationManager.SaveRule(loaded);
            QuestAutomationManager.ArmQuestTalkSelection(100, 77, false, out error);
            QuestAutomationManager.ObserveTalkPacket(Menu(999));
            Check("Unavailable quest does not send arbitrary menu choice", !QuestAutomationManager.IsBusy && QuestAutomationManager.GetStateText(100) == "Failed");
            QuestAutomationManager.SaveRule(loaded);
            QuestAutomationManager.ArmQuestTalkSelection(100, 77, true, out error);
            QuestAutomationManager.ObserveTalkPacket(Menu(100));
            QuestAutomationManager.ObserveQuestIdResponse(999);
            Check("Wrong reward quest ID fails transaction", !QuestAutomationManager.IsBusy && QuestAutomationManager.GetStateText(100) == "Failed");
            QuestAutomationManager.CancelSession();
            Check("Disconnect clears pending runtime", !QuestAutomationManager.IsBusy && QuestAutomationManager.GetStateText(100) == "Inactive");
            QuestAutomationManager.ResetRules();
            DataManager.Placements.Add(new NameValueCollection { ["model_id"] = "2015", ["servername"] = "NPC_TEST", ["region"] = "25001", ["x"] = "184", ["z"] = "-36", ["y"] = "913" });
            var catalogRule = new QuestAutomationRule { QuestId = 300, Enabled = true, RepeatIfAvailable = false };
            QuestAutomationManager.SaveRule(catalogRule);
            Check("PK2 placement works without learned NPC", QuestAutomationManager.TryExecutePending(new Bot()) && Transaction.QuestId == 300);
            QuestAutomationManager.ObserveTalkPacket(Menu(300));
            InfoManager.Character.Quests[300] = new SRQuest { ID = 300, State = 2 };
            QuestAutomationManager.ObserveServerUpdate(300, 1);
            QuestAutomationManager.TryExecutePending(new Bot());
            QuestAutomationManager.ObserveTalkPacket(Menu(300));
            QuestAutomationManager.ObserveQuestIdResponse(300);
            InfoManager.Character.Quests.RemoveKey(300);
            QuestAutomationManager.ObserveServerUpdate(300, 3);
            Check("Repeat disabled leaves confirmed quest inactive", QuestAutomationManager.GetStateText(300) == "Inactive" && !QuestAutomationManager.TryExecutePending(new Bot()));
            QuestAutomationManager.ArmQuestTalkSelection(400, 77, false, out error);
            QuestAutomationManager.Suspend();
            sent = PacketBuilder.Sent.Count;
            QuestAutomationManager.ObserveTalkPacket(Menu(400));
            QuestAutomationManager.ObserveQuestIdResponse(400);
            Check("Stop/teleport cannot send from delayed dialog", !QuestAutomationManager.IsBusy && PacketBuilder.Sent.Count == sent);
            QuestAutomationManager.ResetRules();
            QuestAutomationManager.SetOptions(5, true, false);
            var optionsJson = QuestAutomationManager.ToJson();
            QuestAutomationManager.SetOptions(0, false, true);
            QuestAutomationManager.FromJson(optionsJson);
            Check("Global quest options persist", QuestAutomationManager.MaximumLevelAbovePlayer == 5 && QuestAutomationManager.WaitForAllEnabledQuests && !QuestAutomationManager.EventQuestsInTownOnly);
            QuestAutomationManager.SaveRule(new QuestAutomationRule { QuestId = 501, Enabled = true });
            QuestAutomationManager.SaveRule(new QuestAutomationRule { QuestId = 502, Enabled = true });
            InfoManager.Character.Quests[501] = new SRQuest { ID = 501, State = 2 };
            InfoManager.Character.Quests[502] = new SRQuest { ID = 502, State = 1 };
            Check("Wait all defers completed quest delivery", !QuestAutomationManager.TryExecutePending(new Bot()));
            InfoManager.Character.Quests[502].State = 2;
            Check("All completed releases delivery", QuestAutomationManager.TryExecutePending(new Bot()) && Transaction.Operation == QuestNpcOperation.TurnIn);
            QuestAutomationManager.ResetRules();
            Console.WriteLine(checks + " manager scenarios passed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("FAIL: " + ex); return 1; }
    }
}
