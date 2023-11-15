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
				_modifierId = ModifierIdManager.GetIdByName(modifierName);
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
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplErrorSource(source, nameof(IModifierApplierOwner));
#endif
				return;
			}

			if (!(target is IModifierOwner modifierTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IModifierOwner));
#endif
				return;
			}

			if (source is ICaster casterSource)
			{
				casterSource.TryCast(_modifierId, modifierTarget);
				return;
			}

			applierSource.TryCast(_modifierId, modifierTarget);
		}
	}
}