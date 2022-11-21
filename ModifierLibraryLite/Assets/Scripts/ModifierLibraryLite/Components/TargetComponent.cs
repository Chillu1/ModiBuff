namespace ModifierLibraryLite
{
	public class TargetComponent : ITargetComponent
	{
		public IUnit Target { get; }
		public IUnit Owner { get; }
	}
}