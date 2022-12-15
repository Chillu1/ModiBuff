namespace ModifierLibraryLite.Core
{
	public interface IStackEffect
	{
		void StackEffect(int stacks, float value, IUnit target, IUnit owner);
	}
}