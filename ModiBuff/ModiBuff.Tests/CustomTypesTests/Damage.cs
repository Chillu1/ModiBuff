using System;

namespace ModiBuff.Tests.CustomTypesTests
{
	public struct Damage : IComparable, IComparable<Damage>
	{
		public float Value;

		public Damage(float value) => Value = value;

		public static readonly Damage Null;
		static Damage() => Null = new Damage(0);

		public int CompareTo(object obj)
		{
			if (obj is Damage damage)
				return CompareTo(damage);

			throw new ArgumentException("Object is not a Damage");
		}

		public int CompareTo(Damage other) => Value.CompareTo(other.Value);

		public static Damage operator +(Damage a, Damage b) => new Damage(a.Value + b.Value);

		public static implicit operator Damage(float value) => new Damage(value);
		public static implicit operator Damage(double value) => new Damage((float)value);
	}
}