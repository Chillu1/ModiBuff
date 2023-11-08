namespace ModiBuff.Core.Units
{
	/// <summary>
	///		No operation effect, only used for benchmarking and testing, DO NOT use in production
	/// </summary>
	public sealed class NoOpEffect : IEffect
	{
		public void Effect(IUnit target, IUnit source)
		{
		}
	}
}