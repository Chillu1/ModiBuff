namespace ModiBuff.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		public float Timer => _timer;
		public float Time => _customDuration ?? _duration;

		private readonly float _duration;
		private readonly bool _isRefreshable;
		private readonly IEffect[] _effects;
		private readonly bool _affectedByStatusResistance;
		private float _timer;

		private ITargetComponent _targetComponent;

		private bool _statusResistanceImplemented;
		private IStatusResistance _statusResistanceTarget;

		private float? _customDuration;

		public DurationComponent(float duration, bool refreshable, IEffect[] effects, bool affectedByStatusResistance)
		{
			_duration = duration;
			_isRefreshable = refreshable;
			_effects = effects;
			_affectedByStatusResistance = affectedByStatusResistance;
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
			UpdateTargetStatusResistance();
		}

		public void UpdateTargetStatusResistance()
		{
			if (_affectedByStatusResistance && _targetComponent is SingleTargetComponent singleTargetComponent &&
			    singleTargetComponent.Target is IStatusResistance statusResistance)
			{
				_statusResistanceImplemented = true;
				_statusResistanceTarget = statusResistance;
			}
		}

		public void Update(float deltaTime)
		{
			float duration = _customDuration ?? _duration;
			if (_timer >= duration)
				return;

			//Special calculation if target has status resistance functionality
			_timer += _affectedByStatusResistance && _statusResistanceImplemented
				? deltaTime / _statusResistanceTarget.StatusResistance
				: deltaTime;

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
			_statusResistanceImplemented = false;
			_statusResistanceTarget = null;
			_customDuration = null;
		}

		public TimeComponentSaveData SaveState() =>
			new TimeComponentSaveData(_timer, _statusResistanceImplemented, _customDuration);

		public void LoadState(TimeComponentSaveData saveData)
		{
			_timer = saveData.Timer;
			_statusResistanceImplemented = saveData.StatusResistanceImplemented;
			_customDuration = saveData.CustomTime;
		}
	}
}