using System;
using System.Collections.Generic;
using xBot.Game.Objects.Common;

namespace xBot.Game.Navigation
{
	public class AStarPathfinder
	{
		private struct PriorityQueueNode : IComparable<PriorityQueueNode>
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

			// Snap distance check: if start or target is too far from any node in this region,
			// they do not belong to this region (e.g. separated by river / on another map)
			NavPoint startPt = region.Points[startNode];
			NavPoint targetPt = region.Points[targetNode];
			double startDistSq = startPt.DistanceSquaredTo(startX, startY);
			double targetDistSq = targetPt.DistanceSquaredTo(targetX, targetY);

			// Snap toleransı: mesh aralığı ~2-8m iken 45/55m çok genişti; vadinin
			// karşısındaki dağın mesh'ine yapışıp duvarın içinden rota üretiyordu.
			// 25m yeterli; tutmazsa null dönüp üst katman (multi-region/teleport)
			// veya net hata mesajı devreye girer (takılıp sekme yerine).
			if (startDistSq > 25.0 * 25.0 || targetDistSq > 30.0 * 30.0)
			{
				// Start or Target is not on this walkable mesh!
				return null;
			}

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
			double initialH = Heuristic(region.Points[startNode], region.Points[targetNode]);
			openSet.Enqueue(startNode, initialH);

			bool found = false;
			int maxIterations = 200000;
			int iterations = 0;

			int closestNodeToTarget = startNode;
			double minH = initialH;

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
				double currentH = Heuristic(currentPt, region.Points[targetNode]);
				if (currentH < minH)
				{
					minH = currentH;
					closestNodeToTarget = current;
				}

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
			{
				// Target could not be reached on this landmass/region! Do NOT walk to water edge!
				return null;
			}

			int endNode = targetNode;

			// Reconstruct node path
			List<NavPoint> rawPath = new List<NavPoint>();
			int curr = endNode;
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
				waypoints.Add(new SRCoord(smoothedPath[i].X, smoothedPath[i].Y, (int)smoothedPath[i].Z));
			}

			// If target was found, ensure exact target is the final waypoint
			if (found && waypoints.Count > 0)
			{
				SRCoord last = waypoints[waypoints.Count - 1];
				if (last.DistanceTo(new SRCoord(targetX, targetY)) > 1.0)
				{
					waypoints.Add(new SRCoord(targetX, targetY, (int)region.Points[targetNode].Z));
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
		/// Simplifies the node chain by skipping unnecessary collinear nodes,
		/// BUT only when the straight shortcut stays on the walkable mesh.
		/// Eski kod sadece mesafeye (25m) bakıp ara düğümleri atlıyordu; viraj/dağ
		/// etrafından dolaşan ham yolu düz çizgiyle kesip duvarın içinden geçiriyordu.
		/// Bu yüzden karakter waypoint'e giderken takılıp geri çıkıyordu.
		/// Yeni kod: kestirme en fazla 15m olur, aradaki ham düğümlerin hepsi
		/// kestirme çizgisine en fazla ~4.5m uzakta olmalı (koridor testi) ve Z
		/// farkı küçük olmalı (uçurum/duvar kesmesi engellenir).
		/// </summary>
		private List<NavPoint> SmoothPath(List<NavPoint> path, NavRegion region)
		{
			if (path.Count <= 2)
				return path;

			const double MaxStep = 15.0;
			const double MaxDeviation = 4.5;
			const double MaxZDiff = 4.0;

			List<NavPoint> smoothed = new List<NavPoint>();
			smoothed.Add(path[0]);

			int currentIndex = 0;
			while (currentIndex < path.Count - 1)
			{
				int nextIndex = currentIndex + 1;
				for (int lookAhead = currentIndex + 2; lookAhead < path.Count; lookAhead++)
				{
					double dist = path[currentIndex].DistanceTo(path[lookAhead]);
					if (dist > MaxStep)
						break;

					// Z sıçraması varsa (kat/duvar) kestirme yapma
					if (System.Math.Abs((int)path[currentIndex].Z - (int)path[lookAhead].Z) > MaxZDiff)
						break;

					// Koridor testi: aradaki tüm ham düğümler kestirme çizgisine yakın olmalı.
					// Virajda/dönemeçte ara düğüm çizgiden uzaklaşır -> kestirme reddedilir,
					// viraj korunur ve karakter duvara girmez.
					bool insideCorridor = true;
					for (int k = currentIndex + 1; k < lookAhead; k++)
					{
						double dev = DistancePointToSegment(path[k], path[currentIndex], path[lookAhead]);
						if (dev > MaxDeviation)
						{
							insideCorridor = false;
							break;
						}
						if (System.Math.Abs((int)path[k].Z - (int)path[currentIndex].Z) > MaxZDiff)
						{
							insideCorridor = false;
							break;
						}
					}

					if (insideCorridor)
						nextIndex = lookAhead;
					else
						break; // Daha uzak düğüm daha da sapacağı için dur
				}

				smoothed.Add(path[nextIndex]);
				currentIndex = nextIndex;
			}

			return smoothed;
		}

		/// <summary>
		/// Point-to-segment distance in 2D (X/Y plane).
		/// </summary>
		private static double DistancePointToSegment(NavPoint p, NavPoint a, NavPoint b)
		{
			double dx = b.X - a.X;
			double dy = b.Y - a.Y;
			double lenSq = dx * dx + dy * dy;
			if (lenSq < 1e-8)
				return p.DistanceTo(a.X, a.Y);
			double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq;
			if (t < 0.0) t = 0.0;
			else if (t > 1.0) t = 1.0;
			double projX = a.X + t * dx;
			double projY = a.Y + t * dy;
			double ex = p.X - projX;
			double ey = p.Y - projY;
			return System.Math.Sqrt(ex * ex + ey * ey);
		}
	}
}
