using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	/// <summary>
	///		Custom basic unit implementation
	/// </summary>
	public sealed class Unit : IUnit, IUpdatable, IModifierOwner, IAttacker, IDamagable
	{
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public float Damage { get; private set; }

		public ModifierController ModifierController { get; }

		private readonly TargetingSystem _targetingSystem;

		private float _attackTimer;
		private float _attackCooldown = 1f;

		public Unit(float health, float damage)
		{
			ModifierController = new ModifierController();

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

		public void Cast(int spellId, IUnit target)
		{
			int applierId = ModifierController.GetApplierCastModifier(spellId);
			if (applierId != -1)
				((IModifierOwner)target).TryAddModifier(applierId, this);
		}

		public void SetAttackTarget(IUnit target) => _targetingSystem.SetAttackTarget(target);
		public void SetCastTarget(IUnit target) => _targetingSystem.SetCastTarget(target);

		public float TakeDamage(float damage, IUnit source, bool triggersEvents = true)
		{
			return Health -= damage;
		}

		public bool TryAddModifier(int id, IUnit source)
		{
			return ModifierController.TryAdd(id, this, source);
		}

		public float Attack(IUnit target, bool triggersEvents = true)
		{
			float dealtDamage = ((IDamagable)target).TakeDamage(Damage, this, triggersEvents);

			return dealtDamage;
		}
	}
}