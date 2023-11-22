using System;

namespace ModiBuff.Core
{
	public struct CallbackStateContext
	{
		public readonly object Callback;
		public readonly Func<object> StateGetter;
		public readonly Action<object> StateSetter;

		public CallbackStateContext(object callback, Func<object> stateGetter, Action<object> stateSetter)
		{
			Callback = callback;
			StateGetter = stateGetter;
			StateSetter = stateSetter;
		}
	}

	public sealed class CallbackStateSaveRegisterEffect<TCallback> : IRevertEffect, IEffect,
		IRegisterEffect, IShallowClone<IEffect>, ISavable<CallbackStateSaveRegisterEffect<TCallback>.SaveData>,
		IModifierStateInfo
	{
		public bool IsRevertible => true;

		private readonly TCallback _callbackType;
		private readonly Func<CallbackStateContext> _event;
		private readonly object _callback;
		private readonly Func<object> _stateGetter;
		private readonly Action<object> _stateSetter;

		private bool _isRegistered;

		public CallbackStateSaveRegisterEffect(TCallback callbackType, Func<CallbackStateContext> @event)
		{
			_callbackType = callbackType;
			_event = @event;
			var context = @event();
			_callback = context.Callback;
			_stateGetter = context.StateGetter;
			_stateSetter = context.StateSetter;
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

		public IEffect ShallowClone() => new CallbackStateSaveRegisterEffect<TCallback>(_callbackType, _event);
		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_stateGetter());
		public void LoadState(object data) => _stateSetter(((SaveData)data).State);

		public struct SaveData
		{
			public readonly object State;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(object state) => State = state;
		}
	}
}