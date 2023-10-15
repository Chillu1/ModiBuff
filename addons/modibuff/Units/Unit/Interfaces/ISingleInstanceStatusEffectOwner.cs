namespace ModiBuff.Core.Units
{
	public interface ISingleInstanceStatusEffectOwner<in TLegalAction, in TStatusEffectType>
	{
		ISingleInstanceStatusEffectController<TLegalAction, TStatusEffectType> StatusEffectController { get; }
	}
}