namespace ModiBuff.Core
{
	/// <summary>
	///		Stateful effect that has SaveData, can be cloned, and needs it's state reset
	/// </summary>
	public interface ISavableEffect<TSaveData> : ISavable<TSaveData>, IStateEffect
	{
	}
}