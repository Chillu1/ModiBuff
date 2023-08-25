namespace ModiBuff.Core.Units
{
	public static class ModifierRecipeExtensions
	{
		/// <summary>
		///		Cooldown set for when we can try to apply the modifier to a target.
		/// </summary>
		public static ModifierRecipe ApplyCooldown(this ModifierRecipe recipe, float cooldown)
		{
			recipe.ApplyCheck(new CooldownCheck(cooldown));
			return recipe;
		}

		/// <summary>
		///		When trying to apply a modifier, what should the chance be of it being applied?
		/// </summary>
		public static ModifierRecipe ApplyChance(this ModifierRecipe recipe, float chance)
		{
			if (chance > 1)
				chance /= 100;
			if (chance <= 0 || chance > 1)
				Logger.LogError("Chance must be between 0 and 1");
			recipe.ApplyCheck(new ChanceCheck(chance));
			return recipe;
		}

		/// <summary>
		///		Cost for when we can try to apply the modifier to a target.
		/// </summary>
		public static ModifierRecipe ApplyCost(this ModifierRecipe recipe, CostType costType, float cost)
		{
			recipe.ApplyCheck(new CostCheck(costType, cost));
			return recipe;
		}

		public static ModifierRecipe EffectCooldown(this ModifierRecipe recipe, float cooldown)
		{
			recipe.EffectCheck(new CooldownCheck(cooldown));
			return recipe;
		}

		public static ModifierRecipe EffectChance(this ModifierRecipe recipe, float chance)
		{
			if (chance > 1)
				chance /= 100;
			if (chance <= 0 || chance > 1)
				Logger.LogError("Chance must be between 0 and 1");
			recipe.EffectCheck(new ChanceCheck(chance));
			return recipe;
		}

		public static ModifierRecipe EffectCost(this ModifierRecipe recipe, CostType costType, float cost)
		{
			recipe.EffectCheck(new CostCheck(costType, cost));
			return recipe;
		}
	}
}