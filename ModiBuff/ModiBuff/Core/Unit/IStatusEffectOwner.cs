namespace ModiBuff.Core
{
	public interface IStatusEffectOwner
	{
		bool HasLegalAction(LegalAction legalAction);
		bool HasStatusEffect(StatusEffectType statusEffect);
		void ChangeStatusEffect(StatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration);
	}
}