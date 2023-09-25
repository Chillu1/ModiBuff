using System;

namespace ModiBuff.Core
{
	public sealed class CallbackRegisterEffect<TCallback> : IEffect
	{
		private readonly TCallback _callbackType;
		private readonly Action<IUnit, IUnit> _callback;

		public CallbackRegisterEffect(TCallback callbackType, Action<IUnit, IUnit> callback)
		{
			_callbackType = callbackType;
			_callback = callback;
		}

		public void Effect(IUnit target, IUnit source)
		{
			((ICallbackRegistrable<TCallback>)target).RegisterCallback(_callbackType, _callback);
		}
	}
}