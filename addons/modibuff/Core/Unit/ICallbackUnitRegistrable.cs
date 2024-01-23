namespace ModiBuff.Core
{
	public interface ICallbackUnitRegistrable<in TCallbackUnit>
	{
		void RegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
		void UnRegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
	}
}