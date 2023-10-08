namespace ModiBuff.Core.Units
{
	public static class StatusEffectModifierOwnerExtensions
	{
		public static bool TryCast(this IStatusEffectModifierOwnerLegalTarget<LegalAction, StatusEffectType> owner,
			int modifierId, IModifierOwner target)
		{
			if (!modifierId.IsLegalTarget(((IUnitEntity)target).UnitType, owner.UnitType))
				return false;

			if (!owner.StatusEffectController.HasLegalAction(LegalAction.Cast))
				return false;

			if (!owner.ModifierController.CanCastModifier(modifierId))
				return false;

			target.ModifierController.Add(modifierId, target, owner);
			return true;
		}

		/// <summary>
		///		Skips the check part for check modifiers, use this ONLY in case you're also using <see cref="ModifierController.TryCastCheck"/>
		/// </summary>
		public static bool TryCastNoChecks(
			this IStatusEffectModifierOwnerLegalTarget<LegalAction, StatusEffectType> owner, int modifierId,
			IModifierOwner target)
		{
			if (!modifierId.IsLegalTarget(((IUnitEntity)target).UnitType, owner.UnitType))
				return false;

			if (!owner.StatusEffectController.HasLegalAction(LegalAction.Cast))
				return false;

			if (!owner.ModifierController.ContainsApplier(modifierId))
				return false;

			target.ModifierController.Add(modifierId, target, owner);
			return true;
		}
	}
}