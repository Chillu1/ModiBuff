namespace ModiBuff.Core
{
	/// <summary>
	///		Register effects that subscribe on init
	///		Interface for save/load, to register the callback on load
	/// </summary>
	public interface IRegisterEffect : IShallowClone<IEffect>
	{
	}
}