using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CostTests : BaseModifierTests
	{
		[Test]
		public void CostHealth()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage_CostHealth"));

			Unit.Attack(Unit);

			Assert.AreEqual(UnitHealth - UnitDamage - 5 - 5, Unit.Health);
		}

		[Test]
		public void CostHealth_NotLethal()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage_CostHealth"));

			Unit.TakeDamage(UnitHealth - 1, Unit);
			Unit.Attack(Enemy); //Shouldn't activate, because the Unit would die

			Assert.AreEqual(1, Unit.Health);
		}

		[Test]
		public void CostMana()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage_CostMana"), ApplierType.Attack);

			Unit.Attack(Unit);

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
		}

		[Test]
		public void CostMana_NotEnough()
		{
			Unit.AddApplierModifier(Recipes.GetRecipe("InitDamage_CostMana"), ApplierType.Attack);

			Unit.UseMana(UnitMana - 1);
			Unit.TakeDamage(UnitHealth - 1, Unit);
			Unit.Attack(Enemy); //Shouldn't activate, because the Unit doesn't have enough mana

			Assert.AreEqual(1, Unit.Mana);
		}

		[Test]
		public void CostMana_Effect()
		{
			Unit.TryAddModifierSelf("InitDamage_CostManaEffect");

			Assert.AreEqual(UnitMana - 5, Unit.Mana);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}