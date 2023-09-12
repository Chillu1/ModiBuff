using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class RefreshTests : ModifierTests
	{
		[Test]
		public void NoRefresh()
		{
			AddRecipes(add => add("DurationRemove")
				.Remove(5));

			const string recipeId = "DurationRemove";

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.False(Unit.ContainsModifier(recipeId));
		}

		[Test]
		public void Refresh_Duration()
		{
			AddRecipes(add => add("DurationRefreshRemove")
				.Remove(5).Refresh());

			const string recipeId = "DurationRefreshRemove";

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));
		}

		[Test]
		public void Refresh_Interval()
		{
			AddRecipes(add => add("IntervalRefreshRemove")
				.Effect(new RemoveEffect(), EffectOn.Interval)
				.Interval(5).Refresh());

			const string recipeId = "IntervalRefreshRemove";

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));
		}

		[Test]
		public void Refresh_DurationNotInterval()
		{
			AddRecipes(add => add("DurationRefreshRemove_IntervalDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(5)
				.Remove(5).Refresh());

			const string recipeId = "DurationRefreshRemove_IntervalDamage";

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.True(Unit.ContainsModifier(recipeId));

			Unit.AddModifierSelf(recipeId);
			Unit.Update(4);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.True(Unit.ContainsModifier(recipeId));
		}
	}
}