using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	public sealed class Unit : Core.Units.Unit //TODO Temp, laziness
	{
		private readonly TargetSystem _targetSystem;

		private float _attackTimer;
		private float _attackCooldown = 1f;

		public Unit()
		{
			_targetSystem = new TargetSystem();
		}

		public override void Update(float delta)
		{
			_attackTimer += delta;
			if (_attackTimer >= _attackCooldown)
			{
				_attackTimer = 0;

				if (_targetSystem.AttackTarget != null)
					Attack(_targetSystem.AttackTarget);
			}
		}

		public void Cast(string spellName)
		{
			Cast((Core.Units.Unit)_targetSystem.CastTarget);
		}

		public void SetAttackTarget(IUnit target) => _targetSystem.SetAttackTarget(target);
		public void SetCastTarget(IUnit target) => _targetSystem.SetCastTarget(target);
	}
}