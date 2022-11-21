namespace ModifierLibraryLite
{
	public class TimeComponent : ITimeComponent
	{
		private readonly float _time;
		private float _timer;

		private readonly ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;

		private int _intervalCount;
		private float _totalTime;

		public TimeComponent(float time, ITargetComponent targetComponent, IEffect[] effects)
		{
			_time = time;
			_targetComponent = targetComponent;
			_effects = effects;
		}
		
		public TimeComponent(float time, ITargetComponent targetComponent, IEffect removeEffect)
		{
			_time = time;
			_targetComponent = targetComponent;
			_effects = new[] { removeEffect };
		}

		public void Update(float deltaTime)
		{
			_timer += deltaTime;
			if (_timer < _time)
				return;

			_intervalCount++;
			_totalTime += _timer; //How to use these smart?
			_timer = 0;

			foreach (var effect in _effects)
				effect.Effect(_targetComponent.Target);
		}
	}
}