namespace ModiBuff.Core.Units
{
	public interface IMultiInstanceStatusEffectController<in TLegalAction, in TStatusEffectType>
	{
		bool HasLegalAction(TLegalAction legalAction);
		bool HasStatusEffect(TStatusEffectType statusEffectType);

		void ChangeStatusEffect(int id, TStatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(int id, TStatusEffectType statusEffectType, float duration);
	}
}