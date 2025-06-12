using System;

namespace ModiBuff.Core
{
	public readonly struct ModifierReference : IEquatable<ModifierReference>, IComparable<ModifierReference>
	{
		public readonly int Id;
		public readonly int? GenId;

		public ModifierReference(int id, int? genId = null)
		{
			Id = id;
			GenId = genId;
		}

		public bool Equals(ModifierReference other)
		{
			return Id == other.Id && GenId == other.GenId;
		}

		public override bool Equals(object? obj)
		{
			return obj is ModifierReference other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + Id.GetHashCode();
				hash = hash * 23 + (GenId?.GetHashCode() ?? 0);
				return hash;
			}
		}

		public int CompareTo(ModifierReference other)
		{
			int idComparison = Id.CompareTo(other.Id);
			if (idComparison != 0) return idComparison;
			return Nullable.Compare(GenId, other.GenId);
		}
	}
}