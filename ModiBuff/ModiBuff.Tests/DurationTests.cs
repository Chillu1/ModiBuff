using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class DurationTests : ModifierTests
	{
		[Test]
		public void Duration_Damage()
		{
			AddRecipe("DurationDamage")
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Duration(5);
			Setup();

			Unit.AddModifierSelf("DurationDamage");

			Unit.UpdateTicks(5);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(5);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DurationTick_Damage()
		{
			AddRecipe("DurationTickDamage")
				.Effect(new DamageEffect(5), EffectOn.DurationTick)
				.DurationTick(5);
			Setup();

			Unit.AddModifierSelf("DurationTickDamage");

			Unit.Update(5);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.UpdateTicks(5);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Duration_Remove()
		{
			AddRecipe("DurationRemove")
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DurationRemove");

			Unit.Update(5);

			Assert.False(Unit.ContainsModifier("DurationRemove"));
		}

		[Test]
		public void DurationTick_Remove()
		{
			AddRecipe("DurationTickRemove")
				.RemoveTicks(5);
			Setup();

			Unit.AddModifierSelf("DurationTickRemove");

			Unit.UpdateTicks(5);

			Assert.False(Unit.ContainsModifier("DurationTickRemove"));
		}

		[Test]
		public void Duration_Damage_Once()
		{
			AddRecipe("DurationDamage")
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Duration(5);
			Setup();

			Unit.AddModifierSelf("DurationDamage");

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void DurationTick_Damage_Once()
		{
			AddRecipe("DurationTickDamage")
				.Effect(new DamageEffect(5), EffectOn.DurationTick)
				.DurationTick(5);
			Setup();

			Unit.AddModifierSelf("DurationTickDamage");

			Unit.UpdateTicks(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.UpdateTicks(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void TwoModifiersSameDurationRemove()
		{
			AddRecipe("DurationRemove")
				.Remove(5);
			AddRecipe("DurationRemove2")
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("DurationRemove");
			Unit.AddModifierSelf("DurationRemove2");

			Unit.Update(5f);

			Assert.False(Unit.ContainsModifier("DurationRemove"));
			Assert.False(Unit.ContainsModifier("DurationRemove2"));
		}

		[Test]
		public void TwoModifiersSameDurationTickRemove()
		{
			AddRecipe("DurationTickRemove")
				.RemoveTicks(5);
			AddRecipe("DurationTickRemove2")
				.RemoveTicks(5);
			Setup();

			Unit.AddModifierSelf("DurationTickRemove");
			Unit.AddModifierSelf("DurationTickRemove2");

			Unit.UpdateTicks(5);

			Assert.False(Unit.ContainsModifier("DurationTickRemove"));
			Assert.False(Unit.ContainsModifier("DurationTickRemove2"));
		}

		[Test]
		public void MixedDurationAndDurationTickRemove()
		{
			AddRecipe("DurationMixedRemove")
				.Remove(5)
				.RemoveTicks(10);
			Setup();

			Unit.AddModifierSelf("DurationMixedRemove");

			Unit.Update(4f);
			Unit.UpdateTicks(9);

			Assert.True(Unit.ContainsModifier("DurationMixedRemove"));

			Unit.UpdateTicks(1);

			Assert.False(Unit.ContainsModifier("DurationMixedRemove"));
		}
	}
}