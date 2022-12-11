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

			Assert.False(Unit.ContainsModifier(modifier));
		}

		[Test]
		public void TwoDurationModifiers_DifferentState()
		{
			var modifier1 = Recipes.Get("DurationDamage");
			var modifier2 = Recipes.Get("DurationDamage");
			Unit.TryAddModifier(modifier1, Unit);
			Enemy.TryAddModifier(modifier2, Enemy);

			Unit.Update(5);
			Enemy.Update(2);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}
	}
}