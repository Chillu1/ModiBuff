namespace ModiBuff.Core
{
	public interface IModifierApplierOwner : IUnit
	{
		ModifierApplierController ModifierApplierController { get; }
	}
}