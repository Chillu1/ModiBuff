namespace ModiBuff.Core
{
	public sealed class CallbackRegisterEffect<TCallback> : IRevertEffect, IEffect, IStateEffect, IRegisterEffect
	{
		public bool IsRevertible => true;

		private readonly Callback<TCallback>[] _callbacks;

		private bool _isRegistered;

		public CallbackRegisterEffect(params Callback<TCallback>[] callbacks)
		{
			_callbacks = callbacks;
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
			registrableTarget.RegisterCallbacks(_callbacks);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (!(target is ICallbackRegistrable<TCallback> registrableTarget))
				return;

			registrableTarget.UnRegisterCallbacks(_callbacks);
			_isRegistered = false;
		}

		public void ResetState()
		{
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new CallbackRegisterEffect<TCallback>(_callbacks);

		object IShallowClone.ShallowClone() => ShallowClone();
	}

	public readonly struct Callback<TCallback>
	{
		public readonly TCallback CallbackType;
		public readonly object Action;

		public Callback(TCallback callbackType, object action)
		{
			CallbackType = callbackType;
			Action = action;
		}
	}
}