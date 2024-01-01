namespace ModiBuff.Core
{
	public interface IStackRevertEffect : IRevertEffect
	{
		void RevertStack(int stacks, IUnit target, IUnit source);
	}
}