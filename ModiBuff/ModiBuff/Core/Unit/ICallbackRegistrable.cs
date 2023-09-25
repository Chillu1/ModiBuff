namespace ModiBuff.Core
{
	public delegate void UnitCallback(IUnit target, IUnit source); //TODO any reason for this to be supplied by the user?

	public interface ICallbackRegistrable<in TCallback>
	{
		void RegisterCallback(TCallback callbackType, UnitCallback callback);
	}
}