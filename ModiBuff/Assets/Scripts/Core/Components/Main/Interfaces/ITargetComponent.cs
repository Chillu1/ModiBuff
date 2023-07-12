namespace ModiBuff.Core
{
	public interface ITargetComponent : IComponent
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		IUnit Source { get; }
		//IUnit OriginalOwner { get; }

		void UpdateSource(IUnit source);
	}
}