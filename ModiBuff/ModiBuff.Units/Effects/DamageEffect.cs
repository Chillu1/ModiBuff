#if NET5_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP1_1_OR_GREATER
#define UNSAFE
#endif

using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public sealed class DamageEffect : ITargetEffect, IStackEffect, IStateEffect, IEffect,
		IMetaEffectOwner<DamageEffect, float, float>, IPostEffectOwner<DamageEffect, float>, IEquatable<DamageEffect>
	{
		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;
		private IMetaEffect<float, float>[] _metaEffects;
		private bool _hasMetaEffects;
		private IPostEffect<float>[] _postEffects;
		private bool _hasPostEffects;
		private static int _effectId = 0, _staticId = 0, _staticGenId;
		private readonly int _id, _genId;
		private readonly int _hash;

		private float _extraDamage;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.Effect)
			: this(damage, stackEffect, Targeting.TargetSource, null, null)
		{
		}

		private DamageEffect(float damage, StackEffectType stackEffect, Targeting targeting, IMetaEffect<float, float>[] metaEffects,
			IPostEffect<float>[] postEffects)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_hasMetaEffects = metaEffects != null;
			_postEffects = postEffects;
			_hasPostEffects = postEffects != null;

			_id = _staticId;
			_genId = _staticGenId++;
			int cantorOne = (_id + _genId) * (_id + _genId + 1) / 2 + _genId;
			_hash = (cantorOne + _effectId) * (cantorOne + _effectId + 1) / 2 + _effectId;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public void UpdateHash(int id, int genId)
		{
			unchecked
			{
				//int cantorOne = (id + genId) * (id + genId + 1) / 2 + genId;
				//_hash = (cantorOne + _effectId) * (cantorOne + _effectId + 1) / 2 + _effectId;
			}
		}

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
				Unsafe.As<IDamagable<float, float, float, float>>(target).TakeDamage(damage, source, _hash);
#else
				((IDamagable<float, float, float, float>)target).TakeDamage(damage, source, _hash);
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
			//Debug.Log($"Base damage: {_baseDamage}. Extra damage: {_extraDamage}. StackEffect: {_stackEffect}");
		}

		public void ResetState() => _extraDamage = 0;

		public IEffect ShallowClone() => new DamageEffect(_baseDamage, _stackEffect, _targeting, _metaEffects, _postEffects);

		object IShallowClone.ShallowClone() => ShallowClone();

		public static void Reset()
		{
			_staticId = _staticGenId = _effectId = 0;
		}

		public bool Equals(DamageEffect other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _hash == other._hash;
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is DamageEffect other && Equals(other);
		}

		public override int GetHashCode() => _hash;
	}
}