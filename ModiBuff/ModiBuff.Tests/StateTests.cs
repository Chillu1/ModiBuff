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
			AddRecipe("DurationDamage")
				.Effect(new DamageEffect(5), EffectOn.Duration)
				.Duration(5);
			Setup();

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
			Setup();

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
			AddRecipe("DurationRemove")
				.Remove(5);
			Setup();

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
			AddRecipe("DurationRemove")
				.Remove(5);
			Setup();

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
			AddRecipe("IntervalDamage_DurationRemove")
				.Interval(4)
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Remove(5);
			Setup();

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
			AddRecipe("DamageEveryTwoStacks")
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2);
			Setup();

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
			AddRecipe("StackBasedDamage")
				.Effect(new DamageEffect(5, StackEffectType.Effect | StackEffectType.Add, 2), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

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
			AddRecipe("IntervalDamage_StackAddDamage")
				.Effect(new DamageEffect(5, StackEffectType.Add, 2), EffectOn.Interval | EffectOn.Stack)
				.Interval(1)
				.Stack(WhenStackEffect.Always);
			Setup();

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
			AddRecipe("StackAddDamageRevertible")
				.Effect(
					new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Effect | StackEffectType.Add, 2),
					EffectOn.Stack)
				.Stack(WhenStackEffect.Always)
				.Remove(5);
			Setup();

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
		public void AddDamageOnStack_NoMutableState()
		{
			AddRecipe("StackAddDamage")
				.Effect(new AddDamageEffect(5, stackEffect: StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("StackAddDamage");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.AddModifierSelf("StackAddDamage");
			Assert.AreEqual(UnitDamage + 5 * 2, Unit.Damage);

			Enemy.AddModifierSelf("StackAddDamage");
			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);

			Unit.AddModifierSelf("StackAddDamage");
			Assert.AreEqual(UnitDamage + 5 * 3, Unit.Damage);

			Unit.Update(5); //Removed
			Assert.AreEqual(UnitDamage + 5 * 3, Unit.Damage);
			Enemy.Update(5); //Removed
			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);

			Unit.AddModifierSelf("StackAddDamage");
			Assert.AreEqual(UnitDamage + 5 * 4, Unit.Damage);
		}

		[Test]
		public void OneTimeInit_ResetState()
		{
			//InitDamageOneTime With1Seconds linger, to not work again (global effect cooldown)
			AddRecipe("OneTimeInitDamage")
				.OneTimeInit()
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

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
			AddRecipe("InitDamage_Cooldown_Effect")
				.EffectCooldown(1)
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Enemy.AddModifierSelf("InitDamage_Cooldown_Effect");
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);
		}

		[Test]
		public void NoUpdateOnSameUpdateTick()
		{
			AddRecipe("AddModifierApplierDamage")
				.Effect(new DamageEffect(5), EffectOn.Interval)
				.Interval(0.1f);
			AddRecipe("AddModifierApplierIntervalApplier")
				.Effect(new ApplierEffect("AddModifierApplierDamage"), EffectOn.Interval)
				.Interval(1);
			Setup();

			Unit.AddModifierSelf("AddModifierApplierIntervalApplier");

			Unit.Update(1); //Modifier should be added, but not updated

			Assert.AreEqual(UnitHealth, Unit.Health);

			Unit.Update(0.1f);

			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void UnitProjectileSavedState_DamageBasedOnDistanceMoved()
		{
			AddRecipe("InitDamageDistanceMultiplier")
				.Effect(new DamageEffect(5).SetMetaEffects(new DistanceMultiplierMetaEffect(10f, 1f)), EffectOn.Init);
			Setup();

			Enemy.Move(10, 0);
			var projectile = new Projectile(Vector2.Zero, Unit, IdManager.GetId("InitDamageDistanceMultiplier"));
			projectile.Move(10, 0);
			projectile.Hit(Enemy);

			Assert.AreEqual(EnemyHealth - 5 * 2, Enemy.Health);
		}

		/*[Test]
		public void UnitEffectSavedState_DamageBasedOnDistanceMoved()
		{
			//It's hard to make having mutable state in effects work
			//Since we'd need to update that state every time we ex "cast" the modifier
			//Which leads to a lot of complexity, where effects should be as isolated as possible

			AddRecipe("InitDamageDistanceMultiplier")
				.Effect(new DamageEffect(5, new SavedStateMultiplier( /*Need to feed InitialPosition here#1#))
					.SetMetaEffects(new DistanceMultiplierMetaEffect(10f, 1f)), EffectOn.Init);
			Setup();

			Enemy.Move(10, 0);
			//This would update the initial position of the mutable state in damage effect
			//Unit.TryCast(IdManager.GetId("InitDamageDistanceMultiplier"));
			Enemy.ModifierController.Add(IdManager.GetId("InitDamageDistanceMultiplier"), Enemy, Unit);

			Assert.AreEqual(EnemyHealth - 5 * 2, Enemy.Health);
		}*/
	}
}