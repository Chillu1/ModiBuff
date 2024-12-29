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

		[Test]
		public void MultipleConditionalMetaEffectsWithTwoConditionalEffects()
		{
			var metaEffects = new[]
			{
				//new AddValueMetaEffect2(5).Condition(new StatusEffect(StatusEffectType.Root)),
				new AddValueMetaEffect(5).ConditionMeta(new StatusEffect(StatusEffectType.Root)),
				new MultiplyValueMetaEffect(2).ConditionMeta(new StatusEffect(StatusEffectType.Silence))
			};
			AddRecipe("AddFlatOnRooted_MultiplyOnSilenced_HealOnDisarmed")
				.Effect(new DamageEffect(5)
					.Condition(new StatusEffect(StatusEffectType.Disarm, true))
					.SetMetaEffects(metaEffects), EffectOn.Init)
				.Effect(new HealEffect(5)
					.Condition(new StatusEffect(StatusEffectType.Disarm))
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

		//[Test]//TODO
		public void ApplyDoTIfTargetIsFlammablePostEffectCondition()
		{
			//TODO
			//A way to make modifiers react/do stuff based on other unit state, aka a replacment for addable post/meta modifiers
			// So ex. using target tags, or getting some other state, activating post/meta effects on an effect. So we dont have have to have stateful post/meta modifiers with either adding or enabling them
			AddRecipe("DoT")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(1)
				.Remove(5).Refresh();

			/*AddRecipe("FlammableDebuff")
				.Effect(new DebuffEffect(DebuffType.Flammable), EffectOn.Init)
				.Remove(5).Refresh();
			AddRecipe("FlamingAttack")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(2)
						//.Condition<ApplyPostEffect>(new Debuff(DebuffType.Flammable, true))
						//Or
						.SetPostEffects(new ApplyPostEffect("DoT")
							.Condition<ApplyPostEffect>(new Debuff(DebuffType.Flammable, true))),
					EffectOn.Interval)
				.Interval(1)
				.Remove(5).Refresh();

			AddRecipe("FlamingAttack2")
				.Effect(new ApplierEffect("FireDmg"), EffectOn.Init)
				.EffectCondition(ConditionType.Flammable) //Or like this? Whats better?
				.Effect(new ApplierEffect("FireDoT"), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("FlamingAttack");
			Unit.Update(5);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.AddModifierSelf("FlammableDebuff");*/
		}
	}
}