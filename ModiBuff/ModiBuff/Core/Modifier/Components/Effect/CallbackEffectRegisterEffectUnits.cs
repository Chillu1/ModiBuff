using System;

namespace ModiBuff.Core
{
	public sealed class CallbackEffectRegisterEffectUnits<TCallback> : IRevertEffect, IEffect,
		IRecipeFeedEffects, IRegisterEffect
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly Func<IEffect, Func<IUnit, IUnit, Delegate>> _event;

		private Func<IUnit, IUnit, Delegate>[] _callbacks;

		private bool _isRegistered;

		public CallbackEffectRegisterEffectUnits(TCallback callbackType,
			Func<IEffect, Func<IUnit, IUnit, Delegate>> @event)
		{
			_callbackType = callbackType;
			_event = @event;
		}

		public void SetEffects(IEffect[] effects)
		{
			_callbacks = new Func<IUnit, IUnit, Delegate>[effects.Length];
			for (int i = 0; i < effects.Length; i++)
				_callbacks[i] = _event(effects[i]);
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (!(target is ICallbackRegistrable<TCallback> registrableTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(ICallbackRegistrable<TCallback>));
#endif
				return;
			}

			if (_isRegistered)
				return;

			_isRegistered = true;
			for (int i = 0; i < _callbacks.Length; i++)
				registrableTarget.RegisterCallback(_callbackType, _callbacks[i](target, source));
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (!(target is ICallbackRegistrable<TCallback> registrableTarget))
				return;

			for (int i = 0; i < _callbacks.Length; i++)
				registrableTarget.UnRegisterCallback(_callbackType, _callbacks[i](target, source));
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new CallbackEffectRegisterEffectUnits<TCallback>(_callbackType, _event);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}