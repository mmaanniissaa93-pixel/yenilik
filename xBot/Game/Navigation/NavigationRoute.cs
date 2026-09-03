using System.Collections.Generic;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
	public enum RouteSegmentType
	{
		Walk,
		Teleport
	}

	public class RouteSegment
	{
		public RouteSegmentType Type { get; set; }
		public List<SRCoord> Waypoints { get; set; }
		public TeleportLinkInfo TeleportLink { get; set; }

		public static RouteSegment CreateWalk(List<SRCoord> waypoints)
		{
			return new RouteSegment
			{
				Type = RouteSegmentType.Walk,
				Waypoints = waypoints ?? new List<SRCoord>()
			};
		}

		public static RouteSegment CreateTeleport(TeleportLinkInfo link)
		{
			return new RouteSegment
			{
				Type = RouteSegmentType.Teleport,
				TeleportLink = link
			};
		}
	}

	public class NavigationRoute
	{
		public List<RouteSegment> Segments { get; set; } = new List<RouteSegment>();

		public bool IsEmpty => Segments == null || Segments.Count == 0;

		public int TotalWaypointsCount
		{
			get
			{
				int count = 0;
				if (Segments != null)
				{
					foreach (var s in Segments)
					{
						if (s.Type == RouteSegmentType.Walk && s.Waypoints != null)
							count += s.Waypoints.Count;
					}
				}
				return count;
			}
		}
	}
}
