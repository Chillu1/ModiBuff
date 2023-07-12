using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class IntervalComponent : ITimeComponent
	{
		public bool IsRefreshable { get; }

		private readonly float _interval;
		private float _timer;

		private ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;
		private readonly bool _check;

		private readonly ModifierCheck _modifierCheck;

		//private int _intervalCount;
		//private float _totalTime;

		public IntervalComponent(float interval, bool refreshable, IEffect[] effects, ModifierCheck check)
		{
			_interval = interval;
			IsRefreshable = refreshable;
			_effects = effects;
			_modifierCheck = check;

			_check = check != null;
		}

		public IntervalComponent(float interval, bool refreshable, IEffect effect, ModifierCheck check) :
			this(interval, refreshable, new[] { effect }, check)
		{
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Update(float deltaTime)
		{
			_timer += deltaTime;
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
			_targetComponent = null;
		}

		public ITimeComponent DeepClone() => new IntervalComponent(_interval, IsRefreshable, _effects, _modifierCheck);
	}
}