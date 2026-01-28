namespace ModiBuff.Core
{
	public interface IModifierApplierOwner : IUnit
	{
		bool ContainsApplier(int modifierId, ApplierType applierType);
		bool ContainsApplier(int modifierId, int applierGenId, ApplierType applierType);

		bool TryApply(int modifierId, int? applierGenId, IUnit target);

		int AddApplierModifierNew(int modifierId, ApplierType applierType, ICheck[]? checks = null);
		bool RemoveApplier(int id, int? applierGenId, ApplierType applierType);
	}
}