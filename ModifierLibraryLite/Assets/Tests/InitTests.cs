using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class InitTests : BaseModifierTests
	{
		[Test]
		public void InitDamage()
		{
			Unit.TryAddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void InitDamage_InitTwice_DamageTwice()
		{
			var modifier = Recipes.GetRecipe("InitDamage");

			Unit.TryAddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}
	}
}