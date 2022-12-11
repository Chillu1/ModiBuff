using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class StackTests : BaseModifierTests
	{
		[Test]
		public void Stack_Damage()
		{
			var recipe = Recipes.GetRecipe("StackDamage");

			Unit.TryAddModifier(recipe, Unit);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryAddModifier(recipe, Unit);
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}
	}
}