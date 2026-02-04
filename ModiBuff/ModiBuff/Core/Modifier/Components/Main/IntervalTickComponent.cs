namespace ModiBuff.Core
{
	public sealed class IntervalTickComponent : ITimeTickComponent
	{
		public float CurrentTicks => _ticks;
		public float Ticks => _tickInterval;

		private readonly int _tickInterval;
		private readonly bool _isRefreshable;
		private int _ticks;

		private ITargetComponent _targetComponent;

		private readonly IEffect[] _effects;

		private readonly ModifierCheck? _modifierCheck;

		private int? _customInterval;

		public IntervalTickComponent(int tickInterval, bool refreshable, IEffect[] effects, ModifierCheck? check)
		{
			_tickInterval = tickInterval;
			_isRefreshable = refreshable;
			_effects = effects;
			_modifierCheck = check;
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Update()
		{
			_ticks++;

			if (_ticks < (_customInterval ?? _tickInterval))
				return;

			_ticks = 0;

			if (_modifierCheck?.CheckUse(_targetComponent.Source) == false)
				return;

			switch (_targetComponent)
			{
				case MultiTargetComponent multiTargetComponent:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].Effect(multiTargetComponent.Targets, multiTargetComponent.Source);
					break;
				case SingleTargetComponent singleTargetComponent:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].Effect(singleTargetComponent.Target, singleTargetComponent.Source);
					break;
			}
		}

		public void Refresh()
		{
			if (_isRefreshable)
				_ticks = 0;
		}

		public void SetData(ModifierData data)
		{
			if (data is ModifierIntervalTickData intervalTickData)
				_customInterval = intervalTickData.IntervalTick;
		}

		public void ResetState() => _ticks = 0;

		public TimeTickComponentSaveData SaveState() => new TimeTickComponentSaveData(_ticks, _customInterval);
		public void LoadState(TimeTickComponentSaveData saveData)
		{
			_ticks = saveData.CurrentTicks;
			_customInterval = saveData.CustomTicks;
		}
	}
}