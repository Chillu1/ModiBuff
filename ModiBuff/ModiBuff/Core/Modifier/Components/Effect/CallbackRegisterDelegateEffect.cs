namespace ModiBuff.Core
{
	/// <summary>
	///		Delegate version of callback registering, can only use manual self defined behaviour
	/// </summary>
	public sealed class CallbackRegisterDelegateEffect<TCallback> : IRevertEffect, IEffect, IStateEffect
	{
		public bool IsRevertible { get; }

		private readonly TCallback _callbackType;
		private readonly UnitCallback _callbacks;

		private bool _isRegistered;

		public CallbackRegisterDelegateEffect(TCallback callbackType, UnitCallback callbacks, bool isRevertible = true)
		{
			_callbackType = callbackType;
			_callbacks = callbacks;
			IsRevertible = isRevertible;
		}

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_callbacks == null)
				Logger.LogError("[ModiBuff] Callback wasn't set");
#endif

			if (_isRegistered)
				return;

			((ICallbackRegistrable<TCallback>)target).RegisterCallbacks(_callbackType, _callbacks);
			_isRegistered = true;
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((ICallbackRegistrable<TCallback>)target).UnRegisterCallbacks(_callbackType, _callbacks);
			_isRegistered = false;
		}

		public void ResetState()
		{
			_isRegistered = false;
		}

		public IEffect ShallowClone() =>
			new CallbackRegisterDelegateEffect<TCallback>(_callbackType, _callbacks, IsRevertible);

		object IShallowClone.ShallowClone() => ShallowClone();
	}
}