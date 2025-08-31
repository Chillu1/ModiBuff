namespace ModiBuff.Core
{
	public interface IModifierApplierOwner : IUnit
	{
		bool ContainsApplier(int modifierId, ApplierType applierType);

		bool TryApply(int modifierId, IUnit target);

		void AddApplierModifierNew(int modifierId, ApplierType applierType, ICheck[]? checks = null);
		//void RemoveApplier(int id, ApplierType applierType);
	}
}