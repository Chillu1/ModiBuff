using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	/// <summary>
	///		Core internal modifier tags, can be combined with user tags.
	/// </summary>
	[Flags]
	public enum TagType : ulong
	{
		None = 0,
		IntervalIgnoresStatusResistance = 1ul << 0,
		DurationIgnoresStatusResistance = 1ul << 1,

		//Most likely need to reserve around 8-16 bits, or split internal and user tagging
		//Will be decided/fixed on 1.0 release
		Reserved1 = 1ul << 2,
		Reserved2 = 1ul << 3,
		Reserved3 = 1ul << 4,
		Reserved4 = 1ul << 5,
		Reserved5 = 1ul << 6,
		Reserved6 = 1ul << 7,
		Reserved7 = 1ul << 8,
		Reserved8 = 1ul << 9,
		Reserved9 = 1ul << 10,
		Reserved10 = 1ul << 11,
		Reserved11 = 1ul << 12,
		Reserved12 = 1ul << 13,
		Reserved13 = 1ul << 14,
		Reserved14 = 1ul << 15,
		Reserved15 = 1ul << 16,

		LastReserved = Reserved15
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