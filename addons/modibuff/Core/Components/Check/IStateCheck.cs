namespace ModiBuff.Core
{
	public interface IStateCheck : ICheck, IDefaultState, IStateReset, IShallowClone
	{
	}

	public interface IStateCheck<out TSelf> : IShallowClone<TSelf>, IStateCheck
	{
	}
}