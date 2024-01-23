namespace ModiBuff.Core
{
	/// <summary>
	///		Register effects that contain mutable state
	/// </summary>
	public interface IMutableStateRegisterEffect : IRegisterEffect, IStateReset
	{
	}
}