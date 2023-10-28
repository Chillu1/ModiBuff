namespace ModiBuff.Core
{
	public interface IStackRevertEffect
	{
		void RevertStack(int stacks, float value, IUnit target, IUnit source);
	}
}