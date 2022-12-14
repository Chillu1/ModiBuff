using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class CostTests : BaseModifierTests
	{
		[Test]
		public void InitDamage_CostHealth()
		{
			Unit.AddApplierModifiers(Recipes.GetRecipe("InitDamage_CostHealth"));

			Unit.Attack(Unit);

			Assert.AreEqual(UnitHealth - UnitDamage - 5 - 5, Unit.Health);
		}

		[Test]
		public void InitDamage_CostHealth_NotLethal()
		{
			Unit.AddApplierModifiers(Recipes.GetRecipe("InitDamage_CostHealth"));

			Unit.TakeDamage(UnitHealth - 1, Unit);
			Unit.Attack(Enemy); //Shouldn't activate, because the Unit would die

			Assert.AreEqual(1, Unit.Health);
		}
	}
}