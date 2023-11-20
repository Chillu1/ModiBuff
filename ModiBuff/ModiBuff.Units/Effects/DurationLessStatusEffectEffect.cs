namespace ModiBuff.Core.Units
{
	public sealed class DurationLessStatusEffectEffect : IStackEffect, IEffect, IRevertEffect
	{
		public bool IsRevertible => true;

		private readonly StatusEffectType _statusEffectType;
		private readonly StackEffectType _stackEffect;

		public DurationLessStatusEffectEffect(StatusEffectType statusEffectType,
			StackEffectType stackEffect = StackEffectType.Effect)
		{
			_statusEffectType = statusEffectType;
			_stackEffect = stackEffect;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (!(target is IDurationLessStatusEffectOwner<LegalAction, StatusEffectType> statusEffectTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target,
					nameof(IDurationLessStatusEffectOwner<LegalAction, StatusEffectType>));
#endif
				return;
			}

			statusEffectTarget.StatusEffectController.ApplyStatusEffect(_statusEffectType);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (!(target is IDurationLessStatusEffectOwner<LegalAction, StatusEffectType> statusEffectTarget))
				return;

			statusEffectTarget.StatusEffectController.RemoveStatusEffect(_statusEffectType);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}
	}
}