using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class IntervalTests : BaseModifierTests
	{
		[Test]
		public void Init_DoT()
		{
			var recipe = Recipes.GetRecipe("InitDoT");
			Unit.TryAddModifier(recipe, Unit); //Init

			Assert.AreEqual(UnitHealth - 10, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 10 * 2, Unit.Health);
		}
	}
}