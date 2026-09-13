using System;
using System.Diagnostics;

namespace xBot.App
{
    // One outstanding character cast. Pet commands do not use this channel.
    // B070 failures contain no request id, so overlapping casts are ambiguous.
    public sealed class CharacterActionTracker
    {
        private readonly object sync = new object();
        private readonly Func<long> clock;
        private bool pending;
        private bool reserved;
        private uint source, skill, target;
        private bool basic, attack;
        private int animation;
        private long deadline, readyAt;
        public uint PendingSkillId { get { lock (sync) return pending ? skill : 0; } }

        public CharacterActionTracker() : this(() => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency) { }
        public CharacterActionTracker(Func<long> clock) { this.clock = clock; }

        public bool CanStart
        {
            get { lock (sync) { Expire(); return !reserved && !pending && clock() >= readyAt; } }
        }

        public bool TryReserve()
        {
            lock (sync)
            {
                Expire();
                if (reserved || pending || clock() < readyAt) return false;
                reserved = true;
                return true;
            }
        }
        public void Release() { lock (sync) reserved = false; }

        private void Expire()
        {
            if (pending && clock() >= deadline)
            {
                pending = false;
                readyAt = Math.Max(readyAt, deadline + 200);
            }
        }

        public bool TryBegin(uint sourceId, uint skillId, uint targetId, bool isBasic,
            bool isAttack, int animationMs)
        {
            lock (sync)
            {
                Expire();
                if (reserved || pending || clock() < readyAt || sourceId == 0) return false;
                source = sourceId;
                skill = skillId;
                target = targetId;
                basic = isBasic;
                attack = isAttack;
                animation = Math.Max(isAttack ? 350 : 0, Math.Min(10000, animationMs));
                deadline = clock() + 1500;
                pending = true;
                return true;
            }
        }

        public bool Confirm(uint sourceId, uint skillId, uint targetId, bool isBasic, out bool wasAttack)
        {
            lock (sync)
            {
                wasAttack = false;
                Expire();
                if (!pending || sourceId != source || (basic ? !isBasic : skillId != skill)
                    || (target != 0 && targetId != target)) return false;
                pending = false;
                readyAt = clock() + animation;
                wasAttack = attack;
                return true;
            }
        }

        public bool Reject(ushort error, out bool wasAttack)
        {
            lock (sync)
            {
                wasAttack = false;
                Expire();
                if (!pending) return false;
                pending = false;
                wasAttack = attack;
                readyAt = clock() + (error == 5 ? 500 : error == 0x0C ? 350 : 150);
                return true;
            }
        }

        public bool ConfirmAppliedBuff(uint owner, uint skillId)
        {
            lock (sync)
            {
                // Buff-add is authoritative even on servers which omit B070
                // for self buffs. Never let it acknowledge an attack.
                if (attack || source != owner || skill != skillId) return false;
                pending = false;
                readyAt = clock();
                return true;
            }
        }

        public void Reset()
        {
            lock (sync) { pending = reserved = false; readyAt = 0; source = skill = target = 0; }
        }
    }
}
