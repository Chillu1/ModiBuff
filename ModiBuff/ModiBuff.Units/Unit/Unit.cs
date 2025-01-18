using System;
using System.Collections;
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
		IManaOwner<float, float>, IHealthCost<float>, IAddDamage<float>, IPreAttacker, ICallbackCounter,
		IStatusEffectOwner<LegalAction, StatusEffectType>, IStatusResistance, IKillable,
		ICallbackUnitRegistrable<CallbackUnitType>, IPosition<Vector2>, IMovable<Vector2>, IUnitEntity,
		IStatusEffectModifierOwnerLegalTarget<LegalAction, StatusEffectType>, IPoisonable,
		ISingleInstanceStatusEffectOwner<LegalAction, StatusEffectType>, ICallbackRegistrable<CallbackType>,
		IAllNonGeneric, ICaster, IStateReset, IIdOwner, IDurationLessStatusEffectOwner<LegalAction, StatusEffectType>,
		IAuraOwner, IDebuffable
	{
		public int Id { get; }
		public UnitTag UnitTag { get; private set; }
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }
		public float Mana { get; private set; }
		public float MaxMana { get; private set; }
		public float StatusResistance { get; private set; } = 1f;
		public UnitType UnitType { get; private set; }
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

		IDurationLessStatusEffectController<LegalAction, StatusEffectType>
			IDurationLessStatusEffectOwner<LegalAction, StatusEffectType>.StatusEffectController =>
			_durationLessStatusEffectController;

		private readonly List<IUnit>[] _auraTargets;

		private readonly DebuffType[] _debuffs;

		private readonly MultiInstanceStatusEffectController _statusEffectController;
		private readonly StatusEffectController _singleInstanceStatusEffectController;
		private readonly DurationLessStatusEffectController _durationLessStatusEffectController;

		private static int _idCounter;

		public Unit(float health = 500, float damage = 10, float healValue = 5, float mana = 1000,
			UnitType unitType = UnitType.Good, UnitTag unitTag = UnitTag.Default)
		{
			Id = _idCounter++;
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

			_strongDispelCallbacks = new List<IEffect>();
			_strongHitCallbacks = new List<IEffect>();
			_strongHitUnitCallbacks = new List<UnitCallback>();

			_poisonEvents = new List<PoisonEvent>();

			_dispelEvents = new List<DispelEvent>();
			_strongDispelEvents = new List<StrongDispelEvent>();
			_healthChangedEvents = new List<HealthChangedEvent>();
			_damageChangedEvents = new List<DamageChangedEvent>();
			_statusEffectAddedEvents = new List<AddStatusEffectEvent>();
			_statusEffectRemovedEvents = new List<RemoveStatusEffectEvent>();
			_onCastEvents = new List<CastEvent>();

			_updateTimerCallbacks = new List<UpdateTimerEvent>();

			_auraTargets = new List<IUnit>[2];
			for (int i = 0; i < _auraTargets.Length; i++)
				_auraTargets[i] = new List<IUnit> { this };

			_debuffs = new DebuffType[Enum.GetValues(typeof(DebuffType)).Length];

			ModifierController = ModifierControllerPool.Instance.Rent();
			ModifierApplierController = ModifierControllerPool.Instance.RentApplier();
			_statusEffectController = new MultiInstanceStatusEffectController
				(this, StatusEffectType.None, _statusEffectAddedEvents, _statusEffectRemovedEvents);
			_singleInstanceStatusEffectController = new StatusEffectController();
			_durationLessStatusEffectController = new DurationLessStatusEffectController();
		}

		public Unit(float health, float damage, ModifierAddReference[] modifierAddReferences,
			UnitType unitType, UnitTag unitTag)
			: this(health, damage, unitType: unitType, unitTag: unitTag)
		{
			foreach (var modifierAddReference in modifierAddReferences)
				this.TryAddModifierReference(modifierAddReference);
		}

		public static Unit LoadUnit(int oldId)
		{
			var unit = new Unit(0, 0, 0, 0, UnitType.Neutral, UnitTag.None);
			UnitHelper.LoadUnit(unit, oldId, unit.Id);
			return unit;
		}

		public void Update(float deltaTime)
		{
			_statusEffectController.Update(deltaTime);
			_singleInstanceStatusEffectController.Update(deltaTime);
			ModifierController.Update(deltaTime);
			ModifierApplierController.Update(deltaTime);

			_callbackTimer += deltaTime;
			if (_callbackTimer >= CallbackTimerCooldown)
			{
				_callbackTimer = 0;

				for (int i = 0; i < _updateTimerCallbacks.Count; i++)
					_updateTimerCallbacks[i]();
			}
		}

		/// <summary>
		///		Should be called before we attack/on attack. For modifiers like split shot that we want to trigger when starting an attack.
		/// </summary>
		public void PreAttack(IUnit target)
		{
			if (target is IUnitEntity entity && !UnitType.IsLegalTarget(entity.UnitType))
				return;
			if (!_statusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_singleInstanceStatusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_durationLessStatusEffectController.HasLegalAction(LegalAction.Act))
				return;

			if (++_preAttackCounter <= MaxEventCount)
			{
				for (int i = 0; i < _beforeAttackEffects.Count; i++)
					_beforeAttackEffects[i].Effect(target, this);
			}

			if (_preAttackCounter <= MaxEventCount)
			{
				ResetEventCounters();
				(target as ICallbackCounter)?.ResetEventCounters();
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
			    !_singleInstanceStatusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_durationLessStatusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			var killableTarget = target as IKillable;
			bool wasDead = killableTarget != null && killableTarget.IsDead;

			if (target is IModifierOwner modifierOwner)
				this.ApplyAllAttackModifier(modifierOwner);

			if (++_onAttackCounter <= MaxEventCount)
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
					if (++_onKillCounter <= MaxEventCount)
						for (int i = 0; i < _onKillEffects.Count; i++)
							_onKillEffects[i].Effect(target, this);
				}
			}

			if (_onAttackCounter <= MaxEventCount &&
			    _onKillCounter <= MaxEventCount)
			{
				ResetEventCounters();
				(target as ICallbackCounter)?.ResetEventCounters();
			}

			return dealtDamage;
		}

		public float TakeDamage(float damage, IUnit source)
		{
			if (++_whenAttackedCounter <= MaxEventCount)
			{
				for (int i = 0; i < _whenAttackedEffects.Count; i++)
					_whenAttackedEffects[i].Effect(this, source);
			}

			float oldHealth = Health;
			float newHealth = oldHealth - damage;
			float dealtDamage = oldHealth - newHealth;
			Health = newHealth;

			if (++_afterAttackedCounter <= MaxEventCount)
			{
				for (int i = 0; i < _afterAttackedEffects.Count; i++)
					_afterAttackedEffects[i].Effect(this, source);
			}

			if (dealtDamage > 0)
			{
				if (++_healthChangedCounter <= MaxEventCount)
				{
					for (int i = 0; i < _healthChangedEvents.Count; i++)
						_healthChangedEvents[i](this, source, Health, dealtDamage);
				}
			}

			//if damage was bigger than half health, trigger strong attack callbacks
			if (dealtDamage > MaxHealth * 0.5f && ++_strongHitCounter <= MaxEventCount)
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

				ResetState();

				IsDead = true;
			}

			if (_whenAttackedCounter <= MaxEventCount &&
			    _afterAttackedCounter <= MaxEventCount &&
			    _healthChangedCounter <= MaxEventCount &&
			    _strongHitCounter <= MaxEventCount)
			{
				ResetEventCounters();
				(source as ICallbackCounter)?.ResetEventCounters();
			}

			return dealtDamage;
		}

		public float Heal(float heal, IUnit source)
		{
			float oldHealth = Health;
			if (++_healCounter <= MaxEventCount)
			{
				for (int i = 0; i < _whenHealedEffects.Count; i++)
					_whenHealedEffects[i].Effect(this, source);
			}

			if (_healCounter <= MaxEventCount)
			{
				ResetEventCounters();
				(source as ICallbackCounter)?.ResetEventCounters();
			}

			Health += heal;
			if (Health > MaxHealth)
				Health = MaxHealth;
			return Health - oldHealth;
		}

		public float Heal(IHealable<float, float> target)
		{
			if (!_statusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_singleInstanceStatusEffectController.HasLegalAction(LegalAction.Act) ||
			    !_durationLessStatusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			if (++_healTargetCounter <= MaxEventCount)
			{
				for (int i = 0; i < _onHealEffects.Count; i++)
					_onHealEffects[i].Effect(target, this);
			}

			float valueHealed = target.Heal(HealValue, this);

			if (_healTargetCounter <= MaxEventCount)
			{
				ResetEventCounters();
				(target as ICallbackCounter)?.ResetEventCounters();
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

			if (++_onCastCounter <= MaxEventCount)
			{
				for (int i = 0; i < _onCastEffects.Count; i++)
					_onCastEffects[i].Effect(target, this);
				for (int i = 0; i < _onCastEvents.Count; i++)
					_onCastEvents[i](target, this, modifierId);
			}

			modifierTarget.ModifierController.Add(modifierId, modifierTarget, this);

			if (_onCastCounter <= MaxEventCount)
			{
				ResetEventCounters();
				(target as ICallbackCounter)?.ResetEventCounters();
			}
		}

		public void AddDamage(float damage)
		{
			Damage += damage;
			if (++_addDamageCounter <= MaxEventCount)
			{
				for (int i = 0; i < _damageChangedEvents.Count; i++)
					_damageChangedEvents[i](this, Damage, damage);
			}

			if (_addDamageCounter <= MaxEventCount)
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

			if (++_poisonDamageCounter <= MaxEventCount)
			{
				for (int i = 0; i < _poisonEvents.Count; i++)
					_poisonEvents[i](this, source, stacks, totalStacks, dealtDamage);
			}

			if (_poisonDamageCounter <= MaxEventCount)
			{
				ResetEventCounters();
				(source as ICallbackCounter)?.ResetEventCounters();
			}

			return dealtDamage + oldHealth - Health;
		}

		//---StatusResistances---

		public void ChangeStatusResistance(float value)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (value <= 0)
			{
				Logger.LogError("[ModiBuff.Units] StatusResistance can't be negative or zero.");
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
		///		If we'd want to an easier way to setup callbacks, without the need for a custom signature.
		///		We could split our "dispels" into multiple events, like here.
		/// </summary>
		public void StrongDispel(IUnit source)
		{
			if (++_strongDispelCounter <= MaxEventCount)
			{
				for (int i = 0; i < _strongDispelCallbacks.Count; i++)
					_strongDispelCallbacks[i].Effect(this, source);
				for (int i = 0; i < _strongDispelEvents.Count; i++)
					_strongDispelEvents[i](this, source);
			}

			if (_strongDispelCounter <= MaxEventCount)
				ResetEventCounters();
		}

		//---Aura---

		public void AddAuraTargets(int id, params Unit[] targets) => _auraTargets[id].AddRange(targets);
		public IList<IUnit> GetAuraTargets(int auraId) => _auraTargets[auraId];

		public void AddDebuff(DebuffType debuffType, IUnit source)
		{
			_debuffs[(int)debuffType]++;
			//TODO Event
		}

		public void RemoveDebuff(DebuffType debuffType, int stacksApplied, IUnit source)
		{
			_debuffs[(int)debuffType] -= stacksApplied;
			//TODO Event
		}

		public bool ContainsDebuff(DebuffType debuffType) => _debuffs[(int)debuffType] > 0;

		public void ResetState()
		{
			ResetEventCounters();
			ClearEvents(_whenAttackedEffects, _afterAttackedEffects, _whenDeathEffects, _whenHealedEffects,
				_beforeAttackEffects, _onAttackEffects, _onCastEffects, _onKillEffects, _onHealEffects,
				_strongDispelCallbacks, _strongHitCallbacks, _strongHitUnitCallbacks, _poisonEvents,
				_dispelEvents, _strongDispelEvents, _healthChangedEvents, _damageChangedEvents,
				_statusEffectAddedEvents, _statusEffectRemovedEvents, _onCastEvents, _updateTimerCallbacks);

			for (int i = 0; i < _auraTargets.Length; i++)
				_auraTargets[i].Clear();
			_callbackTimer = 0;
			_statusEffectController.ResetState();
			_singleInstanceStatusEffectController.ResetState();
			_durationLessStatusEffectController.ResetState();
			ModifierControllerPool.Instance.Return(ModifierController);
			ModifierControllerPool.Instance.ReturnApplier(ModifierApplierController);

			void ClearEvents(params IList[] lists)
			{
				for (int i = 0; i < lists.Length; i++)
					lists[i].Clear();
			}
		}

		public SaveData SaveState()
		{
			return new SaveData(Id, UnitTag, Health, MaxHealth, Damage, HealValue, Mana, MaxMana, StatusResistance,
				UnitType, IsDead, ModifierController.SaveState(), ModifierApplierController.SaveState(),
				_statusEffectController.SaveState(), _singleInstanceStatusEffectController.SaveState());
		}

		public void LoadState(SaveData data)
		{
			UnitTag = data.UnitTag;
			Health = data.Health;
			MaxHealth = data.MaxHealth;
			Damage = data.Damage;
			HealValue = data.HealValue;
			Mana = data.Mana;
			MaxMana = data.MaxMana;
			StatusResistance = data.StatusResistance;
			UnitType = data.UnitType;
			IsDead = data.IsDead;
			ModifierController.LoadState(data.ModifierControllerSaveData, this);
			ModifierApplierController.LoadState(data.ModifierApplierControllerSaveData);
			_statusEffectController.LoadState(data.MultiInstanceStatusEffectControllerSaveData);
			_singleInstanceStatusEffectController.LoadState(data.SingleInstanceStatusEffectControllerSaveData);
		}

		public override string ToString()
		{
			return $"Health: {Health}, Damage: {Damage}, HealValue: {HealValue}";
		}

		public readonly struct SaveData
		{
			public readonly int Id;
			public readonly UnitTag UnitTag;
			public readonly float Health;
			public readonly float MaxHealth;
			public readonly float Damage;
			public readonly float HealValue;
			public readonly float Mana;
			public readonly float MaxMana;
			public readonly float StatusResistance;
			public readonly UnitType UnitType;
			public readonly bool IsDead;

			public readonly ModifierController.SaveData ModifierControllerSaveData;

			public readonly ModifierApplierController.SaveData ModifierApplierControllerSaveData;
			public readonly MultiInstanceStatusEffectController.SaveData MultiInstanceStatusEffectControllerSaveData;
			public readonly StatusEffectController.SaveData SingleInstanceStatusEffectControllerSaveData;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(int id, UnitTag unitTag, float health, float maxHealth, float damage, float healValue,
				float mana, float maxMana, float statusResistance, UnitType unitType, bool isDead,
				ModifierController.SaveData modifierControllerSaveData,
				ModifierApplierController.SaveData modifierApplierControllerSaveData,
				MultiInstanceStatusEffectController.SaveData multiInstanceStatusEffectControllerSaveData,
				StatusEffectController.SaveData singleInstanceStatusEffectControllerSaveData)
			{
				Id = id;
				UnitTag = unitTag;
				Health = health;
				MaxHealth = maxHealth;
				Damage = damage;
				HealValue = healValue;
				Mana = mana;
				MaxMana = maxMana;
				StatusResistance = statusResistance;
				UnitType = unitType;
				IsDead = isDead;
				ModifierControllerSaveData = modifierControllerSaveData;
				ModifierApplierControllerSaveData = modifierApplierControllerSaveData;
				MultiInstanceStatusEffectControllerSaveData = multiInstanceStatusEffectControllerSaveData;
				SingleInstanceStatusEffectControllerSaveData = singleInstanceStatusEffectControllerSaveData;
			}
		}
	}
}