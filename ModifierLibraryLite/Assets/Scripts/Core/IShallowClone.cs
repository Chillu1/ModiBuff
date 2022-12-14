namespace ModifierLibraryLite.Core
{
	public interface IShallowClone<out T>
	{
		T ShallowClone();
	}
}