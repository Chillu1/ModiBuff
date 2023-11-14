namespace ModiBuff.Core
{
	public interface ITargetComponent : IStateReset
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		IUnit Source { get; set; }
		//IUnit OriginalOwner { get; }

		object SaveState();
		void LoadState(object saveData);
	}
}