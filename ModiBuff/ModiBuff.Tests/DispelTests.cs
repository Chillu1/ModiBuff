using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class DispelTests : ModifierTests
	{
		[Test]
		public void DispelDoT()
		{
			AddRecipe("DoTDispellable")
				.Dispel()
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			Setup();

			Unit.AddModifierSelf("DoTDispellable");
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Dispel(DispelType.Time, Unit);
			Unit.Update(0);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DispelDoTDuration()
		{
			AddRecipe("DoTDispellableDuration")
				.Dispel()
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DoTDispellableDuration");
			Unit.Dispel(DispelType.Stack, Unit);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Dispel(DispelType.Duration, Unit);
			Unit.Update(0);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void BasicDispel()
		{
			AddRecipe("BasicDispellable")
				.Dispel()
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("BasicDispellable");

			Unit.Dispel(DispelType.Basic, Unit);
			Unit.Update(0);
			Assert.False(Unit.ContainsModifier("BasicDispellable"));
		}

		[Test]
		public void StrongDispel()
		{
			AddRecipe("StrongDispellable")
				.Dispel(DispelType.Strong)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("StrongDispellable");

			Unit.Dispel(DispelType.Basic, Unit);
			Unit.Update(0);
			Assert.True(Unit.ContainsModifier("StrongDispellable"));

			Unit.Dispel(DispelType.Strong, Unit);
			Unit.Update(0);
			Assert.False(Unit.ContainsModifier("StrongDispellable"));
		}

		[Test]
		public void IntervalStrongDispel()
		{
			AddRecipe("StrongDispellable")
				.Dispel(DispelType.Strong)
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval);
			Setup();

			Unit.AddModifierSelf("StrongDispellable");

			Unit.Dispel(DispelType.Basic, Unit);
			Unit.Update(1);
			Assert.True(Unit.ContainsModifier("StrongDispellable"));
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Dispel(DispelType.Strong, Unit);
			Unit.Update(0);
			Unit.Update(1);
			Assert.False(Unit.ContainsModifier("StrongDispellable"));
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}