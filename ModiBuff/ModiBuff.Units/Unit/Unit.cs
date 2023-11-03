using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ModiBuff.Tests")]

namespace ModiBuff.Core.Units
{
	//Possible to implement through 3 ways, through interfaces:

	//IMasterDamage<float, float, float>, IMasterHealth<float, float, float, float>

	//NonGeneric default implementations (all float & other default Unit types):
	//IDamagable, IHealable, IAttacker, IHealer, IManaOwner, IHealthCost, IAddDamage, IEventOwner, IStatusEffectOwner

	//Or the manual generic one:
	public class Unit : IUpdatable, IModifierOwner, IAttacker<float, float>, IDamagable<float, float, float, float>,
		IHealable<float, float>, IHealer<float, float>, IManaOwner<float, float>, IHealthCost<float>, IAddDamage<float>,
		IPreAttacker, IEventOwner<EffectOnEvent>, IStatusEffectOwner<LegalAction, StatusEffectType>, IStatusResistance,
		ICallbackRegistrable<CallbackType>, IReactable<ReactType>, IPosition<Vector2>, IMovable<Vector2>, IUnitEntity,
		IStatusEffectModifierOwnerLegalTarget<LegalAction, StatusEffectType>, IPoisonable,
		ICustomCallbackRegistrable<CustomCallbackType>
	{
		public UnitTag UnitTag { get; private set; }
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }
		public float Mana { get; private set; }
		public float MaxMana { get; private set; }
		public float StatusResistance { get; private set; } = 1f;
		public UnitType UnitType { get; }
		public Vector2 Position { get; private set; }
		public int PoisonStacks { get; private set; }

		public bool IsDead { get; private set; }

		public ModifierController ModifierController { get; }

		public IMultiInstanceStatusEffectController<LegalAction, StatusEffectType> StatusEffectController =>
			_statusEffectController;

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
		private UnitCallback _strongHitDelegateCallbacks;

		private readonly List<PoisonEvent> _poisonEvents;

		private readonly List<DispelEvent> _dispelEvents;
		private readonly List<HealthChangedEvent> _healthChangedEvent;
		private readonly List<DamageChangedEvent> _damageChangedEvent;

		private readonly List<IUnit> _targetsInRange;
		private readonly List<Modifier> _auraModifiers;

		private readonly MultiInstanceStatusEffectController _statusEffectController;

		public Unit(float health = 500, float damage = 10, float healValue = 5, float mana = 1000,
			UnitType unitType = UnitType.Good, UnitTag unitTag = UnitTag.Default)
		{
			UnitTag = unitTag;

			Health = health;
			MaxHealth = health;
			Damage = damage;
			HealValue = healValue;
			Mana = mana;
			MaxMana = mana;
			UnitType = unitType;

			_whenAttackedEffects = new List<IEffect>();
			_afterAttackedEffects = new List<IEffect>();
			_whenCastEffects = new List<IEffect>();
			_whenDeathEffects = new List<IEffect>();
			_whenHealedEffects = new List<IEffect>();
			_beforeAttackEffects = new List<IEffect>();
			_onAttackEffects = new List<IEffect>();
			_onCastEffects = new List<IEffect>();
			_onKillEffects = new List<IEffect>();
			_onHealEffects = new List<IEffect>();

			_strongHitCallbacks = new List<IEffect>();

			_poisonEvents = new List<PoisonEvent>();

			_dispelEvents = new List<DispelEvent>();
			_healthChangedEvent = new List<HealthChangedEvent>();
			_damageChangedEvent = new List<DamageChangedEvent>();

			_targetsInRange = new List<IUnit>();
			_targetsInRange.Add(this);
			_auraModifiers = new List<Modifier>();

			ModifierController = new ModifierController(this);
			_statusEffectController = new MultiInstanceStatusEffectController();
		}

		public Unit(float health, float damage, ModifierAddReference[] modifierAddReferences,
			UnitType unitType, UnitTag unitTag)
			: this(health, damage, unitType: unitType, unitTag: unitTag)
		{
			foreach (var modifierAddReference in modifierAddReferences)
				ModifierController.TryAdd(modifierAddReference);
		}

		public void Update(float deltaTime)
		{
			_statusEffectController.Update(deltaTime);
			ModifierController.Update(deltaTime);
			for (int i = 0; i < _auraModifiers.Count; i++)
				_auraModifiers[i].Update(deltaTime);
		}

		/// <summary>
		///		Should be called before we attack/on attack. For modifiers like split shot that we want to trigger when starting an attack.
		/// </summary>
		public void PreAttack(IUnit target)
		{
			if (!_statusEffectController.HasLegalAction(LegalAction.Act))
				return;

			_preAttackCounter++;

			if (_preAttackCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _beforeAttackEffects.Count; i++)
					_beforeAttackEffects[i].Effect(target, this);
			}
		}

		public float Attack(IUnit target)
		{
			return Attack((Unit)target);
		}

		public float Attack(Unit target)
		{
			if (!_statusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			_onAttackCounter++;

			this.ApplyAllAttackModifier(target);

			if (_onAttackCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _onAttackEffects.Count; i++)
					_onAttackEffects[i].Effect(target, this);
			}

			float dealtDamage = target.TakeDamage(Damage, this);

			if (target.Health <= 0)
			{
				_onKillCounter++;

				if (_onKillCounter <= MaxRecursionEventCount)
					for (int i = 0; i < _onKillEffects.Count; i++)
						_onKillEffects[i].Effect(target, this);
			}

			if (_onAttackCounter <= MaxRecursionEventCount &&
			    _onKillCounter <= MaxRecursionEventCount)
			{
				target.ResetEventCounters();
				ResetEventCounters();
			}

			return dealtDamage;
		}

		public float TakeDamage(float damage, IUnit source)
		{
			_whenAttackedCounter++;
			if (_whenAttackedCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _whenAttackedEffects.Count; i++)
					_whenAttackedEffects[i].Effect(this, source);
			}

			float oldHealth = Health;
			Health -= damage;
			float dealtDamage = oldHealth - Health;

			_afterAttackedCounter++;
			if (_afterAttackedCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _afterAttackedEffects.Count; i++)
					_afterAttackedEffects[i].Effect(this, source);
			}

			_healthChangedCounter++;
			if (_healthChangedCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _healthChangedEvent.Count; i++)
					_healthChangedEvent[i](this, source, Health, dealtDamage);
			}

			//if damage was bigger than half health, trigger strong attack callbacks
			if (dealtDamage > MaxHealth * 0.5f)
			{
				for (int i = 0; i < _strongHitCallbacks.Count; i++)
					_strongHitCallbacks[i].Effect(this, source);
				_strongHitDelegateCallbacks?.Invoke(this, source);
			}

			if (Health <= 0 && !IsDead)
			{
				for (int i = 0; i < _whenDeathEffects.Count; i++)
					_whenDeathEffects[i].Effect(this, source);

				//Unit Death TODO Destroy/pool unit
				ModifierController.Clear();

				IsDead = true;
			}

			if (_whenAttackedCounter <= MaxRecursionEventCount &&
			    _afterAttackedCounter <= MaxRecursionEventCount &&
			    _healthChangedCounter <= MaxRecursionEventCount)
			{
				ResetEventCounters();
				(source as IEventOwner)?.ResetEventCounters();
			}

			return dealtDamage;
		}

		public float Heal(float heal, IUnit source)
		{
			_healCounter++;

			float oldHealth = Health;
			if (_healCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _whenHealedEffects.Count; i++)
					_whenHealedEffects[i].Effect(this, source);
			}

			if (_healCounter <= MaxRecursionEventCount)
			{
				ResetEventCounters();
				(source as IEventOwner)?.ResetEventCounters();
			}

			Health += heal;
			if (Health > MaxHealth)
				Health = MaxHealth;
			return Health - oldHealth;
		}

		public float Heal(IHealable<float, float> target)
		{
			if (!_statusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			_healTargetCounter++;

			if (_healTargetCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _onHealEffects.Count; i++)
					_onHealEffects[i].Effect(target, this);
			}

			float valueHealed = target.Heal(HealValue, this);

			if (_healTargetCounter <= MaxRecursionEventCount)
			{
				(target as IEventOwner)?.ResetEventCounters();
				ResetEventCounters();
			}

			return valueHealed;
		}

		public void AddDamage(float damage)
		{
			_addDamageCounter++;

			Damage += damage;
			if (_addDamageCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _damageChangedEvent.Count; i++)
					_damageChangedEvent[i](this, Damage, damage);
			}

			if (_addDamageCounter <= MaxRecursionEventCount)
				ResetEventCounters();
		}

		public void UseHealth(float value)
		{
			Health -= value;
		}

		public void UseMana(float value)
		{
			Mana -= value;
		}

		public void Move(Vector2 value) => Move(value.X, value.Y);

		public void Move(float x, float y)
		{
			var position = Position;
			position.X += x;
			position.Y += y;
			Position = position;
		}

		public float TakeDamagePoison(float damage, int stacks, int totalStacks, IUnit source)
		{
			PoisonStacks = totalStacks;

			float dealtDamage = TakeDamage(damage, source);

			float oldHealth = Health;

			_poisonDamageCounter++;
			if (_poisonDamageCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _poisonEvents.Count; i++)
					_poisonEvents[i](this, source, stacks, totalStacks, dealtDamage);
			}

			if (_poisonDamageCounter <= MaxRecursionEventCount)
			{
				ResetEventCounters();
				(source as IEventOwner)?.ResetEventCounters();
			}

			return dealtDamage + oldHealth - Health;
		}

		//---StatusResistances---

		public void ChangeStatusResistance(float value)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (value <= 0)
			{
				Logger.LogError("StatusResistance can't be negative or zero.");
				return;
			}
#endif
			StatusResistance = value;
		}

		//---Modifier based---

		public void Dispel(TagType tag, IUnit source)
		{
			for (int i = 0; i < _dispelEvents.Count; i++)
				_dispelEvents[i](this, source, tag);
		}

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

		public void RegisterReact(ReactCallback<ReactType>[] reactCallbacks)
		{
			for (int i = 0; i < reactCallbacks.Length; i++)
			{
				ref readonly var callback = ref reactCallbacks[i];
				switch (callback.ReactType)
				{
					case ReactType.Dispel:
						if (!(callback.Action is DispelEvent dispelEvent))
						{
							Logger.LogError(
								"objectDelegate is not of type DispelEvent, use named delegates instead.");
							break;
						}

						//dispelEvent.DynamicInvoke(this, this, TagType.None);
						_dispelEvents.Add(dispelEvent);
						break;
					case ReactType.CurrentHealthChanged:
						if (!(callback.Action is HealthChangedEvent healthEvent))
						{
							Logger.LogError(
								"objectDelegate is not of type HealthChangedEvent, use named delegates instead.");
							break;
						}

						healthEvent.DynamicInvoke(this, this, Health, 0f);
						_healthChangedEvent.Add(healthEvent);
						break;
					case ReactType.DamageChanged:
						if (!(callback.Action is DamageChangedEvent damageEvent))
						{
							Logger.LogError(
								"objectDelegate is not of type DamagedChangedEvent, use named delegates instead.");
							break;
						}

						damageEvent.DynamicInvoke(this, Damage, 0f);
						_damageChangedEvent.Add(damageEvent);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(reactCallbacks), callback.ReactType, null);
				}
			}
		}

		public void UnRegisterReact(ReactCallback<ReactType>[] reactCallbacks)
		{
			for (int i = 0; i < reactCallbacks.Length; i++)
			{
				ref readonly var callback = ref reactCallbacks[i];
				switch (callback.ReactType)
				{
					case ReactType.Dispel:
						//TODO Always revert internal effect?
						var dispelEvent = (DispelEvent)callback.Action;
						_dispelEvents.Remove(dispelEvent);
						//dispelEvent.DynamicInvoke(this, this, TagType.None);
						break;
					case ReactType.CurrentHealthChanged:
						//TODO Always revert internal effect?
						var healthChangedEvent = (HealthChangedEvent)callback.Action;
						if (_healthChangedEvent.Remove(healthChangedEvent))
							healthChangedEvent.DynamicInvoke(this, this, Health, 0f);
#if DEBUG && !MODIBUFF_PROFILE
						else
							Logger.LogError("Could not remove healthChangedEvent: " + healthChangedEvent);
#endif
						break;
					case ReactType.DamageChanged:
						//TODO Always revert internal effect?
						var damageChangedEvent = (DamageChangedEvent)callback.Action;
						if (_damageChangedEvent.Remove(damageChangedEvent))
							damageChangedEvent.DynamicInvoke(this, Damage, 0f);
#if DEBUG && !MODIBUFF_PROFILE
						else
							Logger.LogError("Could not remove damageChangedEvent: " + damageChangedEvent);
#endif
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(reactCallbacks), callback.ReactType, null);
				}
			}
		}

		public void RegisterCallbacks(CustomCallback<CustomCallbackType>[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				ref readonly var callback = ref callbacks[i];
				switch (callback.CallbackType)
				{
					case CustomCallbackType.PoisonDamage:
						if (!(callback.Action is PoisonEvent poisonEvent))
						{
							Logger.LogError(
								"objectDelegate is not of type HealthChangedEvent, use named delegates instead.");
							break;
						}

						_poisonEvents.Add(poisonEvent);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(callbacks), callback.CallbackType, null);
				}
			}
		}

		//---Aura---

		public void AddCloseTargets(params Unit[] targets)
		{
			_targetsInRange.AddRange(targets);
		}

		public void AddAuraModifier(int id)
		{
			var modifier = ModifierPool.Instance.Rent(id);
			modifier.UpdateTargets(_targetsInRange, this);
			_auraModifiers.Add(modifier);
		}

		public override string ToString()
		{
			return $"Health: {Health}, Damage: {Damage}, HealValue: {HealValue}";
		}
	}
}