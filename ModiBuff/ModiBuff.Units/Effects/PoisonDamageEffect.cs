using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public sealed class PoisonDamageEffect : IStackEffect, IMutableStateEffect, IEffect,
		IMetaEffectOwner<PoisonDamageEffect, float, int, float>, IPostEffectOwner<PoisonDamageEffect, float, int>
	{
		public bool UsesMutableState => _stackEffect.UsesMutableState();

		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;
		private readonly float _stackValue;
		private readonly Targeting _targeting;
		private IMetaEffect<float, int, float>[] _metaEffects;
		private IPostEffect<float, int>[] _postEffects;

		private float _extraDamage;
		private int _poisonStacks;

		public PoisonDamageEffect(float damage, StackEffectType stackEffect = StackEffectType.Effect,
			float stackValue = -1,
			Targeting targeting = Targeting.TargetSource)
			: this(damage, stackEffect, stackValue, targeting, null, null)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static PoisonDamageEffect Create(float damage, StackEffectType stackEffect = StackEffectType.Effect,
			float stackValue = -1, Targeting targeting = Targeting.TargetSource,
			IMetaEffect<float, int, float>[] metaEffects = null, IPostEffect<float, int>[] postEffects = null) =>
			new PoisonDamageEffect(damage, stackEffect, stackValue, targeting, metaEffects, postEffects);

		private PoisonDamageEffect(float damage, StackEffectType stackEffect, float stackValue, Targeting targeting,
			IMetaEffect<float, int, float>[] metaEffects, IPostEffect<float, int>[] postEffects)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
			_stackValue = stackValue;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_postEffects = postEffects;
		}

		public PoisonDamageEffect SetMetaEffects(params IMetaEffect<float, int, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
		}

		public PoisonDamageEffect SetPostEffects(params IPostEffect<float, int>[] postEffects)
		{
			_postEffects = postEffects;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			float damage = _baseDamage;

			if (_metaEffects != null)
				foreach (var metaEffect in _metaEffects)
					damage = metaEffect.Effect(damage, _poisonStacks, target, source);

			damage += _extraDamage;

			float returnDamageInfo = Effect(damage, target, source);

			if (_postEffects != null)
				foreach (var postEffect in _postEffects)
					postEffect.Effect(returnDamageInfo, _poisonStacks, target, source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float Effect(float damage, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			return ((IPoisonable)target).TakeDamagePoison(damage, _poisonStacks, source);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			_poisonStacks++;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += _stackValue;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += _stackValue * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void ResetState()
		{
			_extraDamage = 0;
			_poisonStacks = 0;
		}

		public IEffect ShallowClone() =>
			new PoisonDamageEffect(_baseDamage, _stackEffect, _stackValue, _targeting, _metaEffects, _postEffects);

		object IShallowClone.ShallowClone() => ShallowClone();
	}
}