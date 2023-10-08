namespace ModiBuff.Core.Units
{
	public static class ModifierIdExtensions
	{
		public static bool IsLegalTarget(this int modifierId, UnitType target, UnitType source)
		{
			return ((TagType)ModifierRecipes.GetTag(modifierId)).IsLegalTarget(target, source);
		}
	}
}