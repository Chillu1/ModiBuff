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
	}
}