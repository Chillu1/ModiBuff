namespace ModiBuff.Core
{
	/// <summary>
	///		Resets all state to default values, used in pooling
	/// </summary>
	public interface IStateReset
	{
		void ResetState();
	}
}