using System;
using System.Collections.Generic;
using System.Linq;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
    public static class TransitionRoutePlanner
    {
        private sealed class State
        {
            public SRCoord Position;
            public double Cost;
            public List<RouteSegment> Segments;
        }

        public static NavigationRoute Find(SRCoord start, SRCoord target, IList<TeleportLinkInfo> links,
            Func<SRCoord, SRCoord, List<SRCoord>> walk, Func<SRCoord, SRCoord, List<SRCoord>> approach)
        {
            var pending = new List<State> { new State { Position = start, Segments = new List<RouteSegment>() } };
            var visited = new HashSet<TeleportLinkInfo>();
            // Each link's arrival state is expanded at most once; cycles cannot loop forever.
            while (pending.Count > 0)
            {
                var state = pending.OrderBy(s => s.Cost).First();
                pending.Remove(state);
                var finish = TeleportTransitionPolicy.SameSpace(state.Position, target) ? walk(state.Position, target) : null;
                if (finish != null && finish.Count > 0)
                {
                    state.Segments.Add(RouteSegment.CreateWalk(finish));
                    return new NavigationRoute { Segments = state.Segments };
                }
                foreach (var link in links)
                {
                    if (visited.Contains(link) || !TeleportTransitionPolicy.SameSpace(state.Position, link.BoardCoord)) continue;
                    var path = approach(state.Position, link.BoardCoord);
                    if (path == null || path.Count == 0) continue;
                    if (link.TransitionMode == TransitionMode.WalkTrigger && path.Last().DistanceTo(link.BoardCoord) > WalkTriggerNavigator.MaxTailDistance) continue;
                    visited.Add(link);
                    var segments = new List<RouteSegment>(state.Segments);
                    // Transition executor owns the final approach, including the trigger tail.
                    var outer = path.TakeWhile(p => p.DistanceTo(link.BoardCoord) > 40).ToList();
                    if (outer.Count > 0) segments.Add(RouteSegment.CreateWalk(outer));
                    segments.Add(RouteSegment.CreateTeleport(link));
                    pending.Add(new State { Position = link.ArriveCoord, Segments = segments,
                        Cost = state.Cost + state.Position.DistanceTo(link.BoardCoord) + 10 });
                }
            }
            return null;
        }
    }
}
