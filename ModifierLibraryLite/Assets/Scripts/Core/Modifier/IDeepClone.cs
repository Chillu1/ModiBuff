namespace ModifierLibraryLite.Core
{
	public interface IDeepClone<out T>
	{
		T DeepClone();
	}
}