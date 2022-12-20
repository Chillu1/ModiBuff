namespace ModiBuff.Core
{
	public interface IStackEffect : IShallowClone<IStackEffect>
	{
		void StackEffect(int stacks, float value, ITargetComponent targetComponent);
	}
}