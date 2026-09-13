using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace xBot.App
{
    // Failed/unacknowledged buffs wait individually. A busy action channel does
    // not count as an attempt and cannot discard a due buff.
    public sealed class BuffRetryTracker
    {
        private readonly object sync = new object();
        private readonly Dictionary<uint, long> retryAt = new Dictionary<uint, long>();
        private readonly Func<long> clock;
        public BuffRetryTracker() : this(() => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency) { }
        public BuffRetryTracker(Func<long> clock) { this.clock = clock; }

        public bool CanTry(uint skill)
        {
            lock (sync)
            {
                long until;
                return !retryAt.TryGetValue(skill, out until) || clock() >= until;
            }
        }
        public bool TrySend(uint skill, Func<bool> send)
        {
            lock (sync)
            {
                long until;
                if (retryAt.TryGetValue(skill, out until) && clock() < until) return false;
                if (!send()) return false;
                retryAt[skill] = clock() + 2000;
                return true;
            }
        }
        public void Applied(uint skill) { lock (sync) retryAt.Remove(skill); }
        public void Rejected(uint skill, int retryMs)
        {
            lock (sync) retryAt[skill] = clock() + Math.Max(250, retryMs);
        }
        public void Reset() { lock (sync) retryAt.Clear(); }
    }
}
