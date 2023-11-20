namespace ModiBuff.Core.Units
{
	public interface ISingleInstanceStatusEffectController<in TLegalAction, in TStatusEffectType>
		: IUpdatableStatusEffectController<TLegalAction, TStatusEffectType>
	{
		void ChangeStatusEffect(TStatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(TStatusEffectType statusEffectType, float duration);
	}
}