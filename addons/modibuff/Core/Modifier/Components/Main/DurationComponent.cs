namespace ModiBuff.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		public float Timer => _timer;
		public float Time => _duration;

		private readonly float _duration;
		private readonly bool _isRefreshable;
		private readonly IEffect[] _effects;
		private readonly bool _affectedByStatusResistance;
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
			if (_timer >= _duration)
				return;

			//Special calculation if target has status resistance functionality
			_timer += _affectedByStatusResistance && _statusResistanceImplemented
				? deltaTime / _statusResistanceTarget.StatusResistance
				: deltaTime;

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
			if (_isRefreshable)
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