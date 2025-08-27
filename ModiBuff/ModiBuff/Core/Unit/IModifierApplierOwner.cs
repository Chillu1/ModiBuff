namespace ModiBuff.Core
{
	public interface IModifierApplierOwner : IUnit
	{
		ModifierApplierController ModifierApplierController { get; }
		void AddApplierModifierNew(int modifierId, ApplierType applierType, ICheck[]? checks = null);
	}
}