namespace ModiBuff.Core.Units
{
	public sealed class DamagePostEffect : IPostEffect<float>, ISaveableRecipeEffect
	{
		private readonly Targeting _targeting;

		public DamagePostEffect(Targeting targeting = Targeting.TargetSource)
		{
			_targeting = targeting;
		}

		public void Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			if (target is not IAttackable<float, float> attackableTarget)
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IAttackable<float, float>));
#endif
				return;
			}

			attackableTarget.TakeDamage(value, source);
		}

		//TODO
		public object SaveRecipeState() => new object();
	}
}