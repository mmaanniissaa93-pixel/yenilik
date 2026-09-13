using System;
using System.Collections.Generic;

namespace xBot.App
{
    // One pickup in flight per actor; a failed drop cannot monopolize the loop.
    public sealed class PickupQueue
    {
        private readonly Dictionary<uint, long> deferred = new Dictionary<uint, long>();
        public uint Target { get; private set; }
        public uint Actor { get; private set; }
        private long deadline;

        public bool IsDeferred(uint target, long now)
        {
            long until;
            if (!deferred.TryGetValue(target, out until)) return false;
            if (now < until) return true;
            deferred.Remove(target);
            return false;
        }

        public bool TryBegin(uint actor, uint target, long now, int timeout)
        {
            if (Target != 0 || actor == 0 || target == 0 || IsDeferred(target, now)) return false;
            Actor = actor;
            Target = target;
            deadline = now + timeout;
            return true;
        }

        public void Observe(bool present, bool actorAvailable, long now)
        {
            if (Target == 0) return;
            if (!present || !actorAvailable) { Target = Actor = 0; return; }
            if (now >= deadline) Fail(now);
        }

        public void Fail(long now)
        {
            if (Target != 0)
            {
                if (deferred.Count > 256) deferred.Clear();
                deferred[Target] = now + 10000;
            }
            Target = Actor = 0;
        }

        public void Reset() { Target = Actor = 0; deferred.Clear(); }
    }
}
