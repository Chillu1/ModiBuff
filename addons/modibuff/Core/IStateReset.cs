namespace ModiBuff.Core
{
	/// <summary>
	///		Resets all state to default values, used in pooling
	/// </summary>
	/// <remarks>
	///		For effects, it only resets stack effect state,
	///		since stack should be the only place where state gets mutated,
	///		unless it's a register effect
	/// </remarks>
	public interface IStateReset
	{
		void ResetState();
	}
}