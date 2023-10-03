using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;

namespace ModiBuff.Examples.BasicConsole
{
	public delegate void DeathEvent(IUnit target, IUnit source);

	/// <summary>
	///		Our own unit class, which implements the IUnit interface,
	///		we inherit from ModiBuff.Units interfaces, to use one effect from there
	/// </summary>
	public sealed class Unit : IModifierOwner, IUpdatable, IDamagable, IAttacker, IHealable,
		ISingleStatusEffectOwner
	{
		//Every unit that can have modifiers needs to inherit IModifierOwner
		//By inheriting it we need to implement the ModifierController property
		//The modifier controller is the only class for units that manages modifiers
		//We simply add modifier ids to it, and it will handle the rest
		public ModifierController ModifierController { get; }

		//Basic implementation of status effects, unit can't attack when it's disarmed
		//Move when it's rooted/frozen/stunned, etc.
		public ISingleInstanceStatusEffectController<LegalAction, StatusEffectType> StatusEffectController { get; }

		//The rest are all game logic fields/properties
		public string Name { get; }

		public bool IsDead { get; private set; }
		public event DeathEvent DeathEvent;

		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }

		private readonly TargetingSystem _targetingSystem;

		public Unit(string name, float health, float damage)
		{
			Name = name;
			Health = MaxHealth = health;
			Damage = damage;

			//Remember to create the modifier controller in the constructor
			//and feed it the owner (this)
			ModifierController = new ModifierController(this);
			StatusEffectController = new StatusEffectController();
			_targetingSystem = new TargetingSystem();
		}

		public void Update(float deltaTime)
		{
			if (IsDead)
				return;

			//We need to update the modifier controller each frame/tick
			//To update the modifier timers (interval, duration)
			ModifierController.Update(deltaTime);
			StatusEffectController.Update(deltaTime);
		}

		public void SetAttackTarget(IUnit target)
		{
			((Unit)target).DeathEvent += ResetTarget;
			_targetingSystem.SetAttackTarget(target);

			void ResetTarget(IUnit unit, IUnit source)
			{
				_targetingSystem.SetAttackTarget(null);
				((Unit)target).DeathEvent -= ResetTarget;
			}
		}

		public float AutoAttack()
		{
			if (_targetingSystem.AttackTarget == null)
				return 0;

			return Attack(_targetingSystem.AttackTarget);
		}

		public float Attack(IUnit target) => Attack((Unit)target);

		/// <summary>
		///		Attempts to attack the target, returns the damage dealt
		///		Will not work if the unit is stunned or disarmed
		/// </summary>
		public float Attack(Unit target)
		{
			if (!StatusEffectController.HasLegalAction(LegalAction.Act))
				return 0;

			//This method will try to apply all our applier attack modifiers to the target
			this.ApplyAllAttackModifier(target);

			float damageDealt = target.TakeDamage(Damage, this);

			return damageDealt;
		}

		public float TakeDamage(float damage, IUnit source)
		{
			if (IsDead)
				return 0;

			float originalHealth = Health;

			Health -= damage;
			Console.GameMessage($"{this} took {damage} damage from {source}. Health: {Health}/{MaxHealth}");

			if (Health <= 0)
			{
				Health = 0;
				IsDead = true;
				DeathEvent?.Invoke(this, source);
				Console.GameMessage($"{this} died");
			}

			return originalHealth - Health;
		}

		public float Heal(float heal, IUnit source)
		{
			if (IsDead)
				return 0;

			float originalHealth = Health;
			Health += heal;

			if (Health > MaxHealth)
				Health = MaxHealth;

			Console.GameMessage($"{this} healed {heal} from {source}. Health: {Health}/{MaxHealth}");

			return Health - originalHealth;
		}

		public string GetDebugString()
		{
			return $"Unit, health: {Health}/{MaxHealth}, damage: {Damage}";
		}

		public override string ToString() => Name;
	}
}