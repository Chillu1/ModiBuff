namespace ModiBuff.Core
{
	/// <summary>
	///		Registers a callback of effects to the target, for non-IEffect version see <see cref="CallbackRegisterDelegateEffect{TCallback}"/>
	/// </summary>
	public sealed class CallbackRegisterEffect<TCallback> : IRecipeFeedEffects, IRevertEffect, IEffect,
		IShallowClone<CallbackRegisterEffect<TCallback>>
	{
		//Callback register should always be revertible, since we're using IEffect instances that will be pooled back 
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;

		private IEffect[] _callbacks;

		public CallbackRegisterEffect(TCallback callbackType)
		{
			_callbackType = callbackType;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static CallbackRegisterEffect<TCallback> Create(TCallback callbackType, params IEffect[] callbacks) =>
			new CallbackRegisterEffect<TCallback>(callbackType, callbacks);

		private CallbackRegisterEffect(TCallback callbackType, IEffect[] callbacks)
		{
			_callbackType = callbackType;
			_callbacks = callbacks;
		}

		public void SetEffects(IEffect[] callbacks) => _callbacks = callbacks;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_callbacks == null)
				Logger.LogError("[ModiBuff] Callback wasn't set");
#endif

			((ICallbackRegistrable<TCallback>)target).RegisterCallbacks(_callbackType, _callbacks);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((ICallbackRegistrable<TCallback>)target).UnRegisterCallbacks(_callbackType, _callbacks);
		}

		public CallbackRegisterEffect<TCallback> ShallowClone() =>
			new CallbackRegisterEffect<TCallback>(_callbackType);

		object IShallowClone.ShallowClone() => ShallowClone();
	}
}