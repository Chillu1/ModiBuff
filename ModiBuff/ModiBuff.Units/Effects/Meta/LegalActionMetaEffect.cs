namespace ModiBuff.Core.Units
{
	public class LegalActionMetaEffect : IMetaEffect<float, float>
	{
		private readonly float _percent;
		private readonly bool _has;
		private readonly LegalAction _legalAction;
		private readonly Targeting _targeting;

		public LegalActionMetaEffect(float percent, LegalAction legalAction, bool has = true, Targeting targeting = Targeting.TargetSource)
		{
			_percent = percent;
			_has = has;
			_legalAction = legalAction;
			_targeting = targeting;
		}

		public float Effect(float value, IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);

			if (((IStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController.HasLegalAction(_legalAction) == _has)
				return value * _percent;

			return value;
		}
	}
}