namespace ModiBuff.Core
{
	public interface ICallbackEffectRegistrable<in TCallback>
	{
		void RegisterCallbacks(TCallback callbackType, object[] callbacks);
		void UnRegisterCallbacks(TCallback callbackType, object[] callbacks);
	}
}