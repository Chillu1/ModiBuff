namespace ModiBuff.Core
{
	public interface ICallbackUnitRegistrable<in TCallbackUnit> : IEventOwner
	{
		void RegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
		void UnRegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
	}
}