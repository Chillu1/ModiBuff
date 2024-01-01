namespace ModiBuff.Core
{
	public interface ICaster : IModifierApplierOwner
	{
		void TryCast(int modifierId, IUnit target);
	}
}