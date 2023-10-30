using System;

namespace ModiBuff.Core.Units
{
	[Flags]
	public enum UnitTag
	{
		None = 0,
		Lifestealable = 1 << 0,

		Default = Lifestealable,
	}

	public static class UnitTagExtensions
	{
		public static bool HasTag(this UnitTag unitTag, UnitTag tag) => (unitTag & tag) == tag;
	}
}