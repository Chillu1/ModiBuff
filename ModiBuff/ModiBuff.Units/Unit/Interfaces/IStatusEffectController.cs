namespace ModiBuff.Core.Units
{
	public interface IStatusEffectController<in TLegalAction, in TStatusEffectType> : IUpdatable
	{
		bool HasLegalAction(TLegalAction legalAction);
		bool HasStatusEffect(TStatusEffectType statusEffectType);
	}
}