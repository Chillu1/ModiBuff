namespace ModiBuff.Core.Units
{
	public sealed class HealActionEffect : IEffect
	{
		public void Effect(IUnit target, IUnit source)
		{
			if (!(source is IHealer<float, float> healerSource))
				return;
			if (!(target is IHealable<float, float> healableTarget))
				return;

			healerSource.Heal(healableTarget);
		}
	}
}