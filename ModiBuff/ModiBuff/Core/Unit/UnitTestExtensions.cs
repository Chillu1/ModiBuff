namespace ModiBuff.Core
{
	internal static class UnitTestExtensions
	{
		internal static void AddModifierSelf(this IModifierOwner unit, string name)
		{
			unit.AddModifier(ModifierIdManager.GetIdOld(name), unit);
		}

		internal static void AddModifierTarget(this IModifierOwner unit, string name, IUnit target)
		{
			unit.ModifierController.Add(ModifierIdManager.GetIdOld(name), target, unit);
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