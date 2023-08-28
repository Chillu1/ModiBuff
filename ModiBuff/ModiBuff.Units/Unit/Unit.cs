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
	public class Unit : IUpdatable, IModifierOwner, IAttacker<float, float>, IDamagable<float, float, float, float>,
		IHealable<float, float>, IHealer<float, float>, IManaOwner<float, float>, IHealthCost<float>, IAddDamage<float>,
		IEventOwner<EffectOnEvent>, IStatusEffectOwner<LegalAction, StatusEffectType>, IStatusResistance,
		IStatusEffectModifierOwner<LegalAction, StatusEffectType>
	{
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }
		public float Mana { get; private set; }
		public float MaxMana { get; private set; }
		public float StatusResistance { get; private set; } = 1f;

		public bool IsDead { get; private set; }

		public ModifierController ModifierController { get; }
		public IStatusEffectController<LegalAction, StatusEffectType> StatusEffectController => _statusEffectController;

		//Note: These event lists should only be used for classic effects.
		//If you try to tie core game logic to them, you will most likely have trouble with sequence of events.
		private List<IEffect> _whenAttackedEffects, _whenCastEffects, _whenDeathEffects, _whenHealedEffects;
		private List<IEffect> _onAttackEffects, _onCastEffects, _onKillEffects, _onHealEffects;

		private List<IUnit> _targetsInRange;
		private List<Modifier> _auraModifiers;

		private readonly StatusEffectController _statusEffectController;

		public Unit(float health = 500, float damage = 10, float healValue = 5, float mana = 1000)
		{
			Health = health;
			MaxHealth = health;
			Damage = damage;
			HealValue = healValue;
			Mana = mana;
			MaxMana = mana;

			_whenAttackedEffects = new List<IEffect>();
			_whenCastEffects = new List<IEffect>();
			_whenDeathEffects = new List<IEffect>();
			_whenHealedEffects = new List<IEffect>();
			_onAttackEffects = new List<IEffect>();
			_onCastEffects = new List<IEffect>();
			_onKillEffects = new List<IEffect>();
			_onHealEffects = new List<IEffect>();

			_targetsInRange = new List<IUnit>();
			_targetsInRange.Add(this);
			_auraModifiers = new List<Modifier>();

			ModifierController = new ModifierController(this);
			_statusEffectController = new StatusEffectController();
		}

		public Unit(float health, float damage, ModifierAddReference[] modifierAddReferences) : this(health, damage)
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

		public float Attack(IUnit target, bool triggersEvents = true)
		{
			return Attack((Unit)target, triggersEvents);
		}

		public float Attack(Unit target, bool triggersEvents = true)
		{
			if (!_statusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			this.ApplyAllAttackModifier(target);

			for (int i = 0; i < _onAttackEffects.Count; i++)
				_onAttackEffects[i].Effect(target, this);

			float dealtDamage = target.TakeDamage(Damage, this, triggersEvents);

			if (target.Health <= 0)
				for (int i = 0; i < _onKillEffects.Count; i++)
					_onKillEffects[i].Effect(target, this);

			return dealtDamage;
		}

		public float TakeDamage(float damage, IUnit source, bool triggersEvents = true)
		{
			if (triggersEvents)
				for (int i = 0; i < _whenAttackedEffects.Count; i++)
					_whenAttackedEffects[i].Effect(this, source);

			float oldHealth = Health;
			Health -= damage;
			float dealtDamage = oldHealth - Health;

			if (triggersEvents && Health <= 0 && !IsDead)
			{
				for (int i = 0; i < _whenDeathEffects.Count; i++)
					_whenDeathEffects[i].Effect(this, source);
				//Unit Death TODO Destroy/pool unit
				ModifierController.Clear();

				IsDead = true;
			}

			return dealtDamage;
		}

		public float Heal(float heal, IUnit source, bool triggersEvents = true)
		{
			float oldHealth = Health;
			if (triggersEvents)
				for (int i = 0; i < _whenHealedEffects.Count; i++)
					_whenHealedEffects[i].Effect(this, source);
			Health += heal;
			return Health - oldHealth;
		}

		public float Heal(IHealable<float, float> target, bool triggersEvents = true)
		{
			if (!_statusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			if (triggersEvents)
				for (int i = 0; i < _onHealEffects.Count; i++)
					_onHealEffects[i].Effect(target, this);

			return target.Heal(HealValue, this, triggersEvents);
		}

		public void AddDamage(float damage)
		{
			Damage += damage;
		}

		public void UseHealth(float value)
		{
			Health -= value;
		}

		public void UseMana(float value)
		{
			Mana -= value;
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