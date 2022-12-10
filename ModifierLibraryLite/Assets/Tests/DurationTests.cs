using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class DurationTests : BaseModifierTests
	{
		[Test]
		public void Duration_Damage()
		{
			var modifier = Recipes.Get("DurationDamage");
			Unit.TryAddModifier(modifier, Unit);

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Duration_Remove()
		{
			var modifier = Recipes.Get("DurationRemove");
			Unit.TryAddModifier(modifier, Unit);

			Unit.Update(5);

			Assert.True(!Unit.ContainsModifier(modifier));
		}
	}
}