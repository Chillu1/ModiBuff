using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("ModiBuff.Tests")]

namespace ModiBuff.Core.Units
{
	public class Unit : IUnit, IUpdatable, IModifierOwner, IAttacker, IDamagable, IHealable, IHealer, IManaOwner, IHealthCost, IAddDamage,
		IEventOwner, IStatusEffectOwner
	{
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }
		public float Mana { get; private set; }
		public float MaxMana { get; private set; }

		public ModifierController ModifierController { get; }

		//This can be done with an array of list, but it's better performance wise.
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

			ModifierController = new ModifierController();
			_statusEffectController = new StatusEffectController();
		}

		public Unit(float health, float damage, ModifierAddReference[] modifierAddReferences) : this(health, damage)
		{
			foreach (var modifierAddReference in modifierAddReferences)
				TryAddModifier(modifierAddReference, this);
		}

		public void Update(float deltaTime)
		{
			_statusEffectController.Update(deltaTime);
			ModifierController.Update(deltaTime);
			for (int i = 0; i < _auraModifiers.Count; i++)
				_auraModifiers[i].Update(deltaTime);
		}

		public void Cast(int id, Unit target)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Cast) == 0)
				return;

			int applierId = ModifierController.GetApplierCastModifier(id);
			if (applierId != -1)
				target.TryAddModifier(applierId, this);
		}

		public void Cast(Unit target)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Cast) == 0)
				return;

			target.TryApplyModifiers(ModifierController.GetApplierCheckModifiers(), this);
			target.TryApplyModifiers(ModifierController.GetApplierCastModifiers(), this);
		}

		public float Attack(IUnit target, bool triggersEvents = true) => Attack((Unit)target, triggersEvents);

		public float Attack(Unit target, bool triggersEvents = true)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Act) == 0)
				return 0;

			target.TryApplyModifiers(ModifierController.GetApplierCheckModifiers(), this);
			target.TryApplyModifiers(ModifierController.GetApplierAttackModifiers(), this);
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
			{
				for (int i = 0; i < _whenAttackedEffects.Count; i++)
					_whenAttackedEffects[i].Effect(this, source);
			}

			float oldHealth = Health;
			Health -= damage;
			float dealtDamage = oldHealth - Health;

			if (triggersEvents)
			{
				if (Health <= 0)
					for (int i = 0; i < _whenDeathEffects.Count; i++)
						_whenDeathEffects[i].Effect(this, source);
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

		public float Heal(IHealable target, bool triggersEvents = true)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Act) == 0)
				return 0;

			if (triggersEvents)
				for (int i = 0; i < _onHealEffects.Count; i++)
					_onHealEffects[i].Effect((IUnit)target, this);

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

		//---StatusEffects---

		public bool HasLegalAction(LegalAction legalAction)
		{
			return _statusEffectController.HasLegalAction(legalAction);
		}

		public bool HasStatusEffect(StatusEffectType statusEffect)
		{
			return _statusEffectController.HasStatusEffect(statusEffect);
		}

		public void ChangeStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			_statusEffectController.ChangeStatusEffect(statusEffectType, duration);
		}

		public void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			_statusEffectController.DecreaseStatusEffect(statusEffectType, duration);
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
					Debug.LogError("Unknown event type: " + @event);
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
					Debug.LogError("Unknown event type: " + @event);
					return;
			}

			void Remove(List<IEffect> effects, IEffect effectToRemove)
			{
				bool remove = effects.Remove(effectToRemove);
				if (!remove)
					Debug.LogError("Could not remove event: " + effectToRemove.GetType());
			}
		}

		public bool TryAddModifier(ModifierAddReference addReference, IUnit sender)
		{
			return ModifierController.TryAdd(addReference, this, sender);
		}

		internal bool AddApplierModifier(IModifierRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			return ModifierController.TryAddApplier(recipe.Id, recipe.HasApplyChecks, applierType);
		}

		public bool TryAddModifier(int id, IUnit source)
		{
			return ModifierController.TryAdd(id, this, source);
		}

		private void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit source)
		{
			foreach (var check in modifierChecks)
			{
				if (!check.Check(source))
					continue;

				TryAddModifier(check.Id, source);
			}
		}

		private void TryApplyModifiers(IReadOnlyList<int> recipes, IUnit source)
		{
			for (int i = 0; i < recipes.Count; i++)
				TryAddModifier(recipes[i], source);
		}

		public bool ContainsModifier(string id) => ModifierController.Contains(ModifierIdManager.GetId(id));

		//---Aura---

		public void AddCloseTargets(params Unit[] targets)
		{
			_targetsInRange.AddRange(targets);
		}

		public void AddAuraModifier(string name)
		{
			//modifier.SetTargets();
			var modifier = ModifierPool.Instance.Rent(ModifierIdManager.GetId(name));
			//modifier.SetAuraTargets(_targetsInRange, this);
			_auraModifiers.Add(modifier);
		}

		public override string ToString()
		{
			return $"Health: {Health}, Damage: {Damage}, HealValue: {HealValue}";
		}
	}
}