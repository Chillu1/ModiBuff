namespace ModiBuff.Core.Units
{
	public sealed class StatusEffectEffect : IStateEffect, IStackEffect, IRevertEffect, IEffect, IModifierIdOwner
	{
		public bool IsRevertible { get; }

		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;
		private readonly StackEffectType _stackEffect;
		private int _id;

		private float _extraDuration;
		private float _totalDuration;

		public StatusEffectEffect(StatusEffectType statusEffectType, float duration, bool revertible = false,
			StackEffectType stackEffect = StackEffectType.Effect) : this(statusEffectType, duration, revertible, stackEffect, -1)
		{
		}

		private StatusEffectEffect(StatusEffectType statusEffectType, float duration, bool revertible, StackEffectType stackEffect, int id)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
			_id = id;
		}

		public void SetModifierId(int id) => _id = id;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_id == -1)
				Logger.LogError("ModifierId is not set for status effect effect.");
#endif

			if (IsRevertible)
				_totalDuration = _duration + _extraDuration;
			((IStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController
				.ChangeStatusEffect(_id, _statusEffectType, _duration + _extraDuration);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_id == -1)
				Logger.LogError("ModifierId is not set for status effect effect.");
#endif

			((IStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController
				.DecreaseStatusEffect(_id, _statusEffectType, _totalDuration);
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

		public IEffect ShallowClone() => new StatusEffectEffect(_statusEffectType, _duration, IsRevertible, _stackEffect, _id);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}