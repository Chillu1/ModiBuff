namespace ModiBuff.Core
{
	public interface IStackComponent : ITarget, IComponent, IShallowClone<IStackComponent>
	{
		void Stack();
		void ResetState();
	}
}