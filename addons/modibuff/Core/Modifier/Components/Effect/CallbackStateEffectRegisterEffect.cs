using System;

namespace ModiBuff.Core
{
	public sealed class CallbackStateEffectRegisterEffect<TCallback, TEffectStateData> :
		IEffect, IRevertEffect, IRecipeFeedEffects, IMutableStateRegisterEffect, IShallowClone<IEffect>,
		ISavable<CallbackStateEffectRegisterEffect<TCallback, TEffectStateData>.SaveData>,
		IEffectStateInfo<CallbackStateEffectRegisterEffect<TCallback, TEffectStateData>.Data>
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly Func<IEffect, CallbackStateContext<TEffectStateData>> _event;

		private Func<TEffectStateData> _stateGetter;
		private Action<TEffectStateData> _stateSetter;
		private TEffectStateData _defaultState;
		private object[] _callbacks;

		private bool _isRegistered;

		public CallbackStateEffectRegisterEffect(TCallback callbackType,
			Func<IEffect, CallbackStateContext<TEffectStateData>> @event)
		{
			_callbackType = callbackType;
			_event = @event;
		}

		public void SetEffects(IEffect[] effects)
		{
			_callbacks = new object[effects.Length];
			for (int i = 0; i < effects.Length; i++)
			{
				var context = _event(effects[i]);
				_callbacks[i] = context.Callback;
				_stateGetter = context.StateGetter;
				_stateSetter = context.StateSetter;
				_defaultState = context.DefaultState;
			}
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
			registrableTarget.RegisterCallbacks(_callbackType, _callbacks);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (!(target is ICallbackRegistrable<TCallback> registrableTarget))
				return;

			registrableTarget.UnRegisterCallbacks(_callbackType, _callbacks);
			_isRegistered = false;
		}

		public Data GetEffectData() => new Data(_stateGetter());

		public void ResetState() => _stateSetter(_defaultState);

		public IEffect ShallowClone() =>
			new CallbackStateEffectRegisterEffect<TCallback, TEffectStateData>(_callbackType, _event);

		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_stateGetter());
		public void LoadState(object data) => _stateSetter(((SaveData)data).State);

		public struct Data
		{
			public readonly TEffectStateData State;

			public Data(TEffectStateData state) => State = state;
		}

		public struct SaveData
		{
			public readonly TEffectStateData State;

#if MODIBUFF_SYSTEM_TEXT_JSON && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(TEffectStateData state) => State = state;
		}
	}
}