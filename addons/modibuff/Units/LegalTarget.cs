using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	[Flags]
	public enum LegalTarget : ulong
	{
		None = 0,
		Self = TagType.LegalTargetSelf,
		Ally = TagType.LegalTargetAlly,
		Enemy = TagType.LegalTargetEnemy,

		//Structure = 1ul << 3,
		//Units = Ally | Enemy,
		All = Self | Ally | Enemy // | Structure
	}

	public static class LegalTargetingExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasTarget(this LegalTarget one, LegalTarget two)
		{
			return (one & two) == two;
		}

		public static TagType ToTagType(this LegalTarget legalTarget)
		{
			switch (legalTarget)
			{
				case LegalTarget.Self:
					return TagType.LegalTargetSelf;
				case LegalTarget.Ally:
					return TagType.LegalTargetAlly;
				case LegalTarget.Enemy:
					return TagType.LegalTargetEnemy;
				case LegalTarget.All:
					return TagType.LegalTargetAll;
				default:
					throw new ArgumentOutOfRangeException(nameof(legalTarget), legalTarget, null);
			}
		}
	}
}