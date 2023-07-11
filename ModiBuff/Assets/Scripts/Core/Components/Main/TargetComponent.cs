namespace ModiBuff.Core
{
	public sealed class TargetComponent : ITargetComponent
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		public IUnit Source { get; private set; }

		public IUnit Target { get; }

		public TargetComponent(IUnit target, IUnit source)
		{
			Target = target;
			Source = source;
		}

		public void UpdateSource(IUnit source)
		{
			Source = source;
		}
	}
}