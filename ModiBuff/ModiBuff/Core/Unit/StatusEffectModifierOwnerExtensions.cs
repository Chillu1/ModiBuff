namespace ModiBuff.Core
{
	public static class StatusEffectModifierOwnerExtensions
	{
		public static void TryCast(this IStatusEffectModifierOwner owner, int modifierId, IModifierOwner target)
		{
			if (!owner.StatusEffectController.HasLegalAction(LegalAction.Act))
				return;

			if (owner.ModifierController.CanCastModifier(modifierId))
				target.ModifierController.TryAdd(modifierId, target, owner);
		}

		internal static void TryCastAll(this IStatusEffectModifierOwner owner, IModifierOwner target)
		{
			if (!owner.StatusEffectController.HasLegalAction(LegalAction.Cast))
				return;

			target.ModifierController.TryApplyCastModifiers(target, owner);
		}
	}
}