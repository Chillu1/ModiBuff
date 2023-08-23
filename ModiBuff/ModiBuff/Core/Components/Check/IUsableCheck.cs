namespace ModiBuff.Core
{
	public interface IUsableCheck : ICheck
	{
		void Use(IUnit unit);
	}
}