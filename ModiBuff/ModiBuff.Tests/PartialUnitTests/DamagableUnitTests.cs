using System.Collections.Generic;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using NUnit.Framework;
using IEventOwner = ModiBuff.Core.IEventOwner;

namespace ModiBuff.Tests
{
	//Unit logic to test for:
	//No attack (can't attack, can't add damage)
	//Can't be status effected
	//No health (can't be healed, can't be damaged)
	//No mana (can't use mana, can't be mana checked (both full mana, and mana cost))
	//	What about effects where they cost mana/health to activate, they'll just never be activated?
	//No callbacks for X, Y, Z
	//Logic checks in post effects

	public sealed class DamagableUnitTests : ModifierTests
	{
		private sealed class DamagableUnit : IUnit, IModifierOwner, IDamagable, IEventOwner,
			ICallbackRegistrable<CallbackType>, IUpdatable, IUnitEntity
		{
			public UnitTag UnitTag { get; }
			public UnitType UnitType { get; }

			public float Health { get; private set; }
			public float MaxHealth { get; }
			public bool IsDead { get; private set; }

			public ModifierController ModifierController { get; }

			private const int MaxRecursionEventCount = 1;

			private readonly List<HealthChangedEvent> _healthChangedEvents;
			private int _healthChangedCounter;

			public DamagableUnit(UnitType unitType = UnitType.Good)
			{
				UnitType = unitType;
				UnitTag = UnitTag.Default;
				MaxHealth = Health = 500f;

				ModifierController = new ModifierController(this);
				_healthChangedEvents = new List<HealthChangedEvent>();
			}

			public void Update(float delta)
			{
				ModifierController.Update(delta);
			}

			public float TakeDamage(float damage, IUnit source)
			{
				float oldHealth = Health;
				Health -= damage;

				float dealtDamage = oldHealth - Health;

				_healthChangedCounter++;
				if (_healthChangedCounter <= MaxRecursionEventCount)
				{
					for (int i = 0; i < _healthChangedEvents.Count; i++)
						_healthChangedEvents[i](this, source, Health, dealtDamage);
				}

				if (Health <= 0 && !IsDead)
				{
					ModifierController.Clear();
					IsDead = true;
				}

				if (_healthChangedCounter <= MaxRecursionEventCount)
				{
					ResetEventCounters();
					(source as IEventOwner)?.ResetEventCounters();
				}

				return dealtDamage;
			}

			public void RegisterCallbacks(Callback<CallbackType>[] callbacks)
			{
				for (int i = 0; i < callbacks.Length; i++)
				{
					var callback = callbacks[i];
					switch (callback.CallbackType)
					{
						case CallbackType.CurrentHealthChanged:
							if (CheckCallback(callback.Action, out HealthChangedEvent healthEvent))
							{
								healthEvent.Invoke(this, this, Health, 0f);
								_healthChangedEvents.Add(healthEvent);
							}

							break;
						default:
							Logger.Log(
								$"CallbackType {callback.CallbackType} is not implemented. For unit {nameof(DamagableUnit)}.");
							break;
					}
				}
			}

			public void UnRegisterCallbacks(Callback<CallbackType>[] callbacks)
			{
				for (int i = 0; i < callbacks.Length; i++)
				{
					var callback = callbacks[i];
					switch (callback.CallbackType)
					{
						case CallbackType.CurrentHealthChanged:
							if (_healthChangedEvents.Remove((HealthChangedEvent)callback.Action))
								((HealthChangedEvent)callback.Action).Invoke(this, this, Health, 0f);
							break;
						default:
							Logger.Log(
								$"CallbackType {callback.CallbackType} is not implemented. For unit {nameof(DamagableUnit)}.");
							break;
					}
				}
			}

			public void ResetEventCounters()
			{
				_healthChangedCounter = 0;
			}

			private static bool CheckCallback<TCallback>(object callbackObject, out TCallback callbackOut)
			{
				if (!(callbackObject is TCallback callback))
				{
					Logger.LogError($"objectDelegate is not of type {nameof(TCallback)}, use named delegates instead.");
					callbackOut = default;
					return false;
				}

				callbackOut = callback;
				return true;
			}
		}

		[Test]
		public void TryDamageAndHeal_UnHealableUnit()
		{
			AddRecipe("InitDamageHeal")
				.Effect(new DamageEffect(5), EffectOn.Init)
				.Effect(new HealEffect(5, HealEffect.EffectState.IsRevertible), EffectOn.Init)
				.Remove(1);
			Setup();

			var unit = new DamagableUnit();
			unit.AddModifierSelf("InitDamageHeal");
			Assert.AreEqual(UnitHealth - 5, unit.Health);
			unit.Update(1);
			Assert.AreEqual(UnitHealth - 5, unit.Health);
		}

		[Test]
		public void TryAddDamage_NoDamageUnit()
		{
			AddRecipe("InitAddDamage")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(1);
			Setup();

			var unit = new DamagableUnit();
			unit.AddModifierSelf("InitAddDamage"); //Try add damage, can't
			unit.Update(1); //Try revert, can't
		}

		[Test]
		public void TryStun_NoStatusEffectsUnit()
		{
			AddRecipe("InitStun")
				.Effect(new StatusEffectEffect(StatusEffectType.Stun, 2f, true), EffectOn.Init)
				.Remove(1);
			Setup();

			var unit = new DamagableUnit();
			unit.AddModifierSelf("InitStun");
			unit.Update(1);
			Assert.False(unit.ContainsModifier("InitStun"));
		}

		[Test]
		public void TryDamagePostHeal_UnHealableUnit()
		{
			AddRecipe("InitDamagePostHeal")
				.Effect(new DamageEffect(5).SetPostEffects(new LifeStealPostEffect(1f)), EffectOn.Init);
			Setup();

			var unit = new DamagableUnit();
			unit.AddModifierSelf("InitDamagePostHeal");
			Assert.AreEqual(UnitHealth - 5, unit.Health);
		}

		//TODO Making the same code over and over will be redundant, better to make an array of scenarios with:
		//Recipe setup, then all actions & checks
		//Might be hard/not smart, since we need all the info in there to be static, which can be fixed, but maybe not worth
	}
}