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
			AddRecipes(add => add("InitDamage_CostHealth")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init));

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostHealth"), ApplierType.Attack);

			Unit.Attack(Unit);

			Assert.AreEqual(UnitHealth - UnitDamage - 5 - 5, Unit.Health);
		}

		[Test]
		public void CostHealth_NotLethal()
		{
			AddRecipes(add => add("InitDamage_CostHealth")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init));

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostHealth"), ApplierType.Attack);

			Unit.TakeDamage(UnitHealth - 1, Unit);
			Unit.Attack(Enemy); //Shouldn't activate, because the Unit would die

			Assert.AreEqual(1, Unit.Health);
		}

		[Test]
		public void CostMana()
		{
			AddRecipes(add => add("InitDamage_CostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init));

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostMana"), ApplierType.Attack);

			Unit.Attack(Unit);

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
		}

		[Test]
		public void CostMana_NotEnough()
		{
			AddRecipes(add => add("InitDamage_CostMana")
				.ApplyCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init));

			Unit.AddApplierModifier(Recipes.GetGenerator("InitDamage_CostMana"), ApplierType.Attack);

			Unit.UseMana(UnitMana - 1);
			Unit.TakeDamage(UnitHealth - 1, Unit);
			Unit.Attack(Enemy); //Shouldn't activate, because the Unit doesn't have enough mana

			Assert.AreEqual(1, Unit.Mana);
		}

		[Test]
		public void CostMana_Effect()
		{
			AddRecipes(add => add("InitDamage_CostManaEffect")
				.EffectCost(CostType.Mana, 5)
				.Effect(new DamageEffect(5), EffectOn.Init));

			Unit.AddModifierSelf("InitDamage_CostManaEffect");

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void CostHealth_HealSelf()
		{
			AddRecipes(add => add("InitDamage_CostHealth_HealSelf")
				.ApplyCost(CostType.Health, 5)
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new HealEffect(5), EffectOn.Init, Targeting.SourceSource));

			var generator = Recipes.GetGenerator("InitDamage_CostHealth_HealSelf");

			Unit.AddApplierModifier(generator, ApplierType.Cast);

			ModifierOwnerExtensions.TryCast(Unit, generator.Id, Enemy);

			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
			Assert.AreEqual(UnitHealth, Unit.Health);
		}
	}
}