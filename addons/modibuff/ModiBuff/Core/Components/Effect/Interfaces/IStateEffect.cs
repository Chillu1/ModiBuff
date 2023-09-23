namespace ModiBuff.Core
{
	/// <summary>
	///		Stateful effect, need to clone and
	/// </summary>
	public interface IStateEffect : IStateReset, IShallowClone<IEffect>
	{
	}
}