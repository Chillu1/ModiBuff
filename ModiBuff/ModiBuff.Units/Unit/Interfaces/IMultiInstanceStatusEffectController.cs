namespace ModiBuff.Core.Units
{
	public interface IMultiInstanceStatusEffectController<in TLegalAction, in TStatusEffectType>
		: IStatusEffectController<TLegalAction, TStatusEffectType>
	{
		void ChangeStatusEffect(int id, int genId, TStatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(int id, int genId, TStatusEffectType statusEffectType, float duration);
	}
}