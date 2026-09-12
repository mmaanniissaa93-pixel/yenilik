using System;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    // Local recovery uses observed movement, so it also works for small objects
    // that are absent from the coarse navigation graph.
    public sealed class LocalObstacleNavigator
    {
        public const int MaxDetours = 4;
        private readonly Func<SRCoord> position;
        private readonly Action<SRCoord> move;
        private readonly Action<int> wait;
        private readonly Func<bool> canContinue;
        private readonly Func<SRCoord, bool> allowed;

        public LocalObstacleNavigator(Func<SRCoord> position, Action<SRCoord> move,
            Action<int> wait, Func<bool> canContinue, Func<SRCoord, bool> allowed)
        {
            this.position = position;
            this.move = move;
            this.wait = wait;
            this.canContinue = canContinue;
            this.allowed = allowed;
        }

        public bool MoveTo(SRCoord destination, double tolerance)
        {
            if (!canContinue() || destination == null || !allowed(destination)) return false;
            SRCoord start = position();
            if (start == null) return false;
            double bestDistance = start.DistanceTo(destination);
            if (bestDistance <= tolerance) return true;

            move(destination);
            int lastProgress = 0;
            for (int elapsed = 100; elapsed <= 8000; elapsed += 100)
            {
                wait(100);
                SRCoord current = position();
                if (!canContinue() || current == null) break;
                double distance = current.DistanceTo(destination);
                if (distance <= tolerance)
                {
                    // The movement request points at the mob; stop at attack
                    // range instead of continuing into the obstructing object.
                    if (tolerance > 1) move(current);
                    return true;
                }
                if (bestDistance - distance >= 0.25)
                {
                    bestDistance = distance;
                    lastProgress = elapsed;
                }
                // Measure a full interval after issuing movement; never count
                // the initial standing position as a collision.
                if (elapsed - lastProgress >= 1200) break;
            }

            SRCoord stopped = position();
            if (stopped != null) move(stopped);
            return false;
        }

        public bool TryDetour(SRCoord target, int attempt)
        {
            SRCoord origin = position();
            if (!canContinue() || origin == null || target == null || attempt < 0 || attempt >= MaxDetours)
                return false;
            double dx = target.PosX - origin.PosX;
            double dy = target.PosY - origin.PosY;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < 0.1) return false;
            dx /= distance;
            dy /= distance;
            double side = (attempt % 2 == 0 ? 1 : -1) * (attempt < 2 ? 4.0 : 7.0);
            double forward = Math.Min(distance, 4.0);
            SRCoord lateral = Offset(origin, -dy * side, dx * side);
            SRCoord beyond = Offset(lateral, dx * forward, dy * forward);
            if (!allowed(lateral) || !allowed(beyond)) return false;

            // Complete the lateral leg before moving past the obstacle.
            // Being within attack range does not mean line of sight is clear.
            return MoveTo(lateral, 0.6) && MoveTo(beyond, 0.6);
        }

        private static SRCoord Offset(SRCoord origin, double dx, double dy)
        {
            return origin.inDungeon()
                ? new SRCoord(origin.PosX + dx, origin.PosY + dy, origin.Region, origin.Z)
                : new SRCoord(origin.PosX + dx, origin.PosY + dy, origin.Z);
        }
    }

    public sealed class CombatObstacleRecovery
    {
        private int failures;
        private int attempts;

        public void OnSuccess()
        {
            failures = 0;
            attempts = 0;
        }

        // True means retry combat; false means release this target.
        public bool OnFailure(bool enabled, Func<int, bool> tryDetour)
        {
            if (++failures < 3) return true;
            if (!enabled) return false;
            while (attempts < LocalObstacleNavigator.MaxDetours)
            {
                if (tryDetour(attempts++))
                {
                    failures = 0;
                    return true;
                }
            }
            return false;
        }
    }
}
