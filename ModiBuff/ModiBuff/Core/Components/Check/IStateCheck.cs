namespace ModiBuff.Core
{
	public interface IStateCheck : ICheck, IStateReset, IShallowClone
	{
	}

	public interface IStateCheck<out TSelf> : IShallowClone<TSelf>, IStateCheck
	{
	}
}