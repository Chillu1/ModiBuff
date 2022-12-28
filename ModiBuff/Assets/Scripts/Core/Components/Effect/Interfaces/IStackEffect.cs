namespace ModiBuff.Core
{
	public interface IStackEffect
	{
		void StackEffect(int stacks, float value, ITargetComponent targetComponent);
	}
}