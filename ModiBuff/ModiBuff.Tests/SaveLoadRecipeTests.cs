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
			RecipeState.LoadState<CallbackUnitType>(loadData, Recipes);
			Setup();
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
			int id = IdManager.GetId(name).Value;
			var modifierInfo = Recipes.GetModifierInfo(id);
			Assert.AreEqual(modifierInfo.InternalName, name);
			Assert.AreEqual(modifierInfo.DisplayName, displayName);
			Assert.AreEqual(modifierInfo.Description, description);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveStackRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("StackDamage")
				.Effect(new DamageEffect(5, false, StackEffectType.Effect | StackEffectType.Add, 2f,
					Targeting.TargetSource), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(UnitHealth - 5 - 2 - 5 - 2 - 2, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
			Unit.Update(0);
			Assert.False(Unit.ContainsModifier("RemoveStack"));
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveRemoveCallbackUnitRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("InitAddDamageRevertibleCallback")
				.Dispel(DispelType.Strong)
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(RemoveEffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongDispel);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("InitAddDamageRevertibleCallback");
			Unit.Dispel(DispelType.Strong, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
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
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveTagRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("TagDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Tag(TagType.DurationIgnoresStatusResistance);

			SaveLoadStateAndSetup(saveRecipes);

			int id = IdManager.GetId("TagDamage").Value;
			Assert.True(ModifierRecipes.GetTag(id).HasTag(TagType.DurationIgnoresStatusResistance));
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveRemoveTagRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("RemoveTagDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.RemoveTag(TagType.Default);

			SaveLoadStateAndSetup(saveRecipes);

			int id = IdManager.GetId("RemoveTagDamage").Value;
			Assert.False(ModifierRecipes.GetTag(id).HasTag(TagType.Default));
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveSetTagRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("SetTagDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.SetTag(TagType.DurationIgnoresStatusResistance);

			SaveLoadStateAndSetup(saveRecipes);

			int id = IdManager.GetId("SetTagDamage").Value;
			Assert.True(ModifierRecipes.GetTag(id).HasTag(TagType.DurationIgnoresStatusResistance));
			Assert.False(ModifierRecipes.GetTag(id).HasTag(TagType.Default));
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveTogglableAddDamageInitRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("TogglableAddDamage")
				.Effect(new AddDamageEffect(5, EffectState.IsTogglable), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("TogglableAddDamage");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.AddModifierSelf("TogglableAddDamage");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectConditionRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageOnFullHealth")
				.Effect(new DamageEffect(5).Condition(new ValueFull(StatTypeCondition.Health)), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DamageOnFullHealth");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("DamageOnFullHealth");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectStatelessMetaAddValueRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageWithAddValueMeta")
				.Effect(new DamageEffect(5).SetMetaEffects(new ReverseValueMetaEffect()), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.TakeDamage(5, Unit);
			Unit.AddModifierSelf("DamageWithAddValueMeta");
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectStatefulMetaAddValueRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageWithAddValueMeta")
				.Effect(new DamageEffect(5).SetMetaEffects(new AddValueMetaEffect(2)), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DamageWithAddValueMeta");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectMetaConditionRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageWithAddValueMetaWithCondition")
				.Effect(new DamageEffect(5).SetMetaEffects(
					new AddValueMetaEffect(2).Condition(new ValueFull(StatTypeCondition.Health))), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DamageWithAddValueMetaWithCondition");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
			Unit.AddModifierSelf("DamageWithAddValueMetaWithCondition");
			Assert.AreEqual(UnitHealth - 5 - 2 - 5, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectStatelessPostDamageRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DoubleDamagePost")
				.Effect(new DamageEffect(5).SetPostEffects(new DamagePostEffect()), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DoubleDamagePost");
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectStatefullAddDamageRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageLifsteal")
				.Effect(new DamageEffect(5).SetPostEffects(new LifeStealPostEffect(0.5f)), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DamageLifsteal");
			Assert.AreEqual(UnitHealth - 5 + 5 * 0.5f, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectPostMetaEffectRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageAddValueMetaInPostEffect")
				.Effect(new DamageEffect(5)
					.SetPostEffects(new DamagePostEffect().SetMetaEffects(new AddValueMetaEffect(2))), EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DamageAddValueMetaInPostEffect");
			Assert.AreEqual(UnitHealth - 5 - 5 - 2, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectPostMetaConditionRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageWithPostAddValueMetaWithCondition")
				.Effect(new DamageEffect(5)
						.SetPostEffects(new DamagePostEffect()
							.SetMetaEffects(new AddValueMetaEffect(2)
								.Condition(new StatusEffectCond(StatusEffectType.Stun)))),
					EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DamageWithPostAddValueMetaWithCondition");
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
			Unit.ChangeStatusEffect(StatusEffectType.Stun, 1f, Unit);
			Unit.AddModifierSelf("DamageWithPostAddValueMetaWithCondition");
			Assert.AreEqual(UnitHealth - 5 - 5 - 5 - 5 - 2, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageEffectMetaMetaRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("DamageWithAddValueMetaMultiplyMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(new AddValueMetaEffect(2f).SetMetaEffects(new MultiplyValueMetaEffect(2f))),
					EffectOn.Init);

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("DamageWithAddValueMetaMultiplyMeta");
			Assert.AreEqual(UnitHealth - 5 - 4, Unit.Health);
		}

		[Test]
#if !MODIBUFF_SYSTEM_TEXT_JSON
		[Ignore("MODIBUFF_SYSTEM_TEXT_JSON not set. Skipping test")]
#endif
		public void SaveDamageMetaEffectAndOrConditionsRecipeLoad()
		{
			var saveRecipes = new ModifierRecipes(IdManager, EffectTypeIdManager);
			saveRecipes.Add("AddFlatOnRootedAndSilenced_MultiplyOnDisarmedOrFrozen")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(
							new AddValueMetaEffect(5).Condition(new AndCondition(
								new StatusEffectCond(StatusEffectType.Root),
								new StatusEffectCond(StatusEffectType.Silence)))
							, new MultiplyValueMetaEffect(2).Condition(new OrCondition(
								new StatusEffectCond(StatusEffectType.Disarm),
								new StatusEffectCond(StatusEffectType.Freeze)
							))),
					EffectOn.Init)
				.Remove(1).Refresh();

			SaveLoadStateAndSetup(saveRecipes);

			Unit.AddModifierSelf("AddFlatOnRootedAndSilenced_MultiplyOnDisarmedOrFrozen");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.ChangeStatusEffect(StatusEffectType.Root, 1, Unit);
			Unit.AddModifierSelf("AddFlatOnRootedAndSilenced_MultiplyOnDisarmedOrFrozen");
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
			Unit.ChangeStatusEffect(StatusEffectType.Silence, 1, Unit);
			Unit.AddModifierSelf("AddFlatOnRootedAndSilenced_MultiplyOnDisarmedOrFrozen");
			Assert.AreEqual(UnitHealth - 5 - 5 - 10, Unit.Health);

			Unit.Update(1);

			Unit.ChangeStatusEffect(StatusEffectType.Freeze, 1, Unit);
			Unit.AddModifierSelf("AddFlatOnRootedAndSilenced_MultiplyOnDisarmedOrFrozen");
			Assert.AreEqual(UnitHealth - 5 - 5 - 10 - 10, Unit.Health);
		}
	}
}