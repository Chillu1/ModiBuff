namespace ModiBuff.Core
{
	public static class ModifierOwnerExtensions
	{
		public static void AddModifier(this IModifierOwner owner, int id, IUnit source)
		{
			owner.ModifierController.Add(id, owner, source);
		}

		//TODO Remove
		public static void TryCast(this IModifierOwner owner, int modifierId, IModifierOwner target)
		{
			if (owner.ModifierController.CanCastModifier(modifierId))
				target.ModifierController.Add(modifierId, target, owner);
		}

		public static void ApplyAllAttackModifier(this IModifierOwner owner, IModifierOwner target)
		{
			target.ModifierController.TryApplyAttackNonCheckModifiers(
				owner.ModifierController.GetApplierAttackModifierIds(), target, owner);
			target.ModifierController.TryApplyAttackCheckModifiers(
				owner.ModifierController.GetApplierAttackCheckModifiers(), target, owner);
		}
	}
}