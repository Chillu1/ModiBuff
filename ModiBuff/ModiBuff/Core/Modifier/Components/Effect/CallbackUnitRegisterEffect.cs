namespace ModiBuff.Core
{
	/// <summary>
	///		Registers a callback of effects to the target, for non-IEffect version see <see cref="CallbackRegisterDelegateEffect{TCallback}"/>
	/// </summary>
	public sealed class CallbackUnitRegisterEffect<TCallbackUnit> : IRecipeFeedEffects, IRevertEffect, IEffect,
		IStateEffect, IRegisterEffect
	{
		//Callback register should always be revertible, since we're using IEffect instances that will be pooled back 
		public bool IsRevertible => true;

		private readonly TCallbackUnit _callbackType;

		private IEffect[] _callbacks;

		private bool _isRegistered;

		public CallbackUnitRegisterEffect(TCallbackUnit callbackType)
		{
			_callbackType = callbackType;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static CallbackUnitRegisterEffect<TCallbackUnit> Create(TCallbackUnit callbackType,
			params IEffect[] callbacks) =>
			new CallbackUnitRegisterEffect<TCallbackUnit>(callbackType, callbacks);

		private CallbackUnitRegisterEffect(TCallbackUnit callbackType, IEffect[] callbacks)
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
			if (!(target is ICallbackUnitRegistrable<TCallbackUnit> registrableTarget))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(ICallbackUnitRegistrable<TCallbackUnit>));
#endif
				return;
			}

			if (_isRegistered)
				return;

			registrableTarget.RegisterCallbacks(_callbackType, _callbacks);
			_isRegistered = true;
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (!(target is ICallbackUnitRegistrable<TCallbackUnit> registrableTarget))
				return;

			registrableTarget.UnRegisterCallbacks(_callbackType, _callbacks);
			_isRegistered = false;
		}

		public void ResetState()
		{
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new CallbackUnitRegisterEffect<TCallbackUnit>(_callbackType);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}