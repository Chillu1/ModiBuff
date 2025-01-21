using System;

namespace ModiBuff.Core.Units
{
	public sealed class HealEffect : IMutableStateEffect, IStackEffect, IRevertEffect, IEffect,
		ICallbackEffect, IConditionEffect, IStackRevertEffect, IMetaEffectOwner<HealEffect, float, float>,
		IPostEffectOwner<HealEffect, float>, IEffectStateInfo<HealEffect.Data>, ISavableEffect<HealEffect.SaveData>,
		ISaveableRecipeEffect<DamageEffect.RecipeSaveData>
	{
		public bool IsRevertible => _effectState != 0;
		public bool IsStackRevertible => _effectState.HasFlag(EffectState.ValueIsRevertible);
		public bool UsesMutableState => IsRevertible || _stackEffect.UsesMutableState();
		public bool UsesMutableStackEffect => _stackEffect.UsesMutableState();
		public Condition[] Conditions { get; set; }

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
			: this(heal, effectState, stack, stackValue, targeting, null, null, null)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static HealEffect Create(float heal, EffectState effectState = EffectState.None,
			StackEffectType stack = StackEffectType.Effect, float stackValue = -1,
			Targeting targeting = Targeting.TargetSource, IMetaEffect<float, float>[] metaEffects = null,
			IPostEffect<float>[] postEffects = null, Condition[] conditions = null) =>
			new HealEffect(heal, effectState, stack, stackValue, targeting, metaEffects, postEffects, conditions);

		private HealEffect(float heal, EffectState effectState, StackEffectType stack, float stackValue,
			Targeting targeting, IMetaEffect<float, float>[] metaEffects, IPostEffect<float>[] postEffects,
			Condition[] conditions)
		{
			_heal = heal;
			_effectState = effectState;
			_stackEffect = stack;
			_stackValue = stackValue;
			_targeting = targeting;
			_metaEffects = metaEffects;
			_postEffects = postEffects;

			Conditions = conditions ?? Array.Empty<Condition>();
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
			if (!this.Check(_heal, target, source))
				return;

			float returnHeal = 0;

			_targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
			if (effectTarget is IHealable<float, float> healableTarget)
			{
				float heal = _metaEffects.TryApply(_heal, target, source) + _extraHeal;

				//TODO Design choice, do we want to revert overheal? Or only applied heals?

				returnHeal = healableTarget.Heal(heal, effectSource);

				if (IsRevertible)
					_totalHeal += returnHeal;
			}
#if MODIBUFF_EFFECT_CHECK
			else
				EffectHelper.LogImplError(effectTarget, nameof(IHealable<float, float>));
#endif

			_postEffects.TryEffect(returnHeal, target, source);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			if (target is not IHealable<float, float> healableTarget)
				return;

			healableTarget.Heal(-_totalHeal, source);
			_totalHeal = 0;
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

		//TODO Should callback effects use stack logic?
		public void CallbackEffect(IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Set) != 0)
				_extraHeal = _stackValue;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraHeal += _stackValue;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void RevertStack(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Effect) != 0 && _effectState == EffectState.IsRevertible)
			{
				_targeting.UpdateTargetSource(ref target, ref source);
				//TODO Do we want a custom negative heal method?
				if (target is IHealable<float, float> healableTarget)
				{
					_totalHeal -= _heal + _extraHeal;
					healableTarget.Heal(-_heal - _extraHeal, source);
				}
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

		public IEffect ShallowClone() => new HealEffect(_heal, _effectState, _stackEffect, _stackValue, _targeting,
			_metaEffects, _postEffects, Conditions);

		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_extraHeal, _totalHeal);

		public void LoadState(object data)
		{
			var saveData = (SaveData)data;
			_extraHeal = saveData.ExtraHeal;
			_totalHeal = saveData.TotalHeal;
		}

		public object SaveRecipeState() => new RecipeSaveData(_heal, _effectState, _stackEffect, _stackValue,
			_targeting, this.GetMetaSaveData(_metaEffects), this.GetPostSaveData(_postEffects));

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

		public readonly struct SaveData
		{
			public readonly float ExtraHeal;
			public readonly float TotalHeal;

			public SaveData(float extraHeal, float totalHeal)
			{
				ExtraHeal = extraHeal;
				TotalHeal = totalHeal;
			}
		}

		public readonly struct RecipeSaveData
		{
			public readonly float BaseHeal;
			public readonly EffectState EffectState;
			public readonly StackEffectType StackEffect;
			public readonly float StackValue;
			public readonly Targeting Targeting;
			public readonly object[] MetaEffects;
			public readonly object[] PostEffects;

			public RecipeSaveData(float baseHeal, EffectState effectState, StackEffectType stackEffect,
				float stackValue, Targeting targeting, object[] metaEffects, object[] postEffects)
			{
				BaseHeal = baseHeal;
				EffectState = effectState;
				StackEffect = stackEffect;
				StackValue = stackValue;
				Targeting = targeting;
				MetaEffects = metaEffects;
				PostEffects = postEffects;
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