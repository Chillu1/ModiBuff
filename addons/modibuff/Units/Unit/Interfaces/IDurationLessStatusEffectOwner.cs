namespace ModiBuff.Core.Units
{
	public interface IDurationLessStatusEffectOwner<in TLegalAction, in TStatusEffectType>
	{
		IDurationLessStatusEffectController<TLegalAction, TStatusEffectType> StatusEffectController { get; }
	}
}