using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class RefreshTests : BaseModifierTests
	{
		[Test]
		public void NoRefresh()
		{
			string recipeId = "DurationRemove";

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.False(Unit.ContainsModifier(recipeId));
		}

		[Test]
		public void Refresh_Duration()
		{
			string recipeId = "DurationRefreshRemove";

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));
		}

		[Test]
		public void Refresh_Interval()
		{
			string recipeId = "IntervalRefreshRemove";

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));
		}

		[Test]
		public void Refresh_DurationNotInterval()
		{
			string recipeId = "DurationRefreshRemove_IntervalDamage";

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.TryAddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.True(Unit.ContainsModifier(recipeId));
		}
	}
}