using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class RefreshTests : BaseModifierTests
	{
		[Test]
		public void NoRefresh()
		{
			var recipe = Recipes.GetRecipe("DurationRemove");

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipe));

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.False(Unit.ContainsModifier(recipe));
		}

		[Test]
		public void Refresh_Duration()
		{
			var recipe = Recipes.GetRecipe("DurationRefreshRemove");

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipe));

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipe));
		}

		[Test]
		public void Refresh_Interval()
		{
			var recipe = Recipes.GetRecipe("IntervalRefreshRemove");

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipe));

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipe));
		}

		[Test]
		public void Refresh_DurationNotInterval()
		{
			var recipe = Recipes.GetRecipe("DurationRefreshRemove_IntervalDamage");

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipe));

			Unit.TryAddModifier(recipe, Unit);
			Unit.Update(4);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.True(Unit.ContainsModifier(recipe));
		}
	}
}