namespace ModiBuff.Core.Units
{
	public static class ModifierIdExtensions
	{
		public static bool IsLegalTarget(this int modifierId, IUnitEntity target, IUnitEntity source)
		{
			var tag = (TagType)ModifierRecipes.GetTag(modifierId);
			if (tag.HasTag(TagType.LegalTargetSelf) && target == source)
				return true;

			return tag.IsLegalTarget(target.UnitType, source.UnitType);
		}
	}
}