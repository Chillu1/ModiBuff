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
	}
}