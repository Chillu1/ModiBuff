using UnityEngine;

namespace ModifierLibraryLite.Core.Units
{
	public class Unit : IUnit
	{
		private readonly IModifierController _modifierController;

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

		//---Modifier based---

		public bool TryAddModifier(Modifier modifier, IUnit target)
		{
			modifier.TargetComponent.SetOwner(this);
			modifier.TargetComponent.SetTarget(target);
			var result = _modifierController.TryAdd(modifier);

			return result.Success;
		}
	}
}