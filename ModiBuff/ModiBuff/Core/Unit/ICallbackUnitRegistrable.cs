namespace ModiBuff.Core
{
	//TODO any reason for this to be supplied by the user? Makes it easier with casting for needed behaviour
	public delegate void UnitCallback(IUnit target, IUnit source);

	public interface ICallbackUnitRegistrable<in TCallbackUnit> : IEventOwner
	{
		void RegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
		void UnRegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
		void RegisterCallbacks(TCallbackUnit callbackType, UnitCallback callback);
		void UnRegisterCallbacks(TCallbackUnit callbackType, UnitCallback callback);
	}
}