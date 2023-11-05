namespace ModiBuff.Core
{
	public interface ICallbackRegistrable<TCallback>
	{
		void RegisterCallbacks(Callback<TCallback>[] callbacks);
		void UnRegisterCallbacks(Callback<TCallback>[] callbacks);
	}
}