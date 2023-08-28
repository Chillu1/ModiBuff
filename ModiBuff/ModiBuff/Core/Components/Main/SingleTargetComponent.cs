namespace ModiBuff.Core
{
	public sealed class SingleTargetComponent : ISingleTargetComponent
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		public IUnit Source { get; private set; }

		public IUnit Target { get; private set; }

		public SingleTargetComponent()
		{
		}

		public SingleTargetComponent(IUnit target, IUnit source)
		{
			Target = target;
			Source = source;
		}

		public void UpdateSource(IUnit source) => Source = source;
		public void UpdateTarget(IUnit target) => Target = target;

		public void ResetState()
		{
			Source = null;
			Target = null;
		}
	}
}