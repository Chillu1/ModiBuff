using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	[Flags]
	public enum TagType
	{
		None = 0,
		IntervalIgnoresStatusResistance = Core.TagType.IntervalIgnoresStatusResistance,
		DurationIgnoresStatusResistance = Core.TagType.DurationIgnoresStatusResistance,
	}

	public static class TagTypeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasTag(this TagType tagType, TagType tag)
		{
			return (tagType & tag) == tag;
		}
	}
}