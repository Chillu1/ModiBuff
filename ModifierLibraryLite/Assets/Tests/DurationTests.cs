using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class DurationTests : BaseModifierTests
	{
		[Test]
		public void Duration_Damage()
		{
			var recipe = Recipes.GetRecipe("DurationDamage");
			Unit.TryAddModifier(recipe, Unit);

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Duration_Remove()
		{
			var recipe = Recipes.GetRecipe("DurationRemove");
			Unit.TryAddModifier(recipe, Unit);

			Unit.Update(5);

			Assert.False(Unit.ContainsModifier(recipe));
		}
	}
}