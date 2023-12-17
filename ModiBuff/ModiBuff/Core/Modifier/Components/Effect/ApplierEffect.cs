using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ApplierEffect : IStackEffect, IEffect
	{
		public bool HasApplierType => _applierType != ApplierType.None;

		private readonly int _modifierId;
		private readonly ApplierType _applierType;
		private readonly bool _hasApplyChecks;
		private readonly Targeting _targeting;

		public ApplierEffect(string modifierName, ApplierType applierType = ApplierType.None,
			bool hasApplyChecks = false, Targeting targeting = Targeting.TargetSource)
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

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);

			switch (_applierType)
			{
				case ApplierType.None:
					break;
				case ApplierType.Cast:
				case ApplierType.Attack:
					if (!(target is IModifierApplierOwner modifierApplierOwnerTarget))
					{
#if MODIBUFF_EFFECT_CHECK
						EffectHelper.LogImplError(target, nameof(IModifierApplierOwner));
#endif
						return;
					}

					modifierApplierOwnerTarget.ModifierApplierController.TryAddApplier(_modifierId, _hasApplyChecks,
						_applierType);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (!(target is IModifierOwner modifierOwnerTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IModifierOwner));
#endif
				return;
			}

			modifierOwnerTarget.ModifierController.Add(_modifierId, modifierOwnerTarget, source);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(target, source);
		}
	}
}