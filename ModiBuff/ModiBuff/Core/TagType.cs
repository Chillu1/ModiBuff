using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	/// <summary>
	///		Core internal modifier tags, can be combined with user tags.
	/// </summary>
	[Flags]
	public enum TagType
	{
		None = 0,
		IntervalIgnoresStatusResistance = 1,
		DurationIgnoresStatusResistance = 2,
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