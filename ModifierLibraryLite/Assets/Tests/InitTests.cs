using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class InitTests : BaseModifierTests
	{
		[Test]
		public void InitDamage()
		{
			var modifier = Recipes.GetRecipe("InitDamage");

			Unit.TryAddModifier(modifier, Unit);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void InitDamage_InitTwice_DamageTwice()
		{
			var modifier = Recipes.GetRecipe("InitDamage");

			Unit.TryAddModifier(modifier, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifier(modifier, Unit);
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}
	}
}