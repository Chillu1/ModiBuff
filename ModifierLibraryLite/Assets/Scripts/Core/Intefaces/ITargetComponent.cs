namespace ModifierLibraryLite.Core
{
	public interface ITargetComponent
	{
		IUnit Target { get; }

		IUnit Owner { get; }
		//IUnit OriginalOwner { get; }
	}
}