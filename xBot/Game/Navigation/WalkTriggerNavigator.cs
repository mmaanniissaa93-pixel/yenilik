using System;
using System.Collections.Generic;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
    public sealed class WalkTriggerNavigator
    {
        // Real cave endpoints can be 10-14m off mesh. Cross that bounded gap in
        // <=3m measured steps, never one long move from an arbitrary position.
        public const double MaxTailDistance = 16;
        public Func<SRCoord> Position;
        public Func<bool> Active, Loading;
        public Func<SRCoord, IList<SRCoord>, List<SRCoord>> FindPath;
        public Func<SRCoord, Func<bool>, bool> Walk;
        public Action<int> Pause;
        public Action<string> Log;

        public bool Execute(TeleportLinkInfo link)
        {
            var before = Position();
            if (!TeleportTransitionPolicy.SameSpace(before, link.BoardCoord)) return false;
            Func<bool> arrived = () => !Loading() && TeleportTransitionPolicy.HasArrived(before, Position(), link.ArriveCoord);
            Func<bool> interrupted = () => Loading() || arrived() || !TeleportTransitionPolicy.SameSpace(Position(), link.BoardCoord);
            var failed = new List<SRCoord>();
            Log($"[CAVE-TRANSITION] type=WalkTrigger source={link.SourceId} trigger={link.BoardCoord} destination={link.ArriveCoord} beforeRegion={before.Region}");
            for (int attempt = 0; attempt < 2 && Active(); attempt++)
            {
                if (WaitForArrival(arrived, 0)) return Success();
                if (!TeleportTransitionPolicy.SameSpace(Position(), link.BoardCoord)) return false;
                var path = FindPath(Position(), failed);
                if (path == null || path.Count == 0 || path[path.Count - 1].DistanceTo(link.BoardCoord) > MaxTailDistance)
                { Log("[CAVE-TRANSITION] no reachable mesh point within bounded trigger tail"); return false; }
                bool blocked = false;
                foreach (var point in path)
                {
                    if (!Active()) return false;
                    if (interrupted()) break;
                    if (!Walk(point, interrupted)) { failed.Add(point); blocked = true; break; }
                }
                if (!Active()) return false;
                if (interrupted())
                {
                    if (WaitForArrival(arrived, 100)) return Success();
                    if (!TeleportTransitionPolicy.SameSpace(Position(), link.BoardCoord)) return false;
                    continue;
                }
                if (blocked) continue;
                var current = Position();
                if (current == null || current.DistanceTo(link.BoardCoord) > MaxTailDistance + 0.75) return false;
                var approachOrigin = current;
                for (int step = 0; step < 7 && Active() && !interrupted(); step++)
                {
                    current = Position();
                    double distance = current.DistanceTo(link.BoardCoord);
                    if (distance <= 0.3) break;
                    double k = Math.Min(3, distance) / distance;
                    double x = current.PosX + (link.BoardCoord.PosX - current.PosX) * k;
                    double y = current.PosY + (link.BoardCoord.PosY - current.PosY) * k;
                    var point = current.inDungeon() ? new SRCoord(x, y, current.Region, link.BoardCoord.Z) : new SRCoord(x, y, link.BoardCoord.Z);
                    Log($"[CAVE-TRANSITION] tail step={step} before={current} target={point}");
                    bool moved = Walk(point, interrupted);
                    Log($"[CAVE-TRANSITION] tail result={moved} after={Position()}");
                    if (!moved) break;
                }
                // The database point can sit on the near edge of the trigger volume.
                // Walk through the gate in several bounded steps. A single 2m command
                // can stop at the client collision edge without ever entering the
                // server trigger, which is what happens at the Donwhang dungeon gate.
                current = Position();
                var triggerTarget = link.TriggerCoord;
                if (triggerTarget != null && current != null
                    && TeleportTransitionPolicy.SameSpace(current, triggerTarget)
                    && current.DistanceTo(triggerTarget) > 0.75
                    && Active() && !interrupted())
                {
                    // Keep each command short so a collision at the doorway cannot
                    // turn the continuation point into another long blind move.
                    for (int step = 0; step < 8 && Active() && !interrupted(); step++)
                    {
                        current = Position();
                        if (current == null) break;
                        double triggerDistance = current.DistanceTo(triggerTarget);
                        if (triggerDistance <= 0.75) break;
                        double k = Math.Min(3, triggerDistance) / triggerDistance;
                        var point = current.inDungeon()
                            ? new SRCoord(current.PosX + (triggerTarget.PosX - current.PosX) * k,
                                current.PosY + (triggerTarget.PosY - current.PosY) * k,
                                current.Region, triggerTarget.Z)
                            : new SRCoord(current.PosX + (triggerTarget.PosX - current.PosX) * k,
                                current.PosY + (triggerTarget.PosY - current.PosY) * k,
                                triggerTarget.Z);
                        Log($"[CAVE-TRANSITION] trigger approach step={step} target={point}");
                        if (!Walk(point, interrupted)) break;
                        if (!TeleportTransitionPolicy.SameSpace(Position(), link.BoardCoord)) break;
                    }
                    current = Position();
                    if (WaitForArrival(arrived, 100)) return Success();
                    if (current == null || !TeleportTransitionPolicy.SameSpace(current, link.BoardCoord)) return false;
                }

                double approachDistance = approachOrigin.DistanceTo(link.BoardCoord);
                var directionOrigin = approachDistance > 0.5 ? approachOrigin : before;
                var crossingBase = triggerTarget ?? link.BoardCoord;
                if (triggerTarget != null)
                    directionOrigin = link.BoardCoord;
                double directionDistance = directionOrigin == null ? 0 : directionOrigin.DistanceTo(crossingBase);
                if (Active() && !interrupted() && current != null && current.DistanceTo(crossingBase) <= 0.75
                    && directionDistance > 0.5)
                {
                    double dx = (crossingBase.PosX - directionOrigin.PosX) / directionDistance;
                    double dy = (crossingBase.PosY - directionOrigin.PosY) / directionDistance;
                    var anchor = current;
                    bool crossed = false;
                    // Keep the sweep on the approach line. Side-to-side probing
                    // makes the character visibly shuffle at the doorway and does
                    // not help a directional trigger such as the DW entrance.
                    foreach (int depth in new[] { 3, 6, 9, 12, 16, 20, 24 })
                    {
                        if (!Active() || interrupted()) break;
                        var crossing = anchor.inDungeon()
                            ? new SRCoord(crossingBase.PosX + dx * depth, crossingBase.PosY + dy * depth,
                                anchor.Region, crossingBase.Z)
                            : new SRCoord(crossingBase.PosX + dx * depth, crossingBase.PosY + dy * depth,
                                crossingBase.Z);
                        Log($"[CAVE-TRANSITION] crossing trigger depth={depth} target={crossing}");
                        if (!Walk(crossing, interrupted)) break;
                        crossed = !TeleportTransitionPolicy.SameSpace(Position(), link.BoardCoord);
                        if (crossed) break;
                    }
                }
                if (WaitForArrival(arrived, 100)) return Success();
                Log($"[CAVE-TRANSITION] timeout attempt={attempt + 1}/2 afterRegion={Position()?.Region}");
            }
            return false;

            bool Success()
            { Log($"[CAVE-TRANSITION] success beforeRegion={before.Region} afterRegion={Position()?.Region} position={Position()}"); return true; }
        }

        private bool WaitForArrival(Func<bool> arrived, int polls)
        {
            for (int i = 0; i <= polls && Active(); i++)
            {
                if (arrived()) return true;
                if (i < polls) Pause(100);
            }
            return false;
        }
    }
}
