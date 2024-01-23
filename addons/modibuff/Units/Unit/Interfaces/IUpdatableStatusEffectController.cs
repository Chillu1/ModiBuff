namespace ModiBuff.Core.Units
{
	public interface IUpdatableStatusEffectController<in TLegalAction, in TStatusEffectType> :
		IStatusEffectController<TLegalAction, TStatusEffectType>, IUpdatable
	{
	}
}