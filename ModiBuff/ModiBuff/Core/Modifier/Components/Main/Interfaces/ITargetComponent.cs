namespace ModiBuff.Core
{
	public interface ITargetComponent : IStateReset, ISavable
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		IUnit Source { get; set; }
		//IUnit OriginalOwner { get; }
	}
}