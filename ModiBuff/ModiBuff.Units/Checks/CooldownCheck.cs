namespace ModiBuff.Core.Units
{
	public sealed class CooldownCheck : IUpdatableCheck, IStateCheck<CooldownCheck>
	{
		private readonly float _cooldown;
		private float _timer;

		public CooldownCheck(float cooldown)
		{
			_cooldown = cooldown;

			//Start with cooldown ready
			_timer = cooldown;
		}

		public void Update(float deltaTime)
		{
			if (_timer > _cooldown)
				return;

			_timer += deltaTime;
		}

		public bool Check() => _timer >= _cooldown;

		public void ResetState() => _timer = 0;

		public CooldownCheck ShallowClone() => new CooldownCheck(_cooldown);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}