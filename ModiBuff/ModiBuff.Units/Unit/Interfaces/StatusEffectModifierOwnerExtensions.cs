namespace ModiBuff.Core.Units
{
	public static class StatusEffectModifierOwnerExtensions
	{
		public static bool HasStatusEffectSingle(
			this ISingleInstanceStatusEffectOwner<LegalAction, StatusEffectType> owner,
			StatusEffectType statusEffectType)
		{
			return owner.StatusEffectController.HasStatusEffect(statusEffectType);
		}

		public static bool HasStatusEffectMulti(
			this IStatusEffectOwner<LegalAction, StatusEffectType> owner,
			StatusEffectType statusEffectType)
		{
			return owner.StatusEffectController.HasStatusEffect(statusEffectType);
		}

		public static bool HasStatusEffectDurationLess(
			this IDurationLessStatusEffectOwner<LegalAction, StatusEffectType> owner,
			StatusEffectType statusEffectType)
		{
			return owner.StatusEffectController.HasStatusEffect(statusEffectType);
		}
	}
}