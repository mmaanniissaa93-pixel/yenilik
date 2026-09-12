using System;
using System.Collections.Generic;
using xBot.App;
using xBot.Game.Objects.Common;

internal static class Program
{
    private static void Check(bool passed, string name)
    {
        if (!passed) throw new Exception(name);
        Console.WriteLine("PASS: " + name);
    }

    private static void Main()
    {
        var target = new SRCoord(8.0, 0.0);
        var world = new World();
        world.Blocked = p => Math.Pow(p.PosX - 4, 2) + p.PosY * p.PosY < 1.5 * 1.5;
        var recovery = new CombatObstacleRecovery();
        var navigator = world.Navigator();
        Check(world.Position.DistanceTo(target) < 15, "Tree target starts inside attack range");
        Check(!world.ClearShot(target), "Tree initially blocks the attack");
        Check(recovery.OnFailure(true, a => navigator.TryDetour(target, a)), "First rejection retries without walking");
        Check(world.Commands.Count == 0, "A single skill rejection does not trigger a detour");
        recovery.OnFailure(true, a => navigator.TryDetour(target, a));
        Check(recovery.OnFailure(true, a => navigator.TryDetour(target, a)), "Repeated blocked attacks trigger a detour even in range");
        Check(world.ClearShot(target), "Sideways then forward clears the tree before retrying attack");
        Check(world.Commands.Count == 2 && world.CommandTimes[1] >= 1600,
            "Forward leg waits for lateral arrival at slow movement speed (longer than 750 ms)");

        world = new World();
        world.Blocked = p => p.PosY > 0.9 && p.PosX < 3;
        navigator = world.Navigator();
        recovery = new CombatObstacleRecovery();
        recovery.OnFailure(true, a => navigator.TryDetour(target, a));
        recovery.OnFailure(true, a => navigator.TryDetour(target, a));
        Check(recovery.OnFailure(true, a => navigator.TryDetour(target, a)) && world.Position.PosY < -2,
            "Blocked first side falls back to the opposite side");

        world = new World();
        world.Blocked = p => Math.Pow(p.PosX - 3, 2) + p.PosY * p.PosY < 1;
        navigator = world.Navigator();
        target = new SRCoord(20.0, 0.0);
        Check(!navigator.MoveTo(target, 3), "Direct approach detects lack of movement at the tree");
        Check(navigator.TryDetour(target, 0) && navigator.MoveTo(target, 3),
            "Out-of-range approach reaches the mob after a completed detour");

        world = new World { Blocked = p => true };
        navigator = world.Navigator();
        recovery = new CombatObstacleRecovery();
        int attempts = 0;
        for (int i = 0; i < 2; i++) recovery.OnFailure(true, a => false);
        Check(!recovery.OnFailure(true, a => { attempts++; return navigator.TryDetour(target, a); }) && attempts == 4,
            "Fully enclosed character releases target after four bounded attempts");
        Check(world.Time <= 4800, "Stationary character does not wait blindly for long travel times");

        recovery = new CombatObstacleRecovery();
        attempts = 0;
        for (int i = 0; i < 2; i++) recovery.OnFailure(false, a => { attempts++; return true; });
        Check(!recovery.OnFailure(false, a => { attempts++; return true; }) && attempts == 0,
            "Disabled collision or bypass setting never invokes detour movement");
        recovery = new CombatObstacleRecovery();
        bool retry = true;
        int failures = 0;
        while (retry && failures++ < 100) retry = recovery.OnFailure(true, a => true);
        Check(!retry && failures == 15, "Movement without attack success cannot reset the recovery budget forever");
        recovery.OnSuccess();
        Check(recovery.OnFailure(true, a => true), "Confirmed attack success resets the failure streak");

        world = new World();
        navigator = world.Navigator(p => p.PosY <= 2);
        Check(!navigator.TryDetour(target, 0) && world.Commands.Count == 0,
            "Training boundary is checked before sending any detour leg");
        world = new World { CancelAt = 400 };
        Check(!world.Navigator().TryDetour(target, 0) && world.Time == 400,
            "Bot stop or target death cancels movement before the forward leg");

        world = new World { Position = new SRCoord(25000.0, 25000.0, (ushort)0x8001, 123) };
        target = new SRCoord(25008.0, 25000.0, (ushort)0x8001, 123);
        Check(world.Navigator().TryDetour(target, 0), "Dungeon detour can complete");
        Check(world.Commands.TrueForAll(p => p.Region == 0x8001 && p.Z == 123),
            "Dungeon region and height are preserved on both waypoints");
    }

    private sealed class World
    {
        public SRCoord Position = new SRCoord(0.0, 0.0);
        public Func<SRCoord, bool> Blocked = p => false;
        public int Time;
        public int CancelAt = int.MaxValue;
        public List<SRCoord> Commands = new List<SRCoord>();
        public List<int> CommandTimes = new List<int>();
        private SRCoord destination;

        public LocalObstacleNavigator Navigator(Func<SRCoord, bool> allowed = null)
        {
            return new LocalObstacleNavigator(() => Position, p => {
                destination = p;
                Commands.Add(p);
                CommandTimes.Add(Time);
            }, Tick, () => Time < CancelAt, allowed ?? (p => true));
        }

        private void Tick(int ms)
        {
            Time += ms;
            if (destination == null) return;
            double distance = Position.DistanceTo(destination);
            if (distance < 0.01) return;
            double scale = Math.Min(1, ms * 0.002 / distance);
            double x = Position.PosX + (destination.PosX - Position.PosX) * scale;
            double y = Position.PosY + (destination.PosY - Position.PosY) * scale;
            var next = Position.inDungeon() ? new SRCoord(x, y, Position.Region, Position.Z) : new SRCoord(x, y, Position.Z);
            if (!Blocked(next)) Position = next;
        }

        public bool ClearShot(SRCoord target)
        {
            for (int i = 0; i <= 100; i++)
            {
                double t = i / 100.0;
                if (Blocked(new SRCoord(Position.PosX + (target.PosX - Position.PosX) * t,
                    Position.PosY + (target.PosY - Position.PosY) * t))) return false;
            }
            return true;
        }
    }
}
