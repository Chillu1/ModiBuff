using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		private readonly float _duration;
		private float _time;

		private ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;

		public DurationComponent(float duration, IEffect[] effects)
		{
			_duration = duration;
			_effects = effects;
		}

		public DurationComponent(float duration, IEffect effect) : this(duration, new[] { effect })
		{
		}

		public DurationComponent(float duration, IRemoveEffect effect) : this(duration, new IEffect[] { effect })
		{
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

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

		public ITimeComponent DeepClone() => new DurationComponent(_duration, _effects);
	}
}