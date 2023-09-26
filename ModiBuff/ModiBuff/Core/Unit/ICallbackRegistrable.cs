namespace ModiBuff.Core
{
	//TODO any reason for this to be supplied by the user? Makes it easier with casting for needed behaviour
	public delegate void UnitCallback(IUnit target, IUnit source);

	public interface ICallbackRegistrable<in TCallback>
	{
		void RegisterCallback(TCallback callbackType, IEffect[] callbacks);
		void RegisterCallback(TCallback callbackType, UnitCallback callback);
	}
}