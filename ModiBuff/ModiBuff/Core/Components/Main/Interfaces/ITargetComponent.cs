namespace ModiBuff.Core
{
	public interface ITargetComponent : IStateReset
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		IUnit Source { get; }
		//IUnit OriginalOwner { get; }

		void UpdateSource(IUnit source);
	}
}