using System;

namespace xBot.Game.Navigation
{
	public struct NavPoint
	{
		public int Index;
		public float X;
		public float Y;
		public ushort Z;
		public ushort EdgeFlag;

		public NavPoint(int index, float x, float y, ushort z, ushort edgeFlag)
		{
			Index = index;
			X = x;
			Y = y;
			Z = z;
			EdgeFlag = edgeFlag;
		}

		public double DistanceTo(float targetX, float targetY)
		{
			double dx = X - targetX;
			double dy = Y - targetY;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		public double DistanceTo(NavPoint other)
		{
			double dx = X - other.X;
			double dy = Y - other.Y;
			return Math.Sqrt(dx * dx + dy * dy);
		}
	}
}
