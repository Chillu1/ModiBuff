namespace ModiBuff.Core.Units
{
	public interface IStatusEffectController<in TLegalAction, in TStatusEffectType>
	{
		bool HasLegalAction(TLegalAction legalAction);
		bool HasStatusEffect(TStatusEffectType statusEffectType);
	}
}