using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Examples.SimpleSolo
{
	/// <summary>
	///		Custom basic unit implementation
	/// </summary>
	public sealed class Unit : IUnit, IUpdatable, IModifierOwner, IAttacker<float>, IDamagable<float>, IBlockOwner
	{
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }

		public ModifierController ModifierController { get; }

		public int BlockInstance { get; private set; }

		private readonly TargetingSystem _targetingSystem;

		private float _attackTimer;
		private float _attackCooldown = 1f;

		public Unit(float health, float damage)
		{
			_targetingSystem = new TargetingSystem();

			ModifierController = new ModifierController(this);

			Health = MaxHealth = health;
			Damage = damage;
		}

		public void Update(float deltaTime)
		{
			_attackTimer += deltaTime;
			if (_attackTimer >= _attackCooldown)
			{
				_attackTimer = 0;

				if (_targetingSystem.AttackTarget != null)
					Attack(_targetingSystem.AttackTarget);
			}

			ModifierController.Update(deltaTime);
		}

		public void Cast(int id, IModifierOwner target)
		{
			this.TryCast(id, target);
		}

		public void SetAttackTarget(IUnit target) => _targetingSystem.SetAttackTarget(target);
		public void SetCastTarget(IUnit target) => _targetingSystem.SetCastTarget(target);

		public float TakeDamage(float damage, IUnit source, bool triggersEvents = true)
		{
			if (BlockInstance > 0) //Example custom game logic implementation
			{
				RemoveBlock(1);
				return 0;
			}

			return Health -= damage;
		}

		public float Attack(IUnit target, bool triggersEvents = true)
		{
			float dealtDamage = ((IDamagable<float, float>)target).TakeDamage(Damage, this, triggersEvents);

			return dealtDamage;
		}

		public void AddBlock(int amount) => BlockInstance += amount;
		public void RemoveBlock(int amount) => BlockInstance -= amount;
	}
}