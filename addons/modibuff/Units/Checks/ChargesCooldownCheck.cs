namespace ModiBuff.Core.Units
{
	public sealed class ChargesCooldownCheck : IUpdatableCheck, INoUnitCheck, IDataCheck<ChargesCooldownCheck.Data>,
		IStateCheck<ChargesCooldownCheck, ChargesCooldownCheck.SaveData>
	{
		private readonly float _cooldown;
		private readonly int _maxCharges;

		private float _timer;
		private int _charges;

		public ChargesCooldownCheck(float cooldown, int maxCharges)
		{
			_cooldown = cooldown;
			_maxCharges = maxCharges;

			_charges = maxCharges;
		}

		public void Update(float deltaTime)
		{
			if (_timer <= 0 && _charges == _maxCharges)
				return;

			_timer -= deltaTime;

			if (_timer > 0)
				return;

			_timer = _cooldown;
			_charges++;
		}

		public bool Check() => _charges > 0;

		public Data GetData() => new Data(_cooldown, _timer);

		/// <summary>
		///		Resets the timer to cooldown, so the check is not ready.
		/// </summary>
		public void RestartState() => _charges--;

		/// <summary>
		///		Sets the timer to 0, so the check is ready.
		/// </summary>
		public void ResetState()
		{
			_timer = 0;
			_charges = _maxCharges;
		}

		public ChargesCooldownCheck ShallowClone() => new ChargesCooldownCheck(_cooldown, _maxCharges);
		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_timer, _charges);

		public void LoadState(object data)
		{
			var saveData = (SaveData)data;
			_timer = saveData.Timer;
			_charges = saveData.Charges;
		}

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

		public struct SaveData
		{
			public readonly float Timer;
			public readonly int Charges;

			public SaveData(float timer, int charges)
			{
				Timer = timer;
				Charges = charges;
			}
		}
	}
}