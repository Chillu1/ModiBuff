namespace ModiBuff.Core.Units
{
	/// <summary>
	///		Damage effect with no mutable state, example of a modifier-less effect implementation, don't use with modifiers
	/// </summary>
	public sealed class DamageEffectNoState : IEffect, IMetaEffectOwner<DamageEffectNoState, float, float>,
		IPostEffectOwner<DamageEffectNoState, float>
	{
		private readonly float _damage;
		private readonly Targeting _targeting;
		private IMetaEffect<float, float>[] _metaEffects;
		private IPostEffect<float>[] _postEffects;

		public DamageEffectNoState(float damage, Targeting targeting = Targeting.TargetSource)
		{
			_damage = damage;
			_targeting = targeting;
		}

		public DamageEffectNoState SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
		}

		public DamageEffectNoState SetPostEffects(params IPostEffect<float>[] postEffects)
		{
			_postEffects = postEffects;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			float returnDamageInfo = 0;

			_targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
			if (effectTarget is IDamagable<float, float, float, float> damagableTarget)
			{
				float damage = _damage;

				if (_metaEffects != null)
					foreach (var metaEffect in _metaEffects)
						damage = metaEffect.Effect(damage, target, source);

				returnDamageInfo = damagableTarget.TakeDamage(damage, effectSource);
			}

			if (_postEffects != null)
				foreach (var postEffect in _postEffects)
					postEffect.Effect(returnDamageInfo, target, source);
		}
	}
}