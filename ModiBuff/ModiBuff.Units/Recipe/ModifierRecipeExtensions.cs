namespace ModiBuff.Core.Units
{
	public static class ModifierRecipeExtensions
	{
		public static ModifierRecipe Tag(this ModifierRecipe recipe, TagType tag)
		{
			return recipe.Tag(tag.ToInternalTag());
		}

		public static ModifierRecipe LegalTarget(this ModifierRecipe recipe, LegalTarget target)
		{
			recipe.RemoveTag((Core.TagType)TagType.LegalTargetAll);
			return recipe.Tag(target.ToTagType());
		}

		public static ModifierRecipe CustomStack(this ModifierRecipe recipe, CustomStackEffectOn customStackEffectOn)
		{
			recipe.Tag(Core.TagType.CustomStack);
			return recipe.ModifierAction(ModifierAction.Stack, customStackEffectOn.ToEffectOn());
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, ConditionType conditionType)
		{
			return recipe.EffectCheck(unit => conditionType.CheckConditionType(unit));
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, StatType statType, float statValue,
			ComparisonType comparisonType = ComparisonType.GreaterOrEqual)
		{
			return recipe.EffectCheck(unit => statType.CheckStatType(unit, comparisonType, statValue));
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, LegalAction legalAction)
		{
			return recipe.EffectCheck(unit => legalAction.CheckLegalAction(unit));
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, StatusEffectType statusEffectType)
		{
			return recipe.EffectCheck(unit => statusEffectType.CheckStatusEffectType(unit));
		}

		public static ModifierRecipe EffectCondition(this ModifierRecipe recipe, string modifierName)
		{
			int? modifierId = recipe.IdManager.GetId(modifierName);
			return recipe.EffectCheck(unit => modifierId.CheckModifierId(unit));
		}

		public static ModifierRecipe EffectCooldown(this ModifierRecipe recipe, float cooldown)
		{
			return recipe.EffectCheck(new CooldownCheck(cooldown));
		}

		public static ModifierRecipe EffectChance(this ModifierRecipe recipe, float chance)
		{
			if (chance > 1)
				chance /= 100;
			if (chance <= 0 || chance > 1)
				Logger.LogError("[ModiBuff.Units] Chance must be between 0 and 1");
			return recipe.EffectCheck(new ChanceCheck(chance));
		}

		public static ModifierRecipe EffectCost(this ModifierRecipe recipe, CostType costType, float cost)
		{
			return recipe.EffectCheck(new CostCheck(costType, cost));
		}

		public static ModifierRecipe CallbackUnit(this ModifierRecipe recipe, CallbackUnitType callbackType)
		{
			return recipe.CallbackUnit(callbackType);
		}
	}
}