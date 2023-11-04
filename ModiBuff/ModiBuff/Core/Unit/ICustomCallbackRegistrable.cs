namespace ModiBuff.Core
{
	public interface ICustomCallbackRegistrable<TCustomCallbackType>
	{
		void RegisterCallbacks(CustomCallback<TCustomCallbackType>[] callbacks);
	}
}