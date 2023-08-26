using System;
using System.Collections.Generic;
using Godot;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Godot
{
	public abstract class ModifierRecipesGodot : ModifierRecipes
	{
		private const string DefaultPath = "res://resources/modibuff_recipes/";

		private readonly string _path;

		public ModifierRecipesGodot(ModifierIdManager idManager, string path = DefaultPath) : base(idManager)
		{
			_path = path;
		}

		protected override void SetupRecipes()
		{
			SetupResourceRecipes();
		}

		private void SetupResourceRecipes()
		{
			var resources = new List<IModifierRecipeResource>();

			foreach (string file in DirAccess.GetFilesAt(_path))
			{
				var resource = ResourceLoader.Load(_path + file);
				if (resource == null)
					continue;

				var modifierRecipeResource = (IModifierRecipeResource)resource;

				if (string.IsNullOrEmpty(modifierRecipeResource.Name))
					modifierRecipeResource.SetName(file.Replace(".tres", ""));

				resources.Add(modifierRecipeResource);
			}

			foreach (var recipeResource in resources)
			{
				switch (recipeResource)
				{
					case ModifierEventRecipeResource modifierEventRecipeResource:
						AddEventResource(modifierEventRecipeResource);
						break;
					case ModifierRecipeResource modifierRecipeResource:
						AddResource(modifierRecipeResource);
						break;
					case AuraModifierRecipeResource auraModifierRecipeResource:
						AddAuraResource(auraModifierRecipeResource);
						break;
					default:
						GD.PushError($"Resource {recipeResource} is not a valid recipe resource");
						break;
				}
			}
		}

		private void AddResource(ModifierRecipeResource recipeResource)
		{
			if (!recipeResource.Validate())
			{
				GD.PushError($"Recipe {recipeResource.Name} is invalid, skipping...");
				return;
			}

			var recipe = Add(recipeResource.Name);
			Save(recipe, recipeResource);

			//---ApplyConditions---

			if (recipeResource.ApplyCondition != ConditionType.None)
				recipe.ApplyCondition(recipeResource.ApplyCondition);

			if (recipeResource.StatApplyCondition != null)
				recipe.ApplyCondition(recipeResource.StatApplyCondition.StatType, recipeResource.StatApplyCondition.Value,
					recipeResource.StatApplyCondition.ComparisonType);

			if (recipeResource.StatusEffectApplyCondition != StatusEffectType.None)
				recipe.ApplyCondition(recipeResource.StatusEffectApplyCondition);

			if (!string.IsNullOrEmpty(recipeResource.ModifierNameApplyCondition))
				recipe.ApplyCondition(recipeResource.ModifierNameApplyCondition);

			if (recipeResource.ApplyCooldown > 0)
				recipe.ApplyCooldown(recipeResource.ApplyCooldown);

			if (recipeResource.ApplyCost is { CostType: not CostType.None })
				recipe.ApplyCost(recipeResource.ApplyCost.CostType, recipeResource.ApplyCost.Amount);

			if (recipeResource.ApplyChance > 0)
				recipe.ApplyChance(recipeResource.ApplyChance);

			//---EffectConditions---

			if (recipeResource.EffectCondition != ConditionType.None)
				recipe.EffectCondition(recipeResource.EffectCondition);

			if (recipeResource.StatEffectCondition != null)
				recipe.EffectCondition(recipeResource.StatEffectCondition.StatType, recipeResource.StatEffectCondition.Value,
					recipeResource.StatEffectCondition.ComparisonType);

			if (recipeResource.StatusEffectEffectCondition != StatusEffectType.None)
				recipe.EffectCondition(recipeResource.StatusEffectEffectCondition);

			if (!string.IsNullOrEmpty(recipeResource.ModifierNameEffectCondition))
				recipe.EffectCondition(recipeResource.ModifierNameEffectCondition);

			if (recipeResource.EffectCooldown > 0)
				recipe.EffectCooldown(recipeResource.EffectCooldown);

			if (recipeResource.EffectCost is { CostType: not CostType.None })
				recipe.EffectCost(recipeResource.EffectCost.CostType, recipeResource.EffectCost.Amount);

			if (recipeResource.EffectChance > 0)
				recipe.EffectChance(recipeResource.EffectChance);

			//---Actions---

			if (recipeResource.OneTimeInit)
				recipe.OneTimeInit();

			if (recipeResource.Interval > 0)
				recipe.Interval(recipeResource.Interval);

			if (recipeResource.Duration > 0)
				recipe.Duration(recipeResource.Duration);

			if (recipeResource.RemoveDuration > 0)
				recipe.Remove(recipeResource.RemoveDuration);

			if (recipeResource.RefreshType != RefreshType.Invalid)
				recipe.Refresh(recipeResource.RefreshType.GetModiBuffRefreshType());

			if (recipeResource.StackResource != null)
				recipe.Stack(recipeResource.StackResource.WhenStackEffect, recipeResource.StackResource.Value,
					recipeResource.StackResource.MaxStacks, recipeResource.StackResource.EveryXStacks);

			//---Effects---

			foreach (var effectResource in recipeResource.EffectResources)
				recipe.Effect(effectResource.GetEffect(), effectResource.EffectOn, effectResource.Targeting);

			recipeResource.Reset();

			//GD.Print($"Loaded recipe {recipeResource.Name}");
		}

		private void AddEventResource(ModifierEventRecipeResource recipeResource)
		{
			if (!recipeResource.Validate())
			{
				GD.PushError($"Event recipe {recipeResource.Name} is invalid, skipping...");
				return;
			}

			var recipe = AddEvent(recipeResource.Name, recipeResource.EffectOnEvent);
			Save(recipe, recipeResource);

			//---Actions---

			if (recipeResource.RemoveDuration > 0)
				recipe.Remove(recipeResource.RemoveDuration);

			if (recipeResource.Refresh)
				recipe.Refresh();

			//---Effects---

			foreach (var effectResource in recipeResource.EffectResources)
				recipe.Effect(effectResource.GetEffect());


			//GD.Print($"Loaded recipe {recipeResource.Name}");
		}

		private void AddAuraResource(AuraModifierRecipeResource recipeResource)
		{
			if (!recipeResource.Validate())
			{
				GD.PushError($"Aura Recipe {recipeResource.Name} is invalid, skipping...");
				return;
			}

			var recipe = Add(recipeResource.Name);

			//---Actions---

			if (recipeResource.Interval > 0)
				recipe.Interval(recipeResource.Interval);

			//---Effects---

			if (recipeResource.AuraEffectResources != null)
			{
				foreach (var auraEffectModifierRecipeResource in recipeResource.AuraEffectResources)
					AddAuraEffect(auraEffectModifierRecipeResource);
			}

			foreach (var effectResource in recipeResource.EffectResources)
				recipe.Effect(effectResource.GetEffect(), EffectOn.Interval);

			Save(recipe, recipeResource);

			void AddAuraEffect(AuraEffectModifierRecipeResource auraEffectModifierRecipeResource)
			{
				var auraEffectRecipe = Add(auraEffectModifierRecipeResource.Name);
				auraEffectModifierRecipeResource.SetId(auraEffectRecipe.Id);
				if (auraEffectModifierRecipeResource.NeedsSaving)
					recipeResource.SetChanged();

				//---Actions---

				if (auraEffectModifierRecipeResource.OneTimeInit)
					auraEffectRecipe.OneTimeInit();

				if (auraEffectModifierRecipeResource.RemoveDuration > 0)
					auraEffectRecipe.Remove(auraEffectModifierRecipeResource.RemoveDuration).Refresh();

				//---Effects---

				foreach (var effectResource in auraEffectModifierRecipeResource.EffectResources)
					auraEffectRecipe.Effect(effectResource.GetEffect(), effectResource.EffectOn, effectResource.Targeting);
			}
		}

		private static void Save<T>(IModifierRecipe recipe, T recipeResource) where T : BaseModifierRecipeResource
		{
			recipeResource.SetId(recipe.Id);
			if (recipeResource.NeedsSaving)
				ResourceSaver.Save(recipeResource);
		}
	}
}