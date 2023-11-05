namespace ModiBuff.Core
{
	public interface ICustomCallbackRegistrable<TCustomCallbackType>
	{
		void RegisterCallbacks(CustomCallback<TCustomCallbackType>[] callbacks);
		void UnRegisterCallbacks(CustomCallback<TCustomCallbackType>[] callbacks);
		void RegisterCallbacks(TCustomCallbackType callbackType, object[] callbacks);
		void UnRegisterCallbacks(TCustomCallbackType callbackType, object[] callbacks);
	}
}