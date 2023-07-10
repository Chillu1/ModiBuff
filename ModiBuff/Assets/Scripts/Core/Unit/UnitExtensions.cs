namespace ModiBuff.Core
{
	public static class UnitExtensions
	{
		/// <summary>
		///		For unit tests only.
		/// </summary>
		internal static bool TryAddModifierSelf(this IModifierOwner unit, string id)
		{
			return unit.TryAddModifier(ModifierIdManager.GetId(id), (IUnit)unit);
		}

		internal static bool TryAddModifierTarget(this IModifierOwner unit, string id, IUnit target)
		{
			return unit.ModifierController.TryAdd(ModifierIdManager.GetId(id), target, (IUnit)unit);
		}

		internal static float AttackN(this IAttacker unit, IUnit target, int n)
		{
			float totalDamage = 0;
			for (int i = 0; i < n; i++)
				totalDamage += unit.Attack(target);
			return totalDamage;
		}

		internal static float HealN(this IHealer unit, IHealable target, int n)
		{
			float totalHeal = 0;
			for (int i = 0; i < n; i++)
				totalHeal += unit.Heal(target);
			return totalHeal;
		}
	}
}