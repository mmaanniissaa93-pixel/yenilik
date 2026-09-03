using System;
using System.Collections.Generic;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
	public class AStarPathfinder
	{
		private class PriorityQueueNode : IComparable<PriorityQueueNode>
		{
			public int PointIndex;
			public double Priority;

			public int CompareTo(PriorityQueueNode other)
			{
				return Priority.CompareTo(other.Priority);
			}
		}

		private class SimplePriorityQueue
		{
			private List<PriorityQueueNode> elements = new List<PriorityQueueNode>();

			public int Count => elements.Count;

			public void Enqueue(int pointIndex, double priority)
			{
				elements.Add(new PriorityQueueNode { PointIndex = pointIndex, Priority = priority });
				int c = elements.Count - 1;
				while (c > 0)
				{
					int p = (c - 1) / 2;
					if (elements[c].CompareTo(elements[p]) >= 0)
						break;
					PriorityQueueNode tmp = elements[c];
					elements[c] = elements[p];
					elements[p] = tmp;
					c = p;
				}
			}

			public int Dequeue()
			{
				int bestIndex = elements[0].PointIndex;
				int last = elements.Count - 1;
				elements[0] = elements[last];
				elements.RemoveAt(last);

				int p = 0;
				while (true)
				{
					int c = p * 2 + 1;
					if (c >= elements.Count)
						break;
					int right = c + 1;
					if (right < elements.Count && elements[right].CompareTo(elements[c]) < 0)
						c = right;
					if (elements[p].CompareTo(elements[c]) <= 0)
						break;
					PriorityQueueNode tmp = elements[p];
					elements[p] = elements[c];
					elements[c] = tmp;
					p = c;
				}

				return bestIndex;
			}
		}

		/// <summary>
		/// Calculates shortest walkable path between (startX, startY) and (targetX, targetY) on NavRegion.
		/// </summary>
		public List<SRCoord> FindPath(NavRegion region, float startX, float startY, float targetX, float targetY)
		{
			if (region == null || region.Points == null || region.Points.Length == 0)
				return null;

			int startNode = region.FindNearestPointIndex(startX, startY);
			int targetNode = region.FindNearestPointIndex(targetX, targetY);

			if (startNode == -1 || targetNode == -1)
				return null;

			if (startNode == targetNode)
			{
				return new List<SRCoord> { new SRCoord(targetX, targetY) };
			}

			int totalPoints = region.Points.Length;
			double[] gScore = new double[totalPoints];
			for (int i = 0; i < totalPoints; i++) gScore[i] = double.MaxValue;

			int[] cameFrom = new int[totalPoints];
			for (int i = 0; i < totalPoints; i++) cameFrom[i] = -1;

			bool[] closedSet = new bool[totalPoints];

			SimplePriorityQueue openSet = new SimplePriorityQueue();
			gScore[startNode] = 0;
			openSet.Enqueue(startNode, Heuristic(region.Points[startNode], region.Points[targetNode]));

			bool found = false;
			int maxIterations = 50000;
			int iterations = 0;

			while (openSet.Count > 0 && iterations++ < maxIterations)
			{
				int current = openSet.Dequeue();

				if (current == targetNode)
				{
					found = true;
					break;
				}

				if (closedSet[current])
					continue;
				closedSet[current] = true;

				NavPoint currentPt = region.Points[current];
				int[] neighbors = region.Neighbors != null && current < region.Neighbors.Length ? region.Neighbors[current] : null;

				if (neighbors == null || neighbors.Length == 0)
					continue;

				for (int n = 0; n < neighbors.Length; n++)
				{
					int neighbor = neighbors[n];
					if (neighbor < 0 || neighbor >= totalPoints || closedSet[neighbor])
						continue;

					NavPoint neighborPt = region.Points[neighbor];
					double tentativeG = gScore[current] + currentPt.DistanceTo(neighborPt);

					if (tentativeG < gScore[neighbor])
					{
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentativeG;
						double f = tentativeG + Heuristic(neighborPt, region.Points[targetNode]);
						openSet.Enqueue(neighbor, f);
					}
				}
			}

			if (!found)
				return null;

			// Reconstruct raw node path
			List<NavPoint> rawPath = new List<NavPoint>();
			int curr = targetNode;
			while (curr != -1)
			{
				rawPath.Add(region.Points[curr]);
				curr = cameFrom[curr];
			}
			rawPath.Reverse();

			// Smooth path
			List<NavPoint> smoothedPath = SmoothPath(rawPath, region);

			// Convert to SRCoord list
			List<SRCoord> waypoints = new List<SRCoord>();
			for (int i = 0; i < smoothedPath.Count; i++)
			{
				waypoints.Add(new SRCoord(smoothedPath[i].X, smoothedPath[i].Y));
			}

			// Ensure exact target is the final waypoint
			if (waypoints.Count > 0)
			{
				SRCoord last = waypoints[waypoints.Count - 1];
				if (last.DistanceTo(new SRCoord(targetX, targetY)) > 1.0)
				{
					waypoints.Add(new SRCoord(targetX, targetY));
				}
			}

			return waypoints;
		}

		private static double Heuristic(NavPoint a, NavPoint b)
		{
			double dx = a.X - b.X;
			double dy = a.Y - b.Y;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>
		/// Simplifies the node chain by skipping unnecessary collinear nodes.
		/// </summary>
		private List<NavPoint> SmoothPath(List<NavPoint> path, NavRegion region)
		{
			if (path.Count <= 2)
				return path;

			List<NavPoint> smoothed = new List<NavPoint>();
			smoothed.Add(path[0]);

			int currentIndex = 0;
			while (currentIndex < path.Count - 1)
			{
				// Step forward as far as possible without exceeding maximum waypoint step distance (~20m)
				int nextIndex = currentIndex + 1;
				for (int lookAhead = currentIndex + 2; lookAhead < path.Count; lookAhead++)
				{
					double dist = path[currentIndex].DistanceTo(path[lookAhead]);
					if (dist <= 25.0)
					{
						nextIndex = lookAhead;
					}
					else
					{
						break;
					}
				}

				smoothed.Add(path[nextIndex]);
				currentIndex = nextIndex;
			}

			return smoothed;
		}
	}
}
