using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class DurationComponent : ITimeComponent
	{
		public bool IsRefreshable { get; }

		private readonly float _duration;
		private float _timer;

		private ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;

		public DurationComponent(float duration, bool refreshable, IEffect[] effects)
		{
			_duration = duration;
			IsRefreshable = refreshable;
			_effects = effects;
		}

		public DurationComponent(float duration, bool refreshable, IEffect effect) : this(duration, refreshable, new[] { effect })
		{
		}

		public DurationComponent(float duration, bool refreshable, IRemoveEffect effect) :
			this(duration, refreshable, new IEffect[] { effect })
		{
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Update(in float deltaTime)
		{
			if (_timer >= _duration)
				return;

			_timer += deltaTime;
			if (_timer >= _duration)
			{
				int length = _effects.Length;
				for (int i = 0; i < length; i++)
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

		public ITimeComponent DeepClone() => new DurationComponent(_duration, IsRefreshable, _effects);
	}
}