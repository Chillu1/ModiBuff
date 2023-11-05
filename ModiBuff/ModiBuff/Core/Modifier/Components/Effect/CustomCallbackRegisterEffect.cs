using System;

namespace ModiBuff.Core
{
	public sealed class CustomCallbackRegisterEffect<TCallback> : IRevertEffect, IEffect, IStateEffect,
		IRecipeFeedEffects
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly EffectMethod _effectMethod;
		private readonly Func<IEffect, object> _event;

		private object _callback;

		private bool _isRegistered;

		public CustomCallbackRegisterEffect(TCallback callbackType, EffectMethod effectMethod,
			Func<IEffect, object> @event)
		{
			_callbackType = callbackType;
			_effectMethod = effectMethod;
			_event = @event;
		}

		public void SetEffects(IEffect[] effects)
		{
			_callback = _event(effects[0]);
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (_isRegistered)
				return;

			_isRegistered = true;
			((ICustomCallbackRegistrable<TCallback>)target).RegisterCallback(_callbackType, _callback);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			//((ICustomCallbackRegistrable<TCallback>)target).UnRegisterCallback(_callbacks);
			_isRegistered = false;
		}

		public void ResetState()
		{
			_isRegistered = false;
		}

		public IEffect ShallowClone() =>
			new CustomCallbackRegisterEffect<TCallback>(_callbackType, _effectMethod, _event);

		object IShallowClone.ShallowClone() => ShallowClone();
	}
}