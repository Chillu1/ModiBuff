using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CostTests : ModifierTests
	{
		[Test]
		public void CostHealth()
		{
			AddRecipe("InitDamage_CostHealth")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostHealth"), ApplierType.Attack);

			Unit.Attack(Unit);

			Assert.AreEqual(UnitHealth - UnitDamage - 5 - 5, Unit.Health);
		}

		[Test]
		public void CostHealth_NotLethal()
		{
			AddRecipe("InitDamage_CostHealth")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostHealth"), ApplierType.Attack);

			Unit.TakeDamage(UnitHealth - 1, Unit);
			Unit.Attack(Enemy); //Shouldn't activate, because the Unit would die

			Assert.AreEqual(1, Unit.Health);
		}

		[Test]
		public void CostMana()
		{
			AddRecipe("InitDamage_CostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostMana"), ApplierType.Attack);

			Unit.Attack(Unit);

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
		}

		[Test]
		public void CostMana_NotEnough()
		{
			AddRecipe("InitDamage_CostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostMana"), ApplierType.Attack);

			Unit.UseMana(UnitMana - 1);
			Unit.TakeDamage(UnitHealth - 1, Unit);
			Unit.Attack(Enemy); //Shouldn't activate, because the Unit doesn't have enough mana

			Assert.AreEqual(1, Unit.Mana);
		}

		[Test]
		public void CostMana_Effect()
		{
			AddRecipe("InitDamage_CostManaEffect")
				.EffectCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_CostManaEffect");

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void CostHealth_HealSelf()
		{
			AddRecipe("InitDamage_CostHealth_HealSelf")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new HealEffect(5, targeting: Targeting.SourceSource), EffectOn.Init);
			Setup();

			var generator = Recipes.GetGenerator("InitDamage_CostHealth_HealSelf");

			Unit.AddApplierModifier(generator, ApplierType.Cast);

			Unit.TryCast(generator.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void CostSixtyPercentHealth_Damage()
		{
			AddRecipe("InitDamage_CostSixtyPercentHealth")
				.ApplyCostPercent(CostType.Health, 0.6f)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostSixtyPercentHealth"), ApplierType.Cast);

			Unit.TryCast("InitDamage_CostSixtyPercentHealth", Enemy);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
			Assert.AreEqual(UnitHealth - UnitHealth * 0.6f, Unit.Health);

			Unit.TryCast("InitDamage_CostSixtyPercentHealth", Enemy);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
			Assert.AreEqual(UnitHealth - UnitHealth * 0.6f, Unit.Health);
		}

		[Test]
		public void TripleEffect_SharedCostTwoEffects()
		{
			AddRecipe("InitStackIntervalDamage_CostMana")
				.EffectCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			Setup();

			Unit.AddModifierSelf("InitStackIntervalDamage_CostMana");
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
			Assert.AreEqual(UnitMana - 5, Unit.Mana);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5 - 5, Unit.Health);
			Assert.AreEqual(UnitMana - 5 - 5, Unit.Mana);
		}
	}
}