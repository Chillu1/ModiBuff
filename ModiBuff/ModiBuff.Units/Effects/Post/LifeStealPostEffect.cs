using System;

namespace ModiBuff.Core.Units
{
	public sealed class LifeStealPostEffect : IConditionEffect, IPostEffect<float>,
		IMetaEffectOwner<LifeStealPostEffect, float, float>
	{
		public Condition[] Conditions { get; set; } = Array.Empty<Condition>();

		private readonly float _lifeStealPercent;
		private readonly Targeting _targeting;

		private IMetaEffect<float, float>[] _metaEffects;

		public LifeStealPostEffect(float lifeStealPercent, Targeting targeting = Targeting.TargetSource)
		{
			_lifeStealPercent = lifeStealPercent;
			_targeting = targeting;
		}

		public LifeStealPostEffect SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
		}

		public void Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);

			if (!((IUnitEntity)source).UnitTag.HasTag(UnitTag.Lifestealable))
				return;
			if (target is not IHealable<float, float> healableTarget)
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IHealable<float, float>));
#endif
				return;
			}

			float lifeStealPercent = _lifeStealPercent;

			if (_metaEffects != null)
				foreach (var metaEffect in _metaEffects)
					if (metaEffect is not IConditionEffect conditionEffect ||
					    conditionEffect.Check(lifeStealPercent, target, source))
						lifeStealPercent = metaEffect.Effect(lifeStealPercent, target, source);

			healableTarget.Heal(value * lifeStealPercent, source);
		}
	}
}