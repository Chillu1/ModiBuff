using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
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

			Unit.TryAddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
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

		[Test]
		public void DamageEveryTwoStacks_Twice()
		{
			DoAndAssert(UnitHealth, Unit);

			DoAndAssert(EnemyHealth, Enemy);

			DoAndAssert(EnemyHealth - 5, Enemy);

			DoAndAssert(UnitHealth - 5, Unit);

			DoAndAssert(EnemyHealth - 5, Enemy);
			DoAndAssert(EnemyHealth - 10, Enemy);

			DoAndAssert(UnitHealth - 5, Unit);

			DoAndAssert(UnitHealth - 10, Unit);

			void DoAndAssert(float expectedHealth, IUnit unit)
			{
				unit.TryAddModifierSelf("DamageEveryTwoStacks");
				Assert.AreEqual(expectedHealth, unit.Health);
			}
		}

		[Test]
		public void Stack_DamageStackBased()
		{
			Unit.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health); //1 stack = +2 damage == 2

			Enemy.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(EnemyHealth - 5 - 2, Enemy.Health);
			Enemy.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(EnemyHealth - 10 - 6, Enemy.Health); //2 stacks = +4 damage == 4

			Unit.TryAddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health); //2 stacks = +4 damage == 6
		}

		[Test]
		public void IntervalDamage_AddDamageOnStack()
		{
			Unit.TryAddModifierSelf("IntervalDamage_StackAddDamage");

			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);

			Unit.TryAddModifierSelf("IntervalDamage_StackAddDamage");

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health);
		}
	}
}