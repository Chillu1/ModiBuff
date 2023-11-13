namespace ModiBuff.Core
{
	public interface ISavableEffect
	{
		object SaveState();
		void LoadState(object saveData);
	}

	public interface ISavableEffect<out TData> : ISavableEffect
	{
		new TData GetSaveData();
		new void LoadSaveData(object saveData);
	}
}