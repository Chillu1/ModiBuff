using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class MetaEffectTests : ModifierTests
	{
		[Test]
		public void DamageBasedOnHealth()
		{
			AddRecipe("InitDamageValueBasedOnStatMeta")
				.Effect(new DamageEffect(5)
					.SetMetaEffects(new StatPercentMetaEffect(StatType.Health, Targeting.SourceTarget)), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageValueBasedOnStatMeta");
			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Unit.TryCast(generator.Id, Enemy); //5 * 1

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(UnitHealth / 2f, Unit);

			Unit.TryCast(generator.Id, Enemy); //5 * 0.5

			Assert.AreEqual(EnemyHealth - 5 - 2.5f, Enemy.Health);
		}

		[Test]
		public void DamageBasedOnHealthAndMana()
		{
			AddRecipe("InitDamageValueBasedOnHealthAndManaMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(
							new StatPercentMetaEffect(StatType.Health, Targeting.SourceTarget),
							new StatPercentMetaEffect(StatType.Mana, Targeting.SourceTarget)),
					EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageValueBasedOnHealthAndManaMeta");
			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Unit.TryCast(generator.Id, Enemy); //5 * 1

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.UseMana(UnitMana / 2f);

			Unit.TryCast(generator.Id, Enemy); //5 * 0.5 * 0.5

			Assert.AreEqual(EnemyHealth - 5 - 1.25f, Enemy.Health);
		}

		[Test]
		public void CanCastHalfMulti_IsStunnedDoubleMulti()
		{
			AddRecipe("InitDamageValueBasedOnStatusEffectMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(
							new LegalActionMetaEffect(0.5f, LegalAction.Cast, false),
							new LegalActionMetaEffect(2f, LegalAction.Act, false)),
					EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageValueBasedOnStatusEffectMeta");
			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Unit.TryCast(generator.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Enemy.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Disarm, 1f, Unit);

			Unit.TryCast(generator.Id, Enemy); //5 * 2f
			Assert.AreEqual(EnemyHealth - 5 - 10f, Enemy.Health);

			Enemy.Update(1f);
			Enemy.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Silence, 1f, Unit);

			Unit.TryCast(generator.Id, Enemy); //5 * 0.5f
			Assert.AreEqual(EnemyHealth - 5 - 10f - 2.5f, Enemy.Health);
		}

		[Test]
		public void DoubleMultiplierWhenSilenced()
		{
			AddRecipe("InitDamageValue2XWhenDisarmedMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(new LegalActionMetaEffect(2f, LegalAction.Act, false, Targeting.SourceTarget)),
					EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageValue2XWhenDisarmedMeta");
			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Unit.TryCast(generator.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.StatusEffectController.ChangeStatusEffect(0, 0, StatusEffectType.Disarm, 1f, Enemy);

			Unit.TryCast(generator.Id, Enemy); //5 * 2f
			Assert.AreEqual(EnemyHealth - 5 - 10f, Enemy.Health);
		}

		[Test]
		public void ConditionalEffectsBasedOnManaUsage()
		{
			AddRecipe("InitDamageDynamicEffectValueOnManaSpentMeta")
				.Effect(
					new DamageEffect(0).SetMetaEffects(
						new DynamicEffectBasedOnManaSpentMetaEffect(new[] { (1, 1), (2, 1.5f), (3f, 2f) })),
					EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageDynamicEffectValueOnManaSpentMeta");
			Unit.AddApplierModifier(generator, ApplierType.Cast);
			Unit.UseMana(UnitMana);

			Unit.UseMana(-3);
			Unit.TryCast(generator.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 2, Enemy.Health);
			Assert.AreEqual(0, Unit.Mana);

			Unit.UseMana(-2);
			Unit.TryCast(generator.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 2 - 1.5, Enemy.Health);
			Assert.AreEqual(0, Unit.Mana);

			Unit.UseMana(-1);
			Unit.TryCast(generator.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 2 - 1.5 - 1, Enemy.Health);
			Assert.AreEqual(0, Unit.Mana);

			Unit.TryCast(generator.Id, Enemy);
			Assert.AreEqual(EnemyHealth - 2 - 1.5 - 1, Enemy.Health);
			Assert.AreEqual(0, Unit.Mana);
		}

		[Test]
		public void ReverseValueOnFullHealthMetaEffectCondition()
		{
			AddRecipe("InitHealValueBasedOnStatMeta")
				.Effect(new HealEffect(5)
						.SetMetaEffects(new ReverseValueMetaEffect()
							.Condition(new ValueFull(StatTypeCondition.Health))),
					EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitHealValueBasedOnStatMeta");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("InitHealValueBasedOnStatMeta");
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void ReverseValueOnNotFullHealthMetaEffectCondition()
		{
			AddRecipe("InitDamageValueBasedOnStatMeta")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(new ReverseValueMetaEffect()
							.Condition(new ValueFull(StatTypeCondition.Health, true))),
					EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamageValueBasedOnStatMeta");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("InitDamageValueBasedOnStatMeta");
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void ReverseValueOnNotFullHealthAndHigherThanHalfMetaEffectCondition()
		{
			AddRecipe("InitDamageValueBasedOnStatMeta")
				.Effect(new HealEffect(5)
						.SetMetaEffects(new ReverseValueMetaEffect().Condition(
							new ValueFull(StatTypeCondition.Health, true),
							new ValueComparisonPercent(StatType.Health, ComparisonType.GreaterOrEqual, 0.5f))),
					EffectOn.Init);
			Setup();

			Unit.TakeDamage(5, Unit);

			Unit.AddModifierSelf("InitDamageValueBasedOnStatMeta");
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.AddModifierSelf("InitDamageValueBasedOnStatMeta");
			Assert.AreEqual(UnitHealth - 5 - 5 - UnitHealth / 2f + 5, Unit.Health);
		}

		[Test]
		public void ReversePoisonDamageValueOnNotFullHealthMetaEffectCondition()
		{
			AddRecipe("PoisonValueBasedOnNotFullHealthMeta")
				.Stack(WhenStackEffect.Always)
				.Effect(new PoisonDamageEffect().SetMetaEffects(new ReverseValueMetaEffect()
						.Condition(new ValueFull(StatTypeCondition.Health, true))),
					EffectOn.Interval | EffectOn.Stack)
				.Interval(1)
				.Remove(5).Refresh();
			Setup();


			Unit.AddModifierSelf("PoisonValueBasedOnNotFullHealthMeta");
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}


		[Test]
		public void MetaEffectWithConditionalEffect()
		{
			AddRecipe("DamageAddFlatOnRooted")
				.Effect(new DamageEffect(5)
						.SetMetaEffects(
							new AddValueMetaEffect(5).Condition(new StatusEffectCond(StatusEffectType.Root))
						),
					EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("DamageAddFlatOnRooted");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.ChangeStatusEffect(StatusEffectType.Root, 1, Unit);
			Unit.AddModifierSelf("DamageAddFlatOnRooted");
			Assert.AreEqual(UnitHealth - 5 - 10, Unit.Health);

			Unit.Update(1);

			Unit.ChangeStatusEffect(StatusEffectType.Disarm, 1, Unit);
			Unit.AddModifierSelf("DamageAddFlatOnRooted");
			Assert.AreEqual(UnitHealth - 5 - 10 - 5, Unit.Health);
		}

		[Test]
		public void MultipleConditionalMetaEffectsWithTwoConditionalEffects()
		{
			var metaEffects = new IMetaEffect<float, float>[]
			{
				new AddValueMetaEffect(5).Condition(new StatusEffectCond(StatusEffectType.Root)),
				new MultiplyValueMetaEffect(2).Condition(new StatusEffectCond(StatusEffectType.Silence))
			};
			AddRecipe("AddFlatOnRooted_MultiplyOnSilenced_HealOnDisarmed")
				.Effect(new DamageEffect(5)
					.Condition(new StatusEffectCond(StatusEffectType.Disarm, true))
					.SetMetaEffects(metaEffects), EffectOn.Init)
				.Effect(new HealEffect(5)
					.Condition(new StatusEffectCond(StatusEffectType.Disarm))
					.SetMetaEffects(metaEffects), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("AddFlatOnRooted_MultiplyOnSilenced_HealOnDisarmed");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.ChangeStatusEffect(StatusEffectType.Root, 1, Unit);
			Unit.AddModifierSelf("AddFlatOnRooted_MultiplyOnSilenced_HealOnDisarmed");
			Assert.AreEqual(UnitHealth - 5 - 10, Unit.Health);

			Unit.ChangeStatusEffect(StatusEffectType.Silence, 1, Unit);
			Unit.AddModifierSelf("AddFlatOnRooted_MultiplyOnSilenced_HealOnDisarmed");
			Assert.AreEqual(UnitHealth - 5 - 10 - 20, Unit.Health);

			Unit.ChangeStatusEffect(StatusEffectType.Disarm, 1, Unit);
			Unit.AddModifierSelf("AddFlatOnRooted_MultiplyOnSilenced_HealOnDisarmed");
			Assert.AreEqual(UnitHealth - 5 - 10 - 20 + 20, Unit.Health);

			Unit.Update(1);

			Unit.ChangeStatusEffect(StatusEffectType.Disarm, 1, Unit);
			Unit.AddModifierSelf("AddFlatOnRooted_MultiplyOnSilenced_HealOnDisarmed");
			Assert.AreEqual(UnitHealth - 5 - 10 - 20 + 20 + 5, Unit.Health);
		}

		[Test]
		public void MultipleConditionalMetaEffectsWithAndOr()
		{
			AddRecipe("AddFlatOnRootedAndSilenced_MultiplyOnDisarmedOrFrozen")
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
			Setup();

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

		[Test]
		public void LevelSystemThroughMetaEffects()
		{
			var recipe = AddRecipe("LevelingDamage");
			int id = recipe.Id;

			recipe
				.Effect(new DamageEffect(5)
						.SetMetaEffects(
							new AddValueMetaEffect(5).Condition(new LevelCond(id, 2)),
							new MultiplyValueMetaEffect(2).Condition(new LevelCond(id, 1)))
						.SetPostEffects(new LifeStealPostEffect(0.5f)
							.SetMetaEffects(new AddValueMetaEffect(0.5f).Condition(new LevelCond(id, 5)))
							.Condition(new LevelCond(id, 4)))
					, EffectOn.Init)
				.Effect(new HealEffect(5).Condition(new LevelCond(id, 3)), EffectOn.Init);
			Setup();

			void Action()
			{
				Unit.Heal(Unit.MaxHealth, Unit);
				Unit.AddModifierLevel(id);
				Unit.AddModifierSelf("LevelingDamage");
			}

			Unit.AddModifierSelf("LevelingDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Action();
			Assert.AreEqual(UnitHealth - 10, Unit.Health);

			Action();
			Assert.AreEqual(UnitHealth - 20, Unit.Health);

			Action();
			Assert.AreEqual(UnitHealth - 20 + 5, Unit.Health);

			Unit.Heal(Unit.MaxHealth, Unit);
			Unit.TakeDamage(100, Unit);
			Unit.AddModifierLevel(id);
			Unit.AddModifierSelf("LevelingDamage");
			Assert.AreEqual(UnitHealth - 100 - 20 + 5 + 20 * 0.5f, Unit.Health);

			Unit.Heal(Unit.MaxHealth, Unit);
			Unit.TakeDamage(100, Unit);
			Unit.AddModifierLevel(id);
			Unit.AddModifierSelf("LevelingDamage");
			Assert.AreEqual(UnitHealth - 100 - 20 + 5 + 20 * 1f, Unit.Health);
		}
	}
}