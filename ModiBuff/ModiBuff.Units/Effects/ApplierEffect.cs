using System;

namespace ModiBuff.Core.Units
{
	public sealed class ApplierEffect : IStackEffect, IEffect, IMetaEffectOwner<ApplierEffect, int, int>
	{
		private readonly int _modifierId;
		private readonly ApplierType? _applierType;
		private readonly Targeting _targeting;
		private IMetaEffect<int, int>[]? _metaEffects;

		public ApplierEffect(string modifierName, ApplierType? applierType = null,
			Targeting targeting = Targeting.TargetSource)
		{
			//Could ask the user to instead supply the id, but that isn't ideal
			int? id = ModifierIdManager.GetIdByName(modifierName);
			if (id == null)
				Logger.LogError("[ModiBuff.Units] Can't find modifier with name " + modifierName +
				                ". Either wrong order of effect initialization or wrong modifier name.");

			_modifierId = id ?? -1;

			_applierType = applierType;
			_targeting = targeting;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static ApplierEffect Create(int modifierId, ApplierType? applierType = null,
			Targeting targeting = Targeting.TargetSource) =>
			new ApplierEffect(modifierId, applierType, targeting);

		private ApplierEffect(int modifierId, ApplierType? applierType, Targeting targeting)
		{
			_modifierId = modifierId;
			_applierType = applierType;
			_targeting = targeting;
		}

		public ApplierEffect SetMetaEffects(params IMetaEffect<int, int>[] metaEffects)
		{
			_metaEffects = metaEffects;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
			int modifierId = _modifierId;
			if (_metaEffects != null)
				foreach (var metaEffect in _metaEffects)
					modifierId = metaEffect.Effect(modifierId, target, source);

			switch (_applierType)
			{
				case null:
					break;
				case ApplierType.Cast:
				case ApplierType.Attack:
					if (!(effectTarget is IModifierApplierOwner modifierApplierOwnerTarget))
					{
#if MODIBUFF_EFFECT_CHECK
						EffectHelper.LogImplError(effectTarget, nameof(IModifierApplierOwner));
#endif
						return;
					}

					modifierApplierOwnerTarget.AddApplierModifierNew(modifierId, _applierType.Value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (!(effectTarget is IModifierOwner modifierOwnerTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(effectTarget, nameof(IModifierOwner));
#endif
				return;
			}

			modifierOwnerTarget.ModifierController.Add(modifierId, modifierOwnerTarget, effectSource);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(target, source);
		}
	}
}