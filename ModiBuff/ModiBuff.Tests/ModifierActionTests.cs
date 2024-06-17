using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierActionTests : ModifierTests
	{
		[Test]
		public void IntervalStackDamage()
		{
			AddRecipe("IntervalStackDamage")
				.Interval(1)
				.ModifierAction(ModifierAction.Stack, EffectOn.Interval)
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5), EffectOn.Stack);
			Setup();

			Unit.AddModifierSelf("IntervalStackDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5, Unit.Health);
		}

		[Test]
		public void AddDamageDurationRefreshOnStrongHit()
		{
			AddRecipe("DurationAddDamageStrongHitRefresh")
				.Effect(new AddDamageEffect(5), EffectOn.Duration)
				.ModifierAction(ModifierAction.Refresh, EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit)
				.Duration(2).Refresh();
			Setup();

			Unit.AddModifierSelf("DurationAddDamageStrongHitRefresh");
			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Refresh timer
			Unit.Update(1);
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.Update(1);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void AddDamageOn2StacksResetStacksOnStrongHit()
		{
			AddRecipe("StackAddDamageStrongHitResetStacks")
				.Effect(new AddDamageEffect(5), EffectOn.Stack)
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit)
				.Stack(WhenStackEffect.EveryXStacks, everyXStacks: 2);
			Setup();

			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Reset stacks
			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage, Unit.Damage);

			Unit.AddModifierSelf("StackAddDamageStrongHitResetStacks");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}

		[Test]
		public void AddDamageStackTimers_ResetStacks_RevertEffects()
		{
			AddRecipe("AddDamageStackTimerResetStacks")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Stack)
				.ModifierAction(ModifierAction.ResetStacks, EffectOn.CallbackUnit)
				.CallbackUnit(CallbackUnitType.StrongHit)
				.Stack(WhenStackEffect.Always, independentStackTime: 5);
			Setup();

			Unit.AddModifierSelf("AddDamageStackTimerResetStacks");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.AddModifierSelf("AddDamageStackTimerResetStacks");
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Reset stacks
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void InstanceStackableAddDamageDurationRefreshOnStrongHit()
		{
			AddRecipe("InstanceStackableDurationAddDamageHealthChangedRefresh")
				.InstanceStackable()
				.Effect(new AddDamageEffect(5), EffectOn.CallbackEffect)
				.ModifierAction(ModifierAction.Refresh, EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.CurrentHealthChanged,
					effect => new HealthChangedEvent((target, source, health, deltaHealth) =>
					{
						if (deltaHealth > 0)
							effect.Effect(target, source);
					}))
				.Remove(3).Refresh();
			Setup();

			Unit.AddModifierSelf("InstanceStackableDurationAddDamageHealthChangedRefresh");
			Unit.AddModifierSelf("InstanceStackableDurationAddDamageHealthChangedRefresh");
			Unit.Update(1);

			Unit.TakeDamage(1, Unit);
			Assert.AreEqual(UnitDamage + 5 + 5, Unit.Damage);

			Unit.Update(1);
			Unit.AddModifierSelf("InstanceStackableDurationAddDamageHealthChangedRefresh");
			Unit.Update(2);

			Unit.TakeDamage(1, Unit);
			Assert.AreEqual(UnitDamage + 5 + 5 + 5, Unit.Damage);
		}
	}
}