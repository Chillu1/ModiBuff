using System;

namespace ModiBuff.Core.Units
{
	public readonly struct Vector2 : IEquatable<Vector2>, IComparable<Vector2>
	{
		public readonly float X;
		public readonly float Y;

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static Vector2 Zero => new Vector2();

		public float DistanceTo(Vector2 other)
		{
			return (float)Math.Sqrt(DistanceToSquared(other));
		}

		public float Distance()
		{
			return (float)Math.Sqrt(X * X + Y * Y);
		}

		public float DistanceToSquared(Vector2 other)
		{
			float dx = X - other.X;
			float dy = Y - other.Y;
			return dx * dx + dy * dy;
		}

		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X + b.X, a.Y + b.Y);
		}

		public static Vector2 operator *(Vector2 a, float b)
		{
			return new Vector2(a.X * b, a.Y * b);
		}

		public static Vector2 Abs(Vector2 a)
		{
			return new Vector2(Math.Abs(a.X), Math.Abs(a.Y));
		}

		public bool Equals(Vector2 other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector2 other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X.GetHashCode() * 397) ^ Y.GetHashCode();
			}
		}

		public int CompareTo(Vector2 other)
		{
			int xComparison = X.CompareTo(other.X);
			return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
		}
	}

	public static class Vector2Extensions
	{
		public static Vector2 Abs(this Vector2 a)
		{
			return new Vector2(Math.Abs(a.X), Math.Abs(a.Y));
		}
	}
}