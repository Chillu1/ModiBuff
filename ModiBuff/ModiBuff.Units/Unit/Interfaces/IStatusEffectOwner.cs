namespace ModiBuff.Core.Units
{
	public interface IStatusEffectOwner<in TLegalAction, in TStatusEffectType>
	{
		IMultiInstanceStatusEffectController<TLegalAction, TStatusEffectType> StatusEffectController { get; }
	}
}