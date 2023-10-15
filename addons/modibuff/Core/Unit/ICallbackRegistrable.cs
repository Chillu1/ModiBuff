namespace ModiBuff.Core
{
	//TODO any reason for this to be supplied by the user? Makes it easier with casting for needed behaviour
	public delegate void UnitCallback(IUnit target, IUnit source);

	public interface ICallbackRegistrable<in TCallback>
	{
		void RegisterCallbacks(TCallback callbackType, IEffect[] callbacks);
		void UnRegisterCallbacks(TCallback callbackType, IEffect[] callbacks);
		void RegisterCallbacks(TCallback callbackType, UnitCallback callback);
		void UnRegisterCallbacks(TCallback callbackType, UnitCallback callback);
	}
}