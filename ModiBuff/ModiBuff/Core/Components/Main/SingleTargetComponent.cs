namespace ModiBuff.Core
{
	public sealed class SingleTargetComponent : ITargetComponent
	{
		/// <summary>
		///		Unit that applied the modifier.
		/// </summary>
		public IUnit Source { get; set; }

		public IUnit Target { get; set; }

		public SingleTargetComponent()
		{
		}

		public SingleTargetComponent(IUnit target, IUnit source)
		{
			Target = target;
			Source = source;
		}

		public void ResetState()
		{
			Source = null;
			Target = null;
		}
	}
}