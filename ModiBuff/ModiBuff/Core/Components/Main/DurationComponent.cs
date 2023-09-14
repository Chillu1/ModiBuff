namespace ModiBuff.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		public bool IsRefreshable { get; }

		private readonly float _duration;
		private float _timer;

		private ITargetComponent _targetComponent;

		private bool _statusResistanceImplemented;
		private IStatusResistance _statusResistanceTarget;

		private readonly IEffect[] _effects;

		public DurationComponent(float duration, bool refreshable, IEffect[] effects)
		{
			_duration = duration;
			IsRefreshable = refreshable;
			_effects = effects;
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
			if (_timer >= _duration)
				return;

			//Special calculation if target has status resistance functionality
			_timer += _statusResistanceImplemented ? deltaTime / _statusResistanceTarget.StatusResistance : deltaTime;

			if (_timer < _duration)
				return;

			int length = _effects.Length;
			switch (_targetComponent)
			{
				case MultiTargetComponent targetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].Effect(targetComponent.Targets, targetComponent.Source);
					break;
				case SingleTargetComponent targetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].Effect(targetComponent.Target, targetComponent.Source);
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