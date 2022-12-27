using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("ModifierLibraryLite.Tests")]

namespace ModiBuff.Core.Units
{
	public class Unit : IUnit
	{
		private readonly ModifierController _modifierController;

		public float Health { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }
		public float Mana { get; private set; }

		//This can be done with an array of list, but it's better performance wise.
		private List<IEffect> _whenAttackedEffects;
		private List<IEffect> _whenCastEffects;
		private List<IEffect> _whenDeathEffects;
		private List<IEffect> _whenHealedEffects;

		private List<IEffect> _onAttackEffects;
		private List<IEffect> _onCastEffects;
		private List<IEffect> _onKillEffects;
		private List<IEffect> _onHealEffects;

		private List<Unit> _targetsInRange;
		private List<Modifier> _auraModifiers;

		private readonly StatusEffectController _statusEffectController;

		public Unit(float health = 500, float damage = 10, float healValue = 5, float mana = 1000)
		{
			Health = health;
			Damage = damage;
			HealValue = healValue;
			Mana = mana;

			_whenAttackedEffects = new List<IEffect>();
			_whenCastEffects = new List<IEffect>();
			_whenDeathEffects = new List<IEffect>();
			_whenHealedEffects = new List<IEffect>();
			_onAttackEffects = new List<IEffect>();
			_onCastEffects = new List<IEffect>();
			_onKillEffects = new List<IEffect>();
			_onHealEffects = new List<IEffect>();

			_targetsInRange = new List<Unit>();
			_targetsInRange.Add(this);
			_auraModifiers = new List<Modifier>();

			_modifierController = new ModifierController();
			_statusEffectController = new StatusEffectController();
		}

		public virtual void Update(float deltaTime) //TODO Remove virtual
		{
			_statusEffectController.Update(deltaTime);
			_modifierController.Update(deltaTime);
			for (int i = 0; i < _auraModifiers.Count; i++)
				_auraModifiers[i].Update(deltaTime);
		}

		public void Cast(Unit target)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Cast) == 0)
				return;

			target.TryApplyModifiers(_modifierController.GetApplierCheckModifiers(), this);
			target.TryApplyModifiers(_modifierController.GetApplierCastModifiers(), this);
		}

		public float Attack(IUnit target, bool triggersEvents = true) => Attack((Unit)target, triggersEvents);

		public float Attack(Unit target, bool triggersEvents = true)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Act) == 0)
				return 0;

			target.TryApplyModifiers(_modifierController.GetApplierCheckModifiers(), this);
			target.TryApplyModifiers(_modifierController.GetApplierAttackModifiers(), this);
			for (int i = 0; i < _onAttackEffects.Count; i++)
				_onAttackEffects[i].Effect(target, this);

			float dealtDamage = target.TakeDamage(Damage, this, triggersEvents);

			if (target.Health <= 0)
				for (int i = 0; i < _onKillEffects.Count; i++)
					_onKillEffects[i].Effect(target, this);

			return dealtDamage;
		}

		public float TakeDamage(float damage, IUnit acter, bool triggersEvents = true)
		{
			float oldHealth = Health;
			Health -= damage;
			float dealtDamage = oldHealth - Health;

			if (triggersEvents)
			{
				for (int i = 0; i < _whenAttackedEffects.Count; i++)
					_whenAttackedEffects[i].Effect(this, acter);

				if (Health <= 0)
					for (int i = 0; i < _whenDeathEffects.Count; i++)
						_whenDeathEffects[i].Effect(this, acter);
			}

			return dealtDamage;
		}

		public float Heal(float heal, IUnit acter)
		{
			float oldHealth = Health;
			for (int i = 0; i < _whenHealedEffects.Count; i++)
				_whenHealedEffects[i].Effect(this, acter);
			Health += heal;
			return Health - oldHealth;
		}

		public float Heal(IUnit target)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Act) == 0)
				return 0;

			for (int i = 0; i < _onHealEffects.Count; i++)
				_onHealEffects[i].Effect(target, this);

			return target.Heal(HealValue, this);
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
			return _modifierController.TryAdd(addReference, this, sender);
		}

		public bool AddApplierModifier(IModifierRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			return _modifierController.TryAddApplier(recipe, applierType);
		}

		public bool TryAddModifier(int id, IUnit acter)
		{
			return _modifierController.TryAdd(id, this, acter);
		}

		public bool TryAddModifierTarget(int id, IUnit target, IUnit acter)
		{
			return _modifierController.TryAdd(id, target, acter);
		}

		private void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit acter)
		{
			foreach (var check in modifierChecks)
			{
				if (!check.Check(acter))
					continue;

				TryAddModifier(check.Id, acter);
			}
		}

		private void TryApplyModifiers(IReadOnlyList<int> recipes, IUnit acter)
		{
			for (int i = 0; i < recipes.Count; i++)
				TryAddModifier(recipes[i], acter);
		}

		public bool ContainsModifier(string id) => _modifierController.Contains(ModifierIdManager.GetId(id));

		public void PrepareRemoveModifier(int id)
		{
			_modifierController.PrepareRemove(id);
		}

		/// <summary>
		///		Only to be used for testing, use <see cref="PrepareRemoveModifier(int)"/> instead.
		/// </summary>
		/// <param name="id"></param>
		public void RemoveModifier(int id) => _modifierController.Remove(id);

		//---Aura---

		public void AddCloseTargets(params Unit[] targets)
		{
			_targetsInRange.AddRange(targets);
		}

		public void AddAuraModifier(IModifierRecipe recipe)
		{
			var modifier = recipe.Create();
			//modifier.SetTargets();
			_auraModifiers.Add(modifier);
		}

		public override string ToString()
		{
			return $"Health: {Health}, Damage: {Damage}, HealValue: {HealValue}";
		}
	}
}