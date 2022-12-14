using NUnit.Framework;

namespace ModifierLibraryLite.Tests
{
	public sealed class StateTests : BaseModifierTests
	{
		[Test]
		public void TwoDurationModifiers_DifferentState()
		{
			Unit.TryAddModifierSelf("DurationDamage");
			Enemy.TryAddModifierSelf("DurationDamage");

			Unit.Update(5);
			Enemy.Update(2);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void TwoInitModifiers_DifferentState()
		{
			Unit.TryAddModifierSelf("InitDamage");
			Enemy.TryAddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void TwoDurationRemoveModifiers_DifferentState()
		{
			Unit.TryAddModifierSelf("DurationRemove");
			Enemy.TryAddModifierSelf("DurationRemove");

			Unit.Update(5);
			Enemy.Update(2);

			Assert.False(Unit.ContainsModifier("DurationRemove"));
			Assert.True(Enemy.ContainsModifier("DurationRemove"));
		}

		[Test]
		public void TwoDurationRemoveModifiers_DifferentState_ReverseOrder()
		{
			Unit.TryAddModifierSelf("DurationRemove");
			Enemy.TryAddModifierSelf("DurationRemove");

			Unit.Update(2);
			Enemy.Update(5);

			Assert.True(Unit.ContainsModifier("DurationRemove"));
			Assert.False(Enemy.ContainsModifier("DurationRemove"));
		}

		[Test]
		public void ModifierGoneWhenRemoved()
		{
			string modifierName = "IntervalDamage_DurationRemove";

			Unit.TryAddModifierSelf(modifierName);

			Unit.Update(4);
			Assert.True(Unit.ContainsModifier(modifierName));
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(1);
			Assert.False(Unit.ContainsModifier(modifierName));
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.Update(20);
			Assert.False(Unit.ContainsModifier(modifierName));
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}