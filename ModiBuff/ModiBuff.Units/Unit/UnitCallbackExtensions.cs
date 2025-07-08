using System;

namespace ModiBuff.Core.Units
{
	public static class UnitCallbackExtensions
	{
		public static bool CheckCallback<TCallback>(this Delegate callbackObject, out TCallback callbackOut)
		{
			if (!(callbackObject is TCallback callback))
			{
				Logger.LogError(
					$"[ModiBuff.Units] objectDelegate is not of type {typeof(TCallback)} but {callbackObject.GetType()}, use named delegates instead.");
				callbackOut = default;
				return false;
			}

			callbackOut = callback;
			return true;
		}
	}
}