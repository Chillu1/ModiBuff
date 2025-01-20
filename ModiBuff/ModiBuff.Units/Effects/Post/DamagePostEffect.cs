namespace ModiBuff.Core.Units
{
	public sealed class DamagePostEffect : IPostEffect<float>, ISaveableRecipeEffect,
		IMetaEffectOwner<DamagePostEffect, float, float>
	{
		private readonly Targeting _targeting;

		private IMetaEffect<float, float>[] _metaEffects; //TODO Serialize

		public DamagePostEffect(Targeting targeting = Targeting.TargetSource)
		{
			_targeting = targeting;
		}

		public DamagePostEffect SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
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

			if (_metaEffects != null)
				foreach (var metaEffect in _metaEffects)
					if (metaEffect is not IConditionEffect conditionEffect ||
					    conditionEffect.Check(value, target, source))
						value = metaEffect.Effect(value, target, source);

			attackableTarget.TakeDamage(value, source);
		}

		//TODO
		public object SaveRecipeState() => new object();
	}
}