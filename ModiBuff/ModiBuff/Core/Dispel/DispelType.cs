using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	[Flags]
	public enum DispelType
	{
		None,
		Interval,
		Duration,
		Stack,
		Basic,
		Strong,

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