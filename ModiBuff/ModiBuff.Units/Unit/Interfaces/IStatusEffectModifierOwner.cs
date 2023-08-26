namespace ModiBuff.Core.Units
{
	public interface IStatusEffectModifierOwner<TLegalAction, TStatusEffectType> : IModifierOwner,
		IStatusEffectOwner<TLegalAction, TStatusEffectType>
	{
	}
}