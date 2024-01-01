namespace ModiBuff.Core
{
	public interface IStateCheck : ICheck, IDefaultState, IStateReset, IShallowClone, ISavable
	{
	}

	public interface IStateCheck<out TSelf, TSaveData> : IShallowClone<TSelf>, IStateCheck, ISavable<TSaveData>
	{
	}
}