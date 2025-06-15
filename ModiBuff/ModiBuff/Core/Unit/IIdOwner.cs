namespace ModiBuff.Core
{
	public interface IIdOwner<out TId>
	{
		TId Id { get; }
	}
}