namespace ModiBuff.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		public float Timer => _timer + _extraDuration;
		public float Time => _duration + _extraDuration;

		private readonly float _duration;
		private readonly bool _isRefreshable;
		private readonly IEffect[] _effects;
		private readonly bool _affectedByStatusResistance;
		private float _extraDuration;
		private float _timer;

		private ITargetComponent _targetComponent;

		private bool _statusResistanceImplemented;
		private IStatusResistance _statusResistanceTarget;

		public DurationComponent(float duration, bool refreshable, IEffect[] effects, bool affectedByStatusResistance)
		{
			_duration = duration;
			_isRefreshable = refreshable;
			_effects = effects;
			_affectedByStatusResistance = affectedByStatusResistance;

			_timer = duration;
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
			if (_timer == 0)
				return;

			//Special calculation if target has status resistance functionality
			float deltaChange = _affectedByStatusResistance && _statusResistanceImplemented
				? deltaTime / _statusResistanceTarget.StatusResistance
				: deltaTime;

			if (_extraDuration == 0)
				_timer -= deltaChange;
			else
				_extraDuration -= deltaChange;

			if (_timer > 0)
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
				_timer = _duration;
		}

		public void AddDuration(float duration) => _extraDuration += duration;

		public void ResetState()
		{
			_timer = _duration;
			_statusResistanceImplemented = false;
			_statusResistanceTarget = null;
		}

		public TimeComponentSaveData SaveState() => new TimeComponentSaveData(_timer, _statusResistanceImplemented);

		public void LoadState(TimeComponentSaveData saveData)
		{
			_timer = saveData.Timer;
			_statusResistanceImplemented = saveData.StatusResistanceImplemented;
		}
	}
}