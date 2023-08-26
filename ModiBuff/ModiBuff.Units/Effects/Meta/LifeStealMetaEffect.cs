namespace ModiBuff.Core.Units
{
	public sealed class LifeStealMetaEffect : IMetaEffect<float>
	{
		private readonly float _lifeStealPercent;

		public Targeting Targeting { get; }

		public LifeStealMetaEffect(float lifeStealPercent, Targeting targeting = Targeting.TargetSource)
		{
			_lifeStealPercent = lifeStealPercent;
			Targeting = targeting;
		}

		public void Effect(float value, IUnit target, IUnit source, bool triggerEvents)
		{
			((IHealable<float, float>)target).Heal(value * _lifeStealPercent, source, triggerEvents);
		}
	}
}