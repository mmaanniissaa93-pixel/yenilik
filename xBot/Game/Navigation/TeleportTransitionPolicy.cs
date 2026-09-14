using System;
using System.Collections.Generic;
using System.Linq;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
    public static class TeleportTransitionPolicy
    {
        public static bool SameSpace(SRCoord a, SRCoord b)
        {
            return a != null && b != null && a.inDungeon() == b.inDungeon()
                && (!a.inDungeon() || a.Region == b.Region);
        }

        public static TransitionMode Classify(TeleportLinkInfo link, IEnumerable<TeleportLinkInfo> links)
        {
            // Extractor's tid1=4 alone means "not a model", not necessarily a trigger.
            // Require a free, reciprocal, entity-less dungeon gate. In particular the
            // outdoor Roc return ferry (id=0) and paid selectable exits stay Interaction.
            if (link.NpcId != 0 || link.HasEntityModel || link.IsFerry || link.TypeId1 != 4 || link.Gold != 0
                || link.BoardCoord == null || link.ArriveCoord == null
                || !(link.BoardCoord.inDungeon() || link.ArriveCoord.inDungeon())
                || !(link.ServerName ?? "").StartsWith("GATE_", StringComparison.OrdinalIgnoreCase))
                return TransitionMode.Interaction;
            bool reverse = links.Any(r => r.SourceId == link.DestinationId && r.DestinationId == link.SourceId
                && SameSpace(r.BoardCoord, link.ArriveCoord) && r.BoardCoord.DistanceTo(link.ArriveCoord) < 1
                && SameSpace(r.ArriveCoord, link.BoardCoord) && r.ArriveCoord.DistanceTo(link.BoardCoord) < 1);
            return reverse ? TransitionMode.WalkTrigger : TransitionMode.Interaction;
        }

        public static bool HasArrived(SRCoord before, SRCoord current, SRCoord destination)
        {
            if (before == null || !SameSpace(current, destination) || current.DistanceTo(destination) > 25) return false;
            // An ordinary outdoor sector boundary is not teleport success. Same-region
            // room portals require a displacement, not merely reaching the trigger.
            return !SameSpace(before, current) || (before.DistanceTo(current) >= 30 && before.DistanceTo(destination) > 30);
        }
    }
}
