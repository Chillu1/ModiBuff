namespace ModiBuff.Core
{
	public static class UnitExtensions
	{
		/// <summary>
		///		For unit tests only.
		/// </summary>
		internal static bool TryAddModifierSelf(this IUnit unit, string id)
		{
			return unit.TryAddModifier(ModifierIdManager.GetId(id), unit);
		}

		internal static bool TryAddModifierTarget(this IUnit unit, string id, IUnit target)
		{
			return unit.TryAddModifierTarget(ModifierIdManager.GetId(id), target, unit);
		}
	}
}