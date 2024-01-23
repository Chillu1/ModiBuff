namespace ModiBuff.Core
{
	public static class UnitCallbackExtensions
	{
		public static void RegisterCallbacks<TCallbackType>(this ICallbackRegistrable<TCallbackType> owner,
			TCallbackType callbackType, object[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
				owner.RegisterCallback(callbackType, callbacks[i]);
		}

		public static void UnRegisterCallbacks<TCallbackType>(this ICallbackRegistrable<TCallbackType> owner,
			TCallbackType callbackType, object[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
				owner.UnRegisterCallback(callbackType, callbacks[i]);
		}

		public static void RegisterCallbacks<TCallbackType>(this ICallbackRegistrable<TCallbackType> owner,
			Callback<TCallbackType>[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				ref var callback = ref callbacks[i];
				owner.RegisterCallback(callback.CallbackType, callback.Action);
			}
		}

		public static void UnRegisterCallbacks<TCallbackType>(this ICallbackRegistrable<TCallbackType> owner,
			Callback<TCallbackType>[] callbacks)
		{
			for (int i = 0; i < callbacks.Length; i++)
			{
				ref var callback = ref callbacks[i];
				owner.UnRegisterCallback(callback.CallbackType, callback.Action);
			}
		}
	}
}