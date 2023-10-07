using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	/// <summary>
	///		Core internal modifier tags, can be combined with user tags.
	/// </summary>
	[Flags]
	public enum TagType : uint //ulong?
	{
		None = 0,
		IntervalIgnoresStatusResistance = 1 << 0,
		DurationIgnoresStatusResistance = 1 << 1,

		//Most likely need to reserve around 8 bits, or split internal and user tagging
		Reserved1 = 1 << 2,
		Reserved2 = 1 << 3,
		Reserved3 = 1 << 4,
		Reserved4 = 1 << 5,
		Reserved5 = 1 << 6,
		Reserved6 = 1 << 7,
		Reserved7 = 1 << 8,

		LastReserved = Reserved7
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