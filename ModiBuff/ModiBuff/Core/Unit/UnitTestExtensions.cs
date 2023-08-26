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
			return unit.ModifierController.TryAddApplier(recipe.Id, ((IModifierApplyCheckRecipe)recipe).HasApplyChecks, applierType);
		}
	}
}