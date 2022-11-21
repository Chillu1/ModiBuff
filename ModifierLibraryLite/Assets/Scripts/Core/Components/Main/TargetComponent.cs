namespace ModifierLibraryLite.Core
{
	public class TargetComponent : ITargetComponent
	{
		public IUnit Owner { get; private set; }
		public IUnit Target { get; private set; }

		//TODO Temporary?
		public void SetOwner(IUnit owner)
		{
			Owner = owner;
		}

		//TODO Temporary?
		public void SetTarget(IUnit target)
		{
			Target = target;
		}
	}
}