namespace ModiBuff.Core.Units
{
	public static class StatusEffectModifierOwnerExtensions
	{
		public static void TryCast(this IStatusEffectModifierOwner owner, int modifierId, IModifierOwner target)
		{
			if (!owner.StatusEffectController.HasLegalAction(LegalAction.Cast))
				return;

			if (owner.ModifierController.CanCastModifier(modifierId))
				target.ModifierController.TryAdd(modifierId, target, owner);
		}
	}
}