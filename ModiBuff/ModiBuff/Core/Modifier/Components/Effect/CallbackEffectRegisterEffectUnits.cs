using System;

namespace ModiBuff.Core
{
	public sealed class CallbackEffectRegisterEffectUnits<TCallback> : IRevertEffect, IEffect,
		IRecipeFeedEffects, IRegisterEffect, IShallowClone<IEffect>
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly Func<IEffect, Func<IUnit, IUnit, object>> _event;

		private Func<IUnit, IUnit, object>[] _callbacks;

		private bool _isRegistered;

		public CallbackEffectRegisterEffectUnits(TCallback callbackType,
			Func<IEffect, Func<IUnit, IUnit, object>> @event)
		{
			_callbackType = callbackType;
			_event = @event;
		}

		public void SetEffects(IEffect[] effects)
		{
			_callbacks = new Func<IUnit, IUnit, object>[effects.Length];
			for (int i = 0; i < effects.Length; i++)
				_callbacks[i] = _event(effects[i]);
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (!(target is ICallbackEffectRegistrable<TCallback> registrableTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(ICallbackEffectRegistrable<TCallback>));
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
			if (!(target is ICallbackEffectRegistrable<TCallback> registrableTarget))
				return;

			for (int i = 0; i < _callbacks.Length; i++)
				registrableTarget.UnRegisterCallback(_callbackType, _callbacks[i](target, source));
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new CallbackEffectRegisterEffectUnits<TCallback>(_callbackType, _event);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}