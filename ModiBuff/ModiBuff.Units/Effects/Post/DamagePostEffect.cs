namespace ModiBuff.Core.Units
{
	public sealed class DamagePostEffect : IPostEffect<float>, IMetaEffectOwner<DamagePostEffect, float, float>,
		ISaveableRecipeEffect<DamagePostEffect.RecipeSaveData>
	{
		private readonly Targeting _targeting;

		private IMetaEffect<float, float>[] _metaEffects;

		public DamagePostEffect(Targeting targeting = Targeting.TargetSource) : this(targeting, null)
		{
		}

		private DamagePostEffect(Targeting targeting, IMetaEffect<float, float>[] metaEffects)
		{
			_targeting = targeting;
			_metaEffects = metaEffects;
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

			value = _metaEffects.TryApply(value, target, source);

			attackableTarget.TakeDamage(value, source);
		}

		public object SaveRecipeState() => new RecipeSaveData(_targeting, this.GetMetaSaveData(_metaEffects));

		public readonly struct RecipeSaveData
		{
			public readonly Targeting Targeting;
			public readonly object[] MetaEffects;

			public RecipeSaveData(Targeting targeting, object[] metaEffects)
			{
				Targeting = targeting;
				MetaEffects = metaEffects;
			}
		}
	}
}