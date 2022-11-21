using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		private readonly float _duration;
		private float _time;

		private readonly ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;

		public DurationComponent(float duration, ITargetComponent targetComponent, IEffect[] effects)
		{
			_duration = duration;
			_targetComponent = targetComponent;
			_effects = effects;
		}

		public DurationComponent(float duration, ITargetComponent targetComponent, IEffect effect)
			: this(duration, targetComponent, new[] { effect })
		{
		}

		public DurationComponent(float duration, ITargetComponent targetComponent, IRemoveEffect effect)
			: this(duration, targetComponent, new IEffect[] { effect })
		{
		}

		public void Update(in float deltaTime)
		{
			if (_time >= _duration)
				return;

			_time += deltaTime;
			if (_time >= _duration)
			{
				int length = _effects.Length;
				for (int i = 0; i < length; i++)
					_effects[i].Effect(_targetComponent.Target, _targetComponent.Owner);
			}
		}
	}
}