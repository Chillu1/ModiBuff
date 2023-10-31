namespace ModiBuff.Core
{
	/// <summary>
	///		If the effect doesn't always use mutable state functionality
	/// </summary>
	public interface IMutableStateEffect : IStateEffect
	{
		bool UsesMutableState { get; }
	}
}