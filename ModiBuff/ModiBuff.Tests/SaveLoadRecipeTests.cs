using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Extensions.Serialization.Json;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class SaveLoadRecipeTests : ModifierTests
	{
		private SaveController _saveController;

		public override void OneTimeSetup()
		{
			SkipInitDamageRecipe = true;
			base.OneTimeSetup();
		}

		public override void IterationSetup()
		{
			base.IterationSetup();
			_saveController = new SaveController("fullSave.json");
		}

		//[Test]
		public void SaveRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager);
			saveRecipes.Add("InitDamage2")
				.Effect(new DamageEffect(5), EffectOn.Init);

			string jsonRecipeState = _saveController.Save(RecipeState.SaveState(saveRecipes));
			Logger.Log(jsonRecipeState);
			var loadData = _saveController.Load<RecipeState.SaveData>(jsonRecipeState);
			RecipeState.LoadState(loadData, Recipes);

			Setup();

			Unit.AddModifierSelf("InitDamage2");
			//Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void SaveRecipeIntervalLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager);
			saveRecipes.Add("InitDamage2")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(5);

			string jsonRecipeState = _saveController.Save(RecipeState.SaveState(saveRecipes));
			Logger.Log(jsonRecipeState);
			var loadData = _saveController.Load<RecipeState.SaveData>(jsonRecipeState);
			Recipes.TempRegisterEffects(new Func<float, IEffect>[] { damage => new DamageEffect(damage) });
			IdManager.Clear();
			ModifierRecipes.SetInstance(Recipes);
			RecipeState.LoadState(loadData, Recipes);

			Setup();

			Unit.AddModifierSelf("InitDamage2");
			Unit.Update(5);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}