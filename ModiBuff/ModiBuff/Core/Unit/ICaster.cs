namespace ModiBuff.Core
{
	public interface ICaster : IModifierApplierOwner
	{
		bool TryCast(int modifierId, IUnit target);
	}
}