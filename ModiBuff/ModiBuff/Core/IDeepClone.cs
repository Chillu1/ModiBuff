namespace ModiBuff.Core
{
	public interface IDeepClone<out T>
	{
		T DeepClone();
	}
}