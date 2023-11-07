using System;

namespace ModiBuff.Core
{
	public sealed class CallbackStateRegisterEffect<TCallback> : IRevertEffect, IEffect, IStateEffect
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly Func<object> _event;
		private readonly object _callback;

		private bool _isRegistered;

		public CallbackStateRegisterEffect(TCallback callbackType, Func<object> @event)
		{
			_callbackType = callbackType;
			_event = @event;
			_callback = @event();
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (_isRegistered)
				return;

			_isRegistered = true;
			((ICallbackEffectRegistrable<TCallback>)target).RegisterCallback(_callbackType, _callback);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((ICallbackEffectRegistrable<TCallback>)target).UnRegisterCallback(_callbackType, _callback);
			_isRegistered = false;
		}

		public void ResetState()
		{
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new CallbackStateRegisterEffect<TCallback>(_callbackType, _event);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}