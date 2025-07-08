using System;

namespace ModiBuff.Core
{
	public interface ICallbackRegistrable<in TCallback>
	{
		void RegisterCallback(TCallback callbackType, Delegate callback);
		void UnRegisterCallback(TCallback callbackType, Delegate callback);
	}
}