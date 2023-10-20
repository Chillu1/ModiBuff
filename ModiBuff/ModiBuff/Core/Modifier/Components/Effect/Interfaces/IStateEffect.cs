namespace ModiBuff.Core
{
	/// <summary>
	///		Stateful effect, need to clone and
	/// </summary>
	//TODO Refactor/sort these interfaces
	public interface IStateEffect : IMutableStateEffect, IStateReset, IShallowClone<IEffect>
	{
	}
}