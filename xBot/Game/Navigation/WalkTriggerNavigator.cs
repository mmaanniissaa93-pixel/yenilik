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
