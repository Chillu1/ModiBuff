namespace ModifierLibraryLite.Core
{
	public interface IModifierController
	{
		void Update(in float delta);
		(bool Success, Modifier Modifier) TryAdd(Modifier modifier);
	}
}