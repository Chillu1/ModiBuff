using System.Collections.Generic;
using UnityEngine;

namespace ModiBuff.Core
{
	public sealed class ActerApplierEffect : IEffect
	{
		private readonly int _modifierId;

		public ActerApplierEffect(string modifierName)
		{
			int modifierId = 0;

			try
			{
				modifierId = ModifierIdManager.GetId(modifierName);
			}
			catch (KeyNotFoundException)
			{
				Debug.LogError("Can't find modifier with name " + modifierName +
				               ". Either wrong order of effect initialization or wrong modifier name.");
			}

			_modifierId = modifierId;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			acter.TryAddModifier(_modifierId, target);
		}
	}
}