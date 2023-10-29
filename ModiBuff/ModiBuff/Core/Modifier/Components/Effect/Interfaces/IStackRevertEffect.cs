namespace ModiBuff.Core
{
	public interface IStackRevertEffect : IRevertEffect
	{
		void RevertStack(int stacks, float value, IUnit target, IUnit source);
	}
}