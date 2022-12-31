namespace ModiBuff.Core
{
	public interface ITargetComponent : IComponent
	{
		IUnit Target { get; }

		IUnit Source { get; }
		//IUnit OriginalOwner { get; }
	}
}