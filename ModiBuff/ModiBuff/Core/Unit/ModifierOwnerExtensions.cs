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
	}
}