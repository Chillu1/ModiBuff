using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("ModifierLibraryLite.Tests")]

namespace ModifierLibraryLite.Core.Units
{
	public class Unit : IUnit
	{
		private readonly ModifierController _modifierController;

		public float Health { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }
		public float Mana { get; private set; }

		private List<UnitEvent> _onDamageEvents;

		private readonly StatusEffectController _statusEffectController;

		public Unit(float health = 500, float damage = 10, float healValue = 5, float mana = 1000)
		{
			Health = health;
			Damage = damage;
			HealValue = healValue;
			Mana = mana;
			_onDamageEvents = new List<UnitEvent>();

			_modifierController = new ModifierController();
			_statusEffectController = new StatusEffectController();
		}

		public void Update(in float deltaTime)
		{
			_statusEffectController.Update(deltaTime);
			_modifierController.Update(deltaTime);
		}

		public void Cast(Unit target)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Cast) == 0)
				return;

			target.TryApplyModifiers(_modifierController.GetApplierCastModifiers(), this);
		}

		public float Attack(IUnit target) => Attack((Unit)target);

		public float Attack(Unit target)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Act) == 0)
				return 0;

			target.TryApplyModifiers(_modifierController.GetApplierCheckModifiers(), this);
			target.TryApplyModifiers(_modifierController.GetApplierAttackModifiers(), this);
			float dealtDamage = target.TakeDamage(Damage, this);
			return dealtDamage;
		}

		public float TakeDamage(float damage, IUnit acter, bool triggersEvents = true)
		{
			float oldHealth = Health;
			Health -= damage;
			float dealtDamage = oldHealth - Health;

			if (triggersEvents)
				for (int i = 0; i < _onDamageEvents.Count; i++)
					_onDamageEvents[i](this, acter);

			return dealtDamage;
		}

		public float Heal(float heal, IUnit acter)
		{
			float oldHealth = Health;
			Health += heal;
			return Health - oldHealth;
		}

		public float Heal(IUnit target)
		{
			if ((_statusEffectController.LegalActions & LegalAction.Act) == 0)
				return 0;

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

		public bool HasStatusEffect(StatusEffectType statusEffect)
		{
			return _statusEffectController.HasStatusEffect(statusEffect);
		}

		public void ChangeStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			_statusEffectController.ChangeStatusEffect(statusEffectType, duration);
		}

		//---Modifier based---

		public void AddEffectEvent(IEffect effect, EffectOnEvent @event, TargetType targetType)
		{
			//Maybe don't have full modifier's for events, but instead only effects? This would take away stack, functionality, etc.
			//Also would make the effects unrevertable.
			//We could store the modifier directly, and call init/refresh/stack on it?
			//TODO GetModifierId

			switch (@event)
			{
				case EffectOnEvent.OnHit:
					_onDamageEvents.Add((self, unit) => effect.Effect(unit, self));
					break;
				default:
					Debug.LogError("Unknown event type: " + @event);
					return;
			}
		}

		public void RemoveEffectEvent(IEffect effect, EffectOnEvent @event, TargetType targetType)
		{
			switch (@event)
			{
				case EffectOnEvent.OnHit:
					_onDamageEvents.Remove((self, unit) => effect.Effect(unit, self));
					break;
				default:
					Debug.LogError("Unknown event type: " + @event);
					return;
			}
		}

		public bool AddApplierModifier(ModifierRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			return _modifierController.TryAddApplier(recipe, applierType);
		}

		public bool TryAddModifier(int id, IUnit target, IUnit acter)
		{
			return _modifierController.TryAdd(id, acter, target, acter);
		}

		public bool TryAddModifier(ModifierRecipe recipe, IUnit target)
		{
			return _modifierController.TryAdd(recipe.Id, this, target, this);
		}

		private void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit acter)
		{
			foreach (var check in modifierChecks)
			{
				if (!check.Check(acter))
					continue;

				TryAddModifier(check.Id, this, acter);
			}
		}

		private void TryApplyModifiers(IReadOnlyList<int> recipes, IUnit acter)
		{
			for (int i = 0; i < recipes.Count; i++)
				TryAddModifier(recipes[i], this, acter);
		}

		public bool ContainsModifier(string id) => _modifierController.Contains(ModifierIdManager.GetId(id));

		public void PrepareRemoveModifier(int id)
		{
			_modifierController.PrepareRemove(id);
		}

		public void RemoveModifier(int id) => _modifierController.Remove(id);

		public override string ToString()
		{
			return $"Health: {Health}, Damage: {Damage}, HealValue: {HealValue}";
		}
	}
}