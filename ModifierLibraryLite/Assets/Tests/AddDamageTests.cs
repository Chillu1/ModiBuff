using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class AddDamageTests : BaseModifierTests
	{
		[Test]
		public void Init_AddDamage()
		{
			var recipe = Recipes.GetRecipe("InitAddDamage");

			Unit.TryAddModifier(recipe, Unit);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}