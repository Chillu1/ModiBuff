namespace ModiBuff.Core
{
	public sealed class IntervalComponent : ITimeComponent
	{
		public bool IsRefreshable { get; }

		private readonly float _interval;
		private float _timer;

		private ITargetComponent _targetComponent;

		private bool _usesStatusResistance;
		private bool _statusResistanceImplemented;
		private IStatusResistance _statusResistanceTarget;

		private readonly IEffect[] _effects;
		private readonly bool _check;

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

			_check = check != null;
		}

		public IntervalComponent(float interval, bool refreshable, IEffect effect, ModifierCheck check, bool usesStatusResistance) :
			this(interval, refreshable, new[] { effect }, check, usesStatusResistance)
		{
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
			if (targetComponent is ISingleTargetComponent singleTargetComponent &&
			    singleTargetComponent.Target is IStatusResistance statusResistance)
			{
				_statusResistanceImplemented = true;
				_statusResistanceTarget = statusResistance;
			}
		}

		public void UpdateOwner(IUnit owner)
		{
			if (_targetComponent is ISingleTargetComponent singleTargetComponent &&
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

			if (_check && !_modifierCheck.Check(_targetComponent.Source))
				return;

			int length = _effects.Length;
			switch (_targetComponent)
			{
				case IMultiTargetComponent targetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].Effect(targetComponent.Targets, targetComponent.Source);
					break;
				case ISingleTargetComponent targetComponent:
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
			_targetComponent.ResetState();
			_statusResistanceImplemented = false;
			_statusResistanceTarget = null;
		}

		public ITimeComponent DeepClone() =>
			new IntervalComponent(_interval, IsRefreshable, _effects, _modifierCheck, _usesStatusResistance);
	}
}