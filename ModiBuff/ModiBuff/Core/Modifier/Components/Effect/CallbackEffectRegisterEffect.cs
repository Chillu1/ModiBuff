using System;

namespace ModiBuff.Core
{
	public sealed class CallbackEffectRegisterEffect<TCallback> : IRevertEffect, IEffect, IStateEffect,
		IRecipeFeedEffects
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly Func<IEffect, object> _event;

		private object[] _callbacks;

		private bool _isRegistered;

		public CallbackEffectRegisterEffect(TCallback callbackType, Func<IEffect, object> @event)
		{
			_callbackType = callbackType;
			_event = @event;
		}

		public void SetEffects(IEffect[] effects)
		{
			_callbacks = new object[effects.Length];
			for (int i = 0; i < effects.Length; i++)
				_callbacks[i] = _event(effects[i]);
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (_isRegistered)
				return;

			_isRegistered = true;
			((ICustomCallbackRegistrable<TCallback>)target).RegisterCallbacks(_callbackType, _callbacks);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((ICustomCallbackRegistrable<TCallback>)target).UnRegisterCallbacks(_callbackType, _callbacks);
			_isRegistered = false;
		}

		public void ResetState()
		{
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new CallbackEffectRegisterEffect<TCallback>(_callbackType, _event);

		object IShallowClone.ShallowClone() => ShallowClone();
	}
}