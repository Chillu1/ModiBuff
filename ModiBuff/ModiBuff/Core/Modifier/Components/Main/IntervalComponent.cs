namespace ModiBuff.Core
{
	public sealed class IntervalComponent : ITimeComponent
	{
		public float Timer => _timer;
		public float Time => _customInterval ?? _interval;

		private readonly float _interval;
		private readonly bool _isRefreshable;
		private float _timer;

		private ITargetComponent _targetComponent;

		private readonly bool _affectedByStatusResistance;
		private bool _statusResistanceImplemented;
		private IStatusResistance _statusResistanceTarget;

		private readonly IEffect[] _effects;

		private readonly ModifierCheck _modifierCheck;

		private float? _customInterval;
		//private int _intervalCount;
		//private float _totalTime;

		public IntervalComponent(float interval, bool refreshable, IEffect[] effects, ModifierCheck check,
			bool affectedByStatusResistance)
		{
			_interval = interval;
			_isRefreshable = refreshable;
			_effects = effects;
			_modifierCheck = check;
			_affectedByStatusResistance = affectedByStatusResistance;
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
			UpdateTargetStatusResistance();
		}

		public void UpdateTargetStatusResistance()
		{
			if (_targetComponent is SingleTargetComponent singleTargetComponent &&
			    singleTargetComponent.Target is IStatusResistance statusResistance)
			{
				_statusResistanceImplemented = true;
				_statusResistanceTarget = statusResistance;
			}
		}

		public void Update(float deltaTime)
		{
			//Special calculation if target has status resistance functionality
			_timer += _affectedByStatusResistance && _statusResistanceImplemented
				? deltaTime / _statusResistanceTarget.StatusResistance
				: deltaTime;

			float interval = _customInterval ?? _interval;
			if (_timer < interval)
				return;

			//_intervalCount++;
			//_totalTime += _timer;

			_timer -= interval;

			if (_modifierCheck != null && !_modifierCheck.CheckUse(_targetComponent.Source))
				return;

			switch (_targetComponent)
			{
				case MultiTargetComponent multiTargetComponent:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].Effect(multiTargetComponent.Targets, multiTargetComponent.Source);
					break;
				case SingleTargetComponent singleTargetComponent:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].Effect(singleTargetComponent.Target, singleTargetComponent.Source);
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
			if (data is ModifierIntervalData intervalData)
				_customInterval = intervalData.Interval;
		}

		public void ResetState()
		{
			_timer = 0;
			_statusResistanceImplemented = false;
			_statusResistanceTarget = null;
			_customInterval = null;
		}

		public TimeComponentSaveData SaveState() =>
			new TimeComponentSaveData(_timer, _statusResistanceImplemented, _customInterval);

		public void LoadState(TimeComponentSaveData saveData)
		{
			_timer = saveData.Timer;
			_statusResistanceImplemented = saveData.StatusResistanceImplemented;
			_customInterval = saveData.CustomTime;
		}
	}
}