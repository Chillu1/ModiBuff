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

		//TODO Temporary?
		public void SetSender(IUnit sender)
		{
			Sender = sender;
		}

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