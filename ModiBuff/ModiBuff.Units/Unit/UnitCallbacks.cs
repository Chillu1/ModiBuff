using System;
using System.Collections.Generic;

namespace ModiBuff.Core.Units
{
	//Often updated delegates
	public delegate void HealthChangedEvent(IUnit target, IUnit source, float newHealth, float deltaHealth);

	public delegate void DispelEvent(IUnit target, IUnit source, TagType tag);

	public delegate void PoisonEvent(IUnit target, IUnit source, int poisonStacks, int totalStacks, float dealtDamage);

	public delegate void StatusEffectEvent(IUnit target, IUnit source, StatusEffectType statusEffect,
		LegalAction oldLegalAction, LegalAction newLegalAction);

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

		private readonly List<PoisonEvent> _poisonEvents;

		private readonly List<DispelEvent> _dispelEvents;
		private readonly List<HealthChangedEvent> _healthChangedEvents;
		private readonly List<DamageChangedEvent> _damageChangedEvents;
		private readonly List<StatusEffectEvent> _statusEffectAddedEvents;
		private readonly List<StatusEffectEvent> _statusEffectRemovedEvents;

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
		public void RegisterCallbacks(CallbackUnitType callbackType, IEffect[] callbacks)
		{
			switch (callbackType)
			{
				case CallbackUnitType.StrongHit:
					_strongHitCallbacks.AddRange(callbacks);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void UnRegisterCallbacks(CallbackUnitType callbackType, IEffect[] callbacks)
		{
			switch (callbackType)
			{
				case CallbackUnitType.StrongHit:
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

		public void RegisterCallbacks(Callback<CallbackType>[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				ref readonly var callback = ref callbacks[i];
				switch (callback.CallbackType)
				{
					case CallbackType.Dispel:
						if (CheckCallback(callback.Action, out DispelEvent dispelEvent))
							_dispelEvents.Add(dispelEvent);
						break;
					case CallbackType.PoisonDamage:
						if (CheckCallback(callback.Action, out PoisonEvent poisonEvent))
							_poisonEvents.Add(poisonEvent);
						break;
					case CallbackType.CurrentHealthChanged:
						if (CheckCallback(callback.Action, out HealthChangedEvent healthEvent))
						{
							healthEvent.Invoke(this, this, Health, 0f);
							_healthChangedEvents.Add(healthEvent);
						}

						break;
					case CallbackType.DamageChanged:
						if (CheckCallback(callback.Action, out DamageChangedEvent damageEvent))
						{
							damageEvent.Invoke(this, Damage, 0f);
							_damageChangedEvents.Add(damageEvent);
						}

						break;
					case CallbackType.StrongHit:
						if (CheckCallback(callback.Action, out UnitCallback unitCallback))
							_strongHitUnitCallbacks.Add(unitCallback);
						break;
					case CallbackType.StatusEffectAdded:
						if (CheckCallback(callback.Action, out StatusEffectEvent statusEffectEvent))
						{
							_statusEffectController.TriggerEvent(statusEffectEvent);
							_statusEffectAddedEvents.Add(statusEffectEvent);
						}

						break;
					case CallbackType.StatusEffectRemoved:
						if (CheckCallback(callback.Action, out StatusEffectEvent statusEffectRemovedEvent))
						{
							_statusEffectController.TriggerEvent(statusEffectRemovedEvent);
							_statusEffectRemovedEvents.Add(statusEffectRemovedEvent);
						}

						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(callbacks), callback.CallbackType, null);
				}
			}
		}

		public void UnRegisterCallbacks(Callback<CallbackType>[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				ref readonly var callback = ref callbacks[i];
				switch (callback.CallbackType)
				{
					case CallbackType.Dispel:
						_dispelEvents.Remove((DispelEvent)callback.Action);
						break;
					case CallbackType.PoisonDamage:
						_poisonEvents.Remove((PoisonEvent)callback.Action);
						break;
					case CallbackType.CurrentHealthChanged:
						if (_healthChangedEvents.Remove((HealthChangedEvent)callback.Action))
							((HealthChangedEvent)callback.Action).Invoke(this, this, Health, 0f);
						break;
					case CallbackType.DamageChanged:
						if (_damageChangedEvents.Remove((DamageChangedEvent)callback.Action))
							((DamageChangedEvent)callback.Action).Invoke(this, Damage, 0f);
						break;
					case CallbackType.StrongHit:
						_strongHitUnitCallbacks.Remove((UnitCallback)callback.Action);
						break;
					case CallbackType.StatusEffectAdded:
						if (_statusEffectAddedEvents.Remove((StatusEffectEvent)callback.Action))
							_statusEffectController.TriggerEvent((StatusEffectEvent)callback.Action);
						break;
					case CallbackType.StatusEffectRemoved:
						if (_statusEffectRemovedEvents.Remove((StatusEffectEvent)callback.Action))
							_statusEffectController.TriggerEvent((StatusEffectEvent)callback.Action);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void RegisterCallback(CallbackType callbackType, object callback)
		{
			switch (callbackType)
			{
				case CallbackType.Dispel:
					if (CheckCallback(callback, out DispelEvent dispelEvent))
						_dispelEvents.Add(dispelEvent);
					break;
				case CallbackType.PoisonDamage:
					if (CheckCallback(callback, out PoisonEvent poisonEvent))
						_poisonEvents.Add(poisonEvent);
					break;
				case CallbackType.CurrentHealthChanged:
					if (CheckCallback(callback, out HealthChangedEvent healthEvent))
					{
						healthEvent.Invoke(this, this, Health, 0f);
						_healthChangedEvents.Add(healthEvent);
					}

					break;
				case CallbackType.DamageChanged:
					if (CheckCallback(callback, out DamageChangedEvent damageEvent))
					{
						damageEvent.Invoke(this, Damage, 0f);
						_damageChangedEvents.Add(damageEvent);
					}

					break;
				case CallbackType.StatusEffectAdded:
					if (CheckCallback(callback, out StatusEffectEvent statusEffectEvent))
					{
						_statusEffectController.TriggerEvent(statusEffectEvent);
						_statusEffectAddedEvents.Add(statusEffectEvent);
					}

					break;
				case CallbackType.StatusEffectRemoved:
					if (CheckCallback(callback, out StatusEffectEvent statusEffectRemovedEvent))
					{
						_statusEffectController.TriggerEvent(statusEffectRemovedEvent);
						_statusEffectRemovedEvents.Add(statusEffectRemovedEvent);
					}

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void UnRegisterCallback(CallbackType callbackType, object callback)
		{
			switch (callbackType)
			{
				case CallbackType.Dispel:
					_dispelEvents.Remove((DispelEvent)callback);
					break;
				case CallbackType.PoisonDamage:
					_poisonEvents.Remove((PoisonEvent)callback);
					break;
				case CallbackType.CurrentHealthChanged:
					if (_healthChangedEvents.Remove((HealthChangedEvent)callback))
						((HealthChangedEvent)callback).Invoke(this, this, Health, 0f);
					break;
				case CallbackType.DamageChanged:
					if (_damageChangedEvents.Remove((DamageChangedEvent)callback))
						((DamageChangedEvent)callback).Invoke(this, Damage, 0f);
					break;
				case CallbackType.StatusEffectAdded:
					if (_statusEffectAddedEvents.Remove((StatusEffectEvent)callback))
						_statusEffectController.TriggerEvent((StatusEffectEvent)callback);
					break;
				case CallbackType.StatusEffectRemoved:
					if (_statusEffectRemovedEvents.Remove((StatusEffectEvent)callback))
						_statusEffectController.TriggerEvent((StatusEffectEvent)callback);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void RegisterCallbacks(CallbackType callbackType, object[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
				RegisterCallback(callbackType, callbacks[i]);
		}

		public void UnRegisterCallbacks(CallbackType callbackType, object[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
				UnRegisterCallback(callbackType, callbacks[i]);
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
}