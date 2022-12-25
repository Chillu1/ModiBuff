namespace ModiBuff.Core
{
	public sealed class TargetComponent : ITargetComponent
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		public IUnit Acter { get; private set; }

		public IUnit Target { get; private set; }

		public TargetComponent(IUnit target, IUnit acter)
		{
			Target = target;
			Acter = acter;
		}
	}
}