using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class PostEffectTests : ModifierTests
	{
		[Test]
		public void LifeSteal_OnDamageEffectInit()
		{
			AddRecipe("InitDamageLifeStealPost")
				.Effect(new DamageEffect(5)
					.SetPostEffects(new LifeStealPostEffect(0.5f, Targeting.SourceTarget)), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageLifeStealPost");
			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Unit.TakeDamage(2.5f, Unit);

			Unit.TryCast(generator.Id, Enemy);

			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void AddDamage_OnKill_WithDamageEffectInit()
		{
			AddRecipe("InitDamageAddDamageOnKillPost")
				.Effect(new DamageEffect(5)
					.SetPostEffects(new AddDamageOnKillPostEffect(2, Targeting.SourceTarget)), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamageAddDamageOnKillPost");
			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Enemy.TakeDamage(EnemyHealth - 5, Unit);

			Unit.TryCast(generator.Id, Enemy);

			Assert.AreEqual(UnitDamage + 2, Unit.Damage);
			Assert.AreEqual(0, Enemy.Health);
		}

		[Test]
		public void HealTargetDamageSelf()
		{
			AddRecipe("HealDamageSelfPost")
				.Effect(new HealEffect(5)
					.SetPostEffects(new DamagePostEffect(Targeting.SourceTarget)), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("HealDamageSelfPost");
			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Enemy.TakeDamage(5, Enemy);

			Unit.TryCast(generator.Id, Enemy);

			Assert.AreEqual(EnemyHealth, Enemy.Health);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void ReverseValueOnFullHealthPostEffectCondition()
		{
			AddRecipe("InitDamageLifestealOnNotFullMana")
				.Effect(new DamageEffect(5)
						.SetPostEffects(new LifeStealPostEffect(2f)
							.Condition<LifeStealPostEffect>(new ValueFull(StatTypeCondition.Mana, true))),
					EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamageLifestealOnNotFullMana");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.UseMana(5);
			Unit.AddModifierSelf("InitDamageLifestealOnNotFullMana");
			Assert.AreEqual(UnitHealth, Unit.Health);
		}
	}
}