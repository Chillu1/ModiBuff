namespace ModiBuff.Core
{
	public static class ModifierOwnerExtensions
	{
		public static void AddModifier(this IModifierOwner owner, int id, IUnit source)
		{
			owner.ModifierController.Add(id, owner, source);
		}

		public static void Dispel(this IModifierOwner owner, DispelType dispelType, IUnit source)
		{
			owner.ModifierController.Dispel(dispelType, owner, source);
		}

		public static void TryAddModifierReference(this IUnit owner, ModifierAddReference reference)
		{
			TryAddModifierReference(owner, reference, owner);
		}

		public static void TryAddModifierReference(this IUnit owner, ModifierAddReference reference, IUnit target)
		{
			if (reference.IsApplierType)
			{
				if (owner is IModifierApplierOwner modifierApplierOwner)
					modifierApplierOwner.ModifierApplierController.TryAddApplier(reference.Id,
						reference.HasApplyChecks, reference.ApplierType!.Value);
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
			if (owner.CanCastModifier(modifierId))
				target.ModifierController.Add(modifierId, target, owner);
#if DEBUG
			else
				Logger.Log($"[ModiBuff] Can't cast modifier id {modifierId} from {owner} to {target}");
#endif
		}

		//TODO Remove?
		public static void TryCastEffect(this IModifierApplierOwner owner, int effectId, IUnit target)
		{
			if (owner.ModifierApplierController.CanCastEffect(effectId))
				target.ApplyEffect(effectId, owner);
#if DEBUG
			else
				Logger.Log($"[ModiBuff] Can't cast effect id {effectId} from {owner} to {target}");
#endif
		}

		public static void ApplyAllAttackModifier(this IModifierApplierOwner owner, IModifierOwner target)
		{
			owner.ApplyAttackNonCheckModifiers(target);
			owner.TryApplyAttackCheckModifiers(target);
		}

		public static void ApplyAttackNonCheckModifiers(this IModifierApplierOwner owner, IModifierOwner target)
		{
			foreach (int id in owner.ModifierApplierController.GetApplierAttackModifierIds())
				target.ModifierController.Add(id, target, owner);
		}

		public static void TryApplyAttackCheckModifiers(this IModifierApplierOwner owner, IModifierOwner target)
		{
			foreach (var check in owner.ModifierApplierController.GetApplierAttackCheckModifiers())
				if (check.CheckUse(owner))
					target.ModifierController.Add(check.Id, target, owner);
		}

		/// <summary>
		///		Only triggers the check, does not trigger the modifiers effect. Used when modifiers 
		/// </summary>
		public static bool TryCastCheck(this IModifierApplierOwner owner, int id)
		{
			return owner.ModifierApplierController.TryCastCheck(id, owner);
		}

		/// <summary>
		///		Checks if we can cast the modifier, triggers the check if it exists
		/// </summary>
		public static bool CanCastModifier(this IModifierApplierOwner owner, int id)
		{
			return owner.ModifierApplierController.CanCastModifier(id, owner);
		}

		public static bool CanUseAttackModifier(this IModifierApplierOwner owner, int id)
		{
			return owner.ModifierApplierController.CanUseAttackModifier(id, owner);
		}
	}
}