namespace ModiBuff.Core.Units
{
	public static class ModifierRecipeExtensions
	{
		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, ConditionType conditionType)
		{
			recipe.ApplyCheck(unit => conditionType.CheckConditionType(unit));
			return recipe;
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, StatType statType, float statValue,
			ComparisonType comparisonType = ComparisonType.GreaterOrEqual)
		{
			recipe.ApplyCheck(unit => statType.CheckStatType(unit, comparisonType, statValue));
			return recipe;
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, LegalAction legalAction)
		{
			recipe.ApplyCheck(unit => legalAction.CheckLegalAction(unit));
			return recipe;
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, StatusEffectType statusEffectType)
		{
			recipe.ApplyCheck(unit => statusEffectType.CheckStatusEffectType(unit));
			return recipe;
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, string modifierName)
		{
			int modifierId = recipe.IdManager.GetId(modifierName);
			recipe.ApplyCheck(unit => modifierId.CheckModifierId(unit));
			return recipe;
		}

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

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, ConditionType conditionType)
		{
			recipe.EffectCheck(unit => conditionType.CheckConditionType(unit));
			return recipe;
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, StatType statType, float statValue,
			ComparisonType comparisonType = ComparisonType.GreaterOrEqual)
		{
			recipe.EffectCheck(unit => statType.CheckStatType(unit, comparisonType, statValue));
			return recipe;
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, LegalAction legalAction)
		{
			recipe.EffectCheck(unit => legalAction.CheckLegalAction(unit));
			return recipe;
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, StatusEffectType statusEffectType)
		{
			recipe.EffectCheck(unit => statusEffectType.CheckStatusEffectType(unit));
			return recipe;
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, string modifierName)
		{
			int modifierId = recipe.IdManager.GetId(modifierName);
			recipe.EffectCheck(unit => modifierId.CheckModifierId(unit));
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