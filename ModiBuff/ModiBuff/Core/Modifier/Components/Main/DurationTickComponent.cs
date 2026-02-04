namespace ModiBuff.Core
{
    public sealed class DurationTickComponent : ITimeTickComponent
	{
		public float CurrentTicks => _ticks;
		public float Ticks => _tickDuration;

		private readonly int _tickDuration;
		private readonly bool _isRefreshable;
		private int _ticks;

		private ITargetComponent _targetComponent;

		private readonly IEffect[] _effects;

		private int? _customDuration;

		public DurationTickComponent(int tickDuration, bool refreshable, IEffect[] effects)
		{
			_tickDuration = tickDuration;
			_isRefreshable = refreshable;
			_effects = effects;
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Update()
		{
			var tickDuration = _customDuration ?? _tickDuration;
			if (_ticks >= tickDuration)
				return;

			_ticks++;

			if (_ticks < tickDuration)
				return;

			switch (_targetComponent)
			{
				case MultiTargetComponent targetComponent:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].Effect(targetComponent.Targets, targetComponent.Source);
					break;
				case SingleTargetComponent targetComponent:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].Effect(targetComponent.Target, targetComponent.Source);
					break;
			}
		}

		public void Refresh()
		{
			if (_isRefreshable)
				_ticks = 0;
		}

		public void SetData(ModifierData data)
		{
			if (data is ModifierDurationTickData durationTickData)
				_customDuration = durationTickData.DurationTick;
		}

		public void ResetState() => _ticks = 0;

		public TimeTickComponentSaveData SaveState() => new TimeTickComponentSaveData(_ticks, _customDuration);
		public void LoadState(TimeTickComponentSaveData saveData)
		{
			_ticks = saveData.CurrentTicks;
			_customDuration = saveData.CustomTicks;
		}
	}
}