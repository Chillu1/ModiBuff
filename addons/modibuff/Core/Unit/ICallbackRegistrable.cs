namespace ModiBuff.Core
{
	public interface ICallbackRegistrable<in TCallback>
	{
		void RegisterCallback(TCallback callbackType, object callback);
		void UnRegisterCallback(TCallback callbackType, object callback);
	}
}