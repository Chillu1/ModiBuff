namespace ModifierLibraryLite.Core
{
	public sealed class CooldownCheck
	{
		private readonly float _cooldown;
		private float _timer;

		public bool IsReady => _timer >= _cooldown;

		public CooldownCheck(float cooldown)
		{
			_cooldown = cooldown;
		}

		public void Update(in float deltaTime)
		{
			if (_timer > _cooldown)
				return;

			_timer += deltaTime;
		}

		public void ResetCooldown() => _timer = 0;
	}
}