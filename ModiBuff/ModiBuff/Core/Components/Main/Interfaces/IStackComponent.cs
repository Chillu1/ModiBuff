namespace ModiBuff.Core
{
	public interface IStackComponent : ITarget, IComponent, IStateReset, IShallowClone<IStackComponent>
	{
		void Stack();
	}
}