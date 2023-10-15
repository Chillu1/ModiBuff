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
		IStatusEffectModifierOwnerLegalTarget<LegalAction, StatusEffectType>
	{
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }
		public float Mana { get; private set; }
		public float MaxMana { get; private set; }
		public float StatusResistance { get; private set; } = 1f;
		public UnitType UnitType { get; }
		public Vector2 Position { get; private set; }

		public bool IsDead { get; private set; }

		public ModifierController ModifierController { get; }

		public IMultiInstanceStatusEffectController<LegalAction, StatusEffectType> StatusEffectController =>
			_statusEffectController;

		//Note: These event lists should only be used for classic effects.
		//If you try to tie core game logic to them, you will most likely have trouble with sequence of events.
		private readonly List<IEffect> _whenAttackedEffects, _whenCastEffects, _whenDeathEffects, _whenHealedEffects;

		private readonly List<IEffect> _beforeAttackEffects,
			_onAttackEffects,
			_onCastEffects,
			_onKillEffects,
			_onHealEffects;

		private int _whenAttackedCount, _whenCastCount, _whenDeathCount, _whenHealedCount;
		private int _beforeAttackCount, _onAttackCount, _onCastCount, _onKillCount, _onHealCount;

		private readonly List<IEffect> _strongHitCallbacks;
		private UnitCallback _strongHitDelegateCallbacks;

		private readonly List<DispelEvent> _dispelEvents;
		private readonly List<HealthChangedEvent> _healthChangedEvent;
		private readonly List<DamageChangedEvent> _damageChangedEvent;
		private int _healthChangedCount = -1, _damageChangedCount;

		private readonly List<IUnit> _targetsInRange;
		private readonly List<Modifier> _auraModifiers;

		private readonly MultiInstanceStatusEffectController _statusEffectController;

		public Unit(float health = 500, float damage = 10, float healValue = 5, float mana = 1000,
			UnitType unitType = UnitType.Good)
		{
			Health = health;
			MaxHealth = health;
			Damage = damage;
			HealValue = healValue;
			Mana = mana;
			MaxMana = mana;
			UnitType = unitType;

			_whenAttackedEffects = new List<IEffect>();
			_whenCastEffects = new List<IEffect>();
			_whenDeathEffects = new List<IEffect>();
			_whenHealedEffects = new List<IEffect>();
			_beforeAttackEffects = new List<IEffect>();
			_onAttackEffects = new List<IEffect>();
			_onCastEffects = new List<IEffect>();
			_onKillEffects = new List<IEffect>();
			_onHealEffects = new List<IEffect>();

			_strongHitCallbacks = new List<IEffect>();

			_dispelEvents = new List<DispelEvent>();
			_healthChangedEvent = new List<HealthChangedEvent>();
			_damageChangedEvent = new List<DamageChangedEvent>();

			_targetsInRange = new List<IUnit>();
			_targetsInRange.Add(this);
			_auraModifiers = new List<Modifier>();

			ModifierController = new ModifierController(this);
			_statusEffectController = new MultiInstanceStatusEffectController();
		}

		public Unit(float health, float damage, ModifierAddReference[] modifierAddReferences, UnitType unitType)
			: this(health, damage, unitType: unitType)
		{
			foreach (var modifierAddReference in modifierAddReferences)
				ModifierController.TryAdd(modifierAddReference);
		}

		public void Update(float deltaTime)
		{
			_whenAttackedCount = _whenCastCount = _whenDeathCount = _whenHealedCount = 0;
			_beforeAttackCount = _onAttackCount = _onCastCount = _onKillCount = _onHealCount = 0;
			_healthChangedCount = -1;
			_damageChangedCount = 0;

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

			if (_beforeAttackCount == 0)
			{
				_beforeAttackCount++;
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

			this.ApplyAllAttackModifier(target);

			if (_onAttackCount == 0)
			{
				_onAttackCount++;
				for (int i = 0; i < _onAttackEffects.Count; i++)
					_onAttackEffects[i].Effect(target, this);
			}

			float dealtDamage = target.TakeDamage(Damage, this);

			if (target.Health <= 0 && _onKillCount == 0)
			{
				_onKillCount++;
				for (int i = 0; i < _onKillEffects.Count; i++)
					_onKillEffects[i].Effect(target, this);
			}

			return dealtDamage;
		}

		public float TakeDamage(float damage, IUnit source)
		{
			if (_whenAttackedCount == 0)
			{
				_whenAttackedCount++;
				for (int i = 0; i < _whenAttackedEffects.Count; i++)
					_whenAttackedEffects[i].Effect(this, source);
			}

			float oldHealth = Health;
			Health -= damage;
			float dealtDamage = oldHealth - Health;

			//TODO This counting becomes a problem, since we'll miss on damage after the first damage instance this frame
			//One solution could be to sum all the changes this frame, and then trigger the event once at the end of the frame.
			if (_healthChangedCount <= 0)
			{
				_healthChangedCount++;
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

			if (_whenDeathCount == 0 && Health <= 0 && !IsDead)
			{
				_whenDeathCount++;
				for (int i = 0; i < _whenDeathEffects.Count; i++)
					_whenDeathEffects[i].Effect(this, source);
				//Unit Death TODO Destroy/pool unit
				ModifierController.Clear();

				IsDead = true;
			}

			return dealtDamage;
		}

		public float Heal(float heal, IUnit source)
		{
			float oldHealth = Health;
			if (_whenHealedCount == 0)
			{
				_whenHealedCount++;
				for (int i = 0; i < _whenHealedEffects.Count; i++)
					_whenHealedEffects[i].Effect(this, source);
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

			if (_onHealCount == 0)
			{
				_onHealCount++;
				for (int i = 0; i < _onHealEffects.Count; i++)
					_onHealEffects[i].Effect(target, this);
			}

			return target.Heal(HealValue, this);
		}

		public void AddDamage(float damage)
		{
			Damage += damage;
			if (_damageChangedCount == 0)
			{
				_damageChangedCount++;
				for (int i = 0; i < _damageChangedEvent.Count; i++)
					_damageChangedEvent[i](this, Damage, damage);
			}
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

		public void AddEffectEvent(IEffect effect, EffectOnEvent @event)
		{
			switch (@event)
			{
				case EffectOnEvent.WhenAttacked:
					_whenAttackedEffects.Add(effect);
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
				ref var callback = ref reactCallbacks[i];
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
				ref var callback = ref reactCallbacks[i];
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