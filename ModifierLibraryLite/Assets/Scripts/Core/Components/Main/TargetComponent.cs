namespace ModifierLibraryLite.Core
{
	public class TargetComponent : ITargetComponent
	{
		public IUnit Sender { get; private set; }

		public IUnit Owner { get; private set; }

		public IUnit Target { get; private set; }

		public TargetComponent()
		{
		}

		public TargetComponent(IUnit sender, IUnit owner, IUnit target)
		{
			Sender = sender;
			Owner = owner;
			Target = target;
		}
	}
}