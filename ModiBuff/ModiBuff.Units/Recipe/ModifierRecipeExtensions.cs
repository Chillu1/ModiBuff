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

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, ConditionType conditionType)
		{
			return recipe.ApplyCheck(unit => conditionType.CheckConditionType(unit));
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, StatType statType, float statValue,
			ComparisonType comparisonType = ComparisonType.GreaterOrEqual)
		{
			return recipe.ApplyCheck(unit => statType.CheckStatType(unit, comparisonType, statValue));
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, LegalAction legalAction)
		{
			return recipe.ApplyCheck(unit => legalAction.CheckLegalAction(unit));
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, StatusEffectType statusEffectType)
		{
			return recipe.ApplyCheck(unit => statusEffectType.CheckStatusEffectType(unit));
		}

		public static ModifierRecipe ApplyCondition(this ModifierRecipe recipe, string modifierName)
		{
			int modifierId = recipe.IdManager.GetId(modifierName);
			return recipe.ApplyCheck(unit => modifierId.CheckModifierId(unit));
		}

		/// <summary>
		///		Cooldown set for when we can try to apply the modifier to a target.
		/// </summary>
		public static ModifierRecipe ApplyCooldown(this ModifierRecipe recipe, float cooldown)
		{
			return recipe.ApplyCheck(new CooldownCheck(cooldown));
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
			return recipe.ApplyCheck(new ChanceCheck(chance));
		}

		/// <summary>
		///		Cost for when we can try to apply the modifier to a target.
		/// </summary>
		public static ModifierRecipe ApplyCost(this ModifierRecipe recipe, CostType costType, float cost)
		{
			return recipe.ApplyCheck(new CostCheck(costType, cost));
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
			int modifierId = recipe.IdManager.GetId(modifierName);
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
				Logger.LogError("Chance must be between 0 and 1");
			return recipe.EffectCheck(new ChanceCheck(chance));
		}

		public static ModifierRecipe EffectCost(this ModifierRecipe recipe, CostType costType, float cost)
		{
			return recipe.EffectCheck(new CostCheck(costType, cost));
		}

		public static ModifierRecipe Callback(this ModifierRecipe recipe, CallbackType callbackType)
		{
			return recipe.CallbackEffect(callbackType);
		}

		public static ModifierRecipe Event(this ModifierRecipe recipe, EffectOnEvent @event)
		{
			return recipe.Event(@event);
		}
	}
}