#if NET5_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_1_OR_GREATER
#define UNSAFE
using System.Runtime.CompilerServices;
#endif

using System;

namespace ModiBuff.Core.Units
{
	public sealed class DamageEffect : ITargetEffect, IStackEffect, IStateEffect, IEffect,
		IMetaEffectOwner<DamageEffect, float, float>, IPostEffectOwner<DamageEffect, float>,
		IModifierStateInfo<DamageEffect.Data>
	{
		public bool UsesMutableState { get; }

		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;
		private IMetaEffect<float, float>[] _metaEffects;
		private bool _hasMetaEffects;
		private IPostEffect<float>[] _postEffects;
		private bool _hasPostEffects;

		private float _extraDamage;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.Effect)
			: this(damage, stackEffect, Targeting.TargetSource, null, null, stackEffect.UsesMutableState())
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static DamageEffect Create(float damage, StackEffectType stackEffect = StackEffectType.Effect,
			Targeting targeting = Targeting.TargetSource, IMetaEffect<float, float>[] metaEffects = null,
			IPostEffect<float>[] postEffects = null) =>
			new DamageEffect(damage, stackEffect, targeting, metaEffects, postEffects, stackEffect.UsesMutableState());

		private DamageEffect(float damage, StackEffectType stackEffect, Targeting targeting,
			IMetaEffect<float, float>[] metaEffects, IPostEffect<float>[] postEffects, bool usesMutableState)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_hasMetaEffects = metaEffects != null;
			_postEffects = postEffects;
			_hasPostEffects = postEffects != null;
			UsesMutableState = usesMutableState;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

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
				Unsafe.As<IDamagable<float, float, float, float>>(target).TakeDamage(damage, source);
#else
				((IDamagable<float, float, float, float>)target).TakeDamage(damage, source);
#endif
			if (!_hasPostEffects)
				return;

			foreach (var postEffect in _postEffects)
				postEffect.Effect(returnDamageInfo, target, source);
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public Data GetEffectData() => new Data(_baseDamage, _extraDamage);

		public void ResetState() => _extraDamage = 0;

		public IEffect ShallowClone() =>
			new DamageEffect(_baseDamage, _stackEffect, _targeting, _metaEffects, _postEffects, UsesMutableState);

		object IShallowClone.ShallowClone() => ShallowClone();

		public readonly struct Data
		{
			public readonly float BaseDamage;
			public readonly float ExtraDamage;

			public Data(float baseDamage, float extraDamage)
			{
				BaseDamage = baseDamage;
				ExtraDamage = extraDamage;
			}
		}
	}
}