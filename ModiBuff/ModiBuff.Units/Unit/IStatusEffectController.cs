namespace ModiBuff.Core.Units
{
	public interface IStatusEffectController
	{
		bool HasLegalAction(LegalAction legalAction);
		bool HasStatusEffect(StatusEffectType statusEffectType);

		void ChangeStatusEffect(StatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration);
	}
}