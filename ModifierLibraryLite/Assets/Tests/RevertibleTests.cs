using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class RevertibleTests : BaseModifierTests
	{
		[Test]
		public void Init_AddDamage_Remove_RevertDamage()
		{
			var recipe = Recipes.GetRecipe("InitAddDamageRevertible");
			Unit.TryAddModifier(recipe, Unit);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			Unit.Update(5);

			Assert.AreEqual(UnitDamage, Unit.Damage);
		}
	}
}