namespace ModiBuff.Core
{
	public interface ISavable
	{
		object SaveState();
		void LoadState(object data);
	}

	public interface ISavable<TSaveData> : ISavable
	{
		new TSaveData SaveState();
		void LoadState(TSaveData data);
	}
}