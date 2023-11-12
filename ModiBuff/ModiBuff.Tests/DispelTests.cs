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
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Dispel(DispelType.Interval);
			Setup();

			Unit.AddModifierSelf("DoTDispellable");
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.ModifierController.Dispel(DispelType.Time, Unit, Unit);
			Unit.Update(0);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DispelDoTDuration()
		{
			AddRecipe("DoTDispellableDuration")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Dispel(DispelType.Interval | DispelType.Duration)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DoTDispellableDuration");
			Unit.ModifierController.Dispel(DispelType.Stack, Unit, Unit);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.ModifierController.Dispel(DispelType.Duration, Unit, Unit);
			Unit.Update(0);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}