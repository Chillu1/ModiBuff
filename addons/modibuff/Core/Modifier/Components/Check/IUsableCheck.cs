namespace ModiBuff.Core
{
	public interface IUsableCheck : IUnitCheck
	{
		void Use(IUnit unit);
	}
}