namespace ModiBuff.Core
{
	public interface ISavableEffect
	{
		object GetSaveData();
		void LoadSaveData(object saveData);
	}

	public interface ISavableEffect<out TData> : ISavableEffect
	{
		new TData GetSaveData();
		new void LoadSaveData(object saveData);
	}
}