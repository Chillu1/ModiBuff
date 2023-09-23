namespace ModiBuff.Core
{
	public interface IModifierOwner : IUnit
	{
		ModifierController ModifierController { get; } //TODO Refactor/remove?
	}
}