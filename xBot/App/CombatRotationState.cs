using System;
using System.Collections.Generic;

namespace xBot.App
{
    // State belongs to one combat session; timestamps are supplied for deterministic tests.
    public sealed class CombatRotationState
    {
        private readonly Dictionary<uint, long> firstDot = new Dictionary<uint, long>();
        private readonly Dictionary<uint, long> deferred = new Dictionary<uint, long>();

        public bool DotDelayElapsed(uint id, bool hasDot, long now, int delaySeconds)
        {
            if (!hasDot) { firstDot.Remove(id); deferred.Remove(id); return false; }
            if (!firstDot.TryGetValue(id, out long since)) firstDot[id] = since = now;
            return now - since >= Math.Max(0, Math.Min(300, delaySeconds)) * 1000L;
        }

        public void Defer(uint id, long now) { deferred[id] = now + 30000; }
        public bool IsDeferred(uint id, bool hasDot, long now)
        {
            if (!hasDot) { firstDot.Remove(id); deferred.Remove(id); return false; }
            if (!deferred.TryGetValue(id, out long until)) return false;
            if (now < until) return true;
            deferred.Remove(id);
            firstDot.Remove(id);
            return false;
        }

        public void Prune(Func<uint, bool> isLive)
        {
            foreach (uint id in new List<uint>(firstDot.Keys))
                if (!isLive(id)) { firstDot.Remove(id); deferred.Remove(id); }
        }
        public void Reset() { firstDot.Clear(); deferred.Clear(); }
    }
}
