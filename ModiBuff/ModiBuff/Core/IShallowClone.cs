namespace ModiBuff.Core
{
	public interface IShallowClone<out T> : IShallowClone
	{
		new T ShallowClone();
	}

	public interface IShallowClone
	{
		object ShallowClone();
	}
}