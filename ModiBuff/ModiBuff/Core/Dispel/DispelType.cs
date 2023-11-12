using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	[Flags]
	public enum DispelType
	{
		None,
		Interval = 1,
		Duration = 2,
		Stack = 4,
		Basic = 8,
		Strong = 16,

		Time = Interval | Duration,

		All = Interval | Duration | Stack | Basic | Strong
	}

	public static class DispelTypeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasAny(this DispelType dispelType, DispelType dispel)
		{
			return (dispelType & dispel) != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has(this DispelType dispelType, DispelType dispel)
		{
			return (dispelType & dispel) == dispel;
		}
	}
}