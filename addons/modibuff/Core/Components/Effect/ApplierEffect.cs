using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ApplierEffect : ITargetEffect, IStackEffect, IEffect
	{
		private readonly int _modifierId;
		private Targeting _targeting;

		public ApplierEffect(string modifierName)
		{
			try
			{
				//Could ask the user to instead supply the id, but that isn't ideal
				_modifierId = ModifierIdManager.GetIdOld(modifierName);
			}
			catch (KeyNotFoundException)
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError("Can't find modifier with name " + modifierName +
				                ". Either wrong order of effect initialization or wrong modifier name.");
#endif
			}
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			((IModifierOwner)target).AddModifier(_modifierId, source);
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(target, source);
		}
	}
}