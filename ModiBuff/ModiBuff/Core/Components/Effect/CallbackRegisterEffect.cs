namespace ModiBuff.Core
{
	public sealed class CallbackRegisterEffect<TCallback> : ICallbackEffect, IEffect, IShallowClone<CallbackRegisterEffect<TCallback>>
	{
		private readonly TCallback _callbackType;

		//TODO Callback or IEffect? Secondary class for IEffects only?
		private UnitCallback _callback;

		public CallbackRegisterEffect(TCallback callbackType)
		{
			_callbackType = callbackType;
		}

		public CallbackRegisterEffect(TCallback callbackType, UnitCallback callback)
		{
			_callbackType = callbackType;
			_callback = callback;
		}

		public void SetCallback(UnitCallback callback) => _callback = callback;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_callback == null)
				Logger.LogError("Callback wasn't set");
#endif

			((ICallbackRegistrable<TCallback>)target).RegisterCallback(_callbackType, _callback);
		}

		public CallbackRegisterEffect<TCallback> ShallowClone() => new CallbackRegisterEffect<TCallback>(_callbackType, _callback);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}