namespace ModiBuff.Core
{
	public interface IStackComponent : ITarget, IStateReset, IStackReference
	{
		void Stack();
		void ResetStacks();
	}
}