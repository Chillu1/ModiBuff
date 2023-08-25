using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Examples.BasicConsole
{
	public delegate void DeathEvent(IUnit target, IUnit source);

	public sealed class Unit : IUpdatable, IModifierOwner, IAttacker<float>, IDamagable<float>
	{
		public string Name { get; }

		public bool IsDead { get; private set; }
		public event DeathEvent DeathEvent;

		public float Health { get; private set; }
		public float MaxHealth { get; private set; }

		public float Damage { get; private set; }

		public ModifierController ModifierController { get; }

		private TargetingSystem _targetingSystem;

		public Unit(string name, float health, float damage, float attackCooldown = 1f)
		{
			Name = name;
			Health = MaxHealth = health;
			Damage = damage;

			ModifierController = new ModifierController(this);
			_targetingSystem = new TargetingSystem();
		}

		public void Update(float deltaTime)
		{
			if (IsDead)
				return;

			ModifierController.Update(deltaTime);
		}

		public bool TryAddModifier(int id, IUnit source) => ModifierController.TryAdd(id, this, source);

		public void SetAttackTarget(IUnit target)
		{
			((Unit)target).DeathEvent += delegate { _targetingSystem.SetAttackTarget(null); };
			_targetingSystem.SetAttackTarget(target);
		}

		public float Attack()
		{
			if (_targetingSystem.AttackTarget == null)
				return 0;

			return Attack(_targetingSystem.AttackTarget);
		}

		public float Attack(IUnit target, bool triggersEvents = true)
		{
			float damageDealt = ((IDamagable<float, float>)target).TakeDamage(Damage, this, triggersEvents);

			return damageDealt;
		}

		public float TakeDamage(float damage, IUnit source, bool triggersEvents = true)
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

		public string GetDebugString()
		{
			return $"Unit, health: {Health}/{MaxHealth}, damage: {Damage}";
		}

		public override string ToString() => Name;
	}
}