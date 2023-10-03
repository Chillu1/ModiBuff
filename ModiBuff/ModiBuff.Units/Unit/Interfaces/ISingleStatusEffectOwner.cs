namespace ModiBuff.Core.Units
{
	public interface ISingleStatusEffectOwner<in TLegalAction, in TStatusEffectType>
	{
		ISingleInstanceStatusEffectController<TLegalAction, TStatusEffectType> StatusEffectController { get; }
	}
}