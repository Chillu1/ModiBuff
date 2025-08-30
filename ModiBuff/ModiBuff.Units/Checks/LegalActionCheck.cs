namespace ModiBuff.Core.Units
{
	public sealed class LegalActionCheck : IUnitCheck
	{
		private readonly LegalAction _legalAction;

		public LegalActionCheck(LegalAction legalAction) => _legalAction = legalAction;

		public bool Check(IUnit source)
		{
			var statusEffectOwner = (IStatusEffectOwner<LegalAction, StatusEffectType>)source;
			return statusEffectOwner.StatusEffectController.HasLegalAction(_legalAction);
		}
	}
}