namespace ModiBuff.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		public float Timer => _timer;
		public float Time => _customDuration ?? _duration;

		private readonly float _duration;
		private readonly bool _isRefreshable;
		private readonly IEffect[] _effects;
		private float _timer;

		private ITargetComponent _targetComponent;

		private float? _customDuration;

		public DurationComponent(float duration, bool refreshable, IEffect[] effects)
		{
			_duration = duration;
			_isRefreshable = refreshable;
			_effects = effects;
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
		}

		public void Update(float deltaTime)
		{
			float duration = _customDuration ?? _duration;
			if (_timer >= duration)
				return;

			_timer += deltaTime;

			if (_timer < duration)
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
				_timer = 0;
		}

		public void SetData(ModifierData data)
		{
			if (data is ModifierDurationData durationData)
				_customDuration = durationData.Duration;
		}

		public void ResetState()
		{
			_timer = 0;
			_customDuration = null;
		}

		public TimeComponentSaveData SaveState() => new TimeComponentSaveData(_timer, _customDuration);

		public void LoadState(TimeComponentSaveData saveData)
		{
			_timer = saveData.Timer;
			_customDuration = saveData.CustomTime;
		}
	}
}