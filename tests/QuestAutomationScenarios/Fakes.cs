using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

// Boundary fakes: the production manager, transaction, policy and coordinates
// run unchanged. Outbound packets are recorded; no game or account is contacted.
namespace xBot.App.Theme { public enum LogLevel { Info, Warning } }
namespace xBot.App
{
    public class Window { public static Window Get => null; public void Log(string text, Theme.LogLevel level = Theme.LogLevel.Info) { } }
    public class Bot
    {
        public bool isBotting = true;
        public bool UseReturnScroll() => true;
        public bool WaitMovement(SRCoord point, int attempts) { xBot.Game.InfoManager.Character.Position = point; return true; }
        public bool WaitSelectEntity(uint id, int attempts, int delay, string log) => true;
    }
    public class Script { public Script(string path) { } public void Run(int index) { } }
}
namespace xBot.Game.Objects.Common { public class SRQuest { public uint ID; public byte State; } }
namespace xBot.Game.Objects.Item { public class SRItem { public byte ID4; } }
namespace xBot.Game.Objects.Entity
{
    public class SRNpc
    {
        public uint UniqueID, ID; public string ServerName;
        public SRCoord Position; public SRCoord GetRealtimePosition() => Position;
    }
    public class TestCharacter
    {
        public byte Level = 100;
        public xDictionary<uint, SRQuest> Quests = new xDictionary<uint, SRQuest>();
        public xDictionary<uint, SRItem> Inventory = new xDictionary<uint, SRItem>();
        public SRCoord Position = new SRCoord((ushort)25001, 184, -36, 913);
        public SRCoord GetRealtimePosition() => Position;
    }
}
namespace xBot.Game
{
    public static class InfoManager
    {
        public static bool inGame = true, inTeleport;
        public static TestCharacter Character = new TestCharacter();
        public static xDictionary<uint, SRNpc> Npcs = new xDictionary<uint, SRNpc>();
    }
    public static class DataManager
    {
        public static List<NameValueCollection> Placements = new List<NameValueCollection>();
        public static NameValueCollection GetQuestData(uint id) => new NameValueCollection {
            ["namestring"] = "SN_Q_" + id, ["servername"] = "QNO_TEST_" + id, ["name"] = "Quest " + id, ["level"] = "1"
        };
        public static bool TryGetQuestRewardItems(uint id, out List<NameValueCollection> rewards) { rewards = new List<NameValueCollection>(); return true; }
        public static List<NameValueCollection> GetQuestNpcPositions(uint id, uint model, string code) => Placements;
    }
    public static class PacketBuilder
    {
        public static readonly List<string> Sent = new List<string>();
        public static void SelectQuestTalkOption(byte action) => Sent.Add("choice:" + action);
        public static void ReceiveEventQuestReward(uint id, uint reward) => Sent.Add("reward:" + id);
        public static void CloseNPC(uint id) => Sent.Add("close:" + id);
        public static void TalkNPC(uint id, byte mode) => Sent.Add("talk:" + id);
    }
}
namespace xBot.Game.Navigation
{
    public enum TownServiceType { PotionMerchant }
    public class TownServiceInfo { public SRCoord Coord; public uint NpcId; }
    public class TownManager { public bool IsNearTown(SRCoord coord, double distance) => true; public static TownManager Get = new TownManager(); public TownServiceInfo FindNearestService(SRCoord coord, TownServiceType type) => null; }
    public class NavigationManager
    {
        public static NavigationManager Get = new NavigationManager();
        public List<SRCoord> FindPath(SRCoord from, SRCoord to) => new List<SRCoord> { to };
    }
}
