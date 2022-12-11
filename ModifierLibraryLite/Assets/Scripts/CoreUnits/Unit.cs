using System.Collections.Generic;
using UnityEngine;

namespace ModifierLibraryLite.Core.Units
{
	public class Unit : IUnit
	{
		private readonly ModifierController _modifierController;

		public float Health { get; private set; }
		public float Damage { get; private set; }
		public float HealValue { get; private set; }


		public Unit(float health = 500, float damage = 10, float healValue = 5)
		{
			Health = health;
			Damage = damage;
			HealValue = healValue;

			_modifierController = new ModifierController();
		}

		public void Update(in float deltaTime)
		{
			_modifierController.Update(deltaTime);
		}

		public float Attack(IUnit target)
		{
			target.TryApplyModifiers(_modifierController.GetApplierModifiers(), this);
			float dealtDamage = target.TakeDamage(Damage, this);
			return dealtDamage;
		}

		public float TakeDamage(float damage, IUnit acter)
		{
			float oldHealth = Health;
			Health -= damage;
			return oldHealth - Health;
		}

		public float Heal(float heal, IUnit acter)
		{
			float oldHealth = Health;
			Health += heal;
			return Health - oldHealth;
		}

		public float Heal(IUnit target)
		{
			return target.Heal(HealValue, this);
		}

		public void AddDamage(float damage)
		{
			Damage += damage;
		}

		//---Modifier based---

		public bool AddApplierModifiers(params ModifierRecipe[] recipes)
		{
			return _modifierController.TryAddAppliers(recipes);
		}

		public bool TryAddModifier(Modifier modifier, IUnit target, IUnit sender = null)
		{
			//TODO Do we want to save the sender of the original modifier? Ex. for thorns. Because owner is always the owner of the modifier instance
			modifier.SetTargets(target, this, sender);

			return _modifierController.TryAdd(modifier).Success;
		}

		public void TryApplyModifiers(IReadOnlyCollection<ModifierRecipe> getApplierModifiers, IUnit acter)
		{
			foreach (var modifierRecipe in getApplierModifiers)
				TryAddModifier(modifierRecipe.Create(), this, acter);
		}

		public bool ContainsModifier(Modifier modifier)
		{
			return _modifierController.Contains(modifier);
		}
	}
}