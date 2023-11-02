namespace ModiBuff.Core.Units
{
	public sealed class HealFromPoisonStacksMetaEffect : IPostEffect<float, int>
	{
		private readonly float _multiplier;
		private readonly Targeting _targeting;

		public HealFromPoisonStacksMetaEffect(float multiplier, Targeting targeting = Targeting.TargetSource)
		{
			_multiplier = multiplier;
			_targeting = targeting;
		}

		public void Effect(float damage, int poisonStacks, IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			((IHealable<float, float>)target).Heal(poisonStacks * _multiplier, source);
		}
	}
}