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
			_targeting.UpdateTargetSource(ref target, ref source);

			if (!((IUnitEntity)source).UnitTag.HasTag(UnitTag.Lifestealable))
				return;

			((IHealable<float, float>)target).Heal(value * _lifeStealPercent, source);
			((IEventOwner)source).ResetEventGenId();
			((IEventOwner)target).ResetEventGenId();
		}
	}
}