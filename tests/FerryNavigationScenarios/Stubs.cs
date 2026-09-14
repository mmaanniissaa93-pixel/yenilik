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
    public class TeleportManager
    {
        public static TeleportManager Get = new TeleportManager();
        public TeleportLinkInfo TestLink;
        public System.Collections.Generic.List<TeleportLinkInfo> Links = new System.Collections.Generic.List<TeleportLinkInfo>();
        public bool IsLinkBlacklisted(TeleportLinkInfo link) => false;
        public TeleportLinkInfo FindBestLink(SRCoord start, SRCoord target) => TestLink;
    }
}
namespace xBot.Game.Navigation
{
    public static class CollisionPolicy
    {
        public static bool DisableTaklamakan => false;
        public static bool IgnoreTeleportLevel => false;
        public static bool IsLinkAllowed(TeleportLinkInfo link, int level) => true;
        public static bool IsTaklamakanRegion(int region) => false;
    }
}

namespace xBot.Game { public static class InfoManager { public static TestCharacter Character => null; } public class TestCharacter { public int Level; } }
