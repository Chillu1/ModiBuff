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
			AddRecipes(add => add("DurationDamage")
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Duration(5));

			Unit.AddModifierSelf("DurationDamage");

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void Duration_Remove()
		{
			AddRecipes(add => add("DurationRemove")
				.Remove(5));

			Unit.AddModifierSelf("DurationRemove");

			Unit.Update(5);

			Assert.False(Unit.ContainsModifier("DurationRemove"));
		}

		[Test]
		public void Duration_Damage_Once()
		{
			AddRecipes(add => add("DurationDamage")
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Duration(5));

			Unit.AddModifierSelf("DurationDamage");

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(5);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}