using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ApplierEffect : IStackEffect, IEffect
	{
		private readonly int _modifierId;
		private readonly ApplierType _applierType;
		private readonly bool _hasApplyChecks;
		private readonly Targeting _targeting;

		//TODO Automatic hasApplyChecks, how?
		public ApplierEffect(string modifierName, ApplierType applierType = ApplierType.None,
			bool hasApplyChecks = false, Targeting targeting = Targeting.TargetSource)
		{
			try
			{
				//Could ask the user to instead supply the id, but that isn't ideal
				_modifierId = ModifierIdManager.GetIdOld(modifierName);
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
					((IModifierOwner)target).ModifierController.TryAddApplier(_modifierId, _hasApplyChecks,
						_applierType);
					break;
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}

			((IModifierOwner)target).ModifierController.Add(_modifierId, target, source);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(target, source);
		}
	}
}