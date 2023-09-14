namespace ModiBuff.Core
{
	public sealed class IntervalComponent : ITimeComponent
	{
		public bool IsRefreshable { get; }

		private readonly float _interval;
		private float _timer;

		private ITargetComponent _targetComponent;

		private readonly bool _usesStatusResistance;
		private bool _statusResistanceImplemented;
		private IStatusResistance _statusResistanceTarget;

		private readonly IEffect[] _effects;

		private readonly ModifierCheck _modifierCheck;

		//private int _intervalCount;
		//private float _totalTime;

		public IntervalComponent(float interval, bool refreshable, IEffect[] effects, ModifierCheck check, bool affectedByStatusResistance)
		{
			_interval = interval;
			IsRefreshable = refreshable;
			_effects = effects;
			_modifierCheck = check;
			_usesStatusResistance = affectedByStatusResistance;
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
			if (targetComponent is SingleTargetComponent singleTargetComponent &&
			    singleTargetComponent.Target is IStatusResistance statusResistance)
			{
				_statusResistanceImplemented = true;
				_statusResistanceTarget = statusResistance;
			}
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
			_timer += _usesStatusResistance && _statusResistanceImplemented
				? deltaTime / _statusResistanceTarget.StatusResistance
				: deltaTime;

			if (_timer < _interval)
				return;

			//_intervalCount++;
			//_totalTime += _timer;

			_timer -= _interval;

			if (_modifierCheck != null && !_modifierCheck.Check(_targetComponent.Source))
				return;

			int length = _effects.Length;
			switch (_targetComponent)
			{
				case MultiTargetComponent multiTargetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].Effect(multiTargetComponent.Targets, multiTargetComponent.Source);
					break;
				case SingleTargetComponent singleTargetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].Effect(singleTargetComponent.Target, singleTargetComponent.Source);
					break;
			}
		}

		public void Refresh()
		{
			if (IsRefreshable)
				_timer = 0;
		}

		public void ResetState()
		{
			_timer = 0;
			_statusResistanceImplemented = false;
			_statusResistanceTarget = null;
		}
	}
}