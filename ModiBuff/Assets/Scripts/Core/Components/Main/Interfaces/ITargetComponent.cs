namespace ModiBuff.Core
{
	public interface ITargetComponent : IComponent
	{
		IUnit Target { get; }

		IUnit Owner { get; }
		//IUnit OriginalOwner { get; }
	}
}