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
			AddRecipes(add => add("DurationDamage")
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Duration(5));

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
			SetupSystems();

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
			AddRecipes(add => add("DurationRemove")
				.Remove(5));

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
			AddRecipes(add => add("DurationRemove")
				.Remove(5));

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
			AddRecipes(add => add("IntervalDamage_DurationRemove")
				.Interval(4)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5));

			const string modifierName = "IntervalDamage_DurationRemove";

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
			AddRecipes(add => add("DamageEveryTwoStacks")
				.Effect(new DamageEffect(5, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, value: -1, maxStacks: -1, everyXStacks: 2));

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
			AddRecipes(add => add("StackBasedDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2));

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
			AddRecipes(add => add("IntervalDamage_StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add), EffectOn.Interval | EffectOn.Stack)
				.Interval(1)
				.Stack(WhenStackEffect.Always, value: 2));

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
			AddRecipes(add => add("StackAddDamageRevertible")
				.Effect(new AddDamageEffect(5, true, StackEffectType.Effect | StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2)
				.Remove(5));

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
			//InitDamageOneTime With1Seconds linger, to not work again (global effect cooldown)
			AddRecipes(add => add("OneTimeInitDamage")
				.OneTimeInit()
				.Effect(new DamageEffect(5), EffectOn.Init)
			);

			Pool.Clear();
			int recipeId = IdManager.GetId("OneTimeInitDamage");
			Pool.Allocate(recipeId, 1);

			Unit.AddModifierSelf("OneTimeInitDamage"); //Init
			Unit.AddModifierSelf("OneTimeInitDamage"); //No init

			Unit.ModifierController.Remove(new ModifierReference(recipeId, 0)); //Remove, back to pool, reset state

			Unit.AddModifierSelf("OneTimeInitDamage"); //Use again, init

			Assert.AreEqual(UnitHealth - 10, Unit.Health);
		}

		[Test]
		public void InitDamage_Cooldown_Effect()
		{
			AddRecipes(add => add("InitDamage_Cooldown_Effect")
				.EffectCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init));

			Unit.AddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Enemy.AddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void NoUpdateOnSameUpdateTick()
		{
			AddRecipes(
				add => add("AddModifierApplierDamage")
					.Effect(new DamageEffect(5), EffectOn.Interval)
					.Interval(0.1f),
				add => add("AddModifierApplierIntervalApplier")
					.Effect(new ApplierEffect("AddModifierApplierDamage"), EffectOn.Interval)
					.Interval(1));

			Unit.AddModifierSelf("AddModifierApplierIntervalApplier");

			Unit.Update(1); //Modifier should be added, but not updated

			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(0.1f);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}
	}
}