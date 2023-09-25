using System;

namespace ModiBuff.Core
{
	public sealed class CallbackRegisterEffect : IEffect
	{
		private readonly Action<IUnit, IUnit> _callback;

		public CallbackRegisterEffect(Action<IUnit, IUnit> callback) => _callback = callback;

		public void Effect(IUnit target, IUnit source)
		{
			((ICallbackRegistrable)target).RegisterCallback(_callback);
		}
	}
}