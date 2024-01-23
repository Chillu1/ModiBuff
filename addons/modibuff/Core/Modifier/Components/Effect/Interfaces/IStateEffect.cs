namespace ModiBuff.Core
{
	/// <summary>
	///		Stateful effect, can be cloned, and needs it's state reset
	///		If the state doesn't need to be reset (it's overriden), then only clone is needed
	/// </summary>
	public interface IStateEffect : IStateReset, IShallowClone<IEffect>
	{
	}
}