namespace ModiBuff.Core.Units
{
	public interface IStatusEffectController<TLegalAction, TStatusEffectType>
	{
		bool HasLegalAction(TLegalAction legalAction);
		bool HasStatusEffect(TStatusEffectType statusEffectType);

		void ChangeStatusEffect(TStatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(TStatusEffectType statusEffectType, float duration);
	}
}