namespace ModiBuff.Core
{
	/// <summary>
	///		Delegate version of callback registering, can only use manual self defined behaviour
	/// </summary>
	public sealed class CallbackRegisterDelegateEffect<TCallback> : IEffect
	{
		private readonly TCallback _callbackType;
		private readonly UnitCallback _callbacks;

		public CallbackRegisterDelegateEffect(TCallback callbackType, UnitCallback callbacks)
		{
			_callbackType = callbackType;
			_callbacks = callbacks;
		}

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_callbacks == null)
				Logger.LogError("Callback wasn't set");
#endif

			((ICallbackRegistrable<TCallback>)target).RegisterCallback(_callbackType, _callbacks);
		}
	}
}