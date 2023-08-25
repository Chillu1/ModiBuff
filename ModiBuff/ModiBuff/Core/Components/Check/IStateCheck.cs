namespace ModiBuff.Core
{
	public interface IStateCheck : ICheck, IStateReset, IShallowClone
	{
	}

	public interface IStateCheck<TSelf> : IShallowClone<TSelf>, IStateCheck
	{
	}
}