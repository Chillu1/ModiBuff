using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public sealed class DamageEffect : IStackEffect, IMutableStateEffect, IEffect, ICallbackEffect, IConditionEffect,
		IStackRevertEffect, ISetDataEffect, IMetaEffectOwner<DamageEffect, float, float>,
		IPostEffectOwner<DamageEffect, float>, IEffectStateInfo<DamageEffect.Data>,
		ISavableEffect<DamageEffect.SaveData>, ISaveableRecipeEffect<DamageEffect.RecipeSaveData>
	{
		public bool IsStackRevertible => _valueIsRevertible;
		public bool UsesMutableState => _stackEffect.UsesMutableState();
		public bool UsesMutableStackEffect => _stackEffect.UsesMutableState();
		public Condition[] Conditions { get; set; }

		private readonly float _baseDamage;
		private readonly bool _valueIsRevertible;
		private readonly StackEffectType _stackEffect;
		private readonly float? _stackValue;
		private readonly Targeting _targeting;
		private IMetaEffect<float, float>[] _metaEffects;
		private IPostEffect<float>[] _postEffects;

		private float _extraDamage;

		public DamageEffect(float damage, bool valueIsRevertible = false,
			StackEffectType stackEffect = StackEffectType.Effect, float? stackValue = null,
			Targeting targeting = Targeting.TargetSource)
			: this(damage, valueIsRevertible, stackEffect, stackValue, targeting, null, null, null)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static DamageEffect Create(float damage, bool valueIsRevertible = false,
			StackEffectType stackEffect = StackEffectType.Effect, float? stackValue = null,
			Targeting targeting = Targeting.TargetSource, IMetaEffect<float, float>[] metaEffects = null,
			IPostEffect<float>[] postEffects = null, ICondition[] conditions = null) =>
			new DamageEffect(damage, valueIsRevertible, stackEffect, stackValue, targeting, metaEffects, postEffects,
				conditions);

		private DamageEffect(float damage, bool valueIsRevertible, StackEffectType stackEffect, float? stackValue,
			Targeting targeting, IMetaEffect<float, float>[] metaEffects, IPostEffect<float>[] postEffects,
			ICondition[] conditions)
		{
			_baseDamage = damage;
			_valueIsRevertible = valueIsRevertible;
			_stackEffect = stackEffect;
			_stackValue = stackValue;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_postEffects = postEffects;
			Conditions = conditions?.Cast<Condition>().ToArray() ?? Array.Empty<Condition>();
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
			//TODO Moved outside the effects? Less code, more type checking
			if (!this.Check(_baseDamage, target, source))
				return;

			_targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
			float returnDamageInfo = 0;

			//if(effectTarget is IDamagable)
			if (effectTarget is IAttackable<float, float> damagableTarget)
			{
				float damage = _metaEffects.TryApply(_baseDamage, target, source) + _extraDamage;

				returnDamageInfo = damagableTarget.TakeDamage(damage, effectSource);
			}
#if MODIBUFF_EFFECT_CHECK
			else
				EffectHelper.LogImplError(effectTarget, nameof(IAttackable<float, float>));
#endif

			_postEffects.TryEffect(returnDamageInfo, target, source);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += _stackValue!.Value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += _stackValue!.Value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		//TODO Should callback effects use stack logic?
		public void CallbackEffect(IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Set) != 0)
				_extraDamage = _stackValue!.Value;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += _stackValue!.Value;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void SetData(IData data)
		{
			switch (data)
			{
				case EffectData<int> effectData:
					_extraDamage = effectData.Value;
					break;
				case EffectData<float> effectData:
					_extraDamage = effectData.Value;
					break;
				case ModifierData:
					break;
				default:
					Logger.LogError($"Unsupported data type: {data.GetType()}");
					break;
			}
		}

		public void RevertStack(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage -= _stackValue!.Value * stacks;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage -= _stackValue!.Value;

			if ((_stackEffect & StackEffectType.SetStacksBased) != 0)
				_extraDamage = 0;

			if ((_stackEffect & StackEffectType.Set) != 0)
				_extraDamage = 0;
		}

		public Data GetEffectData() => new Data(_baseDamage, _extraDamage);
		object IEffectStateInfo.GetEffectData() => GetEffectData();

		public void ResetState() => _extraDamage = 0;

		public IEffect ShallowClone() => new DamageEffect(_baseDamage, _valueIsRevertible, _stackEffect, _stackValue,
			_targeting, _metaEffects, _postEffects, Conditions);

		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_extraDamage);
		public void LoadState(object saveData) => _extraDamage = ((SaveData)saveData).ExtraDamage;

		public object SaveRecipeState() => new RecipeSaveData(_baseDamage, _valueIsRevertible, _stackEffect,
			_stackValue, _targeting, this.GetMetaSaveData(_metaEffects), this.GetPostSaveData(_postEffects),
			this.GetConditionSaveData(Conditions));

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
			public readonly bool ValueIsRevertible;
			public readonly StackEffectType StackEffect;
			public readonly float? StackValue;
			public readonly Targeting Targeting;
			public readonly object[] MetaEffects;
			public readonly object[] PostEffects;
			public readonly object[] Conditions;

			public RecipeSaveData(float baseDamage, bool valueIsRevertible, StackEffectType stackEffect,
				float? stackValue, Targeting targeting, object[] metaEffects, object[] postEffects, object[] conditions)
			{
				BaseDamage = baseDamage;
				ValueIsRevertible = valueIsRevertible;
				StackEffect = stackEffect;
				StackValue = stackValue;
				Targeting = targeting;
				MetaEffects = metaEffects;
				PostEffects = postEffects;
				Conditions = conditions;
			}
		}
	}
}