namespace ModiBuff.Core.Units
{
	public interface IStatusEffectOwner<TLegalAction, TStatusEffectType>
	{
		IStatusEffectController<TLegalAction, TStatusEffectType> StatusEffectController { get; }
	}
}