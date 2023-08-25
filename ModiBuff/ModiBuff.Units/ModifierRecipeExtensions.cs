namespace ModiBuff.Core.Units
{
	public static class ModifierRecipeExtensions
	{
		/// <summary>
		///		Cost for when we can try to apply the modifier to a target.
		/// </summary>
		public static ModifierRecipe ApplyCost(this ModifierRecipe recipe, CostType costType, float cost)
		{
			recipe.ApplyCheck(new CostCheck(costType, cost));
			return recipe;
		}

		public static ModifierRecipe EffectCost(this ModifierRecipe recipe, CostType costType, float cost)
		{
			recipe.EffectCheck(new CostCheck(costType, cost));
			return recipe;
		}
	}
}