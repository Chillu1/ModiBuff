namespace ModiBuff.Core
{
	public interface IModifierOwner : IUnit
	{
		ModifierController ModifierController { get; } //TODO Refactor/remove?
	}

	public static class ModifierOwnerExtensions
	{
		public static bool TryAddModifier(this IModifierOwner owner, int id, IUnit source)
		{
			return owner.ModifierController.TryAdd(id, owner, source);
		}
	}
}