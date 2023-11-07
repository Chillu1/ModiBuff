namespace ModiBuff.Core.Units
{
	public sealed class DispelStatusEffectEffect : IEffect, IStackEffect
	{
		private readonly StatusEffectType _statusEffect;

		public DispelStatusEffectEffect(StatusEffectType statusEffect)
		{
			_statusEffect = statusEffect;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (_statusEffect == StatusEffectType.All)
			{
				((IStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController.DispelAll(source);
				return;
			}

			((IStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController
				.DispelStatusEffect(_statusEffect, source);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			Effect(target, source);
		}
	}
}