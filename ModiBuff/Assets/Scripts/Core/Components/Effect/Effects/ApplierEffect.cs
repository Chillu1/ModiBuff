using System.Collections.Generic;
using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class ApplierEffect : IEffect, IStackEffect
	{
		private readonly int _modifierId;

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

		public void Effect(IUnit target, IUnit acter)
		{
			target.TryAddModifier(_modifierId, acter);
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			//Applier effect can't have different ways of using stacks/value
			Effect(targetComponent.Target, targetComponent.Acter);
		}
	}
}