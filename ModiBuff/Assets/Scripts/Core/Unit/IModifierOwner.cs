namespace ModiBuff.Core
{
	public interface IModifierOwner
	{
		ModifierController ModifierController { get; } //TODO Refactor/remove?

		bool TryAddModifier(int id, IUnit source);
	}
}