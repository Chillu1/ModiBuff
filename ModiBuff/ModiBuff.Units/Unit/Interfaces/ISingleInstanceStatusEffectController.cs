namespace ModiBuff.Core.Units
{
	public interface ISingleInstanceStatusEffectController<in TLegalAction, in TStatusEffectType>
		: IStatusEffectController<TLegalAction, TStatusEffectType>
	{
		void ChangeStatusEffect(TStatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(TStatusEffectType statusEffectType, float duration);
	}
}