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
#if DEBUG
			else
				Logger.Log($"Can't cast {modifierId} from {owner} to {target}");
#endif
		}

		//TODO Remove?
		public static void TryCastEffect(this IModifierOwner owner, int effectId, IModifierOwner target)
		{
			if (owner.ModifierController.CanCastEffect(effectId))
				target.ApplyEffect(effectId, owner);
#if DEBUG
			else
				Logger.Log($"Can't cast {effectId} from {owner} to {target}");
#endif
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