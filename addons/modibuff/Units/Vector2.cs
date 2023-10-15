using System;

namespace ModiBuff.Core.Units
{
	public struct Vector2
	{
		public float X;
		public float Y;

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static Vector2 Zero => new Vector2() { X = 0f, Y = 0f };

		public float DistanceTo(Vector2 other)
		{
			return (float)Math.Sqrt(DistanceToSquared(other));
		}

		public float DistanceToSquared(Vector2 other)
		{
			float dx = X - other.X;
			float dy = Y - other.Y;
			return dx * dx + dy * dy;
		}
	}
}