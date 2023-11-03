using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
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
			float damage = _damage;

			if (_metaEffects != null)
				foreach (var metaEffect in _metaEffects)
					damage = metaEffect.Effect(damage, target, source);

			float returnDamageInfo = Effect(damage, target, source);

			if (_postEffects != null)
				foreach (var postEffect in _postEffects)
					postEffect.Effect(returnDamageInfo, target, source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float Effect(float damage, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);

#if DEBUG && !MODIBUFF_PROFILE
			if (!(target is IDamagable<float, float, float, float>))
				throw new ArgumentException("Target must implement IDamagable");
#endif

			float returnDamage =
#if !DEBUG && UNSAFE
				Unsafe.As<IDamagable<float, float, float, float>>(target).TakeDamage(damage, source);
#else
				((IDamagable<float, float, float, float>)target).TakeDamage(damage, source);
#endif

			return returnDamage;
		}
	}
}