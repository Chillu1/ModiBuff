namespace ModifierLibraryLite.Core
{
	public static class UnitExtensions
	{
		/// <summary>
		///		For unit tests only.
		/// </summary>
		internal static bool TryAddModifierSelf(this IUnit unit, string id)
		{
			return unit.TryAddModifier(ModifierIdManager.GetId(id), unit, unit);
		}

		internal static bool TryAddModifier(this IUnit unit, string id, IUnit target)
		{
			return unit.TryAddModifier(ModifierIdManager.GetId(id), target, unit);
		}
	}
}