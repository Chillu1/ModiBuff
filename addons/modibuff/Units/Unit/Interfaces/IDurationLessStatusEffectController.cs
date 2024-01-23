namespace ModiBuff.Core.Units
{
	public interface IDurationLessStatusEffectController<in TLegalAction, in TStatusEffectType> :
		IStatusEffectController<TLegalAction, TStatusEffectType>
	{
		void ApplyStatusEffect(TStatusEffectType statusEffectType);
		void RemoveStatusEffect(TStatusEffectType statusEffectType);
	}
}