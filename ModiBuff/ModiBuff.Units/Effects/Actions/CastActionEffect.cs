namespace ModiBuff.Core.Units
{
	public sealed class CastActionEffect : IEffect
	{
		private readonly int _modifierId;

		public CastActionEffect(string modifierName)
		{
			//Could ask the user to instead supply the id, but that isn't ideal
			int? id = ModifierIdManager.GetIdByName(modifierName);
			if (id == null)
			{
				Logger.LogError("[ModiBuff.Units] Can't find modifier with name " + modifierName +
				                ". Either wrong order of effect initialization or wrong modifier name.");
			}

			_modifierId = id ?? -1;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (source is not IModifierApplierOwner applierSource)
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplErrorSource(source, nameof(IModifierApplierOwner));
#endif
				return;
			}

			if (target is not IModifierOwner modifierTarget)
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

			applierSource.TryApply(_modifierId, modifierTarget);
		}
	}
}