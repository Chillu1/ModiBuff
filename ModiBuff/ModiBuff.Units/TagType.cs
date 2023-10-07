using System;

namespace ModiBuff.Core.Units
{
	[Flags]
	public enum TagType
	{
		DurationStatusResistance = 1,
	}

	public static class TagTypeExtensions
	{
		public static bool HasTag(this TagType tagType, TagType tag)
		{
			return (tagType & tag) == tag;
		}
	}
}