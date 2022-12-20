namespace ModiBuff.Core
{
	public interface IShallowClone<out T>
	{
		T ShallowClone();
	}
}