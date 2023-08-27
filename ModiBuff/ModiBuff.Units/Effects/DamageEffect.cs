using System;

namespace ModiBuff.Core.Units
{
	public sealed class DamageEffect : IMetaEffectOwner<float>, ITargetEffect, IEventTrigger, IStackEffect, IStateEffect, IEffect
	{
		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;
		private bool _isEventBased;
		private IMetaEffect<float>[] _metaEffects;
		private bool _hasMetaEffects;

		private float _extraDamage;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.Effect) :
			this(damage, stackEffect, Targeting.TargetSource, false, null)
		{
		}

		private DamageEffect(float damage, StackEffectType stackEffect, Targeting targeting, bool isEventBased,
			IMetaEffect<float>[] metaEffects)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
			_targeting = targeting;
			_isEventBased = isEventBased;
			_metaEffects = metaEffects;
			_hasMetaEffects = metaEffects != null;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;
		public void SetEventBased() => _isEventBased = true;

		public IEffect SetMetaEffects(params IMetaEffect<float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			_hasMetaEffects = true;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (!(target is IDamagable<float, float, float, float>))
				throw new ArgumentException("Target must implement IDamagable");
			if (!(source is IDamagable<float, float, float, float>) &&
			    (_targeting == Targeting.SourceTarget || _targeting == Targeting.SourceSource))
				throw new ArgumentException("Source must implement IDamagable when targeting source");
#endif
			_targeting.UpdateTargetSource(ref target, ref source);

			float damageInfo =
				((IDamagable<float, float, float, float>)target).TakeDamage(_baseDamage + _extraDamage, source, !_isEventBased);

			if (!_hasMetaEffects)
				return;

			foreach (var metaEffect in _metaEffects)
				metaEffect.Effect(damageInfo, target, source, !_isEventBased);
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

		public IStateEffect ShallowClone() => new DamageEffect(_baseDamage, _stackEffect, _targeting, _isEventBased, _metaEffects);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}