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
			AddRecipe("DurationRemove")
				.Remove(5);
			Setup();

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
			AddRecipe("DurationRefreshRemove")
				.Remove(5).Refresh();
			Setup();

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
			AddRecipe("IntervalRefreshRemove")
				.Effect(new RemoveEffect(), EffectOn.Interval)
				.Interval(5).Refresh();
			Setup();

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
			AddRecipe("DurationRefreshRemove_IntervalDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(5)
				.Remove(5).Refresh();
			Setup();

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