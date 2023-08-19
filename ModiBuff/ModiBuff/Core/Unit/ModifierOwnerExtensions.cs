namespace ModiBuff.Core
{
	public static class ModifierOwnerExtensions
	{
		public static bool TryAddModifier(this IModifierOwner owner, int id, IUnit source)
		{
			return owner.ModifierController.TryAdd(id, owner, source);
		}

		public static bool TryAddModifier(this IModifierOwner owner, ModifierAddReference addReference, IUnit sender)
		{
			return owner.ModifierController.TryAdd(addReference, owner, sender);
		}

		public static void TryCast(this IModifierOwner owner, int modifierId, IModifierOwner target)
		{
			if (owner.ModifierController.CanCastModifier(modifierId))
				target.ModifierController.TryAdd(modifierId, target, owner);
		}

		public static void ApplyAllAttackModifier(this IModifierOwner owner, IModifierOwner target)
		{
			owner.ModifierController.TryApplyAttackNonCheckModifiers(owner.ModifierController.GetApplierAttackModifierIds(), target, owner);
			owner.ModifierController.TryApplyAttackCheckModifiers(owner.ModifierController.GetApplierAttackCheckModifiers(), target, owner);
		}
	}
}