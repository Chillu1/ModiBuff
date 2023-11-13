namespace ModiBuff.Core
{
	public interface ITargetComponent : IStateReset
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		IUnit Source { get; set; }
		//IUnit OriginalOwner { get; }

		ITargetComponentSaveData SaveState();
		void LoadState(ITargetComponentSaveData saveData);
	}

	public interface ITargetComponentSaveData
	{
	}
}