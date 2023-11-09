namespace ModiBuff.Core
{
	public static class ModifierOwnerExtensions
	{
		public static void AddModifier(this IModifierOwner owner, int id, IUnit source)
		{
			owner.ModifierController.Add(id, owner, source);
		}

		public static void TryAddModifier(this IUnit owner, ModifierAddReference reference)
		{
			TryAddModifier(owner, reference, owner);
		}

		public static void TryAddModifier(this IUnit owner, ModifierAddReference reference, IUnit target)
		{
			if (reference.IsApplierType)
			{
				if (owner is IModifierApplierOwner modifierApplierOwner)
					modifierApplierOwner.ModifierApplierController.TryAddApplier(reference.Id,
						reference.HasApplyChecks, reference.ApplierType);
				else
					Logger.LogError("[ModiBuff] Tried to add an applier to a unit that is not IModifierApplierOwner");
			}
			else
			{
				if (owner is IModifierOwner modifierOwner)
					modifierOwner.ModifierController.Add(reference.Id, target, owner);
				else
					Logger.LogError("[ModiBuff] Tried to add a modifier to a unit that is not IModifierOwner");
			}
		}

		//TODO Remove
		public static void TryCast(this IModifierApplierOwner owner, int modifierId, IModifierOwner target)
		{
			if (owner.ModifierApplierController.CanCastModifier(modifierId))
				target.ModifierController.Add(modifierId, target, owner);
#if DEBUG
			else
				Logger.Log($"Can't cast modifier id {modifierId} from {owner} to {target}");
#endif
		}

		//TODO Remove?
		public static void TryCastEffect(this IModifierApplierOwner owner, int effectId, IUnit target)
		{
			if (owner.ModifierApplierController.CanCastEffect(effectId))
				target.ApplyEffect(effectId, owner);
#if DEBUG
			else
				Logger.Log($"Can't cast effect id {effectId} from {owner} to {target}");
#endif
		}

		public static void ApplyAllAttackModifier(this IModifierApplierOwner owner, IModifierOwner target)
		{
			target.ModifierController.TryApplyAttackNonCheckModifiers(
				owner.ModifierApplierController.GetApplierAttackModifierIds(), target, owner);
			target.ModifierController.TryApplyAttackCheckModifiers(
				owner.ModifierApplierController.GetApplierAttackCheckModifiers(), target, owner);
		}
	}
}