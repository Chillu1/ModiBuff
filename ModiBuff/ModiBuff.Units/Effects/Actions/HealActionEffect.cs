namespace ModiBuff.Core.Units
{
	public sealed class HealActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit source)
		{
			if (!(source is IHealer<float, float> healerSource))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplErrorSource(source, nameof(IHealer<float, float>));
#endif
				return;
			}

			if (!(target is IHealable<float, float> healableTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IHealable<float, float>));
#endif
				return;
			}

			healerSource.Heal(healableTarget);
		}
	}
}