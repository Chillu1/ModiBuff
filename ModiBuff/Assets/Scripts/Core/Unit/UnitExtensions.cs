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

		internal static float AttackN(this IUnit unit, IUnit target, int n)
		{
			float totalDamage = 0;
			for (int i = 0; i < n; i++)
				totalDamage += unit.Attack(target);
			return totalDamage;
		}

		internal static float HealN(this IUnit unit, IUnit target, int n)
		{
			float totalHeal = 0;
			for (int i = 0; i < n; i++)
				totalHeal += unit.Heal(target);
			return totalHeal;
		}
	}
}