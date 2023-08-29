using System;

namespace ModiBuff.Core
{
	public readonly struct ModifierReference : IEquatable<ModifierReference>, IComparable<ModifierReference>
	{
		public readonly int Id;
		public readonly int GenId;

		public ModifierReference(int id, int genId)
		{
			Id = id;
			GenId = genId;
		}

		public bool Equals(ModifierReference other)
		{
			return Id == other.Id && GenId == other.GenId;
		}

		public override bool Equals(object obj)
		{
			return obj is ModifierReference other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Id * 397) ^ GenId;
			}
		}

		public int CompareTo(ModifierReference other)
		{
			int idComparison = Id.CompareTo(other.Id);
			if (idComparison != 0) return idComparison;
			return GenId.CompareTo(other.GenId);
		}
	}
}