using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public sealed class HealEffect : ITargetEffect, IStateEffect, IStackEffect, IRevertEffect, IEffect,
		IMetaEffectOwner<HealEffect, float, float>, IPostEffectOwner<HealEffect, float>
	{
		public bool IsRevertible { get; }

		private readonly float _heal;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;
		private IMetaEffect<float, float>[] _metaEffects;
		private bool _hasMetaEffects;
		private IPostEffect<float>[] _postEffects;
		private bool _hasPostEffects;

		private float _extraHeal;
		private float _totalHeal;

		public HealEffect(float heal, bool revertible = false, StackEffectType stack = StackEffectType.Effect) :
			this(heal, revertible, stack, Targeting.TargetSource, null, null)
		{
		}

		private HealEffect(float heal, bool revertible, StackEffectType stack, Targeting targeting,
			IMetaEffect<float, float>[] metaEffects, IPostEffect<float>[] postEffects)
		{
			_heal = heal;
			IsRevertible = revertible;
			_stackEffect = stack;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_hasMetaEffects = metaEffects != null;
			_postEffects = postEffects;
			_hasPostEffects = postEffects != null;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public HealEffect SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			_hasMetaEffects = true;
			return this;
		}

		public HealEffect SetPostEffects(params IPostEffect<float>[] postEffects)
		{
			_postEffects = postEffects;
			_hasPostEffects = true;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (IsRevertible)
				_totalHeal = _heal + _extraHeal;

			float heal = _heal;

			if (_hasMetaEffects)
				foreach (var metaEffect in _metaEffects)
					heal = metaEffect.Effect(heal, target, source);

			heal += _extraHeal;

			float returnHeal = Effect(heal, target, source);

			if (_hasPostEffects)
				foreach (var postEffect in _postEffects)
					postEffect.Effect(returnHeal, target, source);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			Effect(-_totalHeal, target, source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			return ((IHealable<float, float>)target).Heal(value, source);
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_totalHeal += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_totalHeal += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void ResetState()
		{
			_extraHeal = 0;
			_totalHeal = 0;
		}

		public IEffect ShallowClone() => new HealEffect(_heal, IsRevertible, _stackEffect, _targeting, _metaEffects, _postEffects);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}