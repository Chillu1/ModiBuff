namespace ModiBuff.Core.Units
{
	//The only check where data state is mutable
	public sealed class CooldownCheck : IUpdatableCheck, INoUnitCheck,
		IStateCheck<CooldownCheck>, IDataCheck<CooldownCheck.Data>
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
			if (_timer >= _cooldown)
				return;

			_timer += deltaTime;
		}

		public bool Check() => _timer >= _cooldown;

		public Data GetData() => new Data(_cooldown, _timer);

		/// <summary>
		///		Resets the timer to 0, so the check is not ready.
		/// </summary>
		public void RestartState() => _timer = 0;

		/// <summary>
		///		Sets the timer to the cooldown, so the check is ready.
		/// </summary>
		public void ResetState() => _timer = _cooldown;

		public CooldownCheck ShallowClone() => new CooldownCheck(_cooldown);
		object IShallowClone.ShallowClone() => ShallowClone();

		public struct Data
		{
			public readonly float Cooldown;
			public readonly float Timer;

			public Data(float cooldown, float timer)
			{
				Cooldown = cooldown;
				Timer = timer;
			}
		}
	}
}