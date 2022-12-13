using System.Collections.Generic;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ModifierLibraryLite.Tests")]

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

		public float Attack(Unit target)
		{
			target.TryApplyModifiers(_modifierController.GetApplierCheckModifiers(), this);
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

		internal bool TryAddModifierSelf(string id) //No sender, TEMP?
		{
			return TryAddModifier(id, this);
		}

		public bool TryAddModifier(string id, IUnit target, IUnit sender = null)
		{
			return _modifierController.TryAdd(id, this, target, sender).Success;
		}

		public bool TryAddModifier(ModifierRecipe recipe, IUnit target, IUnit sender = null)
		{
			return _modifierController.TryAdd(recipe.Id, this, target, sender).Success;
		}

		private void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit acter)
		{
			foreach (var check in modifierChecks)
			{
				if (!check.Check())
					continue;

				TryAddModifier(check.Id, this, acter);
			}
		}

		private void TryApplyModifiers(IReadOnlyList<ModifierRecipe> recipes, IUnit acter)
		{
			for (int i = 0; i < recipes.Count; i++)
				TryAddModifier(recipes[i].Id, this, acter);
		}

		public bool ContainsModifier(ModifierRecipe recipe) => _modifierController.Contains(recipe);

		public bool ContainsModifier(string id) => _modifierController.Contains(id);

		public override string ToString()
		{
			return $"Health: {Health}, Damage: {Damage}, HealValue: {HealValue}";
		}
	}
}