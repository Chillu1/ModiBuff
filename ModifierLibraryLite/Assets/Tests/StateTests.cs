using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class StateTests : BaseModifierTests
	{
		[Test]
		public void TwoDurationModifiers_DifferentState()
		{
			var recipe1 = Recipes.GetRecipe("DurationDamage");
			var recipe2 = Recipes.GetRecipe("DurationDamage");
			Unit.TryAddModifier(recipe1, Unit);
			Enemy.TryAddModifier(recipe2, Enemy);

			Unit.Update(5);
			Enemy.Update(2);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void TwoInitModifiers_DifferentState()
		{
			var recipe1 = Recipes.GetRecipe("InitDamage");
			var recipe2 = Recipes.GetRecipe("InitDamage");

			Unit.TryAddModifier(recipe1, Unit);
			Enemy.TryAddModifier(recipe2, Enemy);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}