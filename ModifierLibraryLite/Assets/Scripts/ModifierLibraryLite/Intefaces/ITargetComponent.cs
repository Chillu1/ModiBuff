namespace ModifierLibraryLite
{
	public interface ITargetComponent
	{
		IUnit Target { get; }

		IUnit Owner { get; }
		//IUnit OriginalOwner { get; }
	}
}