using System;

namespace ModiBuff.Core
{
	public struct CallbackStateContext<TObjectSaveData>
	{
		public readonly object Callback;
		public readonly Func<TObjectSaveData> StateGetter;
		public readonly Action<TObjectSaveData> StateSetter;
		public readonly TObjectSaveData DefaultState;

		public CallbackStateContext(object callback, Func<TObjectSaveData> stateGetter,
			Action<TObjectSaveData> stateSetter, TObjectSaveData defaultState = default)
		{
			Callback = callback;
			StateGetter = stateGetter;
			StateSetter = stateSetter;
			DefaultState = defaultState;
		}
	}

	public sealed class CallbackStateSaveRegisterEffect<TCallback, TEffectStateData> : IRevertEffect, IEffect,
		IMutableStateRegisterEffect, IShallowClone<IEffect>,
		ISavable<CallbackStateSaveRegisterEffect<TCallback, TEffectStateData>.SaveData>,
		IEffectStateInfo<CallbackStateSaveRegisterEffect<TCallback, TEffectStateData>.Data>
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly Func<CallbackStateContext<TEffectStateData>> _event;
		private readonly object _callback;
		private readonly TEffectStateData _defaultState;
		private readonly Func<TEffectStateData> _stateGetter;
		private readonly Action<TEffectStateData> _stateSetter;

		private bool _isRegistered;

		public CallbackStateSaveRegisterEffect(TCallback callbackType,
			Func<CallbackStateContext<TEffectStateData>> @event)
		{
			_callbackType = callbackType;
			_event = @event;
			var context = @event();
			_callback = context.Callback;
			_stateGetter = context.StateGetter;
			_stateSetter = context.StateSetter;
			_defaultState = context.DefaultState;
		}

		//Won't work since we'd have to explicitly call this on load. Could have a special interface with reflection, but that's over-engineering
		//public CallbackStateSaveRegisterEffect<TCallback> Load(TCallback callbackType,
		//	Func<object, CallbackStateContext> @event, object data)
		//{
		//	return new CallbackStateSaveRegisterEffect<TCallback>(callbackType, @event, @event(data));
		//}

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
			registrableTarget.RegisterCallback(_callbackType, _callback);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (!(target is ICallbackRegistrable<TCallback> registrableTarget))
				return;

			registrableTarget.UnRegisterCallback(_callbackType, _callback);
			_isRegistered = false;
		}

		public Data GetEffectData() => new Data(_stateGetter());

		public void ResetState() => _stateSetter(_defaultState);

		public IEffect ShallowClone() =>
			new CallbackStateSaveRegisterEffect<TCallback, TEffectStateData>(_callbackType, _event);

		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_stateGetter());
		public void LoadState(object data) => _stateSetter(((SaveData)data).State);

		public readonly struct Data
		{
			public readonly TEffectStateData State;

			public Data(TEffectStateData state) => State = state;
		}

		public readonly struct SaveData
		{
			public readonly TEffectStateData State;

#if MODIBUFF_SYSTEM_TEXT_JSON && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(TEffectStateData state) => State = state;
		}
	}
}