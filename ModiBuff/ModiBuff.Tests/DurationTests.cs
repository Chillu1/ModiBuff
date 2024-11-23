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

			Unit.Update(5);

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
	}
}