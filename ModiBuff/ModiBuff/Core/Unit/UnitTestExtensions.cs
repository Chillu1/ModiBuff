namespace ModiBuff.Core
{
	internal static class UnitTestExtensions
	{
		internal static bool TryAddModifierSelf(this IModifierOwner unit, string name)
		{
			return unit.TryAddModifier(ModifierIdManager.GetIdOld(name), unit);
		}

		internal static bool TryAddModifierTarget(this IModifierOwner unit, string name, IUnit target)
		{
			return unit.ModifierController.TryAdd(ModifierIdManager.GetIdOld(name), target, unit);
		}

		internal static bool ContainsModifier(this IModifierOwner unit, string name)
		{
			return unit.ModifierController.Contains(ModifierIdManager.GetIdOld(name));
		}

		internal static bool AddApplierModifier(this IModifierOwner unit, IModifierRecipe recipe, ApplierType applierType)
		{
			return unit.ModifierController.TryAddApplier(recipe.Id, recipe.HasApplyChecks, applierType);
		}

		internal static float AttackN(this IAttacker<float> unit, IUnit target, int n)
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