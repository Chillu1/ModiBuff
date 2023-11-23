using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	/// <summary>
	///		User tag type, is combined with internal tags.
	/// </summary>
	[Flags]
	public enum TagType : ulong
	{
		Default = Core.TagType.Default | LegalTargetAll,

		None = Core.TagType.None,
		IsInit = Core.TagType.IsInit,
		IsRefresh = Core.TagType.IsRefresh,
		IsStack = Core.TagType.IsStack,
		IntervalIgnoresStatusResistance = Core.TagType.IntervalIgnoresStatusResistance,
		DurationIgnoresStatusResistance = Core.TagType.DurationIgnoresStatusResistance,
		CustomStack = Core.TagType.CustomStack,
		LastReserved = Core.TagType.LastReserved,

		LegalTargetSelf = 1ul << 18,
		LegalTargetAlly = 1ul << 19,
		LegalTargetEnemy = 1ul << 20,

		//LegalTargetStructure = 1ul << 20,
		//LegalTargetUnits = LegalTargetAlly | LegalTargetEnemy,
		LegalTargetAll = LegalTargetSelf | LegalTargetAlly | LegalTargetEnemy, // | LegalTargetStructure,
		BasicDispel = 1ul << 21,
		StrongDispel = 1ul << 22,
		UserTag7 = 1ul << 23,
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

		public static bool IsLegalTarget(this TagType tag, UnitType target, UnitType source)
		{
			if (tag.HasTag(TagType.LegalTargetAlly) && target == source)
				return true;
			if (tag.HasTag(TagType.LegalTargetEnemy) && target != source)
				return true;
			if (tag.HasTag(TagType.LegalTargetAll))
				return true;

#if DEBUG
			Logger.Log(
				$"[ModiBuff.Units] Tag {tag} is not a legal target for UnitType.{target} from UnitType.{source}");
#endif
			return false;
		}
	}
}