namespace ModifierLibraryLite.Core
{
	public interface ITimeComponent : IComponent
	{
		void Update(in float deltaTime);
	}
}