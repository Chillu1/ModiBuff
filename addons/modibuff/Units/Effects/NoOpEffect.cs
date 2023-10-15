namespace ModiBuff.Core.Units
{
	public sealed class NoOpEffect : IEffect
	{
		public void Effect(IUnit target, IUnit source)
		{
		}
	}
}