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

		private void SaveLoadState(ModifierRecipes saveRecipes)
		{
			string jsonRecipeState = _saveController.Save(RecipeState.SaveState(saveRecipes));
			var loadData = _saveController.Load<RecipeState.SaveData>(jsonRecipeState);
			IdManager.Clear();
			ModifierRecipes.SetInstance(Recipes);
			RecipeState.LoadState(loadData, Recipes);
		}

		[Test]
		public void SaveRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);

			SaveLoadState(saveRecipes);
			Setup();

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void SaveNamesRecipeLoad()
		{
			const string name = "InitDamage";
			const string displayName = "Damage mod";
			const string description = "Damages the target on init by 5";
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add(name, displayName, description)
				.Effect(new DamageEffect(5), EffectOn.Init);

			SaveLoadState(saveRecipes);
			Setup();

			Unit.AddModifierSelf("InitDamage");
			int id = IdManager.GetId(name);
			var modifierInfo = Recipes.GetModifierInfo(id);
			Assert.AreEqual(modifierInfo.InternalName, name);
			Assert.AreEqual(modifierInfo.DisplayName, displayName);
			Assert.AreEqual(modifierInfo.Description, description);
		}

		[Test]
		public void SaveStackRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("StackDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add, 2f, Targeting.TargetSource),
					EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			SaveLoadState(saveRecipes);
			Setup();

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5 - 2 - 5 - 2 - 2, Unit.Health);
		}

		[Test]
		public void SaveAddDamageRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("InitAddDamage")
				.Effect(new AddDamageEffect(5), EffectOn.Init);

			SaveLoadState(saveRecipes);
			Setup();

			Unit.AddModifierSelf("InitAddDamage");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void SaveIntervalRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("IntervalDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(5);

			SaveLoadState(saveRecipes);
			Setup();

			Unit.AddModifierSelf("IntervalDamage");
			Unit.Update(5);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		//[Test]
		public void SaveModifierActionRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("IntervalStackDamage")
				.Interval(1)
				.ModifierAction(ModifierAction.Stack, EffectOn.Interval)
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5), EffectOn.Stack);

			SaveLoadState(saveRecipes);
			Setup();

			Unit.AddModifierSelf("IntervalStackDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}
	}
}