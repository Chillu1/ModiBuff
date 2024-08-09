namespace ModiBuff.Core
{
	public interface ICallbackCounter //TODO make more generic (to fit callback naming as well)
	{
		/// <summary>
		///		Resets all event/callback counters, so we can trigger them again
		/// </summary>
		/// <remarks>We always reset all counters because event effects might trigger other callbacks as well</remarks>
		void ResetEventCounters();
	}

	public interface ICallbackUnitRegistrable<in TCallbackUnit>
	{
		void RegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
		void UnRegisterCallbacks(TCallbackUnit callbackType, IEffect[] callbacks);
	}
}