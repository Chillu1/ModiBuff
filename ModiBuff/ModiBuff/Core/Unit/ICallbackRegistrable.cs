using System;

namespace ModiBuff.Core
{
	public interface ICallbackRegistrable<in TCallback>
	{
		void RegisterCallback(TCallback callbackType, Action<IUnit, IUnit> callback);
	}
}