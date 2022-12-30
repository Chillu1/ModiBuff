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
				//TODO Last registered effect instead of name? Won't lead to any problems?
				_modifierId = ModifierIdManager.GetId(modifierName);
			}
			catch (KeyNotFoundException)
			{
				Debug.LogError("Can't find modifier with name " + modifierName +
				               ". Either wrong order of effect initialization or wrong modifier name.");
			}
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public void Effect(IUnit target, IUnit acter)
		{
			switch (_targeting)
			{
				case Targeting.TargetActer:
					target.TryAddModifier(_modifierId, acter);
					break;
				case Targeting.ActerTarget:
					acter.TryAddModifier(_modifierId, target);
					break;
				case Targeting.TargetTarget:
					target.TryAddModifier(_modifierId, target);
					break;
				case Targeting.ActerActer:
					acter.TryAddModifier(_modifierId, acter);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(targetComponent.Target, targetComponent.Acter);
		}
	}
}