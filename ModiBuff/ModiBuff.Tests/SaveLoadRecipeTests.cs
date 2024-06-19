using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Extensions.Serialization.Json;
using NUnit.Framework;
using TagType = ModiBuff.Core.TagType;

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

		private void SaveLoadStateAndSetup(ModifierRecipes saveRecipes)
		{
			string jsonRecipeState = _saveController.Save(RecipeState.SaveState(saveRecipes));
			var loadData = _saveController.Load<RecipeState.SaveData>(jsonRecipeState);
			IdManager.Clear();
			ModifierRecipes.SetInstance(Recipes);
			RecipeState.LoadState(loadData, Recipes);
			Setup();
		}

		[Test]
		public void SaveRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

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

			SaveLoadStateAndSetup(saveRecipes);

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

			SaveLoadStateAndSetup(saveRecipes);

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

			SaveLoadStateAndSetup(saveRecipes);

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

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("IntervalDamage");
			Unit.Update(5);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void SaveModifierActionRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("IntervalStackDamage")
				.Interval(1)
				.ModifierAction(ModifierAction.Stack, EffectOn.Interval)
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5), EffectOn.Stack);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("IntervalStackDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("IntervalStackDamage");
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5 - 5, Unit.Health);
		}

		[Test]
		public void SaveRemoveRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("RemoveDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Remove(1);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("RemoveDamage");
			Assert.True(Unit.ContainsModifier("RemoveDamage"));
			Unit.Update(1);
			Assert.False(Unit.ContainsModifier("RemoveDamage"));
		}

		[Test]
		public void SaveRefreshIntervalRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("RefreshIntervalDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(1)
				.Refresh();

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("RefreshIntervalDamage");
			Unit.Update(0.5f);
			Unit.AddModifierSelf("RefreshIntervalDamage");
			Unit.Update(0.5f);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void SaveRefreshDurationRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("RefreshDuration")
				.Remove(1)
				.Refresh();

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("RefreshDuration");
			Unit.Update(0.5f);
			Unit.AddModifierSelf("RefreshDuration");
			Unit.Update(0.5f);
			Assert.True(Unit.ContainsModifier("RefreshDuration"));
			Unit.Update(0.5f);
			Assert.False(Unit.ContainsModifier("RefreshDuration"));
		}

		[Test]
		public void SaveRemoveStackRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("RemoveStack")
				.Stack(WhenStackEffect.OnMaxStacks, 2)
				.Remove(RemoveEffectOn.Stack);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("RemoveStack");
			Assert.True(Unit.ContainsModifier("RemoveStack"));
			Unit.AddModifierSelf("RemoveStack");
			Assert.False(Unit.ContainsModifier("RemoveStack"));
		}

		[Test]
		public void SaveDispelRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DispelDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(1)
				.Dispel(DispelType.Strong);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DispelDamage");
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Dispel(DispelType.Strong, Unit);
			Unit.Update(0);
			Unit.Update(1);
			Assert.False(Unit.ContainsModifier("DispelDamage"));
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void SaveTagRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("TagDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Tag(TagType.DurationIgnoresStatusResistance);

			SaveLoadStateAndSetup(saveRecipes);

			int id = IdManager.GetId("TagDamage");
			Assert.True(ModifierRecipes.GetTag(id).HasTag(TagType.DurationIgnoresStatusResistance));
		}

		[Test]
		public void SaveRemoveTagRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("RemoveTagDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.RemoveTag(TagType.Default);

			SaveLoadStateAndSetup(saveRecipes);

			int id = IdManager.GetId("RemoveTagDamage");
			Assert.False(ModifierRecipes.GetTag(id).HasTag(TagType.Default));
		}

		[Test]
		public void SaveSetTagRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("SetTagDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.SetTag(TagType.DurationIgnoresStatusResistance);

			SaveLoadStateAndSetup(saveRecipes);

			int id = IdManager.GetId("SetTagDamage");
			Assert.True(ModifierRecipes.GetTag(id).HasTag(TagType.DurationIgnoresStatusResistance));
			Assert.False(ModifierRecipes.GetTag(id).HasTag(TagType.Default));
		}
	}
}