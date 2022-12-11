using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class DamageTests : BaseModifierTests
	{
		[Test]
		public void SelfInit_Damage()
		{
			var recipe = Recipes.GetRecipe("InitDamage");

			Unit.TryAddModifier(recipe, Unit); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TargetInit_Damage()
		{
			var recipe = Recipes.GetRecipe("InitDamage");

			Enemy.TryAddModifier(recipe, Unit); //Init

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}