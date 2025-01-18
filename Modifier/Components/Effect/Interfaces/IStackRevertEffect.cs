namespace ModiBuff.Core
{
	public interface IStackRevertEffect
	{
		bool IsStackRevertible { get; }

		void RevertStack(int stacks, IUnit target, IUnit source);
	}
}