namespace ModifierLibraryLite.Core
{
	public interface IInitComponent : ITarget, IComponent
	{
		void Init();
	}
}