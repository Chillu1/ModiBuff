namespace ModiBuff.Core.Units
{
	public sealed class StatusEffectCheck : IUnitCheck
	{
		private readonly StatusEffectType _statusEffectType;

		public StatusEffectCheck(StatusEffectType statusEffectType) => _statusEffectType = statusEffectType;

		public bool Check(IUnit source)
		{
			var statusEffectOwner = (IStatusEffectOwner<LegalAction, StatusEffectType>)source;
			return statusEffectOwner.StatusEffectController.HasStatusEffect(_statusEffectType);
		}
	}
}