namespace ModiBuff.Core
{
	public sealed class ReactState : IStateReset //TODO Move me
	{
		public float Value;

		public void ResetState() => Value = 0;
	}
}