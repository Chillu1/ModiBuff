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

		//private int _intervalCount;
		//private float _totalTime;

		public IntervalComponent(float interval, bool refreshable, IEffect[] effects)
		{
			_interval = interval;
			IsRefreshable = refreshable;
			_effects = effects;
		}

		public IntervalComponent(float interval, bool refreshable, IEffect effect) : this(interval, refreshable, new[] { effect })
		{
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
			_targetComponent = targetComponent;
		}

		public void Update(in float deltaTime)
		{
			_timer += deltaTime;
			if (_timer < _interval)
				return;

			//_intervalCount++; //TODO Add this as an idea to ModifierLibraryOrg. Any way to use these smart?
			//_totalTime += _timer;

			_timer -= _interval;

			int length = _effects.Length;
			for (int i = 0; i < length; i++)
			{
				//Debug.Log("Type: " + _effects[i].GetType().Name);
				_effects[i].Effect(_targetComponent.Target, _targetComponent.Owner);
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

		public ITimeComponent DeepClone() => new IntervalComponent(_interval, IsRefreshable, _effects);
	}
}