using System.Collections.Generic;

namespace ModiBuff.Core.Units
{
	public sealed class CastActionEffect : IEffect
	{
		private readonly int _modifierId;

		public CastActionEffect(string modifierName)
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
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (!(source is IModifierApplierOwner applierSource))
				return;
			if (!(target is IModifierOwner modifierTarget))
				return;

			if (source is ICaster casterSource)
			{
				casterSource.TryCast(_modifierId, modifierTarget);
				return;
			}

			applierSource.TryCast(_modifierId, modifierTarget);
		}
	}
}