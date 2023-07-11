using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class ApplierEffect : ITargetEffect, IEffect, IStackEffect
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
				Debug.LogError("Can't find modifier with name " + modifierName +
				               ". Either wrong order of effect initialization or wrong modifier name.");
			}
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public void Effect(IUnit target, IUnit source)
		{
			switch (_targeting)
			{
				case Targeting.TargetSource:
					((IModifierOwner)target).TryAddModifier(_modifierId, source);
					break;
				case Targeting.SourceTarget:
					((IModifierOwner)source).TryAddModifier(_modifierId, target);
					break;
				case Targeting.TargetTarget:
					((IModifierOwner)target).TryAddModifier(_modifierId, target);
					break;
				case Targeting.SourceSource:
					((IModifierOwner)source).TryAddModifier(_modifierId, source);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(target, source);
		}
	}
}