using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	/// <summary>
	///		User tag type, is combined with internal tags.
	/// </summary>
	[Flags]
	public enum TagType : uint
	{
		None = Core.TagType.None,
		IntervalIgnoresStatusResistance = Core.TagType.IntervalIgnoresStatusResistance,
		DurationIgnoresStatusResistance = Core.TagType.DurationIgnoresStatusResistance,
		LastReserved = Core.TagType.LastReserved,
		UserTag1 = 1 << 9,
		UserTag2 = 1 << 10,
	}

	public static class TagTypeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasTag(this TagType tagType, TagType tag)
		{
			return (tagType & tag) == tag;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModiBuff.Core.TagType ToInternalTag(this TagType tagType)
		{
			return (ModiBuff.Core.TagType)tagType;
		}
	}
}