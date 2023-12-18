using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ApplierEffect : IStackEffect, IEffect, IMetaEffectOwner<ApplierEffect, int, int>
	{
		public bool HasApplierType => _applierType != ApplierType.None;

		private readonly int _modifierId;
		private readonly ApplierType _applierType;
		private readonly bool _hasApplyChecks;
		private readonly Targeting _targeting;
		private readonly ModifierAddData _modifierAddData;
		private IMetaEffect<int, int>[] _metaEffects;

		public ApplierEffect(string modifierName, ApplierType applierType = ApplierType.None,
			bool hasApplyChecks = false, Targeting targeting = Targeting.TargetSource,
			ModifierAddData addData = default)
		{
			try
			{
				//Could ask the user to instead supply the id, but that isn't ideal
				_modifierId = ModifierIdManager.GetIdByName(modifierName);
			}
			catch (KeyNotFoundException)
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError("[ModiBuff] Can't find modifier with name " + modifierName +
				                ". Either wrong order of effect initialization or wrong modifier name.");
#endif
			}

			_applierType = applierType;
			_hasApplyChecks = hasApplyChecks;
			_targeting = targeting;
			_modifierAddData = addData;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static ApplierEffect Create(int modifierId, ApplierType applierType = ApplierType.None,
			bool hasApplyChecks = false, Targeting targeting = Targeting.TargetSource) =>
			new ApplierEffect(modifierId, applierType, hasApplyChecks, targeting);

		private ApplierEffect(int modifierId, ApplierType applierType, bool hasApplyChecks, Targeting targeting)
		{
			_modifierId = modifierId;
			_applierType = applierType;
			_hasApplyChecks = hasApplyChecks;
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
				case ApplierType.None:
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

					modifierApplierOwnerTarget.ModifierApplierController.TryAddApplier(modifierId, _hasApplyChecks,
						_applierType);
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

			if (_modifierAddData.IsValid)
				modifierOwnerTarget.ModifierController.AddWithData(modifierId, _modifierAddData, modifierOwnerTarget,
					effectSource);
			else
				modifierOwnerTarget.ModifierController.Add(modifierId, modifierOwnerTarget, effectSource);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(target, source);
		}
	}
}