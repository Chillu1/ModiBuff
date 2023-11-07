namespace ModiBuff.Core
{
	public interface ICallbackEffectRegistrable<in TCallback>
	{
		void RegisterCallback(TCallback callbackType, object callback);
		void UnRegisterCallback(TCallback callbackType, object callback);
		void RegisterCallbacks(TCallback callbackType, object[] callbacks);
		void UnRegisterCallbacks(TCallback callbackType, object[] callbacks);
	}
}