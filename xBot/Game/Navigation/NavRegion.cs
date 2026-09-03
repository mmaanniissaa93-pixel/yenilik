using System;

namespace xBot.Game.Navigation
{
	public class NavRegion
	{
		public int RegionId { get; set; }
		public string SourceFile { get; set; }
		public NavPoint[] Points { get; set; }
		public int[][] Neighbors { get; set; }

		public float MinX { get; set; }
		public float MaxX { get; set; }
		public float MinY { get; set; }
		public float MaxY { get; set; }

		public bool Contains(float x, float y, float padding = 15f)
		{
			return x >= (MinX - padding) && x <= (MaxX + padding) &&
			       y >= (MinY - padding) && y <= (MaxY + padding);
		}

		public int FindNearestPointIndex(float x, float y)
		{
			if (Points == null || Points.Length == 0)
				return -1;

			int bestIndex = -1;
			double minDistance = double.MaxValue;

			for (int i = 0; i < Points.Length; i++)
			{
				double d = Points[i].DistanceTo(x, y);
				if (d < minDistance)
				{
					minDistance = d;
					bestIndex = i;
				}
			}
			return bestIndex;
		}
	}
}
