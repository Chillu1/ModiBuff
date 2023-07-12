namespace ModiBuff.Core
{
	public interface ISingleTargetComponent : ITargetComponent
	{
		IUnit Target { get; }
	}
}