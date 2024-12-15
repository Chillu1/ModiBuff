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
							.Condition<ReverseValueMetaEffect>(new ValueFull(StatTypeCondition.Health))),
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
							.Condition<ReverseValueMetaEffect>(new ValueFull(StatTypeCondition.Health, true))),
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
						.SetMetaEffects(new ReverseValueMetaEffect().Condition<ReverseValueMetaEffect>(
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
						.Condition<ReverseValueMetaEffect>(new ValueFull(StatTypeCondition.Health, true))),
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
	}
}