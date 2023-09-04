namespace ModiBuff.Core.Units
{
	public interface IStatusEffectModifierOwner<in TLegalAction, in TStatusEffectType> : IModifierOwner,
		IStatusEffectOwner<TLegalAction, TStatusEffectType>
	{
	}
}