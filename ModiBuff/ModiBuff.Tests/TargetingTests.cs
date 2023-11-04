using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class TargetingTests : ModifierTests
	{
		[Test]
		public void SelfInit_Damage()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void SelfEffect_Damage()
		{
			AddEffect("5Damage", new DamageEffect(5f));
			Setup();

			Unit.ApplyEffectSelf("5Damage");
			Assert.AreEqual(UnitHealth - 5f, Unit.Health);
		}

		[Test]
		public void TargetInit_Damage()
		{
			Setup();

			Enemy.AddModifierTarget("InitDamage", Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TargetEffect_Damage()
		{
			AddEffect("5Damage", new DamageEffect(5f));
			Setup();

			Enemy.ApplyEffectTarget("5Damage", Unit);
			Assert.AreEqual(UnitHealth - 5f, Unit.Health);
		}

		[Test]
		public void InitSelfHeal_DamageTarget()
		{
			AddRecipe("InitSelfHeal_DamageTarget")
				.Effect(new HealEffect(5, targeting: Targeting.SourceTarget), EffectOn.Init)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.TakeDamage(5, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierTarget("InitSelfHeal_DamageTarget", Enemy);

			Assert.AreEqual(UnitHealth, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void ThornsDamage_PostEffectHealTarget()
		{
			AddRecipe("ThornsDamage_PostEffectHealTarget")
				.Effect(new DamageEffect(5, targeting: Targeting.SourceTarget)
					.SetPostEffects(new LifeStealPostEffect(1f)), EffectOn.Event)
				.Event(EffectOnEvent.AfterAttacked);
			Setup();

			Unit.AddModifierSelf("ThornsDamage_PostEffectHealTarget");

			Enemy.Attack(Unit);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
			Assert.AreEqual(UnitHealth - EnemyDamage + 5, Unit.Health);
		}
	}
}