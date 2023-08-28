namespace ModiBuff.Core
{
	public interface IModifierOwner : IUnit
	{
		MultipleModifiersModifierController ModifierController { get; } //TODO Refactor/remove?
	}
}