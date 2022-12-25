namespace ModiBuff.Core
{
	public interface ITargetComponent : IComponent
	{
		IUnit Target { get; }

		IUnit Acter { get; }
		//IUnit OriginalOwner { get; }
	}
}