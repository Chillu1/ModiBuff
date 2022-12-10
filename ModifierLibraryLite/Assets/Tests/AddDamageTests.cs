using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class AddDamageTests : BaseModifierTests
	{
		[Test]
		public void Init_AddDamage()
		{
			var modifier = Recipes.Get("InitAddDamage");

			Unit.TryAddModifier(modifier, Unit);

			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}