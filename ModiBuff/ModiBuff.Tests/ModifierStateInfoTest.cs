using System;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierStateInfoTest : ModifierTests
	{
		[Test]
		public void InitDamage_CorrectBaseDamage_Recipe()
		{
			Setup();

			Unit.AddModifierSelf("InitDamage");
			var state = Unit.ModifierController.GetEffectState<DamageEffect.Data>(IdManager.GetId("InitDamage").Value)
				.Value.Data;
			Assert.AreEqual(5, state.BaseDamage);
			Assert.AreEqual(0, state.ExtraDamage);
		}

		[Test]
		public void InitDamage_CorrectBaseDamage_Manual()
		{
			AddGenerator("InitDamageManual", (id, genId, name, tag) =>
			{
				var damageEffect = new DamageEffect(5);
				var initComponent = new InitComponent(new IEffect[] { damageEffect }, null);

				return new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null, new EffectStateInfo((EffectOn.Init, damageEffect)), null);
			});
			Setup();

			Unit.AddModifierSelf("InitDamageManual");

			var state = Unit.ModifierController.GetEffectState<DamageEffect.Data>(IdManager.GetId("InitDamageManual")
				.Value).Value.Data;
			Assert.AreEqual(5, state.BaseDamage);
			Assert.AreEqual(0, state.ExtraDamage);
		}

		[Test]
		public void InitDoubleStackDamage_CorrectBaseDamage()
		{
			AddRecipe("DoubleStackDamage")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Init | EffectOn.Interval)
				.Effect(new DamageEffect(10, false, StackEffectType.Add, 2), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			int id = IdManager.GetId("DoubleStackDamage").Value;
			Unit.AddModifierSelf("DoubleStackDamage");

			var initDamageState = Unit.ModifierController.GetEffectState<DamageEffect.Data>(id).Value.Data;
			Assert.AreEqual(5, initDamageState.BaseDamage);
			Assert.AreEqual(0, initDamageState.ExtraDamage);
			var stackDamageState =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, stateNumber: 1).Value.Data;
			Assert.AreEqual(10, stackDamageState.BaseDamage);
			Assert.AreEqual(2, stackDamageState.ExtraDamage);

			Unit.AddModifierSelf("DoubleStackDamage");

			var stackUpdatedDamageState =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, stateNumber: 1).Value.Data;
			Assert.AreEqual(10, stackUpdatedDamageState.BaseDamage);
			Assert.AreEqual(4, stackUpdatedDamageState.ExtraDamage);
		}

		[Test]
		public void IntervalDurationTimerReferences()
		{
			AddRecipe("IntervalDurationDamage")
				.Interval(1)
				.Duration(5)
				.Effect(new DamageEffect(5), EffectOn.Interval | EffectOn.Duration);
			Setup();

			Unit.AddModifierSelf("IntervalDurationDamage");
			int id = IdManager.GetId("IntervalDurationDamage").Value;

			var intervalReference = Unit.ModifierController.GetTimer<IntervalComponent>(id);
			var durationReference = Unit.ModifierController.GetTimer<DurationComponent>(id);
			Assert.AreEqual(intervalReference.Time, 1);
			Assert.AreEqual(durationReference.Time, 5);
			Assert.AreEqual(0, intervalReference.Timer);
			Assert.AreEqual(0, durationReference.Timer);
			Unit.Update(0.5f);
			Assert.AreEqual(0.5f, intervalReference.Timer);
			Assert.AreEqual(0.5f, durationReference.Timer);
			Unit.Update(0.5f);
			Assert.AreEqual(0f, intervalReference.Timer);
			Assert.AreEqual(1f, durationReference.Timer);
		}

		[Test]
		public void StackReference()
		{
			AddRecipe("StackDamage")
				.Effect(new DamageEffect(5), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, maxStacks: 5);
			Setup();

			int id = IdManager.GetId("StackDamage").Value;
			Unit.AddModifierSelf("StackDamage");

			var stackReference = Unit.ModifierController.GetStackReference(id);
			Assert.AreEqual(1, stackReference.Stacks);

			Unit.AddModifierSelf("StackDamage");
			Assert.AreEqual(2, stackReference.Stacks);
			Assert.AreEqual(5, stackReference.MaxStacks);
		}


		[Test]
		public void CallbackLocalVarState()
		{
			AddRecipe("InitTakeTwoDamageOnTenDamageTaken")
				.Callback(CallbackType.CurrentHealthChanged, () =>
				{
					float totalDamageTaken = 0f;

					return new CallbackStateContext<float>(new HealthChangedEvent(
						(target, source, health, deltaHealth) =>
						{
							//Don't count "negative damage/healing damage"
							if (deltaHealth > 0)
								totalDamageTaken += deltaHealth;
							if (totalDamageTaken >= 10)
							{
								totalDamageTaken = 0f;
								target.TakeDamage(2, source);
							}
						}), () => totalDamageTaken, value => totalDamageTaken = value);
				});
			Setup();

			Unit.AddModifierSelf("InitTakeTwoDamageOnTenDamageTaken");
			Unit.TakeDamage(5, Unit);

			var state = Unit.ModifierController
				.GetEffectState<CallbackStateSaveRegisterEffect<CallbackType, float>.Data>(
					IdManager.GetId("InitTakeTwoDamageOnTenDamageTaken").Value).Value.Data;
			Assert.AreEqual(5, state.State);

			Unit.TakeDamage(5, Unit);
			var state2 = Unit.ModifierController
				.GetEffectState<CallbackStateSaveRegisterEffect<CallbackType, float>.Data>(
					IdManager.GetId("InitTakeTwoDamageOnTenDamageTaken").Value).Value.Data;
			Assert.AreEqual(2, state2.State);
		}

		[Test]
		public void InitIntervalDurationDamage_CorrectStates()
		{
			AddRecipe("InitIntervalDurationStackStackDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Interval(1)
				.Effect(new DamageEffect(7.5f), EffectOn.Interval)
				.Duration(5)
				.Effect(new DamageEffect(10), EffectOn.Duration)
				.Effect(new DamageEffect(15, false, StackEffectType.Add, 2), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			int id = IdManager.GetId("InitIntervalDurationStackStackDamage").Value;
			Unit.AddModifierSelf("InitIntervalDurationStackStackDamage");

			var initDamage = Unit.ModifierController.GetEffectState<DamageEffect.Data>(id).Value;
			Assert.AreEqual(EffectOn.Init, initDamage.EffectOn);
			Assert.AreEqual(5, initDamage.Data.BaseDamage);
			Assert.AreEqual(0, initDamage.Data.ExtraDamage);
			var stackDamage =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, stateNumber: 3).Value;
			Assert.AreEqual(EffectOn.Stack, stackDamage.EffectOn);
			Assert.AreEqual(15, stackDamage.Data.BaseDamage);
			Assert.AreEqual(2, stackDamage.Data.ExtraDamage);

			Unit.AddModifierSelf("InitIntervalDurationStackStackDamage");

			var stackUpdatedDamageState =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, stateNumber: 3).Value;
			Assert.AreEqual(15, stackUpdatedDamageState.Data.BaseDamage);
			Assert.AreEqual(4, stackUpdatedDamageState.Data.ExtraDamage);
		}

		[Test]
		public void SpecificEffectOn_CorrectState()
		{
			AddRecipe("InitIntervalDurationStackStackDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Interval(1)
				.Effect(new DamageEffect(7.5f), EffectOn.Interval)
				.Duration(5)
				.Effect(new DamageEffect(10), EffectOn.Duration)
				.Effect(new DamageEffect(15, false, StackEffectType.Add, 2), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			int id = IdManager.GetId("InitIntervalDurationStackStackDamage").Value;
			Unit.AddModifierSelf("InitIntervalDurationStackStackDamage");

			var stackDamage =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, effectOn: EffectOn.Stack).Value;
			Assert.AreEqual(EffectOn.Stack, stackDamage.EffectOn);
			Assert.AreEqual(15, stackDamage.Data.BaseDamage);
			Assert.AreEqual(2, stackDamage.Data.ExtraDamage);

			Unit.AddModifierSelf("InitIntervalDurationStackStackDamage");

			var stackUpdatedDamageState =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, effectOn: EffectOn.Stack).Value;
			Assert.AreEqual(15, stackUpdatedDamageState.Data.BaseDamage);
			Assert.AreEqual(4, stackUpdatedDamageState.Data.ExtraDamage);
		}

		[Test]
		public void SpecificEffectOnMultipleEffects_CorrectStates()
		{
			AddRecipe("InitStackStackDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(15, false, StackEffectType.Add, 2), EffectOn.Stack)
				.Effect(new DamageEffect(25, false, StackEffectType.Effect), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			int id = IdManager.GetId("InitStackStackDamage").Value;
			Unit.AddModifierSelf("InitStackStackDamage");

			var stackDamage =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, effectOn: EffectOn.Stack).Value;
			Assert.AreEqual(EffectOn.Stack, stackDamage.EffectOn);
			Assert.AreEqual(15, stackDamage.Data.BaseDamage);
			Assert.AreEqual(2, stackDamage.Data.ExtraDamage);
			var secondStackDamage =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, effectOn: EffectOn.Stack, stateNumber: 1)
					.Value;
			Assert.AreEqual(EffectOn.Stack, secondStackDamage.EffectOn);
			Assert.AreEqual(25, secondStackDamage.Data.BaseDamage);
			Assert.AreEqual(0, secondStackDamage.Data.ExtraDamage);

			Unit.AddModifierSelf("InitStackStackDamage");

			var stackUpdatedDamageState =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, effectOn: EffectOn.Stack).Value;
			Assert.AreEqual(15, stackUpdatedDamageState.Data.BaseDamage);
			Assert.AreEqual(4, stackUpdatedDamageState.Data.ExtraDamage);
			var secondStackUpdatedDamageState =
				Unit.ModifierController.GetEffectState<DamageEffect.Data>(id, effectOn: EffectOn.Stack, stateNumber: 1)
					.Value;
			Assert.AreEqual(25, secondStackUpdatedDamageState.Data.BaseDamage);
			Assert.AreEqual(0, secondStackUpdatedDamageState.Data.ExtraDamage);
		}

		[Test]
		public void InitDoubleStackDamage_GetAllStates()
		{
			AddRecipe("DoubleStackDamage")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Init | EffectOn.Interval)
				.Effect(new DamageEffect(10, false, StackEffectType.Add, 2), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			int id = IdManager.GetId("DoubleStackDamage").Value;
			Unit.AddModifierSelf("DoubleStackDamage");

			var states = Unit.ModifierController.GetEffectStates(id);
			for (int i = 0; i < states.Length; i++)
			{
				object state = states[i].Data;
				var data = (DamageEffect.Data)state;

				if (i == 0)
				{
					Assert.AreEqual(5, data.BaseDamage);
					Assert.AreEqual(0, data.ExtraDamage);
				}
				else if (i == 1)
				{
					Assert.AreEqual(10, data.BaseDamage);
					Assert.AreEqual(2, data.ExtraDamage);
				}
				else
				{
					Assert.Fail($"Unexpected state index {i} for DamageEffect.Data");
				}
			}
		}

		[Test]
		public void InitIntervalDurationDamage_GetAllStates()
		{
			AddRecipe("InitIntervalDurationStackStackDamage")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new DamageEffect(7.5f), EffectOn.Interval)
				.Interval(1)
				.Effect(new DamageEffect(10), EffectOn.Duration)
				.Duration(5)
				.Effect(new DamageEffect(15, false, StackEffectType.Add, 2), EffectOn.Stack)
				.Stack(WhenStackEffect.Always);
			Setup();

			int id = IdManager.GetId("InitIntervalDurationStackStackDamage").Value;
			Unit.AddModifierSelf("InitIntervalDurationStackStackDamage");

			var states = Unit.ModifierController.GetEffectStates(id);
			for (int i = 0; i < states.Length; i++)
			{
				(var effectOn, object state) = states[i];
				var data = (DamageEffect.Data)state;

				if (effectOn.HasFlag(EffectOn.Init))
				{
					Assert.AreEqual(5, data.BaseDamage);
					Assert.AreEqual(0, data.ExtraDamage);
				}
				else if (effectOn.HasFlag(EffectOn.Interval))
				{
					Assert.AreEqual(7.5f, data.BaseDamage);
					Assert.AreEqual(0, data.ExtraDamage);
				}
				else if (effectOn.HasFlag(EffectOn.Duration))
				{
					Assert.AreEqual(10, data.BaseDamage);
					Assert.AreEqual(0, data.ExtraDamage);
				}
				else if (effectOn.HasFlag(EffectOn.Stack))
				{
					Assert.AreEqual(15, data.BaseDamage);
					Assert.AreEqual(2, data.ExtraDamage);
				}
				else
				{
					Assert.Fail($"Unexpected EffectOn {effectOn} for DamageEffect.Data");
				}
			}
		}

		[Test]
		public void NoState_GetEffectState_Missing()
		{
			AddRecipe("NoOp")
				.Effect(new NoOpEffect(), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("NoOp");
			var state = Unit.ModifierController.GetEffectState<DamageEffect.Data>(IdManager.GetId("NoOp").Value);
			Assert.Null(state);
		}

		[Test]
		public void NoState_GetEffectStates_Missing()
		{
			AddRecipe("NoOp")
				.Effect(new NoOpEffect(), EffectOn.Init);
			Setup();

			Unit.AddModifierSelf("NoOp");
			var state = Unit.ModifierController.GetEffectStates(IdManager.GetId("NoOp").Value);
			Assert.Null(state);
		}
	}
}