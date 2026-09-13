using System;
using xBot.Game.Objects.Common;
namespace xBot.App
{
    public class Window
    {
        public static Window Get => null;
        public enum ProcessState { Warning }
        public void Log(string text) { }
        public void LogProcess(string text, ProcessState state = ProcessState.Warning) { }
    }
}
namespace xBot.Game.Navigation
{
    // Only DB/UI dependencies are stubbed. Production graph and executor files are linked.
    public class TeleportLinkInfo
    {
        public string SourceName, DestinationName;
        public SRCoord BoardCoord, ArriveCoord;
    }
    public class TeleportManager
    {
        public static TeleportManager Get = new TeleportManager();
        public TeleportLinkInfo TestLink;
        public TeleportLinkInfo FindBestLink(SRCoord start, SRCoord target) => TestLink;
    }
}
namespace xBot.Game.Navigation
{
    public static class CollisionPolicy
    {
        public static bool DisableTaklamakan => false;
        public static bool IsTaklamakanRegion(int region) => false;
    }
}
