using System;
using System.Collections.Generic;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
    // Runtime dependencies are supplied by Bot; tests can simulate spawn and movement
    // without pretending that a local simulation is a live game-server test.
    public sealed class FerryApproachNavigator
    {
        public Func<SRCoord> Position { get; set; }
        public Func<bool> Active { get; set; }
        public Func<bool> CandidateVisible { get; set; }
        public Func<SRCoord, IList<SRCoord>, List<SRCoord>> FindPath { get; set; }
        public Func<SRCoord, Func<bool>, bool> Walk { get; set; }
        public Action<int> Pause { get; set; }
        public Action<string> Log { get; set; }

        public bool Approach(SRCoord board)
        {
            var failed = new List<SRCoord>();
            // Initial path plus at most two replans, always from the actual current position.
            for (int plan = 0; plan < 3 && Active(); plan++)
            {
                if (CandidateVisible()) return true;
                var current = Position();
                if (current == null || board == null) return false;
                var path = FindPath(current, failed);
                if (path == null || path.Count == 0)
                {
                    Log("[FERRY-NAV] no connected mesh path; approach failed");
                    return false;
                }
                Log($"[FERRY-NAV] current={current} board={board} nearestBoardNode={path[path.Count - 1]}");
                Log($"[FERRY-NAV] path count={path.Count} replan={plan}");
                bool blocked = false;
                for (int i = 0; i < path.Count && Active(); i++)
                {
                    if (CandidateVisible()) return true;
                    current = Position();
                    if (current == null) return false;
                    Log($"[FERRY-NAV] wp={i + 1}/{path.Count} position={path[i]} beforeDistBoard={current.DistanceTo(board):F1}");
                    bool reached = Walk(path[i], CandidateVisible);
                    var after = Position();
                    Log($"[FERRY-NAV] movement result={reached} before={current} after={after} moved={after?.DistanceTo(current):F2} afterDistBoard={after?.DistanceTo(board):F1}");
                    if (!Active()) return false;
                    if (CandidateVisible()) return true;
                    if (!reached)
                    {
                        failed.Add(path[i]);
                        Log("[FERRY-NAV] waypoint failed; recompute mesh path excluding failed node");
                        blocked = true;
                        break;
                    }
                }
                if (!Active()) return false;
                if (blocked) continue;
                // Bounded streaming grace, with cancellation and fresh entity snapshots.
                for (int poll = 0; poll < 10 && Active(); poll++)
                {
                    if (CandidateVisible()) return true;
                    Pause(200);
                }
                if (!Active()) return false;
                if (CandidateVisible()) return true;
                Log("[FERRY-NAV] reached closest nav point but no candidate; check board coordinates / DB / nearby matching");
                return false;
            }
            Log("[FERRY-NAV] recovery exhausted; route failure");
            return false;
        }
    }
}
