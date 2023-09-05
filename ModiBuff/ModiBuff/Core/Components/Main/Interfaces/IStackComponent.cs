namespace ModiBuff.Core
{
	public interface IStackComponent : ITarget, IComponent, IStateReset
	{
		void Stack();
	}
}