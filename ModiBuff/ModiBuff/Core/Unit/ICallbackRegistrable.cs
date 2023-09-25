using System;

namespace ModiBuff.Core
{
	public interface ICallbackRegistrable
	{
		void RegisterCallback(Action<IUnit, IUnit> callback);
	}
}