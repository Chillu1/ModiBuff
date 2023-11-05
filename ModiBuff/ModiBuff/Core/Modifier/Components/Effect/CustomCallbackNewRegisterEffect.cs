namespace ModiBuff.Core
{
	public sealed class CustomCallbackNewRegisterEffect<TCallback> : IRevertEffect, IEffect, IStateEffect
	{
		public bool IsRevertible => true;

		private readonly CustomCallback<TCallback>[] _callbacks;

		private bool _isRegistered;

		public CustomCallbackNewRegisterEffect(params CustomCallback<TCallback>[] callbacks)
		{
			_callbacks = callbacks;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (_isRegistered)
				return;

			_isRegistered = true;
			((ICustomCallbackRegistrable<TCallback>)target).RegisterCallbacks(_callbacks);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			((ICustomCallbackRegistrable<TCallback>)target).UnRegisterCallbacks(_callbacks);
			_isRegistered = false;
		}

		public void ResetState()
		{
			_isRegistered = false;
		}

		public IEffect ShallowClone() => new CustomCallbackNewRegisterEffect<TCallback>(_callbacks);

		object IShallowClone.ShallowClone() => ShallowClone();
	}

	public readonly struct CustomCallback<TCallback>
	{
		public readonly TCallback CallbackType;
		public readonly object Action;

		public CustomCallback(TCallback callbackType, object action)
		{
			CallbackType = callbackType;
			Action = action;
		}
	}
}