using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class StateTests : ModifierTests
	{
		[Test]
		public void TwoDurationModifiers_DifferentState()
		{
			Unit.AddModifierSelf("DurationDamage");
			Enemy.AddModifierSelf("DurationDamage");

			Unit.Update(5);
			Enemy.Update(2);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth, Enemy.Health);
		}

		[Test]
		public void TwoInitModifiers_DifferentState()
		{
			Unit.AddModifierSelf("InitDamage");
			Enemy.AddModifierSelf("InitDamage");

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 10, Unit.Health);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void TwoDurationRemoveModifiers_DifferentState()
		{
			Unit.AddModifierSelf("DurationRemove");
			Enemy.AddModifierSelf("DurationRemove");

			Unit.Update(5);
			Enemy.Update(2);

			Assert.False(Unit.ContainsModifier("DurationRemove"));
			Assert.True(Enemy.ContainsModifier("DurationRemove"));
		}

		[Test]
		public void TwoDurationRemoveModifiers_DifferentState_ReverseOrder()
		{
			Unit.AddModifierSelf("DurationRemove");
			Enemy.AddModifierSelf("DurationRemove");

			Unit.Update(2);
			Enemy.Update(5);

			Assert.True(Unit.ContainsModifier("DurationRemove"));
			Assert.False(Enemy.ContainsModifier("DurationRemove"));
		}

		[Test]
		public void ModifierGoneWhenRemoved()
		{
			string modifierName = "IntervalDamage_DurationRemove";

			Unit.AddModifierSelf(modifierName);

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

			void DoAndAssert(float expectedHealth, IModifierOwner unit)
			{
				unit.AddModifierSelf("DamageEveryTwoStacks");
				Assert.AreEqual(expectedHealth, ((IDamagable<float, float>)unit).Health);
			}
		}

		[Test]
		public void Stack_DamageStackBased()
		{
			Unit.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health); //1 stack = +2 damage == 2

			Enemy.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(EnemyHealth - 5 - 2, Enemy.Health);
			Enemy.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(EnemyHealth - 10 - 6, Enemy.Health); //2 stacks = +4 damage == 4

			Unit.AddModifierSelf("StackBasedDamage");
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health); //2 stacks = +4 damage == 6
		}

		[Test]
		public void IntervalDamage_AddDamageOnStack()
		{
			Unit.AddModifierSelf("IntervalDamage_StackAddDamage");

			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);

			Unit.AddModifierSelf("IntervalDamage_StackAddDamage");

			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 10 - 6, Unit.Health);
		}

		[Test]
		public void AddDamageOnStack_RevertibleRemove()
		{
			Unit.AddModifierSelf("StackAddDamageRevertible");
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			Unit.AddModifierSelf("StackAddDamageRevertible");
			Assert.AreEqual(UnitDamage + 5 + 5 + 2 + 4, Unit.Damage);

			Unit.AddModifierSelf("StackAddDamageRevertible");
			Assert.AreEqual(UnitDamage + 5 + 5 + 5 + 2 + 4 + 6, Unit.Damage);

			Enemy.AddModifierSelf("StackAddDamageRevertible");
			Assert.AreEqual(EnemyDamage + 5 + 2, Enemy.Damage);

			Enemy.AddModifierSelf("StackAddDamageRevertible");
			Assert.AreEqual(EnemyDamage + 5 + 5 + 2 + 4, Enemy.Damage);

			Enemy.Update(5); //Removed
			Assert.AreEqual(EnemyDamage, Enemy.Damage);

			Unit.Update(5); //Removed
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void OneTimeInit_ResetState()
		{
			Pool.Clear();
			int recipeId = IdManager.GetId("OneTimeInitDamage");
			Pool.Allocate(recipeId, 1);

			Unit.AddModifierSelf("OneTimeInitDamage"); //Init
			Unit.AddModifierSelf("OneTimeInitDamage"); //No init

			Unit.ModifierController.Remove(recipeId); //Remove, back to pool, reset state

			Unit.AddModifierSelf("OneTimeInitDamage"); //Use again, init

			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void InitDamage_Cooldown_Effect()
		{
			Unit.AddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Enemy.AddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}
	}
}