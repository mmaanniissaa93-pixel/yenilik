using System.Windows.Forms;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    public static class WinAPI
    {
        public static void InvokeIfRequired(this Control control, MethodInvoker action)
        {
            if (control.InvokeRequired) control.Invoke(action);
            else action(); // Do not swallow failures in these scenarios.
        }
    }
    public sealed class Bot
    {
        public static Bot Get { get; } = new Bot();
        public void MoveTo(SRCoord point) { }
    }
}
namespace xBot.Game
{
    public static class InfoManager { public static bool inGame = false; }
}
namespace xBot.Game.Objects.Entity
{
    public class SREntity
    {
        public SRCoord GetRealtimePosition() { return new SRCoord(0, 0); }
    }
}
