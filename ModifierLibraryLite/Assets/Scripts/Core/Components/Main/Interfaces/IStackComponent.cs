namespace ModifierLibraryLite.Core
{
	public interface IStackComponent : IComponent, IShallowClone<IStackComponent>
	{
		int Stacks { get; }
		bool Stack();
	}
}