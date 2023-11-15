namespace ModiBuff.Core.Units
{
	//The only check where data state is mutable
	public sealed class CooldownCheck : IUpdatableCheck, INoUnitCheck, IDataCheck<CooldownCheck.Data>,
		IStateCheck<CooldownCheck, CooldownCheck.SaveData>
	{
		private readonly float _cooldown;

		private float _timer;
		private float _multiplier;

		public CooldownCheck(float cooldown)
		{
			_cooldown = cooldown;
			_multiplier = 1;
		}

		public void Update(float deltaTime)
		{
			if (_timer <= 0)
				return;

			_timer -= deltaTime * _multiplier;
		}

		public void SetMultiplier(float multiplier) => _multiplier = multiplier;

		public bool Check() => _timer <= 0;

		public Data GetData() => new Data(_cooldown, _timer);

		/// <summary>
		///		Resets the timer to cooldown, so the check is not ready.
		/// </summary>
		public void RestartState() => _timer = _cooldown;

		/// <summary>
		///		Sets the timer to 0, so the check is ready.
		/// </summary>
		public void ResetState()
		{
			_timer = 0;
			_multiplier = 1;
		}

		public CooldownCheck ShallowClone() => new CooldownCheck(_cooldown);
		object IShallowClone.ShallowClone() => ShallowClone();

		public SaveData SaveState() => new SaveData(_timer, _multiplier);
		object ISavable.SaveState() => SaveState();

		public void LoadState(SaveData data)
		{
			_timer = data.Timer;
			_multiplier = data.Multiplier;
		}

		void ISavable.LoadState(object data) => LoadState((SaveData)data);

		public readonly struct Data
		{
			public readonly float Cooldown;
			public readonly float Timer;

			public Data(float cooldown, float timer)
			{
				Cooldown = cooldown;
				Timer = timer;
			}
		}

		public readonly struct SaveData
		{
			public readonly float Timer;
			public readonly float Multiplier;

			public SaveData(float timer, float multiplier)
			{
				Timer = timer;
				Multiplier = multiplier;
			}
		}
	}
}