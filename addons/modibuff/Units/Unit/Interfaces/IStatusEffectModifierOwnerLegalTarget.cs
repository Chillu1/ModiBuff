namespace ModiBuff.Core.Units
{
	public interface IStatusEffectModifierOwnerLegalTarget<in TLegalAction, in TStatusEffectType> :
		IModifierOwner, IStatusEffectOwner<TLegalAction, TStatusEffectType>, IUnitEntity, IModifierApplierOwner
	{
	}
}