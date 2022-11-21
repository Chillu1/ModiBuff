using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class IntervalComponent : ITimeComponent
	{
		private readonly float _interval;
		private float _timer;

		private readonly ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;

		//private int _intervalCount;
		//private float _totalTime;

		public IntervalComponent(float interval, ITargetComponent targetComponent, IEffect[] effects)
		{
			_interval = interval;
			_targetComponent = targetComponent;
			_effects = effects;
		}

		public IntervalComponent(float interval, ITargetComponent targetComponent, IEffect effect)
			: this(interval, targetComponent, new[] { effect })
		{
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
	}
}