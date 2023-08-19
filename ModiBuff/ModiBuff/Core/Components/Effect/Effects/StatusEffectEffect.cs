namespace ModiBuff.Core
{
	public sealed class StatusEffectEffect : IStateEffect, IStackEffect, IRevertEffect, IEffect
	{
		public bool IsRevertible { get; }

		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;
		private readonly StackEffectType _stackEffect;

		private float _extraDuration;
		private float _totalDuration;

		public StatusEffectEffect(StatusEffectType statusEffectType, float duration, bool revertible = false,
			StackEffectType stackEffect = StackEffectType.Effect)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (IsRevertible)
				_totalDuration = _duration + _extraDuration;
			((IStatusEffectOwner)target).StatusEffectController.ChangeStatusEffect(_statusEffectType, _duration + _extraDuration);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((IStatusEffectOwner)target).StatusEffectController.DecreaseStatusEffect(_statusEffectType, _totalDuration);
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDuration += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDuration += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void ResetState()
		{
			_extraDuration = 0;
			_totalDuration = 0;
		}

		public IStateEffect ShallowClone() => new StatusEffectEffect(_statusEffectType, _duration, IsRevertible, _stackEffect);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}