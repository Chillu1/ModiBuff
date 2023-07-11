using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	/// <summary>
	///		Lazy implementation using the default Unit example, allows for all library features to be used
	/// </summary>
	/// <remarks>  Note that <see cref="Core.Units.Unit"/> is not meant to be inherited from</remarks>
	public sealed class FullUnit : Core.Units.Unit
	{
		private readonly TargetingSystem _targetingSystem;

		private float _attackTimer;
		private float _attackCooldown = 1f;

		public FullUnit()
		{
			_targetingSystem = new TargetingSystem();
		}

		public new void Update(float delta)
		{
			_attackTimer += delta;
			if (_attackTimer >= _attackCooldown)
			{
				_attackTimer = 0;

				if (_targetingSystem.AttackTarget != null)
					Attack(_targetingSystem.AttackTarget);
			}
		}

		public void Cast(int spellId)
		{
			Cast(spellId, (Core.Units.Unit)_targetingSystem.CastTarget);
		}

		public void SetAttackTarget(IUnit target) => _targetingSystem.SetAttackTarget(target);
		public void SetCastTarget(IUnit target) => _targetingSystem.SetCastTarget(target);
	}
}