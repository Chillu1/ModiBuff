namespace ModiBuff.Core
{
	public sealed class CallbackRegisterEffect<TCallback> : ICallbackEffect, IEffect, IShallowClone<CallbackRegisterEffect<TCallback>>
	{
		private readonly TCallback _callbackType;

		private IEffect[] _callbacks;

		public CallbackRegisterEffect(TCallback callbackType)
		{
			_callbackType = callbackType;
		}

		private CallbackRegisterEffect(TCallback callbackType, IEffect[] callbacks)
		{
			_callbackType = callbackType;
			_callbacks = callbacks;
		}

		public void SetCallback(IEffect[] callbacks) => _callbacks = callbacks;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_callbacks == null)
				Logger.LogError("Callback wasn't set");
#endif

			((ICallbackRegistrable<TCallback>)target).RegisterCallback(_callbackType, _callbacks);
		}

		public CallbackRegisterEffect<TCallback> ShallowClone() => new CallbackRegisterEffect<TCallback>(_callbackType, _callbacks);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}