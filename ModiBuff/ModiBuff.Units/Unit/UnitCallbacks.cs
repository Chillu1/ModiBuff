using System;
using System.Collections.Generic;

namespace ModiBuff.Core.Units
{
	//Often updated delegates
	public delegate void HealthChangedEvent(IUnit target, IUnit source, float newHealth, float deltaHealth);

	public delegate void UpdateTimerEvent();

	public delegate void CastEvent(IUnit target, IUnit source, int castId);

	public delegate void DispelEvent(IUnit target, IUnit source, TagType tag);

	public delegate void StrongDispelEvent(IUnit target, IUnit source);

	public delegate void PoisonEvent(IUnit target, IUnit source, int poisonStacks, int totalStacks, float dealtDamage);

	public delegate void StatusEffectEvent(IUnit target, IUnit source, StatusEffectType statusEffect,
		LegalAction oldLegalAction, LegalAction newLegalAction);

	//Rarely updated delegates
	public delegate void DamageChangedEvent(IUnit unit, float newDamage, float deltaDamage);

	public partial class Unit
	{
		public const int MaxRecursionEventCount = 1;
		public const int MaxEventCount = 1 + MaxRecursionEventCount;

		private int _preAttackCounter,
			_onAttackCounter,
			_whenAttackedCounter,
			_afterAttackedCounter,
			_healthChangedCounter,
			_onCastCounter,
			_onKillCounter,
			_healCounter,
			_healTargetCounter,
			_addDamageCounter;

		private int _poisonDamageCounter;

		//Note: These event lists should only be used for classic effects.
		//If you try to tie core game logic to them, you will most likely have trouble with sequence of events.
		private readonly List<IEffect> _whenAttackedEffects,
			_afterAttackedEffects,
			_whenDeathEffects,
			_whenHealedEffects;

		private readonly List<IEffect> _beforeAttackEffects,
			_onAttackEffects,
			_onCastEffects,
			_onKillEffects,
			_onHealEffects;

		private readonly List<IEffect> _strongDispelCallbacks;
		private readonly List<IEffect> _strongHitCallbacks;
		private readonly List<UnitCallback> _strongHitUnitCallbacks;

		private readonly List<PoisonEvent> _poisonEvents;

		private readonly List<DispelEvent> _dispelEvents;
		private readonly List<StrongDispelEvent> _strongDispelEvents;
		private readonly List<HealthChangedEvent> _healthChangedEvents;
		private readonly List<DamageChangedEvent> _damageChangedEvents;
		private readonly List<StatusEffectEvent> _statusEffectAddedEvents;
		private readonly List<StatusEffectEvent> _statusEffectRemovedEvents;
		private readonly List<CastEvent> _onCastEvents;

		private readonly List<UpdateTimerEvent> _updateTimerCallbacks;

		public const float CallbackTimerCooldown = 1f;
		private float _callbackTimer;

		/// <summary>
		///		Resets all event/callback counters, so we can trigger them again
		/// </summary>
		/// <remarks>We always reset all counters because event effects might trigger other callbacks as well</remarks>
		public void ResetEventCounters()
		{
			_preAttackCounter = _onAttackCounter = _whenAttackedCounter = _afterAttackedCounter =
				_healthChangedCounter = _onCastCounter = _onKillCounter = _healCounter =
					_healTargetCounter = _addDamageCounter = _poisonDamageCounter = 0;
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
					Logger.LogError("[ModiBuff.Units] Unknown event type: " + @event);
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
					Logger.LogError("[ModiBuff.Units] Unknown event type: " + @event);
#endif
					return;
			}

			void Remove(List<IEffect> effects, IEffect effectToRemove)
			{
				bool remove = effects.Remove(effectToRemove);

#if DEBUG && !MODIBUFF_PROFILE
				if (!remove)
					Logger.LogError("[ModiBuff.Units] Could not remove event: " + effectToRemove.GetType());
#endif
			}
		}

		//---Callbacks---

		//Something to think about when implementing callbacks:
		//single delegates are 76% faster than arrays of IEffects with 1 subscriber/item
		//But arrays are much faster when there are multiple subscribers/items, 58% faster with 2 items, 150% faster with 5 items.
		//It's recommended to use the array version generally. But in cases where most modifiers have single callbacks, use delegates.
		public void RegisterCallback(CallbackType callbackType, object callback)
		{
			switch (callbackType)
			{
				case CallbackType.Dispel:
					if (callback.CheckCallback(out DispelEvent dispelEvent))
						_dispelEvents.Add(dispelEvent);
					break;
				case CallbackType.StrongDispel:
					if (callback.CheckCallback(out StrongDispelEvent strongDispelEvent))
						_strongDispelEvents.Add(strongDispelEvent);
					break;
				case CallbackType.PoisonDamage:
					if (callback.CheckCallback(out PoisonEvent poisonEvent))
						_poisonEvents.Add(poisonEvent);
					break;
				case CallbackType.CurrentHealthChanged:
					if (callback.CheckCallback(out HealthChangedEvent healthEvent))
					{
						healthEvent.Invoke(this, this, Health, 0f);
						_healthChangedEvents.Add(healthEvent);
					}

					break;
				case CallbackType.DamageChanged:
					if (callback.CheckCallback(out DamageChangedEvent damageEvent))
					{
						damageEvent.Invoke(this, Damage, 0f);
						_damageChangedEvents.Add(damageEvent);
					}

					break;
				case CallbackType.StrongHit:
					if (callback.CheckCallback(out UnitCallback unitCallback))
						_strongHitUnitCallbacks.Add(unitCallback);
					break;
				case CallbackType.StatusEffectAdded:
					if (callback.CheckCallback(out StatusEffectEvent statusEffectEvent))
					{
						_statusEffectController.TriggerEvent(statusEffectEvent);
						_statusEffectAddedEvents.Add(statusEffectEvent);
					}

					break;
				case CallbackType.StatusEffectRemoved:
					if (callback.CheckCallback(out StatusEffectEvent statusEffectRemovedEvent))
					{
						_statusEffectController.TriggerEvent(statusEffectRemovedEvent);
						_statusEffectRemovedEvents.Add(statusEffectRemovedEvent);
					}

					break;
				case CallbackType.OnCast:
					if (callback.CheckCallback(out CastEvent castEvent))
						_onCastEvents.Add(castEvent);
					break;
				case CallbackType.Update:
					if (callback.CheckCallback(out UpdateTimerEvent updateTimerEvent))
					{
						updateTimerEvent.Invoke();
						_updateTimerCallbacks.Add(updateTimerEvent);
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
				case CallbackType.StrongDispel:
					_strongDispelEvents.Remove((StrongDispelEvent)callback);
					break;
				case CallbackType.PoisonDamage:
					_poisonEvents.Remove((PoisonEvent)callback);
					break;
				case CallbackType.CurrentHealthChanged:
					var healthChangedEvent = (HealthChangedEvent)callback;
					if (_healthChangedEvents.Remove(healthChangedEvent))
						healthChangedEvent.Invoke(this, this, Health, 0f);
					break;
				case CallbackType.DamageChanged:
					var damageChangedEvent = (DamageChangedEvent)callback;
					if (_damageChangedEvents.Remove(damageChangedEvent))
						damageChangedEvent.Invoke(this, Damage, 0f);
					break;
				case CallbackType.StrongHit:
					_strongHitUnitCallbacks.Remove((UnitCallback)callback);
					break;
				case CallbackType.StatusEffectAdded:
					var statusEffectEvent = (StatusEffectEvent)callback;
					if (_statusEffectAddedEvents.Remove(statusEffectEvent))
						_statusEffectController.TriggerEvent(statusEffectEvent);
					break;
				case CallbackType.StatusEffectRemoved:
					var statusEffectRemovedEvent = (StatusEffectEvent)callback;
					if (_statusEffectRemovedEvents.Remove(statusEffectRemovedEvent))
						_statusEffectController.TriggerEvent(statusEffectRemovedEvent);
					break;
				case CallbackType.OnCast:
					_onCastEvents.Remove((CastEvent)callback);
					break;
				case CallbackType.Update:
					var updateTimerEvent = (UpdateTimerEvent)callback;
					if (_updateTimerCallbacks.Remove(updateTimerEvent))
						updateTimerEvent.Invoke();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}

		public void RegisterCallbacks(CallbackUnitType callbackType, IEffect[] callbacks)
		{
			switch (callbackType)
			{
				case CallbackUnitType.StrongDispel:
					_strongDispelCallbacks.AddRange(callbacks);
					break;
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
				case CallbackUnitType.StrongDispel:
					for (int i = 0; i < callbacks.Length; i++)
					{
						bool removed = _strongDispelCallbacks.Remove(callbacks[i]);
#if DEBUG && !MODIBUFF_PROFILE
						if (!removed)
							Logger.LogError("[ModiBuff.Units] Could not remove callback: " + callbacks[i]);
#endif
					}

					break;
				case CallbackUnitType.StrongHit:
					for (int i = 0; i < callbacks.Length; i++)
					{
						bool removed = _strongHitCallbacks.Remove(callbacks[i]);
#if DEBUG && !MODIBUFF_PROFILE
						if (!removed)
							Logger.LogError("[ModiBuff.Units] Could not remove callback: " + callbacks[i]);
#endif
					}

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(callbackType), callbackType, null);
			}
		}
	}
}