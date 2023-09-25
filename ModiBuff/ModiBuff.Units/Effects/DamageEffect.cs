#if NET5_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_1_OR_GREATER
#define UNSAFE
#endif

using System;

namespace ModiBuff.Core.Units
{
	public sealed class DamageEffect : ITargetEffect, IEventTrigger, IStackEffect, IStateEffect, IEffect,
		IMetaEffectOwner<DamageEffect, float, float>, IPostEffectOwner<DamageEffect, float>
	{
		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;
		private bool _isEventBased;
		private IMetaEffect<float, float>[] _metaEffects;
		private bool _hasMetaEffects;
		private IPostEffect<float>[] _postEffects;
		private bool _hasPostEffects;

		private float _extraDamage;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.Effect) :
			this(damage, stackEffect, Targeting.TargetSource, false, null, null)
		{
		}

		private DamageEffect(float damage, StackEffectType stackEffect, Targeting targeting, bool isEventBased,
			IMetaEffect<float, float>[] metaEffects, IPostEffect<float>[] postEffects)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
			_targeting = targeting;
			_isEventBased = isEventBased;
			_metaEffects = metaEffects;
			_hasMetaEffects = metaEffects != null;
			_postEffects = postEffects;
			_hasPostEffects = postEffects != null;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;
		public void SetEventBased() => _isEventBased = true;

		public DamageEffect SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			_hasMetaEffects = true;
			return this;
		}

		public DamageEffect SetPostEffects(params IPostEffect<float>[] postEffects)
		{
			_postEffects = postEffects;
			_hasPostEffects = true;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
#if DEBUG && !MODIBUFF_PROFILE
			if (!(target is IDamagable<float, float, float, float>))
				throw new ArgumentException("Target must implement IDamagable");
#endif

			float damage = _baseDamage;

			if (_hasMetaEffects)
				foreach (var metaEffect in _metaEffects)
					damage = metaEffect.Effect(damage, target, source);

			damage += _extraDamage;

			float returnDamageInfo =
#if !DEBUG && UNSAFE
				Unsafe.As<IDamagable<float, float, float, float>>(target).TakeDamage(damage, source, !_isEventBased);
#else
				((IDamagable<float, float, float, float>)target).TakeDamage(damage, source, !_isEventBased);
#endif
			if (!_hasPostEffects)
				return;

			foreach (var postEffect in _postEffects)
				postEffect.Effect(returnDamageInfo, target, source, !_isEventBased);
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
			//Debug.Log($"Base damage: {_baseDamage}. Extra damage: {_extraDamage}. StackEffect: {_stackEffect}");
		}

		public void ResetState() => _extraDamage = 0;

		public IEffect ShallowClone() => new DamageEffect(_baseDamage, _stackEffect, _targeting, _isEventBased, _metaEffects, _postEffects);

		object IShallowClone.ShallowClone() => ShallowClone();
	}
}