namespace ModiBuff.Core.Units
{
	public interface IStatusEffectOwner
	{
		IStatusEffectController StatusEffectController { get; }
	}
}