namespace ModiBuff.Core.Units
{
	public sealed class SingleInstanceStatusEffectEffect : IMutableStateEffect, IStackEffect, IRevertEffect,
		IEffect, IModifierStateInfo<SingleInstanceStatusEffectEffect.Data>
	{
		public bool IsRevertible { get; }
		public bool UsesMutableState => IsRevertible || _stackEffect.UsesMutableState();

		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;
		private readonly StackEffectType _stackEffect;
		private readonly float _stackValue;

		private float _extraDuration;
		private float _totalDuration;

		public SingleInstanceStatusEffectEffect(StatusEffectType statusEffectType, float duration,
			bool revertible = false, StackEffectType stackEffect = StackEffectType.Effect, float stackValue = -1)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
			_stackValue = stackValue;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (IsRevertible)
				_totalDuration = _duration + _extraDuration;

			((ISingleInstanceStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController
				.ChangeStatusEffect(_statusEffectType, _duration + _extraDuration);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((ISingleInstanceStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController
				.DecreaseStatusEffect(_statusEffectType, _totalDuration);

			_totalDuration = 0;
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDuration += _stackValue;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDuration += _stackValue * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public Data GetEffectData() => new Data(_duration, _extraDuration);

		public void ResetState()
		{
			_extraDuration = 0;
			_totalDuration = 0;
		}

		public IEffect ShallowClone() =>
			new SingleInstanceStatusEffectEffect(_statusEffectType, _duration, IsRevertible, _stackEffect, _stackValue);

		object IShallowClone.ShallowClone() => ShallowClone();

		public readonly struct Data
		{
			public readonly float BaseDuration;
			public readonly float ExtraDuration;

			public Data(float baseDuration, float extraDuration)
			{
				BaseDuration = baseDuration;
				ExtraDuration = extraDuration;
			}
		}
	}
}