namespace ModiBuff.Core.Units
{
	public sealed class LifeStealPostEffect : IPostEffect<float>
	{
		private readonly float _lifeStealPercent;
		private readonly Targeting _targeting;

		public LifeStealPostEffect(float lifeStealPercent, Targeting targeting = Targeting.TargetSource)
		{
			_lifeStealPercent = lifeStealPercent;
			_targeting = targeting;
		}

		public void Effect(float value, IUnit target, IUnit source)
		{
			if (!((IUnitEntity)target).UnitTag.HasTag(UnitTag.Lifestealable))
				return;

			_targeting.UpdateTargetSource(ref target, ref source);

			((IHealable<float, float>)target).Heal(value * _lifeStealPercent, source);
		}
	}
}