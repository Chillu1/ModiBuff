using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public sealed class HealEffect : IMutableStateEffect, IStackEffect, IRevertEffect, IEffect,
		IStackRevertEffect, IMetaEffectOwner<HealEffect, float, float>, IPostEffectOwner<HealEffect, float>,
		IModifierStateInfo<HealEffect.Data>
	{
		public bool IsRevertible => _effectState != 0;
		public bool UsesMutableState => IsRevertible || _stackEffect.UsesMutableState();

		private readonly float _heal;
		private readonly EffectState _effectState;
		private readonly StackEffectType _stackEffect;
		private readonly float _stackValue;
		private readonly Targeting _targeting;
		private IMetaEffect<float, float>[] _metaEffects;
		private IPostEffect<float>[] _postEffects;

		private float _extraHeal;
		private float _totalHeal;

		public HealEffect(float heal, EffectState effectState = EffectState.None,
			StackEffectType stack = StackEffectType.Effect, float stackValue = -1,
			Targeting targeting = Targeting.TargetSource)
			: this(heal, effectState, stack, stackValue, targeting, null, null)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static HealEffect Create(float heal, EffectState effectState = EffectState.None,
			StackEffectType stack = StackEffectType.Effect, float stackValue = -1,
			Targeting targeting = Targeting.TargetSource, IMetaEffect<float, float>[] metaEffects = null,
			IPostEffect<float>[] postEffects = null) =>
			new HealEffect(heal, effectState, stack, stackValue, targeting, metaEffects, postEffects);

		private HealEffect(float heal, EffectState effectState, StackEffectType stack, float stackValue,
			Targeting targeting, IMetaEffect<float, float>[] metaEffects, IPostEffect<float>[] postEffects)
		{
			_heal = heal;
			_effectState = effectState;
			_stackEffect = stack;
			_stackValue = stackValue;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_postEffects = postEffects;
		}

		public HealEffect SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
		}

		public HealEffect SetPostEffects(params IPostEffect<float>[] postEffects)
		{
			_postEffects = postEffects;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			float heal = _heal;

			if (_metaEffects != null)
				foreach (var metaEffect in _metaEffects)
					heal = metaEffect.Effect(heal, target, source);

			heal += _extraHeal;

			float returnHeal = Effect(heal, target, source);

			if (IsRevertible)
				_totalHeal += returnHeal; //_heal + _extraHeal;

			if (_postEffects != null)
				foreach (var postEffect in _postEffects)
					postEffect.Effect(returnHeal, target, source);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			Effect(-_totalHeal, target, source);
			_totalHeal = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			return ((IHealable<float, float>)target).Heal(value, source);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Set) != 0)
				_extraHeal = _stackValue;

			if ((_stackEffect & StackEffectType.SetStacksBased) != 0)
				_extraHeal = _stackValue * stacks;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraHeal += _stackValue;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraHeal += _stackValue * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void RevertStack(int stacks, IUnit target, IUnit source)
		{
			if (_effectState != EffectState.ValueIsRevertible && (_stackEffect & StackEffectType.Effect) != 0)
			{
				_totalHeal -= _heal + _extraHeal;

				_targeting.UpdateTarget(ref target, source);
				//TODO Do we want a custom negative heal method?
				((IHealable<float, float>)target).Heal(-_heal - _extraHeal, source);
			}

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraHeal -= _stackValue * stacks;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraHeal -= _stackValue;

			if ((_stackEffect & StackEffectType.SetStacksBased) != 0)
				_extraHeal = 0;

			if ((_stackEffect & StackEffectType.Set) != 0)
				_extraHeal = 0;
		}

		public Data GetEffectData() => new Data(_heal, _extraHeal);

		public void ResetState()
		{
			_extraHeal = 0;
			_totalHeal = 0;
		}

		public IEffect ShallowClone() =>
			new HealEffect(_heal, _effectState, _stackEffect, _stackValue, _targeting, _metaEffects, _postEffects);

		object IShallowClone.ShallowClone() => ShallowClone();

		public readonly struct Data
		{
			public readonly float BaseHeal;
			public readonly float ExtraHeal;

			public Data(float baseHeal, float extraHeal)
			{
				BaseHeal = baseHeal;
				ExtraHeal = extraHeal;
			}
		}

		public enum EffectState
		{
			None = 0,
			IsRevertible = 1,
			ValueIsRevertible = 2,
		}
	}
}