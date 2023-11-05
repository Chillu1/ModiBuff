using System;
using System.Collections.Generic;

namespace ModiBuff.Core.Units
{
	//Often updated delegates
	public delegate void HealthChangedEvent(IUnit target, IUnit source, float newHealth, float deltaHealth);

	public delegate void DispelEvent(IUnit target, IUnit source, TagType tag);

	public delegate void PoisonEvent(IUnit target, IUnit source, int poisonStacks, int totalStacks, float dealtDamage);

	//Rarely updated delegates
	public delegate void DamageChangedEvent(IUnit unit, float newDamage, float deltaDamage);

	public partial class Unit
	{
		public const int MaxRecursionEventCount = 1;

		private int _preAttackCounter,
			_onAttackCounter,
			_whenAttackedCounter,
			_afterAttackedCounter,
			_healthChangedCounter,
			_onKillCounter,
			_healCounter,
			_healTargetCounter,
			_addDamageCounter;

		private int _poisonDamageCounter;

		//Note: These event lists should only be used for classic effects.
		//If you try to tie core game logic to them, you will most likely have trouble with sequence of events.
		private readonly List<IEffect> _whenAttackedEffects,
			_afterAttackedEffects,
			_whenCastEffects,
			_whenDeathEffects,
			_whenHealedEffects;

		private readonly List<IEffect> _beforeAttackEffects,
			_onAttackEffects,
			_onCastEffects,
			_onKillEffects,
			_onHealEffects;

		private readonly List<IEffect> _strongHitCallbacks;
		private readonly List<UnitCallback> _strongHitUnitCallbacks;
		private UnitCallback _strongHitDelegateCallbacks;

		private readonly List<PoisonEvent> _poisonEvents;

		private readonly List<DispelEvent> _dispelEvents;
		private readonly List<HealthChangedEvent> _healthChangedEvent;
		private readonly List<DamageChangedEvent> _damageChangedEvent;

		/// <summary>
		///		Resets all event/callback counters, so we can trigger them again
		/// </summary>
		/// <remarks>We always reset all counters because event effects might trigger other callbacks as well</remarks>
		public void ResetEventCounters()
		{
			_preAttackCounter = _onAttackCounter = _whenAttackedCounter = _afterAttackedCounter =
				_healthChangedCounter = _onKillCounter = _healCounter = _healTargetCounter =
					_addDamageCounter = _poisonDamageCounter = 0;
		}

		public void AddEffectEvent(IEffect effect, EffectOnEvent @event)
		{
			switch (@event)
			{
				case EffectOnEvent.WhenAttacked:
					_whenAttackedEffects.Add(effect);
					break;
				case EffectOnEvent.AfterAttacked:
					_afterAttackedEffects.Add(effect);
					break;
				case EffectOnEvent.WhenCast:
					_whenCastEffects.Add(effect);
					break;
				case EffectOnEvent.WhenKilled:
					_whenDeathEffects.Add(effect);
					break;
				case EffectOnEvent.WhenHealed:
					_whenHealedEffects.Add(effect);
					break;
				case EffectOnEvent.BeforeAttack:
					_beforeAttackEffects.Add(effect);
					break;
				case EffectOnEvent.OnAttack:
					_onAttackEffects.Add(effect);
					break;
				case EffectOnEvent.OnCast:
					_onCastEffects.Add(effect);
					break;
				case EffectOnEvent.OnKill:
					_onKillEffects.Add(effect);
					break;
				case EffectOnEvent.OnHeal:
					_onHealEffects.Add(effect);
					break;
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("Unknown event type: " + @event);
#endif
					return;
			}
		}

		public void RemoveEffectEvent(IEffect effect, EffectOnEvent @event)
		{
			switch (@event)
			{
				case EffectOnEvent.WhenAttacked:
					Remove(_whenAttackedEffects, effect);
					break;
				case EffectOnEvent.AfterAttacked:
					Remove(_afterAttackedEffects, effect);
					break;
				case EffectOnEvent.WhenCast:
					Remove(_whenCastEffects, effect);
					break;
				case EffectOnEvent.WhenKilled:
					Remove(_whenDeathEffects, effect);
					break;
				case EffectOnEvent.WhenHealed:
					Remove(_whenHealedEffects, effect);
					break;
				case EffectOnEvent.BeforeAttack:
					Remove(_beforeAttackEffects, effect);
					break;
				case EffectOnEvent.OnAttack:
					Remove(_onAttackEffects, effect);
					break;
				case EffectOnEvent.OnCast:
					Remove(_onCastEffects, effect);
					break;
				case EffectOnEvent.OnKill:
					Remove(_onKillEffects, effect);
					break;
				case EffectOnEvent.OnHeal:
					Remove(_onHealEffects, effect);
					break;
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("Unknown event type: " + @event);
#endif
					return;
			}

			void Remove(List<IEffect> effects, IEffect effectToRemove)
			{
				bool remove = effects.Remove(effectToRemove);

#if DEBUG && !MODIBUFF_PROFILE
				if (!remove)
					Logger.LogError("Could not remove event: " + effectToRemove.GetType());
#endif
			}
		}

		//---Callbacks---

		//Something to think about when implementing callbacks:
		//single delegates are 76% faster than arrays of IEffects with 1 subscriber/item
		//But arrays are much faster when there are multiple subscribers/items, 58% faster with 2 items, 150% faster with 5 items.
		//It's recommended to use the array version generally. But in cases where most modifiers have single callbacks, use delegates.
		public void RegisterCallbacks(CallbackType callbackType, IEffect[] callbacks)
		{
			switch (callbackType)
			{
				case CallbackType.StrongHit:
					_strongHitCallbacks.AddRange(callbacks);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void UnRegisterCallbacks(CallbackType callbackType, IEffect[] callbacks)
		{
			switch (callbackType)
			{
				case CallbackType.StrongHit:
					for (int i = 0; i < callbacks.Length; i++)
					{
						bool removed = _strongHitCallbacks.Remove(callbacks[i]);
#if DEBUG && !MODIBUFF_PROFILE
						if (!removed)
							Logger.LogError("Could not remove callback: " + callbacks[i]);
#endif
					}

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void RegisterCallbacks(CallbackType callbackType, UnitCallback callbacks)
		{
			switch (callbackType)
			{
				case CallbackType.StrongHit:
					_strongHitDelegateCallbacks += callbacks;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void UnRegisterCallbacks(CallbackType callbackType, UnitCallback callbacks)
		{
			switch (callbackType)
			{
				case CallbackType.StrongHit:
					_strongHitDelegateCallbacks -= callbacks;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void RegisterCallbacks(CustomCallback<CustomCallbackType>[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				ref readonly var callback = ref callbacks[i];
				switch (callback.CallbackType)
				{
					case CustomCallbackType.Dispel:
						if (!(callback.Action is DispelEvent dispelEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(DispelEvent)}, use named delegates instead.");
							break;
						}

						_dispelEvents.Add(dispelEvent);
						break;
					case CustomCallbackType.PoisonDamage:
						if (!(callback.Action is PoisonEvent poisonEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(PoisonEvent)}, use named delegates instead.");
							break;
						}

						_poisonEvents.Add(poisonEvent);
						break;
					case CustomCallbackType.CurrentHealthChanged:
						if (!(callback.Action is HealthChangedEvent healthEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(HealthChangedEvent)}, use named delegates instead.");
							break;
						}

						healthEvent.Invoke(this, this, Health, 0f);
						_healthChangedEvent.Add(healthEvent);
						break;
					case CustomCallbackType.DamageChanged:
						if (!(callback.Action is DamageChangedEvent damageEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(DamageChangedEvent)}, use named delegates instead.");
							break;
						}

						damageEvent.Invoke(this, Damage, 0f);
						_damageChangedEvent.Add(damageEvent);
						break;
					case CustomCallbackType.StrongHit:
						if (!(callback.Action is UnitCallback unitCallback))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(UnitCallback)}, use named delegates instead.");
							break;
						}

						_strongHitUnitCallbacks.Add(unitCallback);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(callbacks), callback.CallbackType, null);
				}
			}
		}

		public void UnRegisterCallbacks(CustomCallback<CustomCallbackType>[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				ref readonly var callback = ref callbacks[i];
				switch (callback.CallbackType)
				{
					case CustomCallbackType.Dispel:
						_dispelEvents.Remove((DispelEvent)callback.Action);
						break;
					case CustomCallbackType.PoisonDamage:
						_poisonEvents.Remove((PoisonEvent)callback.Action);
						break;
					case CustomCallbackType.CurrentHealthChanged:
						if (_healthChangedEvent.Remove((HealthChangedEvent)callback.Action))
							((HealthChangedEvent)callback.Action).Invoke(this, this, Health, 0f);
						break;
					case CustomCallbackType.DamageChanged:
						if (_damageChangedEvent.Remove((DamageChangedEvent)callback.Action))
							((DamageChangedEvent)callback.Action).Invoke(this, Damage, 0f);
						break;
					case CustomCallbackType.StrongHit:
						_strongHitUnitCallbacks.Remove((UnitCallback)callback.Action);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void RegisterCallbacks(CustomCallbackType callbackType, object[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				object callback = callbacks[i];

				switch (callbackType)
				{
					case CustomCallbackType.Dispel:
						if (!(callback is DispelEvent dispelEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(DispelEvent)}, use named delegates instead.");
							break;
						}

						_dispelEvents.Add(dispelEvent);
						break;
					case CustomCallbackType.PoisonDamage:
						if (!(callback is PoisonEvent poisonEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(PoisonEvent)}, use named delegates instead.");
							break;
						}

						_poisonEvents.Add(poisonEvent);
						break;
					case CustomCallbackType.CurrentHealthChanged:
						if (!(callback is HealthChangedEvent healthEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(HealthChangedEvent)}, use named delegates instead.");
							break;
						}

						healthEvent.Invoke(this, this, Health, 0f);
						_healthChangedEvent.Add(healthEvent);
						break;
					case CustomCallbackType.DamageChanged:
						if (!(callback is DamageChangedEvent damageEvent))
						{
							Logger.LogError(
								$"objectDelegate is not of type {nameof(DamageChangedEvent)}, use named delegates instead.");
							break;
						}

						damageEvent.Invoke(this, Damage, 0f);
						_damageChangedEvent.Add(damageEvent);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
				}
			}
		}

		public void UnRegisterCallbacks(CustomCallbackType callbackType, object[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				object callback = callbacks[i];

				switch (callbackType)
				{
					case CustomCallbackType.Dispel:
						_dispelEvents.Remove((DispelEvent)callback);
						break;
					case CustomCallbackType.PoisonDamage:
						_poisonEvents.Remove((PoisonEvent)callback);
						break;
					case CustomCallbackType.CurrentHealthChanged:
						if (_healthChangedEvent.Remove((HealthChangedEvent)callback))
							((HealthChangedEvent)callback).Invoke(this, this, Health, 0f);
						break;
					case CustomCallbackType.DamageChanged:
						if (_damageChangedEvent.Remove((DamageChangedEvent)callback))
							((DamageChangedEvent)callback).Invoke(this, Damage, 0f);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
				}
			}
		}
	}
}