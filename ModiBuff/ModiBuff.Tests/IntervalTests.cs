using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class IntervalTests : ModifierTests
	{
		[Test]
		public void IntervalTick_Damage()
		{
			AddRecipe("IntervalTick_Damage")
				.Effect(new DamageEffect(5), EffectOn.IntervalTick)
				.IntervalTick(2);
			Setup();

			Unit.AddModifierSelf("IntervalTick_Damage");

			Unit.Update(1);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UpdateTick();
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UpdateTick();
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.UpdateTick();
			Unit.UpdateTick();
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void IntervalTimeTick_Damage()
		{
			AddRecipe("IntervalTimeTick_Damage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(2)
				.Effect(new DamageEffect(5), EffectOn.IntervalTick)
				.IntervalTick(2);
			Setup();

			Unit.AddModifierSelf("IntervalTimeTick_Damage");

			Unit.Update(1);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UpdateTick();
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UpdateTick();
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void IntervalTick_Refresh_Damage()
		{
			AddRecipe("IntervalTick_Refresh_Damage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(2)
				.Effect(new DamageEffect(5), EffectOn.IntervalTick)
				.IntervalTick(2).Refresh();
			Setup();

			Unit.AddModifierSelf("IntervalTick_Refresh_Damage");
			Unit.Update(1);
			Unit.UpdateTick();
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.AddModifierSelf("IntervalTick_Refresh_Damage");
			Unit.Update(1);
			Unit.UpdateTick();
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}