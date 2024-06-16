namespace ModiBuff.Core.Units
{
	public sealed class DamageEffect : IStackEffect, IMutableStateEffect, IEffect,
		IMetaEffectOwner<DamageEffect, float, float>, IPostEffectOwner<DamageEffect, float>,
		IEffectStateInfo<DamageEffect.Data>, ISavableEffect<DamageEffect.SaveData>,
		ISaveableRecipeEffect<DamageEffect.RecipeSaveData>
	{
		public bool UsesMutableState => _stackEffect.UsesMutableState();
		public bool UsesMutableStackEffect => _stackEffect.UsesMutableState();

		private readonly float _baseDamage;
		private readonly StackEffectType _stackEffect;
		private readonly float _stackValue;
		private readonly Targeting _targeting;
		private IMetaEffect<float, float>[] _metaEffects;
		private IPostEffect<float>[] _postEffects;

		private float _extraDamage;

		public DamageEffect(float damage, StackEffectType stackEffect = StackEffectType.Effect, float stackValue = -1,
			Targeting targeting = Targeting.TargetSource)
			: this(damage, stackEffect, stackValue, targeting, null, null)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static DamageEffect Create(float damage, StackEffectType stackEffect = StackEffectType.Effect,
			float stackValue = -1, Targeting targeting = Targeting.TargetSource,
			IMetaEffect<float, float>[] metaEffects = null, IPostEffect<float>[] postEffects = null) =>
			new DamageEffect(damage, stackEffect, stackValue, targeting, metaEffects, postEffects);

		private DamageEffect(float damage, StackEffectType stackEffect, float stackValue, Targeting targeting,
			IMetaEffect<float, float>[] metaEffects, IPostEffect<float>[] postEffects)
		{
			_baseDamage = damage;
			_stackEffect = stackEffect;
			_stackValue = stackValue;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_postEffects = postEffects;
		}

		public DamageEffect SetMetaEffects(params IMetaEffect<float, float>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
		}

		public DamageEffect SetPostEffects(params IPostEffect<float>[] postEffects)
		{
			_postEffects = postEffects;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
			float returnDamageInfo = 0;

			//if(effectTarget is IDamagable)
			if (effectTarget is IAttackable<float, float> damagableTarget)
			{
				float damage = _baseDamage;

				if (_metaEffects != null)
					foreach (var metaEffect in _metaEffects)
						damage = metaEffect.Effect(damage, target, source);

				damage += _extraDamage;

				returnDamageInfo = damagableTarget.TakeDamage(damage, effectSource);
			}
#if MODIBUFF_EFFECT_CHECK
			else
				EffectHelper.LogImplError(effectTarget, nameof(IAttackable<float, float>));
#endif

			if (_postEffects != null)
				foreach (var postEffect in _postEffects)
					postEffect.Effect(returnDamageInfo, target, source);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += _stackValue;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += _stackValue * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public Data GetEffectData() => new Data(_baseDamage, _extraDamage);

		public void ResetState() => _extraDamage = 0;

		public IEffect ShallowClone() =>
			new DamageEffect(_baseDamage, _stackEffect, _stackValue, _targeting, _metaEffects, _postEffects);

		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_extraDamage);
		public void LoadState(object saveData) => _extraDamage = ((SaveData)saveData).ExtraDamage;

		public object SaveRecipeState() => new RecipeSaveData(_baseDamage);

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

		public readonly struct SaveData
		{
			public readonly float ExtraDamage;

			public SaveData(float extraDamage) => ExtraDamage = extraDamage;
		}

		public readonly struct RecipeSaveData
		{
			public readonly float BaseDamage;

			public RecipeSaveData(float baseDamage) => BaseDamage = baseDamage;
		}
	}
}