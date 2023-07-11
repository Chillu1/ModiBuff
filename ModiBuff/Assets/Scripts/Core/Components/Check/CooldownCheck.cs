namespace ModiBuff.Core
{
	public sealed class CooldownCheck
	{
		private readonly float _cooldown;
		private float _timer;

		public bool IsReady => _timer >= _cooldown;

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

		public void ResetCooldown() => _timer = 0;
	}
}