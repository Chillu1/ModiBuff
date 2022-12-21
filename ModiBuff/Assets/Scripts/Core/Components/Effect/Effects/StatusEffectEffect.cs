namespace ModiBuff.Core
{
	public sealed class StatusEffectEffect : IStackEffect, IRevertEffect, IEffect
	{
		public bool IsRevertible { get; }

		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;
		private readonly StackEffectType _stackEffect;

		private float _extraDuration;

		public StatusEffectEffect(StatusEffectType statusEffectType, float duration, StackEffectType stackEffect = StackEffectType.None)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
			_stackEffect = stackEffect;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			target.ChangeStatusEffect(_statusEffectType, _duration + _extraDuration);
		}

		public void RevertEffect(IUnit target, IUnit owner)
		{
			target.DecreaseStatusEffect(_statusEffectType, _duration + _extraDuration);
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDuration += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDuration += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(targetComponent.Target, targetComponent.Owner);
		}

		public IStackEffect ShallowClone() => new StatusEffectEffect(_statusEffectType, _duration, _stackEffect);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}