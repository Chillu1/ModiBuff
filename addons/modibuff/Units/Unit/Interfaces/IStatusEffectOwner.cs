namespace ModiBuff.Core.Units
{
	public interface IStatusEffectOwner<in TLegalAction, in TStatusEffectType>
	{
		IStatusEffectController<TLegalAction, TStatusEffectType> StatusEffectController { get; }
	}
}