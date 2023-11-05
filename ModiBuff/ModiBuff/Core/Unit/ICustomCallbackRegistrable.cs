namespace ModiBuff.Core
{
	public interface ICustomCallbackRegistrable<TCallback>
	{
		void RegisterCallbacks(CustomCallback<TCallback>[] callbacks);
		void UnRegisterCallbacks(CustomCallback<TCallback>[] callbacks);
		void RegisterCallbacks(TCallback callbackType, object[] callbacks);
		void UnRegisterCallbacks(TCallback callbackType, object[] callbacks);
	}
}