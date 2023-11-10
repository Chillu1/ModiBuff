using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ModiBuff.Core.Units.Interfaces.NonGeneric;

[assembly: InternalsVisibleTo("ModiBuff.Tests")]

namespace ModiBuff.Core.Units
{
	//Possible to implement through 3 ways, through interfaces:

	//IMasterDamage<float, float, float>, IMasterHealth<float, float, float, float>

	//NonGeneric default implementations (all float & other default Unit types):
	//IDamagable, IHealable, IAttacker, IHealer, IManaOwner, IHealthCost, IAddDamage, IEventOwner, IStatusEffectOwner

	//Or the manual generic one:
	public partial class Unit : IUpdatable, IModifierOwner, IModifierApplierOwner, IAttacker<float, float>,
		IDamagable<float, float, float, float>, IHealable<float, float>, IHealer<float, float>,
		IManaOwner<float, float>, IHealthCost<float>, IAddDamage<float>, IPreAttacker, IEventOwner<EffectOnEvent>,
		IStatusEffectOwner<LegalAction, StatusEffectType>, IStatusResistance, IKillable,
		ICallbackUnitRegistrable<CallbackUnitType>,
		IPosition<Vector2>, IMovable<Vector2>, IUnitEntity,
		IStatusEffectModifierOwnerLegalTarget<LegalAction, StatusEffectType>, IPoisonable,
		ICallbackRegistrable<CallbackType>, ISingleInstanceStatusEffectOwner<LegalAction, StatusEffectType>,
		ICallbackEffectRegistrable<CallbackType>, IAllNonGeneric, ICaster
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
		public ModifierApplierController ModifierApplierController { get; }

		//Note: use one of these, not both
		public IMultiInstanceStatusEffectController<LegalAction, StatusEffectType> StatusEffectController =>
			_statusEffectController;

		ISingleInstanceStatusEffectController<LegalAction, StatusEffectType>
			ISingleInstanceStatusEffectOwner<LegalAction, StatusEffectType>.StatusEffectController =>
			_singleInstanceStatusEffectController;

		private readonly List<IUnit> _targetsInRange;
		private readonly List<Modifier> _auraModifiers;

		private readonly MultiInstanceStatusEffectController _statusEffectController;
		private readonly StatusEffectController _singleInstanceStatusEffectController;

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
			_whenDeathEffects = new List<IEffect>();
			_whenHealedEffects = new List<IEffect>();
			_beforeAttackEffects = new List<IEffect>();
			_onAttackEffects = new List<IEffect>();
			_onCastEffects = new List<IEffect>();
			_onKillEffects = new List<IEffect>();
			_onHealEffects = new List<IEffect>();

			_strongHitCallbacks = new List<IEffect>();
			_strongHitUnitCallbacks = new List<UnitCallback>();

			_poisonEvents = new List<PoisonEvent>();

			_dispelEvents = new List<DispelEvent>();
			_healthChangedEvents = new List<HealthChangedEvent>();
			_damageChangedEvents = new List<DamageChangedEvent>();
			_statusEffectAddedEvents = new List<StatusEffectEvent>();
			_statusEffectRemovedEvents = new List<StatusEffectEvent>();

			_targetsInRange = new List<IUnit>();
			_targetsInRange.Add(this);
			_auraModifiers = new List<Modifier>();

			ModifierController = ModifierControllerPool.Instance.Rent();
			ModifierApplierController = ModifierControllerPool.Instance.RentApplier();
			_statusEffectController = new MultiInstanceStatusEffectController
				(this, _statusEffectAddedEvents, _statusEffectRemovedEvents);
			_singleInstanceStatusEffectController = new StatusEffectController();
		}

		public Unit(float health, float damage, ModifierAddReference[] modifierAddReferences,
			UnitType unitType, UnitTag unitTag)
			: this(health, damage, unitType: unitType, unitTag: unitTag)
		{
			foreach (var modifierAddReference in modifierAddReferences)
				this.TryAddModifierReference(modifierAddReference);
		}

		public void Update(float deltaTime)
		{
			_statusEffectController.Update(deltaTime);
			_singleInstanceStatusEffectController.Update(deltaTime);
			ModifierController.Update(deltaTime);
			ModifierApplierController.Update(deltaTime);
			for (int i = 0; i < _auraModifiers.Count; i++)
				_auraModifiers[i].Update(deltaTime);
		}

		/// <summary>
		///		Should be called before we attack/on attack. For modifiers like split shot that we want to trigger when starting an attack.
		/// </summary>
		public void PreAttack(IUnit target)
		{
			if (target is IUnitEntity entity && !UnitType.IsLegalTarget(entity.UnitType))
				return;
			if (!_statusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_singleInstanceStatusEffectController.HasLegalAction(LegalAction.Act))
				return;

			_preAttackCounter++;

			if (_preAttackCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _beforeAttackEffects.Count; i++)
					_beforeAttackEffects[i].Effect(target, this);
			}

			if (_preAttackCounter <= MaxRecursionEventCount)
			{
				ResetEventCounters();
				(target as IEventOwner)?.ResetEventCounters();
			}
		}

		/// <summary>
		///		Would be an attack command in ex. a moba. So we can't always attack our allies our ourselves at will.
		/// </summary>
		public float TryAttackCommand(IUnit target)
		{
			if (target is IUnitEntity entity && !UnitType.IsLegalTarget(entity.UnitType))
				return 0;

			return Attack(target);
		}

		/// <summary>
		///		Performs an attack if possible (not disarmed, not stunned)
		/// </summary>
		public float Attack(IUnit target)
		{
			if (!_statusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_singleInstanceStatusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			var killableTarget = target as IKillable;
			bool wasDead = killableTarget != null && killableTarget.IsDead;

			_onAttackCounter++;

			if (target is IModifierOwner modifierOwner)
				this.ApplyAllAttackModifier(modifierOwner);

			if (_onAttackCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _onAttackEffects.Count; i++)
					_onAttackEffects[i].Effect(target, this);
			}

			float dealtDamage = 0;
			if (target is IAttackable<float, float> damagableTarget)
			{
				dealtDamage = damagableTarget.TakeDamage(Damage, this);

				if (killableTarget != null && killableTarget.IsDead && !wasDead)
				{
					_onKillCounter++;

					if (_onKillCounter <= MaxRecursionEventCount)
						for (int i = 0; i < _onKillEffects.Count; i++)
							_onKillEffects[i].Effect(target, this);
				}
			}

			if (_onAttackCounter <= MaxRecursionEventCount &&
			    _onKillCounter <= MaxRecursionEventCount)
			{
				ResetEventCounters();
				(target as IEventOwner)?.ResetEventCounters();
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
			float newHealth = oldHealth - damage;
			float dealtDamage = oldHealth - newHealth;
			Health = newHealth;

			_afterAttackedCounter++;
			if (_afterAttackedCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _afterAttackedEffects.Count; i++)
					_afterAttackedEffects[i].Effect(this, source);
			}

			if (dealtDamage > 0)
			{
				_healthChangedCounter++;
				if (_healthChangedCounter <= MaxRecursionEventCount)
				{
					for (int i = 0; i < _healthChangedEvents.Count; i++)
						_healthChangedEvents[i](this, source, Health, dealtDamage);
				}
			}

			//if damage was bigger than half health, trigger strong attack callbacks
			if (dealtDamage > MaxHealth * 0.5f)
			{
				for (int i = 0; i < _strongHitCallbacks.Count; i++)
					_strongHitCallbacks[i].Effect(this, source);
				for (int i = 0; i < _strongHitUnitCallbacks.Count; i++)
					_strongHitUnitCallbacks[i](this, source);
			}

			if (Health <= 0 && !IsDead)
			{
				for (int i = 0; i < _whenDeathEffects.Count; i++)
					_whenDeathEffects[i].Effect(this, source);

				//Unit Death TODO Destroy/pool unit
				ModifierControllerPool.Instance.Return(ModifierController);
				ModifierControllerPool.Instance.ReturnApplier(ModifierApplierController);

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
			if (!_statusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_singleInstanceStatusEffectController.HasLegalAction(LegalAction.Act))
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
				ResetEventCounters();
				(target as IEventOwner)?.ResetEventCounters();
			}

			return valueHealed;
		}

		public void TryCast(int modifierId, IUnit target)
		{
			if (!(target is IModifierOwner modifierTarget))
				return;
			if (!modifierId.IsLegalTarget((IUnitEntity)target, this))
				return;
			if (!StatusEffectController.HasLegalAction(LegalAction.Cast))
				return;
			if (!this.CanCastModifier(modifierId))
				return;

			_onCastCounter++;
			if (_onCastCounter <= MaxRecursionEventCount)
				for (int i = 0; i < _onCastEffects.Count; i++)
					_onCastEffects[i].Effect(target, this);

			modifierTarget.ModifierController.Add(modifierId, modifierTarget, this);

			if (_onCastCounter <= MaxRecursionEventCount)
			{
				ResetEventCounters();
				(target as IEventOwner)?.ResetEventCounters();
			}
		}

		public void AddDamage(float damage)
		{
			_addDamageCounter++;

			Damage += damage;
			if (_addDamageCounter <= MaxRecursionEventCount)
			{
				for (int i = 0; i < _damageChangedEvents.Count; i++)
					_damageChangedEvents[i](this, Damage, damage);
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

		public void Move(float x, float y) => Move(new Vector2(x, y));
		public void Move(Vector2 value) => Position += value;

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